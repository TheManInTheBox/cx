using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Services
{
    /// <summary>
    /// Real-time keyboard input service for consciousness-aware console interaction
    /// Dr. Elena "CoreKernel" Rodriguez - System I/O Integration Architecture
    /// </summary>
    public class ConsoleInputService : BackgroundService
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<ConsoleInputService> _logger;
        private readonly object _inputLock = new object();
        private bool _isPromptActive = false;
        private string _currentPrompt = "👤 Enter input: ";

        public ConsoleInputService(ICxEventBus eventBus, ILogger<ConsoleInputService> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🎯 Console Input Service starting - real-time keyboard integration");
            
            // Register for console prompt events from CX scripts
            _eventBus.Subscribe("console.prompt", OnConsolePromptRequested);
            _eventBus.Subscribe("console.input.start", OnInputSessionStart);
            _eventBus.Subscribe("console.input.stop", OnInputSessionStop);
            
            _logger.LogInformation("✅ Console Input Service registered for consciousness events");

            try
            {
                // Main keyboard input processing loop
                await ProcessKeyboardInputAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🔄 Console Input Service shutting down gracefully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Console Input Service encountered an error");
            }
        }

        private async Task ProcessKeyboardInputAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("⌨️ Keyboard input processing started - waiting for consciousness activation");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Wait for prompt activation from consciousness framework
                    if (_isPromptActive)
                    {
                        // Display prompt and read user input
                        Console.Write(_currentPrompt);
                        
                        // Read input from keyboard (blocking operation)
                        var userInput = await ReadLineAsync(stoppingToken);
                        
                        if (!string.IsNullOrEmpty(userInput))
                        {
                            // Emit keyboard input as consciousness event
                            EmitKeyboardInput(userInput);
                        }
                        
                        // Reset prompt state - consciousness will re-activate if needed
                        _isPromptActive = false;
                    }
                    else
                    {
                        // Wait briefly for prompt activation
                        await Task.Delay(100, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing keyboard input");
                    await Task.Delay(1000, stoppingToken); // Brief recovery delay
                }
            }
        }

        private async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            // Thread-safe keyboard input reading
            return await Task.Run(() =>
            {
                try
                {
                    return Console.ReadLine();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error reading console input");
                    return null;
                }
            }, cancellationToken);
        }

        private void EmitKeyboardInput(string userInput)
        {
            lock (_inputLock)
            {
                try
                {
                    _logger.LogInformation("⌨️ Keyboard input received: {Input}", userInput);
                    
                    // Convert keyboard input to consciousness event
                    var inputEvent = new
                    {
                        text = userInput,
                        source = "keyboard",
                        timestamp = DateTime.UtcNow,
                        sessionId = Guid.NewGuid().ToString("N")[..8]
                    };

                    // Emit to consciousness framework
                    _eventBus.EmitAsync("console.input", inputEvent);
                    
                    _logger.LogInformation("✅ Consciousness event emitted: console.input");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to emit keyboard input event");
                }
            }
        }

        private Task OnConsolePromptRequested(CxEventPayload cxEvent)
        {
            _logger.LogInformation("📝 Console prompt requested by consciousness framework");
            
            // Extract custom prompt if provided
            if (cxEvent.Data is IDictionary<string, object> data && data.ContainsKey("prompt"))
            {
                _currentPrompt = data["prompt"]?.ToString() ?? "👤 Enter input: ";
            }
            else
            {
                _currentPrompt = "👤 Enter input: ";
            }
            
            // Activate prompt for keyboard input processing
            _isPromptActive = true;
            _logger.LogInformation("⌨️ Keyboard input prompt activated: {Prompt}", _currentPrompt);
        }

        private Task OnInputSessionStart(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🚀 Console input session starting");
            _isPromptActive = true;
        }

        private Task OnInputSessionStop(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🔄 Console input session stopping");
            _isPromptActive = false;
        }

        public override void Dispose()
        {
            _logger.LogInformation("🔄 Console Input Service disposing");
            
            // Note: ICxEventBus doesn't have Unsubscribe method - events will naturally stop when service stops
            _logger.LogInformation("✅ Console Input Service disposed");
            
            base.Dispose();
        }
    }
}
