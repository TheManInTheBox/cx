# CX Language Unity Bridge Launcher Script
# Automatically detects and launches Unity with the CX Language project

param(
    [string]$UnityVersion = "",
    [switch]$ListVersions,
    [switch]$BatchMode,
    [switch]$NoGraphics,
    [string]$LogFile = ""
)

Write-Host "🎮 CX Language Unity Bridge Launcher" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

# Unity Hub installation path
$unityHubPath = "C:\Program Files\Unity\Hub\Editor"
$projectPath = $PSScriptRoot

# Function to get all Unity installations
function Get-UnityInstallations {
    $installations = @()
    
    if (Test-Path $unityHubPath) {
        $versionDirs = Get-ChildItem -Path $unityHubPath -Directory
        
        foreach ($versionDir in $versionDirs) {
            $unityExe = Join-Path $versionDir.FullName "Editor\Unity.exe"
            
            if (Test-Path $unityExe) {
                $installation = [PSCustomObject]@{
                    Version = $versionDir.Name
                    Path = $versionDir.FullName
                    ExecutablePath = $unityExe
                    IsLTS = $versionDir.Name -match ".*f\d+$" -and ($versionDir.Name.StartsWith("2022.3") -or $versionDir.Name.StartsWith("2023.3") -or $versionDir.Name.StartsWith("2024.3"))
                    InstallDate = $versionDir.CreationTime
                }
                $installations += $installation
            }
        }
    }
    
    # Check for manual installations
    $manualPaths = @(
        "C:\Program Files\Unity\Editor\Unity.exe",
        "C:\Program Files (x86)\Unity\Editor\Unity.exe",
        "C:\Unity\Editor\Unity.exe"
    )
    
    foreach ($path in $manualPaths) {
        if (Test-Path $path) {
            $installation = [PSCustomObject]@{
                Version = "Manual Installation"
                Path = Split-Path $path -Parent
                ExecutablePath = $path
                IsLTS = $false
                InstallDate = (Get-Item $path).CreationTime
            }
            $installations += $installation
        }
    }
    
    return $installations | Sort-Object InstallDate -Descending
}

# Function to select best Unity version
function Select-BestUnityVersion {
    param($installations)
    
    # Prefer LTS versions
    $ltsVersions = $installations | Where-Object { $_.IsLTS }
    if ($ltsVersions.Count -gt 0) {
        return $ltsVersions[0]
    }
    
    # Fall back to latest version
    if ($installations.Count -gt 0) {
        return $installations[0]
    }
    
    return $null
}

# Get Unity installations
Write-Host "🔍 Detecting Unity installations..." -ForegroundColor Yellow
$installations = Get-UnityInstallations

if ($installations.Count -eq 0) {
    Write-Host "❌ No Unity installations found!" -ForegroundColor Red
    Write-Host "Please install Unity through Unity Hub or manually." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Found $($installations.Count) Unity installation(s)" -ForegroundColor Green

# List versions if requested
if ($ListVersions) {
    Write-Host "`n📋 Available Unity Versions:" -ForegroundColor Cyan
    for ($i = 0; $i -lt $installations.Count; $i++) {
        $inst = $installations[$i]
        $ltsTag = if ($inst.IsLTS) { " (LTS)" } else { "" }
        Write-Host "  $($i + 1). $($inst.Version)$ltsTag" -ForegroundColor White
        Write-Host "      Path: $($inst.ExecutablePath)" -ForegroundColor Gray
    }
    exit 0
}

# Select Unity version
$selectedUnity = $null

if ($UnityVersion -ne "") {
    # Find specific version
    $selectedUnity = $installations | Where-Object { $_.Version -like "*$UnityVersion*" } | Select-Object -First 1
    
    if ($null -eq $selectedUnity) {
        Write-Host "❌ Unity version '$UnityVersion' not found!" -ForegroundColor Red
        Write-Host "Use -ListVersions to see available versions." -ForegroundColor Yellow
        exit 1
    }
} else {
    # Select best version automatically
    $selectedUnity = Select-BestUnityVersion $installations
}

Write-Host "🎯 Selected Unity: $($selectedUnity.Version)" -ForegroundColor Green
Write-Host "📂 Unity Path: $($selectedUnity.ExecutablePath)" -ForegroundColor Gray
Write-Host "📁 Project Path: $projectPath" -ForegroundColor Gray

# Build Unity launch arguments
$unityArgs = @()
$unityArgs += "-projectPath"
$unityArgs += "`"$projectPath`""

if ($BatchMode) {
    $unityArgs += "-batchmode"
}

if ($NoGraphics) {
    $unityArgs += "-nographics"
}

if ($LogFile -ne "") {
    $logPath = $LogFile
    if (-not [System.IO.Path]::IsPathRooted($logPath)) {
        $logPath = Join-Path $projectPath $logPath
    }
    
    # Create log directory if needed
    $logDir = Split-Path $logPath -Parent
    if (-not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    
    $unityArgs += "-logFile"
    $unityArgs += "`"$logPath`""
    
    Write-Host "📝 Log File: $logPath" -ForegroundColor Gray
}

# Launch Unity
Write-Host "`n🚀 Launching Unity Editor..." -ForegroundColor Cyan
Write-Host "Command: $($selectedUnity.ExecutablePath) $($unityArgs -join ' ')" -ForegroundColor Gray

try {
    $process = Start-Process -FilePath $selectedUnity.ExecutablePath -ArgumentList $unityArgs -PassThru -NoNewWindow
    
    Write-Host "✅ Unity launched successfully!" -ForegroundColor Green
    Write-Host "🆔 Process ID: $($process.Id)" -ForegroundColor Gray
    
    # Wait a moment and check if process is still running
    Start-Sleep -Seconds 2
    
    if ($process.HasExited) {
        Write-Host "⚠️ Unity process exited immediately. Check for errors." -ForegroundColor Yellow
        if ($process.ExitCode -ne 0) {
            Write-Host "❌ Unity exited with code: $($process.ExitCode)" -ForegroundColor Red
        }
    } else {
        Write-Host "🎮 Unity Editor is running. You can now work with the CX Language Unity Bridge!" -ForegroundColor Green
        Write-Host "`n💡 Tips:" -ForegroundColor Cyan
        Write-Host "  • Open the CxBridgeDemo scene in Assets/Scenes/" -ForegroundColor White
        Write-Host "  • Check the Console window for CX Language integration logs" -ForegroundColor White
        Write-Host "  • Use the Unity Inspector to configure CX Language Bridge settings" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Failed to launch Unity: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 CX Language Unity Bridge setup complete!" -ForegroundColor Green
