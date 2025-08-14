param (
    [switch]$VerifyGpu,
    [switch]$RunTest,
    [switch]$BuildOnly,
    [string]$ModelPath
)

# PowerShell script to test GPU/TorchSharp integration
Write-Host "🚀 GPU Local LLM Test Script" -ForegroundColor Cyan

# Default model path if not specified
if (-not $ModelPath) {
    $ModelPath = "./models/llama-2-7b-chat.Q4_K_M.gguf"
}

# Set environment variables for TorchSharp
$env:CUDA_VISIBLE_DEVICES = "0"
$env:PYTORCH_CUDA_ALLOC_CONF = "max_split_size_mb:128"

function Test-GpuAvailability {
    Write-Host "🔍 Verifying GPU availability with TorchSharp..." -ForegroundColor Yellow
    
    # Build and run the GPU detection tool
    try {
        $outputPath = "./bin/GpuDetection"
        
        # Ensure directory exists
        if (-not (Test-Path $outputPath)) {
            New-Item -Path $outputPath -ItemType Directory -Force | Out-Null
        }
        
        # Create temporary C# file for GPU detection
        $gpuDetectionCode = @'
using System;
using TorchSharp;
using static TorchSharp.torch;

namespace GpuDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("🔍 GPU Detection with TorchSharp");
            
            try
            {
                bool cudaAvailable = cuda.is_available();
                Console.WriteLine($"✅ CUDA available: {cudaAvailable}");
                
                if (cudaAvailable)
                {
                    int deviceCount = cuda.device_count();
                    Console.WriteLine($"📊 CUDA device count: {deviceCount}");
                    
                    for (int i = 0; i < deviceCount; i++)
                    {
                        cuda.set_device(i);
                        Console.WriteLine($"  - Device {i}: {cuda.get_device_name(i)}");
                    }
                    
                    // Create a simple tensor on GPU
                    using (var tensor = torch.ones(new long[] { 3, 3 }, device: cuda.current_device()))
                    {
                        Console.WriteLine($"📈 Test tensor on GPU created successfully");
                        Console.WriteLine($"   Shape: {string.Join("x", tensor.shape)}");
                        Console.WriteLine($"   Device: {tensor.device_type}");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ CUDA is not available on this system");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
'@
        
        $gpuDetectionPath = Join-Path $outputPath "GpuDetection.cs"
        Set-Content -Path $gpuDetectionPath -Value $gpuDetectionCode
        
        # Create project file
        $projectCode = @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="TorchSharp" Version="0.102.5" />
  </ItemGroup>

</Project>
'@
        
        $projectPath = Join-Path $outputPath "GpuDetection.csproj"
        Set-Content -Path $projectPath -Value $projectCode
        
        # Build and run
        Write-Host "🔨 Building GPU detection tool..." -ForegroundColor Yellow
        dotnet build $projectPath -c Release
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
            return $false
        }
        
        Write-Host "🏃 Running GPU detection..." -ForegroundColor Yellow
        dotnet run --project $projectPath --configuration Release
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ GPU detection failed with exit code $LASTEXITCODE" -ForegroundColor Red
            return $false
        }
        
        return $true
    }
    catch {
        Write-Host "❌ Error verifying GPU: $_" -ForegroundColor Red
        return $false
    }
}

function New-GpuLocalLlmService {
    Write-Host "🔨 Building GpuLocalLLMService..." -ForegroundColor Yellow
    
    try {
        # Build the LocalLLM project
        dotnet build ./src/CxLanguage.LocalLLM/CxLanguage.LocalLLM.csproj -c Release
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
            return $false
        }
        
        Write-Host "✅ Build successful" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ Error building GpuLocalLLMService: $_" -ForegroundColor Red
        return $false
    }
}

