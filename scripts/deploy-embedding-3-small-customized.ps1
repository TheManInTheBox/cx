# 🚀 Deploy text-embedding-3-small for CX Language
# Customized deployment script for your Azure resources

# ===============================================
# CONFIGURATION - UPDATE WITH YOUR VALUES
# ===============================================

# Your Azure OpenAI Resources (from your appsettings.json)
$openAIResourceEastUS = "agilai"  # Your East US OpenAI resource name
$openAIResourceEastUS2 = "aaron-md9fkzv3-eastus2"  # Your East US 2 resource name 
$resourceGroup = "ai"  # Your resource group name (based on AI Foundry finding)

# AI Foundry Project (optional - for future use)
$aiFoundryProject = "asdasdasasd"  # Your AI Foundry project name

# Model Configuration
$deploymentName = "text-embedding-3-small"
$modelName = "text-embedding-3-small"
$modelVersion = "1"
$capacity = 120  # Standard TPM

Write-Host "🔧 DEPLOYING text-embedding-3-small MODEL FOR CX LANGUAGE" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Configuration:" -ForegroundColor Yellow
Write-Host "   East US Resource: $openAIResourceEastUS" -ForegroundColor White
Write-Host "   East US 2 Resource: $openAIResourceEastUS2" -ForegroundColor White
Write-Host "   Resource Group: $resourceGroup" -ForegroundColor White
Write-Host "   Model: $modelName v$modelVersion" -ForegroundColor White
Write-Host "   Capacity: $capacity TPM" -ForegroundColor White
Write-Host ""

# ===============================================
# DEPLOYMENT FUNCTIONS
# ===============================================

function Deploy-EmbeddingToResource {
    param(
        [string]$ResourceName,
        [string]$Region,
        [string]$ResourceGroup
    )
    
    Write-Host "🚀 Deploying to $Region ($ResourceName)..." -ForegroundColor Cyan
    
    try {
        # Check if resource exists
        $resource = az cognitiveservices account show --name $ResourceName --resource-group $ResourceGroup 2>$null
        if (-not $resource) {
            Write-Host "❌ Resource $ResourceName not found in $ResourceGroup" -ForegroundColor Red
            return $false
        }
        
        # Create the deployment
        Write-Host "   Creating deployment..." -ForegroundColor Yellow
        $result = az cognitiveservices account deployment create `
            --name $ResourceName `
            --resource-group $ResourceGroup `
            --deployment-name $deploymentName `
            --model-name $modelName `
            --model-version $modelVersion `
            --model-format OpenAI `
            --sku-name "Standard" `
            --sku-capacity $capacity 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Successfully deployed to $Region" -ForegroundColor Green
            
            # Verify deployment
            Write-Host "   Verifying deployment..." -ForegroundColor Yellow
            $deployment = az cognitiveservices account deployment show `
                --name $ResourceName `
                --resource-group $ResourceGroup `
                --deployment-name $deploymentName `
                --output json 2>$null | ConvertFrom-Json
            
            if ($deployment) {
                Write-Host "   📊 Status: $($deployment.properties.provisioningState)" -ForegroundColor Green
                Write-Host "   📈 Capacity: $($deployment.sku.capacity) TPM" -ForegroundColor Green
            }
            Write-Host ""
            return $true
        } else {
            Write-Host "❌ Failed to deploy to $Region" -ForegroundColor Red
            Write-Host "   Error: $result" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Error deploying to $Region`: $_" -ForegroundColor Red
        return $false
    }
}

# ===============================================
# EXECUTE DEPLOYMENTS
# ===============================================

$successCount = 0

# Deploy to East US (if resource name provided)
if ($openAIResourceEastUS -and $openAIResourceEastUS -ne "") {
    $success = Deploy-EmbeddingToResource -ResourceName $openAIResourceEastUS -Region "East US" -ResourceGroup $resourceGroup
    if ($success) { $successCount++ }
}

# Deploy to East US 2 (if resource name provided)
if ($openAIResourceEastUS2 -and $openAIResourceEastUS2 -ne "") {
    $success = Deploy-EmbeddingToResource -ResourceName $openAIResourceEastUS2 -Region "East US 2" -ResourceGroup $resourceGroup
    if ($success) { $successCount++ }
}

# ===============================================
# RESULTS AND NEXT STEPS
# ===============================================

Write-Host "🎉 DEPLOYMENT SUMMARY" -ForegroundColor Green
Write-Host "====================" -ForegroundColor Green
Write-Host "Successful deployments: $successCount" -ForegroundColor White
Write-Host ""

if ($successCount -gt 0) {
    Write-Host "📝 Next Steps:" -ForegroundColor Yellow
    Write-Host "1. ✅ Update your appsettings.json:" -ForegroundColor White
    Write-Host '   "EmbeddingDeploymentName": "text-embedding-3-small"' -ForegroundColor Cyan
    Write-Host ""
    Write-Host "2. ✅ Test the deployment:" -ForegroundColor White
    Write-Host "   dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test_embedding_3_small.cx" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "3. ✅ Run comprehensive vector database tests:" -ForegroundColor White
    Write-Host "   dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_vector_database_test.cx" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🚀 Benefits of text-embedding-3-small:" -ForegroundColor Yellow
    Write-Host "   📈 62% better performance than ada-002" -ForegroundColor White
    Write-Host "   💰 5x cheaper per token" -ForegroundColor White
    Write-Host "   ⚡ Faster inference times" -ForegroundColor White
    Write-Host "   🧠 Superior semantic understanding for AI-native code" -ForegroundColor White
} else {
    Write-Host "❌ No successful deployments" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔧 Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Verify resource names and resource group" -ForegroundColor White
    Write-Host "2. Check Azure CLI authentication: az account show" -ForegroundColor White
    Write-Host "3. Verify subscription has Azure OpenAI access" -ForegroundColor White
    Write-Host "4. Check quota limits in Azure portal" -ForegroundColor White
}

Write-Host ""
Write-Host "📖 For manual deployment, see:" -ForegroundColor Yellow
Write-Host "   scripts/deploy-ai-foundry-embedding.md" -ForegroundColor Cyan
Write-Host "   .github/instructions/azure-openai-deployment.instructions.md" -ForegroundColor Cyan
Write-Host "   .github/instructions/azure-openai-deployment.instructions.md" -ForegroundColor Cyan
