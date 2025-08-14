# Setup GitHub Secrets for Azure Federated Identity
# Run this script to automatically add the required secrets to your GitHub repository

Write-Host "🔐 Setting up GitHub secrets for Azure federated identity..." -ForegroundColor Green

# Your Azure configuration
$ClientId = "291122ee-4f43-4b21-a337-c4d6e2382c8e"
$TenantId = "7dd8cf8b-3a69-4cb3-96c9-0a9e63fe6127"
$SubscriptionId = "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"

# Check if GitHub CLI is installed
try {
    gh --version | Out-Null
} catch {
    Write-Host "❌ GitHub CLI is not installed. Please install it first." -ForegroundColor Red
    Write-Host "Visit: https://cli.github.com/" -ForegroundColor Yellow
    exit 1
}

# Check if logged in to GitHub
try {
    gh auth status | Out-Null
} catch {
    Write-Host "🔑 Please login to GitHub CLI:" -ForegroundColor Yellow
    gh auth login
}

Write-Host "📝 Adding GitHub secrets..." -ForegroundColor Blue

# Add the secrets
try {
    Write-Host "   Adding AZURE_CLIENT_ID..." -ForegroundColor Cyan
    $ClientId | gh secret set AZURE_CLIENT_ID
    
    Write-Host "   Adding AZURE_TENANT_ID..." -ForegroundColor Cyan
    $TenantId | gh secret set AZURE_TENANT_ID
    
    Write-Host "   Adding AZURE_SUBSCRIPTION_ID..." -ForegroundColor Cyan
    $SubscriptionId | gh secret set AZURE_SUBSCRIPTION_ID
    
    Write-Host "✅ All GitHub secrets have been configured successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 You can now run the deployment workflow:" -ForegroundColor Blue
    Write-Host "   gh workflow run deploy-website.yml" -ForegroundColor White
    Write-Host ""
    Write-Host "🌐 Or view the workflow status:" -ForegroundColor Blue
    Write-Host "   gh run list --workflow='deploy-website.yml'" -ForegroundColor White
    
} catch {
    Write-Host "❌ Failed to set GitHub secrets. Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Manual setup instructions:" -ForegroundColor Yellow
    Write-Host "1. Go to: https://github.com/TheManInTheBox/cx/settings/secrets/actions" -ForegroundColor White
    Write-Host "2. Add these three secrets:" -ForegroundColor White
    Write-Host "   - AZURE_CLIENT_ID: $ClientId" -ForegroundColor Cyan
    Write-Host "   - AZURE_TENANT_ID: $TenantId" -ForegroundColor Cyan
    Write-Host "   - AZURE_SUBSCRIPTION_ID: $SubscriptionId" -ForegroundColor Cyan
}
