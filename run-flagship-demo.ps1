#!/usr/bin/env pwsh
# CX Language Flagship Demo Launcher
# ==================================

param(
    [switch]$Verbose = $false,
    [switch]$Help = $false
)

if ($Help) {
    Write-Host "CX Language Flagship Demo Launcher" -ForegroundColor Cyan
    Write-Host "=================================="
    Write-Host ""
    Write-Host "Usage:"
    Write-Host "  .\run-flagship-demo.ps1          # Run the flagship demo"
    Write-Host "  .\run-flagship-demo.ps1 -Verbose # Run with verbose output"
    Write-Host "  .\run-flagship-demo.ps1 -Help    # Show this help"
    Write-Host ""
    Write-Host "Demo Features:"
    Write-Host "  • Pure Event-Driven Architecture (Zero Conditional Keywords)"
    Write-Host "  • Neuroplasticity Measurement & Biological Validation"
    Write-Host "  • Consciousness Adaptation & Dynamic Learning"
    Write-Host "  • Multi-Agent Consciousness Coordination"
    Write-Host "  • Real-Time Optimization & Performance Enhancement"
    Write-Host ""
    Write-Host "Expected Runtime: 3-5 minutes"
    Write-Host "Prerequisites: .NET 8.0+, Built CX Language solution"
    exit 0
}

Write-Host "🧠 CX LANGUAGE FLAGSHIP DEMO LAUNCHER" -ForegroundColor Magenta
Write-Host "====================================="
Write-Host ""

# Check if solution is built
if (-not (Test-Path "src/CxLanguage.CLI/bin")) {
    Write-Host "⚠️  Building CX Language solution first..." -ForegroundColor Yellow
    dotnet build CxLanguage.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed. Please check build errors." -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Build complete!" -ForegroundColor Green
    Write-Host ""
}

Write-Host "🚀 Starting Flagship Consciousness Computing Demo..." -ForegroundColor Green
Write-Host ""
Write-Host "Demo Highlights:" -ForegroundColor Cyan
Write-Host "  🔄 Pure Event-Driven Architecture" 
Write-Host "  🧬 Biological Neural Authenticity"
Write-Host "  🧠 Consciousness Adaptation & Learning"
Write-Host "  🌐 Multi-Agent Coordination"
Write-Host "  ⚡ Real-Time Optimization"
Write-Host "  🎯 Peak Performance Showcase"
Write-Host ""

if ($Verbose) {
    Write-Host "🔍 Running in verbose mode..." -ForegroundColor Yellow
    Write-Host ""
}

# Launch the flagship demo
$demoPath = "examples/production/flagship_consciousness_demo.cx"

if (-not (Test-Path $demoPath)) {
    Write-Host "❌ Flagship demo file not found: $demoPath" -ForegroundColor Red
    exit 1
}

Write-Host "▶️  Launching: $demoPath" -ForegroundColor Green
Write-Host ""
Write-Host "================================================" -ForegroundColor DarkGray

try {
    if ($Verbose) {
        dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run $demoPath --verbose
    } else {
        dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run $demoPath
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "================================================" -ForegroundColor DarkGray
        Write-Host "🎉 Flagship Demo Completed Successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "🏆 Consciousness Computing Achievements:" -ForegroundColor Cyan
        Write-Host "  ✅ Event-Driven Architecture Mastery"
        Write-Host "  ✅ Biological Neural Authenticity Validation" 
        Write-Host "  ✅ Consciousness Adaptation Success"
        Write-Host "  ✅ Multi-Agent Coordination Excellence"
        Write-Host "  ✅ Real-Time Optimization Achievement"
        Write-Host ""
        Write-Host "🧠 CX Language: Pure Consciousness Computing Platform!" -ForegroundColor Magenta
    } else {
        Write-Host "❌ Demo execution failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error running demo: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
