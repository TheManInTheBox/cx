# Test AgileCloud.ai Website Deployment
Write-Host "🌐 Testing AgileCloud.ai website deployment..." -ForegroundColor Green
Write-Host ""

# Test URLs based on standard Azure naming
$StorageUrl = "https://agilecloudwebsite.z13.web.core.windows.net/"
$CdnUrl = "https://agilecloud-website.azureedge.net/"

Write-Host "Testing deployment URLs..." -ForegroundColor Blue
Write-Host ""

# Test storage URL
Write-Host "1. Testing Storage URL: $StorageUrl" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $StorageUrl -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ Storage website is LIVE!" -ForegroundColor Green
        Write-Host "   📄 Title found: " -NoNewline -ForegroundColor Gray
        if ($response.Content -match '<title>(.*?)</title>') {
            Write-Host $matches[1] -ForegroundColor White
        }
    }
} catch {
    Write-Host "   ❌ Storage URL not responding: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test CDN URL  
Write-Host "2. Testing CDN URL: $CdnUrl" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $CdnUrl -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ CDN website is LIVE!" -ForegroundColor Green
        Write-Host "   📄 Title found: " -NoNewline -ForegroundColor Gray
        if ($response.Content -match '<title>(.*?)</title>') {
            Write-Host $matches[1] -ForegroundColor White
        }
    }
} catch {
    Write-Host "   ❌ CDN URL not responding: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🔍 DNS Check for agilecloud.ai:" -ForegroundColor Cyan
try {
    $dnsResult = nslookup agilecloud.ai 2>$null
    if ($dnsResult) {
        Write-Host "   📡 DNS is configured" -ForegroundColor Green
        Write-Host $dnsResult -ForegroundColor Gray
    } else {
        Write-Host "   ⚠️  DNS not configured yet - this is expected if you haven't set up DNS records" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ⚠️  DNS not configured yet - this is expected if you haven't set up DNS records" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📋 Next Steps for Custom Domain:" -ForegroundColor Green
Write-Host "1. Go to Azure Portal → Resource Groups → agilecloud-rg" -ForegroundColor White
Write-Host "2. Find your CDN endpoint and copy the hostname" -ForegroundColor White  
Write-Host "3. Configure DNS with your domain registrar:" -ForegroundColor White
Write-Host "   - CNAME: agilecloud.ai → [your-cdn-endpoint].azureedge.net" -ForegroundColor Gray
Write-Host "   - CNAME: www → [your-cdn-endpoint].azureedge.net" -ForegroundColor Gray
Write-Host "4. Add custom domain in Azure CDN settings" -ForegroundColor White
Write-Host "5. Enable HTTPS for custom domain" -ForegroundColor White
Write-Host ""
Write-Host "📖 See CUSTOM_DOMAIN_SETUP_GUIDE.md for detailed instructions" -ForegroundColor Cyan
