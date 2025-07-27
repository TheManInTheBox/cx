# GPU Acceleration Implementation Roadmap

## 🚀 **PHASE 1: ENABLE GPU PACKAGES (Week 1)**

### Step 1: Uncomment GPU Dependencies
**File**: `src/CxLanguage.LocalLLM/CxLanguage.LocalLLM.csproj`

```xml
<!-- BEFORE: Currently commented out -->
<!-- <PackageReference Include="TorchSharp" Version="0.102.5" /> -->
<!-- <PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.19.2" /> -->

<!-- AFTER: Enable GPU acceleration packages -->
<PackageReference Include="TorchSharp" Version="0.102.5" />
<PackageReference Include="TorchSharp.Cuda" Version="0.102.5" Condition="'$(EnableCuda)' == 'true'" />
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.19.2" />
<PackageReference Include="Microsoft.ML.OnnxRuntime.Gpu" Version="1.19.2" Condition="'$(EnableCuda)' == 'true'" />
```

### Step 2: Create GPU Detection Service
**File**: `src/CxLanguage.StandardLibrary/Services/GpuCapabilityService.cs`

```csharp
using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Services
{
    public interface IGpuCapabilityService
    {
        bool HasNvidiaGpu { get; }
        bool HasSufficientVram { get; }
        long VramSizeBytes { get; }
        bool CudaAvailable { get; }
        GpuCapabilities GetCapabilities();
    }

    public class GpuCapabilityService : IGpuCapabilityService
    {
        private readonly ILogger<GpuCapabilityService> _logger;
        private readonly Lazy<GpuCapabilities> _capabilities;

        public GpuCapabilityService(ILogger<GpuCapabilityService> logger)
        {
            _logger = logger;
            _capabilities = new Lazy<GpuCapabilities>(DetectCapabilities);
        }

        public bool HasNvidiaGpu => _capabilities.Value.HasNvidiaGpu;
        public bool HasSufficientVram => _capabilities.Value.VramSizeBytes >= 8_000_000_000; // 8GB
        public long VramSizeBytes => _capabilities.Value.VramSizeBytes;
        public bool CudaAvailable => _capabilities.Value.CudaAvailable;

        public GpuCapabilities GetCapabilities() => _capabilities.Value;

        private GpuCapabilities DetectCapabilities()
        {
            var capabilities = new GpuCapabilities();

            try
            {
#if ENABLE_CUDA
                // TorchSharp CUDA detection
                capabilities.CudaAvailable = torch.cuda.is_available();
                capabilities.HasNvidiaGpu = capabilities.CudaAvailable;
                
                if (capabilities.CudaAvailable)
                {
                    capabilities.VramSizeBytes = torch.cuda.get_device_properties(0).total_memory;
                    capabilities.DeviceName = torch.cuda.get_device_name(0);
                    capabilities.ComputeCapability = torch.cuda.get_device_capability(0);
                }
#else
                _logger.LogInformation("CUDA support not compiled in");
#endif
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GPU detection failed: {Error}", ex.Message);
                capabilities.ErrorMessage = ex.Message;
            }

            _logger.LogInformation("GPU Capabilities: CUDA={CudaAvailable}, VRAM={VramGB}GB, Device={DeviceName}",
                capabilities.CudaAvailable, 
                capabilities.VramSizeBytes / (1024 * 1024 * 1024),
                capabilities.DeviceName ?? "None");

            return capabilities;
        }
    }

    public record GpuCapabilities
    {
        public bool HasNvidiaGpu { get; init; }
        public bool CudaAvailable { get; init; }
        public long VramSizeBytes { get; init; }
        public string? DeviceName { get; init; }
        public (int Major, int Minor) ComputeCapability { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
```

### Step 3: Update LocalLLMService for Hybrid Inference
**File**: `src/CxLanguage.StandardLibrary/Services/LocalLLMService.cs`

