param (
    [string]$ModelPath,
    [switch]$BuildOnly
)

# PowerShell script to run the GPU Local LLM verification demo
Write-Host "🚀 GPU Local LLM Verification Demo" -ForegroundColor Cyan

# Default model path if not specified
if (-not $ModelPath) {
    $ModelPath = "./models/llama-2-7b-chat.Q4_K_M.gguf"
}

# Ensure directories exist
$outputPath = "./bin/VerificationDemo"
if (-not (Test-Path $outputPath)) {
    New-Item -Path $outputPath -ItemType Directory -Force | Out-Null
}

# Create project file
$projectCode = @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\src\CxLanguage.LocalLLM\CxLanguage.LocalLLM.csproj" />
    <ProjectReference Include="..\src\CxLanguage.Runtime\CxLanguage.Runtime.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.4" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.4" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.4" />
  </ItemGroup>

</Project>
'@

$projectPath = Join-Path $outputPath "CxLanguage.VerificationDemo.csproj"
Set-Content -Path $projectPath -Value $projectCode

# Copy the verification demo code
$sourcePath = "./examples/gpu_llm_verification_demo.cs"
$destPath = Join-Path $outputPath "Program.cs"
Copy-Item -Path $sourcePath -Destination $destPath -Force

# Build the project
Write-Host "🔨 Building verification demo..." -ForegroundColor Yellow
dotnet build $projectPath -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Run the demo if not build-only
if (-not $BuildOnly) {
    Write-Host "🏃 Running verification demo..." -ForegroundColor Yellow
    dotnet run --project $projectPath --configuration Release -- $ModelPath

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Demo failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Write-Host "✅ GPU Local LLM Verification Demo Complete" -ForegroundColor Green
