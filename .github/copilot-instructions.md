# CX Language - Autonomous Programming Platform

## Introduction
This document provides development guidance and standards for the CX Language platform. For detailed syntax and code examples, refer to [cx.instructions.md](instructions/cx.instructions.md).

### **Repository Information**
- **GitHub**: [ahebert-lt/cx](https://github.com/ahebert-lt/cx)
- **Current Milestone**: [Azure OpenAI Realtime API v1.0](https://github.com/ahebert-lt/cx/milestone/4)
- **Version**: 1.0.0 (July 2025)

## TOP PRIORITY 100%
Instrument everything, lots of debug traces. Use the cx language to orchestrate a team of agents to assist you in building the CX Language platform. The agents should be able to handle tasks such as code generation, testing, and documentation. You are the manager of the team and should provide clear instructions to the agents. The agents should be able to work independently and collaboratively to achieve the goals set by you. This is how you will build the CX Language platform.

## Development Standards
- ✅ **ALWAYS** use the latest Cx Language features and syntax defined in cx.instructions.md
- ✅ **Production-Ready**: Complete end-to-end implementations with working unified event system, namespace scoping, and interactive CLI
- ✅ **Real Implementation**: Actual event emission/handling, namespace isolation, press-key-to-exit functionality
- ✅ **Complete CX Integration**: Seamless compiler and runtime operation with zero exceptions
- ✅ **Event-Driven Architecture**: Fully operational unified event system with proper handler registration
- ✅ **Interactive CLI**: Working background event processing with user-controlled termination
- ✅ **Clean Examples**: Organized structure with production/core_features/demos/archive folders
- ❌ **NOT Acceptable**: Simulations of any kind, mocks, partial implementations, placeholder code, POCs

### **Core Components**
```
src/CxLanguage.CLI/           → Command-line interface + Azure OpenAI config
src/CxLanguage.Parser/        → ANTLR4 parser (grammar/Cx.g4 = source of truth)
src/CxLanguage.Compiler/      → IL generation with three-pass compilation
src/CxLanguage.Runtime/       → UnifiedEventBus + CxRuntimeHelper with global event system
src/CxLanguage.StandardLibrary/ → 9 AI services via Semantic Kernel 1.26.0
examples/                     → All CX programs and demonstrations
```

### **File Organization**
- **Production applications**: `examples/production/` directory
- **Core feature tests**: `examples/core_features/` directory  
- **Feature demonstrations**: `examples/demos/` directory
- **Static documentation**: `wiki/` directory (timeless reference material only)
- **Archived examples**: `examples/archive/` directory (historical and experimental code)

### **Examples Organization**
- **`examples/production/`** - Production-ready applications demonstrating complete functionality
- **`examples/core_features/`** - Core system demonstrations and feature tests
- **`examples/demos/`** - Feature demonstrations and showcase applications  
- **`examples/archive/`** - Historical examples and experimental code

## Integration Guide

### **Key Integration Points**
- **Service Provider Registration**: `services.AddSingleton<ICxEventBus, CxEventBus>()`
- **Azure OpenAI Integration**: Realtime API events bridge to CX event system
- **Runtime Bridge**: `CxRuntimeHelper.RegisterInstanceEventHandler()` uses ICxEventBus for AI service instances
- **Fallback Pattern**: Falls back to NamespacedEventBusRegistry when ICxEventBus unavailable

## CLI Usage

### **Quick Commands**
```powershell
# Build and test
dotnet build CxLanguage.sln
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/core_features/event_bus_namespace_demo.cx

# GitHub workflow
gh auth switch --user ahebert-lt
gh issue list --repo ahebert-lt/cx --milestone "Azure OpenAI Realtime API v1.0"
```

## Azure Configuration

### **Azure OpenAI Setup**
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4.1-nano",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ApiKey": "your-api-key",
    "ApiVersion": "2024-10-21"
  }
}
```

## Troubleshooting

### Common Issues
- **Event Handling**: If events aren't being received, check namespace scoping and ensure proper wildcard patterns
- **Runtime Errors**: Verify service declarations and ensure proper implementation of event handlers
- **Azure Integration**: Check appsettings.json configuration for correct Azure OpenAI endpoint and API key
- **Compilation Issues**: Ensure correct syntax according to cx.instructions.md guidelines
- **Performance**: For slow response times, verify Azure OpenAI service availability and network connectivity
- **Debugging**: Use extensive logging and debug traces to identify issues in event processing or service interactions
