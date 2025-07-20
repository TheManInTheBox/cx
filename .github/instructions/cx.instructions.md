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

#### Event-Driven Architecture (Critical Syntax Rules)
```cx
// Event handlers (Aura sensory layer)
on "event.name" (payload)
{
    // Handle incoming event
    print("Received: " + payload.data);
    
    // CRITICAL: 'when' is ONLY usable inside 'on' blocks
    when (payload.priority > 5)
    {
        emit "high.priority", payload;
    }
    
    when (payload.type == "urgent")
    {
        // More event-driven logic
        emit "escalation.needed", { urgency: "high" };
    }
}

// CRITICAL: Outside of 'on' blocks, use 'if' for conditionals
function processData(data)
{
    // ✅ CORRECT: Use 'if' in functions and standalone code
    if (data.score > 0.8)
    {
        // 'emit' is globally available - can be used anywhere
        emit "data.processed", { result: data, confidence: "high" };
    }
    
    // ❌ INCORRECT: Never use 'when' outside of 'on' blocks
    // when (data.score > 0.8) { ... } // This would be WRONG
}

// Event emission (globally available)
emit "event.name", payload;
emit "notification", { message: "Hello", timestamp: "now" };

// Class-level event receivers (instances can have their own 'on' blocks)
class SmartAgent
{
    name: string;
    
    constructor(agentName)
    {
        this.name = agentName;
    }
    
    // Instance-level event receiver
    on "task.assigned" (payload)
    {
        when (payload.assignedTo == this.name)
        {
            emit "task.accepted", { agent: this.name, taskId: payload.taskId };
        }
    }
}
```

**CRITICAL SCOPING RULES:**
- **`on`**: Defines event receiver functions (global or class instance level)
- **`when`**: Conditional logic **ONLY inside `on` event receiver blocks**
- **`emit`**: Event emission - **globally available everywhere** (functions, classes, standalone code, event handlers)
- **`if`**: Regular conditional logic - **used everywhere EXCEPT inside `on` blocks** (where `when` is preferred)

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
- `self` - function introspection keyword (grammar defined, compiler pending)
- `new` - object instantiation (✅ FULLY OPERATIONAL)
- `await` - asynchronous operation waiting (grammar defined, compiler pending)
- `parallel` - parallel execution (✅ FULLY OPERATIONAL - multi-agent coordination working!)
- `on` - event receiver definition: **global or class instance level** (✅ FULLY OPERATIONAL - event-driven architecture foundation)
- `when` - conditional logic: **ONLY inside `on` event receiver blocks** (✅ FULLY OPERATIONAL - event-driven architecture foundation) 
- `emit` - event emission: **globally available anywhere** (functions, classes, standalone code, event handlers) (✅ FULLY OPERATIONAL - event-driven architecture foundation)
- `if` - regular conditional logic: **used everywhere EXCEPT inside `on` blocks** (where `when` is preferred) (✅ FULLY OPERATIONAL)
- `using` - import statement (✅ FULLY OPERATIONAL)
- `return` - function return (✅ FULLY OPERATIONAL)
- `null` - null value (✅ FULLY OPERATIONAL)

#### Import Statements
```cx
using ModuleName from "module-path";
```

#### Built-in Functions
- `print(value)` - output to console

## Common Syntax Errors and Best Practices

### ❌ CRITICAL ERRORS TO AVOID

#### 1. Incorrect `when` Usage (Outside Event Handlers)
```cx
// ❌ WRONG: Using 'when' in regular functions
function processRequest(request)
{
    when (request.priority == "high")  // ERROR! Use 'if' here
    {
        return "Processing high priority";
    }
}

// ✅ CORRECT: Use 'if' in regular functions
function processRequest(request)
{
    if (request.priority == "high")
    {
        emit "priority.request", request; // 'emit' is OK anywhere
        return "Processing high priority";
    }
}
```

