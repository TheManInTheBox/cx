# CX Language Syntax Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Core Language Rules](#co## Syntax Basics

### Formatting Rules
// ❌ NEVER use K&R brackets in CX
if (condition)
{
    doSomething();
}

// ✅ Console output - ALWAYS use print()
print("Hello World");

// ✅ Allman-style brackets - MANDATORY in CX Language
if (condition)
{
    doSomething();
}

## Class and Member Declaration
3. [Syntax Basics](#syntax-basics)
4. [Class and Member Declaration](#class-and-member-declaration)
5. [Event System](#event-system)
6. [Service Integration](#service-integration)
7. [Asynchronous Programming](#asynchronous-programming)
8. [Loops and Iteration](#loops-and-iteration)
9. [Reserved Event Names](#reserved-event-names)
10. [Code Examples](#code-examples)

## Introduction
CX Language is an event-driven programming language designed for AI agent orchestration with built-in cognitive capabilities. This document provides the official syntax rules and coding patterns for CX Language development.

## Core Language Rules
The following rules are mandatory for all CX Language code:

### Syntax Requirements
- Always use Allman-style brackets `{ }` for code blocks
- Use `print()` for console output - NEVER use `console.log()`
- Use `:` for type annotations in field and method declarations
- Use `=` for default values in field declarations

### Variable Declarations
- Use `var` keyword for local variables inside methods/constructors
- Use `var` keyword for global variables at program scope
- **Local Variables**: Use `var` keyword for variables inside methods/constructors/functions
- **Global Variables**: Use `var` keyword for variables declared at program scope
- **Type Annotations**: Use `fieldName: type` for fields, `parameterName: type` for method parameters

### Class Structure
- Use `class` keyword for class declarations - NO `public` or `private` modifiers
- Use `function` keyword for method declarations inside classes
- Use `this` keyword to access class members (fields, methods, event handlers)
- **No Public/Private Modifiers**: CX does not use access modifiers - all members are accessible within class scope
- **No Static Members**: CX does not support static members - all members are instance-based
- **Field declarations**: Use `fieldName: type;` syntax for field declarations in classes
- **Constructor parameters**: Type annotations recommended for clarity
- **Method parameters**: Type annotations recommended for clarity

### Event System
- Use `on` keyword for event handlers - available at both program scope and in classes
- Use `emit` keyword for event emission - always fire-and-forget
- Use `any` for wildcard patterns in event handlers - supports cross-namespace communication
- DO NOT use * for wildcard event handlers - use specific namespaces
- Event names are case-sensitive and follow dot notation (namespace.action)
- Always use descriptive event names for maintainable code
- **Event handlers**: CANNOT be called directly - only invoked via `emit` statements

### Service Integration
- Use `uses` keyword for service declarations at program scope only - NEVER in classes
- **Service Declarations**: Use `uses` keyword at program scope only, NEVER in classes
- **Service injection**: ILLEGAL - classes cannot declare their own service instances
- **Service access**: ONLY via inheritance-based cognitive methods (`this.Think()`, `this.Generate()`, etc.) OR global service instances

## Syntax Basics
- **No Public/Private Modifiers**: CX does not use access modifiers - all members are accessible within class scope
- **No Static Members**: CX does not support static members - all members are instance-based
- **Variable Declaration**: Use `var identifier` for new loop variables
- **Existing Variables**: Can use existing variables without `var` keyword
- **Iteration Target**: Works with arrays, collections, and any iterable object
- **Loop Variable**: Automatically assigned each element during iteration
- **Block Syntax**: Must use Allman-style brackets `{ }` 
- **Scope**: Loop variables follow standard CX scoping rules
- **Type Safety**: Loop variables are dynamically typed, use `typeof()` for type checking
- **`uses` keyword**: ONLY allowed at program scope (top-level), NEVER inside classes
- **Service injection**: ILLEGAL - classes cannot declare their own service instances
- **Event handlers**: Available at both program scope AND in classes
- **Service access**: ONLY via inheritance-based cognitive methods (`this.Think()`, `this.Generate()`, etc.) OR global service instances
- **Class scope**: Fields, methods, constructors, event handlers only - NO `uses` statements or service declarations
- **Field declarations**: Use `fieldName: type;` syntax for field declarations in classes
- **Constructor parameters**: Type annotations recommended for clarity
- **Method parameters**: Type annotations recommended for clarity
- **Local variables**: Use `var` keyword for variables inside methods/constructors/functions
- **Global variables**: Use `var` keyword for variables declared at program scope
- **Async Functions**: Return void - all results delivered via event bus system
- **No Await**: Completely eliminated from CX language for pure fire-and-forget pattern
- **Event handlers**: Cannot return values, execute asynchronously
- **No await keyword**: Completely eliminated from CX language  
- **No return values**: Async functions return void, results via events
- **Fire-and-forget**: All cognitive operations non-blocking by default
- **Event coordination**: Results delivered through event bus system
- **Immediate responses**: Functions complete instantly, async work continues in background
- Reserved event names ensure consistent system behavior across all CX applications
- Custom event names can be used alongside reserved names
- Event names are case-sensitive and follow dot notation (namespace.action)
- Always use descriptive event names for maintainable code

### **Reserved Event Names**
The following event name parts are reserved by the CX Language system and have special meaning:

**System Events:**
- `system` - Core system lifecycle events (system.ready, system.shutdown)
- `alerts` - System alert and notification events
- `dev` - Development and debugging events

**User Interaction Events:**
- `user` - User input and interaction events (user.input, user.message, user.response)
- `ai` - AI processing and cognitive events (ai.request, ai.response, ai.thinking)

**Workflow Events:**
- `async` - Asynchronous operation events (async.complete, async.error)
- `sync` - Synchronous operation events (sync.complete, sync.ready)
- `tasks` - Task management events (tasks.created, tasks.completed)
- `tickets` - Ticket/issue tracking events
- `support` - Support system events

## Reserved Event Names
The following event name parts are reserved by the CX Language system and have special meaning:

**System Events:**
- `system` - Core system lifecycle events (system.ready, system.shutdown)
- `alerts` - System alert and notification events
- `dev` - Development and debugging events

**User Interaction Events:**
- `user` - User input and interaction events (user.input, user.message, user.response)
- `ai` - AI processing and cognitive events (ai.request, ai.response, ai.thinking)

**Workflow Events:**
- `async` - Asynchronous operation events (async.complete, async.error)
- `sync` - Synchronous operation events (sync.complete, sync.ready)
- `tasks` - Task management events (tasks.created, tasks.completed)
- `tickets` - Ticket/issue tracking events
- `support` - Support system events

**General Events:**
- `any` - Wildcard event matching
- `new` - Creation/initialization events
- `critical` - High-priority system events
- `assigned` - Assignment and delegation events

## Code Examples

### Basic Syntax Examples

```cx
// Basic formatting and syntax
if (condition)
{
    print("Hello World");
    doSomething();
}

// Variable declarations
var globalCounter = 0;
var systemStatus = "initialized";

function processData()
{
    var localVariable = "processing";
    print("Status: " + localVariable);
}
```

### Complete Agent Example

```cx
// Complete agent implementation with event handling
uses textService from Cx.AI.TextGeneration;

class AssistantAgent
{
    name: string;
    status: string = "idle";
    
    constructor(agentName: string)
    {
        this.name = agentName;
        print("Agent initialized: " + this.name);
    }
    
    function processMessage(message: string): void
    {
        this.status = "processing";
        
        // Fire-and-forget cognitive methods
        this.Think({ 
            handler: "thinking.complete", 
            prompt: message, 
            name: "user_input_analysis" 
        });
        
        emit agent.processing, { agent: this.name, message: message };
    }
    
    on user.message (payload)
    {
        print("Message received: " + payload.text);
        this.processMessage(payload.text);
    }
    
    on thinking.complete (payload)
    {
        print("Analysis complete");
        this.status = "ready";
        emit agent.response, { 
            agent: this.name, 
            response: payload.result,
            originalMessage: payload.prompt
        };
    }
}

// Create and use the agent
var assistant = new AssistantAgent("Helpful Assistant");
emit user.message, { text: "Can you help me with my project?" };
```

### Multi-Agent System Example

```cx
// System with multiple coordinating agents
class CoordinatorAgent
{
    on agent.response (payload)
    {
        print("Coordinator received response from: " + payload.agent);
        emit system.log, { message: "Response processed by coordinator" };
    }
}

class SpecialistAgent
{
    domain: string;
    
    constructor(specialistDomain: string)
    {
        this.domain = specialistDomain;
    }
    
    on user.query (payload)
    {
        if (payload.domain == this.domain)
        {
            print("Specialist handling query in domain: " + this.domain);
            this.Think({ 
                handler: "specialist.result", 
                prompt: payload.query 
            });
        }
    }
}

// Create the agent system
var coordinator = new CoordinatorAgent();
var techSpecialist = new SpecialistAgent("technology");
var financeSpecialist = new SpecialistAgent("finance");

// Use the system
emit user.query, { domain: "technology", query: "How do I optimize my code?" };
```

### **Class and Member Declaration Syntax**
```
// ✅ Class declaration with optional inheritance
class BasicClass 
{
    // Class members - fields, methods, constructors, event handlers only
}

// ✅ Class with inheritance
class CognitiveAgent : BaseAgent
{
    // ✅ Field declarations (instance variables)
    name: string;              // Basic field declaration with type
    status: string = "active"; // Field with default value
    count: number = 0;         // Numeric field with default
    
    // ✅ Constructor with parameters
    constructor(agentName: string, initialStatus: string)
    {
        this.name = agentName;
        this.status = initialStatus;
    }
    
    // ✅ Method declarations
    function processRequest(input: string): string
    {
        print("Processing: " + input);
        return "completed";
    }
    
    // ✅ Event handlers within class scope
    on user.message (payload) 
    {
        this.processRequest(payload.text);
    }
}

// ✅ Class with interface implementation
class MultimodalAgent : ITextToSpeech, IImageGeneration
{
    capabilities: string[];
    
    constructor(name: string)
    {
        this.name = name;
        this.capabilities = ["text", "speech", "image"];
        
        // ✅ Local variables inside methods/constructors
        var localCounter = 0;
        var tempValue = "processing";
    }
    
    function processContent(input: string): string
    {
        // ✅ Local variables inside methods
        var result = "";
        var processed = input.toLowerCase();
        
        this.Think(processed);
        return result;
    }
}

// ✅ Global variable declarations (program scope)
var globalCounter = 0;
var systemStatus = "initialized";
var agentRegistry = [];

// ✅ Service declarations (program scope only)
uses textService from Cx.AI.TextGeneration;
uses voiceService from Cx.AI.TextToSpeech;
```

## Loops and Iteration

### For...In Loop Syntax
```
// ✅ Basic for...in loop with var declaration
var items = ["apple", "banana", "cherry"];
for (var item in items) 
{
    print("Item: " + item);
    print("Type: " + typeof(item));
}

// ✅ For...in loop with existing variable
var result;
var searchResults = [
    { score: 0.95, content: "First result" },
    { score: 0.87, content: "Second result" }
];

for (result in searchResults) 
{
    print("Score: " + result.score);
    print("Content: " + result.content);
}

// ✅ For...in loop in methods
class DataProcessor 
{
    function processArray(data: any[]): void
    {
        for (var item in data) 
        {
            print("Processing: " + item);
            
            // Local variable inside loop
            var processed = item.toString().toUpperCase();
            print("Processed: " + processed);
        }
    }
}

// ✅ For...in loop with PowerShell results
on powershell.results (payload) 
{
    if (payload.outputs && payload.outputs.length > 0) 
    {
        for (var output in payload.outputs) 
        {
            print("PowerShell Output: " + output);
            
            if (typeof(output) == "string") 
            {
                print("Length: " + output.length);
            }
        }
    }
}
```

## Event System

// Event handlers - available at both program scope AND in classes
on user.input (payload) { ... }        // ✅ CORRECT: Program scope event handler
on system.ready (payload) { ... }      // ✅ CORRECT: Program scope event handler

class MyAgent 
{
    on user.message (payload) { ... }  // ✅ CORRECT: Class scope event handler
    on ai.request (payload) { ... }    // ✅ CORRECT: Class scope event handler
}

// Event emission using reserved event names
emit user.message, { text: "hello" };
emit system.shutdown, { reason: "maintenance" };

// Wildcard event patterns for cross-namespace communication - PRODUCTION READY ✅
on name.any.other.any.final (payload) { ... }     // Joins multiple namespaced event busses
on user.any.response (payload) { ... }             // Matches user.chat.response, user.voice.response, etc.
on system.any.ready (payload) { ... }              // Matches system.audio.ready, system.network.ready, etc.
on agent.any.thinking.any.complete (payload) { ... } // Complex wildcard pattern matching

// Advanced wildcard patterns - ALL SCOPES SUPPORTED ✅
on user.any.input (payload) { ... }                // Global wildcard - catches user.chat.input, user.voice.input, etc.
on ai.any.response (payload) { ... }               // AI service wildcard - catches all AI responses
on voice.any.command (payload) { ... }             // Voice command wildcard - universal command handler
on any.any.critical (payload) { ... }              // Ultra-flexible wildcard - catches any critical events
on any.any.any.complete (payload) { ... }          // Maximum flexibility - catches all completion events

// Class-level wildcards work alongside global wildcards
class ChatAgent 
{
    on user.any.input (payload) 
    {                  // Class-scoped wildcard handler
        print("Chat agent received: " + payload.message);
    }
    
    on voice.any.command (payload) 
    {               // Multi-scope wildcard support
        emit user.chat.message, { text: "Voice: " + payload.command };
    }
}
```

```cx
class MyAgent 
{
    on command.executed (payload) 
    {  // Registered in MyAgent namespace scope
        print("Command completed in MyAgent");
    }
    
    // Advanced wildcard handlers in class scope
    on user.any.input (payload) 
    {   // Class-scoped wildcard - catches all user inputs
        print("MyAgent received user input: " + payload.message);
    }
    
    on ai.any.response (payload) 
    {  // Class-scoped AI wildcard
        print("MyAgent processing AI response: " + payload.response);
    }
    
    function runCommand() 
    {
        this.Execute("some command");  // Emits to Global scope
    }
}

// Global scope wildcard handlers
on system.any.ready (payload) 
{  // Registered in global namespace scope
    print("System ready globally: " + payload.component);
}

on agent.any.thinking.any.complete (payload) 
{  // Complex global wildcard
    print("Agent thinking complete: " + payload.thought);
}

// Ultra-flexible wildcards for maximum event coverage
on any.any.critical (payload) 
{     // Catches ALL critical events system-wide
    print("CRITICAL EVENT: " + payload.message);
}
```

## Service Integration
```cx
// ✅ All classes inherit cognitive capabilities automatically
class CognitiveAgent  // No 'uses' declarations needed - intelligence is built-in!
{
    function processInput(userMessage)
    {
        // Default cognitive methods available to ALL classes - STRUCTURED PARAMETERS:
        this.Think({ handler: "thinking.complete", prompt: userMessage, name: "user_input_analysis" });
        this.Generate({ handler: "content.generated", prompt: userMessage, name: "response_generation" });
        this.Chat({ handler: "chat.sent", message: "Hello!", name: "greeting" });
        this.Communicate({ handler: "status.updated", message: "Processing...", name: "status_update" });
        
        // Personal memory with structured parameters
        this.Learn({
            handler: "knowledge.stored",
            content: userMessage,
            name: "user_input_learning",
            context: "cognitive_processing"
        });
        
        // Search with structured parameters
        this.Search({ handler: "results.found", query: "similar situations", name: "knowledge_lookup" });
        
        // Command execution with structured parameters
        this.Execute({ handler: "command.completed", command: "pwd", name: "directory_check" });
        
        // All results delivered via custom event handlers
    }
}
```

### **Service Method Structured Parameters**
```cx
// All service methods use structured parameters with handler and name properties:

search, { handler: "event.handle", query: "query" };
learn, { handler: "name.of.any.event.handle", name: "another property" };
think, { handler: "name.of.any.event.handle", name: "another property" };
execute, { handler: "name.of.any.handle", name: "another property", command: "pwd" };
communicate, { handler: "name.of.any.event.handle", name: "another property" };
generate, { handler: "name.of.any.event.handle", name: "another property" };
chat, { handler: "name.of.any.event.handle", name: "another property" };
speak, { handler: "name.of.any.event.handle", name: "another property" };
image, { handler: "name.of.any.event.handle", name: "another property" };
analyze, { handler: "name.of.any.event.handle", name: "another property" };
transcribe, { handler: "name.of.any.event.handle", name: "another property" };
audio, { handler: "name.of.any.event.handle", name: "another property" };
```

```cx
// ❌ ILLEGAL: 'uses' keyword inside class declarations
class MyAgent 
{
    uses aiService from Cx.AI.TextGeneration;  // ← COMPILATION ERROR
    
    function doSomething() { ... }
}

// ✅ CORRECT: 'uses' statements at program scope only
uses aiService from Cx.AI.TextGeneration;

class MyAgent 
{
    function doSomething() 
    {
        // ✅ CORRECT: Access built-in cognitive methods via inheritance - FIRE-AND-FORGET
        this.Think("prompt");      // ← Fire-and-forget cognitive methods
        this.Generate("query");    // ← Results via event bus, no blocking
        
        // ✅ CORRECT: Can access global service instances from program scope
        var generated = aiService.Generate("test");   // ← Global service instance
        
        // Immediate return, async work continues in background
        return "Processing initiated";
    }
}
```

## Asynchronous Programming
```cx
// ✅ CORRECT - Using reserved event names
on user.input (payload) { ... }          // User interaction
on system.ready (payload) { ... }        // System lifecycle  
on ai.request (payload) { ... }          // AI processing
on async.complete (payload) { ... }      // Async operations

// ✅ CORRECT - Custom namespaced events
on custom.event (payload) { ... }        // Custom events allowed
on myapp.data.updated (payload) { ... }  // Application-specific events

// ✅ CORRECT - Wildcard event patterns for cross-namespace communication
on name.any.other.any.final (payload) { ... }     // Joins multiple namespaced event busses
on user.any.response (payload) { ... }             // Matches user.chat.response, user.voice.response, etc.
on system.any.ready (payload) { ... }              // Matches system.audio.ready, system.network.ready, etc.
on agent.any.thinking.any.complete (payload) { ... } // Complex wildcard pattern matching
```

### **Event-Driven Async Pattern**


#### **🎯 CRITICAL LANGUAGE RULE: Async Functions Return No Values**
```cx
// ❌ OLD WAY: Complex await patterns with blocking and return values
async function processData(input) 
{
    var result = await this.Think(input);    // Blocking operation
    var learned = await this.Learn(result);  // Sequential blocking
    return { result, learned };              // Return values create complexity
}

// ✅ NEW WAY: Pure fire-and-forget with event bus coordination
function processData(input) 
{
    // All async operations fire-and-forget - no return values, no blocking
    this.Think(input);     // Fires cognitive processing, results via events
    this.Learn(input);     // Fires learning process, results via events
    this.Generate(input);  // Fires generation, results via events
    
    // Immediate response, async results delivered via event bus
    emit processing.started, { input: input };
}

// Results flow through event system
on ai.thought.complete (payload) 
{
    print("Thinking complete: " + payload.result);
    // Continue processing based on thought results...
}

on ai.learning.complete (payload) 
{
    print("Learning complete: " + payload.documentId);
    // Update UI or trigger next steps...
}
```

### **Specialized Capabilities via Interfaces**
```cx
// Optional interfaces for advanced features
class MultimodalAgent : ITextToSpeech, IImageGeneration
{
    function createContent(prompt)
    {
        // Core cognitive methods (inherited automatically) - FIRE-AND-FORGET
        this.Think(prompt);                    // Fire-and-forget thinking
        this.Communicate("Creating content...");  // Fire-and-forget communication
        
        // Specialized methods (only with interfaces) - FIRE-AND-FORGET
        this.Speak("Content created!");        // Requires ITextToSpeech
        this.CreateImage(prompt);              // Requires IImageGeneration
        
        // Personal memory tracking - FIRE-AND-FORGET
        this.Learn({
            prompt: prompt,
            context: "multimodal_creation"
        });
        
        // All results delivered via event bus system
        emit content.creation.started, { prompt: prompt };
    }
}
```

### **Agent Creation Patterns**
```cx
// ✅ REVOLUTIONARY: All classes inherit cognitive capabilities automatically!
class CognitiveAgent  // No 'uses' declarations needed - intelligence is built-in!
{
    function processInput(userMessage)
    {
        // Default cognitive methods available to ALL classes - FIRE-AND-FORGET:
        this.Think(userMessage);        // Fire-and-forget thinking - results via event bus
        this.Generate(userMessage);     // Fire-and-forget generation - results via event bus
        this.Chat("Hello!");            // Fire-and-forget conversation - results via event bus
        this.Communicate("Processing...");  // Fire-and-forget communication - results via event bus
        
        // Personal memory - fire-and-forget learning
        this.Learn({
            input: userMessage,
            response: "processing",
            context: "cognitive_processing"
        });
        
        // Fire-and-forget search - results delivered via events
        this.Search("similar situations");
        
        // No return values needed - all results flow through event bus
    }
}

// Event handlers automatically register as agents
class EventAgent
{
    on user.input (payload)
    {
        // Event handler presence triggers automatic agent registration
        this.Think(payload.message);  // Fire-and-forget cognition!
        emit user.response, { text: "processing..." }; // Immediate response, AI results via events
    }
}
var myAgent = new EventAgent(); // ← Automatically registered as agent
```

#### **🎯 CRITICAL LANGUAGE RULE: Async Functions Return No Values**
```cx
// ❌ OLD WAY: Complex await patterns with blocking and return values
async function processData(input) 
{
    var result = await this.Think(input);    // Blocking operation
    var learned = await this.Learn(result);  // Sequential blocking
    return { result, learned };              // Return values create complexity
}

// ✅ NEW WAY: Pure fire-and-forget with event bus coordination
function processData(input) 
{
    // All async operations fire-and-forget - no return values, no blocking
    this.Think(input);     // Fires cognitive processing, results via events
    this.Learn(input);     // Fires learning process, results via events
    this.Generate(input);  // Fires generation, results via events
    
    // Immediate response, async results delivered via event bus
    emit processing.started, { input: input };
}

// Results flow through event system
on ai.thought.complete (payload) 
{
    print("Thinking complete: " + payload.result);
    // Continue processing based on thought results...
}

on ai.learning.complete (payload) 
{
    print("Learning complete: " + payload.documentId);
    // Update UI or trigger next steps...
}
```

### **Specialized Capabilities via Interfaces**
```cx
// Optional interfaces for advanced features
class MultimodalAgent : ITextToSpeech, IImageGeneration
{
    function createContent(prompt)
    {
        // Core cognitive methods (inherited automatically) - FIRE-AND-FORGET
        this.Think(prompt);                    // Fire-and-forget thinking
        this.Communicate("Creating content...");  // Fire-and-forget communication
        
        // Specialized methods (only with interfaces) - FIRE-AND-FORGET
        this.Speak("Content created!");        // Requires ITextToSpeech
        this.CreateImage(prompt);              // Requires IImageGeneration
        
        // Personal memory tracking - FIRE-AND-FORGET
        this.Learn({
            prompt: prompt,
            context: "multimodal_creation"
        });
        
        // All results delivered via event bus system
        emit content.creation.started, { prompt: prompt };
    }
}
```

```cx
// ❌ ILLEGAL: 'uses' keyword inside class declarations
class MyAgent 
{
    uses aiService from Cx.AI.TextGeneration;  // ← COMPILATION ERROR
    
    function doSomething() { ... }
}

// ✅ CORRECT: 'uses' statements at program scope only
uses aiService from Cx.AI.TextGeneration;

class MyAgent 
{
    function doSomething() 
    {
        // ✅ CORRECT: Access built-in cognitive methods via inheritance - FIRE-AND-FORGET
        this.Think("prompt");      // ← Fire-and-forget cognitive methods
        this.Generate("query");    // ← Results via event bus, no blocking
        
        // ✅ CORRECT: Can access global service instances from program scope
        var generated = aiService.Generate("test");   // ← Global service instance
        
        // Immediate return, async work continues in background
        return "Processing initiated";
    }
}
```
