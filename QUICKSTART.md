# Cx Language Quick Start Guide

Welcome to Cx Language! This guide will get you up and running in just a few minutes.

## Prerequisites

- .NET 8 SDK or later
- Windows, macOS, or Linux

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
   dotnet run -- run ../../var_minimal.cx
   ```
   
   You should see:
   ```
   5
   10
   ```

## Your First Cx Program

Create a file called `hello.cx`:

```cx
var message = "Hello, Cx Language!"
var number = 42

print(message)
print("The answer is:")
print(number)

if (number > 40) {
    print("That's a big number!")
}
```

Run it:
```powershell
dotnet run -- run hello.cx
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
if (age >= 18) {
    print("Adult")
} else {
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
