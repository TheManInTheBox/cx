# Fix Azure Service Principal Permissions
# Run this script to grant the required permissions to your service principal

Write-Host "Fixing Azure service principal permissions..." -ForegroundColor Green

$ServicePrincipalId = "291122ee-4f43-4b21-a337-c4d6e2382c8e"
$SubscriptionId = "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"

# Check if Azure CLI is installed
try {
    az --version | Out-Null
    Write-Host "Azure CLI found" -ForegroundColor Green
} catch {
    Write-Host "Azure CLI is not installed. Please install it first." -ForegroundColor Red
    Write-Host "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Check login status
Write-Host "Checking Azure login status..." -ForegroundColor Blue
try {
    az account show | Out-Null
    Write-Host "Azure CLI authenticated" -ForegroundColor Green
} catch {
    Write-Host "Please login to Azure CLI:" -ForegroundColor Yellow
    az login
}

# Set subscription
Write-Host "Setting Azure subscription..." -ForegroundColor Blue
az account set --subscription $SubscriptionId

# Check current role assignments
Write-Host "Checking current role assignments..." -ForegroundColor Blue
$currentRoles = az role assignment list --assignee $ServicePrincipalId --subscription $SubscriptionId --query "[].roleDefinitionName" -o tsv

if ($currentRoles -contains "Contributor") {
    Write-Host "Service principal already has Contributor role" -ForegroundColor Green
} else {
    Write-Host "Granting Contributor role to service principal..." -ForegroundColor Blue
    try {
        az role assignment create `
          --assignee $ServicePrincipalId `
          --role Contributor `
          --subscription $SubscriptionId
        
        Write-Host "SUCCESS: Contributor role granted!" -ForegroundColor Green
    } catch {
        Write-Host "Failed to grant role. Error: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "You may need to have Owner or User Access Administrator permissions" -ForegroundColor Yellow
        Write-Host "to grant roles to service principals." -ForegroundColor Yellow
    }
}

# Verify the assignment
Write-Host "Verifying role assignments..." -ForegroundColor Blue
az role assignment list --assignee $ServicePrincipalId --subscription $SubscriptionId --output table

Write-Host ""
Write-Host "Permissions setup complete!" -ForegroundColor Green
Write-Host "You can now retry the GitHub Actions workflow:" -ForegroundColor Blue
Write-Host "   gh workflow run deploy-website.yml" -ForegroundColor White
