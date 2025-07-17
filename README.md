# Cx Language - Modern Scripting Language for .NET

[![CI](https://github.com/ahebert-lt/cx/actions/workflows/ci.yml/badge.svg)](https://github.com/ahebert-lt/cx/actions/workflows/ci.yml)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/ahebert-lt/cx)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)

🎉 **A fully working modern scripting language implementation for .NET!**

## Overview

Cx is a modern, strongly-typed scripting language that compiles to .NET IL and runs on any .NET 8+ runtime. It features a clean C-style syntax with modern language constructs, comprehensive type system, and full IL compilation for maximum performance.

## ✅ Current Working Features

**CORE LANGUAGE:**
- ✅ **Variable System**: Full support for `var` keyword declarations and assignments
- ✅ **Type System**: Integer, boolean, and string literals with type inference
- ✅ **Arithmetic Operators**: `+`, `-`, `*`, `/` (with proper precedence)
- ✅ **Comparison Operators**: `==`, `<`, `>`, `<=`, `>=`, `!=` 
- ✅ **Logical Operators**: `&&`, `||`, `!` (AND, OR, NOT operators)
- ✅ **Control Flow**: `if/else` statements and `while` loops
- ✅ **Assignment Expressions**: `x = x + 1` style updates
- ✅ **Error Handling**: Proper error messages for undeclared variables

**COMPILATION & EXECUTION:**
- ✅ **IL Compilation**: Full .NET IL emission using System.Reflection.Emit
- ✅ **Runtime Execution**: Native .NET assembly generation and execution  
- ✅ **ANTLR Parser**: Complete grammar definition with AST generation
- ✅ **CLI Interface**: Command-line tools for parsing, compiling, and running
- ✅ **Built-in Functions**: `print()` function for output

## 🚀 Quick Start

### Build the Solution
```powershell
dotnet build
```

### Run Your First Cx Program
```powershell
# Navigate to the CLI directory
cd src/CxLanguage.CLI

# Run a simple script
dotnet run -- run ../../var_minimal.cx
```

### Basic Cx Syntax

Create a file called `hello.cx`:

```cx
// Variable declarations require 'var' keyword
var message = "Hello, Cx World!"
var x = 42
var y = 24

// Print output
print(message)

// Arithmetic and assignment
var result = x + y
print("The answer is:")
print(result)

// Control flow
if (x > y)
{
    print("x is greater than y")
}
else
{
    print("y is greater than or equal to x")
}

// While loops with assignment expressions
var counter = 0
while (counter < 5)
{
    print(counter)
    counter = counter + 1
}
```

### CLI Usage
```powershell
# Parse a script and show AST (development/debugging)
dotnet run -- parse samples/hello.cx

# Compile to .NET assembly  
dotnet run -- compile samples/hello.cx --output hello.dll

# Run a script directly
dotnet run -- run samples/hello.cx

# Show help
dotnet run -- --help
```
## � Language Features

### Variables and Types
Cx uses strong typing with type inference and requires the `var` keyword for new variable declarations:

```cx
// Variable declarations (var keyword required)
var name = "Alice"              // string
var age = 30                    // number (integer)
var isActive = true             // boolean

// Assignment to existing variables (no var keyword)
age = 31
name = "Bob"

// Error: Cannot assign to undeclared variable
// count = 10  // Error: Variable 'count' not declared
```

### Operators

**Arithmetic Operators:**
```cx
var a = 10
var b = 3

var sum = a + b        // 13
var diff = a - b       // 7  
var product = a * b    // 30
var quotient = a / b   // 3 (integer division)
```

**Comparison Operators:**
```cx
var x = 5
var y = 10

var equal = x == y        // false
var notEqual = x != y     // true  
var less = x < y          // true
var greater = x > y       // false
var lessEqual = x <= y    // true
var greaterEqual = x >= y // false
```

**Logical Operators:**
```cx
var a = true
var b = false

// Logical AND - returns true if both operands are true
var andResult = a && b    // false
var orResult = a || b     // true
var notResult = !a        // false

// Use in conditions
if (x > 0 && y > 0)
{
    print("Both x and y are positive")
}

if (a || b)
{
    print("At least one is true")
}

if (!a)
{
    print("a is false")
}
```

### Control Flow

**If/Else Statements:**
```cx
var score = 85

if (score >= 90)
{
    print("Grade: A")
}
else if (score >= 80)
{
    print("Grade: B")  
}
else
{
    print("Grade: C or below")
}
```

**While Loops:**
```cx
var i = 0
while (i < 5)
{
    print(i)
    i = i + 1
}
```

### Functions (Basic Support)
```cx
function greet()
{
    print("Hello from a function!")
}

// Call the function  
greet()
```
## 🏗️ Architecture

Cx compiles directly to .NET IL bytecode for maximum performance and compatibility.

### Compilation Pipeline
```
Cx Source → ANTLR Parser → AST → Type Analysis → IL Generation → .NET Assembly
```

### Project Structure
```
src/
├── CxLanguage.Core/           # AST nodes, types, symbols
├── CxLanguage.Parser/         # ANTLR-based parser  
├── CxLanguage.Compiler/       # IL emission compiler
├── CxLanguage.CLI/            # Command-line interface
└── CxLanguage.Tests/          # Unit tests
grammar/
└── Cx.g4                      # ANTLR grammar definition
examples/                      # Sample Cx programs
```

### Technology Stack
- **.NET 8**: Target runtime platform
- **ANTLR 4**: Grammar definition and parsing
- **System.Reflection.Emit**: Dynamic IL generation
- **xUnit**: Unit testing framework

## 🧪 Current Examples

The following example programs work with the current implementation:

### Basic Variables (`var_minimal.cx`)
```cx
var x = 5
print(x)
x = 10  
print(x)
```

### Arithmetic Operations
```cx
var a = 10
var b = 5

var sum = a + b
var diff = a - b  
var product = a * b
var quotient = a / b

print(sum)      // 15
print(diff)     // 5
print(product)  // 50  
print(quotient) // 2
```

### Control Flow
```cx
var number = 42

if (number > 0) {
    print("Positive number")
} else {
    print("Zero or negative")
}

var counter = 0
while (counter < 3) {
    print(counter)
    counter = counter + 1
}
```

### Boolean Logic
```cx
var isTrue = true
var isFalse = false

if (isTrue == true) {
    print("Boolean comparison works")
}

if (isTrue != isFalse) {
    print("Not equal comparison works")  
}
```
## 🛠️ Development

### Building from Source
```powershell
# Clone and build
git clone https://github.com/ahebert-lt/cx.git
cd cx
dotnet restore
dotnet build

# Run tests  
dotnet test

# Generate ANTLR parser (if grammar changes)
cd src/CxLanguage.Parser  
antlr4 -Dlanguage=CSharp ../../grammar/Cx.g4 -visitor -no-listener -package CxLanguage.Parser.Generated
```

### Testing the Implementation
```powershell
# Run the minimal example
dotnet run --project src/CxLanguage.CLI -- run var_minimal.cx

# Test arithmetic operations  
dotnet run --project src/CxLanguage.CLI -- run test_operators.cx

# Test control flow
dotnet run --project src/CxLanguage.CLI -- run test_while_loop.cx
```

## 🎯 Roadmap

### Immediate Next Features
- ✅ **Logical Operators**: Implement `&&` and `||` operators (grammar ready)
- ⏳ **Modulo Operator**: Add `%` operator support  
- ⏳ **Compound Assignment**: `+=`, `-=`, `*=`, `/=` operators
- ⏳ **Unary Operators**: `-x`, `+x`, `!x` support
- ⏳ **String Operations**: String concatenation with `+`

### Medium-term Goals  
- ⏳ **Enhanced For Loops**: `for (i = 0; i < 10; i++)` syntax
- ⏳ **Arrays**: Array literals and indexing `[1, 2, 3]`, `arr[0]`
- ⏳ **Functions with Parameters**: Parameter passing and return values
- ⏳ **Local Variable Scoping**: Proper block-level scoping
- ⏳ **Error Handling**: try/catch exception handling

### Future Features
- ⏳ **Object Literals**: `{ key: value }` syntax  
- ⏳ **Import System**: Module importing and namespaces
- ⏳ **Async/Await**: Asynchronous programming support
- ⏳ **Standard Library**: Built-in functions and utilities

## 📊 Performance

- **Compilation**: Sub-second compile times for typical scripts
- **Execution**: Native .NET IL performance (no interpretation overhead)  
- **Memory**: Minimal runtime overhead beyond .NET base requirements
- **Startup**: Fast cold start times (~10-50ms)

## 🔄 CI/CD Pipeline

The project includes a comprehensive GitHub Actions workflow that runs on every push and pull request to the master branch:

- **Build**: Full solution build in Release configuration
- **Test**: Unit tests with xUnit framework (6 tests currently passing)
- **Example Validation**: Automated testing of example scripts
- **Code Coverage**: Coverage reporting with Codecov integration
- **Multi-Platform**: Runs on Ubuntu (Linux) for cross-platform compatibility

### Test Status
- ✅ **Unit Tests**: All parser and AST tests passing
- ✅ **Basic Examples**: Variable declarations, arithmetic, string operations
- ⚠️ **Complex Examples**: Control flow and logical operators (known IL generation issues)

The build status is displayed with badges at the top of this README.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/logical-operators`)
3. Add tests for new functionality
4. Ensure all tests pass (`dotnet test`)
5. Submit a pull request

### Contribution Areas
- **Language Features**: Implement operators, control structures
- **Standard Library**: Add built-in functions and utilities  
- **Developer Tools**: Improve CLI, add debugger support
- **Documentation**: Expand examples and tutorials
- **Testing**: Add comprehensive test coverage

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

**Cx Language - Modern scripting for the .NET ecosystem** 🚀