#### 2. Incorrect `when` Usage (Standalone Code)
```cx
// ❌ WRONG: Using 'when' in standalone code
var score = calculateScore();
when (score > 0.8)  // ERROR! Use 'if' here
{
    emit "high.score", { score: score };
}

// ✅ CORRECT: Use 'if' in standalone code
var score = calculateScore();
if (score > 0.8)
{
    emit "high.score", { score: score }; // 'emit' is OK anywhere
}
```

#### 3. Missing Event Handler Context
```cx
// ❌ WRONG: 'when' without 'on' context
when (condition) { ... }  // ERROR! 'when' needs 'on' block

// ✅ CORRECT: 'when' inside 'on' event handler
on "data.received" (payload)
{
    when (payload.isValid)
    {
        emit "data.validated", payload;
    }
}
```

### ✅ CORRECT PATTERNS

#### Event-Driven Conditional Logic
```cx
on "user.input" (payload)
{
    // Use 'when' for event-driven conditionals
    when (payload.intent == "question")
    {
        emit "question.detected", payload;
    }
    
    when (payload.sentiment < 0.3)
    {
        emit "negative.sentiment", payload;
    }
}
```

#### Function-Level Conditional Logic
```cx
function analyzeData(data)
{
    // Use 'if' for function conditionals
    if (data.length == 0)
    {
        return null;
    }
    
    if (data.confidence > 0.9)
    {
        emit "high.confidence.result", data; // 'emit' works anywhere
        return data.result;
    }
    
    return data.fallback;
}
```

#### Class Methods with Event Integration
```cx
class DataProcessor
{
    threshold: number;
    
    constructor(threshold)
    {
        this.threshold = threshold;
    }
    
    function process(data)
    {
        // Use 'if' in class methods
        if (data.score > this.threshold)
        {
            emit "threshold.exceeded", { 
                processor: this, 
                data: data 
            }; // 'emit' works in class methods too
            return "processed";
        }
        
        return "rejected";
    }
    
    // Instance-level event receiver
    on "external.trigger" (payload)
    {
        // Use 'when' in instance event handlers
        when (payload.targetProcessor == this)
        {
            var result = this.process(payload.data);
            emit "processing.complete", result;
        }
    }
}
```

---

## CRITICAL SYNTAX RULE SUMMARY

### Event-Driven Architecture Keywords - Scoping Rules

| Keyword | Scope | Usage | Status |
|---------|-------|-------|--------|
| **`on`** | Global or Class Instance | Defines event receiver functions | ✅ OPERATIONAL |
| **`when`** | **ONLY inside `on` blocks** | Conditional logic within event handlers | ✅ OPERATIONAL |
| **`emit`** | **Globally available** | Event emission - usable anywhere | ✅ OPERATIONAL |
| **`if`** | **Everywhere EXCEPT `on` blocks** | Regular conditional logic | ✅ OPERATIONAL |

### Memory Aid for Developers

```cx
// ✅ ALWAYS CORRECT PATTERNS

// 1. Event handlers use 'when' for conditionals
on "event.name" (payload)
{
    when (condition) { /* event logic */ }
}

// 2. Functions and standalone code use 'if' for conditionals  
function name() 
{ 
    if (condition) { /* regular logic */ } 
}

// 3. 'emit' works everywhere - functions, classes, events, standalone
emit "event.name", data; // Valid anywhere in CX code

// 4. Class instances can have their own event receivers
class Agent
{
    on "message" (payload) 
    { 
        when (payload.target == this) { /* instance event logic */ } 
    }
}
```

### Phase 5 Status: Event-Driven Architecture ✅ COMPLETE
- Grammar implementation: **COMPLETE** ✅
- AST node generation: **COMPLETE** ✅  
- IL compilation: **COMPLETE** ✅
- Runtime event bus: **Pending implementation** ⏳

**Next Priority**: Runtime event subscription, emission, and dispatch system implementation.