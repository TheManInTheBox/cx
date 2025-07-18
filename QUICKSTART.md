# Cx Language Quick Start Guide

Welcome to Cx Language - **the world's first AI-native agentic programming language!** 🤖

**🎉 Phase 4 Complete**: All AI functions are now operational with enterprise-grade monitoring and **revolutionary runtime function injection**!

## Prerequisites

- .NET 8 SDK or later
- Windows, macOS, or Linux
- Azure OpenAI API key (for AI features)

## Installation

1. **Clone the repository:**
   ```powershell
   git clone https://github.com/ahebert-lt/cx.git
   cd cx
   ```

2. **Build the project:**
   ```powershell
   dotnet build
   ```

3. **Verify installation:**
   ```powershell
   cd src/CxLanguage.CLI
   dotnet run -- run ../../examples/comprehensive_working_demo.cx
   ```
   
   You should see a comprehensive demo of all working features!

## Your First Cx Program

Create a file called `hello.cx`:

```cx
var message = "Hello, Cx Language!"
var number = 42

print(message)
print("The answer is:")
print(number)

if (number > 40) 
{
    print("That's a big number!")
}
```

Run it:
```powershell
dotnet run -- run hello.cx
```

## AI Functions - What Makes Cx Special ✨

Cx Language features **7 native AI functions** built into the language core, including **revolutionary runtime function injection**:

### Your First AI Program

Create `my_first_ai.cx`:
```cx
// AI functions are built into the language!
var analysis = reason("What are the benefits of renewable energy?")
print("AI Analysis:")
print(analysis)

var summary = synthesize("Create a brief summary of artificial intelligence")
print("AI Summary:")
print(summary)

var creative = generate("Write a short poem about programming")
print("AI Creativity:")
print(creative)
```

### **REVOLUTIONARY: Runtime Function Injection**

Create `function_injection_demo.cx`:
```cx
// AI generates, compiles, and injects functions at runtime!
var result = adapt("Create a function to add two numbers", {
    type: "function",
    name: "add",
    parameters: ["a", "b"],
    returnType: "number"
})

// Generated function is now available for immediate use
var sum = add(7, 3)  // Returns 10
print("Sum: " + sum)

// Generate complex mathematical functions
var squareResult = adapt("Create a function to square a number", {
    type: "function",
    name: "square",
    parameters: ["x"],
    returnType: "number"
})

var squared = square(6)  // Returns 36
print("Squared: " + squared)

// Functions persist throughout execution
var anotherSum = add(10, 20)  // Still works: returns 30
var anotherSquare = square(4)  // Still works: returns 16

print("Another Sum: " + anotherSum)
print("Another Square: " + anotherSquare)
```

#### What Just Happened?
1. **AI Generated Code**: The `adapt()` function used AI to generate CX functions
2. **Runtime Compilation**: Generated code was compiled to IL and loaded into memory
3. **Immediate Execution**: Functions became available for use instantly
4. **Persistent Functions**: Generated functions remain available throughout the program

This is the world's first programming language with **runtime function injection** - where AI doesn't just help you write code, but actively writes and executes new functions as your program runs!

### AI Configuration Setup

Create `appsettings.json`:
```json
{
  "AzureOpenAI": {
    "ApiKey": "your-api-key-here",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini"
  }
}
```

Run your AI program:
```powershell
dotnet run -- run my_first_ai.cx
```

Run the **REVOLUTIONARY** runtime function injection demo:
```powershell
dotnet run -- run examples/proof_injection_demo.cx
```

## Language Basics

### Variables
```cx
// Declarations require 'var' keyword
var name = "Alice"
var age = 25
var isActive = true

// Assignments update existing variables
age = 26
name = "Bob"
```

### Arithmetic
```cx
var a = 10
var b = 3

var sum = a + b        // 13
var difference = a - b // 7
var product = a * b    // 30
var quotient = a / b   // 3
```

### Comparisons
```cx
var x = 5
var y = 10

var equal = x == y     // false
var less = x < y       // true
var greater = x > y    // false
```

### Control Flow
```cx
// If/else statements
if (age >= 18) 
{
    print("Adult")
} 
else 
{
    print("Minor")
}

// While loops
var i = 0
while (i < 5) {
    print(i)
    i = i + 1
}
```

### Functions
```cx
function greet() {
    print("Hello from a function!")
}

greet()  // Call the function
```

## Examples

Check out the `examples/` directory for more comprehensive programs:

- `01_basic_variables.cx` - Variable declarations and assignments
- `02_arithmetic.cx` - Mathematical operations
- `03_comparisons.cx` - Comparison operators
- `04_control_flow.cx` - If/else and while loops
- `05_functions.cx` - Function definitions and calls
- `06_comprehensive.cx` - Complete game simulation

## Running Examples

```powershell
# Navigate to CLI directory
cd src/CxLanguage.CLI

# Run any example
dotnet run -- run ../../examples/01_basic_variables.cx
dotnet run -- run ../../examples/06_comprehensive.cx
```

## Testing Your Installation

Run the test suite to verify everything works:

```powershell
# Run tests (from project root)
.\scripts\run_tests.ps1
```

## Common Issues

### Build Errors
- Make sure you have .NET 8 SDK installed
- Try `dotnet clean` then `dotnet build`

### Runtime Errors
- Check that you're using `var` for new variables
- Verify file paths are correct
- Make sure you're in the correct directory

### Grammar Changes
If you modify the grammar file (`grammar/Cx.g4`), regenerate the parser:
```powershell
cd src/CxLanguage.Parser
antlr4 -Dlanguage=CSharp ../../grammar/Cx.g4 -visitor -no-listener -package CxLanguage.Parser.Generated
```

## What's Next?

1. **Explore Examples**: Try all the example programs
2. **Write Your Own**: Create simple programs using current features
3. **Read Documentation**: Check `docs/syntax.md` for complete syntax reference
4. **Contribute**: Help implement new features like logical operators (`&&`, `||`)

## Getting Help

- Check the `README.md` for comprehensive documentation
- Look at `docs/syntax.md` for syntax reference
- Examine example programs for usage patterns
- Report issues on GitHub

Happy coding with Cx! 🚀
