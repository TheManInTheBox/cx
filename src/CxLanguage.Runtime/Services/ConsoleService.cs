using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime.Services
{
    /// <summary>
    /// Handles system.console.write, system.console.read, system.console.clear, and system.console.color events for console I/O with consciousness-aware patterns
    /// </summary>
    public class ConsoleService
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<ConsoleService> _logger;

        // Console.ConsoleColor mapping for proper cross-platform color support
        private static readonly Dictionary<string, ConsoleColor> ColorMapping = new()
        {
            { "black", ConsoleColor.Black },
            { "red", ConsoleColor.Red },
            { "green", ConsoleColor.Green },
            { "yellow", ConsoleColor.Yellow },
            { "blue", ConsoleColor.Blue },
            { "magenta", ConsoleColor.Magenta },
            { "cyan", ConsoleColor.Cyan },
            { "white", ConsoleColor.White },
            { "darkblue", ConsoleColor.DarkBlue },
            { "darkgreen", ConsoleColor.DarkGreen },
            { "darkcyan", ConsoleColor.DarkCyan },
            { "darkred", ConsoleColor.DarkRed },
            { "darkmagenta", ConsoleColor.DarkMagenta },
            { "darkyellow", ConsoleColor.DarkYellow },
            { "gray", ConsoleColor.Gray },
            { "darkgray", ConsoleColor.DarkGray }
        };

        // Store original colors for reset functionality
        private static readonly ConsoleColor OriginalForegroundColor = Console.ForegroundColor;
        private static readonly ConsoleColor OriginalBackgroundColor = Console.BackgroundColor;

        public ConsoleService(ICxEventBus eventBus, ILogger<ConsoleService> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to console write, read, and clear events
            _eventBus.Subscribe("system.console.write", HandleConsoleWriteAsync);
            _eventBus.Subscribe("system.console.read", HandleConsoleReadAsync);
            _eventBus.Subscribe("system.console.clear", HandleConsoleClearAsync);
            _logger.LogInformation("ConsoleService subscribed to console events with integrated color support");
        }

        /// <summary>
        /// Handler for 'system.console.write' event
        /// Supports payloads: { text: string, foregroundColor: string, backgroundColor: string } or { object: any }
        /// Colors are applied temporarily for this write operation only
        /// </summary>
        private Task<bool> HandleConsoleWriteAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Store original colors to restore after writing
                var originalForeground = Console.ForegroundColor;
                var originalBackground = Console.BackgroundColor;
                var colorsChanged = false;

                // Apply temporary foreground color if specified
                if (payload.TryGetValue("foregroundColor", out var fgColorObj) && fgColorObj is string fgColor)
                {
                    var lowerColor = fgColor.ToLower();
                    if (ColorMapping.TryGetValue(lowerColor, out var consoleColor))
                    {
                        Console.ForegroundColor = consoleColor;
                        colorsChanged = true;
                        _logger.LogDebug("Applied temporary foreground color: {Color}", fgColor);
                    }
                    else
                    {
                        _logger.LogWarning("Unknown foreground color: {Color}. Supported colors: {SupportedColors}", 
                            fgColor, string.Join(", ", ColorMapping.Keys));
                    }
                }

                // Apply temporary background color if specified
                if (payload.TryGetValue("backgroundColor", out var bgColorObj) && bgColorObj is string bgColor)
                {
                    var lowerColor = bgColor.ToLower();
                    if (ColorMapping.TryGetValue(lowerColor, out var consoleColor))
                    {
                        Console.BackgroundColor = consoleColor;
                        colorsChanged = true;
                        _logger.LogDebug("Applied temporary background color: {Color}", bgColor);
                    }
                    else
                    {
                        _logger.LogWarning("Unknown background color: {Color}. Supported colors: {SupportedColors}", 
                            bgColor, string.Join(", ", ColorMapping.Keys));
                    }
                }

                // Handle text output
                if (payload.TryGetValue("text", out var textObj) && textObj is not null)
                {
                    Console.WriteLine(textObj?.ToString() ?? string.Empty);
                    
                    // Restore original colors if they were changed
                    if (colorsChanged)
                    {
                        Console.ForegroundColor = originalForeground;
                        Console.BackgroundColor = originalBackground;
                    }
                    
                    return Task.FromResult(true);
                }

                // Handle object output
                if (payload.TryGetValue("object", out var anyObj))
                {
                    if (anyObj is null)
                    {
                        Console.WriteLine("null");
                    }
                    else
                    {
                        // Use CX-aware printer for entities and complex structures
                        CxPrint.Print(anyObj);
                    }
                    
                    // Restore original colors if they were changed
                    if (colorsChanged)
                    {
                        Console.ForegroundColor = originalForeground;
                        Console.BackgroundColor = originalBackground;
                    }
                    
                    return Task.FromResult(true);
                }

                // Fallback: print entire payload (useful for debugging)
                if (payload.Count > 0)
                {
                    CxPrint.Print(new Dictionary<string, object>(payload));
                    
                    // Restore original colors if they were changed
                    if (colorsChanged)
                    {
                        Console.ForegroundColor = originalForeground;
                        Console.BackgroundColor = originalBackground;
                    }
                    
                    return Task.FromResult(true);
                }

                // Nothing to print
                _logger.LogDebug("system.console.write received with empty payload");
                
                // Restore original colors if they were changed (even with no output)
                if (colorsChanged)
                {
                    Console.ForegroundColor = originalForeground;
                    Console.BackgroundColor = originalBackground;
                }
                
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

        /// <summary>
        /// Handler for 'system.console.clear' event
        /// Clears the console screen immediately with cross-platform compatibility
        /// </summary>
        private Task<bool> HandleConsoleClearAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                // Clear the console screen using the standard Console.Clear() method
                // This works cross-platform (Windows, Linux, macOS)
                Console.Clear();
                
                _logger.LogDebug("Console screen cleared successfully");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.console.clear event");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Handler for 'system.console.color.set' event
        /// Supports payloads: { foreground: string, background: string, r: int, g: int, b: int }
        /// Uses Console.ForegroundColor and Console.BackgroundColor for proper cross-platform support
        /// </summary>
        private Task<bool> HandleColorSetAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Handle named foreground colors
                if (payload.TryGetValue("foreground", out var foregroundObj) && foregroundObj is string foregroundColor)
                {
                    var lowerColor = foregroundColor.ToLower();
                    if (ColorMapping.TryGetValue(lowerColor, out var consoleColor))
                    {
                        Console.ForegroundColor = consoleColor;
                        _logger.LogDebug("Set foreground color: {Color}", foregroundColor);
                    }
                    else
                    {
                        _logger.LogWarning("Unknown foreground color: {Color}. Supported colors: {SupportedColors}", 
                            foregroundColor, string.Join(", ", ColorMapping.Keys));
                    }
                }

                // Handle named background colors
                if (payload.TryGetValue("background", out var backgroundObj) && backgroundObj is string backgroundColor)
                {
                    var lowerColor = backgroundColor.ToLower();
                    if (ColorMapping.TryGetValue(lowerColor, out var consoleColor))
                    {
                        Console.BackgroundColor = consoleColor;
                        _logger.LogDebug("Set background color: {Color}", backgroundColor);
                    }
                    else
                    {
                        _logger.LogWarning("Unknown background color: {Color}. Supported colors: {SupportedColors}", 
                            backgroundColor, string.Join(", ", ColorMapping.Keys));
                    }
                }

                // Handle RGB values (Note: Console colors don't support custom RGB, so we'll map to closest standard color)
                if (payload.TryGetValue("r", out var rObj) && 
                    payload.TryGetValue("g", out var gObj) && 
                    payload.TryGetValue("b", out var bObj))
                {
                    if (int.TryParse(rObj?.ToString(), out var r) &&
                        int.TryParse(gObj?.ToString(), out var g) &&
                        int.TryParse(bObj?.ToString(), out var b))
                    {
                        // Map RGB to closest Console color
                        var closestColor = MapRgbToConsoleColor(r, g, b);
                        Console.ForegroundColor = closestColor;
                        _logger.LogDebug("Mapped RGB({R},{G},{B}) to console color: {Color}", r, g, b, closestColor);
                    }
                }

                // Handle hex color values (map to closest Console color)
                if (payload.TryGetValue("hex", out var hexObj) && hexObj is string hexValue)
                {
                    if (TryParseHexColor(hexValue, out var r, out var g, out var b))
                    {
                        var closestColor = MapRgbToConsoleColor(r, g, b);
                        Console.ForegroundColor = closestColor;
                        _logger.LogDebug("Mapped hex color {Hex} -> RGB({R},{G},{B}) to console color: {Color}", 
                            hexValue, r, g, b, closestColor);
                    }
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.console.color.set event");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Handler for 'system.console.color.reset' event
        /// Resets console colors to their original values
        /// </summary>
        private Task<bool> HandleColorResetAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                Console.ForegroundColor = OriginalForegroundColor;
                Console.BackgroundColor = OriginalBackgroundColor;
                _logger.LogDebug("Console colors reset to original values");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling system.console.color.reset event");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Maps RGB values to the closest available ConsoleColor
        /// </summary>
        private static ConsoleColor MapRgbToConsoleColor(int r, int g, int b)
        {
            // Simple RGB to ConsoleColor mapping based on dominant colors
            var red = r > 128;
            var green = g > 128;
            var blue = b > 128;

            return (red, green, blue) switch
            {
                (false, false, false) => ConsoleColor.Black,
                (true, false, false) => ConsoleColor.Red,
                (false, true, false) => ConsoleColor.Green,
                (true, true, false) => ConsoleColor.Yellow,
                (false, false, true) => ConsoleColor.Blue,
                (true, false, true) => ConsoleColor.Magenta,
                (false, true, true) => ConsoleColor.Cyan,
                (true, true, true) => ConsoleColor.White
            };
        }

        /// <summary>
        /// Parses hex color string to RGB components
        /// Supports formats: #RGB, #RRGGBB, RGB, RRGGBB
        /// </summary>
        private static bool TryParseHexColor(string hex, out int r, out int g, out int b)
        {
            r = g = b = 0;

            if (string.IsNullOrEmpty(hex))
                return false;

            // Remove # prefix if present
            hex = hex.TrimStart('#');

            try
            {
                if (hex.Length == 3)
                {
                    // Short format: RGB -> RRGGBB
                    r = Convert.ToInt32(hex.Substring(0, 1) + hex.Substring(0, 1), 16);
                    g = Convert.ToInt32(hex.Substring(1, 1) + hex.Substring(1, 1), 16);
                    b = Convert.ToInt32(hex.Substring(2, 1) + hex.Substring(2, 1), 16);
                    return true;
                }
                else if (hex.Length == 6)
                {
                    // Full format: RRGGBB
                    r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return true;
                }
            }
            catch
            {
                // Invalid hex format
            }

            return false;
        }
    }
}
