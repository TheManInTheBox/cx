# Cx - AI-Native Agentic Programming Language

[![CI](https://github.com/ahebert-lt/cx/actions/workflows/ci.yml/badge.svg)](https://github.com/ahebert-lt/cx/actions/workflows/ci.yml)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/ahebert-lt/cx)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)

**Cx** is a revolutionary programming language where **AI functions are native, not imported**. Designed specifically for autonomous agentic workflows, Cx enables agents to interpret goals, plan actions, learn from feedback, and modify their own behavior dynamically.

## 🚀 Quick Start

```bash
# Build the project
dotnet build CxLanguage.sln

# Test current working features (all Phase 1-3 features working)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase3_for_in_complete.cx

# Test for-in loops (Phase 3 complete)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test_for_in_simple.cx

# Test exception handling (Phase 3 complete)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase3_exception_complete.cx
```

## 📊 Development Status

### ✅ **Completed Phases**
- **Phase 1**: Core language (variables, arithmetic, control flow, logical operators)
- **Phase 2**: Function system (two-pass compilation, parameters, calls)
- **Phase 3**: Advanced features (function return values, for-in loops, exception handling)

### 🎉 **Phase 3 Major Achievements**
- **✅ Function Return Values**: Non-void functions with proper IL generation
- **✅ For-in Loops**: Complete array iteration with variable scoping
- **✅ Exception Handling**: Full try/catch/throw implementation with .NET integration
- **✅ Nested Structures**: Support for nested loops, exceptions, and function calls
- **✅ Allman-style Brackets**: Consistent code formatting across all examples

### 🔄 **Current Focus: Phase 3 Final Features**
- **Enhanced Data Structures**: Object literals and advanced array features
- **Class System**: Classes, inheritance, and object-oriented programming
- **Module System**: Imports and exports

### 🔮 **Future Phases**
- **Phase 4**: Native AI function integration (`task`, `synthesize`, `reason`)
- **Phase 5**: Autonomous agentic capabilities and self-modification

## 🛠️ Current Language Features

### ✅ **Working Features (Phases 1-3)**
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
│   └── CxLanguage.Runtime/     # Runtime support
├── grammar/Cx.g4               # ANTLR4 grammar definition
├── examples/                   # Example .cx files
└── tests/                      # Unit tests
```

### Technology Stack
- **.NET 8**: Target runtime platform
- **ANTLR 4**: Grammar definition and parsing
- **System.Reflection.Emit**: Dynamic IL generation
- **Two-pass compilation**: Function forward references and proper scoping

## 📁 Examples

### ✅ **Fully Working Examples**
- **`phase3_for_in_complete.cx`** - Complete for-in loop demonstrations
- **`phase3_exception_complete.cx`** - Complete exception handling examples
- **`phase3_complete_fixed.cx`** - Complete Phase 3 return values demo
- **`test_for_in_simple.cx`** - Basic for-in loop examples
- **`test_exception_handling.cx`** - Exception handling tests
- **`comprehensive_working_demo.cx`** - All working Phase 1-2 features
- **`07_logical_operators.cx`** - Logical operators demonstration
- **`04_control_flow.cx`** - If/else statements and while loops

### 🔮 **Future Examples**
- **`08_agentic_ai.cx`** - AI function showcase (Phase 4 planned)
- **`09_advanced_ai.cx`** - Advanced AI capabilities (Phase 5 vision)

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

# Run current examples
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase3_for_in_complete.cx

# Test for-in loops
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/test_for_in_simple.cx

# Test exception handling
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase3_exception_complete.cx
```

## 🎯 AI Agentic Vision

### **Why Cx for Autonomous Agents?**
- 🤖 **AI-Native**: AI functions are built into the language, not imported
- 🎯 **Goal-Directed**: Natural language goal interpretation and autonomous planning
- 🔄 **Self-Modifying**: Dynamic code synthesis and behavioral adaptation at runtime
- 📚 **Learning-Enabled**: Continuous improvement through feedback and experience
- 🌐 **Environment-Aware**: Automatic tool discovery and API integration
- ⚡ **Async-First**: Native async/await patterns for concurrent agentic operations

### **Autonomous Agentic Capabilities** (Phase 5 Vision)
- **🎯 Goal Interpretation**: Natural language understanding of complex goals
- **🗺️ Autonomous Planning**: Self-directed task breakdown and execution
- **🔧 Tool Integration**: Dynamic environment discovery and adaptation
- **📚 Learning & Adaptation**: Real-time feedback integration and improvement
- **🔄 Self-Modification**: Runtime code synthesis and behavioral adaptation

## 🤝 Contributing

We welcome contributions to the CX language! Current priority areas:

### **Phase 3 Advanced Features** (Current Focus)
- **Enhanced Data Structures**: Object literals and advanced array features
- **Class System**: Object-oriented programming features
- **Module System**: Imports and exports

**Major Breakthroughs Achieved:**
- ✅ **For-in Loops**: Complete array iteration with proper scoping
- ✅ **Exception Handling**: Full try/catch/throw implementation
- ✅ **Function Return Values**: Non-void functions working perfectly

### **Code Standards**
- Follow C# best practices for backend code
- Use Allman-style brackets in CX example files
- Add XML documentation for public APIs
- All .cx files must be placed in the `examples/` folder

### **Getting Started**
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/new-language-feature`
3. Follow the coding guidelines in `.github/copilot-instructions.md`
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

**Cx Language - The First AI-Native Agentic Programming Language** 🤖🚀

*Building the foundation for AI-native programming with familiar syntax and powerful autonomous capabilities.*
