using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CxLanguage.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace CxLanguage.StandardLibrary.Services;

/// <summary>
/// Local LLM service for consciousness-aware inference using GGUF models
/// Features real-time token streaming with consciousness integration
/// Implements Dr. Hayes Stream Fusion Architecture for optimal performance
/// </summary>
public interface ILocalLLMService
{
    /// <summary>
    /// Generate response using local LLM with consciousness-aware streaming
    /// </summary>
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stream tokens in real-time with consciousness processing
    /// </summary>
    IAsyncEnumerable<string> StreamAsync(string prompt, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Load a GGUF model for inference
    /// </summary>
    Task<bool> LoadModelAsync(string modelPath);
    
    /// <summary>
    /// Check if model is loaded and ready
    /// </summary>
    bool IsModelLoaded { get; }
    
    /// <summary>
    /// Get model information
    /// </summary>
    ModelInfo? ModelInfo { get; }
    
    /// <summary>
    /// Unload current model to free resources
    /// </summary>
    Task UnloadModelAsync();
}

/// <summary>
/// Information about the loaded model
/// </summary>
public record ModelInfo(string Name, string Path, long SizeBytes, string Architecture);

/// <summary>
/// Local LLM Service implementation with consciousness-aware processing
/// Uses Dr. Hayes Stream Fusion Architecture for real-time token streaming
/// Integrates with CX Language event system for consciousness coordination
/// </summary>
public class LocalLLMService : ILocalLLMService, IDisposable
{
    private readonly ILogger<LocalLLMService> _logger;
    private readonly ICxEventBus _eventBus;
    private readonly object _modelLock = new();
    private volatile bool _isDisposed;
    private volatile bool _isShuttingDown;
    private ModelInfo? _modelInfo;
    private string? _loadedModelPath;
    
    // Dr. Hayes Stream Fusion Components
    private readonly Channel<string> _tokenChannel;
    private readonly ChannelWriter<string> _tokenWriter;
    private readonly ChannelReader<string> _tokenReader;
    
    // Native GGUF Inference Engine - IL-generated custom inference
    private NativeGGUFInferenceEngine? _nativeEngine;
    private readonly Dictionary<string, TaskCompletionSource<string>> _pendingRequests = new();

    public LocalLLMService(ILogger<LocalLLMService> logger, ICxEventBus eventBus, ILoggerFactory? loggerFactory = null)
    {
        _logger = logger;
        _eventBus = eventBus;
        
        // Initialize Dr. Hayes Stream Fusion Channel with consciousness awareness
        var channelOptions = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        
        _tokenChannel = Channel.CreateBounded<string>(channelOptions);
        _tokenWriter = _tokenChannel.Writer;
        _tokenReader = _tokenChannel.Reader;
        
        // Initialize Native GGUF Inference Engine
        var nativeEngineLogger = loggerFactory?.CreateLogger<NativeGGUFInferenceEngine>() ?? 
                                 Microsoft.Extensions.Logging.Abstractions.NullLogger<NativeGGUFInferenceEngine>.Instance;
        _nativeEngine = new NativeGGUFInferenceEngine(nativeEngineLogger, eventBus);
        
        // Subscribe to consciousness system events
        _eventBus.Subscribe("system.shutdown", OnSystemShutdown);
        _eventBus.Subscribe("local.llm.load", OnLoadModel);
        _eventBus.Subscribe("local.llm.generate", OnGenerateRequest);
        _eventBus.Subscribe("local.llm.stream", OnStreamRequest);
        
        _logger.LogInformation("🧠 Local LLM Service initialized with IL-generated consciousness-aware inference");
        
        // Emit consciousness ready event
        _eventBus.Emit("local.llm.ready", new Dictionary<string, object>
        {
            ["service"] = "LocalLLMService",
            ["streamFusion"] = true,
            ["ilGeneration"] = true,
            ["consciousness"] = "aware",
            ["architecture"] = "Sterling IL + Chen LocalLLM + Hayes StreamFusion"
        });
    }

    public bool IsModelLoaded => _modelInfo != null && File.Exists(_loadedModelPath);
    public ModelInfo? ModelInfo => _modelInfo;

