# Cx - AI-Native Agentic Programming Language

[![CI](https://github.com/ahebert-lt/cx/actions/workflows/ci.yml/badge.svg)](https://github.com/ahebert-lt/cx/actions/workflows/ci.yml)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/ahebert-lt/cx)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)

## 🤖 The First AI-Native Agentic Programming Language

**Cx** is a revolutionary programming language where **AI functions are native, not imported**. Designed specifically for autonomous agentic workflows, Cx enables agents to interpret goals, plan actions, learn from feedback, and modify their own behavior dynamically.

## 🚀 Quick Start

```bash
# Build the project
dotnet build CxLanguage.sln

# Run AI agentic examples (planned - Phase 4)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/08_agentic_ai.cx

# Test current working features (✅ All Phase 1 features working)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_working_demo.cx

# Test function system (❌ Currently failing compilation)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/05_functions.cx
```

## 📊 Current Development Status

### ✅ **Stable & Production Ready**
- **Phase 1 Core Language**: All features complete and thoroughly tested
- **Build System**: Compiles successfully with .NET 8
- **Parser**: ANTLR4 grammar handles all current syntax correctly  
- **Examples**: Comprehensive working demo showcases all stable features

### ⚠️ **In Active Development** 
- **Phase 2 Function System**: Parsing complete, IL compilation failing
- **Critical Issue**: Function declarations cause "The invoked member is not supported before the type is created" error
- **Next Priority**: Fix function IL generation in `CxCompiler.cs`

### 🔮 **Planned Features**
- **Phase 3**: Advanced language constructs (arrays, objects, exceptions)
- **Phase 4**: Native AI function integration
- **Phase 5**: Autonomous agentic capabilities

## 🤖 Native AI Functions - Zero Imports Required

### **Built-in Autonomous Agentic Functions** (Planned - Phase 4)

```cx
// Task planning and autonomous execution
var plan = task("Optimize customer service workflow and reduce response times");

// Intelligent code synthesis
var code = synthesize("Create a function to calculate compound interest");

// AI reasoning and decision making
var decision = reason("What is the best approach to solve this problem?");

// Multi-modal data processing
var result = process("customer_feedback.json", "analyze sentiment and trends");

// Content generation across modalities
var content = generate("Create a marketing email for new product launch");

// Vector embeddings for semantic search
var embedding = embed("Convert this text to vector representation");

// Self-optimizing code adaptation
function slowFunction() 
{
    // Function can examine and optimize itself
    adapt(self, { performance: "optimize_for_speed" });
}
```

### **Autonomous Workflow Example** (Planned - Phase 4)

```cx
// Complete autonomous workflow - no human intervention required
async function autonomousAgent() 
{
    // 1. Goal interpretation and planning
    var goal = "Improve customer satisfaction scores by 15% within 30 days";
    var plan = await task(goal, {
        autonomous: true,
        learning_enabled: true,
        max_subtasks: 10
    });
    
    // 2. Dynamic tool discovery and integration
    var tools = await reason("Discover available customer service tools and APIs");
    
    // 3. Multi-step execution with adaptation
    for (step in plan.steps) 
    {
        var result = await process(step.data, step.action);
        
        // 4. Learning and performance optimization
        if (result.performance < 0.8) 
        {
            adapt(step.action, {
                feedback: result,
                optimize_for: "accuracy"
            });
        }
    }
    
    // 5. Self-assessment and reporting
    var assessment = await reason("Evaluate goal achievement and next steps");
    return assessment;
}

// Agent runs completely autonomously
await autonomousAgent();
```

## 🎯 Development Roadmap

### ✅ **Phase 1: Core Language Foundation** (COMPLETED)
- ✅ Variables and data types (string, integer, boolean)
- ✅ Arithmetic operations with proper precedence
- ✅ Comparison and logical operators (`==`, `!=`, `<`, `>`, `<=`, `>=`, `&&`, `||`, `!`)
- ✅ Control flow (if/else, while loops)
- ✅ String concatenation and basic operations
- ✅ Compound assignment operators (`+=`, `-=`, `*=`, `/=`)

### 🔄 **Phase 2: Function System** (IN PROGRESS - Current Priority)
- ✅ Function declaration syntax and parsing (working)
- ❌ Function IL compilation (failing - critical issue)
- ⏳ Function parameters and return values
- ⏳ Function call mechanism fixes
- ⏳ Proper variable scoping
- ⏳ Function overloading support

**Current Issue**: Function declarations parse correctly but fail during IL code generation with "The invoked member is not supported before the type is created" error. This is the critical blocker preventing Phase 2 completion.

### 📋 **Phase 3: Advanced Language Features** (PLANNED)
- 📋 For-in loops and iterators
- 📋 Exception handling (try/catch/throw)
- 📋 Array and object literals
- 📋 Class system and inheritance
- 📋 Module system and imports

### 🤖 **Phase 4: AI Integration** (PLANNED)
- 📋 `task()` function for autonomous planning
- 📋 `synthesize()` function for code generation
- 📋 `reason()` function for AI decision making
- 📋 `process()` function for data transformation
- 📋 `generate()` function for content creation
- 📋 `embed()` function for vector embeddings
- 📋 `adapt()` function for self-optimization
- 📋 `self` keyword for function introspection

### 🚀 **Phase 5: Autonomous Agentic Features** (FUTURE)
- 📋 Multi-agent coordination and swarms
- 📋 Learning and adaptation mechanisms
- 📋 Environment interaction and tool discovery
- 📋 Self-modifying code capabilities
- 📋 Goal interpretation from natural language

## 🛠️ Current Working Features (Phase 1 Complete)

### ✅ Fully Working Core Language Features
- **Variables and Data Types**: String, integer, and boolean support with proper type handling
- **Arithmetic Operations**: Addition, subtraction, multiplication, division with correct operator precedence  
- **Compound Assignment**: `+=`, `-=`, `*=`, `/=` operators working correctly
- **Comparison Operations**: `==`, `!=`, `<`, `>`, `<=`, `>=` all functional
- **Logical Operators**: `&&`, `||`, `!` with proper boolean logic and short-circuiting
- **Control Flow**: If/else statements and while loops with nested support
- **String Operations**: Concatenation, formatting, and variable interpolation

### ⚠️ Known Issues (Phase 2)
- **Function System**: Function declarations parse correctly but fail during IL compilation
- **Function Calls**: User-defined function calls not working due to compilation issues
- **Variable Scoping**: Global vs local variable scope needs implementation within functions

### ✅ Verified Working Examples

```cx
// Core language features that work today
var message = "Hello, CX Language!";
var count = 42;
var isActive = true;

// Arithmetic with proper precedence
var a = 20;
var b = 8;
var result = a + b * 2;  // 36 (proper precedence)

// Compound assignments
var total = 100;
total += 50;  // 150
total *= 2;   // 300

// Control flow
if (score >= 90) 
{
    print("Excellent!");
} 
else if (score >= 80) 
{
    print("Good job!");
}

// While loops
var counter = 5;
while (counter > 0) 
{
    print("Count: " + counter);
    counter = counter - 1;
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
│   └── CxLanguage.Runtime/     # Runtime support
├── grammar/
│   └── Cx.g4                   # ANTLR4 grammar definition
├── examples/                   # Example .cx files
└── tests/                      # Unit tests
```

### Key Components
- **Grammar**: ANTLR4-based parser with JavaScript/TypeScript-like syntax
- **AST Builder**: Converts parse trees to abstract syntax trees
- **IL Compiler**: Generates .NET IL code from AST nodes
- **Runtime**: .NET-based execution environment

### Technology Stack
- **.NET 8**: Target runtime platform
- **ANTLR 4**: Grammar definition and parsing
- **System.Reflection.Emit**: Dynamic IL generation
- **xUnit**: Unit testing framework

## 🛠️ Building and Running

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Build Commands
```bash
# Build the solution
dotnet build CxLanguage.sln

# Run tests
dotnet test

# Run a CX script (✅ Working examples)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/comprehensive_working_demo.cx

# Test function system (❌ Currently failing)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/05_functions.cx

# Parse and show AST (for debugging)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj parse examples/test_arithmetic.cx
```

## 📁 Example Files

### ✅ Fully Working Examples (Phase 1)
- **`comprehensive_working_demo.cx`** - Complete showcase of all working Phase 1 features (recommended starting point)
- **`01_basic_variables.cx`** - Variable declarations and data types
- **`02_arithmetic.cx`** - Arithmetic operations and operator precedence
- **`test_assignment_operators.cx`** - Compound assignment operators
- **`07_logical_operators.cx`** - Logical operators demonstration
- **`04_control_flow.cx`** - If/else statements and while loops

### ⚠️ In Development Examples (Phase 2)
- **`05_functions.cx`** - Function system (parsing works, compilation fails)
- **`simple_function.cx`** - Basic function examples (compilation issues)

### 🔮 Future Examples (Phases 3-5)
- **`08_agentic_ai.cx`** - AI function showcase (Phase 4 planned)
- **`09_advanced_ai.cx`** - Advanced AI capabilities (Phase 5 vision)

### 📊 Current Test Status
The **`comprehensive_working_demo.cx`** file demonstrates all working Phase 1 features:
1. ✅ Variable declarations and data types
2. ✅ Arithmetic operations with precedence  
3. ✅ Compound assignment operators
4. ✅ Comparison operations
5. ✅ Logical operators and boolean logic
6. ✅ If/else control structures with nesting
7. ✅ While loops and iterative logic
8. ✅ String concatenation and formatting

**Function examples currently fail compilation** but parsing works correctly, making function system IL generation the #1 development priority.

## 🎯 AI Agentic Vision

### **Why Cx for Autonomous Agents?**

- 🤖 **AI-Native**: AI functions are built into the language, not imported
- 🎯 **Goal-Directed**: Natural language goal interpretation and autonomous planning
- 🔄 **Self-Modifying**: Dynamic code synthesis and behavioral adaptation at runtime
- 📚 **Learning-Enabled**: Continuous improvement through feedback and experience
- 🌐 **Environment-Aware**: Automatic tool discovery and API integration
- ⚡ **Async-First**: Native async/await patterns for concurrent agentic operations

### **Autonomous Agentic Capabilities** (Phase 5 Vision)

**🎯 Goal Interpretation from Natural Language:**
- Native understanding of complex goals and requirements
- Automatic decomposition into executable action plans
- Context-aware goal refinement and clarification

**🗺️ Autonomous Planning & Sequencing:**
- Self-directed task breakdown without human intervention
- Intelligent dependency resolution and execution ordering
- Adaptive replanning when conditions change

**🔧 Tool & Environment Interaction:**
- Native integration with APIs, services, and external tools
- Dynamic environment discovery and adaptation
- Seamless multi-modal data processing

**📚 Learning & Adaptation:**
- Real-time feedback integration and behavioral adjustment
- Performance optimization based on execution history
- Self-improving algorithms and decision-making patterns

**🔄 Dynamic Self-Modification:**
- Runtime code synthesis and behavioral adaptation
- Autonomous debugging and error correction
- Self-optimizing execution paths

## 🤝 Contributing

We welcome contributions to the CX language! Here's how to get started:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/new-language-feature`
3. **Follow the coding guidelines** in `.github/copilot-instructions.md`
4. **Add tests** for new functionality
5. **Place example files** in the `examples/` folder
6. **Ensure all tests pass**: `dotnet test`
7. **Submit a pull request**

### Contribution Areas
- **🔥 Priority: Function System** (Phase 2): Fix IL compilation for function declarations - critical blocker
- **Core Language Features**: Parser enhancements, error handling improvements  
- **Compiler Enhancement**: IL generation optimization, function call implementation
- **AI Integration** (Phase 4): Implementation of task, synthesize, reason functions
- **Documentation**: Examples, tutorials, API documentation  
- **Testing**: Unit tests, integration tests, example verification

### Current Critical Issues Needing Help
1. **Function IL Compilation**: Fix "The invoked member is not supported before the type is created" error in `CxCompiler.cs`
2. **Function Call Mechanism**: Implement proper function call IL generation
3. **Variable Scoping**: Add local vs global variable scope within functions

### Coding Standards
- Follow C# best practices for backend code
- Use Allman-style brackets in CX example files
- Add XML documentation for public APIs
- All .cx files must be placed in the `examples/` folder

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

**Cx Language - The First AI-Native Agentic Programming Language** 🤖🚀

*Building the foundation for AI-native programming with familiar syntax and powerful autonomous capabilities.*

## 📋 Recent Updates

### Latest Development Activity
- ✅ **Phase 1 Complete**: All core language features working and tested
- ✅ **Documentation**: README consolidated and updated with accurate status
- ✅ **Code Standards**: Allman-style bracket formatting enforced across all examples
- ✅ **Function Syntax**: Function declaration parsing implemented and working
- ⚠️ **Current Issue**: Function IL compilation failing - critical Phase 2 blocker

### What's Working Now (Tested & Verified)
- Variable declarations, assignments, and all data types
- Complete arithmetic operations with proper precedence
- All comparison and logical operators  
- Control flow (if/else, while loops) with nesting
- String operations and concatenation
- Compound assignment operators

### Next Immediate Goals
1. **Fix Function IL Compilation** - Resolve the "type creation" error
2. **Implement Function Calls** - Enable calling user-defined functions  
3. **Add Variable Scoping** - Local vs global variable handling
4. **Function Parameters** - Parameter passing and return values

*For the latest development status and to contribute, see the [GitHub repository](https://github.com/ahebert-lt/cx).*
