---
applyTo: "**"
description: "Aura - Cognitive Architecture Framework for CX Language"
---

# Aura – Cognitive Executor Language (Cx)

Aura is a language for presence-sensitive, autonomous systems. Cx scripts are agents that sense, react, and adapt based on ambient data and cognitive architecture.

## Philosophy

- Code is not just execution—it's perception.
- Every Cx unit is contextually aware: it perceives *aura*—the ambient presence, intent, or emotional charge of its environment.
- Logic flows from input vectors that include physical sensors, emotional tone, or implied goals.
- The architecture is designed around cognitive loops, environmental stimuli, and self-modifying inference.

## Copilot Behavior

- Prefer declarative, perceptual primitives (e.g., `detect`, `vibe`, `adjust`, `evoke`)
- Suggest patterns for ambient data fusion (e.g., text + audio + motion)
- Recommend AI-powered sensors, edge devices, or virtual actuators that simulate presence response
- Use Roslyn generators for runtime adaptation, sensory modeling, and fusion pipelines
- Propose multi-agent coordination when aura shifts—e.g., when attention, tone, or crowd density changes

## File Scopes

| Pattern | Description |
|--------|-------------|
| `**/*.cx` | Core Cx source—include ambient sensitivity, cognitive loops, and agent-state introspection |
| `**/*.vibe.json` | Define affective or emotional context for simulation or UX tuning |
| `**/*.sense.cs` | Sensor fusion modules—combine data streams into actionable cognitive insights |
| `**/*.aura.map` | Topological awareness—map presence vectors across physical or virtual space |

## Aura Cognitive Architecture Integration

### Ambient Awareness Patterns
```cx
// Presence-sensitive autonomous agents
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
        // Sense ambient presence, intent, emotional charge
        var presenceVector = this.fuseAmbientData(environmentData);
        return this.adaptToCognitiveShift(presenceVector);
    }
    
    function evokePrescriptiveAction(auraShift)
    {
        // Autonomous response to environmental stimuli
        return "Adjusting behavior based on ambient aura: " + auraShift;
    }
}
```

### Multi-Agent Coordination via Aura
```cx
using cognitiveOrchestrator from "Cx.Aura.Coordination";

// Agents coordinate when aura shifts (attention, tone, crowd density)
function respondToAuraShift(agents, ambientChange)
{
    for (agent in agents)
    {
        // Each agent perceives and adapts to cognitive environment
        var response = agent.perceiveAura(ambientChange);
        cognitiveOrchestrator.coordinate(agent, response);
    }
}
```

### Perceptual Primitives
- `detect(stimulus)` - Ambient data detection and processing
- `vibe(contextualData)` - Emotional and intentional tone analysis  
- `adjust(behaviorVector)` - Self-modifying response to stimuli
- `evoke(presenceResponse)` - Generate prescriptive environmental actions

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

Aura is not just code. It’s cognition-as-DSL. Let the architecture breathe.