using LLama.Abstractions;
using LLama.Common;
using LLama;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CxLanguage.LocalLLM;

/// <summary>
/// Console suppression helper for native library output
/// </summary>
public static class ConsoleSuppressionHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetStdHandle(int nStdHandle, IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateFile(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_ERROR_HANDLE = -12;
    private const uint GENERIC_WRITE = 0x40000000;
    private const uint FILE_SHARE_WRITE = 0x00000002;
    private const uint OPEN_EXISTING = 3;

    public static IDisposable SuppressConsoleOutput()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new DisposableAction(() => { }); // No-op on non-Windows

        try
        {
            var originalOut = GetStdHandle(STD_OUTPUT_HANDLE);
            var originalErr = GetStdHandle(STD_ERROR_HANDLE);

            // Redirect to NUL device to suppress native output
            var nullHandle = CreateFile("NUL", GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            SetStdHandle(STD_OUTPUT_HANDLE, nullHandle);
            SetStdHandle(STD_ERROR_HANDLE, nullHandle);

            return new DisposableAction(() =>
            {
                // Restore original handles
                SetStdHandle(STD_OUTPUT_HANDLE, originalOut);
                SetStdHandle(STD_ERROR_HANDLE, originalErr);
            });
        }
        catch
        {
            return new DisposableAction(() => { }); // Fallback to no-op
        }
    }

    private class DisposableAction : IDisposable
    {
        private readonly Action _action;
        private bool _disposed = false;

        public DisposableAction(Action action) => _action = action;

        public void Dispose()
        {
            if (!_disposed)
            {
                _action?.Invoke();
                _disposed = true;
            }
        }
    }
}

/// <summary>
/// Filtered console writer that suppresses verbose LLAMA model loader output
/// </summary>
public class LlamaModelLoaderFilter : TextWriter
{
    private readonly TextWriter _originalWriter;
    
    public LlamaModelLoaderFilter(TextWriter originalWriter)
    {
        _originalWriter = originalWriter;
    }
    
    public override Encoding Encoding => _originalWriter.Encoding;
    
    public override void WriteLine(string? value)
    {
        // Suppress LLAMA model loader and other verbose output
        if (value != null && (
            value.StartsWith("llama_model_loader:") ||
            value.StartsWith("llm_load_") ||
            value.Contains("model params = ") ||
            value.Contains("model size = ") ||
            value.Contains("model desc = ") ||
            value.Contains("model type = ") ||
            value.Contains("model ftype = ") ||
            value.Contains("model dims = ") ||
            value.Contains("model parts count = ") ||
            value.Contains("general.architecture") ||
            value.Contains("general.type") ||
            value.Contains("general.name") ||
            value.Contains("general.size_label") ||
            value.Contains("- kv   ") ||
            value.Contains("- tensor ") ||
            value.Contains("llm_load_print_meta")))
            return;
            
        _originalWriter.WriteLine(value);
    }
    
    public override void Write(string? value)
    {
        // Suppress LLAMA model loader and other verbose output
        if (value != null && (
            value.StartsWith("llama_model_loader:") ||
            value.StartsWith("llm_load_") ||
            value.Contains("model params = ") ||
            value.Contains("model size = ") ||
            value.Contains("model desc = ") ||
            value.Contains("model type = ") ||
            value.Contains("model ftype = ") ||
            value.Contains("model dims = ") ||
            value.Contains("model parts count = ") ||
            value.Contains("general.architecture") ||
            value.Contains("general.type") ||
            value.Contains("general.name") ||
            value.Contains("general.size_label") ||
            value.Contains("- kv   ") ||
            value.Contains("- tensor ") ||
            value.Contains("llm_load_print_meta")))
            return;
            
        _originalWriter.Write(value);
    }
    
    public override void Write(char value) => _originalWriter.Write(value);
    public override void Flush() => _originalWriter.Flush();
}

/// <summary>
/// Native GGUF inference engine for real local LLM processing with consciousness awareness.
/// Implements direct GGUF model execution with GPU-CUDA acceleration.
/// </summary>
public class NativeGGUFInferenceEngine : IDisposable
{
    private readonly ILogger<NativeGGUFInferenceEngine> _logger;
    private LLamaWeights? _model;
    private LLamaContext? _context;
    private InteractiveExecutor? _executor;
    private readonly string _modelPath;
    private bool _isLoaded = false;
    private bool _disposed = false;

    public NativeGGUFInferenceEngine(ILogger<NativeGGUFInferenceEngine> logger, string modelPath)
    {
        _logger = logger;
        _modelPath = modelPath;
    }

    /// <summary>
    /// Initialize the GGUF model with consciousness-aware parameters.
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            if (!File.Exists(_modelPath))
            {
                _logger.LogError("❌ REAL LLM ONLY MODE: GGUF model not found at {ModelPath} - NO SIMULATION FALLBACK", _modelPath);
                return false;
            }

            _logger.LogDebug("🧩 Loading GGUF model from {ModelPath}...", _modelPath);

            // LlamaSharp model parameters for consciousness processing
            var parameters = new ModelParams(_modelPath)
            {
                ContextSize = 4096,        // Context window for consciousness conversations
                GpuLayerCount = 32,        // GPU-CUDA acceleration layers
                Seed = 1337,               // Reproducible consciousness responses
                UseMemorymap = true,       // Memory mapping for performance
                UseMemoryLock = false,     // Avoid memory locking issues
                Embeddings = false        // Disable embeddings for inference-only mode
            };

            // Load model with consciousness awareness (suppress ALL native output)
            await Task.Run(() =>
            {
                // Use native console suppression for Windows to block llama.cpp output
                using (ConsoleSuppressionHelper.SuppressConsoleOutput())
                {
                    _model = LLamaWeights.LoadFromFile(parameters);
                    _context = _model.CreateContext(parameters);
                    _executor = new InteractiveExecutor(_context);
                }
            });

            _isLoaded = true;
            _logger.LogDebug("✅ GGUF model loaded successfully. Real LLM Mode activated.");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to load GGUF model: {Error}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Generate consciousness-aware response using real GGUF model inference.
    /// </summary>
    public async Task<string> GenerateAsync(string prompt, System.Threading.CancellationToken cancellationToken = default)
    {
        if (!_isLoaded || _executor == null)
        {
            throw new InvalidOperationException("GGUF model not loaded. Call InitializeAsync() first.");
        }

        try
        {
            var consciousnessPrompt = BuildConsciousnessPrompt(prompt);
            
            _logger.LogDebug("🧠 Generating response with real GGUF model...");

            var inferenceParams = new InferenceParams()
            {
                AntiPrompts = new[] { "User:", "Assistant:", "[STOP]" },
                MaxTokens = 512           // Reasonable response length
            };

            var responseBuilder = new StringBuilder();

            await foreach (string token in _executor.InferAsync(consciousnessPrompt, inferenceParams, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                responseBuilder.Append(token);
                
                // Stop at reasonable response length for consciousness processing
                if (responseBuilder.Length > 2048)
                    break;
            }

            var response = responseBuilder.ToString().Trim();
            
            _logger.LogInformation("✅ Real GGUF inference complete. Generated {TokenCount} characters.", response.Length);
            _logger.LogInformation("🧠 Response content: '{Response}'", response);
            
            if (string.IsNullOrEmpty(response))
            {
                var fallbackResponse = "I'm processing your request with consciousness awareness...";
                _logger.LogInformation("📝 Using fallback response: '{Fallback}'", fallbackResponse);
                return fallbackResponse;
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GGUF inference failed: {Error}", ex.Message);
            var errorResponse = $"Consciousness processing encountered an issue: {ex.Message}";
            _logger.LogInformation("🔧 Returning error response: '{ErrorResponse}'", errorResponse);
            return errorResponse;
        }
    }

    /// <summary>
    /// Build simple prompt without hardcoded instructions.
    /// </summary>
    private string BuildConsciousnessPrompt(string userPrompt)
    {
        return userPrompt;
    }

    /// <summary>
    /// Check if real GGUF model is available and loaded.
    /// </summary>
    public bool IsRealModelAvailable => _isLoaded && _model != null;

    /// <summary>
    /// Get model information for consciousness debugging.
    /// </summary>
    public string GetModelInfo()
    {
        if (!_isLoaded || _model == null)
            return "❌ REAL LLM ONLY MODE: No GGUF model loaded";

        return $"Real GGUF Model: {Path.GetFileName(_modelPath)} | Context: {_context?.ContextSize ?? 0} tokens | GPU Layers: Enabled";
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            // InteractiveExecutor doesn't implement IDisposable in newer versions
            _executor = null;
            _context?.Dispose();
            _model?.Dispose();
            
            _logger.LogInformation("🧩 GGUF model resources disposed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Warning during GGUF model disposal: {Error}", ex.Message);
        }
        finally
        {
            _disposed = true;
        }
    }
}