function Start-LlmTest {
    param (
        [string]$ModelPath
    )
    
    Write-Host "🏃 Running LLM test with model: $ModelPath" -ForegroundColor Yellow
    
    try {
        # Create temporary test project
        $outputPath = "./bin/LlmTest"
        
        # Ensure directory exists
        if (-not (Test-Path $outputPath)) {
            New-Item -Path $outputPath -ItemType Directory -Force | Out-Null
        }
        
        # Create test program
        $testCode = @"
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CxLanguage.LocalLLM;

namespace LlmTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 GPU Local LLM Test");
            
            try
            {
                // Set up dependency injection
                var services = new ServiceCollection();
                
                // Add logging
                services.AddLogging(builder => 
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddConsole();
                });
                
                // Add GPU local LLM service
                services.AddGpuLocalLlm();
                
                // Build service provider
                var serviceProvider = services.BuildServiceProvider();
                
                // Get local LLM service
                var llmService = serviceProvider.GetRequiredService<ILocalLLMService>();
                
                // Initialize
                Console.WriteLine("🚀 Initializing service...");
                bool initialized = await llmService.InitializeAsync();
                Console.WriteLine($"✅ Initialization: {initialized}");
                
                // Load model
                string modelPath = "$ModelPath";
                Console.WriteLine($"📥 Loading model: {modelPath}");
                bool modelLoaded = await llmService.LoadModelAsync(modelPath);
                Console.WriteLine($"✅ Model loaded: {modelLoaded}");
                
                if (modelLoaded)
                {
                    var modelInfo = llmService.ModelInfo;
                    Console.WriteLine($"📊 Model info: {modelInfo?.Name}, {modelInfo?.Architecture}, {modelInfo?.SizeBytes / (1024 * 1024)}MB");
                }
                
                // Generate
                string prompt = "Explain how consciousness works in 3 sentences.";
                Console.WriteLine($"🧠 Generating with prompt: {prompt}");
                
                string result = await llmService.GenerateAsync(prompt);
                Console.WriteLine($"📝 Result: {result}");
                
                // Stream
                Console.WriteLine("\n📺 Streaming with prompt: Write a short poem about artificial consciousness.");
                
                await foreach (var token in llmService.StreamAsync("Write a short poem about artificial consciousness."))
                {
                    Console.Write(token);
                }
                
                Console.WriteLine("\n\n✅ Streaming complete");
                
                // Unload
                Console.WriteLine("📤 Unloading model...");
                await llmService.UnloadModelAsync();
                Console.WriteLine("✅ Model unloaded");
                
                // Dispose
                if (llmService is IDisposable disposable)
                {
                    Console.WriteLine("🧹 Disposing service...");
                    disposable.Dispose();
                    Console.WriteLine("✅ Service disposed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
"@
        
        $testPath = Join-Path $outputPath "Program.cs"
        Set-Content -Path $testPath -Value $testCode
        
        # Create project file
        $projectCode = @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\CxLanguage.LocalLLM\CxLanguage.LocalLLM.csproj" />
  </ItemGroup>

</Project>
'@
        
        $projectPath = Join-Path $outputPath "LlmTest.csproj"
        Set-Content -Path $projectPath -Value $projectCode
        
        # Build and run
        Write-Host "🔨 Building LLM test..." -ForegroundColor Yellow
        dotnet build $projectPath -c Release
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
            return $false
        }
        
        Write-Host "🏃 Running LLM test..." -ForegroundColor Yellow
        dotnet run --project $projectPath --configuration Release
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ LLM test failed with exit code $LASTEXITCODE" -ForegroundColor Red
            return $false
        }
        
        return $true
    }
    catch {
        Write-Host "❌ Error running LLM test: $_" -ForegroundColor Red
        return $false
    }
}

# Main execution flow
if ($VerifyGpu) {
    Test-GpuAvailability
}
elseif ($BuildOnly) {
    New-GpuLocalLlmService
}
elseif ($RunTest) {
    if (New-GpuLocalLlmService) {
        Start-LlmTest -ModelPath $ModelPath
    }
}
else {
    # Default: Verify GPU, build, and run test
    if (Test-GpuAvailability) {
        if (New-GpuLocalLlmService) {
            Start-LlmTest -ModelPath $ModelPath
        }
    }
}

Write-Host "✅ GPU Local LLM Test Script Complete" -ForegroundColor Cyan
