# SSL Certificate Status Checker for AgileCloud.ai
Write-Host "Checking SSL Certificate Status for AgileCloud.ai" -ForegroundColor Green
Write-Host ""

Write-Host "Testing: www.agilecloud.ai" -ForegroundColor Yellow

# Test HTTP
try {
    $httpResponse = Invoke-WebRequest -Uri "http://www.agilecloud.ai" -UseBasicParsing -TimeoutSec 10
    Write-Host "  HTTP: Working! Status $($httpResponse.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  HTTP: Not working yet" -ForegroundColor Red
}

# Test HTTPS
try {
    $httpsResponse = Invoke-WebRequest -Uri "https://www.agilecloud.ai" -UseBasicParsing -TimeoutSec 10
    Write-Host "  HTTPS: SSL Certificate Ready! Status $($httpsResponse.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  HTTPS: SSL certificate still provisioning (takes 1-6 hours)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "- If HTTP works: DNS is configured correctly" -ForegroundColor White
Write-Host "- If HTTPS shows certificate error: Normal, wait 2-6 hours" -ForegroundColor White
Write-Host "- When HTTPS works: Custom domain is fully ready!" -ForegroundColor White
