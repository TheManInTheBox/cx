---
applyTo: "**/*.cx"
description: "CX Cognitive Executor Language - Official Language Patterns"
---

# CX Language - Official Patterns Reference

## 🎯 Core Aura Architecture Pattern

```cx
// ✅ PREFERRED PATTERN: Class scope service injection using 'uses' keyword
// Services injected at class level - available in all methods and event handlers
class AuraAnimalAgent
{
    uses textGen from Cx.AI.TextGeneration;
    uses tts from Cx.AI.TextToSpeech;
    
    name: string;
    isAwake: boolean;
    inConversation: boolean;
    
    constructor(config)
    {
        this.name = config.name;
        this.isAwake = false;
        this.inConversation = false;
        
        // ✅ Services available in constructor due to class scope injection
        if (textGen && tts)
        {
            print("✅ Class scope service injection active");
        }
    }
    
    function speakBeepBoop(message, isActivation)
    {
        if (isActivation)
        {
            // ✅ Services available in class methods
            var sound = "[Wild Animal voice] BEEP-BOOP! " + message + " BEEP-BOOP!";
            tts.SpeakAsync(sound);
        }
    }
    
    // Always-On Audio Processing - Priority #1
    on live.audio (payload)
    {
        var audioText = payload.transcript;
        
        // Voice Command Detection
        if (audioText == "aura on" || audioText == "AURA ON")
        {
            this.isAwake = true;
            this.speakBeepBoop("ANIMAL AWAKE! AURA READY!", true);
            emit aura.system.activated, this.name;
        }
        
        // Multi-Modal State-Dependent Processing - Priority #4
        if (this.isAwake && audioText.includes("hello"))
        {
            // ✅ Services available in event handlers
            var response = textGen.GenerateAsync("Wild Animal greeting response");
            tts.SpeakAsync(response);
        }
    }
    
    // State-Dependent Sensory Processing - Priority #3
    on presence.detected (payload)
    {
        if (!this.isAwake) return;  // Intelligent state management
        
        // ✅ Services available in all event handlers
        var reaction = textGen.GenerateAsync("Animal sees movement!");
        tts.SpeakAsync(reaction);
    }
    
    // Cross-namespace event handling - Priority #5
    on aura.system.activated (payload)
    {
        emit coordination.start, { agent: this.name };
        this.beginAutonomousBehavior();
    }
}

// Autonomous Agent Creation with Event-Driven Coordination
var animalAgent = agent AuraAnimalAgent({ name: "ANIMAL" });

// System-Level Event Coordination
emit live.audio, { transcript: "Aura on", confidence: 0.95 };
```

## 🚨 DEVELOPMENT STANDARDS: IMPLEMENTATION-ONLY ACCEPTANCE

**CRITICAL REQUIREMENT**: No coded simulations, mockups, or placeholder implementations are acceptable. All CX Language features and Aura capabilities must be implemented to completion with full operational capability.

**Acceptance Criteria:**
- ✅ **Fully Functional**: Feature works end-to-end in production CX runtime
- ✅ **No Simulations**: Real implementations only - no mock objects or placeholder code
- ✅ **Complete Integration**: Seamless integration with CX Language compiler and runtime
- ✅ **Tested & Verified**: Demonstrable working examples with measurable performance
- ❌ **NOT Acceptable**: Proof-of-concept code, partial implementations, or simulation frameworks

**Quality Gate**: Every feature must pass the "Demo Test" - can be demonstrated live with real CX code execution, not simulated behavior.

**Implementation Standards:**
- All Aura capabilities must compile and execute in CX runtime
- AI service integrations must work with real Azure OpenAI endpoints
- Event-driven architecture must function with production event bus
- Agent coordination must demonstrate real multi-agent interactions
- Voice processing must use actual audio capture and synthesis

## 🧠 Aura Framework Implementation Patterns

