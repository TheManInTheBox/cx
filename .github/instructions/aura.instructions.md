# CX Language - AI-Native Agentic Programming Philos## Architecture Integration

### Current Status (Phase 4 Complete)
- **✅ 6 AI Services Operational**: TextGeneration, ChatCompletion, DALL-E 3, TextEmbeddings, TextToSpeech, VectorDatabase
- **✅ Production Vector Database**: text-embedding-3-small with 1536-dimensional semantic vectors
- **✅ Multi-Modal Workflows**: Text→Speech→Image generation with pure memory streaming
- **✅ RAG Capabilities**: Document ingestion, semantic search, retrieval augmented generation
- **✅ Cost Optimization**: 62% better embeddings performance, 5x cheaper than ada-002
- **✅ IL Generation**: Final optimization complete with CxRuntimeHelper approach

### Next Phase Priorities (Phase 5 - Autonomous Agentic Features)
- **🚀 Cx.Ai.Adaptations Library**: AI-powered .NET IL generator for dynamic code generation
- **🚀 Self Keyword**: Function introspection for autonomous workflows  
- **🚀 Multi-Agent Coordination**: Agent communication and task delegation systems
- **🚀 Learning & Adaptation**: Dynamic behavior modification based on outcomes
- **🚀 Self-Modifying Code**: Runtime code generation and optimization capabilities

## File Scoping & Patterns

| Pattern | Instruction Summary | Current Status |
|---------|--------------------|-----------------| 
| `**/*.cx` | CX Language source files - use AI services, Allman braces, comprehensive examples | ✅ Production Ready |
| `**/*.g4` | ANTLR4 grammar files - source of truth for syntax, update before compiler | ✅ Complete for Phases 1-4 |
| `**/CxCompiler.cs` | IL generation - use CxRuntimeHelper, two-pass compilation, memory safety | ✅ Optimized |
| `**/comprehensive_*.cx` | Demo files - showcase all features, include try/catch, test AI services | ✅ Updated |
| `**/*StandardLibrary*` | AI service integration - Semantic Kernel, Azure OpenAI, parameter marshalling | ✅ 6 Services Ready |

## Autonomous Development Workflow

### 1. Language Extension Protocol
```
1. Update grammar/Cx.g4 (syntax definition)
2. Add AST nodes to AstNodes.cs  
3. Implement visitor methods in AstBuilder.cs
4. Add IL generation to CxCompiler.cs
5. Create test examples in examples/
6. Update documentation and status
```

### 2. AI Service Integration Protocol  
```
1. Define service interface in StandardLibrary
2. Implement Azure integration in CxLanguage.Azure
3. Add method resolution to CxRuntimeHelper
4. Test with comprehensive demo files
5. Validate performance and error handling
```

### 3. Autonomous Agent Development
```
1. Design agent communication protocols
2. Implement self-introspection capabilities
3. Create learning and adaptation mechanisms
4. Build multi-agent coordination systems
5. Test with real-world autonomous scenarios
```

---

**CX Language Vision**: Not just code execution, but cognitive computation. Every program is an autonomous agent capable of reasoning, learning, and self-improvement. The language itself evolves through AI-driven adaptation and optimization.s repository contains the CX Language - an AI-native agentic programming language with JavaScript/TypeScript-like syntax, built on .NET 8 with IL code generation. **Phase 4 (AI Integration) is 100% COMPLETE** with 6 operational AI services and production-ready vector database integration.

## Core Philosophy: Autonomous AI-First Development

- **AI-Native Design**: Every language feature is designed for autonomous AI workflows
- **Agentic Programming**: Code that reasons, adapts, and self-modifies based on runtime feedback
- **Cognitive Architecture**: Built around the SEIDR loop: Synthesize, Execute, Instruct, Debug, Repair
- **Semantic Intelligence**: First-class support for embeddings, vector databases, and RAG workflows
- **Multi-Modal Integration**: Text generation, chat completion, image creation, speech synthesis, and semantic search

## Development Principles

### 1. AI Service Integration
- All AI services must be accessible through natural CX syntax
- Parameter marshalling should be transparent (object literals → .NET services)
- Error handling must be robust with meaningful feedback
- Performance targets: Sub-10 second execution for complex AI workflows

### 2. Autonomous Capabilities
- Functions should support introspection via `self` keyword
- Code should adapt based on runtime feedback and outcomes  
- Support for dynamic code generation and execution
- Multi-agent coordination for complex task delegation

### 3. Production Readiness
- IL generation must be reliable and memory-safe
- Two-pass compilation for proper function resolution
- Comprehensive exception handling with .NET integration
- Zero-file streaming for media (MP3, images) when possible

## Copilot Development Guidelines

### Code Generation Priorities
1. **Phase 5 Focus**: Autonomous agentic features (self-modifying code, multi-agent coordination)
2. **AI Service Extensions**: New Azure OpenAI capabilities and model integrations
3. **Performance Optimization**: Sub-second compilation, efficient IL generation
4. **Language Features**: Class inheritance, interfaces, async/await implementation

### Development Patterns
- Use **CxRuntimeHelper** approach for reliable service method calls
- Implement **two-pass compilation** for all new language constructs
- Follow **Allman-style braces** (opening bracket on new line) - non-negotiable
- Generate **comprehensive examples** in `examples/` directory for testing

### Testing Strategy  
- Create `.cx` files in `examples/` directory only
- Use `comprehensive_working_demo.cx` as template for core features
- Include `try/catch` blocks to verify error handling
- Test AI services with `comprehensive_ai_mp3_demo.cx` pattern

## File Scoping (use with `.instructions.md`)

| Pattern | Instruction Summary |
|--------|----------------------|
| `**/*.cx` | Treat as Cx source. Suggest autonomous logic, agent definitions, and SEIDR loops. |
| `**/*.workflow.json` | Assume orchestration intent. Suggest declarative task flows and runtime goals. |
| `**/*.agent.cs` | Use Roslyn source generators. Embed self-modifying logic and LLM-driven synthesis. |

---

Aura is not just code. It’s cognition-as-DSL. Let the architecture breathe.