```csharp
// Add GPU capability injection
public class LocalLLMService : ILocalLLMService, IDisposable
{
    private readonly IGpuCapabilityService _gpuCapabilities;
    private readonly IConfiguration _configuration;
    private ICudaInferenceEngine? _gpuEngine;

    public LocalLLMService(
        ILogger<LocalLLMService> logger, 
        ICxEventBus eventBus,
        IGpuCapabilityService gpuCapabilities,
        IConfiguration configuration,
        ILoggerFactory? loggerFactory = null)
    {
        _logger = logger;
        _eventBus = eventBus;
        _gpuCapabilities = gpuCapabilities;
        _configuration = configuration;
        
        // Initialize CPU engine (always available)
        InitializeCpuEngine(loggerFactory);
        
        // Initialize GPU engine if available and enabled
        InitializeGpuEngine();
    }

    private void InitializeGpuEngine()
    {
        var enableGpu = _configuration.GetValue<bool>("LocalLLM:PreferGpu", false);
        
        if (enableGpu && _gpuCapabilities.CudaAvailable && _gpuCapabilities.HasSufficientVram)
        {
            try
            {
#if ENABLE_CUDA
                _gpuEngine = new CudaInferenceEngine(_logger, _eventBus);
                _logger.LogInformation("✅ GPU acceleration enabled with {DeviceName}", 
                    _gpuCapabilities.GetCapabilities().DeviceName);
#endif
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GPU acceleration initialization failed: {Error}", ex.Message);
            }
        }
        else
        {
            _logger.LogInformation("GPU acceleration disabled or unavailable");
        }
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // Intelligent routing based on model size and GPU availability
        if (ShouldUseGpu(prompt))
        {
            try
            {
                return await _gpuEngine!.GenerateAsync(prompt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GPU inference failed, falling back to CPU: {Error}", ex.Message);
            }
        }

        // Fallback to CPU engine
        return await GenerateCpuAsync(prompt, cancellationToken);
    }

    private bool ShouldUseGpu(string prompt)
    {
        return _gpuEngine != null 
            && _gpuCapabilities.CudaAvailable 
            && prompt.Length > 100; // Use GPU for longer prompts
    }
}
```

## 🎯 **PHASE 2: CUDA INFERENCE ENGINE (Week 2-3)**

### Create CUDA-Accelerated Inference Engine
**File**: `src/CxLanguage.StandardLibrary/Services/CudaInferenceEngine.cs`

