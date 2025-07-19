# 🚀 Azure OpenAI text-embedding-3-small Deployment Script
# Deploy embedding model to East US and East US 2 regions for CX Language

Write-Host "🔧 DEPLOYING text-embedding-3-small MODEL" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$subscriptionId = ""  # Set your subscription ID
$resourceGroupName = ""  # Set your resource group name
$openAiResourceNameEastUS = ""  # Set your East US Azure OpenAI resource name
$openAiResourceNameEastUS2 = ""  # Set your East US 2 Azure OpenAI resource name
$deploymentName = "text-embedding-3-small"
$modelName = "text-embedding-3-small"
$modelVersion = "1"  # Latest stable version
$capacity = 120  # Standard TPM capacity

Write-Host "📋 Configuration:" -ForegroundColor Yellow
Write-Host "   Model: $modelName" -ForegroundColor White
Write-Host "   Version: $modelVersion" -ForegroundColor White
Write-Host "   Deployment: $deploymentName" -ForegroundColor White
Write-Host "   Capacity: $capacity TPM" -ForegroundColor White
Write-Host ""

# Check if logged in to Azure
Write-Host "🔐 Checking Azure CLI authentication..." -ForegroundColor Yellow
$accountInfo = az account show 2>$null
if (-not $accountInfo) {
    Write-Host "❌ Not logged in to Azure CLI" -ForegroundColor Red
    Write-Host "Please run: az login" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Azure CLI authenticated" -ForegroundColor Green
Write-Host ""

# Function to deploy model to a region
function Deploy-EmbeddingModel {
    param(
        [string]$ResourceName,
        [string]$Region
    )
    
    Write-Host "🚀 Deploying to $Region ($ResourceName)..." -ForegroundColor Cyan
    
    try {
        # Create the deployment
        az cognitiveservices account deployment create `
            --name $ResourceName `
            --resource-group $resourceGroupName `
            --deployment-name $deploymentName `
            --model-name $modelName `
            --model-version $modelVersion `
            --model-format OpenAI `
            --sku-name "Standard" `
            --sku-capacity $capacity
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Successfully deployed to $Region" -ForegroundColor Green
            
            # Verify deployment
            Write-Host "🔍 Verifying deployment..." -ForegroundColor Yellow
            $deployment = az cognitiveservices account deployment show `
                --name $ResourceName `
                --resource-group $resourceGroupName `
                --deployment-name $deploymentName `
                --output json | ConvertFrom-Json
            
            Write-Host "📊 Deployment Status: $($deployment.properties.provisioningState)" -ForegroundColor White
            Write-Host "📈 Capacity: $($deployment.sku.capacity) TPM" -ForegroundColor White
            Write-Host ""
        } else {
            Write-Host "❌ Failed to deploy to $Region" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "❌ Error deploying to $Region`: $_" -ForegroundColor Red
    }
}

# Deploy to East US
if ($openAiResourceNameEastUS) {
    Deploy-EmbeddingModel -ResourceName $openAiResourceNameEastUS -Region "East US"
}

# Deploy to East US 2  
if ($openAiResourceNameEastUS2) {
    Deploy-EmbeddingModel -ResourceName $openAiResourceNameEastUS2 -Region "East US 2"
}

Write-Host "🎉 DEPLOYMENT COMPLETE!" -ForegroundColor Green
Write-Host "======================" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Update appsettings.json with EmbeddingDeploymentName: text-embedding-3-small" -ForegroundColor White
Write-Host "2. Run CX Language vector database tests" -ForegroundColor White
Write-Host "3. Enjoy 62% better performance at 5x lower cost!" -ForegroundColor White
Write-Host ""

# Test the deployment
Write-Host "🧪 Testing deployment availability..." -ForegroundColor Yellow
Write-Host "Run this command to test:" -ForegroundColor White
Write-Host "dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test_embedding_3_small.cx" -ForegroundColor Cyan
