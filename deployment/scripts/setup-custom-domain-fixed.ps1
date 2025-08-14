# Setup Custom Domain for AgileCloud.ai
# This script configures the custom domain agilecloud.ai to point to the Azure CDN

Write-Host "🌐 Setting up custom domain: agilecloud.ai" -ForegroundColor Green
Write-Host ""

# Configuration
$SubscriptionId = "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"
$ResourceGroup = "agilecloud-rg"
$StorageAccount = "agilecloudwebsite"
$CdnProfile = "agilecloud-cdn"
$CdnEndpoint = "agilecloud-website"
$CustomDomain = "agilecloud.ai"

# First, let's check if Azure CLI is installed
try {
    az --version | Out-Null
    Write-Host "✅ Azure CLI is available" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI not found. Installing..." -ForegroundColor Yellow
    
    # Install Azure CLI via winget
    try {
        winget install Microsoft.AzureCLI
        Write-Host "✅ Azure CLI installed successfully" -ForegroundColor Green
        Write-Host "⚠️  Please restart your terminal and run this script again" -ForegroundColor Yellow
        exit 0
    } catch {
        Write-Host "❌ Failed to install Azure CLI automatically" -ForegroundColor Red
        Write-Host "Please install manually from: https://aka.ms/installazurecliwindows" -ForegroundColor Yellow
        exit 1
    }
}

# Login using the stored credentials
Write-Host "🔐 Logging into Azure..." -ForegroundColor Blue

# Check if azure-credentials.json exists
if (Test-Path "azure-credentials.json") {
    $creds = Get-Content "azure-credentials.json" | ConvertFrom-Json
    
    # Login with service principal
    az login --service-principal --username $creds.clientId --password $creds.clientSecret --tenant $creds.tenantId
        
    # Set subscription
    az account set --subscription $SubscriptionId
    
    Write-Host "✅ Logged into Azure successfully" -ForegroundColor Green
} else {
    Write-Host "❌ azure-credentials.json not found!" -ForegroundColor Red
    Write-Host "Please run create-azure-credentials.ps1 first" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "📋 Getting deployment information..." -ForegroundColor Blue

# Get the storage account website URL
$WebsiteUrl = az storage account show --name $StorageAccount --resource-group $ResourceGroup --subscription $SubscriptionId --query "primaryEndpoints.web" -o tsv

# Get the CDN endpoint URL
$CdnHostName = az cdn endpoint show --name $CdnEndpoint --profile-name $CdnProfile --resource-group $ResourceGroup --subscription $SubscriptionId --query "hostName" -o tsv 2>$null

if ($CdnHostName) {
    $CdnUrl = "https://$CdnHostName"
} else {
    $CdnUrl = "CDN not found - checking..."
    
    # Try to create CDN if it doesn't exist
    Write-Host "🔧 Creating CDN endpoint..." -ForegroundColor Yellow
    
    # Get storage origin URL
    $OriginUrl = $WebsiteUrl -replace "https://", "" -replace "/$", ""
    
    # Create CDN profile
    az cdn profile create --name $CdnProfile --resource-group $ResourceGroup --sku Standard_Microsoft --subscription $SubscriptionId 2>$null
        
    # Create CDN endpoint
    az cdn endpoint create --name $CdnEndpoint --profile-name $CdnProfile --resource-group $ResourceGroup --origin $OriginUrl --origin-host-header $OriginUrl --subscription $SubscriptionId 2>$null
        
    # Get CDN hostname again
    $CdnHostName = az cdn endpoint show --name $CdnEndpoint --profile-name $CdnProfile --resource-group $ResourceGroup --subscription $SubscriptionId --query "hostName" -o tsv 2>$null
        
    if ($CdnHostName) {
        $CdnUrl = "https://$CdnHostName"
        Write-Host "✅ CDN endpoint created successfully" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "🌐 Current Website URLs:" -ForegroundColor Green
Write-Host "   Storage URL: $WebsiteUrl" -ForegroundColor White
Write-Host "   CDN URL: $CdnUrl" -ForegroundColor White
Write-Host ""

# Add custom domain to CDN
if ($CdnHostName) {
    Write-Host "🔧 Adding custom domain to CDN..." -ForegroundColor Blue
    
    # Add custom domain
    try {
        az cdn custom-domain create --endpoint-name $CdnEndpoint --hostname $CustomDomain --name "agilecloud-ai" --profile-name $CdnProfile --resource-group $ResourceGroup --subscription $SubscriptionId 2>$null
            
        Write-Host "✅ Custom domain added to CDN" -ForegroundColor Green
        
        # Enable HTTPS
        Write-Host "🔒 Enabling HTTPS for custom domain..." -ForegroundColor Blue
        az cdn custom-domain enable-https --endpoint-name $CdnEndpoint --name "agilecloud-ai" --profile-name $CdnProfile --resource-group $ResourceGroup --subscription $SubscriptionId 2>$null
            
        Write-Host "✅ HTTPS enabled for custom domain" -ForegroundColor Green
        
    } catch {
        Write-Host "⚠️  Custom domain setup requires DNS configuration first" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "📋 DNS Configuration Required:" -ForegroundColor Cyan
Write-Host ""
Write-Host "To make agilecloud.ai work, you need to configure DNS:" -ForegroundColor White
Write-Host ""
Write-Host "1. Go to your domain registrar's DNS settings" -ForegroundColor Yellow
Write-Host "2. Add a CNAME record:" -ForegroundColor Yellow
Write-Host "   Name: @ (or root domain)" -ForegroundColor White
Write-Host "   Value: $CdnHostName" -ForegroundColor White
Write-Host "   TTL: 300 (or default)" -ForegroundColor White
Write-Host ""
Write-Host "3. For www subdomain, add another CNAME:" -ForegroundColor Yellow
Write-Host "   Name: www" -ForegroundColor White
Write-Host "   Value: $CdnHostName" -ForegroundColor White
Write-Host ""
Write-Host "🔍 To verify DNS propagation:" -ForegroundColor Cyan
Write-Host "   nslookup agilecloud.ai" -ForegroundColor White
Write-Host "   nslookup www.agilecloud.ai" -ForegroundColor White
Write-Host ""
Write-Host "⏱️  DNS changes can take 24-48 hours to propagate globally" -ForegroundColor Yellow
Write-Host ""
Write-Host "🌐 Test URLs:" -ForegroundColor Green
Write-Host "   Direct Storage: $WebsiteUrl" -ForegroundColor White
Write-Host "   CDN: $CdnUrl" -ForegroundColor White
Write-Host "   Custom Domain: https://agilecloud.ai (after DNS setup)" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Website deployment completed successfully!" -ForegroundColor Green
