# AgileCloud.ai Website Deployment Script (PowerShell)
# This script deploys the website to Azure and configures the custom domain

param(
    [switch]$Build = $false,
    [switch]$Force = $false
)

# Configuration
$SubscriptionId = "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"
$ResourceGroup = "agilecloud-rg"
$StorageAccount = "agilecloudwebsite"
$CdnProfile = "agilecloud-cdn"
$CdnEndpoint = "agilecloud-website"
$CustomDomain = "agilecloud.ai"
$Location = "East US"

Write-Host "🚀 Starting AgileCloud.ai website deployment..." -ForegroundColor Green

# Check if Azure CLI is installed
try {
    az --version | Out-Null
} catch {
    Write-Host "❌ Azure CLI is not installed. Please install it first." -ForegroundColor Red
    Write-Host "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Check login status
Write-Host "🔐 Checking Azure login status..." -ForegroundColor Blue
try {
    az account show | Out-Null
} catch {
    Write-Host "🔑 Please login to Azure CLI:" -ForegroundColor Yellow
    az login
}

# Set subscription
Write-Host "📋 Setting Azure subscription..." -ForegroundColor Blue
az account set --subscription $SubscriptionId

# Create resource group
Write-Host "📁 Creating resource group..." -ForegroundColor Blue
az group create --name $ResourceGroup --location $Location

# Create storage account
Write-Host "💾 Creating storage account..." -ForegroundColor Blue
az storage account create `
  --name $StorageAccount `
  --resource-group $ResourceGroup `
  --location $Location `
  --sku Standard_LRS `
  --kind StorageV2 `
  --access-tier Hot

# Enable static website hosting
Write-Host "🌐 Enabling static website hosting..." -ForegroundColor Blue
az storage blob service-properties update `
  --account-name $StorageAccount `
  --static-website `
  --404-document 404.html `
  --index-document index.html

# Build website if requested
if ($Build -and (Test-Path "website\package.json")) {
    Write-Host "🔨 Building website..." -ForegroundColor Blue
    Push-Location website
    
    if (Get-Command npm -ErrorAction SilentlyContinue) {
        Write-Host "📦 Installing dependencies..." -ForegroundColor Blue
        npm install
        Write-Host "🏗️ Building optimized version..." -ForegroundColor Blue
        npm run build
    } else {
        Write-Host "⚠️ npm not found, skipping build" -ForegroundColor Yellow
    }
    
    Pop-Location
}

# Determine source directory
$SourceDir = if (Test-Path "website\dist") {
    Write-Host "📂 Using built files from dist/" -ForegroundColor Green
    "website\dist"
} else {
    Write-Host "📂 Using source files from public/" -ForegroundColor Green
    "website\public"
}

# Get storage account key
Write-Host "🔑 Getting storage account key..." -ForegroundColor Blue
$StorageKey = az storage account keys list `
  --account-name $StorageAccount `
  --resource-group $ResourceGroup `
  --query "[0].value" -o tsv

# Upload files
Write-Host "📤 Uploading website files..." -ForegroundColor Blue
az storage blob upload-batch `
  --destination '$web' `
  --source $SourceDir `
  --account-name $StorageAccount `
  --account-key $StorageKey `
  --overwrite true

# Create CDN profile
Write-Host "🌍 Creating CDN profile..." -ForegroundColor Blue
az cdn profile create `
  --name $CdnProfile `
  --resource-group $ResourceGroup `
  --sku Standard_Microsoft

# Get storage account web endpoint
$OriginUrl = az storage account show `
  --name $StorageAccount `
  --resource-group $ResourceGroup `
  --query "primaryEndpoints.web" -o tsv
$OriginUrl = $OriginUrl -replace "https://", "" -replace "/$", ""

# Create CDN endpoint
Write-Host "⚡ Creating CDN endpoint..." -ForegroundColor Blue
az cdn endpoint create `
  --name $CdnEndpoint `
  --profile-name $CdnProfile `
  --resource-group $ResourceGroup `
  --origin $OriginUrl `
  --origin-host-header $OriginUrl

# Configure custom domain
Write-Host "🔗 Configuring custom domain..." -ForegroundColor Blue
try {
    az cdn custom-domain create `
      --endpoint-name $CdnEndpoint `
      --name agilecloud-custom-domain `
      --profile-name $CdnProfile `
      --resource-group $ResourceGroup `
      --hostname $CustomDomain
} catch {
    Write-Host "⚠️ Custom domain configuration may require manual DNS setup" -ForegroundColor Yellow
}

# Purge CDN cache
Write-Host "🧹 Purging CDN cache..." -ForegroundColor Blue
az cdn endpoint purge `
  --name $CdnEndpoint `
  --profile-name $CdnProfile `
  --resource-group $ResourceGroup `
  --content-paths "/*"

# Get URLs
$WebsiteUrl = az storage account show `
  --name $StorageAccount `
  --resource-group $ResourceGroup `
  --query "primaryEndpoints.web" -o tsv

$CdnUrl = try {
    az cdn endpoint show `
      --name $CdnEndpoint `
      --profile-name $CdnProfile `
      --resource-group $ResourceGroup `
      --query "hostName" -o tsv
} catch {
    "not-configured"
}

Write-Host ""
Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Website URLs:" -ForegroundColor Blue
Write-Host "   Primary: $WebsiteUrl" -ForegroundColor White
if ($CdnUrl -ne "not-configured") {
    Write-Host "   CDN: https://$CdnUrl" -ForegroundColor White
}
Write-Host "   Custom Domain: https://$CustomDomain (requires DNS configuration)" -ForegroundColor White
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Blue
Write-Host "1. Configure DNS CNAME record: $CustomDomain -> $CdnUrl" -ForegroundColor Yellow
Write-Host "2. Enable HTTPS certificate in Azure CDN" -ForegroundColor Yellow
Write-Host "3. Test the website at all URLs" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔧 Azure Resources Created:" -ForegroundColor Blue
Write-Host "   - Resource Group: $ResourceGroup" -ForegroundColor White
Write-Host "   - Storage Account: $StorageAccount" -ForegroundColor White
Write-Host "   - CDN Profile: $CdnProfile" -ForegroundColor White
Write-Host "   - CDN Endpoint: $CdnEndpoint" -ForegroundColor White