### Agent Event System Architecture
```cx
class AuraAnimalAgent
{
    uses textGen from Cx.AI.TextGeneration;
    uses tts from Cx.AI.TextToSpeech;
    
    auraEnabled: boolean;
    isAwake: boolean;
    inConversation: boolean;
    
    constructor(config)
    {
        this.auraEnabled = false;
        this.isAwake = false;
        this.inConversation = false;
    }
    
    // ✅ CRITICAL: 'on' blocks INSIDE agent class definition
    // These are NOT regular methods - they're event handlers
    on live.audio (payload)
    {
        // Always processes audio - Priority #1: Always-On Audio Processing
        var audioText = payload.transcript;
        
        if (audioText == "aura on" || audioText == "AURA ON")
        {
            this.auraEnabled = true;
            this.isAwake = true;
            this.inConversation = true;
            this.speakBeepBoop("ANIMAL AWAKE! AURA READY!", true);
            emit aura.system.activated, this.name;
        }
        
        if (audioText == "aura off" || audioText == "AURA OFF")
        {
            this.auraEnabled = false;
            this.isAwake = false;
            this.inConversation = false;
            this.speakBeepBoop("ANIMAL SLEEP NOW... AURA OFF...", false);
            emit aura.system.deactivated, this.name;
        }
        
        // Conversation processing when active
        if (this.auraEnabled && this.isAwake && this.inConversation)
        {
            var response = this.generateAnimalResponse(payload.transcript);
            this.speakAnimalResponse(response);
        }
    }
    
    // Priority #3: Intelligent State Management - State-dependent processing
    on presence.detected (payload)
    {
        if (!this.auraEnabled || !this.isAwake) return; // Smart conditional processing
        
        var presenceResponse = this.generateAnimalResponse("Someone here! Animal see you!");
        this.speakAnimalResponse(presenceResponse);
    }
    
    on environment.change (payload)  
    {
        if (!this.auraEnabled || !this.isAwake) return; // State-dependent processing
        
        var envResponse = this.generateAnimalResponse("Something different! Animal notice!");
        this.speakAnimalResponse(envResponse);
    }
    
    // Priority #2: Animal Personality Integration
    function speakBeepBoop(message, isActivation)
    {
        if (isActivation)
        {
            var activationSound = "[Wild, chaotic Animal from Muppets voice] BEEP-BOOP! BEEP-BOOP! " + message + " DRUMS! CYMBALS! BEEP-BOOP!";
            tts.SpeakAsync(activationSound);
        }
        else
        {
            var shutdownSound = "[Tired Animal voice] beep-boop... " + message + " ...zzz...beep-boop";
            tts.SpeakAsync(shutdownSound);
        }
    }
    
    function generateAnimalResponse(userInput)
    {
        var prompt = "Respond as Animal from the Muppets - wild, enthusiastic, broken English, drum references, short energetic phrases. To: '" + userInput + "'";
        
        return textGen.GenerateAsync(prompt, {
            temperature: 0.9,
            maxTokens: 60
        });
    }
    
    function speakAnimalResponse(response)
    {
        var voiceResponse = "[Wild Animal voice] BEEP-BOOP! " + response + " BEEP-BOOP!";
        tts.SpeakAsync(voiceResponse);
    }
}

// Priority #5: Event-Driven Architecture - Auto-registration
var animalAgent = agent AuraAnimalAgent({ name: "ANIMAL" });
```

### Cognitive Processing Patterns
```cx
// Cognition: Environmental awareness triggers state transitions
on presence.detected (payload)
{
    // Cognitive decision: Only process when aware/enabled
    if (!this.auraEnabled) return; // Attention gating
    
    // Cognitive analysis: Process sensory input
    var analysis = this.analyzePrescience(payload);
    
    // Cognitive action: Respond based on analysis
    emit cognitive.response, { decision: analysis };
}

// Cognitive state introspection
function getCurrentCognitiveState()
{
    return {
        auraEnabled: this.auraEnabled,    // Conscious awareness toggle
        isAwake: this.isAwake,           // Alertness level
        inConversation: this.inConversation, // Social engagement state
        isProcessingOptimal: this.auraEnabled && this.isAwake
    };
}
```

### Multi-Agent Cognitive Coordination
```cx
uses cognitiveOrchestrator from Cx.Aura.Coordination;

// Aura detects environmental shifts, Cx coordinates cognitive responses
function respondToAuraShift(agents, ambientChange)
{
    for (agent in agents)
    {
        // Cognitive sensory processing: Each agent perceives environmental changes
        var cognitiveResponse = agent.perceiveAura(ambientChange);
        
        // Cognitive coordination: Orchestrate based on cognitive state analysis
        var coordinationDecision = cognitiveOrchestrator.analyzeCognitiveStates(agent, cognitiveResponse);
        
        // Multi-agent cognitive integration
        if (coordinationDecision.requiresCollaboration)
        {
            emit cognitive.collaboration.needed, {
                initiatingAgent: agent.name,
                cognitiveState: agent.getCurrentCognitiveState(),
                collaborationPlan: coordinationDecision.plan
            };
        }
        
        // Execute cognitive coordination
        cognitiveOrchestrator.coordinate(agent, cognitiveResponse);
    }
}
```

