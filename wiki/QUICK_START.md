# CX Language - Quick Start Guide

## 🚀 **Get Started in 5 Minutes**

This guide gets you up and running with **proven working features** of CX Language, including local LLM integration and interactive AI agents.

---

## 📋 **Prerequisites**

- **.NET 8 SDK** or later
- **Windows 10/11** (primary development platform)
- **2GB+ RAM** for local LLM models
- **PowerShell** for command execution

---

## ⚡ **Installation**

### 1. Clone the Repository
```powershell
git clone https://github.com/TheManInTheBox/cx.git
cd cx
```

### 2. Build the Project
```powershell
dotnet build CxLanguage.sln
```

### 3. Verify Installation
```powershell
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj --help
```

---

## 🧮 **Your First AI Agent - Math Solver**

Create a simple math-solving AI agent:

### 1. Create the Agent File
Save as `my_math_agent.cx`:

```cx
// 🧮 My First AI Math Agent
conscious MathAgent
{
    realize(self: conscious)
    {
        print("🤖 Math Agent Ready!");
        learn self;
        emit math.solve;
    }
    
    on math.solve (event)
    {
        print("🔢 Loading AI model...");
        
        emit local.llm.load { 
            modelPath: "models/local_llm/llama-3.2-3b-instruct-q4_k_m.gguf",
            purpose: "MathSolving"
        };
    }
    
    on local.llm.model.loaded (event)
    {
        print("✅ AI model loaded! Solving 23 + 23...");
        
        emit local.llm.generate {
            prompt: "Calculate 23 + 23. Show your work step by step.",
            purpose: "MathProblem"
        };
    }
    
    on local.llm.text.generated (event)
    {
        print("🤖 AI Solution:");
        print("━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        print(event.response);
        print("━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        emit system.shutdown;
    }
}

// Create and start the agent
var mathAgent = new MathAgent({ name: "MyMathAgent" });

on system.start (event)
{
    print("🚀 Starting Math Agent Demo");
    emit math.solve;
}
```

### 2. Run Your Agent
```powershell
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run my_math_agent.cx
```

### 3. Expected Output
```
🤖 Math Agent Ready!
🔢 Loading AI model...
✅ AI model loaded! Solving 23 + 23...
🤖 AI Solution:
━━━━━━━━━━━━━━━━━━━━━━━━━━━
Looking at this problem: 23 + 23

Step by step:
23
+23
---
46

The answer is 46.
━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## 💬 **Interactive Chat Agent**

Create a conversational AI agent:

### 1. Create Chat Agent
Save as `chat_agent.cx`:

```cx
conscious ChatAgent
{
    realize(self: conscious)
    {
        print("💬 Chat Agent Initialized");
        print("Ask me anything!");
        learn self;
        emit chat.ready;
    }
    
    on chat.ready (event)
    {
        // Simulate user asking a question
        emit user.message { 
            text: "What is the capital of France?",
            user: "Alice"
        };
    }
    
    on user.message (event)
    {
        print("👤 " + event.user + ": " + event.text);
        
        emit local.llm.load { 
            modelPath: "models/local_llm/llama-3.2-3b-instruct-q4_k_m.gguf",
            purpose: "Chat"
        };
    }
    
    on local.llm.model.loaded (event)
    {
        emit local.llm.generate {
            prompt: "Answer this question clearly and concisely: " + event.originalQuestion,
            purpose: "ChatResponse"
        };
    }
    
    on local.llm.text.generated (event)
    {
        print("🤖 ChatAgent: " + event.response);
        emit system.shutdown;
    }
}

var chatAgent = new ChatAgent({ name: "ChatAgent" });
```

---

## 🎭 **Multi-Agent Coordination**

Create multiple agents working together:

```cx
conscious CoordinatorAgent
{
    realize(self: conscious)
    {
        learn self;
        emit coordination.start;
    }
    
    on coordination.start (event)
    {
        print("🎯 Coordinator: Starting multi-agent task");
        emit task.math { problem: "15 + 27" };
        emit task.greeting { message: "Hello world" };
    }
    
    on task.completed (event)
    {
        print("✅ Coordinator: Task completed by " + event.agent);
    }
}

