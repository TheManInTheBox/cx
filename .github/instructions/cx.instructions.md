# CX Language Syntax Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Core Language Rules](#core-language-rules)
3. [Syntax Basics](#syntax-basics)
4. [Cognitive Boolean Logic](#cognitive-boolean-logic)
5. [Object and Member Declaration](#object-and-member-declaration)
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
- **Aura Cognitive Framework**: A decentralized eventing model where each agent possesses a local `EventHub` (a personal nervous system) for internal processing, all orchestrated by a global `EventBus` that manages inter-agent communication.
- **Voice Processing**: Azure OpenAI Realtime API integration via event system (`emit realtime.connect`, `realtime.text.send`, `realtime.audio.response`)
- **Automatic Object Serialization**: CX objects print as readable JSON with recursive nesting support
- **Dictionary Iteration**: Native support for iterating over dictionaries in for-in loops
- **Dynamic Property Access**: Runtime property resolution for flexible event handling
- **KeyValuePair Support**: Automatic handling of dictionary entries with `.Key` and `.Value` access
- **Fire-and-Forget AI Operations**: Non-blocking cognitive methods with event-based results
- **Serializable Object Parameters**: Pass complex data structures to services using object literals
- **Comma-less Syntax**: Modern clean syntax for AI services and emit statements
- **Biological Neural Authenticity**: Revolutionary synaptic plasticity with LTP (5-15ms), LTD (10-25ms), STDP causality rules

## Core Language Rules
The following rules are mandatory for all CX Language code:

### Syntax Requirements
- Always use Allman-style brackets `{ }` for code blocks
- Use `print()` for console output - NEVER use `console.log()`
- **NO `if` statements**: Use cognitive `is { }` and `not { }` patterns for all decision logic
- **NO `function` declarations**: Use only cognitive functions (`learn`, `think`, `is`, `not`, `await`, `adapt`)
- `print()` automatically serializes complex objects to JSON for debugging
- `print()` displays primitive types (strings, numbers, booleans) directly
- `print()` provides nested object visualization for CX classes
- Use `:` for type annotations in realize constructor parameters
- Use `=` for default values in variable declarations

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
- Use `var` keyword for local variables inside event handlers and realize constructors
- Use `var` keyword for global variables at program scope
- **No Member Fields**: CX Language eliminates instance variables for pure stateless architecture
- **Event-Driven State**: All state management through AI services and event payloads
- **Type Annotations**: Use `parameterName: type` for realize constructor parameters

### Object Structure
- Use `object` keyword for object declarations - NO `public` or `private` modifiers
- Use event handlers (`on eventName`) for all behavioral logic inside objects
- **Pure Event-Driven**: Objects contain only `realize()` constructors and event handlers - NO member fields or methods
- **NO `this` keyword**: CX Language eliminates instance references for pure stateless programming
- **NO `function` declarations**: Traditional functions eliminated - use only cognitive functions for all behavior
- **No Member Fields**: All state is managed through AI services and event payloads - no instance variables
- **No Public/Private Modifiers**: CX does not use access modifiers - pure event-driven architecture
- **No Static Members**: CX does not support static members - all behavior is event-based
- **Realize Constructor**: Use `realize(self: object)` for cognitive object initialization
- **Event handler parameters**: Type annotations recommended for clarity

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
- **No Class-level Injection**: Classes cannot have services injected into them or declare their own service instances.
- **Invocation**: Services are called using their name followed by a comma-less block of parameters, e.g., `think { prompt: "Hello" }`.
- **Serializable Object Parameters**: For complex inputs, pass a serializable object instead of concatenating strings, e.g., `think { prompt: { text: "Analyze this", context: "Full context here" } }`.
- **Core Cognitive Services**: `is` (cognitive boolean logic), `not` (negative boolean logic), `learn` (knowledge acquisition), `think` (reasoning), `await` (smart timing), `adapt` (behavioral evolution)
- **Pure Event-Driven State**: All state management occurs through AI services and event payloads, eliminating the need for instance variables

## Syntax Basics
- **Use constructors to inject data** - DO NOT ACCESS MEMBERS DIRECTLY FOR INITIALIZATION.
- **`objects`**: Use `object` keyword for object declarations - NO `public` or `private` modifiers
- **`objects` behavior**: Objects are blueprints for pure event-driven entities that encapsulate behavior through event handlers. They do not have instance state.
- **No Public/Private Modifiers**: Cx does not use access modifiers - all members are accessible within object scope
- **No Static Members**: CX does not support static members - all members are instance-based
- **Variable Declaration**: Use `var identifier` for new loop variables
- **Existing Variables**: Can use existing variables without `var` keyword
- **Iteration Target**: Works with arrays, collections, and any iterable object
- **Loop Variable**: Automatically assigned each element during iteration
- **Block Syntax**: Must use Allman-style brackets `{ }` 
- **Scope**: Loop variables follow standard CX scoping rules
- **Type Safety**: Loop variables are dynamically typed, use `typeof()` for type checking
- **Service access**: Cognitive services are globally available. They are not methods and are not accessed with `this.`
- **Service injection**: ILLEGAL - classes cannot declare their own service instances
- **Event handlers**: Available at both program scope AND in classes
- **Object scope**: Event handlers and realize constructors only - NO fields, methods, or service declarations
- **Constructor parameters**: Type annotations recommended for clarity
- **Event handler parameters**: Type annotations recommended for clarity
- **Local variables**: Use `var` keyword for variables inside constructors and event handlers
- **Global variables**: Use `var` keyword for variables declared at program scope
- **Event-driven behavior**: All object behavior is implemented through event handlers, not methods
- **Event communication**: Use `emit` statements to trigger behavior across objects
- **Pure Fire-and-Forget**: All cognitive operations non-blocking by default
- **Event handlers**: Cannot return values, execute asynchronously
- **Event coordination**: Results delivered through event bus system
- **Immediate responses**: Event handlers complete instantly, async work continues in background
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

// Global event handler
on system.start (event)
{
    var localVariable = "processing";
    print("Status: " + localVariable);
    emit system.ready;
}
```

### Complete Agent Example

```cx
// Complete agent implementation with pure event-driven architecture

object AssistantAgent
{
    realize(self: object)
    {
        print("Agent initialized: " + self.name);
        learn self;
        emit agent.initialized { name: self.name };
    }
    
    on user.message (event)
    {
        print("Processing message: " + event.text);
        
        // Enhanced cognitive methods with custom payload handlers
        var promptObject = {
            message: event.text,
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
        
        emit agent.processing { message: event.text };
    }
    
    on thinking.complete (event)
    {
        print("Analysis complete with option: " + event.option);
        print("Agent state updated through events");
        
        emit agent.response { 
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

// Create and use the pure event-driven agent
var assistant = new AssistantAgent({ name: "Helpful Assistant" });

// Send a message to trigger enhanced handlers
emit user.message { text: "Can you help me with my project?", user: "Alice", priority: "high" };
```

### Multi-Agent System Example

```cx
// System with multiple coordinating agents - pure event-driven
object CoordinatorAgent
{
    realize(self: object)
    {
        learn self;
        emit coordinator.ready { name: self.name };
    }
    
    on agent.response (payload)
    {
        print("Coordinator received response");
        emit system.log { message: "Response processed by coordinator" };
    }
}

object SpecialistAgent
{
    realize(self: object)
    {
        learn self;
        emit specialist.ready { domain: self.domain };
    }
    
    on user.query (payload)
    {
        // Check domain compatibility through event data
        print("Specialist processing query in domain: " + payload.domain);
        think { 
            handlers: [specialist.result], 
            prompt: payload.query 
        };
    }
}

// Create the agent system
var coordinator = new CoordinatorAgent({ name: "MainCoordinator" });
var techSpecialist = new SpecialistAgent({ domain: "technology" });
var financeSpecialist = new SpecialistAgent({ domain: "finance" });

// Use the system
emit user.query { domain: "technology", query: "How do I optimize my code?" };
```

### Voice Agent Example (Azure Realtime API)

```cx
// PRODUCTION-READY: Voice agent with Azure OpenAI Realtime API integration
// Uses actual Azure WebSocket endpoints for real-time voice processing
object VoiceAgent
{
    realize(self: object)
    {
        learn self;
        emit voice.agent.ready { name: self.name };
    }
    
    on voice.session.start (event)
    {
        print("� Starting voice session...");
        
        // ✅ PROVEN WORKING: Connect to Azure Realtime API first
        emit realtime.connect { demo: "voice_agent" };
    }
    
    on voice.message.send (event)
    {
        print("🔊 Sending voice message: " + event.text);
        
        // ✅ PROVEN WORKING: Send text to Azure for voice synthesis
        emit realtime.text.send { 
            text: event.text,
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
var voiceAgent = new VoiceAgent({ name: "VoiceAgent" });
emit voice.session.start;

// Example response: "Hello! How can I assist you today?" with 88,800 bytes of audio
```

### **Object and Member Declaration Syntax**
```
// ✅ Object declaration with optional inheritance
object BasicObject 
{
    // Object members - realize constructors and event handlers only
}

// ✅ Object with inheritance
object CognitiveAgent : BaseAgent
{
    // ✅ Realize constructor with parameters
    realize(self: object)
    {
        // Pure cognitive initialization without instance fields
        learn self;
        emit agent.ready { name: self.name };
    }
    
    // ✅ Event handlers within object scope
    on user.message (payload) 
    {
        print("Processing: " + payload.text);
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

// ✅ For...in loop for dictionary iteration (event payloads) - in object scope
object EventProcessor
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

// ✅ For...in loop in event handlers
object DataProcessor 
{
    on data.process.array (event)
    {
        for (var item in event.data) 
        {
            print("Processing: " + item);
            
            // Local variable inside loop
            var processed = "item_" + item;
            print("Processed: " + processed);
        }
    }
    
    on data.process.dictionary (event)
    {
        for (var kvp in event.data) 
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

CX Language features intelligent decision-making through cognitive boolean logic using the `is { }` syntax. This **completely replaces** traditional `if (condition)` statements with AI-driven contextual evaluation.

### Cognitive Boolean Syntax
```cx
// ❌ Traditional boolean logic (COMPLETELY ELIMINATED from CX)
if (event.reason.indexOf(this.name) >= 0)
{
    doSomething();
}

// ✅ Cognitive boolean logic - AI-driven decision making
is { 
    context: "Cognitive decision: Should agent proceed?",
    evaluate: event.reason + " contains agent name " + agentName,
    data: { eventReason: event.reason, agentName: agentName },
    handlers: [ decision.ready ]
};  // ✅ Semicolon ends the decision - triggers events only

// ✅ Negative cognitive boolean logic - AI-driven false/negative decision making
not { 
    context: "Cognitive decision: Should agent NOT proceed?",
    evaluate: event.reason + " excludes agent name " + agentName,
    data: { eventReason: event.reason, agentName: agentName },
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
object SmartAgent
{
    realize(self: object)
    {
        learn self;
        emit agent.ready { name: self.name };
    }
    
    on user.message (event)
    {
        // ✅ Cognitive decision about message relevance
        is { 
            context: "Should this agent respond to the user message?",
            evaluate: "Message relevance check for agent",
            data: { message: event.text, priority: event.priority },
            handlers: [ response.decision.made, message.evaluated { urgency: "high" } ]
        };  // ✅ Event-only execution - no code block
    }
    
    // ✅ Event handler for processing the decision result
    on response.decision.made (event)
    {
        print("Processing relevant message");
        print("Message: " + event.message);
        print("Priority: " + event.priority);
    }
    
    on audio.response (event)
    {
        // ✅ Cognitive decision about audio data processing
        is { 
            context: "Is audio data available for processing?",
            evaluate: "Audio data presence check",
            data: { audioData: event.audioData },
            handlers: [ audio.data.evaluated ]
        };  // ✅ Event-only execution
        
        // ✅ Cognitive decision about completion status
        is { 
            context: "Is the audio processing complete?",
            evaluate: "Completion status evaluation",
            data: { isComplete: event.isComplete },
            handlers: [ completion.evaluated ]
        };  // ✅ Event-only execution
    }
    
    // ✅ Event handlers for processing cognitive decisions
    on audio.data.evaluated (event)
    {
        print("Processing audio data");
        emit audio.process { data: event.audioData };
    }
    
    on completion.evaluated (event)
    {
        print("Audio processing complete");
        emit task.finished { task: "audio_processing" };
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
- **Complete Elimination**: Traditional `if` statements are completely removed from CX Language

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
        context: "Cognitive decision: Should agent proceed to speech phase?",
        evaluate: event.reason + " contains agent readiness",
        data: { eventReason: event.reason, timing: event.actualDurationMs },
        handlers: [ agent.decision.ready ]  // ✅ Called ONLY if evaluation is true
    };  // ✅ Note: semicolon ends the cognitive decision - no code block
}
```

## Event System

### **Event Parameter Property Access**
```
// ✅ CORRECT: Event handlers in object scope
object EventAgent
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
object DataProcessor
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
object ComplexEventHandler
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
- **Pure Event-Driven State**: All data access through event parameters, no instance variables
### Event Handler Declaration and Emission
```
// ✅ Event handlers - Only `on system` handlers allowed at program scope
on system.ready (event) { ... }      // ✅ CORRECT: Program scope event handler
on system.shutdown (event) { ... }   // ✅ CORRECT: Program scope event handler

object MyAgent 
{
    on user.message (event) { ... }  // ✅ CORRECT: Object scope event handler
    on ai.request (event) { ... }    // ✅ CORRECT: Object scope event handler
    on user.input (event) { ... }    // ✅ CORRECT: Object scope event handler
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
}; // ✅ CORRECT: Emitting adaptation event of an object instance.

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
// on name.any.other.any.final (payload) { ... }     // Must be in object scope
// on user.any.response (payload) { ... }             // Must be in object scope
on system.any.ready (payload) { ... }              // ✅ CORRECT: System handlers allowed at program scope
// on agent.any.thinking.any.complete (payload) { ... } // Must be in object scope

// ❌ INCORRECT: Advanced wildcard patterns at program scope
// on user.any.input (payload) { ... }                // Must be in object scope
// on ai.any.response (payload) { ... }               // Must be in object scope
// on voice.any.command (payload) { ... }             // Must be in object scope
// on any.any.critical (payload) { ... }              // Must be in object scope
// on any.any.any.complete (payload) { ... }          // Must be in object scope

// ✅ CORRECT: Object-level wildcards - ALL patterns supported in object scope
object ChatAgent 
{
    on user.any.input (payload) 
    {                  // ✅ CORRECT: Object-scoped wildcard handler
        print("Chat agent received: " + payload.message);
    }
    
    on voice.any.command (payload) 
    {               // ✅ CORRECT: Multi-scope wildcard support in object
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
    {  // Object-scoped AI wildcard
        print("MyAgent processing AI response: " + payload.response);
    }
    
    on command.execute (event)
    {
        emit command.executed { data: event.command };
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

### Object-Based Event Handler Patterns

**Core Architecture**: CX Language uses pure event-driven patterns without instance state, eliminating traditional object-oriented concepts.

#### Pure Event-Driven Variable Access
```cx
// ✅ RECOMMENDED: Object-based event handlers with pure event-driven architecture
object SmartAwaitAgent
{
    realize(self: object)
    {
        learn self;
        emit agent.ready { name: self.name };
    }
    
    on phase.start (event)
    {
        print("Starting phase: " + event.phaseName);
        
        // Smart await with event context
        await { 
            reason: "phase_transition_" + event.agentName,
            context: "Moving to " + event.phaseName + " phase",
            minDurationMs: 1000,
            maxDurationMs: 3000,
            handlers: [ phase.started ]
        };
    }
    
    // ✅ CORRECT: Object-based event handler accessing event parameters
    on phase.started (event)
    {
        print("Phase started for agent: " + event.agentName);
        print("Current phase: " + event.phaseName);
        
        // Voice synthesis with controlled speech speed
        emit realtime.text.send {
            text: "Phase " + event.phaseName + " is now active for " + event.agentName,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: 0.9  // Controlled speech speed
        };
    }
    
    on realtime.audio.response (event)
    {
        if (event.isComplete)
        {
            print("Audio complete for phase transition");
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

#### Object-Based Pattern Benefits
- **Event Parameter Access**: Direct access to `event.propertyName` variables in event handlers
- **State Management**: Event handlers can modify state through AI services and event payloads
- **Contextual Logic**: Event handlers can use event context for decision making
- **Scope Isolation**: Each object instance has its own event handler scope
- **Better Debugging**: Event parameters provide context for debugging and logging

#### Smart Await Integration Patterns
```cx
// ✅ PRODUCTION PATTERN: Smart await with object-based handlers
object DebateAgent
{
    realize(self: object)
    {
        learn self;
        emit agent.ready { name: self.name, role: self.role };
    }
    
    on debate.turn.start (event)
    {
        // AI-determined optimal timing before speaking
        await { 
            reason: "pre_speech_pause_" + event.agentName,
            context: "Preparing " + event.role + " response on " + event.topic,
            minDurationMs: 1000,
            maxDurationMs: 2000,
            handlers: [ turn.ready ]
        };
    }
    
    on turn.ready (event)
    {
        print("Agent " + event.agentName + " (" + event.role + ") taking turn");
        
        // Generate response with context
        think {
            prompt: "As a " + event.role + ", respond to: " + event.context,
            handlers: [ response.generated ]
        };
    }
    
    on response.generated (event)
    {
        // Voice synthesis with instance-specific speed
        emit realtime.text.send {
            text: event.result,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: 0.9
        };
    }
    
    on realtime.audio.response (event)
    {
        if (event.isComplete)
        {
            // Smart await for natural pause after speaking
            await { 
                reason: "post_turn_pause_" + event.agentName,
                context: "Natural pause after " + event.agentName + " completes turn",
                minDurationMs: 1500,
                maxDurationMs: 2500,
                handlers: [ turn.complete ]
            };
        }
    }
    
    on turn.complete (event)
    {
        print("Turn complete for: " + event.agentName);
        emit debate.turn.finished { agent: event.agentName, role: event.role };
    }
}
```

#### Voice Speed Control Patterns
```cx
// ✅ Voice speed control for improved comprehension
object VoiceControlAgent
{
    realize(self: object)
    {
        learn self;
        emit agent.ready { name: self.name, speechSpeed: self.speechSpeed };
    }
    
    on voice.speak.slow (event)
    {
        emit realtime.text.send {
            text: event.message,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: 0.9  // Slows speech by 10%
        };
    }
    
    on voice.speak.normal (event)
    {
        emit realtime.text.send {
            text: event.message,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: 1.0  // Normal speed
        };
    }
    
    on voice.speak.fast (event)
    {
        emit realtime.text.send {
            text: event.message,
            deployment: "gpt-4o-mini-realtime-preview",
            speechSpeed: 1.2  // 20% faster than normal
        };
    }
}
```

#### Object-Based Event Handler Rules
- **Prefer Object Handlers**: Use object-based event handlers when accessing event parameters
- **Event Context**: Always use `event.propertyName` for event parameter access
- **State Management**: Event handlers can modify state through AI services and event payloads
- **Scope Isolation**: Each object instance maintains separate event handler scope
- **Global Fallback**: Use global handlers only for system-wide coordination
- **Variable Scope**: Object handlers resolve variable scope more reliably than global handlers
- **Debugging Context**: Event parameters provide better debugging information

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
object AnalysisAgent
{
    realize(self: object)
    {
        learn self;
        emit agent.ready { name: self.name };
    }
    
    on data.analyze (event)
    {
        print("Starting analysis of: " + event.inputData);
        
        // Enhanced AI service with custom payload handlers
        learn {
            data: event.inputData,
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

// ✅ CORRECT: Handler events in object scope - receive BOTH original payload AND custom handler payload
object HandlerDemoObject
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
var agent = new AnalysisAgent({ name: "AnalysisAgent" });
var handlerDemo = new HandlerDemoObject({ name: "HandlerDemo" });
emit data.analyze { inputData: "Customer feedback dataset" };
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
object AsyncProcessor 
{
    realize(self: object)
    {
        learn self;
        emit processor.ready { name: self.name };
    }
    
    on data.process (event) 
    {
        // All async operations are fire-and-forget. No return values, no blocking.
        think { prompt: { input: event.input }, handlers: [thinking.complete] };
        learn { content: { input: event.input }, handlers: [learning.complete] };
        
        // Immediately emit an event to signal the start of processing.
        emit processing.started { input: event.input };
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
object VoiceDemo
{
    realize(self: object)
    {
        learn self;
        emit demo.ready { name: self.name };
    }
    
    on demo.start (event)
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
emit demo.start;
```

### **Azure Realtime API Rules**
- **Event-Based Only**: Use `emit` statements, NOT direct service calls for voice
- **Streaming Responses**: Responses arrive in chunks with `isComplete` flag
- **Audio Data**: Audio responses contain `audioData` property with byte array
- **Session Management**: Always connect → create session → send data
- **Deployment Required**: Include `deployment: "gpt-4o-mini-realtime-preview"` for voice
- **Real-Time Processing**: Responses stream in real-time, handle incrementally
- **Speech Speed Control**: Use `speechSpeed` parameter to control voice timing (0.9 = 10% slower, 1.0 = normal, 1.2 = 20% faster)
- **Production Proven**: All patterns tested and verified in working applications

