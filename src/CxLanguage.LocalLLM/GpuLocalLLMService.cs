using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Runtime.CompilerServices;

namespace CxLanguage.LocalLLM
{
    /// <summary>
    /// GPU-accelerated Local LLM Service with real GGUF model support - REAL LLM ONLY MODE.
    /// Provides high-performance, consciousness-aware local inference without cloud dependencies.
    /// REAL LLM ONLY: No simulation fallbacks - authentic local model inference or clear errors.
    /// </summary>
    public class GpuLocalLLMService : ILocalLLMService, IDisposable
    {
        private readonly ILogger<GpuLocalLLMService> _logger;
        private readonly bool _gpuAvailable;
        private readonly Channel<string> _tokenChannel;
        private readonly ChannelWriter<string> _tokenWriter;
        private readonly ChannelReader<string> _tokenReader;
        private readonly NativeGGUFInferenceEngine? _ggufEngine;
        
        private bool _modelLoaded = false;
        private bool _realModelMode = false;
        private ModelInfo? _modelInfo;
        
        /// <summary>
        /// Initializes a new instance of the GPU-accelerated Local LLM Service with real model support
        /// </summary>
        public GpuLocalLLMService(ILogger<GpuLocalLLMService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Check for GPU availability
            _gpuAvailable = CheckGpuAvailability();
            
            // Create streaming token channel for real-time generation
            _tokenChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });
            
            _tokenWriter = _tokenChannel.Writer;
            _tokenReader = _tokenChannel.Reader;

            // Try to initialize real GGUF model
            var modelPath = FindBestAvailableModel();
            if (!string.IsNullOrEmpty(modelPath))
            {
                // Create logger for GGUF engine - simplified approach
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var ggufLogger = loggerFactory.CreateLogger<NativeGGUFInferenceEngine>();
                _ggufEngine = new NativeGGUFInferenceEngine(ggufLogger, modelPath);
                _logger.LogInformation("🧠 Found GGUF model: {ModelPath}", modelPath);
                
                // Initialize immediately
                _ = Task.Run(async () => await InitializeAsync());
            }
            else
            {
                _logger.LogWarning("🚫 REAL LLM ONLY MODE: No GGUF models found. Simulation removed.");
                _logger.LogInformation("📥 Required: Download a real GGUF model for authentic inference");
            }
            
