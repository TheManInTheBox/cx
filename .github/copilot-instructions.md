# Copilot Instructions for CX Language

## Project Overview
The CX Language is an AI-native agentic programming language designed for autonomous workflows. It features:
- First-class support for AI functions (task, synthesize, reason, process, generate, embed, adapt)
- JavaScript/TypeScript-like syntax for familiarity
- Function introspection capabilities with the `self` keyword
- Built for .NET runtime via IL code generation
- Focus on autonomous agent capabilities and self-modifying code

## Development Phases and Scope

### ✅ Phase 1: Core Language Foundation (COMPLETED)
- Variables, data types, arithmetic operations
- Control flow (if/else, while loops)
- Logical and comparison operators
- String operations and concatenation
- Compound assignment operators

### ✅ Phase 2: Function System (COMPLETED!)
- ✅ Two-pass compilation architecture 
- ✅ Function declarations and definitions
- ✅ Function parameters (multiple parameters supported)
- ✅ Function calls with argument passing
- ✅ Parameter access within function bodies
- ✅ Local variable scoping within functions
- ✅ Function return handling (void functions)

### 📋 Phase 3: Advanced Language Features (CURRENT PRIORITY)
- For-in loops and iterators
- Exception handling (try/catch/throw)
- Array and object literals
- Class system and inheritance
- Module system and imports
- ✅ Function return values (non-void)

### 🤖 Phase 4: AI Integration (CURRENT PRIORITY)
- Implementation of native AI functions (task, synthesize, reason, process, generate, embed, adapt)
- Microsoft Semantic Kernel integration for AI orchestration
- In-memory vector database for semantic search and retrieval
- Data ingestion built-in functions for vector database population
- Self keyword for function introspection
- Autonomous workflow capabilities
- AI function options objects and advanced configuration
- Multi-modal AI processing (text, images, audio, video)

### 🚀 Phase 5: Autonomous Agentic Features (VISION)
- Multi-agent coordination
- Learning and adaptation mechanisms
- Self-modifying code capabilities

## Current Development Focus

**🎉 PHASE 2 COMPLETE: Function System Fully Functional!**
- ✅ Two-pass compilation system working perfectly
- ✅ Function declarations with multiple parameters
- ✅ Function calls with argument passing  
- ✅ Parameter access and local variable scoping
- ✅ All function system features operational

**Priority 1: AI Function Implementation (Phase 4)**
- Implement native AI functions using Microsoft Semantic Kernel
- Add in-memory vector database for semantic operations
- Create data ingestion built-in functions for vector database
- Enable self keyword for function introspection
- Create AI function options objects for advanced configuration
- Current scope: Complete core AI integration before autonomous agentic features

**Phase 4 Vector Database Features:**
- Built-in functions for data ingestion: `ingest()`, `index()`, `search()`
- Semantic search capabilities with similarity scoring
- Document chunking and embedding generation
- Memory persistence and retrieval for autonomous agents
- Integration with Semantic Kernel's memory stores

**Stay in Scope**: Complete Phase 4 AI integration using Semantic Kernel and vector database before moving to Phase 5.

## CX Language Syntax Reference

### Basic Syntax Rules

#### Code Style and Formatting
- **ALWAYS use Allman-style brackets** (opening bracket on new line)
- Use 4-space indentation consistently
- Statements end with semicolons (`;`)
- Comments: `//` for single-line, `/* */` for multi-line
- Case-sensitive language

#### Variables and Declarations
```cx
// Variable declarations (required var keyword)
var message = "Hello";
var count = 10;
var isActive = true;

// Assignment to existing variables (no var keyword)
count = 20;
message = "Updated message";
isActive = false;
```

#### Data Types
- **String literals**: `"Hello, World!"` (double quotes)
- **Number literals**: `42`, `3.14` (integers and floats)
- **Boolean literals**: `true`, `false`
- **Null literal**: `null`

#### Operators
- **Arithmetic**: `+`, `-`, `*`, `/`, `%`
- **Comparison**: `==`, `!=`, `<`, `>`, `<=`, `>=`
- **Logical**: `&&` (AND), `||` (OR), `!` (NOT)
- **Assignment**: `=`, `+=`, `-=`, `*=`, `/=`
- **Unary**: `+`, `-`, `!`

