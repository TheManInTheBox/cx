using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.StandardLibrary.Services
{
    /// <summary>
    /// Automatic Shutdown Timer Service - Implements system.shutdown on a configurable timer
    /// Ensures all CX applications have graceful shutdown behavior with proper cleanup
    /// </summary>
    public class AutoShutdownTimerService : IDisposable
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<AutoShutdownTimerService> _logger;
        private readonly int _shutdownDelayMs;
        private readonly string _shutdownReason;
        private System.Threading.Timer? _shutdownTimer;
        private bool _disposed = false;

        public AutoShutdownTimerService(
            ICxEventBus eventBus,
            ILogger<AutoShutdownTimerService> logger,
            int shutdownDelayMs = 30000, // Default 30 seconds
            string shutdownReason = "auto_timer")
        {
            _eventBus = eventBus;
            _logger = logger;
            _shutdownDelayMs = shutdownDelayMs;
            _shutdownReason = shutdownReason;

            // Subscribe to system events
            _eventBus.Subscribe("system.start", OnSystemStart);
            _eventBus.Subscribe("timer.shutdown.cancel", OnShutdownCancel);
            _eventBus.Subscribe("timer.shutdown.extend", OnShutdownExtend);
            _eventBus.Subscribe("system.shutdown", OnSystemShutdown);

            _logger.LogInformation("⏰ Auto Shutdown Timer Service initialized - {DelaySeconds}s delay", 
                _shutdownDelayMs / 1000);
        }

        private Task OnSystemStart(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🚀 System started - initializing {DelaySeconds}s shutdown timer", 
                _shutdownDelayMs / 1000);

            // Start the shutdown timer
            _shutdownTimer = new System.Threading.Timer(
                OnTimerElapsed,
                null,
                TimeSpan.FromMilliseconds(_shutdownDelayMs),
                Timeout.InfiniteTimeSpan); // Run only once

            // Emit timer started event (fire and forget)
            _ = Task.Run(async () =>
            {
                await _eventBus.EmitAsync("timer.shutdown.started", new
                {
                    delayMs = _shutdownDelayMs,
                    delaySeconds = _shutdownDelayMs / 1000,
                    reason = _shutdownReason,
                    startTime = DateTimeOffset.UtcNow,
                    estimatedShutdownTime = DateTimeOffset.UtcNow.AddMilliseconds(_shutdownDelayMs)
                });
            });

            _logger.LogInformation("⏱️ Shutdown timer started - system will auto-shutdown in {DelaySeconds}s", 
                _shutdownDelayMs / 1000);
        }

        private Task OnShutdownCancel(CxEventPayload cxEvent)
        {
            _logger.LogInformation("❌ Shutdown timer cancelled by request");
            
            _shutdownTimer?.Dispose();
            _shutdownTimer = null;

            _ = Task.Run(async () =>
            {
                await _eventBus.EmitAsync("timer.shutdown.cancelled", new
                {
                    reason = cxEvent.Data,
                    cancelledAt = DateTimeOffset.UtcNow
                });
            });
        }

        private Task OnShutdownExtend(CxEventPayload cxEvent)
        {
            var additionalMs = 30000; // Default 30 seconds extension
            
            if (cxEvent.Data is Dictionary<string, object> dict && dict.ContainsKey("additionalMs"))
            {
                if (int.TryParse(dict["additionalMs"]?.ToString(), out var parsed))
                {
                    additionalMs = parsed;
                }
            }

            _logger.LogInformation("⏰ Extending shutdown timer by {ExtensionSeconds}s", additionalMs / 1000);

            // Cancel existing timer
            _shutdownTimer?.Dispose();

            // Start new timer with extended delay
            _shutdownTimer = new System.Threading.Timer(
                OnTimerElapsed,
                null,
                TimeSpan.FromMilliseconds(additionalMs),
                Timeout.InfiniteTimeSpan);

            _ = Task.Run(async () =>
            {
                await _eventBus.EmitAsync("timer.shutdown.extended", new
                {
                    additionalMs = additionalMs,
                    additionalSeconds = additionalMs / 1000,
                    newEstimatedShutdownTime = DateTimeOffset.UtcNow.AddMilliseconds(additionalMs)
                });
            });
        }

        private async void OnTimerElapsed(object? state)
        {
            try
            {
                _logger.LogInformation("⏰ Auto shutdown timer elapsed - initiating graceful system shutdown");

                // Emit shutdown timer completion event first
                await _eventBus.EmitAsync("timer.shutdown.elapsed", new
                {
                    reason = _shutdownReason,
                    elapsedAt = DateTimeOffset.UtcNow,
                    originalDelayMs = _shutdownDelayMs
                });

                // Small delay to allow event processing
                await Task.Delay(500);

                // Initiate system shutdown
                await _eventBus.EmitAsync("system.shutdown", new
                {
                    reason = _shutdownReason,
                    source = "auto_timer",
                    shutdownTime = DateTimeOffset.UtcNow
                });

                _logger.LogInformation("✅ Auto shutdown initiated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during auto shutdown timer elapsed");
            }
        }

        private Task OnSystemShutdown(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🛑 System shutdown event received - disposing timer");
            
            // Cancel timer since system is shutting down
            _shutdownTimer?.Dispose();
            _shutdownTimer = null;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _logger.LogInformation("🔄 Auto Shutdown Timer Service disposing");
                
                _shutdownTimer?.Dispose();
                _shutdownTimer = null;
                
                // Note: We don't unsubscribe from events as the event bus will be disposed
                
                _disposed = true;
                _logger.LogInformation("✅ Auto Shutdown Timer Service disposed");
            }
        }
    }
}