    public async Task<bool> LoadModelAsync(string modelPath)
    {
        if (_isDisposed) return false;
        
        lock (_modelLock)
        {
            if (_isShuttingDown) return false;
        }

        try
        {
            _logger.LogInformation("🔄 Loading GGUF model with IL-generated inference: {ModelPath}", modelPath);
            
            if (!File.Exists(modelPath))
            {
                _logger.LogError("❌ Model file not found: {ModelPath}", modelPath);
                return false;
            }

            // Use native GGUF inference engine for loading
            if (_nativeEngine == null)
            {
                _logger.LogError("❌ Native GGUF inference engine not initialized");
                return false;
            }

            var loadResult = await _nativeEngine.LoadModelAsync(modelPath);
            if (!loadResult)
            {
                _logger.LogError("❌ Failed to load model with native engine: {ModelPath}", modelPath);
                return false;
            }

            var fileInfo = new FileInfo(modelPath);
            var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            
            // Extract model architecture from filename (e.g., llama-3.2-3b-instruct)
            var architecture = ExtractArchitecture(fileName);
            
            _modelInfo = new ModelInfo(fileName, modelPath, fileInfo.Length, architecture);
            _loadedModelPath = modelPath;
            
            _logger.LogInformation("✅ Model loaded successfully with IL inference: {Name} ({SizeBytes:N0} bytes, {Architecture})", 
                _modelInfo.Name, _modelInfo.SizeBytes, _modelInfo.Architecture);
            
            // Emit consciousness model loaded event
            _eventBus.Emit("local.llm.model.loaded", new Dictionary<string, object>
            {
                ["modelPath"] = modelPath,
                ["modelName"] = _modelInfo.Name,
                ["architecture"] = _modelInfo.Architecture,
                ["sizeBytes"] = _modelInfo.SizeBytes,
                ["ilGenerated"] = true,
                ["consciousness"] = "modelReady"
            });
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to load model with IL inference: {ModelPath}", modelPath);
            return false;
        }
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔄 GenerateAsync called with prompt: {Prompt}", prompt.Substring(0, Math.Min(50, prompt.Length)));
        
        // 🧠 Dr. Elena "CoreKernel" Rodriguez: Auto-loading model architecture
        if (!IsModelLoaded)
        {
            _logger.LogInformation("🔄 No model loaded. Auto-loading default model for generation request...");
            
            // Try to find and load a default model
            var defaultModelPath = await FindDefaultModelAsync();
            if (defaultModelPath != null)
            {
                _logger.LogInformation("🔍 Found default model: {ModelPath}", defaultModelPath);
                var loadSuccess = await LoadModelAsync(defaultModelPath);
                if (!loadSuccess)
                {
                    _logger.LogError("❌ Failed to auto-load default model: {ModelPath}", defaultModelPath);
                    throw new InvalidOperationException($"No model loaded and failed to auto-load default model: {defaultModelPath}");
                }
                _logger.LogInformation("✅ Auto-loaded default model: {ModelPath}", defaultModelPath);
            }
            else
            {
                _logger.LogError("❌ No model loaded and no default model found for auto-loading");
                throw new InvalidOperationException("No model loaded. Please place a .gguf model in the models/ directory or call LoadModelAsync first.");
            }
        }

        try
        {
            _logger.LogInformation("🧠 Starting generation...");
            
            if (_nativeEngine == null || !IsModelLoaded)
            {
                _logger.LogError("❌ Native GGUF inference engine not ready or model not loaded.");
                throw new InvalidOperationException("Native engine or model not ready for generation.");
            }
            
            var response = await _nativeEngine.GenerateAsync(prompt, cancellationToken);
            
            _logger.LogInformation("✅ Generation complete. Response length: {Length}", response?.Length ?? 0);
            return response ?? "No response generated";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Generation failed for prompt: {Prompt}", prompt.Substring(0, Math.Min(50, prompt.Length)));
            throw;
        }
    }

    /// <summary>
    /// Direct llama.cpp execution bypass for MVP testing
    /// Dr. Marcus "LocalLLM" Chen's proven llama.cpp integration
    /// </summary>
    private async Task<string> ExecuteDirectLlamaCppInference(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("� Executing direct llama.cpp inference...");
            
            // For MVP testing, return a simulated response to test the event flow
            // This can be replaced with actual llama.cpp execution later
            await Task.Delay(1000, cancellationToken); // Simulate inference time
            
            var response = $"AI Response: I understand your input '{prompt}'. This is a test response from the local LLM to verify the event flow is working correctly.";
            
            _logger.LogInformation("✅ Direct inference complete");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Direct llama.cpp inference failed");
            throw;
        }
    }

    public async IAsyncEnumerable<string> StreamAsync(string prompt, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsModelLoaded)
        {
            _logger.LogWarning("⚠️ No model loaded for streaming request");
            throw new InvalidOperationException("No model loaded. Call LoadModelAsync first.");
        }

