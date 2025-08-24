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
        private NativeGGUFInferenceEngine? _ggufEngine; // Removed readonly to allow reassignment
        
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
                // Create logger for GGUF engine with minimal logging to suppress verbose output
                var loggerFactory = LoggerFactory.Create(builder => 
                    builder.AddConsole()
                           .SetMinimumLevel(LogLevel.Warning) // Only show warnings and errors from GGUF
                           .AddFilter("CxLanguage.LocalLLM.NativeGGUFInferenceEngine", LogLevel.Warning));
                var ggufLogger = loggerFactory.CreateLogger<NativeGGUFInferenceEngine>();
                _ggufEngine = new NativeGGUFInferenceEngine(ggufLogger, modelPath);
                // Silent mode: Suppress LLM loader messages
                // _logger.LogInformation("🧠 Found GGUF model: {ModelPath}", modelPath);
                
                // DISABLE AUTOMATIC INITIALIZATION to prevent loader messages
                // Initialize only when actually needed by inference requests
                // _ = Task.Run(async () => await InitializeAsync());
            }
            else
            {
                // Silent mode: Suppress warning messages
                // _logger.LogWarning("🚫 REAL LLM ONLY MODE: No GGUF models found. Simulation removed.");
                // _logger.LogInformation("📥 Required: Download a real GGUF model for authentic inference");
            }
            
            // Silent mode: Suppress debug messages
            // _logger.LogDebug("🚀 GpuLocalLLMService initialized - GPU available: {GpuAvailable}", _gpuAvailable);
            if (_gpuAvailable)
            {
                var deviceCount = GetGpuDeviceCount();
                // Silent mode: Suppress CUDA details
                // _logger.LogInformation("📊 CUDA Details: {DeviceCount} devices available", deviceCount);
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
                _logger.LogDebug("🔍 BaseDirectory: {BaseDir}", baseDir);
                var workspaceRoot = FindWorkspaceRoot(baseDir);
                
                if (string.IsNullOrEmpty(workspaceRoot))
                {
                    _logger.LogWarning("⚠️ Could not find workspace root directory from base path");
                    
                    // Try some alternative approaches to find the workspace root
                    var currentDir = new DirectoryInfo(Environment.CurrentDirectory);
                    // Silent mode: Suppress directory logging
                    // _logger.LogInformation("🔍 CurrentDirectory: {CurrentDir}", currentDir.FullName);
                    
                    // Try finding workspace from current directory
                    workspaceRoot = FindWorkspaceRoot(currentDir.FullName);
                    
                    if (string.IsNullOrEmpty(workspaceRoot))
                    {
                        // Last resort - try hardcoded paths that should work in this environment
                        string[] potentialRoots = {
                            @"c:\Users\a7qBIOyPiniwRue6UVvF\cx",
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cx")
                        };
                        
                        foreach (var root in potentialRoots)
                        {
                            // Silent mode: Suppress path search logging
                            // _logger.LogInformation("🔍 Trying hardcoded path: {Path}", root);
                            if (Directory.Exists(root))
                            {
                                workspaceRoot = root;
                                // Silent mode: Suppress workspace root confirmation
                                // _logger.LogInformation("✅ Using hardcoded workspace root: {Root}", workspaceRoot);
                                break;
                            }
                        }
                        
                        if (string.IsNullOrEmpty(workspaceRoot))
                        {
                            _logger.LogWarning("⚠️ Could not find workspace root - falling back to current directory");
                            workspaceRoot = Environment.CurrentDirectory;
                        }
                    }
                }

                _logger.LogDebug("✅ Using workspace root: {WorkspaceRoot}", workspaceRoot);
                var modelsDir = Path.Combine(workspaceRoot, "models");
                
                // Check if models directory exists
                if (!Directory.Exists(modelsDir))
                {
                    _logger.LogWarning("⚠️ Models directory does not exist: {ModelsDir}", modelsDir);
                    
                    // Try to create the directory structure for models
                    try {
                        Directory.CreateDirectory(Path.Combine(modelsDir, "local_llm"));
                        _logger.LogInformation("✅ Created models directory structure");
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "❌ Failed to create models directory: {Error}", ex.Message);
                    }
                }
                
                // Priority order: Llama models (primary), then Phi-3 fallback
                var candidatePaths = new[]
                {
                    Path.Combine(workspaceRoot, "models", "local_llm", "Llama-3.2-3B-Instruct-Q4_K_M.gguf"),
                    Path.Combine(workspaceRoot, "models", "local_llm", "llama-3.2-3b-instruct-q4_k_m.gguf"),
                    Path.Combine(workspaceRoot, "models", "Llama-3.2-3B-Instruct-Q4_K_M.gguf"),
                    Path.Combine(workspaceRoot, "models", "llama-3.2-3b-instruct-q4_k_m.gguf"),
                    Path.Combine(workspaceRoot, "models", "local_llm", "Phi-3-mini-4k-instruct-q4.gguf"),
                    Path.Combine(workspaceRoot, "models", "Phi-3-mini-4k-instruct-q4.gguf")
                };

                _logger.LogDebug("🔍 Looking for GGUF models in paths:");
                foreach (var path in candidatePaths)
                {
                    _logger.LogDebug("  • {Path} - Exists: {Exists}", path, File.Exists(path));
                    
                    if (File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);
                        var modelName = Path.GetFileName(path).Contains("llama", StringComparison.OrdinalIgnoreCase) && 
                                       Path.GetFileName(path).Contains("3b", StringComparison.OrdinalIgnoreCase) ? "Llama 3.2 3B" :
                                       Path.GetFileName(path).Contains("llama", StringComparison.OrdinalIgnoreCase) && 
                                       Path.GetFileName(path).Contains("1b", StringComparison.OrdinalIgnoreCase) ? "Llama 3.2 1B" : 
                                       "Phi-3-mini-4k-instruct";
                        
                        _logger.LogDebug("🎯 Found GGUF model: {ModelName} at {Path} ({Size:F1} MB)", 
                            modelName, path, fileInfo.Length / (1024.0 * 1024.0));
                        return path;
                    }
                }

                // Also try explicitly looking for any .gguf file in the models directory and its subdirectories
                try
                {
                    if (Directory.Exists(workspaceRoot))
                    {
                        var allGgufFiles = Directory.GetFiles(workspaceRoot, "*.gguf", SearchOption.AllDirectories);
                        if (allGgufFiles.Length > 0)
                        {
                            _logger.LogInformation("🔍 Found {Count} .gguf files in workspace:", allGgufFiles.Length);
                            foreach (var file in allGgufFiles)
                            {
                                _logger.LogInformation("  • {Path} - Size: {Size:F1} MB", 
                                    file, new FileInfo(file).Length / (1024.0 * 1024.0));
                            }
                            
                            // Use the first found file
                            _logger.LogInformation("🎯 Using first found GGUF model: {Path}", allGgufFiles[0]);
                            return allGgufFiles[0];
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error during recursive file search: {Error}", ex.Message);
                }

                _logger.LogInformation("🧩 No GGUF models found in workspace. REAL LLM ONLY MODE:");
                _logger.LogInformation("   🚀 Required: Download Phi-3-mini model for authentic consciousness processing");
                _logger.LogInformation("   📥 PowerShell: Invoke-WebRequest -Uri 'https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf/resolve/main/Phi-3-mini-4k-instruct-q4.gguf' -OutFile 'models/local_llm/Phi-3-mini-4k-instruct-q4.gguf'");
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
            try
            {
                if (string.IsNullOrEmpty(startPath))
                {
                    _logger.LogWarning("⚠️ Start path is null or empty");
                    startPath = AppDomain.CurrentDomain.BaseDirectory;
                }
                
                _logger.LogInformation("🔍 Searching for workspace root from: {StartPath}", startPath);
                
                // Check if startPath exists
                if (!Directory.Exists(startPath))
                {
                    _logger.LogWarning("⚠️ Start path doesn't exist: {StartPath}", startPath);
                    startPath = Environment.CurrentDirectory;
                    _logger.LogInformation("🔄 Falling back to current directory: {CurrentDir}", startPath);
                }
                
                var current = new DirectoryInfo(startPath);
                int maxDepth = 10; // Prevent infinite loops by limiting search depth
                int currentDepth = 0;
                
                while (current != null && currentDepth < maxDepth)
                {
                    // Look for workspace indicators
                    var solutionFile = Path.Combine(current.FullName, "CxLanguage.sln");
                    var readmeFile = Path.Combine(current.FullName, "README.md");
                    var modelsDir = Path.Combine(current.FullName, "models");
                    var srcDir = Path.Combine(current.FullName, "src");
                    var examplesDir = Path.Combine(current.FullName, "examples");
                    
                    _logger.LogDebug("👁️ Checking directory: {Path}", current.FullName);
                    
                    // Check various markers that indicate workspace root
                    if (File.Exists(solutionFile))
                    {
                        _logger.LogInformation("✅ Found workspace root by solution file at: {Path}", current.FullName);
                        return current.FullName;
                    }
                    
                    if (File.Exists(readmeFile) && (Directory.Exists(srcDir) || Directory.Exists(modelsDir)))
                    {
                        _logger.LogInformation("✅ Found workspace root by README + src/models at: {Path}", current.FullName);
                        return current.FullName;
                    }
                    
                    if (Directory.Exists(modelsDir) && Directory.Exists(srcDir))
                    {
                        _logger.LogInformation("✅ Found workspace root by models + src directories at: {Path}", current.FullName);
                        return current.FullName;
                    }
                    
                    if (Directory.Exists(examplesDir) && Directory.Exists(srcDir))
                    {
                        _logger.LogInformation("✅ Found workspace root by examples + src directories at: {Path}", current.FullName);
                        return current.FullName;
                    }
                    
                    // Check if we've found a models directory with .gguf files
                    if (Directory.Exists(modelsDir))
                    {
                        var hasGgufFiles = Directory.GetFiles(modelsDir, "*.gguf", SearchOption.AllDirectories).Length > 0;
                        if (hasGgufFiles)
                        {
                            _logger.LogInformation("✅ Found workspace root by models directory with GGUF files at: {Path}", current.FullName);
                            return current.FullName;
                        }
                    }
                    
                    current = current.Parent;
                    currentDepth++;
                }
                
                // If we couldn't find the workspace root through normal means, try a hardcoded path
                string[] potentialRoots = {
                    @"c:\Users\a7qBIOyPiniwRue6UVvF\cx",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cx"),
                    // Specific path from logs
                    @"C:\Users\a7qBIOyPiniwRue6UVvF\cx"
                };
                
                foreach (var root in potentialRoots)
                {
                    _logger.LogInformation("🔍 Trying hardcoded path: {Path}", root);
                    if (Directory.Exists(root))
                    {
                        bool isValid = File.Exists(Path.Combine(root, "CxLanguage.sln")) || 
                                      Directory.Exists(Path.Combine(root, "models")) ||
                                      Directory.Exists(Path.Combine(root, "src"));
                                      
                        if (isValid)
                        {
                            _logger.LogInformation("✅ Found workspace root from hardcoded path: {Path}", root);
                            return root;
                        }
                    }
                }
                
                // Last resort - use the Environment.CurrentDirectory
                var currentDir = Environment.CurrentDirectory;
                _logger.LogWarning("⚠️ Could not find workspace root through standard methods, using current directory: {CurrentDir}", currentDir);
                return currentDir;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error finding workspace root: {Error}", ex.Message);
                
                // Last resort fallback to hardcoded path
                string fallbackPath = @"C:\Users\a7qBIOyPiniwRue6UVvF\cx";
                _logger.LogInformation("🔄 After error, falling back to hardcoded path: {Path}", fallbackPath);
                if (Directory.Exists(fallbackPath))
                {
                    return fallbackPath;
                }
                
                return Environment.CurrentDirectory;
            }
        }

        /// <summary>
        /// Initializes the consciousness-aware LLM service with real model support
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                // Silent mode: Suppress initialization messages
                // _logger.LogInformation("🚀 Initializing GPU-accelerated LLM service...");
                
                // Try to initialize real GGUF model first
                if (_ggufEngine != null)
                {
                    // Silent mode: Suppress GGUF loading attempt messages
                    // _logger.LogInformation("🧠 Attempting to load real GGUF model...");
                    
                    try
                    {
                        // Get the model path for debugging
                        string modelPath = "unknown";
                        var modelPathMethod = _ggufEngine.GetType().GetMethod("GetModelInfo");
                        if (modelPathMethod != null)
                        {
                            modelPath = modelPathMethod.Invoke(_ggufEngine, null) as string ?? "unknown";
                        }
                        
                        // Silent mode: Suppress model path and type logging
                        // _logger.LogInformation("🔧 Using model path: {ModelPath}", modelPath);
                        // _logger.LogInformation("🔧 GGUFEngine type: {Type}", _ggufEngine.GetType().FullName);
                        
                        // Check if the model file exists
                        if (modelPath != "unknown" && !modelPath.Contains("No GGUF model loaded") && File.Exists(modelPath))
                        {
                            // Silent mode: Suppress file existence and size confirmation
                            // _logger.LogInformation("✅ Confirmed model file exists at: {ModelPath}", modelPath);
                            var fileInfo = new FileInfo(modelPath);
                            // _logger.LogInformation("📊 Model file size: {Size:F2} MB", fileInfo.Length / (1024.0 * 1024.0));
                        }
                        else
                        {
                            // Silent mode: Suppress model file warnings
                            // _logger.LogWarning("⚠️ Model file may not exist or path is unknown");
                        }
                    }
                    catch (Exception)
                    {
                        // Silent mode: Suppress model path check warnings
                        // _logger.LogWarning(ex, "⚠️ Error checking model path: {Error}", ex.Message);
                    }
                    
                    // Initialize the model
                    try
                    {
                        _realModelMode = await _ggufEngine.InitializeAsync();
                        // Silent mode: Suppress initialization result logging
                        // _logger.LogInformation("📊 GGUF initialization result: {Result}", _realModelMode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error during GGUF initialization: {Error}", ex.Message);
                        _realModelMode = false;
                    }
                    
                    if (_realModelMode)
                    {
                        // Silent mode: Suppress real LLM mode activation message
                        // _logger.LogInformation("✅ Real LLM Mode activated - using GGUF model inference");
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
                        
                        // Try to determine why initialization failed
                        var modelPath = _ggufEngine.GetModelInfo();
                        _logger.LogInformation("🔍 Model path during failed init: {ModelPath}", modelPath);
                        
                        if (modelPath != null && modelPath.Contains("No GGUF model loaded"))
                        {
                            _logger.LogError("❌ GGUF model was not loaded - possible model file issue");
                        }
                        
                        // Log additional details about the model path
                        var ggufPath = modelPath;
                        if (!string.IsNullOrEmpty(ggufPath) && !ggufPath.Contains("No GGUF model loaded"))
                        {
                            _logger.LogInformation("🔍 Checking model file details for: {Path}", ggufPath);
                            
                            if (File.Exists(ggufPath))
                            {
                                var fileInfo = new FileInfo(ggufPath);
                                _logger.LogInformation("✅ Model file exists: {Path}, Size: {Size:F2} MB", 
                                    ggufPath, fileInfo.Length / (1024.0 * 1024.0));
                                    
                                // File exists but initialization still failed - could be format issue
                                _logger.LogError("❌ File exists but model initialization failed - possible GGUF format issue or LlamaSharp compatibility problem");
                            }
                            else
                            {
                                _logger.LogError("❌ Model file not found at path: {Path}", ggufPath);
                            }
                        }
                        
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("❌ REAL LLM ONLY MODE: No GGUF engine available - NO SIMULATION FALLBACK");
                    
                    // Try to explain why the GGUF engine is not available
                    var modelPath = FindBestAvailableModel();
                    if (string.IsNullOrEmpty(modelPath))
                    {
                        _logger.LogError("❌ No model file found - please download a GGUF model");
                    }
                    else
                    {
                        _logger.LogError("❌ Model file found at {Path} but GGUF engine not created properly", modelPath);
                    }
                    
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
                // Silent mode: Suppress model loading messages
                // _logger.LogInformation("📥 Loading model: {ModelName}", modelName);
                
                // Check if the input is already a full path
                bool isFullPath = Path.IsPathRooted(modelName) && File.Exists(modelName);
                string requestedModelPath;
                
                if (isFullPath)
                {
                    // Use the direct path
                    requestedModelPath = modelName;
                    // Silent mode: Suppress path confirmation
                    // _logger.LogInformation("✅ Using direct model file path: {Path}", requestedModelPath);
                }
                else
                {
                    // Try to find the model in expected locations
                    var workspaceRoot = FindWorkspaceRoot(AppDomain.CurrentDomain.BaseDirectory) ?? Environment.CurrentDirectory;
                    // Silent mode: Suppress workspace search logging
                    // _logger.LogInformation("🔍 Looking for model in workspace: {Root}", workspaceRoot);
                    
                    // First check models/local_llm directory
                    requestedModelPath = Path.Combine(workspaceRoot, "models", "local_llm", modelName);
                    if (!File.Exists(requestedModelPath))
                    {
                        // Then check models directory
                        requestedModelPath = Path.Combine(workspaceRoot, "models", modelName);
                        if (!File.Exists(requestedModelPath))
                        {
                            // Use FindBestAvailableModel as a fallback
                            // Silent mode: Suppress model not found warning
                            // _logger.LogWarning("⚠️ Requested model {ModelName} not found in expected locations", modelName);
                            var fallbackModel = FindBestAvailableModel();
                            if (string.IsNullOrEmpty(fallbackModel))
                            {
                                // Silent mode: Keep only critical errors
                                _logger.LogError("❌ REAL LLM ONLY MODE: No GGUF models available - NO SIMULATION FALLBACK");
                                return false;
                            }
                            requestedModelPath = fallbackModel;
                        }
                    }
                }
                
                // Silent mode: Suppress model loading confirmation
                // _logger.LogInformation("🔧 Loading model from path: {Path}", requestedModelPath);
                
                // If we've previously initialized a model, dispose it
                if (_ggufEngine != null)
                {
                    try {
                        _ggufEngine.Dispose();
                    }
                    catch (Exception ex) {
                        _logger.LogWarning(ex, "⚠️ Warning during GGUF engine disposal: {Error}", ex.Message);
                    }
                }
                
                // Create a new GGUF engine with the requested model
                var loggerFactory = LoggerFactory.Create(builder => 
                    builder.AddConsole()
                           .SetMinimumLevel(LogLevel.Warning) // Only show warnings and errors from GGUF
                           .AddFilter("CxLanguage.LocalLLM.NativeGGUFInferenceEngine", LogLevel.Warning));
                var ggufLogger = loggerFactory.CreateLogger<NativeGGUFInferenceEngine>();
                // Create a new GGUF engine with the requested model
                _ggufEngine = new NativeGGUFInferenceEngine(ggufLogger, requestedModelPath);
                
                // Initialize the model
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
                    // Silent mode: Suppress file size logging
                    // _logger.LogInformation("📊 Model file size: {Size:F2} MB", fileSize / (1024.0 * 1024.0));
                }
                
                _modelInfo = new ModelInfo(
                    Path.GetFileName(requestedModelPath),
                    "1.0",
                    fileSize,
                    _gpuAvailable ? "CUDA" : "CPU",
                    DateTime.UtcNow,
                    requestedModelPath
                );
                
                // Silent mode: Suppress successful loading confirmation
                // _logger.LogInformation("✅ Model loaded successfully: {ModelName}", Path.GetFileName(requestedModelPath));
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