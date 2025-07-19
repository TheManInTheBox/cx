---
 applyTo: "**/*.cx"
 description: "CX Cognitive Executor Language - Autonomous Programming Platform"
---

## CX Language - Cognitive Executor for Autonomous Programming

### Overview
CX (Cognitive Executor) is an autonomous programming language built on the Aura cognitive architecture framework. Designed for Safe, Quality, Productivity, and Autonomy in AI-driven development environments.

**Key Principles:**
- **Safe**: Memory-safe IL generation with comprehensive error handling
- **Quality**: Enterprise-grade reliability with production-tested AI integration  
- **Productivity**: Ultra-fast compilation (~50ms) with intuitive syntax
- **Autonomy**: First-class support for autonomous agents and self-modifying code

**Architecture:**
- **CX Language**: The Cognitive Executor - executable autonomous programming language
- **Aura Framework**: The cognitive architecture powering autonomous decision-making
- **Agent Integration**: Copilot and other AI agents can execute CX code directly

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

// Service-based AI functions (Phase 4 Complete!)
using textGen from "Cx.AI.TextGeneration";
using chatBot from "Cx.AI.ChatCompletion";
using imageGen from "Cx.AI.TextToImage";
using embeddings from "Cx.AI.TextEmbeddings";
using tts from "Cx.AI.TextToSpeech";
using vectorDB from "Cx.AI.VectorDatabase";

// Text generation with parameters
var content = textGen.GenerateAsync("Write a story", {
    temperature: 0.8,
    maxTokens: 500
});

// Chat completion with context
var response = chatBot.CompleteAsync("You are helpful", "Explain AI");

// DALL-E 3 image generation
var image = imageGen.GenerateImageAsync("Futuristic city", {
    size: "1024x1024",
    quality: "hd"
});

// Text embeddings for semantic search
var vector = embeddings.GenerateEmbeddingAsync("AI programming language");

// Text-to-Speech with MP3 pure memory streaming (zero temp files!)
tts.SpeakAsync("Welcome to the future of AI programming!");

// Vector Database for RAG workflows (100% COMPLETE!)
var ingestResult = vectorDB.IngestTextAsync("Document content", "doc-id");
var searchResult = vectorDB.AskAsync("What information is stored?");
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