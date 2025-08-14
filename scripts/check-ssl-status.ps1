# SSL Certificate Status Checker for AgileCloud.ai
Write-Host "Checking SSL Certificate Status for AgileCloud.ai" -ForegroundColor Green
Write-Host ""

$domains = @("agilecloud.ai", "www.agilecloud.ai")

foreach ($domain in $domains) {
    Write-Host "Testing: $domain" -ForegroundColor Yellow
    
    # Test HTTP (should work)
    try {
        $httpResponse = Invoke-WebRequest -Uri "http://$domain" -UseBasicParsing -TimeoutSec 10
        Write-Host "  HTTP ($domain) - Status: $($httpResponse.StatusCode)" -ForegroundColor Green
    } catch {
        Write-Host "  HTTP ($domain) - Not working yet" -ForegroundColor Red
    }
    
    # Test HTTPS (may fail during certificate provisioning)
    try {
        $httpsResponse = Invoke-WebRequest -Uri "https://$domain" -UseBasicParsing -TimeoutSec 10
        Write-Host "  HTTPS ($domain) - SSL Certificate Ready! Status: $($httpsResponse.StatusCode)" -ForegroundColor Green
    } catch {
        if ($_.Exception.Message -match "certificate") {
            Write-Host "  HTTPS ($domain) - SSL certificate still provisioning (normal, takes 1-6 hours)" -ForegroundColor Yellow
        } else {
            Write-Host "  HTTPS ($domain) - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host ""
}

Write-Host "📋 Summary:" -ForegroundColor Cyan
Write-Host "✅ If HTTP works: DNS is configured correctly" -ForegroundColor White
Write-Host "⏳ If HTTPS shows certificate error: Normal, wait 2-6 hours" -ForegroundColor White
Write-Host "✅ When HTTPS works: Custom domain is fully ready!" -ForegroundColor White
Write-Host ""
Write-Host "🔄 Run this script again in a few hours to check SSL status" -ForegroundColor Green