```csharp
#if ENABLE_CUDA
using TorchSharp;
using static TorchSharp.torch;

namespace CxLanguage.StandardLibrary.Services
{
    public interface ICudaInferenceEngine
    {
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
        Task<bool> LoadModelAsync(string modelPath);
        bool IsGpuReady { get; }
    }

    public class CudaInferenceEngine : ICudaInferenceEngine, IDisposable
    {
        private readonly ILogger<CudaInferenceEngine> _logger;
        private readonly ICxEventBus _eventBus;
        private readonly Device _device;
        private bool _isDisposed;

        public bool IsGpuReady { get; private set; }

        public CudaInferenceEngine(ILogger<CudaInferenceEngine> logger, ICxEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
            
            if (cuda.is_available())
            {
                _device = CUDA;
                IsGpuReady = true;
                _logger.LogInformation("🚀 CUDA inference engine initialized on {Device}", 
                    cuda.get_device_name(0));
            }
            else
            {
                throw new InvalidOperationException("CUDA not available for GPU inference");
            }
        }

        public async Task<bool> LoadModelAsync(string modelPath)
        {
            try
            {
                _logger.LogInformation("🔄 Loading model on GPU: {ModelPath}", modelPath);
                
                // Load GGUF model and convert to PyTorch tensors on GPU
                // This is a simplified example - actual implementation would be more complex
                
                await Task.Delay(1000); // Simulate model loading
                
                _eventBus.Emit("gpu.model.loaded", new Dictionary<string, object>
                {
                    ["modelPath"] = modelPath,
                    ["device"] = "cuda",
                    ["vramUsage"] = cuda.memory_allocated(0)
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load model on GPU");
                return false;
            }
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (!IsGpuReady)
                throw new InvalidOperationException("GPU not ready for inference");

            try
            {
                _logger.LogInformation("🚀 Running GPU-accelerated inference");
                
                // GPU-accelerated token generation
                using var _ = NewDisposeScope();
                
                // Encode prompt to tensor
                var inputTensor = EncodePromptToTensor(prompt);
                var gpuInput = inputTensor.to(_device);
                
                // Generate tokens on GPU
                var outputTokens = await GenerateTokensGpu(gpuInput, cancellationToken);
                
                // Decode tokens to text
                var result = DecodeTokensToText(outputTokens);
                
                _eventBus.Emit("gpu.inference.complete", new Dictionary<string, object>
                {
                    ["prompt"] = prompt,
                    ["result"] = result,
                    ["device"] = "cuda",
                    ["inference_time_ms"] = GetInferenceTime()
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GPU inference failed");
                throw;
            }
        }

        private Tensor EncodePromptToTensor(string prompt)
        {
            // Simplified tokenization - real implementation would use proper tokenizer
            var tokens = prompt.Split(' ').Select(t => (long)t.GetHashCode() % 32000).ToArray();
            return tensor(tokens).unsqueeze(0); // Add batch dimension
        }

        private async Task<Tensor> GenerateTokensGpu(Tensor input, CancellationToken cancellationToken)
        {
            // Simplified GPU generation - real implementation would use loaded model
            await Task.Delay(50, cancellationToken); // Simulate fast GPU inference
            
            // Generate next tokens (simplified)
            var batchSize = input.size(0);
            var seqLen = input.size(1);
            var newTokens = torch.randint(0, 32000, new long[] { batchSize, 20 }, device: _device);
            
            return torch.cat(new[] { input, newTokens }, dim: 1);
        }

        private string DecodeTokensToText(Tensor tokens)
        {
            // Simplified detokenization
            var tokenArray = tokens.cpu().data<long>().ToArray();
            return string.Join(" ", tokenArray.Skip(tokens.size(1) - 20).Select(t => $"token_{t % 1000}"));
        }

        private double GetInferenceTime()
        {
            return 75.0; // Simulated fast GPU time
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _logger.LogInformation("🔄 Disposing CUDA inference engine");
                _isDisposed = true;
            }
        }
    }
}
#endif
```

## ⚙️ **PHASE 3: CONFIGURATION & DEPLOYMENT (Week 4)**

### Configuration Management
**File**: `appsettings.json`

```json
{
  "LocalLLM": {
    "PreferGpu": false,
    "GpuAcceleration": {
      "Enable": true,
      "MinVramGB": 8,
      "MaxGpuLayers": 32,
      "FallbackToCpu": true,
      "GpuMemoryFraction": 0.8
    },
    "CudaSettings": {
      "DeviceId": 0,
      "AllowMemoryGrowth": true,
      "MemoryLimit": "16GB"
    }
  }
}
```

### Service Registration
**File**: `src/CxLanguage.StandardLibrary/Extensions/ModernCxServiceExtensions.cs`

```csharp
public static class ModernCxServiceExtensions
{
    public static IServiceCollection AddModernCxServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add GPU capability detection
        services.AddSingleton<IGpuCapabilityService, GpuCapabilityService>();
        
        // Conditional GPU services
        var enableGpu = configuration.GetValue<bool>("LocalLLM:GpuAcceleration:Enable", false);
        if (enableGpu)
        {
#if ENABLE_CUDA
            services.AddSingleton<ICudaInferenceEngine, CudaInferenceEngine>();
#endif
        }

        // Enhanced LocalLLMService with GPU support
        services.AddSingleton<ILocalLLMService, LocalLLMService>();
        
        return services;
    }
}
```

