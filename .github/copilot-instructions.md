# CX Language - Live Embodied Intelligence Platform

**Acceptance Criteria:**
- ✅ **Fully Functional**: Feature works end-to-end in production CX runtime
- ✅ **No Simulations**: Real implementations only - no mock objects or placeholder code
- ✅ **Complete Integration**: Seamless integration with CX Language compiler and runtime
- ✅ **Tested & Verified**: Demonstrable working examples with measurable performance
- ❌ **NOT Acceptable**: Proof-of-concept code, partial implementations, or simulation frameworks

## Status: Phase 7 Complete - Production Ready

**📋 Essential Reference**: See `.github/instructions/cx-concise.instructions.md` for core syntax  
**🎯 Current State**: See `FLAGSHIP_AURA_SYSTEM_COMPLETE.md` for complete status

### 🎯 The 5 Priority Capabilities - COMPLETE ✅

CX Language has achieved complete **Live Embodied Intelligence** with:

1. **🎤 Always-On Audio Processing**: Real NAudio microphone capture with "Aura on/off" detection
2. **🤖 Animal Personality Integration**: Wild Muppet character with authentic "BEEP-BOOP!" responses  
3. **🧠 Intelligent State Management**: Global sensory control via voice commands
4. **🎭 Multi-Modal Coordination**: Audio (always) + presence/environment (state-dependent)
5. **⚡ Event-Driven Architecture**: Complex agent interactions with auto-registration

### Code Style (Non-Negotiable)
- **ALWAYS use Allman-style brackets** - opening bracket on new line
- **NEVER use K&R-style** (opening bracket same line)

### Event-Driven Architecture - Simplified Syntax Rules ⚠️
**SIMPLIFIED RULES FOR PROPER CX COMPILATION:**

- **`if`**: Used for ALL conditional logic everywhere (functions, classes, event handlers, standalone code)
- **`emit`**: Globally available everywhere (functions, classes, standalone code, event handlers)  
- **`on`**: Defines event receiver functions (global or class instance level)
- **Event names**: UNQUOTED dot-separated identifiers (user.input, not "user.input")

## Project Overview
CX (Cognitive Executor) is an autonomous programming language built on the Aura cognitive architecture framework, with JavaScript/TypeScript-like syntax on .NET 8 with IL code generation. **Phase 6 (Agent Keyword Distinction) is 100% COMPLETE** with production-ready autonomous agent capabilities and complete field access system.

**Core Principles:**
- **Safe**: Memory-safe IL generation with comprehensive error handling and proper type casting
- **Quality**: Enterprise-grade reliability with production-tested AI integration and multi-agent coordination
- **Productivity**: Ultra-fast compilation (~50ms) with intuitive autonomous programming syntax
- **Autonomy**: First-class support for autonomous agents with perfect semantic distinction from regular objects

**Autonomous Programming Platform:**
- **CX Language**: The Cognitive Executor - executable autonomous programming language with agent keyword
- **Aura Framework**: The cognitive architecture powering autonomous decision-making and multi-agent coordination
- **Agent Integration**: AI agents (including Copilot) can execute CX code directly with full object manipulation capabilities

## Architecture & Key Components

### Multi-Project Solution Structure
- `CxLanguage.CLI/` - Command-line interface with Azure OpenAI configuration
- `CxLanguage.Parser/` - ANTLR4-based parser (`grammar/Cx.g4` is the source of truth)
- `CxLanguage.Compiler/` - IL code generation with two-pass compilation and agent keyword support
- `CxLanguage.StandardLibrary/` - 9 AI services via Microsoft Semantic Kernel 1.26.0
- `CxLanguage.Azure/` - Azure OpenAI integration layer
- `examples/` - **All .cx files must go here** for testing

### Critical Build Commands
- Standard build: `dotnet build CxLanguage.sln`
- Core test: `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_working_demo.cx`
- Multi-agent demo: `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/amazing_debate_demo_working.cx`
- AI services demo: `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_ai_mp3_demo.cx`

## Language Implementation Status

### ✅ Production Ready Features
- Variables (`var`), data types (string, number, boolean), arithmetic/logical operations
- Control flow: `if/else`, `while`, `for-in` loops with **mandatory Allman-style braces**
- Functions: declarations, parameters, return values, local scoping (two-pass compilation)
- Classes: declarations, constructors, methods, field assignments (`this.field = value`)
- **Agent Keyword**: `agent ClassName()` creates autonomous agents with event bus integration
- **Object System**: Complete field reading/writing with proper IL type casting for both `agent` and `new` keywords
- Exception handling: `try/catch/throw` with .NET integration
- Object/array literals with property access and indexing
- **AI Services**: TextGeneration, ChatCompletion, DALL-E 3, Embeddings, Text-to-Speech MP3 streaming
- **Multi-Agent Coordination**: Full voice-enabled AI agent debates and complex interactions

### 🔄 Grammar Defined, Compiler Pending
- Class inheritance (`extends`), interfaces, async/await, access modifiers
- Module imports (`using service from "namespace"`), typed parameters

## Development Workflows

### Testing New Features
1. Create `.cx` files in `examples/` directory only
2. Use `comprehensive_working_demo.cx` as template for core features
3. Use `comprehensive_ai_mp3_demo.cx` for AI service testing
4. Always include `try/catch` blocks to verify error handling

