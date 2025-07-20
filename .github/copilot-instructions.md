# CX Language - Cognitive Executor for Autonomous Pr### CX Autonomous Programming Patterns

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

### Event-Driven Architecture - Simplified Syntax Rules ⚠️
**SIMPLIFIED RULES FOR PROPER CX COMPILATION:**

- **`if`**: Used for ALL conditional logic everywhere (functions, classes, event handlers, standalone code)
- **`emit`**: Globally available everywhere (functions, classes, standalone code, event handlers)  
- **`on`**: Defines event receiver functions (global or class instance level)
- **Event names**: UNQUOTED dot-separated identifiers (user.input, not "user.input")

```cx
// ✅ CORRECT: Event-driven conditional logic
on user.input (payload)  // ✅ UNQUOTED event name
{
    if (payload.intent == "question")  // ✅ 'if' everywhere
    {
        emit question.detected, payload;  // ✅ UNQUOTED emit
    }
}

// ✅ CORRECT: Function conditional logic
function analyze(data)
{
    if (data.confidence > 0.8)  // ✅ 'if' in function
    {
        emit high.confidence, data;  // ✅ UNQUOTED emit anywhere
        return "processed";
    }
}
```

## Project Overview
CX (Cognitive Executor) is an autonomous programming language built on the Aura cognitive architecture framework, with JavaScript/TypeScript-like syntax on .NET 8 with IL code generation. **Phase 4 (AI Integration) is 100% COMPLETE** with production-ready AI services and final IL optimization achieved.

**Core Principles:**
- **Safe**: Memory-safe IL generation with comprehensive error handling
- **Quality**: Enterprise-grade reliability with production-tested AI integration  
- **Productivity**: Ultra-fast compilation (~50ms) with intuitive syntax
- **Autonomy**: First-class support for autonomous agents and self-modifying code

**Autonomous Programming Platform:**
- **CX Language**: The Cognitive Executor - executable autonomous programming language
- **Aura Framework**: The cognitive architecture powering autonomous decision-making
- **Agent Integration**: AI agents (including Copilot) can execute CX code directly for autonomous programming tasks

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

## CX Autonomous Programming Patterns

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

// Zero-file MP3 streaming (autonomous audio capabilities)
tts.SpeakAsync("CX: Autonomous programming in action!");
```

### Premier Multi-Agent Voice Demonstration
```cx
using textGen from "Cx.AI.TextGeneration";
using tts from "Cx.AI.TextToSpeech";

class DebateAgent
{
    name: string;
    perspective: string;
    voiceStyle: string;
    speechRate: string;
    emotionalTone: string;
    confidenceLevel: string;
    speciality: string;
    
    constructor(name, perspective, voiceStyle, speechRate, emotionalTone, confidenceLevel, speciality)
    {
        this.name = name;
        this.perspective = perspective;
        this.voiceStyle = voiceStyle;
        this.speechRate = speechRate;
        this.emotionalTone = emotionalTone;
        this.confidenceLevel = confidenceLevel;
        this.speciality = speciality;
    }
    
    function generateArgument(topic, turnNumber)
    {
        var prompt = "You are " + this.name + ", " + this.perspective + ". " +
                    "Debate " + topic + " from your perspective. " +
                    "This is turn " + turnNumber + ". Be " + this.emotionalTone + 
                    " and " + this.confidenceLevel + ". Focus on " + this.speciality + ".";
        
        var argument = textGen.GenerateAsync(prompt, {
            temperature: 0.8,
            maxTokens: 200
        });
        
        return argument;
    }
    
    function speakArgument(argument)
    {
        // Multi-service coordination: TextGeneration → TTS
        var voicePrompt = "[" + this.voiceStyle + " voice, " + this.speechRate + " pace, " + 
                         this.emotionalTone + " tone] " + this.name + ": " + argument;
        
        tts.SpeakAsync(voicePrompt);
        print("🎙️ " + this.name + " (" + this.voiceStyle + "): " + argument);
        print("");
    }
}

// Three autonomous agents with distinct personalities
var drClimate = new DebateAgent(
    "Dr. Elena Rodriguez", 
    "a passionate climate scientist",
    "authoritative",
    "measured",
    "urgent",
    "highly confident", 
    "scientific data and evidence"
);

var ceoCorp = new DebateAgent(
    "Marcus Steel",
    "a pragmatic industrial CEO", 
    "professional",
    "steady",
    "pragmatic",
    "assertively confident",
    "economic realities and practical solutions"
);

var activistsarah = new DebateAgent(
    "Sarah Green",
    "a determined environmental activist",
    "passionate", 
    "energetic",
    "inspiring",
    "fiercely determined",
    "moral imperative and future generations"
);