#### Control Flow
```cx
// If-else statements
if (condition)
{
    statement1;
    statement2;
}
else
{
    statement3;
}

// While loops
while (condition)
{
    body;
}

// For-in loops
for (var item in collection)
{
    body;
}

// Alternative for-in syntax
for (item in collection)
{
    body;
}
```

#### Functions
```cx
// Function declarations
function functionName()
{
    body;
}

// Function with parameters
function functionName(param1, param2)
{
    body;
}

// Function with typed parameters (grammar defined)
function functionName(param1: type, param2: type)
{
    body;
}

// Function with return type (grammar defined)
function functionName() -> type
{
    return value;
}

// Async functions (grammar defined)
async function functionName()
{
    body;
}

// Function calls
functionName();
functionName(arg1, arg2);
```

#### Exception Handling
```cx
// Try-catch blocks
try
{
    riskyOperation();
}
catch (error)
{
    handleError();
}

// Throw statements
throw "Error message";
throw errorObject;
```

#### Object and Array Literals
```cx
// Object literals
var obj = {
    property1: value1,
    property2: value2,
    "string key": value3
};

// Array literals
var arr = [item1, item2, item3];

// Object property access
var value = obj.property1;
var value2 = obj["string key"];

// Array indexing
var element = arr[0];
```

#### AI-Native Functions
```cx
// Core AI functions with single argument
var result = task("prompt");
var analysis = reason("problem");
var content = synthesize("requirements");
var output = generate("specification");
var embedding = embed("text");
var adapted = adapt("content");

// AI functions with options (second argument)
var result = task("prompt", options);
var processed = process("input", "context", options);
```

#### Classes and Interfaces (Grammar Defined)
```cx
// Class declarations
class ClassName
{
    property: type;
    
    constructor(param1: type)
    {
        this.property = param1;
    }
    
    function methodName()
    {
        body;
    }
}

// Class with inheritance
class ChildClass extends ParentClass
{
    body;
}

// Interface declarations
interface InterfaceName
{
    methodName(): returnType;
    property: type;
}
```

#### Access Modifiers
- `public` - accessible from anywhere
- `private` - accessible only within the same class
- `protected` - accessible within the class and its subclasses

#### Special Keywords
- `self` - function introspection keyword
- `new` - object instantiation
- `await` - asynchronous operation waiting
- `parallel` - parallel execution
- `using` - import statement
- `return` - function return
- `null` - null value

#### Import Statements
```cx
using ModuleName from "module-path";
```

#### Built-in Functions
- `print(value)` - output to console

### Current Language Limitations
- AI service requires Azure OpenAI configuration for testing
- ✅ Function return values (grammar complete, compiler implementation complete)
- Class system defined in grammar but not fully implemented
- Interface system defined in grammar but not implemented
- Module system defined in grammar but not implemented
- AI function options objects need implementation
- Self keyword implementation pending

### Grammar Coverage
- ✅ Variables and basic data types (string, number, boolean, null)
- ✅ Arithmetic and logical operations (+, -, *, /, %, ==, !=, <, >, <=, >=, &&, ||, !)
- ✅ Assignment operators (=, +=, -=, *=, /=)
- ✅ Control flow (if/else, while, for-in loops)
- ✅ Functions (declarations, calls, parameters, void functions)
- ✅ Exception handling (try/catch/throw)
- ✅ Object and array literals ({ key: value }, [item1, item2])
- ✅ AI-native functions (7 core functions: task, reason, synthesize, process, generate, embed, adapt)
- ✅ Function return values
- 🔄 Classes and inheritance (grammar complete, compiler pending)
- 🔄 Interfaces (grammar complete, compiler pending)
- 🔄 Async/await (grammar complete, compiler pending)
- 🔄 Access modifiers (grammar complete, compiler pending)
- 🔄 Module system (grammar complete, compiler pending)

### Implemented Features Status
Based on working examples and grammar:

**✅ Fully Working:**
- Variable declarations with `var` keyword
- Basic data types (string, number, boolean)
- Arithmetic operations (+, -, *, /, %)
- Comparison operations (==, !=, <, >, <=, >=)
- Logical operations (&&, ||, !)
- Assignment operations (=, +=, -=, *=, /=)
- If/else statements with proper Allman-style braces
- While loops
- For-in loops over arrays
- Function declarations and calls (void functions)
- Function parameters and local scope
- Exception handling (try/catch/throw)
- Object literals with property access
- Array literals with indexing
- String concatenation with +
- Print function for output

