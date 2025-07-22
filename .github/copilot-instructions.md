# CX Language - Autonomous Programming Platform

## 🏗️ **ARCHITECTURE**

### **Core Components**
```
src/CxLanguage.CLI/           → Command-line interface + Azure OpenAI config
src/CxLanguage.Parser/        → ANTLR4 parser (grammar/Cx.g4 = source of truth)
src/CxLanguage.Compiler/      → IL generation with two-pass compilation
src/CxLanguage.StandardLibrary/ → 9 AI services via Semantic Kernel 1.26.0
examples/                     → All CX programs and demonstrations
```

### **Key Features**
- **Event-Driven**: `emit`/`on` syntax with auto-registration and namespace routing
- **AI Integration**: Streamlined cognitive architecture with inheritance-based intelligence
- **Personal Memory**: Each agent maintains private vector database for adaptive learning
- **Multi-Agent**: Voice-enabled AI agent coordination with individual memory systems
- **Inheritance-Based Cognition**: `this.Think()`, `this.Generate()`, `this.Chat()` built into all classes

## 💡 **SYNTAX GUIDE**

### **Core Rules**
```cx
// ✅ Console output - ALWAYS use print()
print("Hello World");

// ✅ Allman-style brackets - MANDATORY in CX Language
if (condition)
{
    doSomething();
}

// ❌ NEVER use K&R brackets in CX
if (condition) {  // ← ILLEGAL - will not compile
    doSomething();
}
```

### **Event System**
```cx
// Event handlers - available at both program scope AND in classes
on user.input (payload) { ... }        // ✅ CORRECT: Program scope event handler
on system.ready (payload) { ... }      // ✅ CORRECT: Program scope event handler

class MyAgent {
    on user.message (payload) { ... }  // ✅ CORRECT: Class scope event handler
    on ai.request (payload) { ... }    // ✅ CORRECT: Class scope event handler
}

// Event emission using reserved event names
emit user.message, { text: "hello" };
emit system.shutdown, { reason: "maintenance" };

// Wildcard event patterns for cross-namespace communication
on name.any.other.any.final (payload) { ... }     // Joins multiple namespaced event busses
on user.any.response (payload) { ... }             // Matches user.chat.response, user.voice.response, etc.
on system.any.ready (payload) { ... }              // Matches system.audio.ready, system.network.ready, etc.
on agent.any.thinking.any.complete (payload) { ... } // Complex wildcard pattern matching
```

### **Event Namespace Scoping**
**CRITICAL**: Event handlers are registered in **namespaced scopes**:
- **Class-level handlers**: Registered in the class namespace (e.g., `MyAgent.command.executed`)
- **Global handlers**: Registered in the global namespace
- **Instance events**: Events emitted from instance methods should target the instance's namespace
- **Namespace isolation**: Handlers in one namespace cannot receive events from other namespaces

```cx
class MyAgent {
    on command.executed (payload) {  // Registered in MyAgent namespace scope
        print("Command completed in MyAgent");
    }
    
    function runCommand() {
        this.Execute("some command");  // Emits to MyAgent namespace scope
    }
}

// Global scope handler
on system.ready (payload) {  // Registered in global namespace scope
    print("System ready globally");
}
```

### **ICxEventBus Interface Integration**
The `ICxEventBus` interface provides standardized event bus integration for Azure services and external systems:

```csharp
// ICxEventBus interface methods
public interface ICxEventBus
{
    Task EmitAsync(string eventName, object payload);
    void Subscribe(string eventName, Func<object, Task> handler);
    void Unsubscribe(string eventName);
}

// Usage in Azure OpenAI services
_eventBus.Subscribe("realtime.connect", async (payload) => {
    await _realtimeService.ConnectAsync();
});

await _eventBus.EmitAsync("realtime.session.started", new { sessionId = "abc123" });
```

**Key Integration Points:**
- **Service Provider Registration**: `services.AddSingleton<ICxEventBus, CxEventBus>()`
- **Azure OpenAI Integration**: Realtime API events bridge to CX event system
- **Runtime Bridge**: `CxRuntimeHelper.RegisterInstanceEventHandler()` uses ICxEventBus for AI service instances
- **Fallback Pattern**: Falls back to NamespacedEventBusRegistry when ICxEventBus unavailable

### **Critical Rules**
- **Event Handlers**: CANNOT be called directly - only invoked via `emit` statements
- **Event Handlers**: ALWAYS fire-and-forget - cannot return values, execute asynchronously
- **Event Namespace Scoping**: Handlers register in class/global namespaces, events must target correct scope
- **ICxEventBus Integration**: Azure services and external systems use ICxEventBus interface for standardized event integration
- **Multiple Event Bus Systems**: Runtime uses GlobalEventBus, NamespacedEventBusRegistry, EventBusServiceRegistry, and ICxEventBus depending on context
- **Cognitive Methods**: Available on all classes automatically via inheritance
- **Async Support**: `on async` syntax for real-time voice and AI processing

