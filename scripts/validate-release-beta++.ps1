# CX Language v1.0.0-beta++ Release Readiness Validation Script
# PowerShell Version

Write-Host "🎭 CX Language v1.0.0-beta++ Release Readiness Check" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

$errors = 0

function Test-FileExists {
    param($filePath)
    if (Test-Path $filePath) {
        Write-Host "✅ $filePath" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ $filePath - MISSING!" -ForegroundColor Red
        return $false
    }
}

function Test-DirectoryExists {
    param($dirPath)
    if (Test-Path $dirPath -PathType Container) {
        Write-Host "✅ $dirPath/" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ $dirPath/ - MISSING!" -ForegroundColor Red
        return $false
    }
}

Write-Host "🎯 Premier Multi-Agent Demo Files:" -ForegroundColor Yellow
if (-not (Test-FileExists "examples/advanced_debate_demo.cx")) { $errors++ }
if (-not (Test-FileExists "wiki/Premier-Multi-Agent-Voice-Debate-Demo.md")) { $errors++ }
Write-Host ""

Write-Host "📚 Documentation Files:" -ForegroundColor Yellow
if (-not (Test-FileExists "README.md")) { $errors++ }
if (-not (Test-FileExists "CHANGELOG.md")) { $errors++ }
if (-not (Test-FileExists "RELEASE_CHECKLIST_BETA_PLUS_PLUS.md")) { $errors++ }
Write-Host ""

Write-Host "🏗️ Build Configuration:" -ForegroundColor Yellow
if (-not (Test-FileExists "Directory.Build.props")) { $errors++ }
if (-not (Test-FileExists "CxLanguage.sln")) { $errors++ }
Write-Host ""

Write-Host "🔄 CI/CD Pipeline:" -ForegroundColor Yellow
if (-not (Test-FileExists ".github/workflows/ci.yml")) { $errors++ }
if (-not (Test-FileExists ".github/workflows/cd.yml")) { $errors++ }
Write-Host ""

Write-Host "📁 Project Structure:" -ForegroundColor Yellow
if (-not (Test-DirectoryExists "src")) { $errors++ }
if (-not (Test-DirectoryExists "examples")) { $errors++ }
if (-not (Test-DirectoryExists "wiki")) { $errors++ }
if (-not (Test-DirectoryExists "grammar")) { $errors++ }
Write-Host ""

Write-Host "🔍 Version Consistency Check:" -ForegroundColor Yellow
Write-Host "Checking for version 1.0.0-beta++ in key files..."

if (Select-String -Path "Directory.Build.props" -Pattern "1.0.0-beta\+\+" -Quiet) {
    Write-Host "✅ Directory.Build.props version updated" -ForegroundColor Green
} else {
    Write-Host "❌ Directory.Build.props version NOT updated" -ForegroundColor Red
    $errors++
}

if (Select-String -Path "CHANGELOG.md" -Pattern "1.0.0-beta\+\+" -Quiet) {
    Write-Host "✅ CHANGELOG.md version updated" -ForegroundColor Green
} else {
    Write-Host "❌ CHANGELOG.md version NOT updated" -ForegroundColor Red
    $errors++
}

Write-Host ""
Write-Host "🎭 Premier Demo Validation:" -ForegroundColor Yellow
if (Select-String -Path "README.md" -Pattern "advanced_debate_demo.cx" -Quiet) {
    Write-Host "✅ Premier demo referenced in README" -ForegroundColor Green
} else {
    Write-Host "❌ Premier demo NOT referenced in README" -ForegroundColor Red
    $errors++
}

if (Select-String -Path "README.md" -Pattern "Multi-Agent Voice" -Quiet) {
    Write-Host "✅ Multi-Agent Voice features highlighted in README" -ForegroundColor Green
} else {
    Write-Host "❌ Multi-Agent Voice features NOT highlighted in README" -ForegroundColor Red
    $errors++
}

Write-Host ""
Write-Host "🚀 CI/CD Pipeline Validation:" -ForegroundColor Yellow
if (Select-String -Path ".github/workflows/ci.yml" -Pattern "Multi-Agent Voice" -Quiet) {
    Write-Host "✅ CI pipeline updated for Multi-Agent Voice" -ForegroundColor Green
} else {
    Write-Host "❌ CI pipeline NOT updated for Multi-Agent Voice" -ForegroundColor Red
    $errors++
}

if (Select-String -Path ".github/workflows/ci.yml" -Pattern "advanced_debate_demo.cx" -Quiet) {
    Write-Host "✅ Premier demo testing in CI pipeline" -ForegroundColor Green
} else {
    Write-Host "❌ Premier demo testing NOT in CI pipeline" -ForegroundColor Red
    $errors++
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
if ($errors -eq 0) {
    Write-Host "🎉 RELEASE READY! All validation checks passed." -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Run: dotnet build --configuration Release" -ForegroundColor White
    Write-Host "2. Test: dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/advanced_debate_demo.cx" -ForegroundColor White
    Write-Host "3. Create tag: git tag v1.0.0-beta++" -ForegroundColor White
    Write-Host "4. Push tag: git push origin v1.0.0-beta++" -ForegroundColor White
    Write-Host ""
    Write-Host "🎭 CX Language Multi-Agent Voice Platform is ready for release!" -ForegroundColor Magenta
    exit 0
} else {
    Write-Host "❌ RELEASE NOT READY! Found $errors errors." -ForegroundColor Red
    Write-Host ""
    Write-Host "Please fix the issues above before releasing." -ForegroundColor Yellow
    exit 1
}
