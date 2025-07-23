# CX Language Syntax Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Core Language Rules](#co## Syntax Basics

### Formatting Rules
```cx
// ❌ NEVER use traditional if statements in CX
if (condition) {
    doSomething();
}

// ❌ NEVER use \n or escape sequences when using print().

// ✅ Console output - ALWAYS use print()
print("Hello World");

// ✅ Cognitive boolean logic with Allman-style brackets - MANDATORY in CX Language
is { 
    context: "Decision context description",
    evaluate: "What is being evaluated",
    data: { condition: true },
    handlers: [ decision.ready ]
};  // ✅ Semicolon ends the decision - triggers events only
```

## Class and Member Declaration
3. [Syntax Basics](#syntax-basics)
4. [Cognitive Boolean Logic](#cognitive-boolean-logic)
5. [Class and Member Declaration](#class-and-member-declaration)
6. [Event System](#event-system)
7. [Service Integration](#service-integration)
8. [Asynchronous Programming](#asynchronous-programming)
9. [Loops and Iteration](#loops-and-iteration)
10. [Reserved Event Names](#reserved-event-names)
11. [Code Examples](#code-examples)

## Introduction
CX Language is an event-driven programming language designed for AI agent orchestration with built-in cognitive capabilities. This document provides the official syntax rules and coding patterns for CX Language development.

**Key Features:**
- **Cognitive Boolean Logic**: `is { }` syntax for AI-driven decision-making with contextual evaluation
- **Negative Cognitive Logic**: `not { }` syntax for AI-driven false/negative decision-making
- **Advanced Event System**: Full event parameter property access with `event.property` syntax
- **Enhanced Handlers Pattern**: Custom payload support with `handlers: [ event.name { custom: "data" } ]`
- **Voice Processing**: Azure OpenAI Realtime API integration via event system (`emit realtime.connect`, `realtime.text.send`, `realtime.audio.response`)
- **Automatic Object Serialization**: CX objects print as readable JSON with recursive nesting support
- **Dictionary Iteration**: Native support for iterating over dictionaries in for-in loops
- **Dynamic Property Access**: Runtime property resolution for flexible event handling
- **KeyValuePair Support**: Automatic handling of dictionary entries with `.Key` and `.Value` access
- **Fire-and-Forget AI Operations**: Non-blocking cognitive methods with event-based results
- **Serializable Object Parameters**: Pass complex data structures to services using object literals
- **Comma-less Syntax**: Modern clean syntax for AI services and emit statements

## Core Language Rules
The following rules are mandatory for all CX Language code:

### Syntax Requirements
- Always use Allman-style brackets `{ }` for code blocks
- Use `print()` for console output - NEVER use `console.log()`
- `print()` automatically serializes complex objects to JSON for debugging
- `print()` displays primitive types (strings, numbers, booleans) directly
- `print()` provides nested object visualization for CX classes
- Use `:` for type annotations in field and method declarations
- Use `=` for default values in field declarations

### Enhanced Object Printing
- **JSON String Output**: The `print()` function always returns values in JSON string format for consistent output formatting
- **Automatic Object Serialization**: All CX objects are automatically serialized to JSON when printed
- **Primitive Type Detection**: Strings, numbers, and booleans print directly without JSON formatting
- **Nested Object Support**: CX objects containing other CX objects display full recursive structure
- **Clean Field Filtering**: Internal fields (ServiceProvider, Logger) are automatically hidden
- **Debugging Ready**: Perfect for inspecting complex agent states and data structures
- **Example Output**: `{"name": "Alice", "age": 30, "data": {"title": "Sample", "active": true}}`
- **Introspection as Code**: Understanding an agent's state is critical for debugging and for the agent's own self-reflection capabilities. Cx elevates this - **Automatic Object Serialization**. Any Cx object, when passed to the `print()` function, is automatically serialized to a clean, human-readable JSON representation.

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
- Only `on system` eventhandlers are allowed in Program scope. No exceptions.
- Use `on` keyword for event handlers - available at both program scope and in classes
- Use `emit` keyword for event emission - always fire-and-forget
- Use `any` for wildcard patterns in event handlers - supports cross-namespace communication
- DO NOT use * for wildcard event handlers - use specific namespaces
- Event names are case-sensitive and follow dot notation (namespace.action)
- Always use descriptive event names for maintainable code
- **Event handlers**: CANNOT be called directly - only invoked via `emit` statements
- **Event Parameter Access**: Use `event.propertyName` for direct property access
- **Dictionary Iteration**: Use `for (var item in event.payload)` to iterate over event data
- **KeyValuePair Properties**: Access dictionary entries with `item.Key` and `item.Value`

### Service Integration
- **Automatic Injection**: All cognitive services are automatically injected into the program's global scope at runtime. There is no need to declare or import them.
- **Global Availability**: Services are available as globally accessible functions throughout the application.
- **No Class-level Injection**: It is illegal for classes to have services injected into them or to declare their own service instances.
- **Invocation**: Services are called using their name followed by a comma-less block of parameters, e.g., `think { prompt: "Hello" }`.
- **Serializable Object Parameters**: For complex inputs, pass a serializable object instead of concatenating strings, e.g., `think { prompt: { text: "Analyze this", context: "Full context here" } }`.
- **Core Cognitive Services**: `is` (cognitive boolean logic), `not` (negative boolean logic), `learn` (knowledge acquisition), `think` (reasoning), `await` (smart timing), `adapt` (behavioral evolution)

## Syntax Basics
- **Use constructors to inject data** - DO NOT ACCESS MEMBERS DIRECTLY FOR INITIALIZATION.
- **`classes`**: Use `class` keyword for class declarations - NO `public` or `private` modifiers
- **`classes` behavior**: Classes are blueprints for objects that encapsulate state and behavior. They do not inherit cognitive capabilities automatically.
- **No Public/Private Modifiers**: Cx does not use access modifiers - all members are accessible within class scope
- **No Static Members**: CX does not support static members - all members are instance-based
- **Variable Declaration**: Use `var identifier` for new loop variables
- **Existing Variables**: Can use existing variables without `var` keyword
- **Iteration Target**: Works with arrays, collections, and any iterable object
- **Loop Variable**: Automatically assigned each element during iteration
- **Block Syntax**: Must use Allman-style brackets `{ }` 
- **Scope**: Loop variables follow standard CX scoping rules
- **Type Safety**: Loop variables are dynamically typed, use `typeof()` for type checking
- **Service access**: Cognitive services are globally available functions. They are not methods and are not accessed with `this.`
- **Service injection**: ILLEGAL - classes cannot declare their own service instances
- **Event handlers**: Available at both program scope AND in classes
- **Class scope**: Fields, methods, constructors, event handlers only - NO service declarations
- **Field declarations**: Use `fieldName: type;` syntax for field declarations in classes
- **Constructor parameters**: Type annotations recommended for clarity
- **Method parameters**: Type annotations recommended for clarity
- **Local variables**: Use `var` keyword for variables inside methods/constructors/functions
- **Global variables**: Use `var` keyword for variables declared at program scope
- **Functions**: Return void - all results delivered via event bus system
- **Pure Fire-and-Forget**: All cognitive operations non-blocking by default
- **Event handlers**: Cannot return values, execute asynchronously
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

**General Events:**
- `any` - Wildcard event matching

## Code Examples

### Basic Syntax Examples

```cx
// Basic formatting and syntax - using cognitive boolean logic
is { 
    context: "Should the system proceed with operation?",
    evaluate: "System readiness check",
    data: { condition: true },
    handlers: [ system.decision.ready ]
};  // ✅ Semicolon ends the decision - triggers events only

// Negative cognitive boolean logic
not { 
    context: "Should the system halt operation?",
    evaluate: "System failure check", 
    data: { condition: false },
    handlers: [ system.decision.negative ]
};  // ✅ Semicolon ends the decision - triggers events only

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
// Complete agent implementation with enhanced handlers and object printing

class AssistantAgent
{
    name: string;
    status: string = "idle";
    
    constructor(agentName: string)
    {
        this.name = agentName;
        print("Agent initialized: " + this.name);
    }
    
    function processMessage(message: string)
    {
        this.status = "processing";
        
        // Enhanced cognitive methods with custom payload handlers
        var promptObject = {
            message: message,
            context: "User interaction with an assistant agent."
        };

        think { 
            prompt: promptObject, 
            name: "user_input_analysis",
            handlers: [ 
                thinking.complete { option: "detailed" },
                analysis.logged { level: "info" }
            ]
        };
        
        emit agent.processing { agent: this.name, message: message };
    }
    
    on user.message (event)
    {
        print("Message received: " + event.text);
        print("Event details:");
        print(event);  // Automatic JSON serialization of event object
        this.processMessage(event.text);
    }
    
    on thinking.complete (event)
    {
        print("Analysis complete with option: " + event.option);
        this.status = "ready";
        print("Agent state:");
        print(this);  // Automatic JSON serialization of agent object
        
        emit agent.response { 
            agent: this.name, 
            response: event.result,
            originalMessage: event.prompt,
            analysisType: event.option
        };
    }
    
    on analysis.logged (event)
    {
        print("Analysis logged at level: " + event.level);
    }
}

// Create and use the enhanced agent
var assistant = new AssistantAgent("Helpful Assistant");

// Demonstrate object printing
print("Creating agent with enhanced features:");
print(assistant);  // Shows JSON: {"name": "Helpful Assistant", "status": "idle"}

// Send a message to trigger enhanced handlers
emit user.message { text: "Can you help me with my project?", user: "Alice", priority: "high" };
```

### Multi-Agent System Example

```cx
// System with multiple coordinating agents
class CoordinatorAgent
{
    on agent.response (payload)
    {
        print("Coordinator received response from: " + payload.agent);
        emit system.log { message: "Response processed by coordinator" };
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
            think { 
                handlers: [specialist.result], 
                prompt: payload.query 
            };
        }
    }
}

// Create the agent system
var coordinator = new CoordinatorAgent();
var techSpecialist = new SpecialistAgent("technology");
var financeSpecialist = new SpecialistAgent("finance");

// Use the system
emit user.query { domain: "technology", query: "How do I optimize my code?" };
```

### Voice Agent Example (Azure Realtime API)

```cx
// PRODUCTION-READY: Voice agent with Azure OpenAI Realtime API integration
// Uses actual Azure WebSocket endpoints for real-time voice processing
class VoiceAgent
{
    name: string = "VoiceAgent";
    
    function startVoiceSession()
    {
        print("� Starting voice session...");
        
        // ✅ PROVEN WORKING: Connect to Azure Realtime API first
        emit realtime.connect { demo: "voice_agent" };
    }
    
    function sendVoiceMessage(text: string)
    {
        print("🔊 Sending voice message: " + text);
        
        // ✅ PROVEN WORKING: Send text to Azure for voice synthesis
        emit realtime.text.send { 
            text: text,
            deployment: "gpt-4o-mini-realtime-preview"
        };
    }
    
    // ✅ PROVEN WORKING: Azure Realtime API connection handler
    on realtime.connected (event)
    {
        print("✅ Azure Realtime connected - creating session");
        emit realtime.session.create { 
            deployment: "gpt-4o-mini-realtime-preview",
            mode: "voice"
        };
    }
    
    // ✅ PROVEN WORKING: Voice session creation handler
    on realtime.session.created (event)
    {
        print("✅ Voice session created - ready for voice input/output");
        this.sendVoiceMessage("Hello, how can I assist you today?");
    }
    
    // ✅ PROVEN WORKING: Real-time text response handler
    on realtime.text.response (event)
    {
        print("✅ Voice response received: " + event.content);
        print("  Complete: " + event.isComplete);
        
        if (event.isComplete)
        {
            print("🎉 Voice interaction complete!");
        }
    }
    
    // ✅ FIXED: Real-time audio response handler with proper type safety
    // CRITICAL: Safe property access for audio data to prevent InvalidCastException
    on realtime.audio.response (event)
    {
        // ✅ FIXED: Safe audio data check without .length property access
        if (event.audioData != null)
        {
            print("🔊 Audio response received - data available");
            print("📊 Audio data type: " + typeof(event.audioData));
        }
        else
        {
            print("🔊 Audio response received - no data");
        }
        
        if (event.isComplete)
        {
            print("🎵 Voice audio synthesis complete!");
        }
    }
}

// ✅ PRODUCTION USAGE: Create and start voice agent
var voiceAgent = new VoiceAgent();
voiceAgent.startVoiceSession();

// Example response: "Hello! How can I assist you today?" with 88,800 bytes of audio
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
    function processRequest(input: string)
    {
        print("Processing: " + input);
    }
    
    // ✅ Event handlers within class scope
    on user.message (payload) 
    {
        this.processRequest(payload.text);
    }
}

// ✅ Global variable declarations (program scope)
var globalCounter = 0;
var systemStatus = "initialized";
var agentRegistry = [];
```

## Loops and Iteration

### For...In Loop Syntax
```
// ✅ Basic for...in loop with var declaration for arrays
var items = ["apple", "banana", "cherry"];
for (var item in items) 
{
    print("Item: " + item);
    print("Type: " + typeof(item));
}

// ✅ For...in loop with existing variable for arrays
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

// ✅ For...in loop for dictionary iteration (event payloads) - in class scope
class EventProcessor
{
    on user.input (event)
    {
        for (var eve in event.payload) 
        {
            print("Key: " + eve.Key);      // Access dictionary key
            print("Value: " + eve.Value);  // Access dictionary value
            print("Type: " + typeof(eve.Value));
        }
    }
}

// ✅ For...in loop in methods
class DataProcessor 
{
    function processArray(data: any[])
    {
        for (var item in data) 
        {
            print("Processing: " + item);
            
            // Local variable inside loop
            print("Processed: " + processed);
        }
    }
    
    function processDictionary(data: object)
    {
        for (var kvp in data) 
        {
            print("Processing key: " + kvp.Key);
            print("Processing value: " + kvp.Value);
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

### Dictionary Iteration Rules
- **Dictionary Support**: For-in loops work with both arrays and `Dictionary<string, object>` 
- **KeyValuePair Objects**: Dictionary iteration produces `KeyValuePair<string, object>` objects
- **Property Access**: Use `.Key` and `.Value` properties to access dictionary entries
- **Event Payloads**: Event data is stored as dictionaries, making `for (var item in event.payload)` possible
- **Runtime Detection**: Compiler automatically detects dictionary vs array and uses appropriate iteration strategy
- **Type Safety**: Loop variables are dynamically typed, use `typeof()` for type checking

## Cognitive Boolean Logic

CX Language features intelligent decision-making through cognitive boolean logic using the `is { }` syntax. This replaces traditional `if (condition)` statements with AI-driven contextual evaluation.

### Cognitive Boolean Syntax
```cx
// ❌ Traditional boolean logic (avoid in CX)
if (event.reason.indexOf(this.name) >= 0)
{
    doSomething();
}

// ✅ Cognitive boolean logic - AI-driven decision making
is { 
    context: "Cognitive decision: Should agent proceed?",
    evaluate: event.reason + " contains agent name " + this.name,
    data: { eventReason: event.reason, agentName: this.name },
    handlers: [ decision.ready ]
};  // ✅ Semicolon ends the decision - triggers events only

// ✅ Negative cognitive boolean logic - AI-driven false/negative decision making
not { 
    context: "Cognitive decision: Should agent NOT proceed?",
    evaluate: event.reason + " excludes agent name " + this.name,
    data: { eventReason: event.reason, agentName: this.name },
    handlers: [ decision.negative ]
};  // ✅ Semicolon ends the decision - triggers events only
```

### Cognitive Boolean Features
- **Contextual Evaluation**: Each `is { }` block includes descriptive context for the decision
- **Natural Language Logic**: Uses descriptive evaluation criteria instead of low-level operations
- **Rich Data Context**: Provides structured information for cognitive evaluation
- **Event-Driven Results**: Can emit events via `handlers` for further processing
- **AI-Native Decision Making**: Decisions can learn and adapt over time

### Cognitive Boolean Pattern Structure

The cognitive boolean pattern follows a structured format for AI-driven decision making:

```cx
is { 
    context: "Clear description of the decision being made",
    evaluate: "Natural language evaluation criteria",
    data: { key: value, contextData: data },
    handlers: [ event.name, another.event { custom: "payload" } ]
};  // ✅ Semicolon ends the decision - triggers handlers only
```

#### Pattern Components
- **`context`**: Descriptive text explaining the decision context
- **`evaluate`**: Natural language evaluation criteria
- **`data`**: Structured data object providing context for the decision
- **`handlers`**: Array of events to trigger when evaluation is true

### Cognitive Boolean Examples

```cx
class SmartAgent
{
    name: string = "Alice";
    
    on user.message (event)
    {
        // ✅ Cognitive decision about message relevance
        is { 
            context: "Should this agent respond to the user message?",
            evaluate: "Message relevance check for " + this.name,
            data: { message: event.text, agent: this.name, priority: event.priority },
            handlers: [ response.decision.made, message.evaluated { urgency: "high" } ]
        };  // ✅ Event-only execution - no code block
    }
    
    // ✅ Event handler for processing the decision result
    on response.decision.made (event)
    {
        print("Processing relevant message for " + this.name);
        print("Message: " + event.message);
        print("Priority: " + event.priority);
    }
    
    on audio.response (event)
    {
        // ✅ Cognitive decision about audio data processing
        is { 
            context: "Is audio data available for processing?",
            evaluate: "Audio data presence check for " + this.name,
            data: { audioData: event.audioData, agent: this.name },
            handlers: [ audio.data.evaluated ]
        };  // ✅ Event-only execution
        
        // ✅ Cognitive decision about completion status
        is { 
            context: "Is the audio processing complete?",
            evaluate: "Completion status evaluation for " + this.name,
            data: { isComplete: event.isComplete, agent: this.name },
            handlers: [ completion.evaluated ]
        };  // ✅ Event-only execution
    }
    
    // ✅ Event handlers for processing cognitive decisions
    on audio.data.evaluated (event)
    {
        print("Processing audio data for " + this.name);
        emit audio.process { data: event.audioData, agent: this.name };
    }
    
    on completion.evaluated (event)
    {
        print("Audio processing complete for " + this.name);
        emit task.finished { agent: this.name, task: "audio_processing" };
    }
}
```

### Cognitive Boolean Rules
- **Replace Traditional If**: Use `is { }` and `not { }` instead of `if (condition)` for all decision logic
- **Context Required**: Always provide meaningful context describing the decision being made
- **Descriptive Evaluation**: Use natural language descriptions instead of technical operations
- **Data Structure**: Include relevant data for the cognitive evaluation
- **Event Integration**: Use `handlers` to emit events based on cognitive decisions
- **AI-First Logic**: Design decisions that can evolve and improve over time
- **Positive vs Negative**: Use `is { }` for positive decisions, `not { }` for negative decisions

### Cognitive Boolean Behavior
- **`is { }` - When Evaluation is True**: The `handlers` are called
- **`is { }` - When Evaluation is False**: The `handlers` are NOT called
- **`not { }` - When Evaluation is False**: The `handlers` are called  
- **`not { }` - When Evaluation is True**: The `handlers` are NOT called
- **Event-Only Execution**: Unlike traditional `if` statements, cognitive boolean logic only triggers events, not code blocks
- **Pure Event-Driven**: All logic flows through the event system for maximum flexibility and AI integration

### Example Behavior Pattern
```cx
on preparation.complete (event)
{
    is { 
        context: "Cognitive decision: Should " + this.name + " proceed to speech phase?",
        evaluate: event.reason + " contains agent name " + this.name,
        data: { eventReason: event.reason, agentName: this.name, timing: event.actualDurationMs },
        handlers: [ agent.decision.ready ]  // ✅ Called ONLY if evaluation is true
    };  // ✅ Note: semicolon ends the cognitive decision - no code block
}
```

### Cognitive Boolean Logic Pattern

The cognitive boolean logic pattern is a fundamental CX Language feature that replaces traditional `if` statements with AI-driven decision making:

```cx
// Positive cognitive logic
is { 
    context: "Clear description of the decision being made",
    evaluate: "Natural language evaluation criteria",
    data: { key: value, contextData: data },
    handlers: [ event.name, another.event { custom: "payload" } ]
};  // ✅ Semicolon ends the decision - triggers handlers only when TRUE

// Negative cognitive logic  
not {
    context: "Clear description of the negative decision being made", 
    evaluate: "Natural language evaluation criteria for false condition",
    data: { key: value, contextData: data },
    handlers: [ event.name, another.event { custom: "payload" } ]
};  // ✅ Semicolon ends the decision - triggers handlers only when FALSE
```

#### Pattern Behavior
- **`is { }` - When Evaluation is True**: The `handlers` are called
- **`is { }` - When Evaluation is False**: The `handlers` are NOT called
- **`not { }` - When Evaluation is False**: The `handlers` are called
- **`not { }` - When Evaluation is True**: The `handlers` are NOT called
- **Event-Only Execution**: Unlike traditional `if` statements, cognitive boolean logic only triggers events, not code blocks
- **Pure Event-Driven**: All logic flows through the event system for maximum flexibility and AI integration

#### Example Behavior Pattern
```cx
on preparation.complete (event)
{
    is { 
        context: "Cognitive decision: Should " + this.name + " proceed to speech phase?",
        evaluate: event.reason + " contains agent name " + this.name,
        data: { eventReason: event.reason, agentName: this.name, timing: event.actualDurationMs },
        handlers: [ agent.decision.ready ]  // ✅ Called ONLY if evaluation is true
    };  // ✅ Note: semicolon ends the cognitive decision - no code block
}
```

## Event System

### Event Parameter Property Access
```
// ✅ Direct property access on event parameters
class EventAgent
{
    on user.input (event)
    {
        print("Message: " + event.message);    // Direct property access
        print("Type: " + event.type);          // Access any property from payload
        print("User: " + event.user);          // Runtime property resolution

        print(event);                           // Auto deserialize to JSON
    }
    
    on system.data (event)
    {
        print("Payload: " + event.payload);    // Access the full payload dictionary
        print("Name: " + event.name);          // Access event name
        print("Timestamp: " + event.timestamp); // Access event metadata
    }
}

// ✅ Dictionary iteration over event payload
class DataProcessor
{
    on data.received (event)
    {
        print("Processing event payload:");
        
        // Iterate over all properties in the event payload
        for (var item in event.payload)
        {
            print("Key: " + item.Key);          // Dictionary key
            print("Value: " + item.Value);      // Dictionary value
            print("Type: " + typeof(item.Value)); // Value type checking
        }
    }
}

// ✅ Nested property access
class ComplexEventHandler
{
    on api.response (event)
    {
        // Access nested object properties
        if (event.response && event.response.data)
        {
            print("Status: " + event.response.status);
            print("Data: " + event.response.data.content);
        }
        
        // Safe property access with type checking
        if (typeof(event.user) == "object")
        {
            print("User ID: " + event.user.id);
            print("User Name: " + event.user.name);
        }
    }
}
```

### Event Parameter Rules
- **Dynamic Property Access**: Event parameters support `event.propertyName` syntax for any property
- **Runtime Resolution**: Properties are resolved at runtime using reflection-based property access
- **Payload Dictionary**: `event.payload` contains the full dictionary of event data
- **Metadata Access**: Built-in properties like `event.name`, `event.timestamp` are always available
- **Class Instance Access**: In class-based event handlers, use `this.fieldName` to access instance variables for proper scope resolution
### Event Handler Declaration and Emission
```
// ✅ Event handlers - Only `on system` handlers allowed at program scope
on system.ready (event) { ... }      // ✅ CORRECT: Program scope event handler
on system.shutdown (event) { ... }   // ✅ CORRECT: Program scope event handler

class MyAgent 
{
    on user.message (event) { ... }  // ✅ CORRECT: Class scope event handler
    on ai.request (event) { ... }    // ✅ CORRECT: Class scope event handler
    on user.input (event) { ... }    // ✅ CORRECT: Class scope event handler
}

// ✅ CORRECT: Emitting system message event
emit system.shutdown { reason: "maintenance" };

// ✅ CORRECT: Signaling adaptation, handlers optional
adapt { 
  name: "MyAgent", 
  reason: "maintenance", 
  handlers [ 
    event.bus { options: "option" }
  ] 
}; // ✅ CORRECT: Emitting adatation event of a class instance.

// Wildcard event patterns for cross-namespace communication - PRODUCTION READY ✅
// ❌ INCORRECT: Non-system handlers at program scope not allowed
// on name.any.other.any.final (payload) { ... }     // Must be in class scope
// on user.any.response (payload) { ... }             // Must be in class scope
on system.any.ready (payload) { ... }              // ✅ CORRECT: System handlers allowed at program scope
// on agent.any.thinking.any.complete (payload) { ... } // Must be in class scope

// ❌ INCORRECT: Advanced wildcard patterns at program scope
// on user.any.input (payload) { ... }                // Must be in class scope
// on ai.any.response (payload) { ... }               // Must be in class scope
// on voice.any.command (payload) { ... }             // Must be in class scope
// on any.any.critical (payload) { ... }              // Must be in class scope
// on any.any.any.complete (payload) { ... }          // Must be in class scope

// ✅ CORRECT: Class-level wildcards - ALL patterns supported in class scope
class ChatAgent 
{
    on user.any.input (payload) 
    {                  // ✅ CORRECT: Class-scoped wildcard handler
        print("Chat agent received: " + payload.message);
    }
    
    on voice.any.command (payload) 
    {               // ✅ CORRECT: Multi-scope wildcard support in class
        emit user.chat.message, { text: "Voice: " + payload.command };
    }
}
```

// Event emission using reserved event names
emit user.message { text: "hello" };
emit system.shutdown { reason: "maintenance" };

// Wildcard event patterns for cross-namespace communication - PRODUCTION READY ✅
// ❌ INCORRECT: Non-system handlers at program scope not allowed
// on name.any.other.any.final (payload) { ... }     // Must be in class scope
// on user.any.response (payload) { ... }             // Must be in class scope
on system.any.ready (payload) { ... }              // ✅ CORRECT: System handlers allowed at program scope
// on agent.any.thinking.any.complete (payload) { ... } // Must be in class scope

// ❌ INCORRECT: Advanced wildcard patterns at program scope
// on user.any.input (payload) { ... }                // Must be in class scope
// on ai.any.response (payload) { ... }               // Must be in class scope
// on voice.any.command (payload) { ... }             // Must be in class scope
// on any.any.critical (payload) { ... }              // Must be in class scope
// on any.any.any.complete (payload) { ... }          // Must be in class scope

// ✅ CORRECT: Class-level wildcards - ALL patterns supported in class scope
class ChatAgent 
{
    on user.any.input (payload) 
    {                  // ✅ CORRECT: Class-scoped wildcard handler
        print("Chat agent received: " + payload.message);
    }
    
    on voice.any.command (payload) 
    {               // ✅ CORRECT: Multi-scope wildcard support in class
        emit user.chat.message { text: "Voice: " + payload.command };
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
```

### Practical Event System Example
```cx
// Comprehensive example demonstrating event property access and dictionary iteration
class DataAnalysisAgent
{
    name: string = "DataAgent";
    
    on user.input (event)
    {
        print("Agent " + this.name + " processing user input:");
        print("Event: " + event.name);
        print("Timestamp: " + event.timestamp);
        
        // Direct property access on event data
        print("Message: " + event.message);
        print("Priority: " + event.priority);
        
        // Iterate over all event data
        print("Full event data:");
        for (var item in event.payload)
        {
            print("  " + item.Key + ": " + item.Value);

            // Cognitive boolean logic example for each payload item
            is {
              context: "Should the agent highlight this event property?",
              evaluate: "Check if " + item.Key + " is 'priority' and value is 'high'",
              data: { key: item.Key, value: item.Value, agent: this.name },
              handlers: [ property.highlighted { key: item.Key, value: item.Value } ]
            };
        }
        
        // Process based on event data
        is {
            context: "Should the agent start analysis based on event type?",
            evaluate: "Event type is analysis_request",
            data: { type: event.type, agent: this.name, message: event.message, timestamp: event.timestamp },
            handlers: [ analysis.started { agent: this.name, request: event.message, timestamp: event.timestamp } ]
        };
    }
    
    on analysis.started (event)
    {
        print("Analysis starting...");
        
        // Check all analysis parameters
        for (var param in event.payload)
        {
            if (param.Key == "complexity" && param.Value == "high")
            {
                print("High complexity analysis detected");
                emit analysis.complexity.high { agent: this.name };
            }
        }
    }
}

// ✅ CORRECT: Global system handlers with wildcard patterns (only system handlers allowed at global scope)
on system.any.ready (event)
{
    print("Global system handler: System component ready");
    print("Event type: " + typeof(event));
    
    // Log all system events for monitoring
    for (var data in event.payload)
    {
        if (data.Key == "critical" && data.Value == true)
        {
            print("Critical system event detected");
        }
    }
}

// ❌ INCORRECT: Non-system handlers must be in class scope
// on user.any.input (event) { ... }  // Must be in class scope

// Usage example
var dataAgent = new DataAnalysisAgent();

emit user.input, { 
    message: "Analyze sales data", 
    type: "analysis_request",
    priority: "high",
    complexity: "medium",
    sensitive: false
};
```

### Multi-Event Handlers System

The CX Language features a powerful **handlers** system that allows single operations to trigger multiple event listeners automatically, enabling sophisticated event orchestration patterns.

### Class-Based Event Handler Patterns

**Critical Finding**: Class-based event handlers provide superior variable scope resolution compared to global handlers when accessing instance state.

#### Class Instance Variable Access
```cx
// ✅ RECOMMENDED: Class-based event handlers with proper instance variable access
class SmartAwaitAgent
{
    name: string = "SmartAgent";
    currentPhase: string = "initialization";
    speechSpeed: number = 0.9;  // 10% slower than normal for better comprehension
    
    function startPhase(phaseName: string)
    {
        this.currentPhase = phaseName;
        print("Starting phase: " + this.currentPhase);
        
        // Smart await with instance context
        await { 
            reason: "phase_transition_" + this.name,
            context: "Moving to " + this.currentPhase + " phase",
            minDurationMs: 1000,
            maxDurationMs: 3000,
            handlers: [ phase.started ]
        };
    }
    
    // ✅ CORRECT: Class-based event handler accessing instance variables
    on phase.started (event)
    {
        print("Phase started for agent: " + this.name);  // ✅ Instance access works
        print("Current phase: " + this.currentPhase);    // ✅ Instance access works
        
        // Voice synthesis with controlled speech speed
        emit realtime.text.send {
            text: "Phase " + this.currentPhase + " is now active for " + this.name,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: this.speechSpeed  // ✅ Instance variable access
        };
    }
    
    on realtime.audio.response (event)
    {
        if (event.isComplete)
        {
            print("Audio complete for agent: " + this.name);  // ✅ Instance access
            this.currentPhase = "ready";  // ✅ Can modify instance state
        }
    }
}

// ❌ PROBLEMATIC: Global event handlers cannot access instance variables
on phase.started (event)
{
    // ❌ ERROR: Cannot access this.name, this.currentPhase, etc.
    print("Global handler - no instance access");
}
```

#### Class-Based Pattern Benefits
- **Instance Variable Access**: Direct access to `this.fieldName` variables in event handlers
- **State Management**: Can modify instance state (`this.currentPhase = "new_value"`)
- **Contextual Logic**: Event handlers can use instance context for decision making
- **Scope Isolation**: Each class instance has its own event handler scope
- **Better Debugging**: Instance variables provide context for debugging and logging

#### Smart Await Integration Patterns
```cx
// ✅ PRODUCTION PATTERN: Smart await with class-based handlers
class DebateAgent
{
    name: string;
    role: string;
    speechSpeed: number = 0.9;  // Slower speech for clarity
    turnComplete: boolean = false;
    
    constructor(agentName: string, agentRole: string)
    {
        this.name = agentName;
        this.role = agentRole;
    }
    
    function takeTurn(topic: string)
    {
        this.turnComplete = false;
        
        // AI-determined optimal timing before speaking
        await { 
            reason: "pre_speech_pause_" + this.name,
            context: "Preparing " + this.role + " response on " + topic,
            minDurationMs: 1000,
            maxDurationMs: 2000,
            handlers: [ turn.ready ]
        };
    }
    
    on turn.ready (event)
    {
        print("Agent " + this.name + " (" + this.role + ") taking turn");
        
        // Generate response with context
        think {
            prompt: "As a " + this.role + ", respond to: " + event.context,
            handlers: [ response.generated ]
        };
    }
    
    on response.generated (event)
    {
        // Voice synthesis with instance-specific speed
        emit realtime.text.send {
            text: event.result,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: this.speechSpeed
        };
    }
    
    on realtime.audio.response (event)
    {
        if (event.isComplete)
        {
            this.turnComplete = true;
            
            // Smart await for natural pause after speaking
            await { 
                reason: "post_turn_pause_" + this.name,
                context: "Natural pause after " + this.name + " completes turn",
                minDurationMs: 1500,
                maxDurationMs: 2500,
                handlers: [ turn.complete ]
            };
        }
    }
    
    on turn.complete (event)
    {
        print("Turn complete for: " + this.name);
        emit debate.turn.finished { agent: this.name, role: this.role };
    }
}
```

#### Voice Speed Control Patterns
```cx
// ✅ Voice speed control for improved comprehension
class VoiceControlAgent
{
    speechSpeed: number = 0.9;  // 10% slower than normal (1.0)
    
    function speakSlowly(message: string)
    {
        emit realtime.text.send {
            text: message,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: this.speechSpeed  // Slows speech by 10%
        };
    }
    
    function speakNormally(message: string)
    {
        emit realtime.text.send {
            text: message,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: 1.0  // Normal speed
        };
    }
    
    function speakQuickly(message: string)
    {
        emit realtime.text.send {
            text: message,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: 1.2  // 20% faster than normal
        };
    }
}
```

#### Class-Based Event Handler Rules
- **Prefer Class Handlers**: Use class-based event handlers when accessing instance variables
- **Instance Context**: Always use `this.fieldName` for instance variable access
- **State Modification**: Event handlers can modify instance state directly
- **Scope Isolation**: Each class instance maintains separate event handler scope
- **Global Fallback**: Use global handlers only for system-wide coordination
- **Variable Scope**: Class handlers resolve variable scope more reliably than global handlers
- **Debugging Context**: Instance variables provide better debugging information

### Multi-Event Handlers System

The CX Language features a powerful **handlers** system that allows single operations to trigger multiple event listeners automatically, enabling sophisticated event orchestration patterns.

#### Handlers Syntax
```cx
// ✅ Enhanced handlers with custom payloads - NEW SYNTAX
learn {
    data: "Customer feedback dataset",
    category: "analysis",
    priority: "high",
    handlers: [ 
        analysis.complete { option: "detailed" }, 
        task.finished { status: "completed" }, 
        notify.users { urgency: "high" }
    ]
};

// ✅ Mixed handlers - some with custom payloads, some without
emit user.input {
    message: "Hello world",
    type: "greeting",
    handlers: [ 
        response.ready,
        chat.sent { channel: "main" },
        log.entry { level: "info" }
    ]
};

// ✅ Simple handlers without custom payloads
search {
    query: "AI agents",
    handlers: [ results.found, cache.updated, metrics.logged ]
};
```

#### Enhanced Handlers Behavior
- **Custom Payload Support**: Each handler can receive unique custom data alongside original payload
- **Comma-less Syntax**: Modern, clean syntax without commas for AI services and emit statements
- **Multi-Event Emission**: Each handler triggers a separate event emission with full payload + custom data
- **Payload Propagation**: Original payload data PLUS custom handler payload passed to ALL handler events
- **Mixed Handler Support**: Can combine handlers with and without custom payloads in same array
- **Property Access**: Handler events receive both original payload (`event.data`) and custom payload (`event.option`)
- **Fire-and-Forget**: All handler events are non-blocking and execute asynchronously

#### Complete Enhanced Handlers Example
```cx
// Production-ready enhanced handlers demonstration
class AnalysisAgent
{
    name: string = "AnalysisAgent";
    
    function analyzeData(inputData: string): void
    {
        print("Starting analysis of: " + inputData);
        
        // Enhanced AI service with custom payload handlers
        learn {
            data: inputData,
            category: "analysis", 
            priority: "high",
            handlers: [ 
                analysis.complete { option: "detailed", format: "json" },
                task.finished { status: "completed", timestamp: "2025-07-22" },
                notify.users { urgency: "high", channel: "alerts" }
            ]
        };
        
        print("Analysis initiated with enhanced handlers");
    }
}

// ✅ CORRECT: Handler events in class scope - receive BOTH original payload AND custom handler payload
class HandlerDemoClass
{
    on analysis.complete (event)
    {
        print("=== ANALYSIS COMPLETE ===");
        print("Analysis finished for data: " + event.data);      // ✅ Original payload
        print("Category: " + event.category);                    // ✅ Original payload
        print("Priority: " + event.priority);                    // ✅ Original payload
        print("Report option: " + event.option);                 // ✅ Custom handler payload
        print("Report format: " + event.format);                 // ✅ Custom handler payload
    }
    
    on task.finished (event)
    {
        print("=== TASK TRACKING ===");
        print("Task completed: " + event.data);                  // ✅ Original payload
        print("Task status: " + event.status);                   // ✅ Custom handler payload
        print("Completion timestamp: " + event.timestamp);       // ✅ Custom handler payload
    }
    
    on notify.users (event)
    {
        print("=== USER NOTIFICATION ===");
        print("Notifying users about: " + event.data);           // ✅ Original payload
        print("Priority level: " + event.priority);              // ✅ Original payload
        print("Notification urgency: " + event.urgency);         // ✅ Custom handler payload
        print("Notification channel: " + event.channel);         // ✅ Custom handler payload
    }
    
    on ai.learn.request (event)
    {
        print("=== AI LEARNING ===");
        print("AI system learning from: " + event.data);         // ✅ Same data as above
        print("Category: " + event.category);                    // ✅ "analysis"
    }
}

// Usage
var agent = new AnalysisAgent();
var handlerDemo = new HandlerDemoClass();
agent.analyzeData("Customer feedback dataset");
```

#### Handlers Rules
- **Square Brackets Only**: Use `[ ]` syntax, NOT curly braces `{ }`
- **Comma Separation**: Separate multiple handlers with commas
- **Property Propagation**: All original properties are available in handler events
- **Event Scope**: Handlers work at both program scope and class scope
- **Mixed Names**: Can mix simple names (`complete`) and dotted names (`analysis.complete`) in same array
- **Performance**: Handlers are efficiently compiled and execute with minimal overhead

### **Cognitive Functions**
```cx
// All cognitive service methods use comma-less structured parameters with enhanced handlers pattern:

// ✅ COGNITIVE BOOLEAN LOGIC: AI-driven decision making
is { 
    context: "Decision context description",
    evaluate: "What is being evaluated",
    data: { condition: true },
    handlers: [ decision.ready ]
};

// ✅ NEGATIVE COGNITIVE BOOLEAN LOGIC: AI-driven false/negative decision making
not { 
    context: "Decision context description",
    evaluate: "What is being evaluated for false condition",
    data: { condition: false },
    handlers: [ decision.negative ]
};

// ✅ COGNITIVE LEARNING: Knowledge acquisition and adaptation
learn { 
    data: "content to learn",
    handlers: [ 
        analysis.complete { option: "detailed" },
        storage.saved { format: "json" }
    ]
};

// ✅ COGNITIVE REASONING: Deep thinking and analysis
think { 
    prompt: "reasoning prompt",
    handlers: [ 
        thinking.complete { option: "detailed" },
        analysis.logged { level: "info" }
    ]
};

// ✅ SMART AWAIT SERVICE: AI-determined optimal timing for natural interactions
await { 
    reason: "post_turn_pause_" + this.name,
    context: "Natural pause after " + this.name + " completes turn",
    minDurationMs: 1000,
    maxDurationMs: 3000,
    handlers: [ turn.complete ]
};

// ✅ COGNITIVE ADAPTATION: Behavioral evolution and learning
adapt { 
    context: "adaptation context",
    handlers: [ 
        adaptation.complete { option: "detailed" },
        adapt.logged { level: "info" }
    ]
};

// Class introspection for self-awareness
learn { self: this };
```

## Asynchronous Programming
```cx
// ❌ INCORRECT - Custom events at program scope not allowed
// on custom.event (payload) { ... }        // Must be in class scope
// on myapp.data.updated (payload) { ... }  // Must be in class scope

// ❌ INCORRECT - Wildcard event patterns at program scope
// on name.any.other.any.final (payload) { ... }     // Must be in class scope
// on user.any.response (payload) { ... }             // Must be in class scope
on system.any.ready (payload) { ... }              // ✅ CORRECT: System handlers allowed at program scope
// on agent.any.thinking.any.complete (payload) { ... } // Must be in class scope

// ✅ CORRECT: Custom events allowed in class scope
class MyApp 
{
    on custom.event (payload) { ... }        // ✅ CORRECT: Custom events in class scope
    on myapp.data.updated (payload) { ... }  // ✅ CORRECT: Application-specific events in class scope
}
```

### **Event-Driven Async Pattern**

Cx uses a pure fire-and-forget model for all asynchronous operations, including cognitive service calls. This eliminates `async/await` and complex callback chains, promoting a clean, event-driven architecture.

-   **No `await` Keyword**: The `await` keyword is completely eliminated from the language.
-   **No Return Values from Async Ops**: Asynchronous functions and service calls return `void`.
-   **Event-Based Results**: All results and continuations are handled through the event bus system.

```cx
// ✅ CORRECT: Pure fire-and-forget with event bus coordination
class AsyncProcessor 
{
    function processData(input) 
    {
        // All async operations are fire-and-forget. No return values, no blocking.
        think { prompt: { input }, handlers: [thinking.complete] };
        learn { content:{ input }, handlers: [learning.complete] };
        
        // Immediately emit an event to signal the start of processing.
        emit processing.started { input: input };
    }
    
    // Results flow through the event system.
    on thinking.complete (event) 
    {
        print("Thinking complete: " + event.result);
    }
    
    on learning.complete (event) 
    {
        print("Learning complete for document: " + event.documentId);
    }
}
```

## Azure Realtime API Integration

### **Voice Processing Events**
CX Language integrates with Azure OpenAI Realtime API through specific event patterns for real-time voice and text processing.

#### **Core Realtime Events**
```cx
// ✅ PRODUCTION READY: Azure Realtime API event patterns
// ✅ AZURE REALTIME API: For production voice processing, use Azure Realtime events:
// emit realtime.connect { demo: "app" };
// emit realtime.session.create { deployment: "gpt-4o-mini-realtime-preview" };
// emit realtime.text.send { text: "message", deployment: "gpt-4o-mini-realtime-preview" };
// Handle responses with: on realtime.text.response, on realtime.audio.response

// Connect to Azure Realtime API
emit realtime.connect { demo: "app_name" };

// Create voice session 
emit realtime.session.create { 
    deployment: "gpt-4o-mini-realtime-preview",
    mode: "voice" 
};

// Send text for voice synthesis
emit realtime.text.send { 
    text: "Hello world",
    deployment: "gpt-4o-mini-realtime-preview"
};

// Send audio for processing
emit realtime.audio.send { 
    audio: audioData,
    deployment: "gpt-4o-mini-realtime-preview"
};
```

#### **Event Handlers for Azure Responses**
```cx
// ✅ CORRECT: Event handlers in class scope
class AzureRealtimeHandler
{
    // Handle connection confirmation
    on realtime.connected (event)
    {
        print("✅ Connected to Azure Realtime API");
        emit realtime.session.create { deployment: "gpt-4o-mini-realtime-preview" };
    }
    
    // Handle session creation
    on realtime.session.created (event)
    {
        print("✅ Voice session ready");
        emit realtime.text.send { text: "Hello!" };
    }
    
    // Handle streaming text responses
    on realtime.text.response (event)
    {
        print("Text chunk: " + event.content);
        print("Complete: " + event.isComplete);
    }
    
    // Handle audio responses
    on realtime.audio.response (event)
    {
        if (event.audioData && event.audioData.length)
        {
            print("Audio received: " + event.audioData.length + " bytes");
        }
        print("Complete: " + event.isComplete);
    }
}
```

#### **Complete Working Example**
```cx
// Proven working voice integration
class VoiceDemo
{
    function startDemo()
    {
        emit realtime.connect { demo: "voice_demo" };
    }
    
    on realtime.connected (event)
    {
        emit realtime.session.create { 
            deployment: "gpt-4o-mini-realtime-preview"
        };
    }
    
    on realtime.session.created (event)
    {
        emit realtime.text.send { 
            text: "Say hello to the user",
            deployment: "gpt-4o-mini-realtime-preview"
        };
    }
    
    on realtime.text.response (event)
    {
        if (event.isComplete)
        {
            print("✅ Complete AI response: " + event.content);
        }
    }
    
    on realtime.audio.response (event)
    {
        // ✅ FIXED: Safe audio data handling without byte array casting issues
        if (event.audioData != null)
        {
            print("🔊 Voice audio data received");
            print("📊 Data type: " + typeof(event.audioData));
        }
        else
        {
            print("🔊 Audio response - no data");
        }
        
        if (event.isComplete)
        {
            print("🎵 Voice synthesis complete!");
        }
    }
}

var demo = new VoiceDemo();
demo.startDemo();
```

### **Azure Realtime API Rules**
- **Event-Based Only**: Use `emit` statements, NOT direct service calls for voice
- **Streaming Responses**: Responses arrive in chunks with `isComplete` flag
- **Audio Data**: Audio responses contain `audioData` property with byte array
- **Session Management**: Always connect → create session → send data
- **Deployment Required**: Include `deployment: "gpt-4o-mini-realtime-preview"` for voice
- **Real-Time Processing**: Responses stream in real-time, handle incrementally
- **Speech Speed Control**: Use `speechSpeed` parameter to control voice timing (0.9 = 10% slower, 1.0 = normal, 1.2 = 20% faster)

