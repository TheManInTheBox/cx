# CX Language Installation Guide

**Version:** v1.0.0-beta  
**Date:** July 19, 2025  
**Platform:** Cross-platform (.NET 8)  

## 🎯 **Quick Installation (5 Minutes)**

### Prerequisites
- **Operating System**: Windows 10/11, macOS, or Linux
- **.NET 8 SDK**: [Download Here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git**: [Download Here](https://git-scm.com/downloads)
- **Azure OpenAI Access**: Required for AI features

### Installation Steps
```bash
# 1. Clone the repository
git clone https://github.com/ahebert-lt/cx.git
cd cx

# 2. Build the language
dotnet build CxLanguage.sln --configuration Release

# 3. Test installation (core features)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_working_demo.cx

# 4. Verify success (should output AI language demo)
```

**✅ Expected Output**: Complete demo showcasing variables, functions, classes, AI services, and vector database integration.

## 🔧 **Azure OpenAI Configuration**

### Step 1: Create Azure OpenAI Resource
1. Go to [Azure Portal](https://portal.azure.com)
2. Create new **Azure OpenAI** resource
3. Note the **Endpoint** and **API Key**
4. Deploy required models (see Model Deployment section)

### Step 2: Configure CX Language
Create `appsettings.json` in the cx folder:
```json
{
  "AzureOpenAI": {
    "ApiKey": "your-api-key-here",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4-turbo",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ImageDeploymentName": "dall-e-3",
    "ApiVersion": "2024-10-21"
  }
}
```

### Step 3: Test AI Features
```bash
# Test complete AI integration
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_ai_mp3_demo.cx
```

**✅ Expected Output**: AI text generation, image creation, embeddings, vector database, and MP3 audio playback.

## 🚀 **Model Deployment (Required)**

### Deploy text-embedding-3-small (Recommended)
```bash
# Using Azure CLI
az cognitiveservices account deployment create \
    --name <your-openai-resource> \
    --resource-group <your-resource-group> \
    --deployment-name "text-embedding-3-small" \
    --model-name "text-embedding-3-small" \
    --model-version "1" \
    --model-format OpenAI \
    --sku-name "Standard" \
    --sku-capacity 120
```

### Or Use Provided Script
```bash
# PowerShell (Windows)
.\scripts\deploy-embedding-3-small-customized.ps1

# The script will:
# ✅ Deploy text-embedding-3-small to your Azure resources
# ✅ Verify deployment status
# ✅ Provide testing instructions
```

### Required Models
| Model | Purpose | Deployment Name | Required |
|-------|---------|-----------------|----------|
| `gpt-4-turbo` or `gpt-4.1-nano` | Text generation, chat | `gpt-4-turbo` | ✅ Yes |
| `text-embedding-3-small` | Vector operations | `text-embedding-3-small` | ✅ Yes |
| `dall-e-3` | Image generation | `dall-e-3` | ⚠️ Optional |

## 🏗️ **Advanced Configuration**

### Production Multi-Service Setup
For high availability and load distribution:
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
          "TextGeneration": "gpt-4.1-nano",
          "ImageGeneration": "dall-e-3"
        }
      }
    ]
  }
}
```

### Environment Variables (Alternative)
```bash
# Set environment variables instead of appsettings.json
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4-turbo"
export AZURE_OPENAI_EMBEDDING_DEPLOYMENT_NAME="text-embedding-3-small"
```

## 🛠️ **Development Environment Setup**

### Visual Studio 2022 (Recommended)
1. Install **Visual Studio 2022** with .NET 8 workload
2. Open `CxLanguage.sln`
3. Set **CxLanguage.CLI** as startup project
4. Configure program arguments: `run examples/comprehensive_working_demo.cx`
5. Press **F5** to run

### VS Code Setup
1. Install **VS Code** with C# extension
2. Open cx folder in VS Code
3. Install recommended extensions
4. Use integrated terminal for commands

### Command Line Development
```bash
# Quick development workflow
cd c:\Users\aaron\Source\cx

# Edit .cx files in examples/ folder
# Then test immediately:
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/myprogram.cx
```

## 📦 **Package Installation (Future)**

### NuGet Package (Coming Soon)
```bash
# Install CX Language CLI globally (planned)
dotnet tool install --global CxLanguage.CLI --version 1.0.0-beta

# Use anywhere on system
cx run myprogram.cx
```

### Docker Container (Planned)
```bash
# Run CX Language in container (planned)
docker run -it cxlanguage/cx:1.0.0-beta
```

## 🧪 **Installation Verification**

### Core Language Test
```cx
// test-installation.cx
print("=== CX Language Installation Test ===");

// Variables and basic operations
var language = "CX";
var version = "1.0.0-beta";
var ready = true;

print("Language: " + language);
print("Version: " + version); 
print("Ready: " + ready);

// Functions
function testFunction(message)
{
    return "Function working: " + message;
}

var result = testFunction("Hello CX!");
print(result);

// Classes
class TestAgent
{
    name: string;
    
    constructor(agentName)
    {
        this.name = agentName;
    }
    
    function greet()
    {
        return "Hello from " + this.name;
    }
}

