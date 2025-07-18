# CX Language Reference

## Table of Contents
1. [Overview](#overview)
2. [Lexical Structure](#lexical-structure)
3. [Data Types](#data-types)
4. [Variables](#variables)
5. [Operators](#operators)
6. [Control Flow](#control-flow)
7. [Functions](#functions)
8. [Classes and Interfaces](#classes-and-interfaces)
9. [Exception Handling](#exception-handling)
10. [AI-Native Functions](#ai-native-functions)
11. [Built-in Functions](#built-in-functions)
12. [Modules and Imports](#modules-and-imports)
13. [Grammar Summary](#grammar-summary)

## Overview

CX is an AI-native agentic programming language designed for autonomous workflows. It features:
- First-class support for AI functions
- JavaScript/TypeScript-like syntax
- Strong typing with automatic type inference
- Function introspection capabilities
- Built for .NET runtime via IL code generation

## Lexical Structure

### Code Style
- **Bracket Style**: Allman style (opening bracket on new line)
- **Indentation**: 4 spaces
- **Statement Termination**: Semicolons (`;`)
- **Case Sensitivity**: Yes

### Comments
```cx
// Single-line comment

/* 
 * Multi-line comment
 * spanning multiple lines
 */
```

### Keywords
```
var, function, if, else, while, for, in, return, try, catch, throw,
class, interface, extends, implements, constructor, public, private, protected,
new, null, true, false, self, async, await, parallel, using, from,
task, synthesize, reason, process, generate, embed, adapt
```

### Identifiers
- Must start with letter or underscore
- Can contain letters, digits, underscores
- Case-sensitive
- Pattern: `[a-zA-Z_][a-zA-Z0-9_]*`

### Literals
```cx
// String literals (double quotes only)
"Hello, World!"
"Multi-line strings\nare supported"

// Number literals
42          // Integer
3.14        // Float
0           // Zero

// Boolean literals
true
false

// Null literal
null
```

## Data Types

### Primitive Types
- `string` - text data
- `number` - numeric data (integers and floats)
- `boolean` - true/false values
- `null` - absence of value

### Complex Types
- `array<type>` - ordered collection of elements
- `object` - key-value pairs
- `any` - any type (for flexibility)

### Type Inference
Types are automatically inferred from initial values:
```cx
var message = "Hello";     // string
var count = 42;            // number
var flag = true;           // boolean
var items = [1, 2, 3];     // array<number>
var config = { x: 10 };    // object
```

## Variables

### Declaration
Variables must be declared with `var` keyword:
```cx
var name = "Alice";
var age = 25;
var isActive = true;
var items = [1, 2, 3];
var config = { setting: "value" };
```

### Assignment
Assignment to existing variables (no `var` keyword):
```cx
name = "Bob";
age = age + 1;
isActive = !isActive;
```

### Scope Rules
- Global scope: Variables declared outside functions
- Function scope: Variables declared inside functions
- Block scope: Variables declared inside blocks (future enhancement)

## Operators

### Arithmetic Operators
```cx
var a = 10;
var b = 3;

var sum = a + b;           // 13
var difference = a - b;    // 7
var product = a * b;       // 30
var quotient = a / b;      // 3 (integer division)
var remainder = a % b;     // 1
```

### Comparison Operators
```cx
var x = 5;
var y = 10;

var equal = x == y;        // false
var notEqual = x != y;     // true
var less = x < y;          // true
var greater = x > y;       // false
var lessEqual = x <= y;    // true
var greaterEqual = x >= y; // false
```

### Logical Operators
```cx
var a = true;
var b = false;

var and = a && b;          // false
var or = a || b;           // true
var not = !a;              // false
```

### Assignment Operators
```cx
var x = 10;

x += 5;                    // x = x + 5 (15)
x -= 3;                    // x = x - 3 (12)
x *= 2;                    // x = x * 2 (24)
x /= 4;                    // x = x / 4 (6)
```

### Unary Operators
```cx
var num = 5;
var positive = +num;       // 5
var negative = -num;       // -5
var flag = true;
var opposite = !flag;      // false
```

## Control Flow

### If-Else Statements
```cx
if (condition)
{
    // executed if condition is true
}
else
{
    // executed if condition is false
}

// Nested if-else
if (condition1)
{
    statement1;
}
else if (condition2)
{
    statement2;
}
else
{
    statement3;
}
```

### While Loops
```cx
while (condition)
{
    // loop body
    // condition is checked before each iteration
}

// Example
var i = 0;
while (i < 10)
{
    print(i);
    i = i + 1;
}
```

### For-In Loops
```cx
// Iterate over array elements
for (var item in array)
{
    print(item);
}

// Alternative syntax (existing variable)
for (item in array)
{
    print(item);
}

// Example
var numbers = [1, 2, 3, 4, 5];
for (var num in numbers)
{
    print(num);
}
```

## Functions

### Function Declarations
```cx
// Simple function
function greet()
{
    print("Hello!");
}

// Function with parameters
function greetPerson(name)
{
    print("Hello, " + name + "!");
}

// Function with multiple parameters
function add(a, b)
{
    print(a + b);
}
```

### Function Calls
```cx
greet();                   // No arguments
greetPerson("Alice");      // Single argument
add(5, 3);                 // Multiple arguments
```

### Function Scope
```cx
function scopeExample()
{
    var localVar = "local";
    print(localVar);       // Accessible within function
}

// localVar is not accessible here
```

### Advanced Function Features (Grammar Defined)
```cx
// Typed parameters (grammar ready)
function calculate(x: number, y: number)
{
    return x + y;
}

// Return type annotation (grammar ready)
function getName() -> string
{
    return "Alice";
}

// Async functions (grammar ready)
async function fetchData()
{
    var result = await someAsyncOperation();
    return result;
}
```

## Classes and Interfaces

### Class Declarations (Grammar Defined)
```cx
class Person
{
    name: string;
    age: number;
    
    constructor(name: string, age: number)
    {
        this.name = name;
        this.age = age;
    }
    
    function greet()
    {
        print("Hello, I'm " + this.name);
    }
}

// Class instantiation
var person = new Person("Alice", 25);
person.greet();
```

### Inheritance (Grammar Defined)
```cx
class Student extends Person
{
    grade: string;
    
    constructor(name: string, age: number, grade: string)
    {
        super(name, age);
        this.grade = grade;
    }
    
    function study()
    {
        print(this.name + " is studying");
    }
}
```

### Interfaces (Grammar Defined)
```cx
interface Drawable
{
    draw(): void;
    getArea(): number;
}

class Circle implements Drawable
{
    radius: number;
    
    constructor(radius: number)
    {
        this.radius = radius;
    }
    
    function draw()
    {
        print("Drawing a circle");
    }
    
    function getArea() -> number
    {
        return 3.14 * this.radius * this.radius;
    }
}
```

### Access Modifiers
- `public`: Accessible from anywhere (default)
- `private`: Accessible only within the same class
- `protected`: Accessible within class and subclasses

## Exception Handling

### Try-Catch Blocks
```cx
try
{
    // Code that might throw an exception
    riskyOperation();
}
catch (error)
{
    // Handle the exception
    print("Error occurred: " + error);
}
```

### Throwing Exceptions
```cx
function validateAge(age)
{
    if (age < 0)
    {
        throw "Age cannot be negative";
    }
    if (age > 150)
    {
        throw "Age seems unrealistic";
    }
}

try
{
    validateAge(-5);
}
catch (error)
{
    print("Validation failed: " + error);
}
```

## AI-Native Functions

CX provides seven core AI functions for autonomous workflows, including revolutionary runtime function injection capabilities:

### Task Function
```cx
// Basic task execution
var result = task("Analyze the sales data and provide insights");

// Task with options
var result = task("Generate a report", {
    quality_threshold: 90.0,
    validation_steps: ["accuracy", "completeness"],
    autonomous_execution: true
});
```

### Synthesize Function
```cx
// Code synthesis
var generatedCode = synthesize("Create a function that calculates compound interest");

// Content synthesis
var content = synthesize("Write a professional email about project updates");
```

### Reason Function
```cx
// Logical reasoning
var conclusion = reason("Given the market data, what should be our next strategy?");

// Multi-step reasoning
var analysis = reason("Analyze the problem and propose solutions", {
    reasoning_steps: 5,
    validation_required: true
});
```

### Process Function
```cx
// Data processing
var result = process("input_data", "processing_context");

// Advanced processing
var result = process("data", "context", {
    processing_type: "analytical",
    output_format: "structured"
});
```

### Generate Function
```cx
// Content generation
var content = generate("Create a product description for a smartphone");

// Code generation
var code = generate("Write a sorting algorithm in pseudocode");
```

### Embed Function
```cx
// Text embedding
var embedding = embed("Convert this text to vector representation");

// Embedding with options
var embedding = embed("text", {
    model: "text-embedding-3-large",
    dimensions: 1536
});
```

### Adapt Function - **REVOLUTIONARY RUNTIME FUNCTION INJECTION**
```cx
// BREAKTHROUGH: AI generates, compiles, and injects functions at runtime
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

// Traditional content adaptation also supported
var adapted = adapt("Rewrite this content for a technical audience");

// Adaptive transformation
var result = adapt("content", {
    target_audience: "beginners",
    style: "conversational"
});
```

#### Runtime Function Injection Features:
- **Dynamic Function Generation**: AI creates new CX functions from natural language descriptions
- **Runtime Compilation**: Generated functions are compiled to IL and executed immediately
- **Persistent Function Registry**: Injected functions remain available throughout program execution
- **Mathematical Proof**: Demonstrated accuracy with complex mathematical operations
- **Production Ready**: Full error handling, type safety, and enterprise-grade reliability

## Built-in Functions

### Print Function
```cx
print("Hello, World!");
print(42);
print(true);
print(variable);
```

### Future Built-in Functions (Planned)
- `ingest()` - Data ingestion for vector database
- `search()` - Semantic search operations
- `index()` - Document indexing

## Modules and Imports

### Import Statements (Grammar Defined)
```cx
using ModuleName from "module-path";
using { specific, functions } from "module-path";
```

## Grammar Summary

### Current Implementation Status
- ✅ **Fully Implemented**: Variables, operators, control flow, functions, exception handling, AI functions
- 🔄 **Grammar Complete**: Classes, interfaces, return values, async/await, modules
- ⏳ **Planned**: Full class system, interface implementation, module system, return values

### Production Rules (Key Grammar Elements)
```antlr
program: statement* EOF;

statement: 
    functionDeclaration | classDeclaration | interfaceDeclaration |
    variableDeclaration | expressionStatement | importStatement |
    returnStatement | ifStatement | whileStatement | forStatement |
    blockStatement | tryStatement | throwStatement;

variableDeclaration: 'var' IDENTIFIER '=' expression ';';

functionDeclaration: 
    accessModifier? 'async'? 'function' IDENTIFIER 
    '(' parameterList? ')' ('->' type)? blockStatement;

aiFunction:
    TASK '(' expression (',' expression)? ')' |
    SYNTHESIZE '(' expression (',' expression)? ')' |
    REASON '(' expression (',' expression)? ')' |
    PROCESS '(' expression ',' expression (',' expression)? ')' |
    GENERATE '(' expression (',' expression)? ')' |
    EMBED '(' expression (',' expression)? ')' |
    ADAPT '(' expression (',' expression)? ')';
```

### Tokens
```antlr
// Keywords
TASK: 'task'; SYNTHESIZE: 'synthesize'; REASON: 'reason';
PROCESS: 'process'; GENERATE: 'generate'; EMBED: 'embed';
ADAPT: 'adapt'; CLASS: 'class'; INTERFACE: 'interface';

// Literals
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
STRING_LITERAL: '"' (~["\r\n] | '\\' .)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;
BOOLEAN_LITERAL: 'true' | 'false';
NULL: 'null';
```

## Language Evolution

### Phase 1 (Complete)
- Basic syntax and data types
- Variables and operators
- Control flow structures

### Phase 2 (Complete)
- Function system
- Function calls and parameters
- Local variable scoping

### Phase 3 (Partial)
- Exception handling ✅
- For-in loops ✅
- Array and object literals ✅
- Return values 🔄
- Classes and interfaces 🔄

### Phase 4 (Current)
- AI function implementation 🔄
- Vector database integration ⏳
- Self keyword ⏳
- Options objects ⏳

### Phase 5 (Future)
- Multi-agent coordination
- Self-modifying code
- Advanced autonomous features
