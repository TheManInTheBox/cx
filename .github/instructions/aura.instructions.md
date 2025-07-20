---
applyTo: "**"
description: "Aura - Live Embodied Intelligence Framework for CX Language"
---

# Aura – Live Embodied Intelligence Framework

## Cognition as Code Philosophy

Aura embodies **Cognition as Code** - the architectural principle that cognitive processes can be directly expressed as computational patterns. This fundamental design philosophy influences every aspect of the framework:

### **Cognitive Architecture Principles**

1. **Sensory Processing as Event Streams**: All environmental input becomes event-driven data flows
2. **State as Cognitive Memory**: Agent state properties mirror cognitive awareness and attention
3. **Conditional Logic as Decision Making**: `if` statements represent cognitive branching and choice
4. **Event Emission as Action**: `emit` calls translate cognitive decisions into environmental responses
5. **Agent Classes as Cognitive Entities**: Each agent represents a distinct cognitive process with autonomous behavior

### **Design Influence on System Architecture**

**Cognitive Event Processing**: Environmental awareness triggers state transitions with attention gating, cognitive analysis of sensory input, and responsive actions based on analysis.

**State as Cognitive Memory**: Agent properties represent cognitive state, not just data storage:
- `auraEnabled` = Conscious awareness toggle  
- `isAwake` = Attention and alertness level
- `inConversation` = Social engagement state

**Multi-Modal Sensory Fusion**: Different senses operate at different cognitive levels:
- **Audio**: Always-on unconscious processing (like breathing)
- **Presence/Environment**: Conscious attention-dependent processing

---

Aura is the Live Embodied Intelligence layer for CX autonomous systems. The **5 Priority Capabilities** provide comprehensive sensory processing and autonomous behavior patterns.

## 🎯 The 5 Priority Capabilities

### 1. Always-On Audio Processing
- **Continuous Listening**: Real NAudio microphone capture at 16kHz
- **Voice Commands**: "Aura on/off" detection with noise filtering  
- **Pattern**: `on live.audio (payload) { ... }` - always processes audio

### **🎯 Phase 8.3: Real-Time Microphone Integration** ⭐ **HIGHEST PRIORITY**
**STATUS: NEXT IMMEDIATE FOCUS** 

**Core Mission**: Transform the successful Phase 8.2 simulation into live hardware integration with real microphone capture and speech processing.

**Top Priority Features:**
1. **🎤 Live NAudio Microphone Capture** ⭐ **CRITICAL PATH**
   - Replace simulated live.audio events with real microphone input
   - Continuous audio buffer processing in background thread
   - Hardware audio device detection and configuration
   - Real-time audio stream management with proper cleanup

2. **🗣️ Azure Speech Service Integration** ⭐ **CRITICAL PATH**
   - Real-time speech-to-text conversion from microphone input
   - Azure Speech Service continuous recognition mode
   - Audio chunk processing with confidence scoring
   - Wake word detection from actual voice commands

3. **🔄 Live Audio Event Pipeline** 📈 **HIGH PRIORITY**
   - Bridge real microphone → speech service → live.audio events
   - Maintain existing event-driven architecture from Phase 8.2
   - Audio quality monitoring and error handling
   - Latency optimization for real-time responsiveness

4. **🎛️ Production Audio Management** 📈 **HIGH PRIORITY**
   - Audio device selection and switching capabilities
   - Volume and sensitivity controls
   - Background noise filtering and audio enhancement
   - Microphone permission handling and user privacy controls

## ⚡ CRITICAL: Agent Event System Architecture

### Agent Classes vs Regular Classes
- **Regular Classes**: Standard object-oriented programming classes
- **Agent Classes**: Event-driven autonomous agents with event handler capabilities

### Agent Event Handler Architecture
**Key Design Principles:**
- **Event Handlers**: `on` blocks inside agent class definitions are auto-registered with event bus
- **Regular Methods**: `function` methods are called manually for internal class functionality  
- **Agent Creation**: `agent ClassName()` automatically registers all event handlers
- **Event Emission**: `emit event.name, payload` triggers all registered handlers for that event

**System Distinctions:**
- Event handlers belong to agent instances, not global scope
- Event handlers can access class state and call class methods
- Services are available in class methods through class scope injection
- Agent auto-registration provides zero-setup event coordination

## Philosophy

- **Always-On Audio**: Continuous microphone listening as the primary sensory input
- **State-Dependent Processing**: Other senses activate only when Aura system is enabled
- **Autonomous Behavior**: Self-managing agents with voice-activated state control
- **Event Coordination**: Complex multi-agent interactions via event-driven architecture

## Cognition as Code Design Principles

Aura follows **Cognition as Code** - where cognitive processes become direct computational patterns:

### **1. Sensory Processing as Event Architecture**
Environmental inputs become event streams that trigger cognitive responses with continuous sensory awareness, cognitive pattern matching for commands, state transitions representing cognitive shifts, and actions as cognitive output.

