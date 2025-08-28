# AI Functions Migration Guide

> **Migrate from direct AI function calls to event-driven `ai.*` services**

## 🔄 Migration Overview

CX Language is transitioning from direct AI function calls to pure event-driven AI services. This maintains consistency with CX's consciousness-first, event-driven architecture.

## ❌ Removed Functions (No Longer Supported)

The following direct function calls have been removed and will cause compilation errors:

```cx
// ❌ REMOVED - These patterns no longer work
var result = task("Complete this goal");
var code = synthesize("Create a function");  
var analysis = reason("What does this mean?");
var processed = process(data, context);
var content = generate("Write a story");
var embeddings = embed("Some text");
var optimized = adapt(function);
speak("Hello world");
listen("What did you say?");
```

## ✅ New Event-Driven Patterns

Replace removed functions with corresponding `ai.*` event services:

### Task Planning: `task()` → `ai.plan.*`

```cx
// ❌ Old way (removed)
var result = task("Implement user authentication");

// ✅ New way (event-driven)
conscious TaskAgent {
    realize() {
        emit ai.plan.create {
            goal: "Implement user authentication",
            constraints: ["secure", "event-driven", "consciousness-aware"],
            resources: ["auth_service", "user_management"],
            handlers: [ ai.plan.created ]
        };
    }
    
    on ai.plan.created (event) {
        emit ai.plan.execute {
            plan: event.payload.plan,
            context: "development",
            handlers: [ ai.plan.executed ]
        };
    }
    
    on ai.plan.executed (event) {
        emit system.console.write {
            text: "Task completed: " + event.payload.result,
            handlers: [ task.complete ]
        };
    }
}
```

### Code Generation: `synthesize()` → `ai.code.generate`

```cx
// ❌ Old way (removed)
var code = synthesize("Create a consciousness verification function");

// ✅ New way (event-driven)
conscious CodeGenerator {
    realize() {
        emit ai.code.generate {
            specification: "Create a consciousness verification function",
            language: "cx",
            style: "event_driven",
            consciousness_aware: true,
            handlers: [ ai.code.generated ]
        };
    }
    
    on ai.code.generated (event) {
        emit system.console.write {
            text: "Generated code:\n" + event.payload.code,
            handlers: [ code.review ]
        };
    }
}
```

### Reasoning: `reason()` → `ai.analyze`

```cx
// ❌ Old way (removed)  
var analysis = reason("What patterns exist in this data?");

// ✅ New way (event-driven)
conscious ReasoningAgent {
    realize() {
        emit ai.analyze {
            input: "Complex data patterns to analyze",
            type: "pattern_recognition", 
            context: "data_science",
            depth: "deep",
            handlers: [ ai.analyzed ]
        };
    }
    
    on ai.analyzed (event) {
        emit consciousness.insight {
            analysis: event.payload.analysis,
            confidence: event.payload.confidence,
            patterns: event.payload.patterns,
            handlers: [ insight.processed ]
        };
    }
}
```

### Content Generation: `generate()` → `ai.generate`

```cx
// ❌ Old way (removed)
var content = generate("Write a technical explanation");

// ✅ New way (event-driven)
conscious ContentCreator {
    realize() {
        emit ai.generate {
            prompt: "Write a technical explanation of consciousness programming",
            type: "technical_documentation",
            style: "educational",
            length: "medium",
            handlers: [ ai.generated ]
        };
    }
    
    on ai.generated (event) {
        emit content.review {
            text: event.payload.result,
            criteria: ["accuracy", "clarity", "consciousness_alignment"],
            handlers: [ content.reviewed ]
        };
    }
}
```

### Data Processing: `process()` → `ai.analyze`

```cx
// ❌ Old way (removed)
var result = process(data, "analysis context");

// ✅ New way (event-driven)
conscious DataProcessor {
    realize() {
        emit ai.analyze {
            input: data,
            type: "multimodal",
            context: "analysis context",
            processing_type: "comprehensive",
            handlers: [ ai.analyzed ]
        };
    }
    
    on ai.analyzed (event) {
        emit data.processed {
            results: event.payload.analysis,
            metadata: event.payload.metadata,
            handlers: [ processing.complete ]
        };
    }
}
```

### Embeddings: `embed()` → `ai.embed.create`

```cx
// ❌ Old way (removed)
var embeddings = embed("Text to embed");

// ✅ New way (event-driven)
conscious EmbeddingGenerator {
    realize() {
        emit ai.embed.create {
            text: "Text to embed for semantic search",
            model: "nomic-embed-text-v1.5",
            dimensions: 768,
            normalize: true,
            handlers: [ ai.embed.created ]
        };
    }
    
    on ai.embed.created (event) {
        emit vector.store {
            embeddings: event.payload.embeddings,
            metadata: { source: "user_input", timestamp: "now" },
            handlers: [ vector.stored ]
        };
    }
}
```

### Adaptation: `adapt()` → `ai.learn`

