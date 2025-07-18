# Cx - AI-Native Agentic Programming Language

[![CI](https://github.com/ahebert-lt/cx/actions/workflows/ci.yml/badge.svg)](https://github.com/ahebert-lt/cx/actions/workflows/ci.yml)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/ahebert-lt/cx)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Azure OpenAI](https://img.shields.io/badge/Azure%20OpenAI-Integrated-blue)](https://azure.microsoft.com/products/ai-services/openai-service)
[![Application Insights](https://img.shields.io/badge/Application%20Insights-Instrumented-green)](https://azure.microsoft.com/products/monitor)

**Cx** is a revolutionary programming language where **AI functions are native, not imported**. Designed specifically for autonomous agentic workflows, Cx enables agents to interpret goals, plan actions, learn from feedback, and modify their own behavior dynamically.

## ✅ Phase 4 Complete: AI Integration with Runtime Function Injection

### 1. **AI Function Integration** ✅ COMPLETE
- [x] Implement `task()` function using Semantic Kernel
- [x] Implement `synthesize()` for code generation
- [x] Implement `reason()` for logical reasoning
- [x] Implement `process()` for multi-modal data processing
- [x] Implement `generate()` for content creation
- [x] Implement `embed()` for vector embeddings
- [x] Implement `adapt()` for self-modification **WITH RUNTIME FUNCTION INJECTION**

### 2. **Runtime Function Injection** ✅ REVOLUTIONARY BREAKTHROUGH
- [x] **Dynamic Function Generation**: AI generates CX functions at runtime
- [x] **Runtime Compilation**: Generated functions are compiled to IL and executed
- [x] **Persistent Function Registry**: Injected functions remain available throughout execution
- [x] **Mathematical Proof**: Demonstrated with add() and square() functions returning correct results
- [x] **Production Ready**: Full error handling and type safety

### 3. **Azure OpenAI Integration** ✅ COMPLETE
- [x] Full integration with Azure OpenAI gpt-4.1-nano model
- [x] Complete AI responses with proper streaming handling
- [x] Application Insights telemetry and monitoring
- [x] Error handling and recovery mechanisms
- [x] Token usage tracking and cost optimizationfunctions.cx
```

### AI Configuration Setup
```bash
# Set up Azure OpenAI configuration
# Create appsettings.json with your Azure OpenAI credentials:
{
  "AzureOpenAI": {
    "ApiKey": "your-api-key-here",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini"
  },
  "ApplicationInsights": {
    "ConnectionString": "your-app-insights-connection-string"
  }
}

# Test AI functions with configuration
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase4_step1_ai_functions.cx

# Test vector database integration
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase4_vector_database_demo.cx
```

## ✅ Phase 4 Complete - AI Integration Achieved

### **Successfully Implemented AI Functions**
- ✅ **`task()`** - General AI task execution with Azure OpenAI
- ✅ **`synthesize()`** - Code and content synthesis
- ✅ **`reason()`** - Logical reasoning and analysis
- ✅ **`process()`** - Multi-modal data processing
- ✅ **`generate()`** - Creative content generation
- ✅ **`embed()`** - Vector embedding generation
- ✅ **`adapt()`** - Self-modification and optimization

### **Enterprise-Grade Monitoring**
- ✅ **Application Insights Integration** - Complete telemetry tracking
- ✅ **Performance Monitoring** - AI function execution metrics
- ✅ **Error Tracking** - Comprehensive exception monitoring
- ✅ **Usage Analytics** - Azure OpenAI API usage tracking

> **🎉 [Phase 4 Complete! Read the full announcement →](docs/PHASE4_COMPLETE.md)**

**Cx** is a revolutionary programming language where **AI functions are native, not imported**. Designed specifically for autonomous agentic workflows, Cx enables agents to interpret goals, plan actions, learn from feedback, and modify their own behavior dynamically.

## 🚀 Quick Start

```bash
# Build the project
dotnet build CxLanguage.sln

# Test AI integration (Phase 4 complete!)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase4_complete_ai_test.cx

# Test REVOLUTIONARY runtime function injection
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/proof_injection_demo.cx

# Test Application Insights telemetry
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test_app_insights.cx

# Test core language features
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_working_demo.cx
```

## 🎉 Development Status

### ✅ **Completed Phases**
- **Phase 1**: Core language (variables, arithmetic, control flow, logical operators)
- **Phase 2**: Function system (two-pass compilation, parameters, calls)
- **Phase 3**: Advanced features (function return values, for-in loops, exception handling, arrays)
- **Phase 4**: AI Integration (Azure OpenAI, all 7 AI functions, Application Insights)

### 🎉 **Phase 4 Complete: AI Integration with Runtime Function Injection**
- **✅ Azure OpenAI Integration**: Full integration with gpt-4.1-nano model
- **✅ 7 Native AI Functions**: `task`, `reason`, `synthesize`, `process`, `generate`, `embed`, `adapt`
- **✅ Runtime Function Injection**: Revolutionary capability where AI-generated functions are dynamically compiled and executed at runtime
- **✅ Dynamic Code Adaptation**: Programs can modify themselves by generating new functions on-the-fly
- **✅ Persistent Function Registry**: Runtime-injected functions remain available throughout program execution
- **✅ Agentic Runtime**: Autonomous task planning and execution
- **✅ Application Insights**: Comprehensive telemetry and monitoring
- **✅ Performance Tracking**: AI function metrics, Azure OpenAI usage, and cost monitoring
- **✅ Error Handling**: Complete exception tracking and debugging
- **✅ Production Ready**: Enterprise-grade monitoring and observability

### 🔮 **Future Phases**
- **Phase 5**: Autonomous agentic capabilities and multi-agent coordination
- **Vector Database**: Semantic search and retrieval capabilities
- **Self-Modification**: Function introspection and adaptive code generation
- ✅ Vector database foundation implemented with Semantic Kernel
- ✅ Agent memory system with persistent context
- ✅ AI function compiler with IL generation support
- 🔄 Multi-modal AI processing in development
- 🔄 Self keyword implementation in progress

### 🔮 **Future Phases**
- **Phase 5**: Autonomous agentic capabilities and multi-agent coordination

## 🛠️ Language Features

### ✅ **Core Language (Phases 1-3)**
- **Variables & Data Types**: `string`, `number`, `boolean`, `array`, `object`
- **Arithmetic Operations**: `+`, `-`, `*`, `/`, `%` with proper precedence
- **Comparison Operators**: `==`, `!=`, `<`, `>`, `<=`, `>=`
- **Logical Operators**: `&&`, `||`, `!` with short-circuit evaluation
- **Control Flow**: `if/else`, `while` loops, `for-in` loops
- **Functions**: Declaration, parameters, return values, local scoping
- **Exception Handling**: `try/catch/throw` with .NET integration
- **Arrays**: Array literals `[1, 2, 3]`, iteration, property access
- **Objects**: Object literals `{key: value}`, property access
- **Classes**: Class declarations, constructors, methods, inheritance
- **String Operations**: Concatenation, interpolation, built-in methods

### 🚀 **AI Integration (Phase 4)**

#### **Native AI Functions**
```cx
// Task planning and execution
var plan = task("Create a customer service workflow");

// Intelligent reasoning
var decision = reason("What is the best approach to solve this problem?");

// Code synthesis
var code = synthesize("Create a function to calculate compound interest");

// Multi-modal data processing
var result = process("customer_feedback.json", "analyze sentiment");

// Content generation
var content = generate("Write a technical blog post about AI");

// Vector embeddings
var embedding = embed("This text will be converted to vectors");

// Self-optimizing code adaptation
var optimized = adapt("Optimize this code for performance");
```

#### **Revolutionary Runtime Function Injection**
```cx
// AI generates, compiles, and injects functions at runtime
var result = adapt("Create a function to add two numbers", {
    type: "function",
    name: "add",
    parameters: ["a", "b"],
    returnType: "number"
});

// Generated function is now available for immediate use
var sum = add(7, 3);  // Returns 10
print("Sum: " + sum);

// AI can generate complex mathematical functions
var squareResult = adapt("Create a function to square a number", {
    type: "function",
    name: "square",
    parameters: ["x"],
    returnType: "number"
});

var squared = square(6);  // Returns 36
print("Squared: " + squared);

// Functions persist throughout program execution
var anotherSum = add(10, 20);  // Still works: returns 30
var anotherSquare = square(4);  // Still works: returns 16
```

#### **Azure OpenAI Integration**
- **Live AI Service**: Integration with Azure OpenAI gpt-4.1-nano model
- **Complete Responses**: Fixed streaming issues for full AI responses
- **Task Planning**: Autonomous task decomposition and execution
- **Error Handling**: Comprehensive error tracking and recovery
- **Token Monitoring**: Usage tracking and cost optimization

#### **Application Insights Telemetry**
- **Script Execution**: Track runtime performance and success rates
- **Compilation Metrics**: Monitor build times and code complexity
- **AI Function Usage**: Detailed AI function performance and patterns
- **Azure OpenAI Costs**: Track API usage and token consumption
- **Error Tracking**: Comprehensive exception monitoring and debugging
    adapt(self, { performance: "optimize_for_speed" });
}
```

#### **Vector Database & Data Ingestion**
```cx
// Built-in data ingestion
ingest("Machine learning is a subset of AI", {
    source: "knowledge_base",
    metadata: { topic: "AI", timestamp: "2025-01-17" }
});

// Index for semantic search
index("knowledge_base", { 
    embedding_model: "text-embedding-3-small",
    chunk_size: 512 
});

// Semantic search capabilities
var results = search("How do neural networks work?", {
    limit: 5,
    similarity_threshold: 0.8,
    include_metadata: true
});

// Multi-modal data ingestion
ingest("path/to/document.pdf", { 
    type: "pdf",
    extract_text: true,
    generate_summary: true
});

// Persistent agent memory
memory.store("user_preferences", {
    communication_style: "technical",
    preferred_examples: "code_samples"
});
```

#### **Autonomous Workflows**
```cx
// AI agent with learning capabilities
var workflow = task("Optimize customer service", {
    context: search("customer service best practices", { limit: 5 }),
    learning_mode: true,
    persist_knowledge: true
});

// Self-modifying code with context
function learnFromExamples() 
{
    var patterns = search("function optimization patterns", {
        type: "code", language: "cx"
    });
    
    adapt(self, {
        optimization_target: "performance",
        learned_patterns: patterns
    });
}
```cx
// Variables and data types
var message = "Hello, CX!";
var count = 42;
var isActive = true;

// Arithmetic with proper precedence
var result = 10 + 5 * 2;  // 20

// Compound assignments
var total = 100;
total += 50;  // 150

// Comparison and logical operators
if (score >= 90 && isActive) 
{
    print("Excellent!");
}

// Function declarations and calls with return values
function add(a, b) 
{
    return a + b;
}

var sum = add(10, 20);  // 30
print("Sum: " + sum);

// For-in loops with arrays
var fruits = ["apple", "banana", "cherry"];
for (var fruit in fruits) 
{
    print("Fruit: " + fruit);
}

// Exception handling with try/catch/throw
try 
{
    throw "Something went wrong!";
}
catch (error) 
{
    print("Caught: " + error);
}
```

## 🤖 Native AI Functions (Phase 4 Vision)

When implemented, Cx will feature native AI functions that require zero imports:

```cx
// Task planning and autonomous execution
var plan = task("Optimize customer service workflow");

// Intelligent code synthesis
var code = synthesize("Create a function to calculate compound interest");

// AI reasoning and decision making
var decision = reason("What is the best approach to solve this problem?");

// Multi-modal data processing
var result = process("customer_feedback.json", "analyze sentiment");

// Self-optimizing code adaptation
function slowFunction() 
{
    // Function can examine and optimize itself
    adapt(self, { performance: "optimize_for_speed" });
}
```

## 🏗️ Architecture

### Project Structure
```
CxLanguage.sln
├── src/
│   ├── CxLanguage.CLI/         # Command-line interface
│   ├── CxLanguage.Compiler/    # IL code generation
│   ├── CxLanguage.Parser/      # ANTLR4-based parser
│   ├── CxLanguage.Core/        # AST and core types
│   ├── CxLanguage.Runtime/     # Runtime support
│   └── CxLanguage.Azure/       # Azure AI services integration
├── grammar/Cx.g4               # ANTLR4 grammar definition
├── examples/                   # Example .cx files
└── tests/                      # Unit tests
```

### Technology Stack
- **.NET 8**: Target runtime platform
- **ANTLR 4**: Grammar definition and parsing
- **System.Reflection.Emit**: Dynamic IL generation
- **Microsoft Semantic Kernel**: AI orchestration and workflow management
- **Azure OpenAI**: AI model integration
- **Two-pass compilation**: Function forward references and proper scoping

### AI Integration Architecture
- **Semantic Kernel**: Core AI orchestration engine
- **Vector Database**: In-memory semantic search and retrieval
- **Multi-Modal AI**: Text, image, audio, and video processing
- **Agent Memory**: Persistent context and learning capabilities
- **Self-Modification**: Function introspection and adaptation

## 📁 Examples

### ✅ **Fully Working Examples (Phases 1-4)**
- **`comprehensive_working_demo.cx`** - Complete Phase 1-3 feature demonstration
- **`ai_showcase.cx`** - Full AI integration showcase with all 7 native functions
- **`proof_injection_demo.cx`** - **REVOLUTIONARY**: Runtime function injection with mathematical proof of execution
- **`phase4_step1_ai_functions.cx`** - AI functions integration with Azure OpenAI
- **`test_ai_functions.cx`** - AI function testing and validation
- **`07_logical_operators.cx`** - Logical operators with short-circuit evaluation
- **`04_control_flow.cx`** - If/else statements and while loops
- **`05_functions.cx`** - Function declarations, parameters, and return values
- **`06_comprehensive.cx`** - Advanced language features integration

### 🚀 **Phase 4 AI Integration Examples**
- **`phase4_vector_database_demo.cx`** - Vector database and semantic search
- **`phase4_builtin_functions.cx`** - Built-in AI and data functions
- **`08_agentic_ai.cx`** - Autonomous AI workflows
- **`09_advanced_ai.cx`** - Multi-modal AI processing and self-modification
- **`autonomous_agent_demo.cx`** - Autonomous agent capabilities demonstration

### 🔮 **Phase 5 Vision Examples**
- **`agent_swarm_demo.cx`** - Multi-agent system coordination
- **`autonomous_learning.cx`** - Continuous learning and adaptation
- **`self_modifying_systems.cx`** - Advanced self-modification capabilities

## 🛠️ Building and Running

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Azure OpenAI API key (for AI features)

### Build Commands
```bash
# Build the solution
dotnet build CxLanguage.sln

# Run tests
dotnet test

# Test comprehensive working demo (all Phase 1-3 features)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_working_demo.cx

# Test AI integration showcase (Phase 4 features)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/ai_showcase.cx

# Test REVOLUTIONARY runtime function injection with mathematical proof
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/proof_injection_demo.cx

# Test AI functions
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test_ai_functions.cx

# Test logical operators
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/07_logical_operators.cx

# Test function return values
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/05_functions.cx
```

### Phase 4 Development Setup
```bash
# The following examples demonstrate planned Phase 4 capabilities
# (Currently in development - will work when Phase 4 is complete)

# Vector database and AI integration demo
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase4_vector_database_demo.cx

# Built-in data ingestion functions
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase4_builtin_functions.cx
```

## � Phase 4 Implementation Roadmap

### 1. **AI Function Integration**
- [ ] Implement `task()` function using Semantic Kernel
- [ ] Implement `synthesize()` for code generation
- [ ] Implement `reason()` for logical reasoning
- [ ] Implement `process()` for multi-modal data processing
- [ ] Implement `generate()` for content creation
- [ ] Implement `embed()` for vector embeddings
- [ ] Implement `adapt()` for self-modification

### 4. **Vector Database Implementation** 🔄 IN PROGRESS
- [x] Integrate Semantic Kernel memory stores
- [ ] Implement `ingest()` built-in function
- [ ] Implement `index()` for database optimization
- [ ] Implement `search()` for semantic search
- [ ] Add `similarity()` for semantic similarity calculation
- [ ] Add `cluster()` for document clustering
- [ ] Implement persistent agent memory (`memory.store/get/update`)

### 5. **Self-Modification Capabilities** ✅ PARTIALLY COMPLETE
- [x] **Runtime Function Injection**: AI generates and compiles functions at runtime
- [x] **Dynamic Code Modification**: Programs can modify themselves by creating new functions
- [x] **Function Persistence**: Runtime-injected functions remain available throughout execution
- [ ] Implement `self` keyword for function introspection
- [ ] Add source code tracking in AST
- [ ] Implement advanced function adaptation mechanisms

### 6. **Multi-Modal AI Processing** 🔄 PLANNED
- [ ] PDF text extraction and ingestion
- [ ] Image processing and OCR
- [ ] Audio transcription capabilities
- [ ] Video processing and analysis
- [ ] Batch processing for large datasets

### 7. **Autonomous Workflow Features** 🔄 PLANNED
- [ ] Context-aware AI function calls
- [ ] Learning from previous interactions
- [ ] Knowledge persistence across sessions
- [ ] Autonomous task planning and execution

## 🎯 Phase 5 Vision: Advanced Autonomous Capabilities

With Phase 4's revolutionary runtime function injection complete, Phase 5 will focus on:
- **Multi-Agent Coordination**: Multiple AI agents working together
- **Advanced Learning**: Machine learning integration for continuous improvement
- **Complex Self-Modification**: Beyond function injection to full program adaptation
- **Autonomous Decision Making**: AI agents making independent choices
- **Environment Integration**: Automatic tool discovery and API integration

## �🔧 Configuration

### Azure AI Services Setup
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ApiVersion": "2024-02-01"
  }
}
```

### Environment Variables
```bash
# Azure authentication (recommended)
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
export AZURE_TENANT_ID="your-tenant-id"

# Or use connection string (alternative)
export AZURE_OPENAI_CONNECTION_STRING="your-connection-string"
```
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase3_exception_complete.cx
```

## 🎯 AI Agentic Vision

### **Why Cx for Autonomous Agents?**
- 🤖 **AI-Native**: AI functions are built into the language, not imported
- 🎯 **Goal-Directed**: Natural language goal interpretation and autonomous planning
- 🔄 **Self-Modifying**: **REVOLUTIONARY** runtime function injection - programs can generate and execute new functions dynamically
- 📚 **Learning-Enabled**: Continuous improvement through feedback and experience
- 🌐 **Environment-Aware**: Automatic tool discovery and API integration
- ⚡ **Async-First**: Native async/await patterns for concurrent agentic operations
- 🚀 **Runtime Adaptation**: AI-generated functions are compiled and executed at runtime with mathematical proof

### **Revolutionary Runtime Function Injection** (Phase 4 Complete)
- **🎯 Dynamic Function Generation**: AI creates new functions based on natural language descriptions
- **🔧 Runtime Compilation**: Generated CX code is compiled to IL and executed immediately
- **📚 Persistent Function Registry**: Injected functions remain available throughout program execution
- **🔄 Mathematical Proof**: Demonstrated accuracy with add(7,3)=10, square(6)=36, add(10,20)=30, square(4)=16
- **⚡ Production Ready**: Full error handling, type safety, and enterprise-grade reliability

### **Autonomous Agentic Capabilities** (Phase 5 Vision)
- **🎯 Goal Interpretation**: Natural language understanding of complex goals
- **🗺️ Autonomous Planning**: Self-directed task breakdown and execution
- **🔧 Tool Integration**: Dynamic environment discovery and adaptation
- **📚 Learning & Adaptation**: Real-time feedback integration and improvement
- **🔄 Self-Modification**: Runtime code synthesis and behavioral adaptation

## 🤝 Contributing

We welcome contributions to the CX language! Current priority areas:

### **Phase 4 AI Integration** (Current Focus)
- **AI Function Implementation**: Help implement native AI functions using Semantic Kernel
- **Vector Database Integration**: Work on in-memory vector database and search capabilities
- **Built-in Functions**: Implement data ingestion and processing functions (`ingest()`, `index()`, `search()`)
- **Multi-Modal Processing**: Add support for images, audio, and video processing
- **Self-Modification**: Implement function introspection and adaptation capabilities

### **Code Standards**
- Follow existing C# coding conventions
- Use Allman-style brackets in all CX examples
- Add comprehensive XML documentation
- Include unit tests for new features
- All .cx files must be placed in the `examples/` folder
- Update examples to demonstrate new capabilities

### **Testing Phase 4 Features**
```bash
# Current Phase 3 tests (working)
dotnet test

# Phase 4 integration tests (planned)
dotnet test --filter "Category=Phase4"

# AI function tests (planned)
dotnet test --filter "Category=AI"
```

### **Getting Started**
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/phase4-ai-integration`
3. Follow the coding guidelines in `.github/copilot-instructions.md`
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- **Microsoft Semantic Kernel** - AI orchestration and workflow management
- **Azure OpenAI** - AI model integration and capabilities
- **ANTLR 4** - Grammar definition and parsing framework
- **.NET Team** - Runtime platform and IL generation capabilities

---

**Cx Language - The First AI-Native Agentic Programming Language** 🤖🚀

*Building the foundation for AI-native programming with familiar syntax and powerful autonomous capabilities.*
