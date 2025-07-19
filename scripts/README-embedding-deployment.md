# 🚀 Azure OpenAI Model Deployment Commands for CX Language

## Quick Deployment Commands

### 1. List your Azure OpenAI resources
```bash
# Find your Azure OpenAI resources
az cognitiveservices account list --query "[?kind=='OpenAI'].{name:name, resourceGroup:resourceGroup, location:location}" --output table
```

### 2. Deploy text-embedding-3-small to East US
```bash
# Replace <resource-name> and <resource-group> with your values
az cognitiveservices account deployment create \
    --name <resource-name> \
    --resource-group <resource-group> \
    --deployment-name "text-embedding-3-small" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120
```

### 3. Deploy text-embedding-3-small to East US 2
```bash
# Replace <resource-name-eastus2> and <resource-group> with your values
az cognitiveservices account deployment create \
    --name <resource-name-eastus2> \
    --resource-group <resource-group> \
    --deployment-name "text-embedding-3-small" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120
```

### 4. Verify deployments
```bash
# Check East US deployment
az cognitiveservices account deployment show \
    --name <resource-name> \
    --resource-group <resource-group> \
    --deployment-name "text-embedding-3-small"

# Check East US 2 deployment
az cognitiveservices account deployment show \
    --name <resource-name-eastus2> \
    --resource-group <resource-group> \
    --deployment-name "text-embedding-3-small"
```

## Example with Actual Commands

Based on your current configuration (update with your actual resource names):

```bash
# 1. Login to Azure
az login

# 2. Set subscription (if needed)
# az account set --subscription "your-subscription-id"

# 3. List OpenAI resources to find exact names
az cognitiveservices account list --query "[?kind=='OpenAI']" --output table

# 4. Deploy to your East US resource
az cognitiveservices account deployment create \
    --name "your-openai-eastus" \
    --resource-group "your-resource-group" \
    --deployment-name "text-embedding-3-small" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120

# 5. Deploy to your East US 2 resource
az cognitiveservices account deployment create \
    --name "your-openai-eastus2" \
    --resource-group "your-resource-group" \
    --deployment-name "text-embedding-3-small" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120
```

## Configuration Update

After successful deployment, update your `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4.1-nano",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ImageDeploymentName": "dall-e-3",
    "ApiKey": "your-api-key-here",
    "ApiVersion": "2024-10-21"
  }
}
```

## Testing Commands

After deployment, test with CX Language:

```bash
# Test the embedding upgrade
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test_embedding_3_small.cx

# Test comprehensive vector database
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_vector_database_test.cx
```

## Troubleshooting

### Common Issues:

1. **Quota exceeded**: Check your subscription quota
   ```bash
   az cognitiveservices account list-usage --name <resource-name> --resource-group <resource-group>
   ```

2. **Model not available**: Check available models
   ```bash
   az cognitiveservices account list-models --name <resource-name> --resource-group <resource-group>
   ```

3. **Deployment already exists**: Delete existing deployment
   ```bash
   az cognitiveservices account deployment delete \
       --name <resource-name> \
       --resource-group <resource-group> \
       --deployment-name "text-embedding-3-small"
   ```

## Model Specifications

- **Model**: `text-embedding-3-small`
- **Version**: `1` (latest stable)
- **Dimensions**: 1536
- **Max Input Tokens**: 8191
- **Performance**: ~62% better than ada-002
- **Cost**: ~5x cheaper than ada-002
- **TPM Capacity**: 120 (standard deployment)