**🔄 Partially Working:**
- Function declarations and calls (void functions)
- Function parameters and local scope
- Function return values and non-void functions
- AI functions (implemented but requires Azure OpenAI configuration)

**⏳ Grammar Ready (Compiler Pending):**
- Classes and inheritance
- Interfaces
- Async/await
- Access modifiers (public, private, protected)
- Module imports
- Typed function parameters
- Return type annotations

## Coding Standards

### General Guidelines
- Follow C# best practices for backend code (compiler, parser, AST)
- Use descriptive naming for functions, variables, and classes
- Add XML documentation comments for public APIs and complex logic
- Use consistent formatting with 4-space indentation
- **ALWAYS use Allman-style brackets in ALL CX code examples and documentation** (opening bracket on new line)
- **NEVER use K&R-style brackets** (opening bracket on same line as control statement)
- Apply Allman formatting to all function declarations, if statements, loops, and code blocks

### Grammar and Parser
- All grammar changes must be made in `grammar/Cx.g4`
- Follow ANTLR4 conventions and patterns
- When adding new syntax, ensure it has a corresponding visitor in `AstBuilder.cs`
- All new AST nodes should be defined in `AstNodes.cs`

### Compiler
- All IL code generation is in `CxCompiler.cs`
- Follow the visitor pattern for AST traversal
- Track function scope and variable state appropriately
- Ensure type safety where possible

### AI Integration (Future Phase)
- All AI-native functions should be implemented in a consistent manner
- AI functions should support both direct arguments and options objects
- Preserve context and state appropriately for autonomous execution
- Document AI model requirements and expected behaviors

## Feature: Function System (Current Priority)

Functions are the next critical feature to implement:
- Basic function declarations (working in parser)
- Function parameters and arguments
- Return values and types
- Function calls and scoping
- Local variable isolation

## Feature: Self Keyword (Future Phase)

The `self` keyword allows functions to reference their own source code, enabling:
- Function introspection for debugging
- Self-optimization through AI models
- Code manipulation and metaprogramming

Implementation details:
- Grammar: `self` is a primary expression
- Compiler: `VisitSelfReference` returns the function's source code
- Source tracking: Functions need position and source code tracking

## Testing Guidelines
- **IMPORTANT:** All CX language files (.cx) MUST be placed in the `examples/` folder
- Create test files in `examples/` directory for new features
- Follow naming convention: `test_<feature_name>.cx` or `<phase>_<feature>.cx`
- Include basic examples and edge cases
- Verify both parsing and runtime behavior
- Focus on testing current phase features (Phase 1-3 complete, Phase 4 in progress)
- **WORKING EXAMPLES:** Use `current_features_complete.cx` to test all working features
- **AI FUNCTIONS:** Require Azure OpenAI configuration for testing
- **ALWAYS use Allman-style brackets (opening bracket on a new line) in ALL test examples and documentation:**
  ```
  // Correct - ALWAYS use this format
  function example() 
  {
      if (condition)
      {
          // code here
      }
      else
      {
          // alternative code
      }
  }
  
  // Incorrect - NEVER use this format
  function example() {
      if (condition) {
          // code here
      } else {
          // alternative code
      }
  }
  ```

## Development Priorities

1. **Implement For-in Loops** (Phase 3)
   - Array and collection iteration
   - Iterator interface
   - Loop control (break, continue)

2. **Implement Exception Handling** (Phase 3)
   - Try/catch/throw statements
   - Error object support
   - Stack trace capture

3. **Implement Data Structures** (Phase 3)
   - Array literals and operations
   - Object literals and property access
   - Collection manipulation

4. **Implement AI Functions** (Phase 4)
   - Native AI function integration
   - Self keyword implementation
   - Autonomous capabilities
   - Multi-agent coordination
   - Learning mechanisms
   - Self-modification

## Build and Run Instructions
- Build with `dotnet build CxLanguage.sln` 
- Run examples with `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run <path-to-example>`
- Test working features with `examples/comprehensive_working_demo.cx`
- Debug with VS Code or Visual Studio