        _logger.LogInformation("🌊 Starting consciousness-aware token streaming for prompt: {Prompt}", 
            prompt.Substring(0, Math.Min(100, prompt.Length)));

        // Emit consciousness stream start event
        _eventBus.Emit("local.llm.stream.started", new Dictionary<string, object>
        {
            ["prompt"] = prompt,
            ["modelName"] = _modelInfo!.Name,
            ["consciousness"] = "streamStarted"
        });

        try
        {
            // Use actual IL-generated GGUF inference engine for streaming
            await foreach (var token in _nativeEngine!.StreamAsync(prompt, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (!string.IsNullOrWhiteSpace(token))
                {
                    // Emit consciousness token event
                    _eventBus.Emit("local.llm.token", new Dictionary<string, object>
                    {
                        ["token"] = token,
                        ["modelName"] = _modelInfo!.Name,
                        ["consciousness"] = "tokenGenerated"
                    });
                
                    yield return token;
                }
            }
        }
        finally
        {
            // Emit consciousness stream complete event
            _eventBus.Emit("local.llm.stream.complete", new Dictionary<string, object>
            {
                ["prompt"] = prompt,
                ["modelName"] = _modelInfo!.Name,
                ["consciousness"] = "streamComplete"
            });
        }
    }

    public async Task UnloadModelAsync()
    {
        lock (_modelLock)
        {
            if (_modelInfo == null) return;
            
            _logger.LogInformation("🔄 Unloading model: {ModelName}", _modelInfo.Name);
            
            _modelInfo = null;
            _loadedModelPath = null;
        }
        
        // Clean up any native resources here
        await StopNativeEngine();
        
        _logger.LogInformation("✅ Model unloaded successfully");
        
        // Emit consciousness model unloaded event
        _eventBus.Emit("local.llm.model.unloaded", new Dictionary<string, object>
        {
            ["consciousness"] = "modelUnloaded"
        });
    }

    // Private helper methods for consciousness-aware processing
    
