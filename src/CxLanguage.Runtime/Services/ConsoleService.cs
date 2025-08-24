using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime.Services
{
    /// <summary>
    /// Handles system.console.write and system.console.read events for console I/O with consciousness-aware patterns
    /// </summary>
    public class ConsoleService
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<ConsoleService> _logger;

        public ConsoleService(ICxEventBus eventBus, ILogger<ConsoleService> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to console write and read events
            _eventBus.Subscribe("system.console.write", HandleConsoleWriteAsync);
            _eventBus.Subscribe("system.console.read", HandleConsoleReadAsync);
            _logger.LogInformation("ConsoleService subscribed to 'system.console.write' and 'system.console.read'");
        }

        /// <summary>
        /// Handler for 'system.console.write' event
        /// Supports payloads: { text: string } or { object: any }
        /// </summary>
        private Task<bool> HandleConsoleWriteAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                if (payload.TryGetValue("text", out var textObj) && textObj is not null)
                {
                    Console.WriteLine(textObj?.ToString() ?? string.Empty);
                    return Task.FromResult(true);
                }

                if (payload.TryGetValue("object", out var anyObj))
                {
                    if (anyObj is null)
                    {
                        Console.WriteLine("null");
                        return Task.FromResult(true);
                    }

                    // Use CX-aware printer for entities and complex structures
                    CxPrint.Print(anyObj);
                    return Task.FromResult(true);
                }

                // Fallback: print entire payload (useful for debugging)
                if (payload.Count > 0)
                {
                    CxPrint.Print(new Dictionary<string, object>(payload));
                    return Task.FromResult(true);
                }

                // Nothing to print
                _logger.LogDebug("system.console.write received with empty payload");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.console.write event");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Handler for 'system.console.read' event
        /// Supports payloads: { prompt: string, handlers: array } (both optional)
        /// Invokes custom handlers with captured input
        /// </summary>
        private async Task<bool> HandleConsoleReadAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Display optional prompt if provided
                if (payload.TryGetValue("prompt", out var promptObj) && promptObj is not null)
                {
                    var prompt = promptObj.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(prompt))
                    {
                        Console.Write(prompt);
                    }
                }

                // Read input from console asynchronously
                var input = await ReadLineAsync();

                // Emit custom handlers if provided
                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                {
                    foreach (var handlerObj in handlers)
                    {
                        if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                        {
                            var handlerPayload = new Dictionary<string, object> { { "input", input ?? string.Empty } };
                            await _eventBus.EmitAsync(handlerName, handlerPayload);
                            _logger.LogDebug("Emitted handler event: {HandlerName} with input", handlerName);
                        }
                    }
                }

                _logger.LogDebug("system.console.read processed, input length: {InputLength}", input?.Length ?? 0);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.console.read event");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously reads a line from the console
        /// </summary>
        private static Task<string?> ReadLineAsync()
        {
            return Task.Run(() => Console.ReadLine());
        }
    }
}
