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
// Event handlers (Aura sensory layer) - FULLY OPERATIONAL
on "event.name" (payload)
{
    // Handle incoming event
    print("Received: " + payload.data);
    
    // CRITICAL: Use 'if' for conditionals everywhere
    if (payload.priority > 5)
    {
        emit "high.priority", payload;
    }
    
    if (payload.type == "urgent")
    {
        // More event-driven logic
        emit "escalation.needed", { urgency: "high" };
    }
}

// ✅ NEW: Class-Based Event Handlers (Auto-Registration)
class Agent
{
    name: string;
    
    constructor(agentName)
    {
        this.name = agentName;
    }
    
    // Instance-level event receivers auto-register with namespace bus
    on support.tickets.new (payload)
    {
        if (payload.priority == "critical")
        {
            emit alerts.critical, {
                ticketId: payload.ticketId,
                escalatedBy: this.name
            };
        }
    }
    
    // Wildcard handlers for cross-namespace events
    on any.critical (payload)
    {
        // Receives system.critical, alerts.critical, dev.critical, etc.
        print("🚨 " + this.name + " handling critical event from any namespace");
    }
}

// Event emission (globally available) - FULLY OPERATIONAL
emit "event.name", payload;
emit "notification", { message: "Hello", timestamp: "now" };

// ✅ NEW: Extended Event Name Grammar
// Supports keywords: new, critical, assigned, tickets, tasks, support, dev, system, alerts
emit support.tickets.new, { ticketId: "T-001", priority: "high" };
emit dev.tasks.assigned, { taskId: "TASK-123", assignee: "developer" };
emit system.critical, { server: "web-01", issue: "high memory usage" };
```

**CRITICAL SCOPING RULES:**
- **`on`**: Defines event receiver functions (global or class instance level)
- **`emit`**: Event emission - **globally available everywhere** (functions, classes, standalone code, event handlers)
- **`if`**: Regular conditional logic - **used everywhere** for all conditionals
- **Event Names**: **UNQUOTED** dot-separated identifiers with keyword support
  - ✅ `on support.tickets.new (payload) { ... }`
  - ✅ `emit dev.tasks.assigned, { taskId: "T-123" };`
  - ✅ `on any.critical (payload) { ... }` - matches ALL namespace critical events

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
- `emit` - event emission: **globally available anywhere** (functions, classes, standalone code, event handlers) (✅ FULLY OPERATIONAL - event-driven architecture foundation)
- `if` - conditional logic: **used everywhere** for all conditionals (✅ FULLY OPERATIONAL)
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

### ✅ CORRECT CONDITIONAL PATTERNS

#### Universal 'if' Usage (All Contexts)
```cx
// ✅ CORRECT: 'if' in regular functions
function processRequest(request)
{
    if (request.priority == "high")
    {
        emit priority.request, request; // 'emit' is OK anywhere
        return "Processing high priority";
    }
}

// ✅ CORRECT: 'if' in standalone code
var score = calculateScore();
if (score > 0.8)
{
    emit high.score, { score: score };
}

// ✅ CORRECT: Use 'if' in standalone code
var score = calculateScore();
if (score > 0.8)
{
}

// ✅ CORRECT: 'if' in event handlers too
on data.received (payload)
{
    if (payload.isValid)
    {
        emit data.validated, payload;
    }
}
```

### ✅ CORRECT PATTERNS

#### Event-Driven Conditional Logic
```cx
on "user.input" (payload)
{
    // Use 'if' for event-driven conditionals
    if (payload.intent == "question")
    {
        emit "question.detected", payload;
    }
    
    if (payload.sentiment < 0.3)
    {
        emit negative.sentiment, payload;
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
        emit high.confidence.result, data; // 'emit' works anywhere
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
            emit threshold.exceeded, { 
                processor: this, 
                data: data 
            }; // 'emit' works in class methods too
            return "processed";
        }
        
        return "rejected";
    }
    
    // Instance-level event receiver
    on external.trigger (payload)
    {
        // Use 'if' in instance event handlers
        if (payload.targetProcessor == this)
        {
            var result = this.process(payload.data);
            emit processing.complete, result;
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
| **`emit`** | **Globally available** | Event emission - usable anywhere | ✅ OPERATIONAL |
| **`if`** | **Everywhere** | Universal conditional logic | ✅ OPERATIONAL |

### Memory Aid for Developers

```cx
// ✅ ALWAYS CORRECT PATTERNS

// 1. Event handlers use 'if' for conditionals
on event.name (payload)
{
    if (condition) { /* event logic */ }
}

// 2. Functions and standalone code use 'if' for conditionals  
function name() 
{ 
    if (condition) { /* regular logic */ } 
}

// 3. 'emit' works everywhere - functions, classes, events, standalone
emit event.name, data; // Valid anywhere in CX code

// 4. Class instances can have their own event receivers
class Agent
{
    on message (payload) 
    { 
        if (payload.target == this) { /* instance event logic */ } 
    }
}
```

### Phase 5 Status: EVENT-DRIVEN ARCHITECTURE ✅ BREAKTHROUGH COMPLETE

#### 🏆 MAJOR ACHIEVEMENT - Complete Event-Driven Architecture Operational
- **Native `emit` Syntax**: `emit event.name, payload` - **WORKING PERFECTLY** ✅
- **Class-Based Event Handlers**: `on event.name (payload) { ... }` inside classes ✅
- **Auto-Registration**: Agents automatically register with namespace bus based on `on` handlers ✅
- **Namespace Scoping**: Event names as namespaces with dot-notation routing ✅
- **Wildcard Support**: `any.critical` matching ALL namespace critical events ✅
- **Grammar Enhancement**: Extended eventNamePart to support keywords like 'new', 'critical', 'assigned' ✅

#### ✅ COMPLETE - Event-Driven Foundation
- Grammar implementation: **COMPLETE** ✅ (`on`, `emit`, `if` + extended keywords)
- AST node generation: **COMPLETE** ✅  
- IL compilation: **COMPLETE** ✅
- Runtime event bus: **OPERATIONAL** ✅
- Working demonstration: **FULLY OPERATIONAL** ✅ (`proper_event_driven_demo.cx`)

#### ✅ COMPLETE - Agent Auto-Registration System
- **Instance Event Handlers**: `on` statements inside classes auto-register agents ✅
- **Namespace-Based Routing**: Events routed by namespace patterns (support.*, dev.*, system.*) ✅
- **Wildcard Registration**: `any.critical` registers for ALL namespace critical events ✅
- **Zero Manual Registration**: No `RegisterNamespacedAgent()` calls needed ✅

#### 🎯 NEXT PRIORITY - Enhanced Wildcard Matching & Cross-Namespace Routing
- **Wildcard Event Delivery**: `any.critical` should receive `system.critical`, `alerts.critical`, etc.
- **Cross-Namespace Patterns**: Advanced routing patterns for multi-agent coordination
- **Dynamic Agent Management**: Runtime addition/removal of agents from event bus
- **Event Bus Statistics**: Real-time monitoring of agent registrations and event flows

**Strategic Impact**: CX Language is now a **true autonomous programming platform** where agents can be created with simple, intuitive syntax and operate with full AI capabilities.