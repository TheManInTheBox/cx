# Azure OpenAI Multi-Service Configuration Guide

## Overview

The CX Language platform now supports **separate Azure OpenAI endpoints and API keys** for each service type. This allows you to:

- **Optimize costs** by using different Azure resources for different model types
- **Scale independently** with separate rate limits and quotas per service
- **Improve security** with service-specific access controls
- **Track usage** with granular billing per service type

## Configuration Structure

### New Per-Service Configuration

```json
{
  "AzureOpenAI": {
    "Chat": {
      "Endpoint": "https://my-chat-resource.openai.azure.com/",
      "DeploymentName": "gpt-4.1-mini",
      "ApiKey": "YOUR_CHAT_API_KEY",
      "ApiVersion": "2024-12-17"
    },
    "Embedding": {
      "Endpoint": "https://my-embedding-resource.openai.azure.com/",
      "DeploymentName": "text-embedding-3-small",
      "ApiKey": "YOUR_EMBEDDING_API_KEY",
      "ApiVersion": "2024-12-17"
    },
    "Image": {
      "Endpoint": "https://my-image-resource.openai.azure.com/",
      "DeploymentName": "dall-e-3",
      "ApiKey": "YOUR_IMAGE_API_KEY",
      "ApiVersion": "2024-12-17"
    },
    "Realtime": {
      "Endpoint": "wss://my-realtime-resource.openai.azure.com",
      "DeploymentName": "gpt-4o-mini-realtime-preview",
      "ApiKey": "YOUR_REALTIME_API_KEY",
      "ApiVersion": "2024-12-17"
    }
  }
}
```

### Backward Compatibility

The system supports **three configuration levels** with automatic fallback:

1. **New Per-Service Config** (Preferred) - Separate config for each service
2. **Legacy Section Config** - Use `Legacy` section with old format
3. **Root-Level Config** - Original flat configuration structure

#### Legacy Section Configuration
```json
{
  "AzureOpenAI": {
    "Legacy": {
      "Endpoint": "https://my-resource.openai.azure.com/",
      "DeploymentName": "gpt-4.1-mini",
      "EmbeddingDeploymentName": "text-embedding-3-small",
      "ImageDeploymentName": "dall-e-3",
      "RealtimeEndpoint": "wss://my-resource.openai.azure.com",
      "RealtimeDeploymentName": "gpt-4o-mini-realtime-preview",
      "ApiKey": "YOUR_API_KEY",
      "ApiVersion": "2024-12-17"
    }
  }
}
```

#### Root-Level Legacy Configuration
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://my-resource.openai.azure.com/",
    "DeploymentName": "gpt-4.1-mini",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ImageDeploymentName": "dall-e-3",
    "RealtimeEndpoint": "wss://my-resource.openai.azure.com",
    "RealtimeDeploymentName": "gpt-4o-mini-realtime-preview",
    "ApiKey": "YOUR_API_KEY",
    "ApiVersion": "2024-12-17"
  }
}
```

## Service Types

### Chat Service
- **Purpose**: Text completion, conversation, cognitive services (`think`, `learn`, `is`, `not`, `iam`)
- **Models**: GPT-4, GPT-4 Turbo, GPT-3.5
- **Usage**: All CX Language cognitive operations

### Embedding Service  
- **Purpose**: Text embeddings for search and similarity
- **Models**: text-embedding-3-small, text-embedding-3-large
- **Usage**: Vector operations, semantic search

### Image Service
- **Purpose**: Image generation and processing
- **Models**: DALL-E 3, DALL-E 2
- **Usage**: Image creation from text descriptions

### Realtime Service
- **Purpose**: Real-time voice processing and WebSocket communication
- **Models**: gpt-4o-mini-realtime-preview
- **Usage**: Voice input/output, real-time conversation
- **Special Note**: Uses WebSocket endpoint (`wss://`)

## Configuration Resolution

The system uses the following priority order:

1. **Per-Service Config**: `AzureOpenAI:Chat:Endpoint` and `AzureOpenAI:Chat:ApiKey`
2. **Legacy Section**: `AzureOpenAI:Legacy:Endpoint` and `AzureOpenAI:Legacy:ApiKey`
3. **Root Level**: `AzureOpenAI:Endpoint` and `AzureOpenAI:ApiKey`

Each service (Chat, Embedding, Image, Realtime) follows this same fallback pattern.

## Migration Guide

### From Single Resource to Multiple Resources

1. **Identify Current Usage**: Review which services your application uses
2. **Deploy Separate Resources**: Create dedicated Azure OpenAI resources for each service type
3. **Update Configuration**: Use the new per-service configuration structure
4. **Test Gradually**: Migrate one service at a time for safety

### Example Migration

**Before** (Single Resource):
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://my-single-resource.openai.azure.com/",
    "ApiKey": "single-api-key",
    "DeploymentName": "gpt-4.1-mini"
  }
}
```

**After** (Multiple Resources):
```json
{
  "AzureOpenAI": {
    "Chat": {
      "Endpoint": "https://my-chat-resource.openai.azure.com/",
      "ApiKey": "chat-api-key",
      "DeploymentName": "gpt-4.1-mini"
    },
    "Realtime": {
      "Endpoint": "wss://my-realtime-resource.openai.azure.com",
      "ApiKey": "realtime-api-key", 
      "DeploymentName": "gpt-4o-mini-realtime-preview"
    }
  }
}
```

## Benefits

### Cost Optimization
- **Pay per service**: Only pay for the services you actually use
- **Regional pricing**: Use cheaper regions for different service types
- **Resource sizing**: Right-size each resource for its specific workload

### Performance
- **Dedicated quotas**: Separate rate limits prevent one service from affecting others
- **Regional distribution**: Place resources closer to users for better latency
- **Specialized scaling**: Scale each service independently based on demand

### Security
- **Principle of least privilege**: Each service only accesses what it needs
- **Isolated keys**: Compromise of one key doesn't affect other services
- **Audit trails**: Clear separation of access logs per service

### Operational
- **Clear monitoring**: Service-specific metrics and alerts
- **Easier troubleshooting**: Issues isolated to specific services
- **Flexible deployment**: Update one service without affecting others

## Testing

Use the included test file to verify your configuration:

```bash
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/azure_multi_service_config_test.cx
```

This test will verify that each service type (Chat, Embedding, Realtime) can connect using their dedicated configuration.

## Troubleshooting

### Common Issues

1. **Missing Configuration**: Ensure at least one configuration level is properly set
2. **WebSocket Endpoints**: Realtime service requires `wss://` protocol
3. **API Version Compatibility**: Use `2024-12-17` for realtime services
4. **Deployment Names**: Verify deployment names match your Azure resource

### Debug Logging

Enable detailed logging to see which configuration is being used:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "CxLanguage.StandardLibrary": "Debug"
    }
  }
}
```

Look for these log messages:
- `🔗 Chat Endpoint: https://...`
- `📦 Chat Deployment: gpt-4.1-mini`  
- `🔗 Realtime Endpoint: wss://...`

## Support

For issues with multi-service configuration:

1. Check the [Migration Guide](#migration-guide) above
2. Verify your `appsettings.local.json` against the examples
3. Test with the provided test file
4. Enable debug logging to see configuration resolution