### Aura Sensory Processing Flow
```cx
class AuraSensorAgent
{
    uses textGen from Cx.AI.TextGeneration;
    
    auraEnabled: boolean;
    
    // Primary sensory input - always active
    on live.audio (payload)
    {
        if (payload.transcript == "aura on")
        {
            this.auraEnabled = true;
            emit aura.system.activated, this.name;
        }
    }
    
    // Secondary sensors - state-dependent
    on presence.detected (payload)
    {
        if (!this.auraEnabled) return; // Intelligent state management
        
        var analysis = textGen.GenerateAsync("Analyze presence: " + payload.data);
        emit presence.analyzed, {
            raw: payload.data,
            analysis: analysis,
            sensor: this.name
        };
    }
}
```

## 🏗️ CX Language Design Principles

### Single-Object Constructor Pattern (Required)
All CX classes must use the single-object constructor pattern for consistency and clean dependency injection:

```cx
// ✅ CORRECT: Single object parameter
constructor(config)
{
    this.name = config.name;
    this.age = config.age;
    this.settings = config.settings;
}

// ✅ CORRECT: Async constructor for initialization requiring await
async constructor(config)
{
    this.name = config.name;
    this.config = config;
    
    // Async initialization - useful for AI service setup
    if (config.autoInitialize)
    {
        await this.initializeAsync();
    }
}

async function initializeAsync()
{
    // AI service initialization that requires await
    var greeting = await textGen.GenerateAsync("Generate welcome message");
    await tts.SpeakAsync(greeting);
    
    this.initialized = true;
}

// Usage with object literal
var instance = new MyClass({
    name: "test",
    age: 25,
    settings: { debug: true }
});

// Usage with async constructor (if needed)
var asyncInstance = await new AsyncMyClass({
    name: "async-test",
    autoInitialize: true
});

// ❌ INCORRECT: Multiple parameters
constructor(name, age, settings) // Not supported in CX
```

**Benefits:**
- **Clean Service Injection**: Services handled separately from user config
- **Flexible Initialization**: Easy to add new properties without breaking changes
- **Consistent API**: All constructors follow the same pattern
- **Object Composition**: Natural integration with CX object literals

### Mandatory Code Style Rules
```cx
// ✅ REQUIRED: Allman-style braces (opening bracket on new line)
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

// ❌ FORBIDDEN: K&R-style brackets
function example() {  // NOT ALLOWED
    if (condition) {  // NOT ALLOWED
```

## 📋 Complete Language Reference

### Variables and Data Types
```cx
var name: string = "value";
var count: number = 42;
var active: boolean = true;
var data: object = { key: "value" };
var items: array = [1, 2, 3];
```

### Control Flow
```cx
// Conditionals
if (condition)
{
    // code
}
else if (otherCondition)
{
    // code
}
else
{
    // code
}

// Loops
while (condition)
{
    // code
}

for (item in collection)
{
    // code
}
```

### Functions
```cx
// Basic function
function functionName()
{
    body;
}

// With parameters
function functionName(param1, param2)
{
    body;
}

// With return type (optional)
function functionName() -> string
{
    return "value";
}

// Async functions - Comprehensive patterns for asynchronous operations
async function asyncFunction()
{
    var result = await someAsyncOperation();
    return result;
}

// Async function with error handling
async function safeAsyncFunction()
{
    try
    {
        var result = await someAsyncOperation();
        return result;
    }
    catch (error)
    {
        print("Async operation failed: " + error);
        throw error;
    }
}

// Async function with multiple await operations
async function multipleAsyncOperations()
{
    // Sequential execution - each waits for the previous
    var first = await firstAsyncOperation();
    var second = await secondAsyncOperation(first);
    var third = await thirdAsyncOperation(second);
    
    return {
        first: first,
        second: second,
        third: third
    };
}

// Async function with conditional await
async function conditionalAsync(shouldExecute)
{
    if (shouldExecute)
    {
        var result = await expensiveOperation();
        return result;
    }
    
    return "skipped";
}

// Async AI service patterns (common in CX)
async function aiServiceAsync()
{
    try
    {
        // AI services return promises that need await
        var generated = await textGen.GenerateAsync("Create a story", {
            temperature: 0.8,
            maxTokens: 500
        });
        
        var spoken = await tts.SpeakAsync(generated);
        
        return {
            text: generated,
            audio: spoken
        };
    }
    catch (error)
    {
        print("AI service failed: " + error);
        return null;
    }
}
```

