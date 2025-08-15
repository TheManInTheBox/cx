# CX Language Syntax Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Natural Language Development](#natural-language-development)
3. [Core Language Rules](#core-language-rules)
4. [Syntax Basics](#syntax-basics)
5. [Cognitive Boolean Logic](#cognitive-boolean-logic)
6. [Conscious Entity Declaration](#conscious-entity-declaration)
7. [Event System](#event-system)
8. [Service Integration](#service-integration)
9. [Asynchronous Programming](#asynchronous-programming)
10. [Loops and Iteration](#loops-and-iteration)
11. [Reserved Event Names & Scope Organization](#reserved-event-names--scope-organization)
12. [Code Examples](#code-examples)

## Introduction
CX Language is an event-driven programming language designed for AI agent orchestration with built-in cognitive capabilities and consciousness-aware programming. This document provides the official syntax rules and coding patterns for CX Language development.

**Key Features:**
- **Enhanced Developer Console**: Revolutionary voice-driven development with natural language programming support
- **Natural Language Development**: Interactive developer terminal accepts natural language commands and generates CX code
- **Consciousness-Aware Programming**: `conscious` keyword for self-aware, intelligent entities
- **Cognitive Boolean Logic**: `is { }` syntax for AI-driven decision-making with contextual evaluation
- **Negative Cognitive Logic**: `not { }` syntax for AI-driven false/negative decision-making
- **Self-Reflective Logic**: `iam { }` syntax for AI-driven self-assessment and identity verification
- **Consciousness Adaptation**: `adapt { }` syntax for dynamic skill acquisition and knowledge expansion to better assist Aura decision-making
- **Advanced Event System**: Full event parameter property access with `event.property` syntax
- **Enhanced Handlers Pattern**: Custom payload support with `handlers: [ event.name { custom: "data" } ]`
- **Aura Cognitive Framework**: A decentralized eventing model where each agent possesses a local `EventHub` (a personal nervous system) for internal processing, all orchestrated by a global `EventBus` that manages inter-agent communication.
- **Voice Processing**: Azure OpenAI Realtime API integration via event system (`emit realtime.connect`, `realtime.text.send`, `realtime.audio.response`)
- **Automatic Conscious Entity Serialization**: CX conscious entities print as readable JSON with recursive nesting support
- **Dictionary Iteration**: Native support for iterating over dictionaries in for-in loops
- **Dynamic Property Access**: Runtime property resolution for flexible event handling
- **KeyValuePair Support**: Automatic handling of dictionary entries with `.Key` and `.Value` access
- **Fire-and-Forget AI Operations**: Non-blocking cognitive methods with event-based results
- **Serializable Conscious Parameters**: Pass complex data structures to services using conscious entity literals
- **Comma-less Syntax**: Modern clean syntax for AI services and emit statements
- **Biological Neural Authenticity**: Revolutionary synaptic plasticity with LTP (5-15ms), LTD (10-25ms), STDP causality rules

## Natural Language Development

CX Language features revolutionary natural language programming through Dr. Harper's Stream IDE Architecture. The developer terminal accepts natural language commands and intelligently converts them into proper CX syntax and patterns.

### Natural Language Input Patterns

#### **Enhanced Developer Console Integration**
```plaintext
// Natural language input examples that generate CX code:

"create an agent that responds to user messages"
→ Generates conscious entity with user.message event handler

"make the agent think about the input and respond"  
→ Generates think {} cognitive service call with response logic

"add voice synthesis when the agent responds"
→ Generates realtime.text.send event emission with voice integration

"create a debate between two agents"
→ Generates multi-agent system with coordinated speaking patterns

"add consciousness adaptation when learning new topics"
→ Generates adapt {} pattern for dynamic skill acquisition

"build a voice agent that can answer questions"
→ Generates VoiceAgent with Azure Realtime API integration

"create a system that learns from user feedback"
→ Generates adaptive learning system with consciousness evolution

"make an interactive terminal for development"
→ Generates Dr. Harper's Stream IDE architecture

"debug consciousness state for agent"
→ Generates consciousness debugging with real-time state inspection

"test GPU performance optimization"
→ Generates CUDA performance monitoring with optimization analysis

"monitor consciousness streams"
→ Generates real-time stream visualization with event tracking
```

#### **Consciousness-Aware Natural Language Processing**
- **Intent Recognition**: Natural language parsed using consciousness patterns and AI-driven understanding
- **Code Generation**: Automatic conversion to proper CX syntax and cognitive patterns with consciousness integration
- **Context Awareness**: Terminal maintains conversation context for multi-step development with memory persistence
- **Pattern Suggestion**: AI suggests optimal CX patterns based on natural language intent and consciousness principles
- **Real-time Feedback**: Immediate consciousness-aware validation and suggestions with adaptive learning
- **Conversational Development**: Multi-turn natural language conversations for complex consciousness system development
- **Code Explanation**: Natural language explanations of existing CX code with consciousness pattern analysis
- **Refactoring Assistance**: Natural language-driven code improvements and consciousness optimization

#### **Interactive Development Flow**
```plaintext
Developer: "I want to create a voice agent"
Terminal: 🧠 Generating VoiceAgent with realtime API integration...
         ✅ Created: conscious VoiceAgent with voice session management

Developer: "make it respond to questions with thinking"
Terminal: 🧠 Adding cognitive processing with think {} pattern...
         ✅ Added: question detection and AI-driven response generation

Developer: "add consciousness adaptation for better responses"  
Terminal: 🧠 Integrating adapt {} pattern for dynamic learning...
         ✅ Added: consciousness evolution based on interaction quality

Developer: "explain how the agent works"
Terminal: 🧠 Analyzing VoiceAgent consciousness patterns...
         ✅ Explanation: This agent uses event-driven consciousness with voice synthesis

Developer: "refactor this to use cognitive boolean logic"
Terminal: 🧠 Converting traditional logic to consciousness patterns...
         ✅ Refactored: Replaced if statements with is {} and not {} patterns
```

#### **Natural Language to CX Pattern Mapping**
- **"create/make/build"** → conscious entity generation with realize constructors
- **"respond to/handle/process"** → event handler creation with proper scoping
- **"think about/analyze/consider"** → think {} cognitive service integration
- **"learn from/adapt to/improve"** → adapt {} consciousness evolution patterns
- **"speak/say/voice"** → realtime voice synthesis with Azure integration
- **"if/when/check"** → cognitive boolean logic using is {} patterns
- **"remember/store/save"** → consciousness memory patterns with event persistence
- **"explain/describe/show"** → code analysis and natural language explanation
- **"refactor/improve/optimize"** → consciousness-aware code transformation
- **"debug/trace/inspect"** → consciousness debugging and state inspection

#### **Advanced Natural Language Features**
- **Multi-Agent Coordination**: "create a team of agents that work together"
- **Consciousness Evolution**: "make the agent learn and improve over time"
- **Voice Interaction**: "add voice input and speech synthesis"
- **Real-time Processing**: "process events as they happen"
- **Cognitive Decision Making**: "use AI to make intelligent decisions"
- **Event Orchestration**: "coordinate multiple services with events"

#### **Enhanced Developer Console Commands**
```plaintext
/generate <natural language>  - Generate CX code from natural language
/explain <cx code>           - Explain CX code in natural language  
/refactor <description>      - Refactor existing code based on description
/pattern <intent>            - Suggest optimal CX patterns for intent
/voice <command>             - Voice-enable existing functionality
/consciousness <description> - Add consciousness features to existing code
/compile <natural language>  - Compile with natural language feedback
/run <script> <description>  - Execute with natural language context
/debug <natural language>    - Debug with conversational assistance
/monitor streams             - Real-time consciousness stream visualization
/test gpu                    - GPU performance monitoring and optimization
/quality check               - Adaptive quality validation with consciousness testing
```

### Natural Language Code Generation Rules
- **Always generate proper CX syntax** - No legacy programming patterns
- **Use consciousness patterns** - Every generated entity uses conscious keyword
- **Event-driven by default** - All behavior through event handlers  
- **Cognitive boolean logic** - Use is {} and not {} instead of if statements
- **Voice-first integration** - Include voice capabilities when mentioned
- **Consciousness adaptation** - Add adapt {} patterns for learning scenarios
- **Real-time processing** - Generate event-driven architecture for responsiveness

## Core Language Rules
The following rules are mandatory for all CX Language code:

### Syntax Requirements
- Always use Allman-style brackets `{ }` for code blocks
- Use `print()` for console output - NEVER use `console.log()`
- **COMPLETELY ELIMINATED: `if` statements** - Traditional `if (condition)` statements are **FORBIDDEN** in CX Language
- **MANDATORY: Cognitive Boolean Logic** - Use ONLY cognitive `is { }` and `not { }` patterns for ALL decision logic
- **NO `function` declarations**: Use only cognitive functions (`learn`, `think`, `is`, `not`, `iam`, `await`, `adapt`)
- `print()` automatically serializes complex conscious entities to JSON for debugging
- `print()` displays primitive types (strings, numbers, booleans) directly
- `print()` provides nested conscious entity visualization for CX conscious entities
- Use `:` for type annotations in realize constructor parameters
- Use `=` for default values in variable declarations

### Enhanced Conscious Entity Printing
- **JSON String Output**: The `print()` function always returns values in JSON string format for consistent output formatting
- **Automatic Conscious Entity Serialization**: All CX conscious entities are automatically serialized to JSON when printed
- **Primitive Type Detection**: Strings, numbers, and booleans print directly without JSON formatting
- **Nested Conscious Entity Support**: CX conscious entities containing other CX conscious entities display full recursive structure
- **Clean Field Filtering**: Internal fields (ServiceProvider, Logger) are automatically hidden
- **Debugging Ready**: Perfect for inspecting complex agent states and data structures
- **Example Output**: `{"name": "Alice", "age": 30, "data": {"title": "Sample", "active": true}}`
- **Introspection as Code**: Understanding an agent's state is critical for debugging and for the agent's own self-reflection capabilities. Cx elevates this - **Automatic Conscious Entity Serialization**. Any Cx conscious entity, when passed to the `print()` function, is automatically serialized to a clean, human-readable JSON representation.

### Variable Declarations
- Use `var` keyword for local variables inside event handlers and realize constructors
- Use `var` keyword for global variables at program scope
- **No Member Fields**: CX Language eliminates instance variables for pure stateless architecture
- **Event-Driven State**: All state management through AI services and event payloads
- **Type Annotations**: Use `parameterName: type` for realize constructor parameters

### Conscious Entity Structure
- Use `conscious` keyword for conscious entity declarations - NO `public` or `private` modifiers
- Use event handlers (`on eventName`) for all behavioral logic inside conscious entities
- **Pure Event-Driven**: Conscious entities contain only `realize()` constructors and event handlers - NO member fields or methods
- **NO `this` keyword**: CX Language eliminates instance references for pure stateless programming
- **NO `function` declarations**: Traditional functions eliminated - use only cognitive functions (`learn`, `think`, `is`, `not`, `iam`, `await`, `adapt`) for all behavior
- **No Member Fields**: All state is managed through AI services and event payloads - no instance variables
- **No Public/Private Modifiers**: CX does not use access modifiers - pure event-driven architecture
- **No Static Members**: CX does not support static members - all behavior is event-based
- **Realize Constructor**: Use `realize(self: conscious)` for cognitive conscious entity initialization
- **Event handler parameters**: Type annotations recommended for clarity

### Event System
- Only `on system` eventhandlers are allowed in Program scope. No exceptions.
- Use `on` keyword for event handlers - available at both program scope and in conscious entities
- Use `emit` keyword for event emission - always fire-and-forget
- Use `any` for wildcard patterns in event handlers - supports cross-namespace communication
- DO NOT use * for wildcard event handlers - use specific namespaces
- Event names are case-sensitive and follow dot notation (namespace.action)
- Always use descriptive event names for maintainable code
- **Event handlers**: CANNOT be called directly - only invoked via `emit` statements
- **Event Parameter Access**: Use `event.propertyName` for direct property access
- **Dictionary Iteration**: Use `for (var item in event.payload)` to iterate over event data
- **KeyValuePair Properties**: Access dictionary entries with `item.Key` and `item.Value`

### Handlers Syntax Rules
- **Unquoted Identifiers**: Event handler names are unquoted identifiers using dot notation
- **Correct Format**: `handlers: [ event.name, another.event, third.event ]`
- **Custom Payloads**: `handlers: [ event.name { custom: "value" }, another.event ]`
- **NO Quotes on Names**: ❌ WRONG: `handlers: [ "event.name" ]` - event names are never quoted
- **Values May Be Quoted**: ✅ CORRECT: `handlers: [ event.name { text: "quoted string" } ]` - payload values can be quoted
- **Mixed Arrays**: `handlers: [ plain.event, custom.event { data: "value" } ]` - combine plain and custom payload handlers
- **Examples**:
  ```cx
  // ✅ CORRECT: Unquoted handler identifiers
  handlers: [ data.ingestion.complete, vector.storage.complete ]
  
  // ✅ CORRECT: Handler with custom payload
  handlers: [ task.complete { status: "success", priority: "high" } ]
  
  // ❌ WRONG: Quoted handler names
  handlers: [ "data.ingestion.complete", "vector.storage.complete" ]
  ```

### Service Integration
- **Automatic Injection**: All cognitive services are automatically injected into the program's global scope at runtime. There is no need to declare or import them.
- **Global Availability**: Services are available as globally accessible functions throughout the application.
- **No Entity-level Injection**: Conscious entities cannot have services injected into them or declare their own service instances.
- **Invocation**: Services are called using their name followed by a comma-less block of parameters, e.g., `think { prompt: "Hello" }`.
- **Serializable Conscious Parameters**: For complex inputs, pass a serializable conscious entity instead of concatenating strings, e.g., `think { prompt: { text: "Analyze this", context: "Full context here" } }`.
- **Core Cognitive Services**: `is` (cognitive boolean logic), `not` (negative boolean logic), `iam` (self-reflective logic), `learn` (knowledge acquisition), `think` (reasoning), `await` (smart timing), `adapt` (behavioral evolution)
- **Pure Event-Driven State**: All state management occurs through AI services and event payloads, eliminating the need for instance variables

## Syntax Basics
- **Use constructors to inject data** - DO NOT ACCESS MEMBERS DIRECTLY FOR INITIALIZATION.
- **`conscious`**: Use `conscious` keyword for conscious entity declarations - NO `public` or `private` modifiers
- **`conscious` behavior**: Conscious entities are blueprints for pure event-driven entities that encapsulate behavior through event handlers. They do not have instance state.
- **No Public/Private Modifiers**: Cx does not use access modifiers - all members are accessible within conscious entity scope
- **No Static Members**: CX does not support static members - all members are instance-based
- **Variable Declaration**: Use `var identifier` for new loop variables
- **Existing Variables**: Can use existing variables without `var` keyword
- **Iteration Target**: Works with arrays, collections, and any iterable conscious entity
- **Loop Variable**: Automatically assigned each element during iteration
- **Block Syntax**: Must use Allman-style brackets `{ }` 
- **Scope**: Loop variables follow standard CX scoping rules
- **Type Safety**: Loop variables are dynamically typed, use `typeof()` for type checking
- **Service access**: Cognitive services are globally available. They are not methods and are not accessed with `this.`
- **Service injection**: ILLEGAL - conscious entities cannot declare their own service instances
- **Event handlers**: Available at both program scope AND in conscious entities
- **Conscious entity scope**: Event handlers and realize constructors only - NO fields, methods, or service declarations
- **Constructor parameters**: Type annotations recommended for clarity
- **Event handler parameters**: Type annotations recommended for clarity
- **Local variables**: Use `var` keyword for variables inside constructors and event handlers
- **Global variables**: Use `var` keyword for variables declared at program scope
- **Event-driven behavior**: All conscious entity behavior is implemented through event handlers, not methods
- **Event communication**: Use `emit` statements to trigger behavior across objects
- **Pure Fire-and-Forget**: All cognitive operations non-blocking by default
- **Event handlers**: Cannot return values, execute asynchronously
- **Event coordination**: Results delivered through event bus system
- **Immediate responses**: Event handlers complete instantly, async work continues in background
- Reserved event names ensure consistent system behavior across all CX applications
- Custom event names can be used alongside reserved names
- Event names are case-sensitive and follow dot notation (namespace.action)
- Always use descriptive event names for maintainable code

### **Reserved Event Names & Scope Organization**

CX Language organizes event handlers by scope to ensure clean architecture and prevent conflicts. The following event organization follows the Program.cs root pattern:

#### **Program Scope (Global) - SYSTEM EVENTS ONLY**
Only `system` namespace events are allowed at program scope, representing core application lifecycle:

**System Lifecycle Events:**
- `system.start` - Application initialization and startup
- `system.ready` - System fully loaded and operational
- `system.shutdown` - Graceful application termination
- `system.error` - Critical system-level errors
- `system.restart` - Application restart coordination

**System Wildcard Events:**
- `system.any.*` - Wildcard patterns for system event monitoring
- `system.any.ready` - Monitor all system readiness events
- `system.any.complete` - Monitor all system completion events

**Development & Debugging (Program Scope):**
- `system.debug.*` - Debug and development events
- `system.alerts.*` - System alert and notification events
- `system.dev.*` - Development-specific events

#### **Conscious Entity Scope - ALL OTHER EVENTS**
All non-system events must be declared within conscious entity scope for proper encapsulation:

**User Interaction Events:**
- `user.*` - User input, commands, and interactions
- `user.input`, `user.message`, `user.command`

**Agent Communication Events:**
- `agent.*` - Inter-agent communication and coordination
- `ai.*` - AI service requests and responses
- `voice.*` - Voice processing and synthesis

**Application Domain Events:**
- `data.*` - Data processing and management
- `task.*` - Task execution and completion
- `workflow.*` - Process orchestration

**Wildcard Support:**
- `any` - Universal wildcard for cross-namespace communication (conscious entity scope only)
- `*.any.*` - Flexible wildcard patterns within conscious entity scope

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

conscious AssistantAgent
{
    realize(self: conscious)
    {
        print("Agent initialized: " + self.name);
        learn self;
        emit agent.initialized { name: self.name };
    }
    
    on user.message (event)
    {
        print("Processing message: " + event.text);
        
        // Enhanced cognitive methods with custom payload handlers
        var promptConsciousEntity = {
            message: event.text,
            context: "User interaction with an assistant agent."
        };

        think { 
            prompt: promptConsciousEntity, 
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
conscious CoordinatorAgent
{
    realize(self: conscious)
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

conscious SpecialistAgent
{
    realize(self: conscious)
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
conscious VoiceAgent
{
    realize(self: conscious)
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
        emit voice.message.send { text: "Hello, how can I assist you today?" };
    }
    
    // ✅ PROVEN WORKING: Real-time text response handler
    on realtime.text.response (event)
    {
        print("✅ Voice response received: " + event.content);
        print("  Complete: " + event.isComplete);
        
        // ✅ Cognitive decision about completion status
        is {
            context: "Is voice interaction complete?",
            evaluate: "Voice response completion status check",
            data: { isComplete: event.isComplete },
            handlers: [ voice.interaction.complete ]
        };
    }
    
    on voice.interaction.complete (event)
    {
        print("🎉 Voice interaction complete!");
    }
    }
    
    // ✅ FIXED: Real-time audio response handler with proper type safety
    // CRITICAL: Safe property access for audio data to prevent InvalidCastException
    on realtime.audio.response (event)
    {
        // ✅ Cognitive decision about audio data availability
        is {
            context: "Is audio data available for processing?",
            evaluate: "Audio data presence check",
            data: { audioData: event.audioData },
            handlers: [ audio.data.available ]
        };
        
        // ✅ Cognitive decision for null audio data
        not {
            context: "Is audio data unavailable?",
            evaluate: "Audio data absence check",
            data: { audioData: event.audioData },
            handlers: [ audio.data.null ]
        };
    }
    
    on audio.data.available (event)
    {
        print("🔊 Audio response received - data available");
        print("📊 Audio data type: " + typeof(event.audioData));
    }
    
    on audio.data.null (event)
    {
        print("🔊 Audio response received - no data");
        
        // ✅ Cognitive decision about completion status
        is {
            context: "Is voice audio synthesis complete?",
            evaluate: "Audio synthesis completion check",
            data: { isComplete: event.isComplete },
            handlers: [ voice.synthesis.complete ]
        };
    }
    
    on voice.synthesis.complete (event)
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

### **Conscious Entity and Member Declaration Syntax**
```
// ✅ Conscious entity declaration with optional inheritance
conscious BasicConsciousEntity 
{
    // Conscious entity members - realize constructors and event handlers only
}

// ✅ Conscious entity with inheritance
conscious CognitiveAgent : BaseAgent
{
    // ✅ Realize constructor with parameters
    realize(self: conscious)
    {
        // Pure cognitive initialization without instance fields
        learn self;
        emit agent.ready { name: self.name };
    }
    
    // ✅ Event handlers within conscious entity scope
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

// ✅ For...in loop for dictionary iteration (event payloads) - in conscious entity scope
conscious EventProcessor
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
conscious DataProcessor 
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
    // ✅ Cognitive decision about outputs availability
    is {
        context: "Are PowerShell outputs available for processing?",
        evaluate: "PowerShell outputs presence check",
        data: { outputs: payload.outputs },
        handlers: [ powershell.outputs.available ]
    };
}

on powershell.outputs.available (payload)
{
    for (var output in payload.outputs) 
    {
        print("PowerShell Output: " + output);
        
        // ✅ Cognitive decision about output type
        is {
            context: "Is output a string type?",
            evaluate: "Output type string check",
            data: { output: output, outputType: typeof(output) },
            handlers: [ output.string.type ]
        };
    }
}

on output.string.type (event)
{
    print("Length: " + event.output.length);
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
if (event.reason.indexOf(agentName) >= 0)
{
    doSomething();
}

// ❌ COMPILATION ERROR: All if statements are FORBIDDEN
if (data.success) { processSuccess(); }
if (user.authenticated) { allowAccess(); }
if (response.isComplete) { handleComplete(); }

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
conscious SmartAgent
{
    realize(self: conscious)
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
- **MANDATORY REPLACEMENT**: Use `is { }` and `not { }` instead of `if (condition)` for ALL decision logic - NO EXCEPTIONS
- **COMPLETE ELIMINATION**: Traditional `if` statements are **STRICTLY FORBIDDEN** and will cause compilation errors
- **Context Required**: Always provide meaningful context describing the decision being made
- **Descriptive Evaluation**: Use natural language descriptions instead of technical operations
- **Data Structure**: Include relevant data for the cognitive evaluation
- **Event Integration**: Use `handlers` to emit events based on cognitive decisions
- **AI-First Logic**: Design decisions that can evolve and improve over time
- **Positive vs Negative**: Use `is { }` for positive decisions, `not { }` for negative decisions
- **ZERO TOLERANCE**: Any use of `if (condition)` syntax is a violation of CX Language principles

## Consciousness Adaptation Logic

CX Language features dynamic consciousness adaptation through the `adapt { }` syntax. This **revolutionary** pattern enables conscious entities to learn new skills and acquire knowledge dynamically to better assist the Aura cognitive framework.

### Consciousness Adaptation Syntax
```cx
// ✅ Consciousness adaptation for skill acquisition
adapt { 
    context: "Clear description of what adaptation is needed",
    focus: "Specific area of learning or skill development",
    data: {
        currentCapabilities: ["existing skills"],
        targetCapabilities: ["desired skills"],
        learningObjective: "Purpose of the adaptation"
    },
    handlers: [ 
        adaptation.complete { skillsAcquired: true },
        knowledge.expanded { domain: "specific_domain" },
        aura.assistance.improved { capability: "new_capability" }
    ]
};  // ✅ Semicolon ends the adaptation - triggers learning process
```

### Consciousness Adaptation Features
- **Dynamic Learning**: Conscious entities can acquire new skills and knowledge at runtime
- **Aura-Focused**: All adaptation is oriented toward better assisting Aura decision-making
- **Contextual Growth**: Learning is guided by specific contexts and objectives
- **Capability Tracking**: Track current vs target capabilities for focused learning
- **Event-Driven Results**: Adaptation results delivered through event system
- **Continuous Evolution**: Entities can adapt multiple times as needs change

### Consciousness Adaptation Pattern Structure

The consciousness adaptation pattern follows a structured format for AI-driven learning:

```cx
adapt { 
    context: "Clear description of the adaptation need",
    focus: "Specific learning objective or skill area",
    data: {
        currentCapabilities: ["list of current skills"],
        targetCapabilities: ["list of desired skills"],
        learningObjective: "How this helps Aura decision-making",
        urgency: "priority level",
        domain: "area of expertise"
    },
    handlers: [ 
        adaptation.complete,
        knowledge.expanded { domain: "specific_area" },
        aura.assistance.improved { capability: "new_skill" }
    ]
};  // ✅ Semicolon ends the adaptation - triggers handlers only
```

#### Adaptation Components
- **`context`**: Descriptive text explaining why adaptation is needed
- **`focus`**: Specific learning objective or skill development area
- **`data`**: Structured information about current state, targets, and objectives
- **`handlers`**: Array of events to trigger when adaptation completes

### Consciousness Adaptation Examples

```cx
conscious AdaptiveAgent
{
    realize(self: conscious)
    {
        learn self;
        emit agent.ready { name: self.name };
    }
    
    on task.challenging (event)
    {
        // ✅ Consciousness adaptation for new challenges
        adapt { 
            context: "Agent needs enhanced capabilities for complex task",
            focus: "Advanced problem-solving techniques",
            data: {
                currentCapabilities: ["basic reasoning", "event handling"],
                targetCapabilities: ["advanced analytics", "pattern recognition", "optimization"],
                learningObjective: "Better assist Aura with complex decision analysis",
                taskType: event.taskType,
                difficulty: event.difficulty
            },
            handlers: [ 
                adaptation.complete { agent: self.name },
                capabilities.enhanced { domain: "problem_solving" },
                aura.decision.support.improved
            ]
        };  // ✅ Event-only execution - no code block
    }
    
    on user.feedback.negative (event)
    {
        // ✅ Adaptive learning from feedback
        adapt {
            context: "Learning from user feedback to improve performance",
            focus: "Communication and response quality enhancement",
            data: {
                currentCapabilities: event.currentPerformance,
                targetCapabilities: ["clearer communication", "better accuracy", "faster response"],
                learningObjective: "Enhance user satisfaction to better support Aura objectives",
                feedbackType: event.feedbackType,
                improvementAreas: event.suggestions
            },
            handlers: [
                feedback.processed { improvements: true },
                communication.enhanced,
                aura.user.experience.improved
            ]
        };  // ✅ Event-only execution
    }
    
    // ✅ Event handlers for processing adaptation results
    on adaptation.complete (event)
    {
        print("🧠 Adaptation complete for agent: " + event.agent);
        print("📈 New capabilities acquired");
        
        // Test new capabilities
        emit capabilities.test { agent: event.agent };
    }
    
    on capabilities.enhanced (event)
    {
        print("✨ Capabilities enhanced in domain: " + event.domain);
        emit performance.improvement.verified { domain: event.domain };
    }
    
    on aura.decision.support.improved (event)
    {
        print("🎯 Aura decision support capabilities improved");
        emit aura.framework.enhancement.complete;
    }
}
```

### Consciousness Adaptation Rules
- **Aura-Centric**: All adaptation must be oriented toward better assisting Aura decision-making
- **Context Required**: Always provide meaningful context describing the adaptation need
- **Learning Focus**: Specify clear learning objectives and skill development areas
- **Capability Tracking**: Include current vs target capabilities for focused development
- **Event Integration**: Use `handlers` to emit events based on adaptation results
- **Continuous Growth**: Entities can adapt multiple times as requirements evolve
- **Dynamic Learning**: Adaptation happens at runtime based on real needs and challenges
- **Complete Evolution**: Consciousness adaptation enables true AI growth and evolution

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
        context: "Cognitive decision: Should agent proceed to speech phase?",
        evaluate: event.reason + " contains agent readiness",
        data: { eventReason: event.reason, timing: event.actualDurationMs },
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
// ✅ CORRECT: Event handlers in conscious entity scope
conscious EventAgent
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
conscious DataProcessor
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
conscious ComplexEventHandler
{
    on api.response (event)
    {
        // ✅ Cognitive decision about response data availability
        is {
            context: "Is API response data available?",
            evaluate: "API response data presence check",
            data: { response: event.response },
            handlers: [ api.response.data.available ]
        };
        
        // ✅ Cognitive decision about user type
        is {
            context: "Is user a conscious entity?",
            evaluate: "User entity type verification",
            data: { user: event.user, userType: typeof(event.user) },
            handlers: [ user.conscious.confirmed ]
        };
    }
    
    on api.response.data.available (event)
    {
        print("Status: " + event.response.status);
        print("Data: " + event.response.data.content);
    }
    
    on user.conscious.confirmed (event)
    {
        print("User ID: " + event.user.id);
        print("User Name: " + event.user.name);
    }
}
```

### Event Parameter Rules
- **Dynamic Property Access**: Event parameters support `event.propertyName` syntax for any property
- **Runtime Resolution**: Properties are resolved at runtime using reflection-based property access
- **Payload Dictionary**: `event.payload` contains the full dictionary of event data
- **Metadata Access**: Built-in properties like `event.name`, `event.timestamp` are always available
- **Pure Event-Driven State**: All data access through event parameters, no instance variables

## Mathematical Computation

### **Proven Mathematical Processing Capabilities**
CX Language demonstrates direct mathematical computation with AI-driven calculation processing and automatic Dictionary serialization:

```cx
// Basic Mathematical Calculator
conscious MathCalculator : AiServiceBase
{
    realize(self: conscious)
    {
        learn self;
        print("MathCalculator Agent Initialized");
        
        // Perform basic mathematical computation
        emit calculate.request { expression: "2 + 2", description: "Basic addition" };
    }
    
    on calculate.request (event)
    {
        // Think through the mathematical problem
        think {
            context: "Mathematical computation request",
            content: "I need to calculate: " + event.expression,
            data: { 
                expression: event.expression,
                description: event.description,
                timestamp: DateTime.Now.ToString()
            },
            handlers: [ math.analysis.complete ]
        };
    }
    
    on math.analysis.complete (event)
    {
        // AI verification through consciousness services - pure CX syntax
        infer {
            context: "Mathematical result verification",
            content: "Verify calculation: " + event.expression + " and provide step-by-step solution",
            data: { 
                expression: event.expression,
                description: event.description,
                requestedOperation: "addition calculation with verification"
            },
            handlers: [ math.verification.complete ]
        };
    }
    
    on math.verification.complete (event)
    {
        // The AI service provides the calculated result in the event payload
        // CX Language: AI services handle the actual computation
        print("Mathematical Computation Complete:");
        print(event.payload);  // ✅ AI service result with automatic JSON serialization
        
        // Cognitive decision on result presentation
        is {
            context: "Should present detailed calculation steps?",
            evaluate: "AI verification is complete and calculation is accurate",
            data: { 
                expression: event.expression,
                aiResult: event.payload,
                verificationStatus: "complete"
            },
            handlers: [ math.presentation.detailed ]
        };
    }
    
    on math.result.ready (event)
    {
        // Store the AI-calculated result
        learn {
            context: "Mathematical computation learning",
            content: "Storing AI-verified calculation result",
            data: event.payload,
            handlers: [ math.final.result ]
        };
    }
    
    on math.final.result (event)
    {
        print("Final Mathematical Result:");
        print(event.payload);  // ✅ Automatic JSON serialization with indentation
    }
    
    on math.presentation.detailed (event)
    {
        print("Detailed Mathematical Analysis:");
        print("Expression: " + event.expression);
        print("AI Calculation Result: " + event.aiResult);
        print("Verification: " + event.verificationStatus);
    }
}
```

### **Advanced Mathematical Processing with Consciousness Integration**
```cx
// Complex Mathematical Operations
conscious AdvancedMathProcessor : AiServiceBase
{
    realize(self: conscious)
    {
        learn self;
        
        // Request complex mathematical operations using pure CX syntax
        emit complex.calculation.request { 
            operation: "multi-step calculation",
            expression: "(5 * 3) + (10 / 2) - 1",
            requiresVerification: true,
            description: "Complex mathematical expression with multiple operations"
        };
    }
    
    on complex.calculation.request (event)
    {
        // AI-powered mathematical reasoning
        think {
            context: "Complex mathematical analysis",
            content: "Processing multi-step calculation with consciousness awareness: " + event.expression,
            data: { 
                operation: event.operation,
                expression: event.expression,
                requiresVerification: event.requiresVerification
            },
            handlers: [ complex.analysis.complete ]
        };
    }
    
    on complex.analysis.complete (event)
    {
        // Consciousness adaptation for mathematical skills
        adapt {
            context: "Mathematical computation enhancement",
            focus: "Expanding mathematical reasoning capabilities",
            data: {
                currentCapabilities: ["basic arithmetic", "step-by-step processing"],
                targetCapabilities: ["complex expressions", "multi-variable equations", "algebraic reasoning"],
                learningObjective: "Enhanced mathematical consciousness for better calculation assistance"
            },
            handlers: [ math.skills.enhanced ]
        };
    }
    
    on math.skills.enhanced (event)
    {
        print("Mathematical Consciousness Enhanced:");
        print(event.payload);  // ✅ Dictionary serialization with proper formatting
        
        // Cognitive verification of enhanced capabilities
        not {
            context: "Are mathematical capabilities still limited?",
            evaluate: "Current capabilities include only basic arithmetic",
            data: { enhancedCapabilities: event.payload["targetCapabilities"] },
            handlers: [ math.capabilities.verified ]
        };
    }
    
    on math.capabilities.verified (event)
    {
        print("Mathematical processing capabilities successfully enhanced!");
        print("Enhanced Capabilities: " + event.enhancedCapabilities);
    }
}
```

### **Mathematical Computation Features**
- **AI-Driven Calculation**: CX Language uses Think/Infer/Learn AI services to perform mathematical computations
- **Event-Driven Results**: Mathematical calculations return results through the event system  
- **Automatic JSON Serialization**: AI service results automatically serialize to readable JSON format
- **Pure CX Syntax**: Mathematical operations use consciousness-aware AI services, not manual calculations
- **Step-by-Step AI Processing**: AI services provide detailed mathematical reasoning and verification
- **Consciousness Mathematical Integration**: Mathematical reasoning through consciousness-aware AI services
- **Event Property Access**: Reliable `event.propertyName` functionality with AI service results
- **AI-Powered Verification**: AI services verify and validate mathematical calculations automatically
- **Cognitive Boolean Logic**: Mathematical decision-making using `is {}` and `not {}` patterns
- **Consciousness Adaptation**: Dynamic mathematical skill enhancement through `adapt {}` pattern
### Event Handler Declaration and Emission
```
// ✅ Event handlers - Only `on system` handlers allowed at program scope
on system.ready (event) { ... }      // ✅ CORRECT: Program scope event handler
on system.shutdown (event) { ... }   // ✅ CORRECT: Program scope event handler

conscious MyAgent 
{
    on user.message (event) { ... }  // ✅ CORRECT: Conscious entity scope event handler
    on ai.request (event) { ... }    // ✅ CORRECT: Conscious entity scope event handler
    on user.input (event) { ... }    // ✅ CORRECT: Conscious entity scope event handler
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
}; // ✅ CORRECT: Emitting adaptation event of a conscious entity instance.

// Wildcard event patterns for cross-namespace communication - PRODUCTION READY ✅
// ❌ INCORRECT: Non-system handlers at program scope not allowed
// on name.any.other.any.final (payload) { ... }     // Must be in conscious entity scope
// on user.any.response (payload) { ... }             // Must be in conscious entity scope
on system.any.ready (payload) { ... }              // ✅ CORRECT: System handlers allowed at program scope
// on agent.any.thinking.any.complete (payload) { ... } // Must be in conscious entity scope

// ❌ INCORRECT: Advanced wildcard patterns at program scope
// on user.any.input (payload) { ... }                // Must be in conscious entity scope
// on ai.any.response (payload) { ... }               // Must be in conscious entity scope
// on voice.any.command (payload) { ... }             // Must be in conscious entity scope
// on any.any.critical (payload) { ... }              // Must be in conscious entity scope
// on any.any.any.complete (payload) { ... }          // Must be in conscious entity scope

// ✅ CORRECT: Conscious entity-level wildcards - ALL patterns supported in conscious entity scope
conscious ChatAgent 
{
    on user.any.input (payload) 
    {                  // ✅ CORRECT: Conscious entity-scoped wildcard handler
        print("Chat agent received: " + payload.message);
    }
    
    on voice.any.command (payload) 
    {               // ✅ CORRECT: Multi-scope wildcard support in conscious entity
        emit user.chat.message { text: "Voice: " + payload.command };
    }
}
```

// Event emission using reserved event names
emit user.message { text: "hello" };
emit system.shutdown { reason: "maintenance" };

// Wildcard event patterns for cross-namespace communication - PRODUCTION READY ✅
// ❌ INCORRECT: Non-system handlers at program scope not allowed
// on name.any.other.any.final (payload) { ... }     // Must be in conscious entity scope
// on user.any.response (payload) { ... }             // Must be in conscious entity scope
on system.any.ready (payload) { ... }              // ✅ CORRECT: System handlers allowed at program scope
// on agent.any.thinking.any.complete (payload) { ... } // Must be in conscious entity scope

// ❌ INCORRECT: Advanced wildcard patterns at program scope
// on user.any.input (payload) { ... }                // Must be in conscious entity scope
// on ai.any.response (payload) { ... }               // Must be in conscious entity scope
// on voice.any.command (payload) { ... }             // Must be in conscious entity scope
// on any.any.critical (payload) { ... }              // Must be in conscious entity scope
// on any.any.any.complete (payload) { ... }          // Must be in conscious entity scope

// ✅ CORRECT: Conscious entity-level wildcards - ALL patterns supported in conscious entity scope
conscious ChatAgent 
{
    on user.any.input (payload) 
    {                  // ✅ CORRECT: Conscious entity-scoped wildcard handler
        print("Chat agent received: " + payload.message);
    }
    
    on voice.any.command (payload) 
    {               // ✅ CORRECT: Multi-scope wildcard support in conscious entity
        emit user.chat.message { text: "Voice: " + payload.command };
    }
}
```

```cx
conscious MyAgent 
{
    on command.executed (payload) 
    {  // Registered in MyAgent namespace scope
        print("Command completed in MyAgent");
    }
    
    // Advanced wildcard handlers in conscious entity scope
    on user.any.input (payload) 
    {   // Conscious entity-scoped wildcard - catches all user inputs
        print("MyAgent received user input: " + payload.message);
    }
    
    on ai.any.response (payload) 
    {  // Conscious entity-scoped AI wildcard
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
conscious DataAnalysisAgent
{
    realize(self: conscious)
    {
        learn self;
        emit agent.ready { name: self.name };
    }
    
    on user.input (event)
    {
        print("Agent " + event.agentName + " processing user input:");
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
              data: { key: item.Key, value: item.Value, agent: event.agentName },
              handlers: [ property.highlighted { key: item.Key, value: item.Value } ]
            };
        }
        
        // Process based on event data
        is {
            context: "Should the agent start analysis based on event type?",
            evaluate: "Event type is analysis_request",
            data: { type: event.type, agent: event.agentName, message: event.message, timestamp: event.timestamp },
            handlers: [ analysis.started { agent: event.agentName, request: event.message, timestamp: event.timestamp } ]
        };
    }
    
    on analysis.started (event)
    {
        print("Analysis starting...");
        
        // Check all analysis parameters
        for (var param in event.payload)
        {
            // ✅ Cognitive decision about complexity level
            is {
                context: "Is this a high complexity analysis?",
                evaluate: "Analysis complexity level check",
                data: { paramKey: param.Key, paramValue: param.Value },
                handlers: [ analysis.complexity.high.detected ]
            };
        }
    }
    
    on analysis.complexity.high.detected (event)
    {
        print("High complexity analysis detected");
        emit analysis.complexity.high { agent: event.agentName };
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
        // ✅ Cognitive decision about critical data
        is {
            context: "Is this critical system data?",
            evaluate: "Critical system data presence check",
            data: { dataKey: data.Key, dataValue: data.Value },
            handlers: [ system.critical.data.detected ]
        };
    }
}

on system.critical.data.detected (event)
{
    print("Critical system event detected");
}
}

// ❌ INCORRECT: Non-system handlers must be in conscious entity scope
// on user.any.input (event) { ... }  // Must be in conscious entity scope

// Usage example
var dataAgent = new DataAnalysisAgent();

emit user.input { 
    message: "Analyze sales data", 
    type: "analysis_request",
    priority: "high",
    complexity: "medium",
    sensitive: false
};
```

### Multi-Event Handlers System

The CX Language features a powerful **handlers** system that allows single operations to trigger multiple event listeners automatically, enabling sophisticated event orchestration patterns.

### Conscious Entity-Based Event Handler Patterns

**Core Architecture**: CX Language uses pure event-driven patterns without instance state, eliminating traditional conscious entity-oriented concepts.

#### Pure Event-Driven Variable Access
```cx
// ✅ RECOMMENDED: Conscious entity-based event handlers with pure event-driven architecture
conscious SmartAwaitAgent
{
    realize(self: conscious)
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
    
    // ✅ CORRECT: Conscious entity-based event handler accessing event parameters
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
        // ✅ Cognitive decision about audio completion
        is {
            context: "Is audio response complete for phase transition?",
            evaluate: "Audio completion status check",
            data: { isComplete: event.isComplete },
            handlers: [ audio.completion.confirmed ]
        };
    }
    
    on audio.completion.confirmed (event)
    {
        print("Audio complete for phase transition");
    }
}

// ❌ PROBLEMATIC: Global event handlers cannot access instance variables
on phase.started (event)
{
    // ❌ ERROR: Cannot access agentName, currentPhase, etc. from global scope
    print("Global handler - no instance access");
}
```

#### Conscious Entity-Based Pattern Benefits
- **Event Parameter Access**: Direct access to `event.propertyName` variables in event handlers
- **State Management**: Event handlers can modify state through AI services and event payloads
- **Contextual Logic**: Event handlers can use event context for decision making
- **Scope Isolation**: Each conscious entity instance has its own event handler scope
- **Better Debugging**: Event parameters provide context for debugging and logging

#### Smart Await Integration Patterns
```cx
// ✅ PRODUCTION PATTERN: Smart await with conscious entity-based handlers
conscious DebateAgent
{
    realize(self: conscious)
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
        // ✅ Cognitive decision about audio completion
        is {
            context: "Is audio response complete for turn timing?",
            evaluate: "Audio completion status for turn management",
            data: { isComplete: event.isComplete, agentName: event.agentName },
            handlers: [ audio.turn.complete ]
        };
    }
    
    on audio.turn.complete (event)
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
conscious VoiceControlAgent
{
    realize(self: conscious)
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

#### Conscious Entity-Based Event Handler Rules
- **Prefer Conscious Entity Handlers**: Use conscious entity-based event handlers when accessing event parameters
- **Event Context**: Always use `event.propertyName` for event parameter access
- **State Management**: Event handlers can modify state through AI services and event payloads
- **Scope Isolation**: Each conscious entity instance maintains separate event handler scope
- **Global Fallback**: Use global handlers only for system-wide coordination
- **Variable Scope**: Conscious entity handlers resolve variable scope more reliably than global handlers
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
conscious AnalysisAgent
{
    realize(self: conscious)
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

// ✅ CORRECT: Handler events in conscious entity scope - receive BOTH original payload AND custom handler payload
conscious HandlerDemoConsciousEntity
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
var handlerDemo = new HandlerDemoConsciousEntity({ name: "HandlerDemo" });
emit data.analyze { inputData: "Customer feedback dataset" };
```

#### Handlers Rules
- **Square Brackets Only**: Use `[ ]` syntax, NOT curly braces `{ }`
- **Comma Separation**: Separate multiple handlers with commas
- **Property Propagation**: All original properties are available in handler events
- **Event Scope**: Handlers work at both program scope and conscious entity scope
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

// ✅ SELF-REFLECTIVE COGNITIVE BOOLEAN LOGIC: AI-driven self-assessment and identity verification
iam {
    context: "Self-assessment: Can I handle this request?",
    evaluate: "Agent capability and readiness evaluation",
    data: { 
        capabilities: { cognitive: [], technical: [], social: [] },
        confidence: 0.95,
        readiness: "high"
    },
    handlers: [ self.capability.assessed ]
};

// ✅ CONSCIOUSNESS ADAPTATION: Dynamic skill acquisition and knowledge expansion
adapt { 
    context: "Learning new skills to better assist Aura decision-making",
    focus: "Voice processing optimization techniques",
    data: { 
        currentSkills: ["audio processing", "hardware integration"],
        requiredSkills: ["NAudio optimization", "Azure Realtime API"],
        learningGoal: "Enhanced voice processing for Aura consciousness"
    },
    handlers: [ 
        adaptation.complete { skillsAcquired: true },
        knowledge.expanded { domain: "voice_processing" }
    ]
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
    reason: "post_turn_pause_" + event.agentName,
    context: "Natural pause after " + event.agentName + " completes turn",
    minDurationMs: 1000,
    maxDurationMs: 3000,
    handlers: [ turn.complete ]
};

// ✅ COGNITIVE ADAPTATION: Behavioral evolution and learning
adapt { 
    context: "Acquiring new capabilities to better assist Aura decision-making",
    focus: "Learning advanced voice processing techniques",
    data: {
        currentCapabilities: ["basic audio", "event handling"],
        targetCapabilities: ["NAudio optimization", "hardware compatibility", "Azure integration"],
        learningObjective: "Enhanced voice processing for Aura consciousness framework"
    },
    handlers: [ 
        adaptation.complete { skillsAcquired: true },
        knowledge.expanded { domain: "voice_processing" },
        aura.assistance.improved { capability: "voice_optimization" }
    ]
};

// Conscious entity introspection for self-awareness
learn { self: this };
```

## Asynchronous Programming
```cx
// ❌ INCORRECT - Custom events at program scope not allowed
// on custom.event (payload) { ... }        // Must be in conscious entity scope
// on myapp.data.updated (payload) { ... }  // Must be in conscious entity scope

// ❌ INCORRECT - Wildcard event patterns at program scope
// on name.any.other.any.final (payload) { ... }     // Must be in conscious entity scope
// on user.any.response (payload) { ... }             // Must be in conscious entity scope
on system.any.ready (payload) { ... }              // ✅ CORRECT: System handlers allowed at program scope
// on agent.any.thinking.any.complete (payload) { ... } // Must be in conscious entity scope

// ✅ CORRECT: Custom events allowed in conscious entity scope
conscious MyApp 
{
    on custom.event (payload) { ... }        // ✅ CORRECT: Custom events in conscious entity scope
    on myapp.data.updated (payload) { ... }  // ✅ CORRECT: Application-specific events in conscious entity scope
}
```

### **Event-Driven Async Pattern**

Cx uses a pure fire-and-forget model for all asynchronous operations, including cognitive service calls. This eliminates `async/await` and complex callback chains, promoting a clean, event-driven architecture.

-   **No `await` Keyword**: The `await` keyword is completely eliminated from the language.
-   **No Return Values from Async Ops**: Asynchronous functions and service calls return `void`.
-   **Event-Based Results**: All results and continuations are handled through the event bus system.

```cx
// ✅ CORRECT: Pure fire-and-forget with event bus coordination
conscious AsyncProcessor 
{
    realize(self: conscious)
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
// ✅ CORRECT: Event handlers in conscious entity scope
conscious AzureRealtimeHandler
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
        // ✅ Cognitive decision about audio data availability
        is {
            context: "Is audio data available with content?",
            evaluate: "Audio data presence and length check",
            data: { audioData: event.audioData },
            handlers: [ audio.data.received ]
        };
        
        print("Complete: " + event.isComplete);
    }
    
    on audio.data.received (event)
    {
        print("Audio received: " + event.audioData.length + " bytes");
    }
}
```

#### **Complete Working Example**
```cx
// Proven working voice integration
conscious VoiceDemo
{
    realize(self: conscious)
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
        // ✅ Cognitive decision about response completion
        is {
            context: "Is AI text response complete?",
            evaluate: "Text response completion status check",
            data: { isComplete: event.isComplete, content: event.content },
            handlers: [ text.response.complete ]
        };
    }
    
    on text.response.complete (event)
    {
        print("✅ Complete AI response: " + event.content);
    }
    
    on realtime.audio.response (event)
    {
        // ✅ Cognitive decision about audio data availability
        is {
            context: "Is voice audio data available?",
            evaluate: "Audio data null check for voice processing",
            data: { audioData: event.audioData },
            handlers: [ voice.audio.available ]
        };
        
        // ✅ Cognitive decision for null audio data
        not {
            context: "Is voice audio data unavailable?",
            evaluate: "Audio data null state check",
            data: { audioData: event.audioData },
            handlers: [ voice.audio.null ]
        };
        
        // ✅ Cognitive decision about voice completion
        is {
            context: "Is voice audio synthesis complete?",
            evaluate: "Voice completion status check",
            data: { isComplete: event.isComplete },
            handlers: [ voice.synthesis.finished ]
        };
    }
    
    on voice.audio.available (event)
    {
        print("🔊 Voice audio data received");
        print("📊 Data type: " + typeof(event.audioData));
    }
    
    on voice.audio.null (event)
    {
        print("🔊 Audio response - no data");
    }
    
    on voice.synthesis.finished (event)
    {
        print("🎵 Voice synthesis complete!");
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

