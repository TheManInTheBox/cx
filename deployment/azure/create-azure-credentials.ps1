# Create Azure Credentials JSON for GitHub Secret
# This script helps you create the AZURE_CREDENTIALS secret for client secret authentication

Write-Host "Creating AZURE_CREDENTIALS JSON for GitHub Actions..." -ForegroundColor Green
Write-Host ""

# Your configuration
$ClientId = "291122ee-4f43-4b21-a337-c4d6e2382c8e"
$TenantId = "7dd8cf8b-3a69-4cb3-96c9-0a9e63fe6127"
$SubscriptionId = "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"

Write-Host "IMPORTANT: You need to create a client secret first!" -ForegroundColor Yellow
Write-Host ""
Write-Host "Steps to create client secret:" -ForegroundColor Cyan
Write-Host "1. Go to Azure Portal -> Azure AD -> App registrations" -ForegroundColor White
Write-Host "2. Find your app: $ClientId" -ForegroundColor White
Write-Host "3. Go to 'Certificates & secrets' -> 'Client secrets'" -ForegroundColor White
Write-Host "4. Click 'New client secret'" -ForegroundColor White
Write-Host "5. Copy the secret VALUE (not the ID)" -ForegroundColor White
Write-Host ""

$ClientSecret = Read-Host "Enter your client secret VALUE"

if ([string]::IsNullOrWhiteSpace($ClientSecret)) {
    Write-Host "Client secret is required!" -ForegroundColor Red
    exit 1
}

# Create the JSON
$AzureCredentials = @{
    clientId = $ClientId
    clientSecret = $ClientSecret
    subscriptionId = $SubscriptionId
    tenantId = $TenantId
} | ConvertTo-Json -Compress

Write-Host ""
Write-Host "AZURE_CREDENTIALS JSON:" -ForegroundColor Green
Write-Host $AzureCredentials -ForegroundColor Yellow
Write-Host ""

# Save to file
$AzureCredentials | Out-File -FilePath "azure-credentials.json" -Encoding UTF8
Write-Host "JSON saved to: azure-credentials.json" -ForegroundColor Green
Write-Host ""

# Try to set GitHub secret if GitHub CLI is available
try {
    gh --version | Out-Null
    Write-Host "Setting GitHub secret..." -ForegroundColor Blue
    
    $AzureCredentials | gh secret set AZURE_CREDENTIALS
    Write-Host "SUCCESS: AZURE_CREDENTIALS secret set!" -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now run the fallback workflow:" -ForegroundColor Blue
    Write-Host "   gh workflow run deploy-website-fallback.yml" -ForegroundColor White
    
} catch {
    Write-Host "GitHub CLI not found. Manual setup required:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Go to: https://github.com/TheManInTheBox/cx/settings/secrets/actions" -ForegroundColor White
    Write-Host "2. Click 'New repository secret'" -ForegroundColor White
    Write-Host "3. Name: AZURE_CREDENTIALS" -ForegroundColor White
    Write-Host "4. Value: Copy the JSON above" -ForegroundColor White
}

Write-Host ""
Write-Host "SECURITY NOTE: Delete azure-credentials.json after use!" -ForegroundColor Red
