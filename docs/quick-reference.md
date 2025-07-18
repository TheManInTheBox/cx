# CX Language Quick Reference

## Basic Syntax
```cx
// Variables (must use var for new variables)
var name = "Alice";
var age = 25;
var isActive = true;

// Reassignment (no var keyword)
name = "Bob";
age = age + 1;
```

## Data Types
```cx
var text = "Hello";           // string
var number = 42;              // number (int/float)
var flag = true;              // boolean
var nothing = null;           // null
var items = [1, 2, 3];        // array
var config = { key: value };  // object
```

## Operators
```cx
// Arithmetic: +, -, *, /, %
var sum = 5 + 3;              // 8

// Comparison: ==, !=, <, >, <=, >=
var isEqual = x == y;         // boolean

// Logical: &&, ||, !
var result = true && false;   // false

// Assignment: =, +=, -=, *=, /=
x += 5;                       // x = x + 5
```

## Control Flow
```cx
// If-else
if (condition)
{
    // code
}
else
{
    // alternative
}

// While loop
while (condition)
{
    // loop body
}

// For-in loop
for (var item in array)
{
    // iterate over array
}
```

## Functions
```cx
// Function declaration
function greet(name)
{
    print("Hello, " + name);
}

// Function call
greet("Alice");

// Multiple parameters
function add(a, b)
{
    print(a + b);
}
```

## Objects and Arrays
```cx
// Object literal
var person = {
    name: "John",
    age: 30,
    "full name": "John Doe"
};

// Property access
var name = person.name;
var fullName = person["full name"];

// Array literal
var colors = ["red", "green", "blue"];

// Array access
var first = colors[0];
```

## Exception Handling
```cx
try
{
    // risky code
}
catch (error)
{
    print("Error: " + error);
}

// Throw exception
throw "Something went wrong";
```

## AI Functions (Phase 4 Complete with Runtime Function Injection)
```cx
// Basic AI functions
var result = task("Analyze this data");
var content = synthesize("Create a report");
var analysis = reason("What's the best approach?");
var output = generate("Write documentation");
var vector = embed("Convert to embedding");

// Process function (2 parameters)
var processed = process("input", "context");

// REVOLUTIONARY: Runtime Function Injection with adapt()
var result = adapt("Create a function to add two numbers", {
    type: "function",
    name: "add",
    parameters: ["a", "b"],
    returnType: "number"
});

// Generated function is now available for immediate use
var sum = add(7, 3);  // Returns 10
print("Sum: " + sum);

// Generate complex mathematical functions
var squareResult = adapt("Create a function to square a number", {
    type: "function",
    name: "square",
    parameters: ["x"],
    returnType: "number"
});

var squared = square(6);  // Returns 36
print("Squared: " + squared);

// Functions persist throughout execution
var anotherSum = add(10, 20);  // Still works: returns 30
var anotherSquare = square(4);  // Still works: returns 16

// Traditional content adaptation also supported
var adapted = adapt("Rewrite for beginners");

// With options (future)
var result = task("prompt", { temperature: 0.7 });
```

## Built-in Functions
```cx
print("Hello, World!");       // Output to console
print(42);                    // Print numbers
print(variable);              // Print variables
```

## Comments
```cx
// Single-line comment

/* 
 * Multi-line comment
 * spanning multiple lines
 */
```

## Grammar-Ready Features (Not Yet Implemented)
```cx
// Classes (grammar complete)
class Person
{
    name: string;
    
    constructor(name: string)
    {
        this.name = name;
    }
    
    function greet()
    {
        print("Hello, " + this.name);
    }
}

// Interfaces (grammar complete)
interface Drawable
{
    draw(): void;
}

// Typed parameters (grammar complete)
function add(a: number, b: number) -> number
{
    return a + b;
}

// Async functions (grammar complete)
async function fetchData()
{
    var result = await getData();
    return result;
}

// Imports (grammar complete)
using ModuleName from "module-path";
```

## Code Style Rules
- **Brackets**: Allman style (opening bracket on new line)
- **Indentation**: 4 spaces
- **Semicolons**: Required for statements
- **Case**: Case-sensitive language
- **Variables**: Must use `var` for new declarations

## Current Limitations
- Function return values partially implemented
- AI functions require Azure OpenAI configuration
- Classes and interfaces not yet implemented
- No module system yet
- No type checking at compile time

## File Extensions
- Source files: `.cx`
- Location: Place all files in `examples/` folder for testing

## Testing
```bash
# Run a CX file
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/filename.cx

# Parse only (show AST)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj parse examples/filename.cx

# Compile only
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj compile examples/filename.cx
```
