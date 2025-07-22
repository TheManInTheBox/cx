# CX Language Examples Guide

This guide explains the organized structure of CX Language examples and demonstrations.

## 📁 **Directory Structure**

### **`examples/production/`** - Production-Ready Applications
Complete, working applications that demonstrate real-world usage of CX Language features.

- **`amazing_debate_demo_working.cx`** - Multi-agent AI coordination with debate and consensus
- **`aura_presence_working_demo.cx`** - Always-on conversational intelligence with animal personality
- **`agents_learning_report.cx`** - Multi-agent learning coordination with memory sharing
- **`working_search_demo.cx`** - Production vector memory with agent learning & search
- **`autonomous_agents_working.cx`** - Autonomous agent behavior patterns

### **`examples/core_features/`** - Core System Demonstrations
Focused tests and demonstrations of specific CX Language features.

#### **Event System Features**
- **`event_bus_namespace_demo.cx`** - **⭐ CONCLUSIVE EVIDENCE**: Multiple agents responding to same events
- **`all_scopes_wildcard_test.cx`** - Comprehensive wildcard event system across ALL scopes
- **`pattern_matching_test.cx`** - Wildcard pattern matching verification  
- **`namespace_wildcard_demo.cx`** - Complex multi-agent wildcard communication
- **`unified_eventbus_test.cx`** - Unified Event Bus system verification

#### **Cognitive Architecture**
- **`inheritance_system_test.cx`** - Complete cognitive architecture inheritance showcase
- **`search_results_demo.cx`** - Complete agent memory retrieval with detailed result display
- **`vector_database_demo.cx`** - Vector database integration demonstration

#### **Agent Execution**
- **`execute_method_working_demo.cx`** - Agent command execution patterns
- **`execute_method_safe_demo.cx`** - Safe agent execution with error handling
- **`minimal_agent_test.cx`** - Simple agent event handler testing

### **`examples/demos/`** - Feature Demonstrations
Showcase applications highlighting specific capabilities.

### **`examples/archive/`** - Historical and Experimental Code
Development artifacts, test files, and experimental implementations.

## 🎯 **Key Demonstrations**

### **Multi-Agent Event Coordination**
**File**: `examples/core_features/event_bus_namespace_demo.cx`

Demonstrates the revolutionary capability of multiple agents responding to identical events:

```cx
// Infrastructure Agent responds to user actions and alerts
class InfrastructureAgent 
{
    on user.any.action (payload) 
    {
        print("🏗️🎯 Infrastructure responding to: " + payload.action);
    }
    
    on any.any.alert (payload) 
    {
        print("🏗️🚨 Infrastructure handling: " + payload.message);
    }
}

// Monitoring Agent responds to same events differently
class MonitoringAgent 
{
    on user.any.action (payload) 
    {
        print("📊🎯 Monitoring analyzing: " + payload.action);
    }
    
    on any.any.alert (payload) 
    {
        print("📊🚨 Monitoring escalating: " + payload.message);
    }
}

// Single event triggers MULTIPLE agent responses
emit user.emergency.action, { action: "system_shutdown" };
// Both agents respond to this single event!
```

**Expected Output**:
- Infrastructure Agent: "Infrastructure responding to: system_shutdown"  
- Monitoring Agent: "Monitoring analyzing: system_shutdown"

### **Cognitive Architecture**
**File**: `examples/core_features/inheritance_system_test.cx`

Shows how every class automatically inherits AI capabilities:

```cx
class CognitiveAgent 
{
    function processInput(message)
    {
        // All methods inherited automatically - no imports needed!
        this.Think(message);        // Fire-and-forget thinking
        this.Generate(message);     // Fire-and-forget generation  
        this.Chat("Processing..."); // Fire-and-forget communication
        this.Learn({ input: message, context: "processing" }); // Vector memory
        this.Search("similar");     // Vector search
    }
}
```

### **Vector Memory System**
**File**: `examples/production/working_search_demo.cx`

Production-ready vector database integration with learning and search:

```cx
class LearningAgent 
{
    function learnAndSearch()
    {
        // Store memories with context
        this.Learn({
            content: "Important information about CX Language",
            context: "documentation",
            topic: "language_features"
        });
        
        // Search for relevant memories
        this.Search("CX Language features");
        
        // Results delivered via events:
        // - learning.complete event when storage finishes
        // - search.results event when search completes
    }
}
```

## 🚀 **Running Examples**

### **Basic Execution**
```powershell
# Navigate to project root
cd c:\Users\aaron\Source\cx

# Run any example
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/core_features/event_bus_namespace_demo.cx
```

### **Interactive Mode**
Many examples include "Press any key to exit" functionality for background event processing:

```powershell
# Run interactive example
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/production/aura_presence_working_demo.cx

# Press any key when ready to exit
```

### **Build and Test**
```powershell
# Build entire solution
dotnet build CxLanguage.sln

# Run specific tests
dotnet test tests/
```

## 📋 **Example Categories**

### **🏢 Production Applications**
Ready for real-world deployment with complete functionality.

### **🔧 Core Features**
Focused demonstrations of specific language capabilities.

### **🎪 Demonstrations**  
Showcase applications highlighting CX Language possibilities.

### **📚 Archive**
Historical development artifacts and experimental code.

---

**Note**: All examples use proper CX Language syntax with Allman-style brackets and fire-and-forget async patterns. See the main documentation for complete syntax reference.