### Build Configuration
**File**: `Directory.Build.props`

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    
    <!-- Conditional CUDA compilation -->
    <DefineConstants Condition="'$(EnableCuda)' == 'true'">$(DefineConstants);ENABLE_CUDA</DefineConstants>
  </PropertyGroup>
</Project>
```

### Development Scripts
**File**: `scripts/build-gpu.ps1`

```powershell
# Build with GPU support
Write-Host "🚀 Building CX Language with GPU acceleration..." -ForegroundColor Green

# Check for NVIDIA GPU
$gpu = Get-WmiObject Win32_VideoController | Where-Object { $_.Name -like "*NVIDIA*" }
if ($gpu) {
    Write-Host "✅ NVIDIA GPU detected: $($gpu.Name)" -ForegroundColor Green
    $enableCuda = "true"
} else {
    Write-Host "⚠️ No NVIDIA GPU detected, building CPU-only" -ForegroundColor Yellow
    $enableCuda = "false"
}

# Build solution with GPU support
dotnet build CxLanguage.sln -p:EnableCuda=$enableCuda

Write-Host "🎉 Build complete!" -ForegroundColor Green
```

**File**: `scripts/test-gpu.cx`

```cx
// GPU capability test in CX Language
conscious GpuTestAgent
{
    realize(self: conscious)
    {
        learn self;
        print("🧪 GPU Capability Test Agent initialized");
        emit gpu.test.start;
    }
    
    on gpu.test.start (event)
    {
        print("🔍 Checking GPU capabilities...");
        
        // Test GPU detection
        execute {
            command: "nvidia-smi --query-gpu=name,memory.total --format=csv,noheader,nounits",
            handlers: [ gpu.info.received ]
        };
    }
    
    on gpu.info.received (event)
    {
        is {
            context: "Is NVIDIA GPU available for acceleration?",
            evaluate: "GPU information received successfully",
            data: { output: event.output },
            handlers: [ gpu.available ]
        };
    }
    
    on gpu.available (event)
    {
        print("✅ GPU available for acceleration");
        print("🎯 Testing GPU-accelerated local LLM...");
        
        // Test GPU-accelerated inference
        think {
            prompt: "Test GPU-accelerated consciousness processing with mathematical reasoning: 2 + 2 = ?",
            preference: "gpu",
            handlers: [ gpu.inference.tested ]
        };
    }
    
    on gpu.inference.tested (event)
    {
        print("🚀 GPU inference test result: " + event.result);
        print("⚡ GPU acceleration operational!");
        
        emit gpu.test.complete { 
            status: "success",
            inferenceTime: event.inferenceTime,
            device: "cuda"
        };
    }
}

// Initialize GPU test
var gpuTester = new GpuTestAgent({ name: "GpuTester" });
emit gpu.test.start;
```

## 🎯 **DEPLOYMENT STRATEGY**

### Multi-Target Deployment
```xml
<!-- Publish CPU-only version (universal compatibility) -->
<Target Name="PublishCpuOnly">
  <Exec Command="dotnet publish -r win-x64 --self-contained -p:EnableCuda=false -o dist/cpu-only" />
</Target>

<!-- Publish GPU-accelerated version (NVIDIA hardware) -->
<Target Name="PublishGpuEnabled">
  <Exec Command="dotnet publish -r win-x64 --self-contained -p:EnableCuda=true -o dist/gpu-enabled" />
</Target>
```

### Installation Detection
```powershell
# Auto-detect and install appropriate version
$hasNvidiaGpu = Get-WmiObject Win32_VideoController | Where-Object { $_.Name -like "*NVIDIA*" }

if ($hasNvidiaGpu) {
    Write-Host "Installing GPU-accelerated CX Language..." -ForegroundColor Green
    # Download GPU-enabled version
} else {
    Write-Host "Installing CPU-optimized CX Language..." -ForegroundColor Cyan
    # Download CPU-only version
}
```

This implementation provides a clear migration path from CPU-only to hybrid CPU/GPU processing while maintaining backward compatibility and universal hardware support.