### Exception Handling
```cx
try
{
    riskyOperation();
}
catch (error)
{
    handleError(error);
}

// Throw statements
throw "Error message";
throw errorObject;
```

### Object and Array Literals
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

## 🤖 AI Service Integration

### Service Injection Patterns
```cx
// ✅ PREFERRED: Class scope service injection
class MyAgent
{
    // Services available in all class methods, event handlers, and constructor
    uses textGen from Cx.AI.TextGeneration;
    uses chatBot from Cx.AI.ChatCompletion;
    uses imageGen from Cx.AI.TextToImage;
    uses embeddings from Cx.AI.TextEmbeddings;
    uses tts from Cx.AI.TextToSpeech;
    uses vectorDB from Cx.AI.VectorDatabase;
    
    constructor(config)
    {
        // Services available in constructor due to class scope injection
        if (textGen && vectorDB)
        {
            print("Services available at class scope");
        }
    }
    
    on document.process (payload)
    {
        // Services available in event handlers due to class scope injection
        var summary = textGen.GenerateAsync("Summarize: " + payload.content);
        vectorDB.IngestTextAsync(summary, "doc-" + payload.id);
    }
    
    function processData(data)
    {
        // Services available in class methods due to class scope injection
        var analysis = textGen.GenerateAsync("Analyze: " + data, {
            temperature: 0.8,
            maxTokens: 500
        });
        
        return analysis;
    }
}

// ❌ NOT RECOMMENDED: Global scope service injection
// This makes services available everywhere but loses class encapsulation benefits
uses globalTextGen from Cx.AI.TextGeneration;
```

### AI Service Usage Examples
```cx
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

### Async/Await Patterns for AI Services
```cx
// ✅ PREFERRED: Async functions for AI service coordination
async function coordinateAIServices()
{
    try
    {
        // Sequential AI operations - each waits for the previous
        var story = await textGen.GenerateAsync("Write a short story", {
            temperature: 0.8,
            maxTokens: 200
        });
        
        var image = await imageGen.GenerateImageAsync(
            "Illustration for: " + story,
            { size: "1024x1024", quality: "hd" }
        );
        
        var audio = await tts.SpeakAsync(story);
        
        return {
            story: story,
            image: image,
            audio: audio
        };
    }
    catch (error)
    {
        print("AI coordination failed: " + error);
        throw error;
    }
}

// ✅ Async class methods with AI services
class AICoordinator
{
    uses textGen from Cx.AI.TextGeneration;
    uses tts from Cx.AI.TextToSpeech;
    uses vectorDB from Cx.AI.VectorDatabase;
    
    // Async constructor pattern (if needed)
    async constructor(config)
    {
        this.config = config;
        
        // Wait for initial AI setup
        var greeting = await textGen.GenerateAsync("Generate welcome message");
        await tts.SpeakAsync(greeting);
        
        print("AI Coordinator initialized with async setup");
    }
    
    // Async method for complex AI workflows
    async processDocument(document)
    {
        try
        {
            // Sequential processing
            var summary = await textGen.GenerateAsync(
                "Summarize this document: " + document.content
            );
            
            var embedding = await embeddings.GenerateEmbeddingAsync(summary);
            
            var ingestResult = await vectorDB.IngestTextAsync(
                summary, 
                "doc-" + document.id
            );
            
            await tts.SpeakAsync("Document processed: " + document.title);
            
            return {
                summary: summary,
                embedding: embedding,
                stored: ingestResult
            };
        }
        catch (error)
        {
            print("Document processing failed: " + error);
            return null;
        }
    }
    
    // Async event handler
    async on document.received (payload)
    {
        var result = await this.processDocument(payload.document);
        
        if (result)
        {
            emit document.processed, {
                documentId: payload.document.id,
                result: result
            };
        }
    }
}

