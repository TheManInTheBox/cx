# CX Language - Cognitive Executor

```
  █████╗ ██╗   ██╗██████╗  █████╗     ██████╗██╗  ██╗
 ██╔══██╗██║   ██║██╔══██╗██╔══██╗   ██╔════╝╚██╗██╔╝
 ███████║██║   ██║██████╔╝███████║   ██║      ╚███╔╝ 
 ██╔══██║██║   ██║██╔══██╗██╔══██║   ██║      ██╔██╗ 
 ██║  ██║╚██████╔╝██║  ██║██║  ██║   ╚██████╗██╔╝ ██╗
 ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝    ╚═════╝╚═╝  ╚═╝
```

**The Cognitive Executor for Autonomous Programming**

*Built on the Aura Cognitive Architecture Framework*

**📋 Quick Start**: See [`QUICKSTART.md`](QUICKSTART.md) for essential setup  
**🎯 Current State**: See [`CHECKPOINT.md`](CHECKPOINT.md) for complete status

---

## 🎯 **Current Status: Phase 7 Complete - Live Embodied Intelligence**

**🏆 FLAGSHIP FEATURE: Aura Live Audio Presence System**

### 🌟 **PRIMARY RELEASE HIGHLIGHTS**
- **🎤 Always-On Audio Processing**: Continuous listening with "Aura on/off" command detection
- **🤖 Animal Personality Integration**: Full Muppet character with wild beep-boop acknowledgments  
- **🧠 Intelligent State Management**: Global sensory enable/disable with conditional processing
- **🎭 Multi-Modal Coordination**: Audio (always active) + presence/environment (state-dependent)
- **⚡ Event-Driven Architecture**: Complex agent interactions with autonomous behavior patterns

### ✅ **Core Platform Achievements**
- **Language Foundation**: JavaScript-like syntax, .NET 8 IL generation, ~50ms compilation
- **AI Integration**: 6 services operational (TextGen, TTS, DALL-E 3, Embeddings, Vector DB, Chat)
- **Autonomous Agents**: `agent` keyword creates AI agents with full object manipulation
- **Live Embodied Intelligence**: Real-time multi-modal sensory processing with AI personalities
- **Multi-Agent Coordination**: Voice-enabled AI agent debates operational

### 🚀 **Key Demos**
- **🌟 `aura_target_scenario_demo.cx`** - **FLAGSHIP: Live Audio Presence System**
- **`amazing_debate_demo_working.cx`** - Multi-agent AI coordination showcase
- **`proper_event_driven_demo.cx`** - Complete event-driven architecture  
- **`syntax_improvements_test.cx`** - All syntax improvements validated

**📋 See [`CHECKPOINT.md`](CHECKPOINT.md) for complete technical details**

## 🚀 **Getting Started**

### **Quick Setup**
```bash
# Clone and build
git clone https://github.com/ahebert-lt/cx.git
cd cx
dotnet build CxLanguage.sln

# Run flagship Aura demo
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/aura_target_scenario_demo.cx
```

### **AI Configuration** (Optional)
Create `appsettings.json`:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4.1-nano",
    "ApiKey": "your-key"
  }
}
```

**📚 Complete Guide**: [`INSTALLATION_GUIDE.md`](INSTALLATION_GUIDE.md)

## 💡 **Example Code**

```cx
// CX Language - Autonomous programming in action
using textGen from Cx.AI.TextGeneration;
using tts from Cx.AI.TextToSpeech;

class DebateAgent
{
    name: string;
    specialty: string;
    
    constructor(name, specialty)
    {
        this.name = name;
        this.specialty = specialty;
    }
    
    function argue(topic)
    {
        var argument = textGen.GenerateAsync(
            "Argue about " + topic + " from " + this.specialty + " perspective"
        );
        
        tts.SpeakAsync(this.name + " says: " + argument);
        return argument;
    }
}

// Create autonomous agents
var agent1 = agent DebateAgent("Dr. Chen", "climate science");
var agent2 = agent DebateAgent("Prof. Smith", "economics");

// Multi-agent coordination
var topic = "renewable energy transition";
var argument1 = agent1.argue(topic);
var argument2 = agent2.argue(topic);

print("🎭 Multi-agent debate complete!");
```

## 🎯 **What's Next: Phase 8**

- **🧠 Self-Modifying Agents**: Agents that learn and adapt behavior based on performance
- **� Distributed Networks**: Multi-agent coordination across processes and systems
- **�🎭 Advanced Multi-Modal**: Vision, document analysis, advanced reasoning integration
- **🔧 Production Deployment**: Cloud-ready autonomous agent services

**📋 Roadmap**: [`NEXT_STEPS.md`](NEXT_STEPS.md)

## 🏗️ **Architecture**

```
src/CxLanguage.CLI/           # Command-line interface
src/CxLanguage.Parser/        # ANTLR4 parser (Cx.g4)
src/CxLanguage.Compiler/      # IL code generation  
src/CxLanguage.StandardLibrary/ # AI service integrations
examples/                     # Sample CX programs
```

**🔧 Technologies**: ANTLR4, .NET 8, Semantic Kernel 1.26.0, Azure OpenAI

## 🤝 **Contributing**

```bash
git clone https://github.com/ahebert-lt/cx.git
cd cx
dotnet build CxLanguage.sln
dotnet test
```

**Standards**: Allman braces, comprehensive testing, documentation updates

---

**CX Language - Phase 7 Complete**  
*Live Embodied Intelligence Platform Ready* 🎊