### **Agent Creation Patterns**
```cx
// ✅ All classes inherit cognitive capabilities automatically
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
```

### **⚠️ CRITICAL SYNTAX RULES**

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

**Key Syntax Restrictions**:
- **`uses` keyword**: ONLY allowed at program scope (top-level), NEVER inside classes
- **Service injection**: ILLEGAL - classes cannot declare their own service instances
- **Event handlers**: Available at both program scope AND in classes
- **Service access**: ONLY via inheritance-based cognitive methods (`this.Think()`, `this.Generate()`, etc.) OR global service instances
- **Class scope**: Fields, methods, constructors, event handlers only - NO `uses` statements or service declarations
- **Async Functions**: Return void - all results delivered via event bus system
- **No Await**: Completely eliminated from CX language for pure fire-and-forget pattern
```

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

**General Events:**
- `any` - Wildcard event matching
- `new` - Creation/initialization events
- `critical` - High-priority system events
- `assigned` - Assignment and delegation events

**Usage Examples:**
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

**Important Notes:**
- Reserved event names ensure consistent system behavior across all CX applications
- Custom event names can be used alongside reserved names
- Event names are case-sensitive and follow dot notation (namespace.action)
- Always use descriptive event names for maintainable code

### **Event-Driven Async Pattern**

CX Language introduces a **Event-Driven Async Pattern** - eliminating traditional await/async complexity in favor of pure fire-and-forget operations with event bus coordination:

#### **🎯 CRITICAL LANGUAGE RULE: Async Functions Return No Values**
```cx
// ❌ OLD WAY: Complex await patterns with blocking and return values
async function processData(input) {
    var result = await this.Think(input);    // Blocking operation
    var learned = await this.Learn(result);  // Sequential blocking
    return { result, learned };              // Return values create complexity
}

// ✅ NEW WAY: Pure fire-and-forget with event bus coordination
function processData(input) {
    // All async operations fire-and-forget - no return values, no blocking
    this.Think(input);     // Fires cognitive processing, results via events
    this.Learn(input);     // Fires learning process, results via events
    this.Generate(input);  // Fires generation, results via events
    
    // Immediate response, async results delivered via event bus
    emit processing.started, { input: input };
}

// Results flow through event system
on ai.thought.complete (payload) {
    print("Thinking complete: " + payload.result);
    // Continue processing based on thought results...
}

on ai.learning.complete (payload) {
    print("Learning complete: " + payload.documentId);
    // Update UI or trigger next steps...
}
```

#### **🎯 Key Event-Driven Async Rules:**
- **No await keyword**: Completely eliminated from CX language  
- **No return values**: Async functions return void, results via events
- **Fire-and-forget**: All cognitive operations non-blocking by default
- **Event coordination**: Results delivered through event bus system
- **Immediate responses**: Functions complete instantly, async work continues in background

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

## 🛠️ **DEVELOPMENT**
- Event names are case-sensitive and follow dot notation (namespace.action)
- Always use descriptive event names for maintainable code

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

### **🚀 REVOLUTIONARY EVENT-DRIVEN ASYNC PATTERN**

**CX Language** introduces the world's first **Event-Driven Async Pattern** - eliminating traditional await/async complexity in favor of pure fire-and-forget operations with event bus coordination:

#### **🎯 CRITICAL LANGUAGE RULE: Async Functions Return No Values**
```cx
// ❌ OLD WAY: Complex await patterns with blocking and return values
async function processData(input) {
    var result = await this.Think(input);    // Blocking operation
    var learned = await this.Learn(result);  // Sequential blocking
    return { result, learned };              // Return values create complexity
}

// ✅ NEW WAY: Pure fire-and-forget with event bus coordination
function processData(input) {
    // All async operations fire-and-forget - no return values, no blocking
    this.Think(input);     // Fires cognitive processing, results via events
    this.Learn(input);     // Fires learning process, results via events
    this.Generate(input);  // Fires generation, results via events
    
    // Immediate response, async results delivered via event bus
    emit processing.started, { input: input };
}

// Results flow through event system
on ai.thought.complete (payload) {
    print("Thinking complete: " + payload.result);
    // Continue processing based on thought results...
}

on ai.learning.complete (payload) {
    print("Learning complete: " + payload.documentId);
    // Update UI or trigger next steps...
}
```

#### **🎯 Key Event-Driven Async Rules:**
- **No await keyword**: Completely eliminated from CX language  
- **No return values**: Async functions return void, results via events
- **Fire-and-forget**: All cognitive operations non-blocking by default
- **Event coordination**: Results delivered through event bus system
- **Immediate responses**: Functions complete instantly, async work continues in background

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

### **⚠️ CRITICAL SYNTAX RULES**

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

**Key Syntax Restrictions**:
- **`uses` keyword**: ONLY allowed at program scope (top-level), NEVER inside classes
- **Service injection**: ILLEGAL - classes cannot declare their own service instances
- **Event handlers**: Available at both program scope AND in classes
- **Service access**: ONLY via inheritance-based cognitive methods (`this.Think()`, `this.Generate()`, etc.) OR global service instances
- **Class scope**: Fields, methods, constructors, event handlers only - NO `uses` statements or service declarations
- **Async Functions**: Return void - all results delivered via event bus system
- **No Await**: Completely eliminated from CX language for pure fire-and-forget pattern

---

## 🛠️ **DEVELOPMENT**

### **Repository Information**
- **GitHub**: [ahebert-lt/cx](https://github.com/ahebert-lt/cx)
- **Current Milestone**: [Azure OpenAI Realtime API v1.0](https://github.com/ahebert-lt/cx/milestone/4)

### **Quick Commands**
```powershell
# Build and test
dotnet build CxLanguage.sln
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/aura_presence_working_demo.cx