// ✅ Error-safe async patterns
async function safeAIOperation(prompt)
{
    var maxRetries = 3;
    var attempt = 0;
    
    while (attempt < maxRetries)
    {
        try
        {
            var result = await textGen.GenerateAsync(prompt, {
                temperature: 0.7,
                maxTokens: 150
            });
            
            return result; // Success - exit retry loop
        }
        catch (error)
        {
            attempt++;
            print("AI operation attempt " + attempt + " failed: " + error);
            
            if (attempt >= maxRetries)
            {
                throw "AI operation failed after " + maxRetries + " attempts";
            }
            
            // Wait before retry (simple delay)
            var delay = attempt * 1000; // Exponential backoff
            await new Promise(resolve => setTimeout(resolve, delay));
        }
    }
}
```

## ⚡ Event-Driven Architecture

### Critical Syntax Rules
- **`if`**: Used for ALL conditional logic everywhere (functions, classes, event handlers, standalone code)
- **`emit`**: Globally available everywhere (functions, classes, standalone code, event handlers)  
- **`on`**: Defines event receiver functions (global or class instance level)
- **Event names**: UNQUOTED dot-separated identifiers (user.input, not "user.input")
- **⚠️ CRITICAL: `emit` ALWAYS requires object literal payload** - never simple strings, variables, or primitive values### Event Handling Patterns
```cx
// ✅ Class-Based Event Handlers (Auto-Registration)
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

// CRITICAL RULE: emit ALWAYS requires object literal - never simple values
// ✅ CORRECT: Object literal required
emit support.tickets.new, { 
    ticketId: "T-001", 
    priority: "high" 
};

emit dev.tasks.assigned, { 
    taskId: "TASK-123", 
    assignee: "developer" 
};

emit system.critical, { 
    server: "web-01", 
    issue: "high memory usage" 
};

// ❌ INCORRECT: Simple values not allowed
emit support.tickets.new, "T-001";        // COMPILE ERROR
emit dev.tasks.assigned, payload.data;    // COMPILE ERROR  
emit system.critical, this.name;          // COMPILE ERROR
```

### Async Event Handler Patterns
```cx
// ✅ Async event handlers for AI service integration
class AIAgent
{
    uses textGen from Cx.AI.TextGeneration;
    uses tts from Cx.AI.TextToSpeech;
    
    name: string;
    
    constructor(config)
    {
        this.name = config.name;
    }
    
    // Async event handler with await operations
    async on document.process (payload)
    {
        try
        {
            print("Processing document: " + payload.documentId);
            
            // Sequential async operations
            var summary = await textGen.GenerateAsync(
                "Summarize: " + payload.content,
                { maxTokens: 200 }
            );
            
            var audioSummary = await tts.SpeakAsync(
                "Document processed: " + summary
            );
            
            // Emit completion event
            emit document.completed, {
                documentId: payload.documentId,
                summary: summary,
                processedBy: this.name
            };
        }
        catch (error)
        {
            print("Document processing failed: " + error);
            
            emit document.failed, {
                documentId: payload.documentId,
                error: error,
                agent: this.name
            };
        }
    }
    
    // Async event handler with conditional await
    async on user.query (payload)
    {
        if (payload.requiresAI)
        {
            var response = await textGen.GenerateAsync(
                "Answer: " + payload.question
            );
            
            emit user.response, {
                queryId: payload.queryId,
                answer: response
            };
        }
        else
        {
            // Synchronous response for simple queries
            emit user.response, {
                queryId: payload.queryId,
                answer: "Simple response: " + payload.question
            };
        }
    }
    
    // Async event handler with error handling and retries
    async on ai.retry.needed (payload)
    {
        var maxRetries = 3;
        var attempt = 0;
        
        while (attempt < maxRetries)
        {
            try
            {
                var result = await textGen.GenerateAsync(payload.prompt);
                
                emit ai.retry.success, {
                    originalPayload: payload,
                    result: result,
                    attempts: attempt + 1
                };
                
                return; // Success - exit retry loop
            }
            catch (error)
            {
                attempt++;
                
                if (attempt >= maxRetries)
                {
                    emit ai.retry.failed, {
                        originalPayload: payload,
                        finalError: error,
                        totalAttempts: attempt
                    };
                }
                
                // Wait before retry
                await new Promise(resolve => setTimeout(resolve, 1000 * attempt));
            }
        }
    }
}
```

### Extended Event Name Grammar Support
Keywords supported in event names: new, critical, assigned, tickets, tasks, support, dev, system, alerts

```cx
// ✅ All these patterns work:
on support.tickets.new (payload) { ... }
on dev.tasks.assigned (payload) { ... }
on system.critical (payload) { ... }
on any.critical (payload) { ... }  // Wildcard matching

