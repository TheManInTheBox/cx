using CxLanguage.Runtime;
using CxLanguage.StandardLibrary.AI.Wait;
using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.EventBridges;

/// <summary>
/// Event bridge for AwaitService - connects await events to actual timing operations
/// </summary>
public class AwaitEventBridge
{
    private readonly AwaitService _awaitService;
    private readonly ICxEventBus _eventBus;
    private readonly ILogger<AwaitEventBridge> _logger;
    private readonly Dictionary<string, TaskCompletionSource> _pendingAwaits;

    public AwaitEventBridge(
        AwaitService awaitService,
        ICxEventBus eventBus,
        ILogger<AwaitEventBridge> logger)
    {
        _awaitService = awaitService ?? throw new ArgumentNullException(nameof(awaitService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pendingAwaits = new Dictionary<string, TaskCompletionSource>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🔗 Initializing AwaitEventBridge...");

            // Subscribe to await request events from CX runtime
            _eventBus.Subscribe("ai.await.request", OnAwaitRequest);

            _logger.LogInformation("✅ AwaitEventBridge initialized successfully");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing AwaitEventBridge: {Error}", ex.Message);
        }
    }

            _logger.LogInformation("✅ AwaitEventBridge initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize AwaitEventBridge: {Error}", ex.Message);
            throw;
        }
    }

    public Task StartAsync()
    {
        _logger.LogInformation("🚀 AwaitEventBridge started");
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _logger.LogInformation("🛑 AwaitEventBridge stopped");
        return Task.CompletedTask;
    }

    private async Task ProcessAwaitRequestAsync(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("⏰ Processing await request: {EventName}", cxEvent.Name);

            // Extract await parameters from event payload
            var payload = cxEvent.Payload as Dictionary<string, object> ?? new Dictionary<string, object>();
            
            var reason = payload.GetValueOrDefault("reason", "unknown")?.ToString() ?? "unknown";
            var context = payload.GetValueOrDefault("context")?.ToString() ?? "";
            var minDurationMs = GetIntValue(payload, "minDurationMs", 2000);
            var maxDurationMs = GetIntValue(payload, "maxDurationMs", 2000);
            var handlers = payload.GetValueOrDefault("handlers") as object[] ?? Array.Empty<object>();

            _logger.LogInformation("⏰ AWAIT: Reason={Reason}, Duration={MinDuration}-{MaxDuration}ms", 
                reason, minDurationMs, maxDurationMs);

            // Execute await with smart timing if min/max differ, otherwise use simple await
            AwaitResult result;
            if (minDurationMs == maxDurationMs)
            {
                result = await _awaitService.AwaitAsync(reason, context, minDurationMs);
            }
            else
            {
                result = await _awaitService.SmartAwaitAsync(reason, context, minDurationMs, maxDurationMs);
            }

            _logger.LogInformation("✅ AWAIT: Completed - Duration={ActualDuration}ms", result.ActualDurationMs);

            // Emit handler events after await completion
            await EmitHandlerEventsAsync(handlers, result);

            // Emit await completion event
            await _eventBus.EmitAsync("ai.await.response", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing await request: {Error}", ex.Message);
            
            // Emit error event
            await _eventBus.EmitAsync("ai.await.error", new
            {
                error = ex.Message,
                timestamp = DateTimeOffset.UtcNow
            });
        }
    }

    private async Task EmitHandlerEventsAsync(object[] handlers, AwaitResult result)
    {
        foreach (var handler in handlers)
        {
            try
            {
                if (handler is string handlerName)
                {
                    _logger.LogInformation("🎯 Emitting handler event: {HandlerName}", handlerName);
                    await _eventBus.EmitAsync(handlerName, result);
                }
                else if (handler is Dictionary<string, object> handlerObj)
                {
                    // Handle complex handler objects with custom payloads
                    var eventName = handlerObj.Keys.FirstOrDefault();
                    if (!string.IsNullOrEmpty(eventName))
                    {
                        var eventPayload = handlerObj[eventName];
                        _logger.LogInformation("🎯 Emitting handler event with payload: {HandlerName}", eventName);
                        await _eventBus.EmitAsync(eventName, eventPayload);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error emitting handler event: {Error}", ex.Message);
            }
        }
    }

    private static int GetIntValue(Dictionary<string, object> payload, string key, int defaultValue)
    {
        if (!payload.TryGetValue(key, out var value))
            return defaultValue;

        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            string strValue when int.TryParse(strValue, out var parsed) => parsed,
            _ => defaultValue
        };
    }
}
