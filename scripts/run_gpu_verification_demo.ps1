param (
    [switch]$SkipModelDownload,
    [string]$ModelPath,
    [switch]$ForceCpu
)

# PowerShell script to run the GPU LLM verification demo
Write-Host "🚀 GPU Local LLM Verification Demo Runner" -ForegroundColor Cyan

# Default model path if not specified
if (-not $ModelPath) {
    $ModelPath = "./models/llama-2-7b-chat.Q4_K_M.gguf"
}

# Model URL for download if needed
$ModelUrl = "https://huggingface.co/TheBloke/Llama-2-7B-Chat-GGUF/resolve/main/llama-2-7b-chat.Q4_K_M.gguf"

# Ensure models directory exists
if (-not (Test-Path "./models")) {
    Write-Host "📁 Creating models directory..." -ForegroundColor Yellow
    New-Item -Path "./models" -ItemType Directory -Force | Out-Null
}

# Download model if needed
if (-not $SkipModelDownload -and -not (Test-Path $ModelPath)) {
    Write-Host "📥 Model not found. Downloading from HuggingFace..." -ForegroundColor Yellow
    try {
        $modelFileName = Split-Path $ModelPath -Leaf
        $downloadPath = Join-Path "./models" $modelFileName
        
        Write-Host "🔗 Downloading from: $ModelUrl" -ForegroundColor Yellow
        Write-Host "📂 Saving to: $downloadPath" -ForegroundColor Yellow
        
        # Create a WebClient to download the file with progress reporting
        $webClient = New-Object System.Net.WebClient
        $downloadComplete = $false
        
        # Register event for download progress
        $eventDataComplete = Register-ObjectEvent -InputObject $webClient -EventName DownloadFileCompleted -Action {
            Write-Host "`n✅ Download completed!" -ForegroundColor Green
            $Global:downloadComplete = $true
        }
        
        # Register event for download progress
        $eventData = Register-ObjectEvent -InputObject $webClient -EventName DownloadProgressChanged -Action {
            $percent = $EventArgs.ProgressPercentage
            $totalBytes = $EventArgs.TotalBytesToReceive
            $receivedBytes = $EventArgs.BytesReceived
            
            $totalMB = [Math]::Round($totalBytes / 1MB, 2)
            $receivedMB = [Math]::Round($receivedBytes / 1MB, 2)
            
            # Create progress bar
            $progressBar = "["
            $progressBarLength = 20
            $filledLength = [Math]::Round($percent / 100 * $progressBarLength)
            
            for ($i = 0; $i -lt $progressBarLength; $i++) {
                if ($i -lt $filledLength) {
                    $progressBar += "="
                } else {
                    $progressBar += " "
                }
            }
            
            $progressBar += "]"
            
            # Display progress
            Write-Host "`r$progressBar $percent% ($receivedMB MB / $totalMB MB)" -NoNewline
        }
        
        # Start download
        $webClient.DownloadFileAsync((New-Object System.Uri($ModelUrl)), $downloadPath)
        
        # Wait for download to complete
        while (-not $downloadComplete) {
            Start-Sleep -Milliseconds 100
        }
        
        # Unregister events
        Unregister-Event -SourceIdentifier $eventData.Name
        Unregister-Event -SourceIdentifier $eventDataComplete.Name
        
        $ModelPath = $downloadPath
        Write-Host "📚 Model downloaded successfully to $ModelPath" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Error downloading model: $_" -ForegroundColor Red
        exit 1
    }
}

# Verify model exists
if (-not (Test-Path $ModelPath)) {
    Write-Host "❌ Model not found at path: $ModelPath" -ForegroundColor Red
    Write-Host "Please provide a valid model path or allow the script to download it." -ForegroundColor Red
    exit 1
}

# Set environment variables
if ($ForceCpu) {
    Write-Host "⚠️ Forcing CPU mode (GPU disabled)" -ForegroundColor Yellow
    $env:CUDA_VISIBLE_DEVICES = "-1"  # Disable CUDA
    $env:DISABLE_GPU = "true"
} else {
    Write-Host "🔍 Using GPU if available" -ForegroundColor Yellow
    $env:CUDA_VISIBLE_DEVICES = "0"  # Enable primary GPU
    $env:PYTORCH_CUDA_ALLOC_CONF = "max_split_size_mb:128"
    $env:DISABLE_GPU = "false"
}

# Build the verification demo
Write-Host "🔨 Building the verification demo..." -ForegroundColor Yellow
try {
    dotnet build ./examples/gpu_llm_verification_demo.cs -o ./bin/VerificationDemo
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Build successful" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error building verification demo: $_" -ForegroundColor Red
    exit 1
}

# Run the verification demo
Write-Host "🏃 Running the verification demo with model: $ModelPath" -ForegroundColor Yellow
try {
    dotnet ./bin/VerificationDemo/gpu_llm_verification_demo.dll "$ModelPath"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Verification demo failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "❌ Error running verification demo: $_" -ForegroundColor Red
    exit 1
}

Write-Host "✅ GPU Local LLM Verification Demo completed successfully" -ForegroundColor Green