emit support.tickets.new, { ticketId: "T-001" };
emit dev.tasks.assigned, { taskId: "TASK-123" };
emit system.critical, { server: "web-01" };
```

## 🎯 Priority Capabilities Implementation Patterns

### 1. Always-On Audio Processing
```cx
on live.audio (payload)  // Continuous listening capability
{
    var audioText = payload.transcript;
    
    // Voice activation detection
    if (audioText == "aura on" || audioText == "AURA ON")
    {
        this.isAwake = true;
        emit aura.system.activated, payload;
    }
}
```

### 2. Animal Personality Integration
```cx
function speakBeepBoop(message, isActivation)
{
    if (isActivation)
    {
        var animalSound = "[Wild Animal voice] BEEP-BOOP! " + message + " BEEP-BOOP!";
        tts.SpeakAsync(animalSound);
    }
    else
    {
        var response = textGen.GenerateAsync("Respond as wild animal character");
        tts.SpeakAsync(response);
    }
}
```

### 3. Intelligent State Management
```cx
class AuraAgent
{
    isAwake: boolean;
    inConversation: boolean;
    
    on environment.change (payload)
    {
        if (!this.isAwake) return;  // State-conditional processing
        
        // Only process when agent is active
        this.processEnvironment(payload);
    }
}
```

### 4. Multi-Modal Coordination
```cx
// Audio always active, other modalities state-dependent
on live.audio (payload)
{
    // Always processes audio input
    this.processAudio(payload);
}

on presence.detected (payload)
{
    if (!this.isAwake) return;  // Only when active
    this.processPresence(payload);
}

on environment.change (payload)
{
    if (!this.isAwake) return;  // Only when active
    this.processEnvironment(payload);
}
```

### 5. Event-Driven Architecture
```cx
// Complex agent interactions
on aura.system.activated (payload)
{
    emit coordination.start, { agent: this.name };
    this.beginAutonomousBehavior();
}

on coordination.start (payload)
{
    if (payload.agent != this.name)
    {
        // React to other agents activating
        emit agent.acknowledgment, { responder: this.name };
    }
}
```

## 📋 Quick Implementation Checklist
- ✅ **Agent Creation**: Use `agent ClassName()` for autonomous agents
- ✅ **Class Service Injection**: Use `uses` statements at class scope (preferred)
- ✅ **Async/Await**: Use `async function` and `await` for asynchronous operations
- ✅ **AI Service Async**: Always await AI service calls (`await textGen.GenerateAsync()`)
- ✅ **Async Constructors**: Use `async constructor()` for initialization requiring await
- ✅ **Async Event Handlers**: Use `async on event.name` for asynchronous event processing
- ✅ **Always-On Audio**: Implement `on live.audio` handlers that always process
- ✅ **State Management**: Use boolean flags (`isAwake`, `inConversation`) for conditional processing
- ✅ **Animal Personality**: Wild character voices with "BEEP-BOOP" acknowledgments
- ✅ **Multi-Modal Coordination**: Audio always active, other senses state-dependent
- ✅ **Event-Driven Flow**: Complex agent interactions via `emit` and `on` patterns
- ✅ **Allman Braces**: Opening bracket on new line (mandatory)
- ✅ **Single Object Constructor**: All constructors take one config parameter

## 🚫 What's Not Supported
- **`using` keyword**: Completely removed, use `uses` instead
- **`parallel` keyword**: Removed - use async/await for concurrency
- **K&R-style braces**: Must use Allman style
- **Multiple constructor parameters**: Use single config object
- **Quoted event names**: Use unquoted dot-notation
- **String methods**: `.toLowerCase()`, `.indexOf()`, `.substring()` - Use basic string operations and comparisons instead
- **JavaScript-style string methods**: CX uses basic string concatenation (`+`) and equality (`==`) only
