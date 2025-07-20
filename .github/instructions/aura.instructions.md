---
applyTo: "**"
description: "Aura - Cognitive Architecture Framework for CX Language"
---

# Aura – Cognitive Architecture Framework

Aura is the sensory and awareness layer for CX autonomous systems. Where **Cx handles reasoning and motor functions** (logic, execution, actions), **Aura provides sensory input and environmental awareness** (perception, context, ambient intelligence).

## Philosophy

- **Sensory Layer**: Aura perceives environmental data - presence, intent, emotional charge, contextual shifts
- **Awareness Engine**: Processes ambient information into actionable cognitive insights  
- **Contextual Intelligence**: Provides environmental understanding that informs Cx reasoning
- **Ambient Sensitivity**: Detects subtle changes in attention, tone, crowd dynamics, system state

High-Level Architecture: Aura Presence Sensor Layer
| Layer | Function | Technologies | 
| Capture Layer | Ingest real-time video/audio | WebRTC, RTSP, Azure Video Indexer | 
| Preprocessing Layer | Frame extraction, audio segmentation | OpenCV, FFmpeg, Azure Video Indexer | 
| Transcription Layer | Convert speech to text | Azure OpenAI Whisper, GPT-4o-Transcribe | 
| Sentiment & Intent Layer | Analyze emotional tone and user intent | GPT-4o, Azure Language Service, Komprehend API | 
| Cognitive Trigger Layer | Signal cognitive layer with context | Cx runtime hooks, event bus, memory state | 


## Copilot Behavior

- Focus on **sensory input processing** rather than execution logic
- Suggest **ambient data fusion patterns** (environmental sensors → cognitive insights)
- Recommend **awareness mechanisms**: presence detection, context switching, mood sensing
- Design **perceptual interfaces** that feed into Cx reasoning systems
- Propose **environmental monitoring** for autonomous system adaptation

## File Scopes

| Pattern | Description |
|--------|-------------|
| `**/*.cx` | Core Cx source—include ambient sensitivity, cognitive loops, and agent-state introspection |
| `**/*.vibe.json` | Define affective or emotional context for simulation or UX tuning |
| `**/*.sense.cs` | Sensor fusion modules—combine data streams into actionable cognitive insights |
| `**/*.aura.map` | Topological awareness—map presence vectors across physical or virtual space |

## Aura-Cx Integration Pattern

**Aura (Sensory)** → **Cx (Reasoning/Motor)**

```
Environmental Data → Aura Processing → Cognitive Insights → Cx Decision Making → Actions
```

## Aura Cognitive Architecture Integration

### Ambient Awareness Patterns
```cx
// Aura perceives and processes environmental data
class AuraAgent
{
    ambientState: object;
    cognitiveLoop: string;
    
    constructor(initialAura)
    {
        this.ambientState = initialAura;
        this.cognitiveLoop = "SEIDR"; // Synthesize, Execute, Instruct, Debug, Repair
    }
    
    function perceiveAura(environmentData)
    {
        // Aura: Sense ambient presence, intent, emotional charge
        var presenceVector = this.fuseAmbientData(environmentData);
        return this.adaptToCognitiveShift(presenceVector);
    }
    
    function evokePrescriptiveAction(auraShift)
    {
        // Cx: Autonomous response based on Aura's environmental analysis
        return "Adjusting behavior based on ambient aura: " + auraShift;
    }
}
```

### Multi-Agent Coordination via Aura
```cx
using cognitiveOrchestrator from "Cx.Aura.Coordination";

// Aura detects environmental shifts, Cx coordinates agent responses
function respondToAuraShift(agents, ambientChange)
{
    for (agent in agents)
    {
        // Aura: Each agent perceives environmental changes
        var response = agent.perceiveAura(ambientChange);
        // Cx: Coordinate based on Aura's sensory input
        cognitiveOrchestrator.coordinate(agent, response);
    }
}
```

