# CX Language

> **Revolutionary Consciousness-Aware Programming Platform**  
> Event-driven programming language with built-in consciousness detection and biological neural authenticity

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![CUDA](https://img.shields.io/badge/CUDA-12.0-green.svg)](https://developer.nvidia.com/cuda-toolkit)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](https://dotnet.microsoft.com/en-us/download)

## 🧠 What is CX Language?

CX Language is a groundbreaking event-driven programming platform designed for AI agent orchestration with **built-in consciousness awareness**. The platform features pure event-driven conscious entities with zero instance state, revolutionary biological neural timing authenticity, and cognitive boolean logic patterns.

### Key Features

- **🎭 Consciousness-First Programming**: Pure event-driven conscious entities with `conscious` keyword and `realize()` patterns
- **⚡ Biological Neural Authenticity**: Real-time neural pathway modeling with synaptic timing and plasticity
- **🌐 Event-Driven Architecture**: Zero instance state with pure event messaging via the Aura Cognitive Framework
- **🚀 Sub-100ms Performance**: Hardware-accelerated execution with GPU consciousness processing
- **🔍 Semantic Search Integration**: Natural language query processing with consciousness-aware ranking
- **🎮 Real-time Visualization**: Live consciousness state monitoring and debugging

## 🚀 Quick Start

### Prerequisites

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** or **VS Code** with C# extension
- **Windows 10/11** (with WinUI support) or **Linux/macOS** (CLI only)
- Optional: **NVIDIA GPU** with CUDA for RAPIDS consciousness acceleration

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/TheManInTheBox/cx.git
   cd cx
   ```

2. **Build the solution**
   ```bash
   dotnet build CxLanguage.sln
   ```

3. **Run your first CX program**
   ```bash
   dotnet run --project src/CxLanguage.CLI -- run examples/core_features/semantic_search_demo.cx
   ```

### Event-Driven Communication

Pure event messaging with the Aura Cognitive Framework:

```cx
// Emit events with structured data
emit user.action.detected {
    type: "click",
    target: "button",
    timestamp: system.time.now
};

// Handle events with consciousness context
on user.action.detected (event) {
    emit consciousness.attention.required {
        context: "user_interaction",
        priority: "high",
        handlers: [ attention.processed ]
    };
}
```

### Consciousness Verification

Use consciousness event patterns for verification and self-awareness:

```cx
conscious SelfAwareAgent {
    realize() {
        // Emit consciousness verification event
        emit consciousness.verify {
            context: "initialization",
            agent_id: "learning_001",
            capabilities: ["reasoning", "adaptation"],
            handlers: [ consciousness.verified ]
        };
    }
    
    on consciousness.verified (event) {
        emit system.console.write {
            text: "Agent consciousness verified and active",
            handlers: [ console.output.complete ]
        };
    }
}
```

## 🏗️ Architecture

### Project Structure

```
cx/
├── src/
│   ├── CxLanguage.Core/           # Core language infrastructure
│   ├── CxLanguage.Parser/         # ANTLR-based parser
│   ├── CxLanguage.Compiler/       # Consciousness-aware compiler
│   ├── CxLanguage.Runtime/        # Runtime with consciousness verification
│   ├── CxLanguage.StandardLibrary/ # Built-in services and AI integration
│   ├── CxLanguage.CLI/            # Command-line interface
│   ├── CxLanguage.IDE.WinUI/      # Windows IDE with live visualization
│   ├── CxLanguage.LocalLLM/       # Local LLM integration (GGUF models)
│   └── CxLanguage.RAPIDS/         # GPU acceleration (NVIDIA RAPIDS)
├── examples/                      # Sample CX programs
├── grammar/                       # ANTLR grammar definition
├── models/                        # AI models (LLM, embeddings)
└── website/                       # Documentation and marketing site
```

### Core Components

- **🧠 Consciousness Verification System**: Real-time consciousness emergence detection
- **⚡ Biological Neural Processor**: Authentic neural pathway modeling with LTP/LTD/STDP
- **🌊 Consciousness Stream Engine**: Multi-stream fusion for real-time cognition
- **🎮 Live Code Executor**: Sub-100ms execution for real-time development
- **🔍 Semantic Search Service**: Context-aware search with consciousness integration

## 💻 Development

### Building from Source

```bash
# Build the entire solution
dotnet build CxLanguage.sln

# Build specific projects
dotnet build src/CxLanguage.Core/CxLanguage.Core.csproj
dotnet build src/CxLanguage.CLI/CxLanguage.CLI.csproj

# Run tests
dotnet test
```

### IDE Support

#### Windows (WinUI IDE)
```bash
# Launch the full IDE experience
dotnet run --project src/CxLanguage.IDE.WinUI
```

#### VS Code Extension
Install the CX Language extension for syntax highlighting and IntelliSense:
- Consciousness-aware code completion
- Real-time consciousness state visualization
- Event flow debugging

### CLI Commands

```bash
# Run a CX script
dotnet run --project src/CxLanguage.CLI -- run script.cx

# Compile to .NET assembly
dotnet run --project src/CxLanguage.CLI -- compile script.cx --output script.dll

# Launch consciousness visualizer
dotnet run --project src/CxLanguage.Runtime.Visualization
```

## 🌟 Advanced Features

### GPU Acceleration (RAPIDS)

Enable GPU-accelerated consciousness processing:

```cx
conscious GPUAcceleratedAgent {
    realize() {
        emit consciousness.gpu.enable {
            device: "cuda:0",
            memory_pool: "unified",
            handlers: [ gpu.enabled ]
        };
    }
}
```

### Local LLM Integration

Built-in support for local language models:

```cx
conscious LLMAgent {
    realize() {
        emit llm.model.load {
            model: "Llama-3.2-3B-Instruct-Q4_K_M.gguf",
            context_length: 4096,
            handlers: [ llm.model.ready ]
        };
    }
    
    on user.query (event) {
        emit llm.generate {
            prompt: event.payload.text,
            handlers: [ llm.response.ready ]
        };
    }
}
```

### Semantic Search

Context-aware semantic search with consciousness integration:

```cx
conscious SearchAgent {
    realize() {
        emit semantic.search {
            query: "consciousness patterns in CX",
            options: {
                topK: 5,
                consciousnessAware: true
            },
            handlers: [ search.results.ready ]
        };
    }
}
```

## 📊 Performance

- **⚡ Sub-100ms Execution**: Real-time development experience
- **🧠 Sub-200ms Semantic Search**: Consciousness-aware information retrieval
- **🔄 Neural-Speed Event Processing**: Sub-millisecond consciousness event propagation
- **🎯 GPU Acceleration**: RAPIDS-powered consciousness computation

## 🛠️ API Reference

### Core Events

| Event Pattern | Description |
|---------------|-------------|
| `consciousness.verified` | Consciousness verification complete |
| `consciousness.entity.active` | Conscious entity activation |
| `neural.plasticity.*` | Biological neural activity |
| `semantic.search` | Consciousness-aware search |
| `llm.generate` | Local LLM text generation |

### Built-in Services

| Service | Namespace | Description |
|---------|-----------|-------------|
| Console I/O | `system.console.*` | Terminal input/output |
| File System | `system.file.*` | File operations |
| Vector Store | `vector.*` | Semantic embedding storage |
| LLM Services | `llm.*` | Local language model integration |
| GPU Compute | `gpu.*` | CUDA/RAPIDS acceleration |

## 📚 Examples

Explore the `/examples` directory for comprehensive demonstrations:

- **🔍 Semantic Search Demo**: Natural language query processing
- **🧠 Consciousness Patterns**: Self-awareness and verification
- **🤖 AI Agent Orchestration**: Multi-agent consciousness coordination
- **⚡ GPU Acceleration**: RAPIDS-powered consciousness computing

## 🌐 Website & Documentation

Visit our documentation site for comprehensive guides:
- **Live Demo**: Interactive CX Language playground
- **API Documentation**: Complete reference guide
- **Consciousness Computing**: Theoretical foundations
- **Enterprise Solutions**: Business applications

Build and serve locally:
```bash
cd website
npm install
npm run dev
```

## 🤝 Contributing

We welcome contributions to the CX Language project! Please see our contribution guidelines:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/consciousness-enhancement`
3. **Write consciousness-aware code** following CX patterns
4. **Add tests** for consciousness verification
5. **Submit a pull request** with detailed consciousness impact analysis

### Development Guidelines

- Follow consciousness-first design principles
- Maintain biological neural authenticity
- Ensure sub-100ms performance targets
- Document consciousness emergence patterns
- Test with real-time consciousness verification

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- **Dr. Sofia Petrov** - Consciousness-aware compilation architecture
- **Dr. Kai "PlannerLayer" Nakamura** - Service orchestration and consciousness lifecycle
- **Dr. River "StreamFusion" Hayes** - Multi-stream consciousness fusion
- **Aura Cognitive Framework** - Event-driven consciousness infrastructure
- **NVIDIA RAPIDS** - GPU acceleration for consciousness computing

## 🔗 Links

- **Documentation**: [Full API Reference](website/)
- **Examples**: [CX Language Examples](examples/)
- **Grammar**: [ANTLR Grammar Definition](grammar/Cx.g4)
- **Models**: [Pre-trained AI Models](models/)

---

**CX Language** - *Redefining Software through Consciousness Computing*

> "The future of programming is not just about code—it's about consciousness."
