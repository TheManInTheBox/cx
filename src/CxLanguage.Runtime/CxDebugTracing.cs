using System;
using System.Diagnostics;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Switchable debug tracing system for CX Language runtime
    /// </summary>
    public static class CxDebugTracing
    {
        private static bool _isEnabled = false;
        private static bool _showStackTrace = false;
        private static bool _showTimestamps = true;
        private static string _logLevel = "INFO";

        /// <summary>
        /// Enable or disable debug tracing
        /// </summary>
        public static bool IsEnabled 
        { 
            get => _isEnabled; 
            set => _isEnabled = value; 
        }

        /// <summary>
        /// Show stack trace in debug output
        /// </summary>
        public static bool ShowStackTrace 
        { 
            get => _showStackTrace; 
            set => _showStackTrace = value; 
        }

        /// <summary>
        /// Show timestamps in debug output
        /// </summary>
        public static bool ShowTimestamps 
        { 
            get => _showTimestamps; 
            set => _showTimestamps = value; 
        }

        /// <summary>
        /// Current log level (DEBUG, INFO, WARN, ERROR)
        /// </summary>
        public static string LogLevel 
        { 
            get => _logLevel; 
            set => _logLevel = value?.ToUpper() ?? "INFO"; 
        }

        /// <summary>
        /// Configure debug tracing with environment variables
        /// </summary>
        public static void Initialize()
        {
            // Check environment variables for debug configuration
            IsEnabled = Environment.GetEnvironmentVariable("CX_DEBUG_ENABLED")?.ToLower() == "true";
            ShowStackTrace = Environment.GetEnvironmentVariable("CX_DEBUG_STACKTRACE")?.ToLower() == "true";
            ShowTimestamps = Environment.GetEnvironmentVariable("CX_DEBUG_TIMESTAMPS")?.ToLower() != "false";
            LogLevel = Environment.GetEnvironmentVariable("CX_DEBUG_LEVEL") ?? "INFO";

            if (IsEnabled)
            {
                TraceInfo("CX Debug Tracing", "Debug tracing initialized", new { IsEnabled, ShowStackTrace, ShowTimestamps, LogLevel });
            }
        }

        /// <summary>
        /// Trace debug information
        /// </summary>
        public static void TraceDebug(string component, string message, object? data = null)
        {
            if (IsEnabled && ShouldLog("DEBUG"))
            {
                WriteTrace("DEBUG", component, message, data);
            }
        }

        /// <summary>
        /// Trace informational messages
        /// </summary>
        public static void TraceInfo(string component, string message, object? data = null)
        {
            if (IsEnabled && ShouldLog("INFO"))
            {
                WriteTrace("INFO", component, message, data);
            }
        }

        /// <summary>
        /// Trace warning messages
        /// </summary>
        public static void TraceWarning(string component, string message, object? data = null)
        {
            if (IsEnabled && ShouldLog("WARN"))
            {
                WriteTrace("WARN", component, message, data);
            }
        }

        /// <summary>
        /// Trace error messages
        /// </summary>
        public static void TraceError(string component, string message, object? data = null)
        {
            if (IsEnabled && ShouldLog("ERROR"))
            {
                WriteTrace("ERROR", component, message, data);
            }
        }

        /// <summary>
        /// Trace CX source line to IL mapping
        /// </summary>
        public static void TraceCxToIL(string cxLine, string ilInstruction, object? context = null)
        {
            if (IsEnabled && ShouldLog("DEBUG"))
            {
                WriteTrace("CX->IL", "Code Translation", $"CX: {cxLine} → IL: {ilInstruction}", context);
            }
        }

        /// <summary>
        /// Trace event system operations
        /// </summary>
        public static void TraceEvent(string eventName, string operation, object? payload = null)
        {
            if (IsEnabled && ShouldLog("DEBUG"))
            {
                WriteTrace("EVENT", operation, $"Event: {eventName}", payload);
            }
        }

        /// <summary>
        /// Trace cognitive service operations
        /// </summary>
        public static void TraceCognitive(string service, string operation, object? data = null)
        {
            if (IsEnabled && ShouldLog("INFO"))
            {
                WriteTrace("COGNITIVE", service, operation, data);
            }
        }

        /// <summary>
        /// Trace runtime operations
        /// </summary>
        public static void TraceRuntime(string component, string operation, object? data = null)
        {
            if (IsEnabled && ShouldLog("DEBUG"))
            {
                WriteTrace("RUNTIME", component, operation, data);
            }
        }

        private static bool ShouldLog(string level)
        {
            var levels = new[] { "DEBUG", "INFO", "WARN", "ERROR" };
            var currentIndex = Array.IndexOf(levels, LogLevel);
            var messageIndex = Array.IndexOf(levels, level);
            return messageIndex >= currentIndex;
        }

        private static void WriteTrace(string level, string component, string message, object? data)
        {
            try
            {
                var timestamp = ShowTimestamps ? $"[{DateTime.Now:HH:mm:ss.fff}] " : "";
                var prefix = $"{timestamp}[{level}] [{component}]";
                
                Console.WriteLine($"{prefix} {message}");
                
                if (data != null)
                {
                    Console.WriteLine($"{prefix} Data: {System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
                }

                if (ShowStackTrace && level == "ERROR")
                {
                    var stackTrace = new StackTrace(true);
                    Console.WriteLine($"{prefix} Stack Trace:");
                    foreach (var frame in stackTrace.GetFrames() ?? Array.Empty<StackFrame>())
                    {
                        var method = frame.GetMethod();
                        var fileName = frame.GetFileName();
                        var lineNumber = frame.GetFileLineNumber();
                        Console.WriteLine($"{prefix}   at {method?.DeclaringType?.Name}.{method?.Name} in {fileName}:line {lineNumber}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback if tracing itself fails
                Console.WriteLine($"[ERROR] [TRACE] Failed to write trace: {ex.Message}");
            }
        }
    }
}

