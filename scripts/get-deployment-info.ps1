# Get Azure Deployment URLs for Custom Domain Setup
Write-Host "Getting Azure deployment information..." -ForegroundColor Green

# Configuration
$SubscriptionId = "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"
$ResourceGroup = "agilecloud-rg"
$StorageAccount = "agilecloudwebsite"
$CdnProfile = "agilecloud-cdn"
$CdnEndpoint = "agilecloud-website"

# Check if azure-credentials.json exists
if (Test-Path "azure-credentials.json") {
    $creds = Get-Content "azure-credentials.json" | ConvertFrom-Json
    
    # Login with service principal
    Write-Host "Logging into Azure..." -ForegroundColor Blue
    az login --service-principal --username $creds.clientId --password $creds.clientSecret --tenant $creds.tenantId
    az account set --subscription $SubscriptionId
    
    Write-Host "Getting website URLs..." -ForegroundColor Blue
    
    # Get the storage account website URL
    $WebsiteUrl = az storage account show --name $StorageAccount --resource-group $ResourceGroup --subscription $SubscriptionId --query "primaryEndpoints.web" -o tsv
    
    # Get the CDN endpoint URL
    $CdnHostName = az cdn endpoint show --name $CdnEndpoint --profile-name $CdnProfile --resource-group $ResourceGroup --subscription $SubscriptionId --query "hostName" -o tsv
    
    Write-Host ""
    Write-Host "=== WEBSITE DEPLOYMENT INFORMATION ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "Storage URL: $WebsiteUrl" -ForegroundColor Yellow
    Write-Host "CDN Hostname: $CdnHostName" -ForegroundColor Yellow
    Write-Host "CDN URL: https://$CdnHostName" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "=== DNS CONFIGURATION FOR AGILECLOUD.AI ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To make agilecloud.ai work, add these DNS records:" -ForegroundColor White
    Write-Host ""
    Write-Host "1. CNAME record for root domain:" -ForegroundColor Green
    Write-Host "   Name: agilecloud.ai (or root/apex)" -ForegroundColor White
    Write-Host "   Value: $CdnHostName" -ForegroundColor White
    Write-Host ""
    Write-Host "2. CNAME record for www:" -ForegroundColor Green
    Write-Host "   Name: www" -ForegroundColor White
    Write-Host "   Value: $CdnHostName" -ForegroundColor White
    Write-Host ""
    Write-Host "After DNS is configured, run this to add custom domain to CDN:" -ForegroundColor Cyan
    Write-Host "az cdn custom-domain create --endpoint-name $CdnEndpoint --hostname agilecloud.ai --name agilecloud-ai --profile-name $CdnProfile --resource-group $ResourceGroup --subscription $SubscriptionId" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Test the current deployment:" -ForegroundColor Green
    Write-Host "- Storage: $WebsiteUrl" -ForegroundColor White
    Write-Host "- CDN: https://$CdnHostName" -ForegroundColor White
    
} else {
    Write-Host "azure-credentials.json not found!" -ForegroundColor Red
    Write-Host "Please run create-azure-credentials.ps1 first" -ForegroundColor Yellow
}