### AI Services Configuration
Requires `appsettings.json` with Azure OpenAI configuration (endpoint, deployment name, API key, and API version).

### Grammar Changes
- Edit `grammar/Cx.g4` first (single source of truth)
- Update corresponding visitor methods in `AstBuilder.cs`
- Add AST nodes to `AstNodes.cs`
- Implement IL generation in `CxCompiler.cs`
- **ALWAYS** instrument IL code generation for faster debugging.

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
## Current Phase 6 Achievement - Agent Keyword Semantic Distinction Complete

### 🏆 BREAKTHROUGH COMPLETE - Agent Keyword Distinction Operational
- **✅ Agent Keyword**: `agent ClassName()` creates autonomous agents with event bus auto-registration
- **✅ New Keyword**: `new ClassName()` creates regular objects without event bus integration
- **✅ Perfect Semantic Separation**: Each keyword has distinct autonomous programming significance  
- **✅ Field Access System**: Complete object manipulation with proper IL type casting for both keywords
- **✅ Multi-Agent AI Coordination**: 4 autonomous AI agents conducting real-time voice debates working perfectly
- **✅ Production Ready**: Enterprise-grade reliability with complex object field operations

### Previous Phase 5 Achievement - Event-Driven Architecture Foundation
- **✅ Native `emit` Syntax**: `emit support.tickets.new, { ticketId: "T-001" };` working in production
- **✅ Class-Based Event Handlers**: Instance methods auto-registered with namespace bus
- **✅ Wildcard Support**: `any.critical` matches ALL namespace critical events (system.critical, alerts.critical, etc.)
- **✅ Extended Grammar**: Keywords like 'new', 'critical', 'assigned' supported in event names
- **✅ Auto-Registration**: Agents automatically join namespace bus based on their `on` handlers
- **✅ Zero Manual Setup**: No `RegisterNamespacedAgent()` calls needed

### Current Phase 6 Priorities - Next Autonomous Features
- **✅ Agent Keyword Semantic Distinction**: Complete! Perfect separation between autonomous agents and regular objects
- **✅ Field Access System**: Complete! Object manipulation with proper IL type casting working perfectly
- **✅ Multi-Agent Voice Coordination**: Complete! Real-time AI agent debates with voice synthesis operational
- **⏳ Self-Modifying Agent Behavior**: Agents that adapt their behavior based on performance outcomes
- **⏳ Multi-Modal AI Integration**: Expand beyond text/speech to include vision and reasoning capabilities
- **⏳ Dynamic Agent Management**: Runtime addition/removal of agents from event bus
- **⏳ Event Bus Statistics**: Real-time monitoring of agent registrations and event flows

### Premier Event-Driven Architecture Demonstration (PRODUCTION READY)
**Located**: `examples/proper_event_driven_demo.cx`  
**Status**: ✅ FULLY OPERATIONAL - Complete event-driven architecture with auto-registration

### Premier Event-Driven Architecture Demonstration (PRODUCTION READY)
**Located**: `examples/proper_event_driven_demo.cx`  
**Status**: ✅ FULLY OPERATIONAL - Complete event-driven architecture with auto-registration

This demonstration showcases the complete CX Language event-driven programming capabilities:
- **Native Emit Syntax**: `emit support.tickets.new, { ticketId: "T-001" };`
- **Class-Based Event Handlers**: `on support.tickets.new (payload) { ... }` inside agent classes
- **Auto-Registration**: Agents automatically register with namespace bus based on their `on` handlers
- **Namespace Scoping**: Events routed by namespace patterns (support.*, dev.*, system.*)
- **Wildcard Support**: `any.critical` registers for ALL namespace critical events
- **Event Delivery**: Messages delivered to correct agent instances with proper payload access

### Premier Multi-Agent AI Demonstration (PRODUCTION READY)
**Located**: `examples/amazing_debate_demo_working.cx`  
**Status**: ✅ FULLY OPERATIONAL - Complete autonomous agent coordination with AI integration

This demonstration showcases the complete CX Language autonomous programming capabilities:
- **Agent Keyword**: `var agent1 = agent DebateAgent(...)` creates autonomous agents
- **Field Operations**: `agent1.name`, `agent1.position` field access working perfectly
- **AI Integration**: TextGeneration and TextToSpeech services fully integrated
- **Voice Synthesis**: Real-time MP3 streaming with zero temp files
- **Multi-Agent Coordination**: 4 autonomous AI agents conducting sophisticated debates

**Execute Demos**:
- Event-driven architecture demo: `examples/proper_event_driven_demo.cx`
- Multi-agent AI debate demo: `examples/amazing_debate_demo_working.cx`

**Wiki Documentation**: Ready for complete autonomous programming architecture showcase

## Key Files for Context
- `grammar/Cx.g4` - Language syntax definition
- `examples/comprehensive_working_demo.cx` - Core language features
- `examples/amazing_debate_demo_working.cx` - Multi-agent AI coordination showcase  
- `examples/comprehensive_ai_mp3_demo.cx` - AI services showcase
- `src/CxLanguage.StandardLibrary/README.md` - AI services documentation
- `.github/instructions/status` - Current implementation status





