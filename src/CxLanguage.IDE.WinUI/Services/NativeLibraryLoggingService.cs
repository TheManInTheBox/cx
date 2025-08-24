using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace CxLanguage.IDE.WinUI.Services;

/// <summary>
/// Service to configure and manage logging levels for native libraries
/// GitHub Issue #229: GGUF Model Metadata Spam fix
/// </summary>
public interface INativeLibraryLoggingService
{
    void ConfigureGGUFLogging();
    void RedirectNativeOutput();
    void SetNativeLogLevel(string component, LogLevel level);
}

/// <summary>
/// Implementation of native library logging configuration
/// </summary>
public class NativeLibraryLoggingService : INativeLibraryLoggingService
{
    private readonly ILogger<NativeLibraryLoggingService> _logger;

    public NativeLibraryLoggingService(ILogger<NativeLibraryLoggingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Configure GGUF library logging to appropriate levels
    /// </summary>
    public void ConfigureGGUFLogging()
    {
        try
        {
            // Set environment variables to control native GGUF logging
            Environment.SetEnvironmentVariable("LLAMA_LOG_LEVEL", "INFO");
            Environment.SetEnvironmentVariable("GGUF_LOG_LEVEL", "INFO");
            
            // Redirect stderr for GGUF metadata to a custom handler
            RedirectNativeOutput();
            
            _logger.LogDebug("✅ GGUF native library logging configured to reduce metadata spam");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to configure GGUF logging: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Redirect native library output to custom handlers
    /// </summary>
    public void RedirectNativeOutput()
    {
        try
        {
            // Create a custom console output interceptor for native libraries
            // This will filter out metadata spam while preserving actual errors
            var originalOut = Console.Out;
            var originalError = Console.Error;
            
            var filteredOut = new FilteredTextWriter(originalOut, FilterNativeOutput);
            var filteredError = new FilteredTextWriter(originalError, FilterNativeErrors);
            
            Console.SetOut(filteredOut);
            Console.SetError(filteredError);
            
            _logger.LogDebug("✅ Native library output filtering enabled");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to redirect native output: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Set logging level for specific native components
    /// </summary>
    public void SetNativeLogLevel(string component, LogLevel level)
    {
        try
        {
            var envVarName = $"{component.ToUpper()}_LOG_LEVEL";
            var levelValue = level switch
            {
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "FATAL",
                _ => "INFO"
            };
            
            Environment.SetEnvironmentVariable(envVarName, levelValue);
            _logger.LogDebug("Set {Component} log level to {Level}", component, levelValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set log level for {Component}: {Error}", component, ex.Message);
        }
    }

    /// <summary>
    /// Filter native output to reduce spam
    /// </summary>
    private bool FilterNativeOutput(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
            
        // Filter out common metadata spam patterns
        var spamPatterns = new[]
        {
            "llama_model_loader: - kv",
            "llama_model_loader: loaded meta data",
            "llm_load_print_meta:",
            "model size =",
            "model params =",
            "backend buffer",
            "kv",
            "general.architecture",
            "general.type"
        };
        
        return !spamPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Filter native error output to preserve real errors while reducing metadata spam
    /// </summary>
    private bool FilterNativeErrors(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
            
        // Always allow actual error messages
        var errorIndicators = new[]
        {
            "error",
            "exception",
            "failed",
            "cannot",
            "unable",
            "invalid"
        };
        
        // Check if this is a real error
        if (errorIndicators.Any(indicator => text.Contains(indicator, StringComparison.OrdinalIgnoreCase)))
        {
            // But filter out metadata "errors" that are actually informational
            var metadataPatterns = new[]
            {
                "llama_model_loader:",
                "llm_load_print_meta:",
                "backend buffer",
                "model size",
                "model params"
            };
            
            if (metadataPatterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            {
                return false; // Filter out metadata "errors"
            }
            
            return true; // Allow real errors
        }
        
        return FilterNativeOutput(text); // Use same filtering as standard output
    }
}

/// <summary>
/// Custom TextWriter that filters output before writing to the underlying writer
/// </summary>
public class FilteredTextWriter : TextWriter
{
    private readonly TextWriter _underlying;
    private readonly Func<string, bool> _filter;

    public FilteredTextWriter(TextWriter underlying, Func<string, bool> filter)
    {
        _underlying = underlying;
        _filter = filter;
    }

    public override Encoding Encoding => _underlying.Encoding;

    public override void Write(char value)
    {
        _underlying.Write(value);
    }

    public override void Write(string? value)
    {
        if (value != null && _filter(value))
        {
            _underlying.Write(value);
        }
    }

    public override void WriteLine(string? value)
    {
        if (value != null && _filter(value))
        {
            _underlying.WriteLine(value);
        }
    }

    public override Task WriteAsync(string? value)
    {
        if (value != null && _filter(value))
        {
            return _underlying.WriteAsync(value);
        }
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(string? value)
    {
        if (value != null && _filter(value))
        {
            return _underlying.WriteLineAsync(value);
        }
        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _underlying?.Dispose();
        }
        base.Dispose(disposing);
    }
}