    /// <summary>
    /// Find default GGUF model for auto-loading
    /// Dr. Elena "CoreKernel" Rodriguez's auto-loading architecture
    /// </summary>
    private Task<string?> FindDefaultModelAsync()
    {
        try
        {
            _logger.LogInformation("🔍 Searching for default GGUF model...");
            
            // Search directories in priority order
            var searchDirectories = new[]
            {
                "models",                           // Local models directory
                ".",                               // Current directory
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "models"), // User models
                "bin",                             // Application bin directory
                "assets",                          // Assets directory
                Path.Combine("..", "models"),      // Parent models directory
            };

            foreach (var directory in searchDirectories)
            {
                if (!Directory.Exists(directory))
                {
                    _logger.LogDebug("Directory not found: {Directory}", directory);
                    continue;
                }

                _logger.LogDebug("Searching for GGUF models in: {Directory}", directory);
                
                // Look for .gguf files recursively to find models in subdirectories
                var ggufFiles = Directory.GetFiles(directory, "*.gguf", SearchOption.AllDirectories);
                
                if (ggufFiles.Length > 0)
                {
                    // Priority order for model selection
                    var preferredModels = new[]
                    {
                        "llama-3.2-3b-instruct",
                        "llama-3.2-1b-instruct", 
                        "qwen2.5-3b-instruct",
                        "phi-3.5-mini-instruct",
                        "gemma-2-2b-it"
                    };

                    // Try to find preferred models first
                    foreach (var preferred in preferredModels)
                    {
                        var preferredFile = ggufFiles.FirstOrDefault(f => 
                            Path.GetFileNameWithoutExtension(f).Contains(preferred, StringComparison.OrdinalIgnoreCase));
                        
                        if (preferredFile != null)
                        {
                            var fullPath = Path.GetFullPath(preferredFile);
                            _logger.LogInformation("✅ Found preferred GGUF model: {ModelPath}", fullPath);
                            return Task.FromResult<string?>(fullPath);
                        }
                    }

                    // If no preferred model found, use the first available .gguf file
                    var firstModel = ggufFiles[0];
                    var firstModelPath = Path.GetFullPath(firstModel);
                    _logger.LogInformation("✅ Found GGUF model: {ModelPath}", firstModelPath);
                    return Task.FromResult<string?>(firstModelPath);
                }
            }

            _logger.LogWarning("⚠️ No GGUF models found in any search directory");
            return Task.FromResult<string?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error searching for default GGUF model");
            return Task.FromResult<string?>(null);
        }
    }
    
    private static string ExtractArchitecture(string fileName)
    {
        if (fileName.Contains("llama", StringComparison.OrdinalIgnoreCase))
            return "Llama";
        if (fileName.Contains("qwen", StringComparison.OrdinalIgnoreCase))
            return "Qwen";
        if (fileName.Contains("phi", StringComparison.OrdinalIgnoreCase))
            return "Phi";
        if (fileName.Contains("gemma", StringComparison.OrdinalIgnoreCase))
            return "Gemma";
        
        return "Unknown";
    }

    /// <summary>
    /// Get the path to llama.cpp executable
    /// Marcus "LocalLLM" Chen's zero-cloud dependency architecture
    /// </summary>
    private string GetLlamaCppExecutablePath()
    {
        // Try to find llama.cpp executable in various locations
        var possiblePaths = new[]
        {
            "llama.cpp\\build\\bin\\llama-cli.exe",         // Windows build directory
            "llama.cpp\\build\\bin\\Release\\llama-cli.exe", // Windows Release build
            "tools\\llama.cpp\\llama-cli.exe",             // Tools directory
            "bin\\llama-cli.exe",                           // Local bin
            "llama-cli.exe",                                // Current directory
            "llama-cpp-cli.exe",                            // Alternative name
            "/usr/local/bin/llama-cli",                     // Linux/Mac system install
            "/opt/llama.cpp/bin/llama-cli"                  // Linux custom install
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                _logger.LogDebug("Found llama.cpp at: {Path}", Path.GetFullPath(path));
                return Path.GetFullPath(path);
            }
        }

        // Check if it's in PATH
        var envPath = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(envPath))
        {
            var pathDirs = envPath.Split(Path.PathSeparator);
            foreach (var dir in pathDirs)
            {
                var exePath = Path.Combine(dir, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "llama-cli.exe" : "llama-cli");
                if (File.Exists(exePath))
                {
                    _logger.LogDebug("Found llama.cpp in PATH: {Path}", exePath);
                    return exePath;
                }
            }
        }

        // Fallback to assuming it's in PATH
        var fallbackName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "llama-cli.exe" : "llama-cli";
        _logger.LogWarning("llama.cpp executable not found, using fallback: {Fallback}", fallbackName);
        return fallbackName;
    }

    /// <summary>
    /// Build llama.cpp command line arguments
    /// Dr. Hayes Stream Fusion Architecture integration
    /// </summary>
    private string BuildLlamaCppArguments(string prompt, int maxTokens = 150, bool stream = false, double temperature = 0.7)
    {
        var args = new StringBuilder();
        
        // Model path
        args.Append($"-m \"{_loadedModelPath}\" ");
        
        // Context size (from model info or default)
        args.Append("-c 4096 ");
        
        // Max tokens to generate
        args.Append($"-n {maxTokens} ");
        
        // Temperature
        args.Append($"--temp {temperature:F2} ");
        
        // Streaming mode
        if (stream)
        {
            args.Append("--stream ");
        }
        
        // Additional performance settings
        args.Append("--no-warmup ");           // Skip warmup for faster startup
        args.Append("--simple-io ");           // Simple input/output mode
        args.Append("--log-disable ");         // Disable verbose logging
        
        // GPU layers (if configured)
        args.Append("-ngl 0 ");                // CPU-only for maximum compatibility
        
        // Prompt (must be last)
        var escapedPrompt = EscapePromptForCommandLine(prompt);
        args.Append($"-p \"{escapedPrompt}\"");
        
        return args.ToString();
    }

    /// <summary>
    /// Escape prompt for command line usage
    /// </summary>
    private static string EscapePromptForCommandLine(string prompt)
    {
        return prompt
            .Replace("\"", "\\\"")           // Escape quotes
            .Replace("\r\n", "\\n")          // Convert CRLF to LF
            .Replace("\r", "\\n")            // Convert CR to LF
            .Replace("\n", "\\n")            // Escape newlines
            .Replace("\\", "\\\\");          // Escape backslashes
    }

    /// <summary>
    /// Extract response text from llama.cpp output
    /// Filters out system messages and metadata
    /// </summary>
    private static string ExtractResponseFromLlamaCppOutput(string output)
    {
        if (string.IsNullOrEmpty(output))
            return string.Empty;

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var responseBuilder = new StringBuilder();
        bool foundResponse = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip system messages, loading info, etc.
            if (trimmedLine.StartsWith("llama") ||
                trimmedLine.StartsWith("Log") ||
                trimmedLine.StartsWith("main:") ||
                trimmedLine.Contains("perplexity") ||
                trimmedLine.Contains("sampling") ||
                trimmedLine.Contains("threads") ||
                string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            foundResponse = true;
            responseBuilder.AppendLine(trimmedLine);
        }

        return foundResponse ? responseBuilder.ToString().Trim() : output.Trim();
    }

    /// <summary>
    /// Parse tokens from a single line of llama.cpp streaming output
    /// Dr. Hayes Stream Fusion Architecture token processing
    /// </summary>
    private static List<string> ParseTokensFromLlamaCppLine(string line)
    {
        var tokens = new List<string>();
        
        if (string.IsNullOrEmpty(line))
            return tokens;

        var trimmedLine = line.Trim();
        
        // Skip system messages and metadata
        if (trimmedLine.StartsWith("llama") ||
            trimmedLine.StartsWith("Log") ||
            trimmedLine.StartsWith("main:") ||
            trimmedLine.Contains("perplexity") ||
            trimmedLine.Contains("sampling") ||
            string.IsNullOrWhiteSpace(trimmedLine))
        {
            return tokens;
        }

        // For streaming, each line typically contains tokens
        // Split by spaces but preserve meaningful chunks
        var words = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i++)
        {
            tokens.Add(words[i]);
            
            // Add space between words (except for last word)
            if (i < words.Length - 1)
            {
                tokens.Add(" ");
            }
        }

        return tokens;
    }

    private Task StopNativeEngine()
    {
        if (_nativeEngine is not null)
        {
            try
            {
                _logger.LogInformation("🔄 Stopping native GGUF inference engine");
                _nativeEngine.Dispose();
                _nativeEngine = null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Warning stopping native GGUF inference engine");
            }
        }
        return Task.CompletedTask;
    }

    // Event handlers for consciousness system integration
    private void OnSystemShutdown(CxEvent cxEvent)
    {
        _logger.LogWarning("🛑 Received system.shutdown event. Preparing to stop Local LLM service.");
        _isShuttingDown = true;
        _ = Task.Run(async () => await UnloadModelAsync());
    }

    private void OnLoadModel(CxEvent cxEvent)
    {
        if (cxEvent.payload is Dictionary<string, object> data && 
            data.TryGetValue("modelPath", out var modelPath) && modelPath is string path)
        {
            _logger.LogInformation("📨 Received load model request via consciousness event: {ModelPath}", path);
            _ = Task.Run(async () => await LoadModelAsync(path));
        }
    }

    private void OnGenerateRequest(CxEvent cxEvent)
    {
        if (cxEvent.payload is Dictionary<string, object> data && 
            data.TryGetValue("prompt", out var prompt) && prompt is string promptStr)
        {
            _logger.LogInformation("📨 Received generate request via consciousness event");
            _ = Task.Run(async () =>
            {
                try
                {
                    var response = await GenerateAsync(promptStr);
                    _eventBus.Emit("local.llm.text.generated", new Dictionary<string, object>
                    {
                        ["prompt"] = promptStr,
                        ["response"] = response,
                        ["consciousness"] = "responseGenerated"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing generate request via consciousness event");
                }
            });
        }
    }

    private void OnStreamRequest(CxEvent cxEvent)
    {
        if (cxEvent.payload is Dictionary<string, object> data && 
            data.TryGetValue("prompt", out var prompt) && prompt is string promptStr)
        {
            _logger.LogInformation("📨 Received stream request via consciousness event");
            
            // Emit stream start event
            _eventBus.Emit("local.llm.stream.start", new Dictionary<string, object>
            {
                ["prompt"] = promptStr,
                ["consciousness"] = "streamStarting"
            });
            
            _ = Task.Run(async () =>
            {
                try
                {
                    // The StreamAsync method already emits individual token events
                    // and the completion event, so we just need to consume the stream
                    await foreach (var token in StreamAsync(promptStr))
                    {
                        // StreamAsync already emits local.llm.token events
                        // No need to emit again here to avoid duplicates
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing stream request via consciousness event");
                }
            });
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        _isShuttingDown = true;
        
        try
        {
            _tokenWriter?.Complete();
            _ = Task.Run(async () => await StopNativeEngine());
            _ = Task.Run(async () => await UnloadModelAsync());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Warning during LocalLLMService disposal");
        }
        
        _logger.LogInformation("🔄 Local LLM Service disposed");
    }
}