var agent = new TestAgent("Installation Test");
print(agent.greet());

// Exception handling
try
{
    print("Exception handling working");
}
catch (error)
{
    print("Error: " + error);
}

print("✅ Core language installation successful!");
```

### AI Services Test
```cx
// test-ai-services.cx
using textGen from "Cx.AI.TextGeneration";
using chatBot from "Cx.AI.ChatCompletion";

print("=== AI Services Test ===");

try
{
    // Test text generation
    var greeting = textGen.GenerateAsync("Say hello to CX Language users", {
        temperature: 0.7,
        maxTokens: 50
    });
    print("AI Greeting: " + greeting);
    
    // Test chat completion
    var response = chatBot.CompleteAsync(
        "Is CX Language working correctly?",
        {
            systemMessage: "You are a helpful AI assistant."
        }
    );
    print("AI Response: " + response);
    
    print("✅ AI services installation successful!");
}
catch (error)
{
    print("❌ AI services error: " + error);
    print("Check Azure OpenAI configuration in appsettings.json");
}
```

### Run Verification Tests
```bash
# Test core language features
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run test-installation.cx

# Test AI services (requires Azure OpenAI config)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run test-ai-services.cx
```

## 🐛 **Troubleshooting**

### Common Issues

#### Build Errors
```bash
❌ Error: Package restore failed
✅ Solution: dotnet restore CxLanguage.sln
```

#### Missing .NET 8
```bash
❌ Error: The specified SDK 'Microsoft.NET.Sdk' was not found
✅ Solution: Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
```

#### Azure OpenAI Connection Issues
```bash
❌ Error: Unauthorized (401) or Not Found (404)
✅ Solutions:
   1. Check API key in appsettings.json
   2. Verify endpoint URL format
   3. Confirm model deployments exist
   4. Check Azure resource access permissions
```

#### Model Not Found
```bash
❌ Error: The API deployment for this resource does not exist
✅ Solutions:
   1. Deploy text-embedding-3-small model
   2. Use scripts/deploy-embedding-3-small-customized.ps1
   3. Verify deployment names match configuration
```

### Performance Issues

#### Slow Compilation
```bash
# Clean and rebuild
dotnet clean
dotnet build --configuration Release
```

#### AI Service Timeouts
```json
{
  "AzureOpenAI": {
    "RequestTimeout": 60,
    "MaxRetries": 3,
    "RetryDelay": 2
  }
}
```

### Getting Help

1. **Check Documentation**: README.md and examples
2. **Review Examples**: Working code in examples/ folder
3. **GitHub Issues**: Report bugs and get help
4. **Azure Support**: For Azure OpenAI service issues

## 📋 **System Requirements**

### Minimum Requirements
- **OS**: Windows 10, macOS 10.15, Ubuntu 18.04 or newer
- **RAM**: 4GB minimum, 8GB recommended
- **.NET**: .NET 8 SDK required
- **Storage**: 500MB for source + dependencies
- **Network**: Internet connection for Azure OpenAI services

### Recommended Configuration
- **OS**: Windows 11, macOS 13, Ubuntu 22.04
- **RAM**: 16GB for large AI workloads
- **CPU**: Multi-core processor for parallel AI operations
- **Storage**: SSD for faster compilation
- **Network**: High-speed connection for AI service calls

### Azure Requirements
- **Azure Subscription**: Active with Azure OpenAI access
- **Resource Quotas**: Sufficient TPM (Tokens Per Minute) allocation
- **Regional Access**: Access to regions with required models
- **Cost Management**: Monitor and control API usage costs

## 🚀 **Next Steps After Installation**

### 1. Learn CX Language
- **Tutorial**: Follow examples from simple to complex
- **Language Reference**: Complete syntax documentation
- **Best Practices**: Performance optimization and patterns

### 2. Build AI Applications
- **Text Generation**: Creative content and technical writing
- **Conversational AI**: Chatbots and virtual assistants
- **Image Generation**: Visual content creation with DALL-E 3
- **Vector Search**: Semantic search and RAG applications

### 3. Deploy to Production
- **Configuration**: Multi-service setup for reliability
- **Monitoring**: Application Insights integration
- **Security**: API key management and access control
- **Scaling**: Performance optimization and cost management

### 4. Join the Community
- **GitHub**: Contribute code, report issues, request features
- **Discussions**: Q&A and knowledge sharing
- **Roadmap**: Influence future development priorities
- **Examples**: Share your CX Language applications

## 🎉 **Welcome to CX Language!**

You're now ready to build the future of AI-native applications with CX Language. Start with the comprehensive demo, explore the examples, and begin creating your own AI-powered programs.

**Happy coding with CX Language - where AI meets programming!** 🚀

---

## 📞 **Support Resources**

- **Documentation**: Complete guides in repository
- **Examples**: Working demos and tutorials
- **GitHub Issues**: Bug reports and feature requests
- **Discussions**: Community Q&A and support
- **Azure Documentation**: Azure OpenAI service guides

---

**Installation Guide Version**: 1.0  
**Last Updated**: July 19, 2025  
**CX Language Version**: v1.0.0-beta
