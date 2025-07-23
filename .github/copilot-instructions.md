# CX Language - Autonomous Programming Platform

## Introduction
This document provides development guidance and standards for the CX Language platform. For detailed syntax and code examples, refer to [cx.instructions.md](instructions/cx.instructions.md).

### **Repository Information**
- **GitHub**: [ahebert-lt/cx](https://github.com/ahebert-lt/cx)
- **Current Milestone**: [Azure OpenAI Realtime API v1.0](https://github.com/ahebert-lt/cx/milestone/4)
- **Version**: 1.0.0 (July 2025)

## TOP PRIORITY 100%
- ABSOLUTLY NO SIMULATIONS!!!! WORKING CODE ONLY!!!!
- PRINTED OUTPUT DOES NOT CONFIRM FUNCTIONALITY, ONLY DEBUG OUTPUT DOES
- TOP CONCERN: Consider the implications of every architectural and implementation decision on production reliability, scalability, and maintainability.
- Always check for existing scaffolding before starting a new feature.
- Instrument everything, lots of debug and runtime trace. Trace cx source code line to executing IL is a must. 
- Use the cx language to orchestrate a team of agents to assist you in building the Cx Language platform. 
- The agents should be able to handle tasks such as code generation, testing, and documentation. 
- You are the manager of the team and should provide clear instructions to the agents. 
- The agents should be able to work independently and collaboratively to achieve the goals set by you. 
- This is how we will build the Cx Language platform.

## Development Standards
- ✅ **ALWAYS** use the latest Cx Language features and syntax defined in cx.instructions.md
- ✅ **Production-Ready**: Complete end-to-end implementations with working Aura Cognitive Framework, namespace scoping, and interactive CLI
- ✅ **Real Implementation**: Actual event emission/handling, namespace isolation, press-key-to-exit functionality
- ✅ **Complete CX Integration**: Seamless compiler and runtime operation with zero exceptions
- ✅ **Event-Driven Architecture**: Fully operational Aura Cognitive Framework with proper handler registration
- ✅ **Interactive CLI**: Working background event processing with user-controlled termination
- ✅ **Enhanced Handlers Pattern**: Complete implementation with custom payload support `handlers: [ analysis.complete { option: "value" } ]`
- ✅ **Automatic Object Serialization**: CX objects display as JSON with recursive nesting for debugging
- ✅ **Comma-less Syntax**: Modern clean syntax for AI services and emit statements
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
research/                     → Research papers and critical information on AI architecture and documentation on ground breaking achievements in Aura/Cx Language
wiki/                         → Static documentation (timeless reference material only)
.github/                      → GitHub workflows, issue templates, and Copilot instructions
.github/copilot-instructions.md → Copilot instructions for code generation
.github/instructions/cx.instructions.md → Detailed CX language syntax and guidelines
.github/issue_templates/      → Issue templates for bug reports and feature requests
.github/workflows/            → GitHub Actions workflows for CI/CD
.github/issue_templates/bug_report.md → Template for bug reports
.github/issue_templates/feature_request.md → Template for feature requests
```

### **Revolutionary Language Design**
- **Pure Event-Driven Architecture**: Classes contain ONLY `realize()` constructors and event handlers
- **No Member Fields**: All state management through AI services and event payloads
- **No `this` Keyword**: Complete elimination of instance references for pure stateless programming
- **Cognitive Constructors**: `realize(self: object)` with `learn self;` for AI-driven initialization
- **Event-Only Communication**: All behavior flows through the Aura Cognitive Framework
- **Aura Cognitive Framework**: A decentralized eventing model where each agent possesses a local `EventHub` (a personal nervous system) for internal processing, all orchestrated by a global `EventBus` that manages inter-agent communication.

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

## Latest Language Enhancements

### **Enhanced Handlers Pattern (v1.0)**
- **Custom Payload Support**: `handlers: [ analysis.complete { option: "detailed" }, task.finished { status: "done" } ]`
- **Mixed Handler Arrays**: Combine handlers with and without custom payloads in same array
- **Comma-less Syntax**: Clean, modern syntax for AI services and emit statements
- **Payload Propagation**: Handler events receive both original payload AND custom handler data

### **Class-Based Event Handler Patterns (v1.0)**
- **Instance Variable Access**: Class-based event handlers provide superior scope resolution with `this.fieldName` pattern
- **State Management**: Event handlers can modify instance state directly (`this.currentPhase = "new_value"`)
- **Variable Scope**: Class handlers resolve variable scope more reliably than global handlers
- **Smart Await Integration**: AI-determined optimal timing with `await { reason: "context", minDurationMs: 1000, maxDurationMs: 3000 }`

### **Automatic Object Serialization (v1.0)**
- **CX Object Detection**: Automatically detects CX objects inheriting from AiServiceBase
- **Recursive JSON Display**: Nested CX objects display with full structure and proper indentation
- **Clean Field Filtering**: Internal fields (ServiceProvider, Logger) automatically hidden
- **Primitive Type Handling**: Strings, numbers, booleans print directly without JSON formatting
- **Debug-Ready Output**: Perfect for inspecting complex agent states and data structures

### **Voice Speed Control (v1.0)**
- **Speech Speed Parameter**: Use `speechSpeed: 0.9` to slow speech by 10% for better comprehension
- **Flexible Speed Control**: Range from 0.8 (slow) to 1.2 (fast) for different interaction needs
- **Instance-Based Control**: Each agent can have its own speech speed via `this.speechSpeed`
- **Debug-Ready Output**: Perfect for inspecting complex agent states and data structures

### **Modern Syntax Standards**
- **Comma-less Parameters**: `learn { data: "content", handlers: [...] }` instead of `learn, { data: "content" }`
- **Enhanced Event Access**: `event.propertyName` for direct property access in handlers
- **Dictionary Iteration**: Native `for (var item in event.payload)` with `.Key` and `.Value` access

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
- **Object Inspection**: Use `print(objectName)` to automatically serialize CX objects to JSON for debugging
- **Compilation Issues**: Ensure correct syntax according to cx.instructions.md guidelines
- **Performance**: For slow response times, verify Azure OpenAI service availability and network connectivity
- **Debugging**: Use extensive logging and debug traces to identify issues in event processing or service interactions
