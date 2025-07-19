---
applyTo: "**"
description: "Azure OpenAI Model Deployment Instructions for CX Language"
---

# Azure OpenAI Model Deployment Instructions

## Overview
This document provides comprehensive instructions for deploying Azure OpenAI models for the CX Language, specifically focusing on embedding models and multi-region deployments.

## Prerequisites

### Required Tools
- Azure CLI installed and configured
- Azure subscription with OpenAI service access
- Appropriate resource group and Azure OpenAI resources in target regions

### Authentication
```bash
# Login to Azure
az login

# Verify authentication
az account show

# Set subscription (if needed)
az account set --subscription "your-subscription-id"
```

## Model Deployment Process

### Step 1: Resource Discovery
```bash
# List all Azure OpenAI resources
az cognitiveservices account list --query "[?kind=='OpenAI'].{name:name, resourceGroup:resourceGroup, location:location}" --output table

# Check existing deployments
az cognitiveservices account deployment list --name <resource-name> --resource-group <resource-group> --output table
```

### Step 2: Embedding Model Deployment

#### For CX Language Vector Database Integration:
```bash
# Deploy text-embedding-3-small (recommended)
az cognitiveservices account deployment create \
    --name <azure-openai-resource-name> \
    --resource-group <resource-group-name> \
    --deployment-name "text-embedding-3-small" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120
```

#### Alternative Models (if needed):
```bash
# Deploy text-embedding-ada-002 (legacy)
az cognitiveservices account deployment create \
    --name <azure-openai-resource-name> \
    --resource-group <resource-group-name> \
    --deployment-name "text-embedding-ada-002" \
    --model-name "text-embedding-ada-002" \
    --model-version "2" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120
```

### Step 3: Multi-Region Deployment

For high availability and load distribution across East US and East US 2:

```bash
# East US deployment
az cognitiveservices account deployment create \
    --name <eastus-resource-name> \
    --resource-group <resource-group> \
    --deployment-name "text-embedding-3-small" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120

# East US 2 deployment
az cognitiveservices account deployment create \
    --name <eastus2-resource-name> \
    --resource-group <resource-group> \
    --deployment-name "text-embedding-3-small" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120
```

### Step 4: Verification
```bash
# Verify deployment status
az cognitiveservices account deployment show \
    --name <resource-name> \
    --resource-group <resource-group> \
    --deployment-name "text-embedding-3-small" \
    --query "{name:name, status:properties.provisioningState, model:properties.model.name, version:properties.model.version, capacity:sku.capacity}"

# Test endpoint connectivity
curl -X POST "https://<your-resource>.openai.azure.com/openai/deployments/text-embedding-3-small/embeddings?api-version=2024-08-01-preview" \
     -H "api-key: <your-api-key>" \
     -H "Content-Type: application/json" \
     -d '{"input": "test", "encoding_format": "float"}'
```

## CX Language Configuration

### Update appsettings.json
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

### Multi-Service Configuration (appsettings.multiservice.example.json)
```json
{
  "AzureOpenAI": {
    "DefaultService": "EastUS2",
    "ServiceSelection": {
      "TextEmbedding": "EastUS"
    },
    "Services": [
      {
        "Name": "EastUS",
        "Endpoint": "https://eastus-resource.openai.azure.com/",
        "ApiKey": "eastus-api-key",
        "Models": {
          "TextEmbedding": "text-embedding-3-small"
        }
      },
      {
        "Name": "EastUS2", 
        "Endpoint": "https://eastus2-resource.openai.azure.com/",
        "ApiKey": "eastus2-api-key",
        "Models": {
          "ChatCompletion": "gpt-4.1-nano",
          "TextGeneration": "gpt-4.1-nano"
        }
      }
    ]
  }
}
```

## Testing and Validation

### CX Language Tests
```bash
# Test embedding model upgrade
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test_embedding_3_small.cx

# Test comprehensive vector database
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_vector_database_test.cx

# Test multi-service configuration
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_ai_mp3_demo.cx
```

### Performance Validation
Expected results after successful deployment:
- ✅ Compilation: ~30-50ms
- ✅ Embedding generation: 1536-dimensional vectors
- ✅ Vector database ingestion: Sub-second response
- ✅ Semantic search: Sub-7 second query times
- ✅ No HTTP 404 errors

## Troubleshooting Guide

### Common Issues and Solutions

#### 1. Deployment Not Found (HTTP 404)
```bash
# Check if deployment exists
az cognitiveservices account deployment list --name <resource-name> --resource-group <resource-group>

# Recreate deployment if missing
az cognitiveservices account deployment create [parameters...]
```

#### 2. Quota Exceeded
```bash
# Check usage and limits
az cognitiveservices account list-usage --name <resource-name> --resource-group <resource-group>

# Request quota increase through Azure portal
```

#### 3. Model Not Available
```bash
# List available models
az cognitiveservices account list-models --name <resource-name> --resource-group <resource-group> --query "[?name=='text-embedding-3-small']"

# Check regional availability
az cognitiveservices model list --location eastus --query "[?name=='text-embedding-3-small']"
```

#### 4. API Version Compatibility
- Use `2024-08-01-preview` or later for `text-embedding-3-small`
- Use `2024-10-21` for general chat completion models
- Update CX Language configuration accordingly

## Best Practices

### Capacity Planning
- **Standard TPM**: 120 (default)
- **High-volume**: 240-480 TPM
- **Enterprise**: 1000+ TPM with dedicated capacity

### Regional Strategy
- **Primary**: East US 2 (latest features)
- **Secondary**: East US (fallback)
- **Load balancing**: Multi-service configuration in CX Language

### Cost Optimization
- Use `text-embedding-3-small` instead of `ada-002` (5x cheaper)
- Monitor usage with Azure Cost Management
- Set up alerts for unexpected usage spikes

### Security
- Use Azure Key Vault for API keys in production
- Implement network security groups for resource access
- Enable diagnostic logging for audit trails

## Model Specifications Reference

### text-embedding-3-small
- **Dimensions**: 1536
- **Max Input**: 8191 tokens
- **Performance**: ~62% better than ada-002
- **Cost**: ~5x cheaper than ada-002
- **Recommended For**: CX Language vector database, semantic search

### text-embedding-ada-002 (Legacy)
- **Dimensions**: 1536
- **Max Input**: 8191 tokens
- **Performance**: Baseline
- **Cost**: Higher than 3-small
- **Status**: Superseded by 3-small

## Automation Scripts

### PowerShell Script
See: `scripts/deploy-embedding-model.ps1`

### Bash Script
```bash
#!/bin/bash
# deploy-embedding-model.sh
RESOURCE_NAME="your-openai-resource"
RESOURCE_GROUP="your-resource-group"
DEPLOYMENT_NAME="text-embedding-3-small"

az cognitiveservices account deployment create \
    --name "$RESOURCE_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --deployment-name "$DEPLOYMENT_NAME" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120
```

## Future Considerations

### Upcoming Models
- Monitor Azure AI Foundry for new embedding models
- Test performance improvements with CX Language benchmarks
- Update deployment scripts for new model versions

### Scaling Strategy
- Implement auto-scaling based on TPM usage
- Consider Azure AI Foundry managed endpoints
- Plan for multi-tenant deployments

---

**Last Updated**: July 2025  
**CX Language Version**: Phase 4 Complete  
**Azure OpenAI API Version**: 2024-10-21