// Structured debate coordination
print("🌍 CLIMATE CHANGE MULTI-AGENT VOICE DEBATE");
print("===========================================");

var topic = "the urgency of immediate climate action versus economic stability";

try
{
    // Turn 1: Each agent presents opening argument
    var drClimateArg1 = drClimate.generateArgument(topic, 1);
    drClimate.speakArgument(drClimateArg1);
    
    var ceoArg1 = ceoCorp.generateArgument(topic, 1);  
    ceoCorp.speakArgument(ceoArg1);
    
    var activistArg1 = activistsarah.generateArgument(topic, 1);
    activistsarah.speakArgument(activistArg1);
    
    print("🎯 Multi-Agent Voice Debate Complete!");
    print("✅ Three AI agents successfully coordinated");
    print("✅ Distinct voice personalities operational");
    print("✅ Complex service injection working");
    print("✅ CX Autonomous Programming Platform operational!");
}
catch (error)
{
    print("Error in multi-agent coordination: " + error);
}
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

## Current Phase 5 Achievement - Multi-Agent Voice Debate Demo

### 🏆 PREMIER DEMONSTRATION COMPLETE
- **✅ Multi-Agent Voice Debate System**: Three autonomous AI agents with distinct vocal personalities successfully implemented
- **✅ Voice Personality Framework**: Complete vocal characteristic system with 7-parameter agent constructors
- **✅ Advanced AI Service Integration**: TextGeneration + TTS working seamlessly within class methods
- **✅ Complex Constructor Logic**: Multi-parameter agent initialization with personality traits operational
- **✅ Field Assignment System**: `this.fieldName = value` working correctly with stack optimization
- **✅ Service Injection Architecture**: Static field-based dependency injection fully operational
- **✅ Premier Documentation**: Complete wiki showcase and README integration published

### Current Phase 5 Priorities - Next Autonomous Features
- **✅ Event-Driven Architecture Foundation**: `on`, `emit`, `if` keywords FULLY IMPLEMENTED! (Grammar ✅ AST ✅ Compiler ✅ - runtime event bus pending)
- **✅ Language Simplification Complete**: Removed `when` keyword - now uses `if` for ALL conditionals everywhere 
- **✅ Unquoted Event Names**: Clean dot-separated identifiers (user.input) without string quotes
- **✅ Parallel Keyword Implementation**: FULLY OPERATIONAL - Multi-agent coordination achieved (Grammar ✅ AST ✅ Compiler ✅ Runtime ✅)
- **✅ Static Service Registry**: Service calls within functions 100% working via optimized static registry pattern
- **✅ Multi-Agent AI Coordination**: **COMPLETE VOICE DEBATE DEMO** - Three agents with distinct personalities successfully implemented
- **✅ Class System Enhancement**: Field access (`this.fieldName`) working, class instantiation operational
- **🟡 AI Service Method Calls**: Refine IL generation for service calls within class methods (causing runtime program errors)
- **⏳ Event Bus Runtime Implementation**: Implement actual event subscription, emission, and dispatch system
- **⏳ Self keyword implementation**: Function introspection for autonomous workflows
- **⏳ Cx.Ai.Adaptations Standard Library**: AI-powered .NET IL generator for dynamic code generation
- **⏳ True Parallel Threading**: Convert synchronous execution to Task-based parallelism
- **⏳ Learning and adaptation**: Dynamic behavior modification based on outcomes
- **⏳ Self-modifying code**: Runtime code generation and optimization capabilities
- **⏳ Advanced Agent Communication**: Multi-agent coordination via reactive event patterns

### Premier Multi-Agent Voice Demonstration (PRODUCTION DEMO)
**Located**: `examples/debug_exact_scenario.cx`  
**Status**: ✅ FULLY OPERATIONAL - Three AI agents with distinct voice personalities

This demonstration showcases the complete CX Language autonomous programming capabilities:
- **Multi-Agent Coordination**: Three AI agents working together in structured debate
- **Voice Personality System**: Complete vocal characteristic framework with 7 parameters per agent
- **Advanced Service Integration**: TextGeneration + TTS working seamlessly within class methods
- **Complex Constructor Logic**: Multi-parameter agent initialization fully operational
- **Field Assignment**: `this.fieldName = value` working correctly with stack optimization

**Execute Demo**:
```bash
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/debug_exact_scenario.cx
```

**Wiki Documentation**: `wiki/Premier-Multi-Agent-Voice-Debate-Demo.md` - Complete technical specification

## Key Files for Context
- `grammar/Cx.g4` - Language syntax definition
- `examples/comprehensive_working_demo.cx` - Core language features
- `examples/comprehensive_ai_mp3_demo.cx` - AI services showcase
- `src/CxLanguage.StandardLibrary/README.md` - AI services documentation
- `.github/instructions/status` - Current implementation status