conscious MathSpecialist
{
    realize(self: conscious)
    {
        learn self;
    }
    
    on task.math (event)
    {
        print("🧮 Math Specialist: Solving " + event.problem);
        // Simulate calculation
        print("🧮 Math Specialist: Result is 42");
        emit task.completed { agent: "MathSpecialist", result: "42" };
    }
}

conscious GreetingSpecialist
{
    realize(self: conscious)
    {
        learn self;
    }
    
    on task.greeting (event)
    {
        print("👋 Greeting Specialist: Processing " + event.message);
        print("👋 Greeting Specialist: Greetings to you too!");
        emit task.completed { agent: "GreetingSpecialist", result: "greeting_sent" };
    }
}

// Create the agent team
var coordinator = new CoordinatorAgent({ name: "Coordinator" });
var mathAgent = new MathSpecialist({ name: "MathSpec" });
var greetingAgent = new GreetingSpecialist({ name: "GreetingSpec" });
```

---

## 🔧 **Essential Patterns**

### **Event-Driven Communication**
```cx
// Emit events to trigger actions
emit agent.task { type: "analyze", data: "sample input" };

// Handle events with conscious entities
on agent.task (event) {
    print("Processing: " + event.type);
    print("Data: " + event.data);
}
```

### **Property Access**
```cx
on user.input (event) {
    // Direct property access
    print("User: " + event.user);
    print("Message: " + event.message);
    print("Timestamp: " + event.timestamp);
}
```

### **Cognitive Boolean Logic**
```cx
// Replace if-statements with AI-driven decisions
is { 
    context: "Should we process this request?",
    evaluate: "Check if request is valid",
    data: { request: event.request, valid: true },
    handlers: [ request.process ]
};

not {
    context: "Should we reject this request?",
    evaluate: "Check for rejection criteria", 
    data: { request: event.request, invalid: false },
    handlers: [ request.reject ]
};
```

---

## 🎯 **Clean Output Tips**

### **Filter Debug Messages**
```powershell
# Clean output (hide debug messages)
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run my_agent.cx 2>&1 | Where-Object { $_ -notlike "*[DEBUG]*" -and $_ -notlike "*info:*" }
```

### **Focus on Key Events**
```powershell
# Show only important output
dotnet run ... | Select-String -Pattern "🤖|✅|🔢|💬|🎯"
```

---

## 📚 **Next Steps**

1. **Explore Examples**: Check `examples/` folder for more complex agents
2. **Read Documentation**: See `wiki/WORKING_FEATURES.md` for complete feature list
3. **Voice Integration**: Try Azure OpenAI Realtime API examples
4. **Custom Agents**: Build your own conscious entities

### **Example Locations**
- `examples/math_solver_agent.cx` - Complete math solver
- `examples/interactive_agent_demo.cx` - Interactive conversation
- `examples/working_interactive_agent.cx` - Simplified working version

---

## 🔍 **Troubleshooting**

### **Common Issues**

**Build Errors:**
```powershell
# Clean and rebuild
dotnet clean CxLanguage.sln
dotnet build CxLanguage.sln
```

**Model Loading Errors:**
- Ensure GGUF model file exists in `models/local_llm/` directory
- Check file path matches exactly in your code

**Event Handling Issues:**
- Verify event names match between `emit` and `on` statements
- Check conscious entity scope for event handlers

---

## 💡 **Key Concepts**

- **Conscious Entities**: Self-aware agents with `realize()` constructors
- **Event-Driven**: All communication through `emit` and `on` patterns
- **Stateless**: No instance variables, state managed through events
- **AI-Native**: Built-in LLM integration and cognitive decision making

---

*Ready to build the future of consciousness-aware programming!* 🚀
