# Cx Language Test Runner
# This script runs all test files and reports results

Write-Host "=== Cx Language Test Suite ===" -ForegroundColor Green
Write-Host ""

# Change to the correct directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
Set-Location "$projectRoot\src\CxLanguage.CLI"

$testFiles = @(
    "..\..\tests\test_basic_variables.cx",
    "..\..\tests\test_arithmetic.cx", 
    "..\..\tests\test_comparisons.cx",
    "..\..\tests\test_boolean.cx",
    "..\..\tests\test_if_else.cx",
    "..\..\tests\test_while_loop.cx",
    "..\..\tests\test_assignment.cx",
    "..\..\tests\test_functions.cx",
    "..\..\tests\test_errors.cx"
)

$passed = 0
$failed = 0

foreach ($testFile in $testFiles) {
    $testName = [System.IO.Path]::GetFileNameWithoutExtension($testFile)
    Write-Host "Running test: $testName" -ForegroundColor Yellow
    
    try {
        $output = & dotnet run -- run $testFile 2>&1
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-Host "✓ PASS: $testName" -ForegroundColor Green
            $passed++
        } else {
            Write-Host "✗ FAIL: $testName" -ForegroundColor Red
            Write-Host "Error output: $output" -ForegroundColor Red
            $failed++
        }
    }
    catch {
        Write-Host "✗ FAIL: $testName (Exception)" -ForegroundColor Red
        Write-Host "Exception: $_" -ForegroundColor Red
        $failed++
    }
    
    Write-Host ""
}

Write-Host "=== Test Results ===" -ForegroundColor Green
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor Red
Write-Host "Total:  $($passed + $failed)"

if ($failed -eq 0) {
    Write-Host "All tests passed! ✓" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some tests failed! ✗" -ForegroundColor Red
    exit 1
}
