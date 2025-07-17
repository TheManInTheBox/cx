# Cx Language Syntax Reference

## Table of Contents

1. [Basic Syntax](#basic-syntax)
2. [Code Style](#code-style)
3. [Variables](#variables)
4. [Types](#types)
5. [Operators](#operators)
6. [Control Flow](#control-flow)
7. [Functions](#functions)
8. [Comments](#comments)
9. [Error Handling](#error-handling)

## Basic Syntax

Cx uses a clean, C-style syntax with modern language features:

```cx
// Statements are separated by newlines
var x = 42
var name = "Alice"
print("Hello, World!")
```

## Code Style

### Bracket Style
Cx follows **Allman style** (also known as BSD style) bracket placement for better readability:

```cx
// Preferred: Allman style (opening brace on new line)
if (condition)
{
    statement1
    statement2
}
else
{
    statement3
}

while (condition)
{
    body
}

function myFunction()
{
    functionBody
}
```

**Not recommended:** K&R style (opening brace on same line):
```cx
// Avoid this style
if (condition) 
{
    statement
}
```

### Indentation
- Use 4 spaces for indentation
- Be consistent with spacing around operators
- Place each statement on its own line

## Variables

### Variable Declarations
All new variables must be declared with the `var` keyword:

```cx
// Variable declarations (required var keyword)
var message = "Hello"
var count = 10
var isActive = true

// Assignment to existing variables (no var keyword)
count = 20
message = "Updated message"
isActive = false
```

### Variable Assignment
Assignment expressions update existing variables:

```cx
var x = 5
print(x)        // Output: 5

x = x + 1       // Assignment expression
print(x)        // Output: 6

x = x * 2
print(x)        // Output: 12
```

### Error Handling
Attempting to assign to an undeclared variable produces a clear error:

```cx
// This will cause a compilation error:
// undeclaredVar = 10
// Error: Variable 'undeclaredVar' not declared. Use 'var' keyword to declare new variables.
```
```

## Types

Cx supports strong typing with automatic type inference:

### Primitive Types

```cx
// String literals
var message = "Hello, World!"
var name = "Alice"

// Integer literals  
var count = 42
var age = 25

// Boolean literals
var isActive = true
var isComplete = false
```

### Type Inference
Types are automatically inferred from the initial value:

```cx
var message = "Hello"     // Inferred as string
var number = 42           // Inferred as integer  
var flag = true           // Inferred as boolean
```

## Operators

### Arithmetic Operators
```cx
var a = 10
var b = 3

var sum = a + b           // Addition: 13
var difference = a - b    // Subtraction: 7
var product = a * b       // Multiplication: 30  
var quotient = a / b      // Division: 3 (integer division)
```

### Comparison Operators
```cx
var x = 5
var y = 10

var equal = x == y        // Equal: false
var notEqual = x != y     // Not equal: true
var less = x < y          // Less than: true
var greater = x > y       // Greater than: false
var lessEqual = x <= y    // Less than or equal: true
var greaterEqual = x >= y // Greater than or equal: false
```

### Logical Operators
```cx
var a = true
var b = false

// Logical AND - returns true if both operands are true
var and1 = a && b        // false (true AND false)
var and2 = a && a        // true (true AND true)

// Logical OR - returns true if at least one operand is true
var or1 = a || b         // true (true OR false)
var or2 = b || b         // false (false OR false)

// Logical NOT - returns the opposite boolean value
var not1 = !a           // false (NOT true)
var not2 = !b           // true (NOT false)

// Combining with comparisons
var x = 10
var y = 5
if (x > y && a) 
{        // true AND true = true
    print("Both conditions are true")
}

if (x < y || a) 
{        // false OR true = true
    print("At least one condition is true")
}

// Complex expressions with parentheses
var complex = !(x < y) && (a || b)  // true AND true = true
```

### Unary Operators
```cx
var num = 5
var flag = true

var negative = -num      // Unary minus: -5
var positive = +num      // Unary plus: 5 (no change)
var inverted = !flag     // Logical NOT: false
```

### Operator Precedence
Operators follow standard mathematical precedence:

1. `!`, `-`, `+` (Unary NOT, Minus, Plus)
2. `*`, `/` (Multiplication, Division)
3. `+`, `-` (Addition, Subtraction)  
4. `<`, `>`, `<=`, `>=` (Comparison)
5. `==`, `!=` (Equality)
6. `&&` (Logical AND)
7. `||` (Logical OR)
8. `=` (Assignment)

## Control Flow

### If/Else Statements
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
else if (score >= 70)
{
    print("Grade: C")
}
else
{
    print("Grade: F")
}

// Single line conditions
var age = 20
if (age >= 18)
{
    print("Adult")
}
```

### While Loops
```cx
// Basic while loop
var counter = 0
while (counter < 5)
{
    print(counter)
    counter = counter + 1
}

// While loop with complex condition
var x = 10
while (x > 0)
{
    print(x)
    x = x - 2
}
```

### For Loops (Defined in Grammar)
For loops are defined in the grammar but basic implementation needs enhancement:

```cx
// Coming soon - enhanced for loop support:
// for (var i = 0; i < 10; i = i + 1) {
//     print(i)
// }
```

## Functions

### Basic Functions
```cx
// Simple function
function greet() 
{
    print("Hello from a function!")
}

// Call the function
greet()
```

### Functions with Parameters (Basic Support)
```cx
// Function parameters are defined in grammar but need full implementation:
// function add(a: number, b: number) -> number 
// {
//     return a + b
// }
```

## Comments

Cx supports single-line comments:

```cx
// This is a single-line comment
var x = 5  // Comment at end of line

// Multi-line comments with //
// Line 1 of comment
// Line 2 of comment
var y = 10
```

## Error Handling

### Compilation Errors
The compiler provides clear error messages for common mistakes:

```cx
// Error: Variable not declared
// undeclaredVar = 10
// Error: Variable 'undeclaredVar' not declared. Use 'var' keyword to declare new variables.

// Error: Invalid syntax
// var x =     // Missing value
// Error: Expected expression after '='
```

## Built-in Functions

### print() Function
The `print()` function outputs values to the console:

```cx
// Print literals
print("Hello, World!")
print(42)
print(true)

// Print variables
var message = "Welcome"
var count = 10
print(message)
print(count)

// Print expressions
var a = 5
var b = 3
print(a + b)        // Prints: 8
print(a > b)        // Prints: true
```

## Complete Example

Here's a comprehensive example demonstrating current Cx features:

```cx
// Variable declarations
var name = "Alice"
var age = 25
var isStudent = true

// Print basic info
print("Name:")
print(name)
print("Age:")
print(age)

// Arithmetic operations
var birthYear = 2024 - age
print("Birth year:")
print(birthYear)

// Control flow
if (age >= 18) {
    print("Adult")
} else {
    print("Minor")
}

// While loop with assignment expressions
var countdown = 5
while (countdown > 0) {
    print(countdown)
    countdown = countdown - 1
}
print("Blast off!")

// Boolean comparisons
var canVote = age >= 18
if (canVote == true) {
    print("Eligible to vote")
}

// Multiple conditions
if (isStudent == true) {
    if (age < 26) {
        print("Student discount available")
    }
}
```

## Language Limitations (Current Version)

The current implementation has these limitations that are planned for future releases:

- **Logical Operators**: `&&` and `||` are in grammar but not implemented in IL compiler
- **String Concatenation**: String `+` operator not yet implemented  
- **Arrays**: Array literals and indexing not implemented
- **Object Literals**: Object syntax not implemented
- **Advanced Functions**: Parameter passing and return values need enhancement
- **Import System**: Module importing not implemented
- **Exception Handling**: try/catch not implemented

## Next Steps

To extend your Cx programs:

1. **Use Current Features**: Build programs with variables, arithmetic, comparisons, and control flow
2. **Test Thoroughly**: The `print()` function helps debug your programs
3. **Follow Conventions**: Always use `var` for new variables, clear variable names
4. **Report Issues**: Help improve the language by reporting bugs or unclear error messages

---

*This documentation reflects the current working implementation of Cx language.*
message = greet("World")
print(message)
```

### Function Parameters

```cx
// Required parameters
function add(a: number, b: number) -> number {
    return a + b
}

// Default parameters (planned)
// function greet(name: string = "World") -> string {
//     return "Hello, " + name
// }
```

### Async Functions

```cx
async function fetchData(url: string) -> object {
    response = await http.get(url)
    return response.json()
}
```

### Function Types

```cx
// Function type annotation
processor: (string) -> string = function(text: string) -> string {
    return text.toUpperCase()
}
```

## Control Flow

### If Statements

```cx
if (score >= 90) {
    grade = "A"
} else if (score >= 80) {
    grade = "B"
} else {
    grade = "F"
}

// Single line if
if (condition) doSomething()
```

### While Loops

```cx
counter = 0
while (counter < 10) {
    print(counter)
    counter = counter + 1
}
```

### For Loops

```cx
// For-in loop
for item in items {
    print(item)
}

// Loop with index (planned)
// for (item, index) in items {
//     print(index + ": " + item)
// }
```

### Return Statements

```cx
function calculate(x: number) -> number {
    if (x < 0) {
        return 0
    }
    return x * 2
}
```

## AI Integration

### Basic AI Calls

```cx
using ai from "azure.openai"

model = ai.connect("gpt-4-turbo")

// Simple generation
response = await model.generate("Write a poem about coding")
print(response)
```

### Structured Analysis

```cx
result = await model.analyze(document, {
    task: "sentiment_analysis",
    format: "json",
    schema: {
        sentiment: "string",
        confidence: "number",
        topics: "array<string>"
    }
})

print("Sentiment: " + result.sentiment)
print("Confidence: " + result.confidence)
```

### AI Configuration

```cx
// Configure model parameters
config = {
    temperature: 0.7,
    maxTokens: 1000,
    topP: 0.9,
    frequencyPenalty: 0.0,
    presencePenalty: 0.0
}

response = await model.generate(prompt, config)
```

## Async/Await

### Async Functions

```cx
async function processData() -> array<object> {
    data = await fetchRemoteData()
    results = await processResults(data)
    return results
}
```

### Await Expressions

```cx
// Await function calls
result = await longRunningOperation()

// Await in expressions
total = (await getCount()) + (await getOffset())

// Await in control flow
if (await checkCondition()) {
    doSomething()
}
```

### Error Handling (Planned)

```cx
// Try-catch for async operations
// try {
//     result = await riskyOperation()
// } catch (error) {
//     print("Error: " + error.message)
// }
```

## Parallel Processing

### Parallel Keyword

```cx
// Process multiple items concurrently
results = await parallel items.map(item => processItem(item))

// Parallel function calls
data = await parallel [
    fetchUserData(userId),
    fetchUserPosts(userId),
    fetchUserFriends(userId)
]
```

### Parallel with AI

```cx
// Analyze multiple documents concurrently
analyses = await parallel documents.map(doc => {
    content = storage.read(doc)
    return ai.analyze(content, { task: "summary" })
})
```

### Parallel Configuration (Planned)

```cx
// Configure concurrency limits
// results = await parallel(items, { maxConcurrency: 5 }).map(processItem)
```

## Imports

### Basic Imports

```cx
using ai from "azure.openai"
using storage from "azure.storage"
using http from "system.http"
```

### Import Aliases

```cx
// Import with custom alias
using gpt from "azure.openai"
using blob from "azure.storage.blob"
```

### Selective Imports (Planned)

```cx
// Import specific functions
// using { analyze, generate } from "azure.openai"
```

## Object Literals

### Basic Objects

```cx
person = {
    name: "Alice",
    age: 30,
    city: "Seattle"
}

print(person.name)  // Access property
```

### Nested Objects

```cx
config = {
    database: {
        host: "localhost",
        port: 5432,
        name: "mydb"
    },
    ai: {
        model: "gpt-4-turbo",
        temperature: 0.7
    }
}
```

### Dynamic Properties

```cx
key = "dynamicKey"
obj = {
    staticKey: "value1",
    [key]: "value2"  // Computed property name
}
```

## Arrays

### Array Creation

```cx
// Array literals
numbers = [1, 2, 3, 4, 5]
names = ["Alice", "Bob", "Charlie"]
mixed = [1, "hello", true, { key: "value" }]

// Empty array
empty = []
```

### Array Access

```cx
first = numbers[0]      // First element
last = numbers[-1]      // Last element (planned)
slice = numbers[1:3]    // Slice (planned)
```

### Array Methods (Planned)

```cx
// Map
doubled = numbers.map(n => n * 2)

// Filter
evens = numbers.filter(n => n % 2 == 0)

// Reduce
sum = numbers.reduce((acc, n) => acc + n, 0)

// Length
count = numbers.length
```

## Comments

### Line Comments

```cx
// This is a line comment
x = 42  // Comment at end of line
```

### Block Comments

```cx
/*
This is a block comment
spanning multiple lines
*/
function example() {
    /* Inline block comment */ return true
}
```

### Documentation Comments (Planned)

```cx
/**
 * Calculates the factorial of a number
 * @param n The number to calculate factorial for
 * @returns The factorial result
 */
// function factorial(n: number) -> number {
//     if (n <= 1) return 1
//     return n * factorial(n - 1)
// }
```

## Best Practices

### Naming Conventions

```cx
// Variables and functions: camelCase
userName = "alice"
totalCount = 42

function calculateTotal() -> number {
    return 100
}

// Constants: UPPER_CASE (planned)
// MAX_RETRIES = 3
// API_BASE_URL = "https://api.example.com"
```

### Async Best Practices

```cx
// Always await async calls
result = await asyncOperation()  // ✅ Good

// Don't forget await
// result = asyncOperation()     // ❌ Bad - returns Promise

// Use parallel for independent operations
results = await parallel [
    operation1(),
    operation2(),
    operation3()
]  // ✅ Good - runs concurrently
```

### AI Integration Best Practices

```cx
// Use structured analysis for consistent results
analysis = await ai.analyze(text, {
    task: "sentiment_analysis",
    format: "json",
    schema: { sentiment: "string", score: "number" }
})  // ✅ Good - structured output

// Handle AI errors gracefully
// try {
//     result = await ai.generate(prompt)
// } catch (error) {
//     result = "Error: Could not generate response"
// }  // ✅ Good - error handling
```