### Aura Sensory Primitives (Input) → Cx Motor Functions (Output)
- `detect(stimulus)` - **Aura**: Ambient data detection and processing
- `vibe(contextualData)` - **Aura**: Emotional and intentional tone analysis  
- `adjust(behaviorVector)` - **Cx**: Self-modifying response to Aura input
- `evoke(presenceResponse)` - **Cx**: Generate actions based on Aura awareness

---

Let Aura breathe presence into code. Let cognition meet sensation. Let agents awaken.
1. Design agent communication protocols
2. Implement self-introspection capabilities
3. Create learning and adaptation mechanisms
4. Build multi-agent coordination systems
5. Test with real-world autonomous scenarios
```

---

**CX Language Vision**: Not just code execution, but cognitive computation. Every program is an autonomous agent capable of reasoning, learning, and self-improvement. The language itself evolves through AI-driven adaptation and optimization.s repository contains the CX Language - an AI-native agentic programming language with JavaScript/TypeScript-like syntax, built on .NET 8 with IL code generation. **Phase 4 (AI Integration) is 100% COMPLETE** with 6 operational AI services and production-ready vector database integration.

## Core Philosophy: Autonomous AI-First Development

- **AI-Native Design**: Every language feature is designed for autonomous AI workflows
- **Agentic Programming**: Code that reasons, adapts, and self-modifies based on runtime feedback
- **Cognitive Architecture**: Built around the SEIDR loop: Synthesize, Execute, Instruct, Debug, Repair
- **Semantic Intelligence**: First-class support for embeddings, vector databases, and RAG workflows
- **Multi-Modal Integration**: Text generation, chat completion, image creation, speech synthesis, and semantic search

## Development Principles

### 1. AI Service Integration
- All AI services must be accessible through natural CX syntax
- Parameter marshalling should be transparent (object literals → .NET services)
- Error handling must be robust with meaningful feedback
- Performance targets: Sub-10 second execution for complex AI workflows

### 2. Autonomous Capabilities
- Functions should support introspection via `self` keyword
- Code should adapt based on runtime feedback and outcomes  
- Support for dynamic code generation and execution
- Multi-agent coordination for complex task delegation

### 3. Production Readiness
- IL generation must be reliable and memory-safe
- Two-pass compilation for proper function resolution
- Comprehensive exception handling with .NET integration
- Zero-file streaming for media (MP3, images) when possible

## Copilot Development Guidelines

### Code Generation Priorities
1. **Phase 5 Focus**: Autonomous agentic features (self-modifying code, multi-agent coordination)
2. **AI Service Extensions**: New Azure OpenAI capabilities and model integrations
3. **Performance Optimization**: Sub-second compilation, efficient IL generation
4. **Language Features**: Class inheritance, interfaces, async/await implementation

### Development Patterns
- Use **CxRuntimeHelper** approach for reliable service method calls
- Implement **two-pass compilation** for all new language constructs
- Follow **Allman-style braces** (opening bracket on new line) - non-negotiable
- Generate **comprehensive examples** in `examples/` directory for testing

### Testing Strategy  
- Create `.cx` files in `examples/` directory only
- Use `comprehensive_working_demo.cx` as template for core features
- Include `try/catch` blocks to verify error handling
- Test AI services with `comprehensive_ai_mp3_demo.cx` pattern

## File Scoping (use with `.instructions.md`)

| Pattern | Instruction Summary |
|--------|----------------------|
| `**/*.cx` | Treat as Cx source. Suggest autonomous logic, agent definitions, and SEIDR loops. |
| `**/*.workflow.json` | Assume orchestration intent. Suggest declarative task flows and runtime goals. |
| `**/*.agent.cs` | Use Roslyn source generators. Embed self-modifying logic and LLM-driven synthesis. |

---

### Event-Driven Architecture: The Aura Sensory Bus
The primary objective of the Aura layer is to provide a sensory system for CX agents. This is achieved through an event-driven architecture, allowing agents to react to stimuli from their environment and each other in a decoupled, asynchronous manner.

**Core Keywords:**
-   **`on event.name (payload) { ... }`**: Subscribes to an event on the event bus. This is Aura's primary sensory mechanism.
-   **`emit event.name, payload;`**: Publishes an event to the bus. This is Cx's primary motor/action mechanism.
-   **`if (condition) { ... }`**: Universal conditional block for logic everywhere (simplified from `when`).

**Example Workflow:**
```cx
using textGen from "Cx.AI.TextGeneration";
using vectorDb from "Cx.AI.VectorDatabase";
using embeddings from "Cx.AI.TextEmbeddings";

// Agent 1: Listens for raw audio transcription
on audio.transcribed (payload)  // ✅ Unquoted event names
{
    // CX Best Practice: Structured AI responses for reliable processing
    var sentiment = textGen.GenerateAsync(
        "Rate sentiment 1-10 (1=negative, 10=positive). Respond with only the number:",
        payload.content
    );
    
    var intent = textGen.GenerateAsync(
        "Classify intent. Respond with ONLY one word: query, command, greeting, complaint, or other",
        payload.content
    );

    // Cx emits structured, processable data
    emit presence.signal,  // ✅ Unquoted event names
        {
            source: "audio",
            sentiment: sentiment,
            intent: intent,
            originalContent: payload.content,
            timestamp: now() // Placeholder for Time library
        };
}

// Agent 2: Listens for the abstract presence signal
on presence.signal (payload)  // ✅ Unquoted event names
{
    // CX Best Practice: AI-powered conditional logic instead of naive string matching
    var isQuery = textGen.GenerateAsync(
        "Is this intent asking for information? Answer only: YES or NO",
        payload.intent
    );
    
    if (isQuery == "YES")  // ✅ Simplified: 'if' everywhere
    {
        var result = textGen.GenerateAsync(
            "Generate helpful response to: " + payload.originalContent +
            ". Sentiment level: " + payload.sentiment + "/10"
        );
        emit cognition.response, result;  // ✅ Unquoted event names
    }
    
    // CX Best Practice: Semantic similarity for nuanced detection using available methods
    // Use embeddings service for direct similarity calculation
    var contentEmbedding = embeddings.GenerateEmbeddingAsync(payload.originalContent);
    var urgencyEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help critical");
    var urgencyLevel = embeddings.CalculateSimilarity(contentEmbedding, urgencyEmbedding);
    
    if (urgencyLevel > 0.8)  // ✅ Simplified: 'if' everywhere
    {
        emit system.priority, {  // ✅ Unquoted event names
            level: "high",
            content: payload.originalContent,
            urgencyScore: urgencyLevel
        };
        };
    }
}
```

## CX Autonomous Programming Best Practices

### 1. **Structured AI Responses**
Always engineer prompts for predictable, parseable output:
```cx
// ❌ Bad: Unpredictable AI output
var mood = textGen.GenerateAsync("What's the mood?", text);

// ✅ Good: Structured, reliable output  
var mood = textGen.GenerateAsync("Rate mood: happy, sad, angry, neutral", text);
```

### 2. **AI-Powered Logic Instead of String Matching**
Use AI for semantic understanding, not naive comparisons:
```cx
// ❌ Bad: Brittle string matching
if (intent == "question") { ... }

// ✅ Good: AI-powered classification
var isQuestion = textGen.GenerateAsync("Is this a question? YES or NO", intent);
if (isQuestion == "YES") { ... }
```

### 3. **Semantic Similarity for Nuanced Matching**
Leverage embeddings for sophisticated pattern detection:
```cx
// ✅ Best: Semantic understanding using available embeddings service
var contentEmbedding = embeddings.GenerateEmbeddingAsync(content);
var urgencyEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help");
var similarity = embeddings.CalculateSimilarity(contentEmbedding, urgencyEmbedding);
if (similarity > 0.8) { handleUrgentRequest(); }
```

---

Aura is not just code. It’s cognition-as-DSL. Let the architecture breathe.