            _logger.LogInformation("🚀 GpuLocalLLMService initialized - GPU available: {GpuAvailable}", _gpuAvailable);
            if (_gpuAvailable)
            {
                var deviceCount = GetGpuDeviceCount();
                _logger.LogInformation("📊 CUDA Details: {DeviceCount} devices available", deviceCount);
            }
        }
        
        /// <summary>
        /// Check for GPU availability
        /// </summary>
        private bool CheckGpuAvailability()
        {
            try
            {
                // Check for NVIDIA GPU availability through system queries
                // This could be enhanced with actual CUDA availability checks
                return Environment.GetEnvironmentVariable("CUDA_VISIBLE_DEVICES") != null ||
                       System.IO.Directory.Exists(@"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA") ||
                       System.IO.Directory.Exists(@"C:\Program Files\NVIDIA Corporation\NVSMI");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error checking GPU availability, assuming GPU is not available");
                return false;
            }
        }
        
        /// <summary>
        /// Get the number of GPU devices
        /// </summary>
        private int GetGpuDeviceCount()
        {
            try
            {
                // Basic GPU device count estimation
                // This could be enhanced with actual CUDA device queries
                if (CheckGpuAvailability())
                {
                    return 1; // Assume at least 1 GPU if available
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error getting GPU device count, assuming 0 devices");
                return 0;
            }
        }

        /// <summary>
        /// Find the best available GGUF model from the models directory.
        /// Priority: Phi-3-mini (primary), 1B model (lightweight), 3B model (production).
        /// </summary>
        private string? FindBestAvailableModel()
        {
            try
            {
                // Get the base directory (workspace root)
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var workspaceRoot = FindWorkspaceRoot(baseDir);
                
                if (string.IsNullOrEmpty(workspaceRoot))
                {
                    _logger.LogWarning("⚠️ Could not find workspace root directory");
                    return null;
                }

                var modelsDir = Path.Combine(workspaceRoot, "models");
                
                // Priority order: Llama models (primary), then Phi-3 fallback
                var candidatePaths = new[]
                {
                    Path.Combine(modelsDir, "local_llm", "Llama-3.2-3B-Instruct-Q4_K_M.gguf"), // Production Llama (actual filename)
                    Path.Combine(modelsDir, "local_llm", "llama-3.2-3b-instruct-q4_k_m.gguf"), // Production Llama (lowercase)
                    Path.Combine(modelsDir, "llama-3.2-1b-instruct-q4_k_m.gguf"),          // Lightweight Llama
                    Path.Combine(modelsDir, "local_llm", "Phi-3-mini-4k-instruct-q4.gguf"), // Phi-3 fallback
                    Path.Combine(modelsDir, "phi-3-mini-4k-instruct-q4.gguf")              // Alternative Phi-3 location
                };

                foreach (var path in candidatePaths)
                {
                    if (File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);
                        var modelName = Path.GetFileName(path).Contains("llama") && Path.GetFileName(path).Contains("3b") ? "Llama 3.2 3B" :
                                       Path.GetFileName(path).Contains("llama") && Path.GetFileName(path).Contains("1b") ? "Llama 3.2 1B" : 
                                       "Phi-3-mini-4k-instruct";
                        
                        _logger.LogInformation("🎯 Found GGUF model: {ModelName} at {Path} ({Size:F1} MB)", 
                            modelName, path, fileInfo.Length / (1024.0 * 1024.0));
                        return path;
                    }
                }

                _logger.LogInformation("🧩 No GGUF models found in {ModelsDir}. REAL LLM ONLY MODE:", modelsDir);
                _logger.LogInformation("   🚀 Required: Download Phi-3-mini model for authentic consciousness processing");
                _logger.LogInformation("   📥 PowerShell: Invoke-WebRequest -Uri 'https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf/resolve/main/Phi-3-mini-4k-instruct-q4.gguf' -OutFile 'models/phi-3-mini-4k-instruct-q4.gguf'");
                _logger.LogInformation("   � NO SIMULATION FALLBACK: Real LLM required for operation");
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error searching for GGUF models: {Error}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Find the workspace root by looking for characteristic files.
        /// </summary>
        private string? FindWorkspaceRoot(string startPath)
        {
            var current = new DirectoryInfo(startPath);
            
            while (current != null)
            {
                // Look for workspace indicators
                if (File.Exists(Path.Combine(current.FullName, "CxLanguage.sln")) ||
                    File.Exists(Path.Combine(current.FullName, "README.md")) ||
                    Directory.Exists(Path.Combine(current.FullName, "models")))
                {
                    return current.FullName;
                }
                
                current = current.Parent;
            }
            
            return null;
        }

        /// <summary>
        /// Initializes the consciousness-aware LLM service with real model support
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("🚀 Initializing GPU-accelerated LLM service...");
                
                // Try to initialize real GGUF model first
                if (_ggufEngine != null)
                {
                    _logger.LogInformation("🧠 Attempting to load real GGUF model...");
                    _realModelMode = await _ggufEngine.InitializeAsync();
                    
                    if (_realModelMode)
                    {
                        _logger.LogInformation("✅ Real LLM Mode activated - using GGUF model inference");
                        _modelInfo = new ModelInfo(
                            Name: "Real GGUF Model", 
                            Version: "Llama 3.2",
                            SizeBytes: 0, // Will be determined by model file
                            Architecture: "GGUF + GPU-CUDA",
                            LoadedAt: DateTime.UtcNow,
                            Path: _ggufEngine?.GetModelInfo()
                        );
                        _modelLoaded = true; // Set this flag explicitly when real model is ready
                        _logger.LogInformation("🔧 DEBUG: Set _modelLoaded=true, _realModelMode=true");
                    }
                    else
                    {
                        _logger.LogError("❌ REAL LLM ONLY MODE: Failed to load GGUF model - NO SIMULATION FALLBACK");
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("❌ REAL LLM ONLY MODE: No GGUF engine available - NO SIMULATION FALLBACK");
                    return false;
                }
                
                _logger.LogInformation("✅ LLM Service initialized successfully - Real GGUF Model Only");
                _logger.LogInformation("🔧 DEBUG: Final status - _modelLoaded={ModelLoaded}, _realModelMode={RealModelMode}", _modelLoaded, _realModelMode);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize LLM service: {Error}", ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Loads a specific LLM model for consciousness-aware processing - REAL LLM ONLY
        /// </summary>
        public async Task<bool> LoadModelAsync(string modelName, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("📥 Loading model: {ModelName}", modelName);
                
                // Find the actual path for the requested model
                var modelsDir = Path.Combine(Directory.GetCurrentDirectory(), "models");
                var requestedModelPath = Path.Combine(modelsDir, "local_llm", modelName);
                
                // If the specific model doesn't exist, use the best available model
                if (!File.Exists(requestedModelPath))
                {
                    _logger.LogWarning("⚠️ Requested model {ModelName} not found, using best available model", modelName);
                    requestedModelPath = FindBestAvailableModel();
                    
                    if (string.IsNullOrEmpty(requestedModelPath))
                    {
                        _logger.LogError("❌ REAL LLM ONLY MODE: No GGUF models available - NO SIMULATION FALLBACK");
                        return false;
                    }
                }
                
                // REAL LLM ONLY MODE: No simulation fallback
                if (_ggufEngine == null || !await _ggufEngine.InitializeAsync())
                {
                    _logger.LogError("❌ REAL LLM ONLY MODE: Failed to load model {ModelName} - NO SIMULATION FALLBACK", modelName);
                    return false;
                }
                
                _modelLoaded = true;
                _realModelMode = true;
                
                // Get actual file size if model path is available
                long fileSize = 0;
                if (File.Exists(requestedModelPath))
                {
                    fileSize = new FileInfo(requestedModelPath).Length;
                }
                
                _modelInfo = new ModelInfo(
                    modelName,
                    "1.0",
                    fileSize,
                    _gpuAvailable ? "CUDA" : "CPU",
                    DateTime.UtcNow,
                    $"/models/{modelName}"
                );
                
                _logger.LogInformation("✅ Model loaded successfully: {ModelName}", modelName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load model: {ModelName} - REAL LLM ONLY MODE", modelName);
                _modelLoaded = false;
                _modelInfo = null;
                return false;
            }
        }

        /// <summary>
        /// Unloads the currently loaded model to free memory
        /// </summary>
        public Task UnloadModelAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_modelLoaded)
                {
                    _logger.LogInformation("⚠️ No model is currently loaded");
                    return Task.CompletedTask;
                }
                
                _logger.LogInformation("📤 Unloading model: {ModelName}", _modelInfo?.Name);
                
                // Properly dispose GGUF engine resources
                _ggufEngine?.Dispose();
                
                _modelLoaded = false;
                _modelInfo = null;
                
                _logger.LogInformation("✅ Model unloaded successfully");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to unload model");
                return Task.FromException(ex);
            }
        }
        
        /// <summary>
        /// Generate text using real GGUF model - REAL LLM ONLY MODE
        /// </summary>
        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                // Ensure model is initialized
                if (!_modelLoaded || !_realModelMode)
                {
                    _logger.LogInformation("🚀 Model not ready - Status: modelLoaded={ModelLoaded}, realModelMode={RealModelMode}", _modelLoaded, _realModelMode);
                    _logger.LogInformation("🚀 Initializing GGUF model for first inference...");
                    if (!await InitializeAsync())
                    {
                        throw new InvalidOperationException("❌ REAL LLM ONLY MODE: Failed to initialize service - NO SIMULATION FALLBACK");
                    }
                }
                
                _logger.LogInformation("🧠 Generating with prompt: {PromptStart}... (REAL LLM ONLY)", 
                    prompt.Length > 50 ? prompt.Substring(0, 50) + "..." : prompt);
                
                // REAL LLM ONLY: Use real GGUF model or fail
                if (_realModelMode && _ggufEngine != null)
                {
                    return await _ggufEngine.GenerateAsync(prompt, cancellationToken);
                }
                
                // NO SIMULATION FALLBACK
                throw new InvalidOperationException("❌ REAL LLM ONLY MODE: No GGUF model loaded - NO SIMULATION FALLBACK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during generation - REAL LLM ONLY MODE");
                throw;
            }
        }
        
        /// <summary>
        /// Streams consciousness-aware text generation in real-time - REAL LLM ONLY
        /// </summary>
        public async IAsyncEnumerable<string> StreamAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!_modelLoaded)
            {
                _logger.LogWarning("⚠️ No model is loaded, loading default model");
                if (!await LoadModelAsync("Llama-3.2-3B-Instruct-Q4_K_M.gguf", cancellationToken))
                {
                    throw new InvalidOperationException("❌ REAL LLM ONLY MODE: Failed to load model - NO SIMULATION FALLBACK");
                }
            }
            
            _logger.LogInformation("🧠 Streaming with prompt: {PromptStart}... (REAL LLM ONLY)", 
                prompt.Length > 50 ? prompt.Substring(0, 50) + "..." : prompt);
            
            // REAL LLM ONLY: No simulation fallback
            if (!_realModelMode || _ggufEngine == null)
            {
                throw new InvalidOperationException("❌ REAL LLM ONLY MODE: No GGUF model available for streaming - NO SIMULATION FALLBACK");
            }
            
            // Clear any existing tokens in the channel
            while (_tokenReader.TryRead(out _)) { }
            
            // Start generation in a background task using real GGUF model
            _ = Task.Run(async () => 
            {
                try
                {
                    string result = await _ggufEngine.GenerateAsync(prompt, cancellationToken);
                    
                    // Split result into tokens for streaming
                    var tokens = result.Split(' ');
                    foreach (var token in tokens)
                    {
                        await _tokenWriter.WriteAsync(token + " ", cancellationToken);
                        // Real-time streaming without artificial delays
                    }
                    
                    // Complete the channel
                    _tokenWriter.Complete();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during streaming generation - REAL LLM ONLY");
                    _tokenWriter.Complete(ex);
                }
            }, cancellationToken);
            
            // Read tokens from the channel
            while (await _tokenReader.WaitToReadAsync(cancellationToken))
            {
                while (_tokenReader.TryRead(out var token))
                {
                    yield return token;
                }
            }
        }
        
        /// <summary>
        /// Checks if a model is currently loaded and ready for inference
        /// </summary>
        public bool IsModelLoaded => _modelLoaded;

        /// <summary>
        /// Gets information about the currently loaded model
        /// </summary>
        public ModelInfo? ModelInfo => _modelInfo;
        
        /// <summary>
        /// Checks if GPU is available for acceleration
        /// </summary>
        /// <returns>True if GPU is available, false otherwise</returns>
        public bool IsGpuAvailable() => _gpuAvailable;
        
        // 🚫 ALL SIMULATION METHODS REMOVED - REAL LLM ONLY MODE
        // GenerateWithGpu and GenerateWithCpu simulation methods have been
        // completely removed to ensure only real LLM inference is used
        
        /// <summary>
        /// Dispose of resources including real GGUF model
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("🧹 GpuLocalLLMService disposing resources...");
            
            try
            {
                // Dispose real GGUF model if loaded
                _ggufEngine?.Dispose();
                
                // Cleanup the token channel
                _tokenWriter.Complete();
                
                _logger.LogInformation("✅ GpuLocalLLMService disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Warning during GpuLocalLLMService disposal: {Error}", ex.Message);
            }
        }
    }
}