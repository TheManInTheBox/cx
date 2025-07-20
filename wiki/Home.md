# Cx Language Wiki

Welcome to the **Cx (Cognitive Executor)** language wiki - your comprehensive guide to autonomous programming with AI-native development.

## 🎭 **FEATURED: Multi-Agent Voice Debate Demo**

**🌟 [Premier Multi-Agent Voice Debate Demo](Premier-Multi-Agent-Voice-Debate-Demo.md) - The Ultimate CX Demonstration**

Three autonomous AI agents with distinct voice personalities debating climate change:
- **Dr. Green**: Authoritative-scientific voice, fast-paced, urgent tone
- **Prof. Smith**: Calm-analytical voice, measured-slow, contemplative tone  
- **Maya**: Youthful-energetic voice, medium-paced, passionate tone

**Revolutionary features demonstrated**: Multi-agent coordination, voice personality system, speech synthesis integration, complex service injection, and structured turn-based interaction.

---

## 🧠 What is Cx?

Cx is an autonomous programming language built on the **Aura cognitive architecture framework**, featuring JavaScript/TypeScript-like syntax on .NET 8 with IL code generation. **Phase 4 (AI Integration) is 100% COMPLETE** with 6 operational AI services and production-ready vector database integration.

### Core Philosophy: Autonomous AI-First Development
- **AI-Native Design**: Every language feature designed for autonomous AI workflows  
- **Agentic Programming**: Code that reasons, adapts, and self-modifies based on runtime feedback
- **Cognitive Architecture**: Built around the SEIDR loop: Synthesize, Execute, Instruct, Debug, Repair
- **Semantic Intelligence**: First-class support for embeddings, vector databases, and RAG workflows

## 📚 Documentation Sections

### Featured Demonstrations
- **[🎭 Premier Multi-Agent Voice Debate Demo](Premier-Multi-Agent-Voice-Debate-Demo.md)** - The ultimate CX showcase
- [[Autonomous Programming Best Practices]] - Proven patterns for AI-native development
- [[Event-Driven Architecture]] - Aura sensory framework with `on`, `emit`, `if`

### Getting Started
- [[Installation Guide]] - Set up your Cx development environment
- [[Quick Start Tutorial]] - Your first autonomous Cx program
- [[Language Reference]] - Complete syntax and features overview

### Advanced Features
- [[Semantic Similarity Patterns]] - AI-powered text analysis and classification
- [[Multi-Agent Coordination]] - Parallel AI agents working together
- [[AI Services Overview]] - All 6 AI services available in Cx
- [[Vector Database Service]] - RAG workflows and knowledge management
- [[Text Embeddings Service]] - Semantic similarity and classification
- [[Image Generation Service]] - DALL-E 3 integration for visual content
- [[Text-to-Speech Service]] - MP3 audio generation and streaming

### Advanced Topics
- [[Aura Cognitive Architecture]] - The sensory and awareness layer
- [[IL Code Generation]] - How Cx compiles to .NET IL
- [[Performance Optimization]] - Sub-50ms compilation and runtime efficiency
- [[Multi-Service Configuration]] - Azure OpenAI multi-region setup

### Examples and Tutorials
- [[Climate Debate Demo]] - 4 parallel AI agents debating climate change
- [[Presence Detection System]] - Real-time audio transcription and intent analysis  
- [[Content Classification Pipeline]] - Semantic routing and department assignment
- [[RAG Knowledge Assistant]] - Question answering with vector database

### Development
- [[Contributing Guide]] - How to contribute to Cx development
- [[Architecture Overview]] - Internal system design and components
- [[Testing Patterns]] - How to test autonomous Cx programs
- [[Debugging Autonomous Code]] - Tools and techniques for AI-native debugging

## 🚀 Quick Examples

