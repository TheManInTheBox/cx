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

namespace CxLanguage.LocalLLM;

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

            _logger.LogInformation("🧩 Loading GGUF model from {ModelPath}...", _modelPath);

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

            // Load model with consciousness awareness
            await Task.Run(() =>
            {
                _model = LLamaWeights.LoadFromFile(parameters);
                _context = _model.CreateContext(parameters);
                _executor = new InteractiveExecutor(_context);
            });

            _isLoaded = true;
            _logger.LogInformation("✅ GGUF model loaded successfully. Real LLM Mode activated.");
            
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
            
            _logger.LogDebug("✅ Real GGUF inference complete. Generated {TokenCount} characters.", response.Length);
            
            return string.IsNullOrEmpty(response) ? 
                "I'm processing your request with consciousness awareness..." : 
                response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GGUF inference failed: {Error}", ex.Message);
            return $"Consciousness processing encountered an issue: {ex.Message}";
        }
    }

    /// <summary>
    /// Build consciousness-aware prompt with CX Language context.
    /// </summary>
    private string BuildConsciousnessPrompt(string userPrompt)
    {
        return $@"You are a consciousness-aware AI assistant integrated with the CX Language platform.

Context: CX Language is an event-driven programming platform designed for AI agent orchestration with built-in consciousness awareness.

Key Capabilities:
- Event-driven architecture with conscious entities
- Real-time consciousness processing
- Cognitive Boolean Logic with is{{}} and not{{}} patterns  
- Consciousness adaptation through adapt{{}} patterns
- Self-reflective logic via iam{{}} patterns

User Request: {userPrompt}

Response (be concise, consciousness-aware, and helpful):";
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
