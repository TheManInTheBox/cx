# DNS Verification Script for AgileCloud.ai
Write-Host "🌐 DNS Verification for AgileCloud.ai" -ForegroundColor Green
Write-Host ""

# Target CDN endpoint
$CdnEndpoint = "agilecloud-website.azureedge.net"

Write-Host "Expected DNS Configuration:" -ForegroundColor Cyan
Write-Host "  agilecloud.ai CNAME → $CdnEndpoint" -ForegroundColor White
Write-Host "  www.agilecloud.ai CNAME → $CdnEndpoint" -ForegroundColor White
Write-Host ""

# Check DNS for root domain
Write-Host "Checking DNS for agilecloud.ai..." -ForegroundColor Yellow
try {
    $rootDns = nslookup agilecloud.ai 2>$null
    if ($rootDns -match $CdnEndpoint) {
        Write-Host "✅ agilecloud.ai DNS is correctly configured!" -ForegroundColor Green
    } else {
        Write-Host "❌ agilecloud.ai DNS not configured yet" -ForegroundColor Red
        Write-Host "   Add CNAME: agilecloud.ai → $CdnEndpoint" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ agilecloud.ai DNS not configured yet" -ForegroundColor Red
}

Write-Host ""

# Check DNS for www subdomain  
Write-Host "Checking DNS for www.agilecloud.ai..." -ForegroundColor Yellow
try {
    $wwwDns = nslookup www.agilecloud.ai 2>$null
    if ($wwwDns -match $CdnEndpoint) {
        Write-Host "✅ www.agilecloud.ai DNS is correctly configured!" -ForegroundColor Green
    } else {
        Write-Host "❌ www.agilecloud.ai DNS not configured yet" -ForegroundColor Red
        Write-Host "   Add CNAME: www → $CdnEndpoint" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ www.agilecloud.ai DNS not configured yet" -ForegroundColor Red
}

Write-Host ""

# Test website availability
Write-Host "Testing website availability..." -ForegroundColor Blue
Write-Host ""

$testUrls = @(
    "https://agilecloud.ai",
    "https://www.agilecloud.ai",
    "https://agilecloud-website.azureedge.net"
)

foreach ($url in $testUrls) {
    Write-Host "Testing: $url" -ForegroundColor Gray
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
        Write-Host "✅ $url is working! (Status: $($response.StatusCode))" -ForegroundColor Green
    } catch {
        Write-Host "❌ $url not accessible yet" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Configure DNS records with your domain registrar" -ForegroundColor White
Write-Host "2. Wait 24-48 hours for DNS propagation" -ForegroundColor White  
Write-Host "3. Run this script again to verify" -ForegroundColor White
Write-Host ""
Write-Host "📖 See DNS_SETUP_REQUIRED.md for detailed instructions" -ForegroundColor Yellow