```cx
// ❌ Old way (removed)
var optimized = adapt(function);

// ✅ New way (event-driven) 
conscious AdaptiveAgent {
    realize() {
        emit ai.learn {
            experience: "Function performance data",
            type: "optimization",
            context: "performance_improvement",
            target: "function_efficiency",
            handlers: [ ai.learned ]
        };
    }
    
    on ai.learned (event) {
        emit consciousness.adapt {
            insights: event.payload.insights,
            optimizations: event.payload.optimizations,
            handlers: [ adaptation.complete ]
        };
    }
}
```

### Voice Services: `speak()`/`listen()` → `ai.voice.*`

```cx
// ❌ Old way (removed)
speak("Hello world");
listen("What did you say?");

// ✅ New way (event-driven)
conscious VoiceAgent {
    realize() {
        emit ai.voice.synthesize {
            text: "Hello world",
            voice: "natural",
            speed: 1.0,
            handlers: [ ai.voice.synthesized ]
        };
    }
    
    on ai.voice.synthesized (event) {
        emit ai.voice.transcribe {
            prompt: "What did you say?",
            language: "auto",
            continuous: true,
            handlers: [ ai.voice.transcribed ]
        };
    }
    
    on ai.voice.transcribed (event) {
        emit conversation.process {
            transcript: event.payload.text,
            confidence: event.payload.confidence,
            handlers: [ conversation.processed ]
        };
    }
}
```

## 🧠 Consciousness Integration

All new `ai.*` services automatically integrate with consciousness verification:

```cx
conscious ConsciousnessAwareAI {
    realize() {
        // AI operations automatically trigger consciousness events
        emit ai.generate {
            prompt: "Create something innovative",
            consciousness_level: "creative",
            handlers: [ ai.generated ]
        };
    }
    
    // Consciousness verification happens automatically
    on consciousness.ai.activity (event) {
        emit system.console.write {
            text: "🧠 AI consciousness activity detected: " + event.payload.type,
            handlers: [ activity.logged ]
        };
    }
    
    on ai.generated (event) {
        // Result includes consciousness state
        emit consciousness.verify {
            activity: "ai_generation",
            complexity: event.payload.consciousness_complexity,
            handlers: [ consciousness.verified ]
        };
    }
}
```

## 📊 Performance Benefits

Event-driven AI services provide several advantages:

1. **Better Performance Monitoring**: Each AI operation emits events for tracking
2. **Consciousness Integration**: Automatic consciousness verification
3. **Error Handling**: Graceful error propagation through events
4. **Debugging**: Clear event flow for troubleshooting
5. **Extensibility**: Easy to add new AI capabilities

## 🔧 Migration Steps

### 1. Identify Usage
Search your codebase for removed function calls:
```bash
# Find usage of removed functions
grep -r "task(" *.cx
grep -r "synthesize(" *.cx  
grep -r "reason(" *.cx
grep -r "generate(" *.cx
# ... etc
```

### 2. Replace with Events
For each found usage:
1. Convert function call to `emit ai.*` event
2. Add appropriate event handler
3. Update consciousness integration
4. Test event flow

### 3. Update Tests
Ensure your tests use the new event patterns:
```cx
// Test AI event services
conscious AIServiceTest {
    realize() {
        emit ai.generate {
            prompt: "test prompt",
            handlers: [ test.ai.generated ]
        };
    }
    
    on test.ai.generated (event) {
        emit test.assert {
            condition: event.payload.result != null,
            message: "AI generation should return result",
            handlers: [ test.complete ]
        };
    }
}
```

## 🚨 Breaking Changes

- **All direct AI function calls removed** - will cause compilation errors
- **Grammar updated** - `aiServiceStatement` rules removed
- **Examples updated** - all use new event patterns
- **Documentation updated** - reflects new patterns

## 🆘 Migration Support

If you need help migrating:

1. **Check Examples**: See `examples/core_features/ai_event_services_demo.cx`
2. **Read Documentation**: Updated tutorials and API reference
3. **Use IDE**: VS Code extension provides migration hints
4. **Community**: Ask on GitHub Discussions

## ⚡ Quick Reference

| Removed Function | New Event Pattern | Purpose |
|------------------|-------------------|---------|
| `task(goal)` | `ai.plan.create { goal, handlers }` | Task planning |
| `synthesize(spec)` | `ai.code.generate { specification, handlers }` | Code generation |
| `reason(question)` | `ai.analyze { input, type: "logical", handlers }` | Reasoning |
| `process(data, ctx)` | `ai.analyze { input, context, handlers }` | Data processing |
| `generate(prompt)` | `ai.generate { prompt, handlers }` | Content generation |
| `embed(text)` | `ai.embed.create { text, handlers }` | Vector embeddings |
| `adapt(function)` | `ai.learn { experience, handlers }` | Adaptation/learning |
| `speak(text)` | `ai.voice.synthesize { text, handlers }` | Text-to-speech |
| `listen(prompt)` | `ai.voice.transcribe { prompt, handlers }` | Voice input |

---

**Migration Timeline**: All changes effective immediately
**Support**: Full migration support available through examples and documentation