# GitHub workflow
gh auth switch --user ahebert-lt
gh issue list --repo ahebert-lt/cx --milestone "Azure OpenAI Realtime API v1.0"
```

### **CLI Features**
- **Interactive Mode**: Supports "Press any key to exit" for event-driven programs that wait for background events
- **Event System**: Full event emission and handler registration with namespace isolation
- **Real-time Processing**: Background event processing with user-controlled termination

### **Azure OpenAI Setup**
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4.1-nano",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ApiKey": "your-api-key",
    "ApiVersion": "2024-10-21"
  }
}
```

### **Event Bus Architecture**
The CX language implements a **Unified Event Bus System** that consolidates all event handling functionality:

**Production-Ready Implementation:**
- **`UnifiedEventBus`** (`src/CxLanguage.Runtime/UnifiedEventBus.cs`) - Single consolidated event bus with all capabilities
- **ICxEventBus Interface** (`src/CxLanguage.Core/Events/ICxEventBus.cs`) - Standardized interface for Azure and external integrations
- **`UnifiedEventBusRegistry.Instance`** - Global singleton for unified event bus access

**Key Features:**
- ✅ **Multiple Scoping Strategies**: Global, Agent, Channel, Role, Namespace scoping
- ✅ **Thread-Safe Operations**: Concurrent handling with comprehensive logging
- ✅ **Pattern Matching**: Wildcard and regex event pattern support
- ✅ **Azure Integration**: Direct compatibility with Azure OpenAI Realtime API
- ✅ **Statistics & Monitoring**: Real-time event bus statistics and performance tracking
- ✅ **Interactive CLI**: Background event processing with user-controlled termination

**Integration Points:**
- Event handlers register via `CxRuntimeHelper.RegisterEventHandler()` (global scope) and `CxRuntimeHelper.RegisterInstanceEventHandler()` (class scope)
- Compiler generates registration calls for CX `on` statements at both program and class levels
- Instance event handlers follow namespace scoping: class handlers registered in class namespace, events must target correct scope
- Azure OpenAI Realtime API integration via `ICxEventBus` interface with service provider DI container support

### **Development Standards**
- ✅ **Production-Ready**: Complete end-to-end implementations with working unified event system, namespace scoping, and interactive CLI
- ✅ **Real Implementation**: Actual event emission/handling, namespace isolation, press-key-to-exit functionality
- ✅ **Complete CX Integration**: Seamless compiler and runtime operation with zero exceptions
- ✅ **Event-Driven Architecture**: Fully operational unified event system with proper handler registration
- ✅ **Interactive CLI**: Working background event processing with user-controlled termination
- ✅ **Clean Examples**: Organized structure with production/core_features/demos/archive folders
- ❌ **NOT Acceptable**: Simulations of any kind, mocks, partial implementations, placeholder code, POCs

### **File Organization**
- **Production applications**: `examples/production/` directory
- **Core feature tests**: `examples/core_features/` directory  
- **Feature demonstrations**: `examples/demos/` directory
- **Static documentation**: `wiki/` directory (timeless reference material only)
- **Archived examples**: `examples/archive/` directory (historical and experimental code)

## 🏆 **KEY DEMONSTRATIONS**
- **🧠 `examples/production/working_search_demo.cx`** → Production vector memory with agent learning & search
- **📚 `examples/core_features/search_results_demo.cx`** → Complete agent memory retrieval with detailed result display
- **🎯 `examples/production/agents_learning_report.cx`** → Multi-agent learning coordination with memory sharing
- **✅ `examples/core_features/inheritance_system_test.cx`** → Complete cognitive architecture inheritance showcase
- **🎪 `examples/production/amazing_debate_demo_working.cx`** → Multi-Agent AI Coordination with vector memory
- **⚡ `examples/core_features/unified_eventbus_test.cx`** → Unified Event Bus system verification
- **🌟 `examples/production/aura_presence_working_demo.cx`** → Always-On Conversational Intelligence with Animal personality

### **Examples Organization**
- **`examples/production/`** - Production-ready applications demonstrating complete functionality
- **`examples/core_features/`** - Core system demonstrations and feature tests
- **`examples/demos/`** - Feature demonstrations and showcase applications  
- **`examples/archive/`** - Historical examples and experimental code


