# CX Language - AI Coding Agent Instructions

## Project Overview
CX is an AI-native agentic programming language with JavaScript/TypeScript-like syntax, built on .NET 8 with IL code generation. **Phase 4 (AI Integration) is 100% COMPLETE** with production-ready AI services and final IL optimization achieved.

## Architecture & Key Components

### Multi-Project Solution Structure
- `CxLanguage.CLI/` - Command-line interface with Azure OpenAI configuration
- `CxLanguage.Parser/` - ANTLR4-based parser (`grammar/Cx.g4` is the source of truth)
- `CxLanguage.Compiler/` - IL code generation with two-pass compilation
- `CxLanguage.StandardLibrary/` - 9 AI services via Microsoft Semantic Kernel 1.26.0
- `CxLanguage.Azure/` - Azure OpenAI integration layer
- `examples/` - **All .cx files must go here** for testing

### Critical Build Commands
```bash
# Standard build and test workflow
dotnet build CxLanguage.sln
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_working_demo.cx

# AI services demo (requires Azure OpenAI config)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_ai_mp3_demo.cx
```

## Language Implementation Status

### ✅ Production Ready Features
- Variables (`var`), data types (string, number, boolean), arithmetic/logical operations
- Control flow: `if/else`, `while`, `for-in` loops with **mandatory Allman-style braces**
- Functions: declarations, parameters, return values, local scoping (two-pass compilation)
- Classes: declarations, constructors, methods, field assignments (`this.field = value`)
- Exception handling: `try/catch/throw` with .NET integration
- Object/array literals with property access and indexing
- **AI Services**: TextGeneration, ChatCompletion, DALL-E 3, Embeddings, Text-to-Speech MP3 streaming
- **FINAL IL OPTIMIZATION COMPLETE**: All service call patterns working with CxRuntimeHelper approach

### 🔄 Grammar Defined, Compiler Pending
- Class inheritance (`extends`), interfaces, async/await, access modifiers
- Module imports (`using service from "namespace"`), typed parameters

## CX Language Patterns

### Code Style (Non-Negotiable)
```cx
// ALWAYS use Allman-style brackets - opening bracket on new line
function example()
{
    if (condition)
    {
        // code here
    }
    else
    {
        // alternative code
    }
}

// NEVER use K&R-style (opening bracket same line)
```

### AI Service Integration Pattern
```cx
using textGen from "Cx.AI.TextGeneration";
using chatBot from "Cx.AI.ChatCompletion";
using tts from "Cx.AI.TextToSpeech";

// Parameter objects for AI configuration
var story = textGen.GenerateAsync("Write a story", {
    temperature: 0.8,
    maxTokens: 500,
    topP: 0.9
});

// Zero-file MP3 streaming (revolutionary feature)
tts.SpeakAsync("Pure memory audio playback!");
```

### Class System Pattern
```cx
class Agent
{
    name: string;
    
    constructor(agentName)
    {
        this.name = agentName;
    }
    
    function process(task)
    {
        return "Processing: " + task;
    }
}

var agent = new Agent("CX Assistant");
```

## Development Workflows

### Testing New Features
1. Create `.cx` files in `examples/` directory only
2. Use `comprehensive_working_demo.cx` as template for core features
3. Use `comprehensive_ai_mp3_demo.cx` for AI service testing
4. Always include `try/catch` blocks to verify error handling

### AI Services Configuration
Requires `appsettings.json` with Azure OpenAI configuration:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4.1-nano",
    "ApiVersion": "2024-10-21"
  }
}
```

### Grammar Changes
- Edit `grammar/Cx.g4` first (single source of truth)
- Update corresponding visitor methods in `AstBuilder.cs`
- Add AST nodes to `AstNodes.cs`
- Implement IL generation in `CxCompiler.cs`

## Critical Integration Points

### Two-Pass Compilation Architecture
Functions are collected in first pass, then compiled in second pass to support forward references and proper scoping.

### Microsoft Semantic Kernel Integration
All AI services use Semantic Kernel 1.26.0 for orchestration. Parameter marshalling converts CX object literals to .NET service parameters automatically.

### Parameter Resolution Priority
Method resolution prioritizes string parameters for CX function calls, enabling natural language AI interactions.

## Current Phase 5 Priorities - Autonomous Agentic Features
- **Cx.Ai.Adaptations Standard Library** (AI-powered .NET IL generator for dynamic code generation)
- **Self keyword implementation** for function introspection and autonomous workflows
- **Multi-agent coordination** for agent communication and task delegation
- **Learning and adaptation** mechanisms for dynamic behavior modification
- **Self-modifying code** capabilities for runtime optimization
- **Autonomous workflow orchestration** for complex task planning and execution

## Documentation Updates Required
When making changes, always update:
- `README.md` with latest development status
- `.github/instructions/status` with feature completion tracking
- Example files in `examples/` to demonstrate new capabilities

## Key Files for Context
- `grammar/Cx.g4` - Language syntax definition
- `examples/comprehensive_working_demo.cx` - Core language features
- `examples/comprehensive_ai_mp3_demo.cx` - AI services showcase
- `src/CxLanguage.StandardLibrary/README.md` - AI services documentation
- `.github/instructions/status` - Current implementation status





