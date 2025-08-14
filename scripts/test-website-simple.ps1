# Test AgileCloud.ai Website Deployment
Write-Host "Testing AgileCloud.ai website deployment..." -ForegroundColor Green

# Test URLs based on standard Azure naming
$StorageUrl = "https://agilecloudwebsite.z13.web.core.windows.net/"
$CdnUrl = "https://agilecloud-website.azureedge.net/"

Write-Host ""
Write-Host "1. Testing Storage URL: $StorageUrl" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $StorageUrl -UseBasicParsing -TimeoutSec 10
    Write-Host "   ✅ Storage website is LIVE! Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Storage URL not responding" -ForegroundColor Red
}

Write-Host ""
Write-Host "2. Testing CDN URL: $CdnUrl" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $CdnUrl -UseBasicParsing -TimeoutSec 10
    Write-Host "   ✅ CDN website is LIVE! Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ❌ CDN URL not responding" -ForegroundColor Red
}

Write-Host ""
Write-Host "Next Steps for Custom Domain:" -ForegroundColor Green
Write-Host "1. Go to Azure Portal → Resource Groups → agilecloud-rg" -ForegroundColor White
Write-Host "2. Find your CDN endpoint and copy the hostname" -ForegroundColor White  
Write-Host "3. Configure DNS with your domain registrar" -ForegroundColor White
Write-Host "4. Add custom domain in Azure CDN settings" -ForegroundColor White
Write-Host ""
Write-Host "See CUSTOM_DOMAIN_SETUP_GUIDE.md for detailed instructions" -ForegroundColor Cyan