### Event-Driven Autonomous Agent
```cx
using textGen from "Cx.AI.TextGeneration";
using embeddings from "Cx.AI.TextEmbeddings";

// Agent responds to environmental stimuli
on "user.message" (payload)
{
    // Structured AI response for reliable processing
    var sentiment = textGen.GenerateAsync(
        "Rate sentiment 1-10 (1=negative, 10=positive). Respond only with number:",
        payload.content
    );
    
    var intent = textGen.GenerateAsync(
        "Classify intent as: question, command, greeting, complaint, or other",
        payload.content
    );
    
    // Semantic similarity for nuanced pattern detection
    var urgencyEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help critical");
    var contentEmbedding = embeddings.GenerateEmbeddingAsync(payload.content);
    var urgencyLevel = embeddings.CalculateSimilarity(contentEmbedding, urgencyEmbedding);
    
    when (urgencyLevel > 0.8)
    {
        emit "priority.urgent", {
            content: payload.content,
            sentiment: sentiment,
            intent: intent,
            urgencyScore: urgencyLevel
        };
    }
}
```

### Multi-Agent Coordination
```cx
class DebateAgent
{
    perspective: string;
    constructor(p) { this.perspective = p; }
    
    function argue(topic)
    {
        return textGen.GenerateAsync(
            "Argue about " + topic + " from the perspective of " + this.perspective
        );
    }
}

var agents = [
    new DebateAgent("a climate scientist"),
    new DebateAgent("an industrial CEO"), 
    new DebateAgent("a policy maker"),
    new DebateAgent("a citizen activist")
];

// Run all agents concurrently
parallel for (agent in agents)
{
    var argument = agent.argue("renewable energy transition");
    emit "debate.position", {
        perspective: agent.perspective,
        argument: argument
    };
}
```

## 🎯 Key Features

### ✅ Production Ready
- **Memory-Safe**: IL code generation ensures stability
- **Fast Compilation**: Sub-50ms compile times
- **Error Handling**: Comprehensive exception management
- **AI Integration**: 6 operational AI services

### ✅ Autonomous Programming
- **Event-Driven**: `on`, `when`, `emit` for reactive systems
- **Parallel Execution**: Multi-agent coordination 
- **Semantic Intelligence**: Embeddings and similarity matching
- **Self-Modifying**: Runtime code adaptation (Phase 5)

### ✅ Developer Experience
- **Familiar Syntax**: JavaScript/TypeScript-like with Allman bracing
- **Zero Configuration**: AI services work out of the box
- **Rich Examples**: Comprehensive demo programs
- **Performance**: Production-grade reliability

## 🔬 Current Status

### Phase 4: AI Integration - ✅ COMPLETE
- ✅ 6 AI services fully operational
- ✅ Vector database with RAG capabilities  
- ✅ Embedding generation and similarity calculations
- ✅ Image generation and MP3 audio streaming
- ✅ Multi-service Azure OpenAI configuration

### Phase 5: Autonomous Agentic Features - 🔄 IN PROGRESS
- ✅ Event-driven architecture foundation (`on`, `when`, `emit`)
- ✅ Parallel keyword for multi-agent coordination
- ⏳ Event bus runtime implementation
- ⏳ Self keyword for function introspection
- ⏳ True parallel threading with Task-based execution
- ⏳ Learning and adaptation mechanisms

## 🌟 Why Choose Cx?

### For AI Developers
- **Native AI Integration**: No complex setup or API wrappers
- **Autonomous Patterns**: Built-in support for agent-based programming
- **Semantic Intelligence**: First-class embeddings and vector operations
- **Event-Driven**: Perfect for reactive AI systems

### For Enterprise
- **Production Ready**: Memory-safe IL compilation
- **Performance**: Sub-second execution for complex AI workflows
- **Scalable**: Multi-agent coordination and parallel processing
- **Reliable**: Comprehensive error handling and .NET integration

### For Researchers  
- **Cognitive Architecture**: Aura framework for autonomous systems
- **Self-Modifying Code**: Runtime adaptation and learning
- **Multi-Modal**: Text, image, audio generation capabilities
- **Experimental**: Cutting-edge autonomous programming paradigms

---

**Get Started**: [[Installation Guide]] | **Examples**: [[Quick Start Tutorial]] | **Community**: [GitHub Discussions](https://github.com/ahebert-lt/cx/discussions)

*Cx enables a new paradigm of autonomous programming where AI agents can think, code, and execute solutions in real-time through the Aura cognitive architecture framework.*