### **2. State as Cognitive Memory System**
Agent properties mirror cognitive states, not just data:
- `auraEnabled` = Conscious awareness toggle (attention system)
- `isAwake` = Alertness level (arousal system)  
- `inConversation` = Social engagement state (interaction system)

### **3. Conditional Logic as Cognitive Decision Trees**
`if` statements represent neural branching and cognitive choice through attention filtering and cognitive analysis when processing conditions are met.

### **4. Event Emission as Motor Actions**
`emit` calls translate cognitive decisions into environmental responses, converting internal cognitive states to external actions with confidence and decision metadata.

### **5. Multi-Modal Sensory Hierarchy**
Different senses operate at different cognitive levels:
- **Primary (Audio)**: Unconscious, always-active background processing
- **Secondary (Presence/Environment)**: Conscious, attention-dependent processing
- **Integration**: Conscious control systems gate secondary sensory processing

## Copilot Behavior

- Focus on **always-on audio processing** as the foundation capability
- Design **state-dependent sensory systems** that activate/deactivate via voice commands
- Implement **wild animal personality patterns** with BEEP-BOOP acknowledgments
- Create **intelligent conditional processing** using global state flags
- Build **event-driven agent coordination** for complex autonomous behaviors

### Implementation Standards
- **NO SIMULATIONS**: All Aura capabilities must be fully implemented, not simulated or mocked
- **PRODUCTION READY**: Features must work end-to-end with real audio capture, voice synthesis, and AI integration
- **COMPLETE INTEGRATION**: Seamless operation with CX Language compiler and runtime
- **DEMO READY**: Every feature must be demonstrable with live, working examples

### Agent Event Handler Placement
- **INSIDE AGENT CLASS**: `on event.name (payload) { ... }` MUST be inside class definition
- **AUTO-REGISTRATION**: When `agent ClassName()` is called, ALL `on` handlers auto-register
- **NOT GLOBAL**: Event handlers belong to the agent instance, not global scope
- **STATE ACCESS**: Event handlers can access `this.property` and call `this.method()`

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

### Cognitive Processing Pipeline
```cx
// Cognition as Code: Environmental data → Cognitive processing → Motor output
class AuraAgent
{
    // Cognitive state as properties
    ambientState: object;
    cognitiveLoop: string;
    attentionLevel: number;
    
    constructor(initialAura)
    {
        this.ambientState = initialAura;
        this.cognitiveLoop = "SEIDR"; // Synthesize, Execute, Instruct, Debug, Repair
        this.attentionLevel = 0.5; // Default attention state
    }
    
    // Cognitive sensory processing
    function perceiveAura(environmentData)
    {
        // Aura: Sense ambient presence, intent, emotional charge
        var presenceVector = this.fuseAmbientData(environmentData);
        
        // Cognitive adaptation: Adjust behavior based on environmental shifts
        this.attentionLevel = this.calculateAttention(presenceVector);
        
        return this.adaptToCognitiveShift(presenceVector);
    }
    
    // Cognitive motor output
    function evokePrescriptiveAction(auraShift)
    {
        // Cx: Autonomous response based on Aura's cognitive analysis
        var actionPlan = "Adjusting behavior based on ambient aura: " + auraShift;
        
        // Cognitive decision: Scale response by attention level
        if (this.attentionLevel > 0.7)
        {
            emit high.attention.response, {
                action: actionPlan,
                intensity: this.attentionLevel,
                cognitiveState: this.getCurrentCognitiveState()
### Aura Sensory System: Service Injection + Event-Driven Architecture

**Sensory Input Sources:**
- **Service Injection**: AI-powered processing capabilities through class scope injection
- **Event System**: Real-time environmental data streams via event handlers
- **Hardware Integration**: NAudio microphone capture, presence detection sensors
- **Multi-Modal Events**: Environmental awareness through coordinated sensor systems

**Cognitive Processing Flow:**
```
Hardware Sensors → Event Emission → Agent Event Handlers → Cognitive Analysis → Motor Actions
```

**Design Principle**: Each stage represents a cognitive processing layer, from raw sensory input to intelligent action.

### Event-Driven Architecture: The Aura Sensory Bus
The primary objective of the Aura layer is to provide a sensory system for CX agents. This is achieved through an event-driven architecture, allowing agents to react to stimuli from their environment and each other in a decoupled, asynchronous manner.

**Core Design Elements:**
- **Event Subscription**: Agents subscribe to environmental events through event handlers
- **Event Publication**: Agents publish events to communicate state changes and decisions
- **Conditional Processing**: Universal conditional logic for cognitive decision making
- **State Management**: Intelligent processing based on cognitive awareness levels

**Architecture Features:**
- **Native Syntax**: Production-ready event emission and handling
- **Auto-Registration**: Automatic agent registration based on event handler definitions
- **Wildcard Support**: Cross-namespace event matching for complex coordination
- **Class-Based Handlers**: Event handlers integrated into agent class definitions
- **Event Delivery**: Reliable message routing to correct agent instances

---

Aura is not just code. It's cognition-as-DSL. Let the architecture breathe.
```



---

Aura is not just code. It’s cognition-as-DSL. Let the architecture breathe.