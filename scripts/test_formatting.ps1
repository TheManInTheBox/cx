# CX Language Formatting Service Test

$testCode = @"
conscious calculator{realize(self: conscious){learn self;}handlers:[calculate.request{operation:"add",numbers:[2,2]}]calculate.request event=>{emit infer{data:event.payload,handlers:[calculation.complete]};}}
"@

Write-Host "🎯 CX Language Formatting Test" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Cyan

Write-Host "`n📝 Input (Unformatted K&R Style):" -ForegroundColor Yellow
Write-Host $testCode -ForegroundColor Gray

Write-Host "`n✨ Expected Output (Allman Style):" -ForegroundColor Green

$expectedOutput = @"
conscious calculator
{
    realize(self: conscious)
    {
        learn self;
    }
    
    handlers:
    [
        calculate.request
        {
            operation: "add",
            numbers: [ 2, 2 ]
        }
    ]
    
    calculate.request event =>
    {
        emit infer
        {
            data: event.payload,
            handlers: [ calculation.complete ]
        };
    }
}
"@

Write-Host $expectedOutput -ForegroundColor White

Write-Host "`n🔧 Test Instructions for IDE:" -ForegroundColor Cyan
Write-Host "1. Start the IDE: dotnet run" -ForegroundColor White
Write-Host "2. Clear the editor and paste the unformatted code above" -ForegroundColor White
Write-Host "3. Press Ctrl+F to trigger formatting" -ForegroundColor White
Write-Host "4. Verify the output matches the expected format" -ForegroundColor White

Write-Host "`n✅ Expected Features:" -ForegroundColor Green
Write-Host "• Opening braces { on new lines (Allman style)" -ForegroundColor White
Write-Host "• Proper 4-space indentation" -ForegroundColor White
Write-Host "• Clean consciousness entity structure" -ForegroundColor White
Write-Host "• Properly formatted arrays and objects" -ForegroundColor White
Write-Host "• Correct event handler formatting" -ForegroundColor White
