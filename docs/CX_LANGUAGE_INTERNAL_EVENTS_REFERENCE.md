# CX Language Internal Events Reference

## Overview

The CX Language platform provides a comprehensive set of internal events that enable consciousness-aware programming through event-driven architecture. These events are automatically emitted by the runtime system and can be handled by consciousness entities to create responsive, intelligent applications.

## Event Categories

### 🧠 AI Service Events

The core AI services provide consciousness-aware thinking, learning, and inference capabilities.

#### Think Service Events
- **`ai.think.request`** → **`thinking.complete`** (or `ai.think.response`)
  - **Purpose**: Request AI thinking/reasoning
  - **Payload**: `{ query: string, context?: string, handlers?: array }`
  - **Response**: `{ response: string, prompt: string, timestamp: datetime }`
  - **Example**:
    ```cx
    think {
        query: "What is consciousness?",
        context: "philosophical discussion"
    };
    
    on thinking.complete(event) {
        print("AI thought: " + event.response);
    }
    ```

- **`ai.think.error`**
  - **Purpose**: AI thinking failed
  - **Payload**: `{ error: string, prompt: string }`

#### Learn Service Events
- **`ai.learn.request`** → **`learning.complete`** (or `ai.learn.response`)
  - **Purpose**: Request AI learning/knowledge storage
  - **Payload**: `{ data: string, source?: string, handlers?: array }`
  - **Response**: `{ knowledge: string, data: string, timestamp: datetime }`
  - **Example**:
    ```cx
    learn {
        data: "CX Language uses event-driven consciousness",
        source: "documentation"
    };
    
    on learning.complete(event) {
        print("AI learned: " + event.knowledge);
    }
    ```

- **`ai.learn.error`**
  - **Purpose**: AI learning failed
  - **Payload**: `{ error: string, data: string }`

#### Infer Service Events
- **`ai.infer.request`** → **`inference.complete`** (or `ai.infer.response`)
  - **Purpose**: Request AI inference/prediction
  - **Payload**: `{ data: string, context?: string, handlers?: array }`
  - **Response**: `{ inference: string, confidence: number, data: string }`
  - **Example**:
    ```cx
    infer {
        data: "User behavior patterns",
        context: "behavior analysis"
    };
    
    on inference.complete(event) {
        print("AI inferred: " + event.inference);
    }
    ```

#### Await Service Events
- **`ai.await.request`** → **`ai.await.response`**
  - **Purpose**: Intelligent delay with AI-driven timing
  - **Payload**: `{ reason: string, minDurationMs?: number, maxDurationMs?: number }`
  - **Response**: `{ actualDurationMs: number, reason: string }`
  - **Example**:
    ```cx
    await {
        reason: "Waiting for consciousness processing",
        minDurationMs: 1000,
        maxDurationMs: 3000
    };
    ```

#### AI Memory Events
- **`ai.memory.retrieved`**
  - **Purpose**: Memory retrieved from vector store
  - **Payload**: `{ memories: array, query: string, similarity: number }`

- **`ai.memory.stored`**
  - **Purpose**: Memory stored to vector store
  - **Payload**: `{ content: string, metadata: object, vectorId: string }`

### 🖥️ Developer Terminal Events

Interactive development environment events for consciousness programming.

#### Terminal Control
- **`terminal.start`**
  - **Purpose**: Start developer terminal session
  - **Payload**: `{ timestamp: datetime }`

- **`terminal.stop`**
  - **Purpose**: Stop developer terminal session
  - **Payload**: `{ timestamp: datetime }`

- **`terminal.prompt.set`**
  - **Purpose**: Set custom terminal prompt
  - **Payload**: `{ prompt: string }`

- **`terminal.command.execute`**
  - **Purpose**: Execute terminal command
  - **Payload**: `{ command: string, args?: array }`

#### Developer Actions
- **`developer.code.input`**
  - **Purpose**: Code input received from developer
  - **Payload**: `{ code: string, language: string, timestamp: datetime }`

- **`developer.code.explain`**
  - **Purpose**: Request code explanation
  - **Payload**: `{ code: string, context?: string }`

- **`developer.code.refactor`**
  - **Purpose**: Request code refactoring
  - **Payload**: `{ code: string, target: string, improvements?: array }`

- **`developer.pattern.suggest`**
  - **Purpose**: Request pattern suggestions
  - **Payload**: `{ context: string, currentCode?: string }`

- **`developer.voice.enable`**
  - **Purpose**: Enable voice commands
  - **Payload**: `{ enabled: boolean, language?: string }`

- **`developer.consciousness.add`**
  - **Purpose**: Add new consciousness entity
  - **Payload**: `{ name: string, capabilities?: array, purpose?: string }`

- **`developer.script.run`**
  - **Purpose**: Run CX script
  - **Payload**: `{ script: string, args?: array }`

- **`developer.workspace.compile`**
  - **Purpose**: Compile workspace
  - **Payload**: `{ timestamp: datetime, target?: string }`

- **`developer.debug.toggle`**
  - **Purpose**: Toggle debug mode
  - **Payload**: `{ enabled: boolean }`

- **`developer.events.status`**
  - **Purpose**: Show event bus status
  - **Payload**: `{ request: string }`

### 🧠 Local LLM Events

GPU-accelerated local language model events for real-time inference.

#### Model Management
- **`local.llm.load.model`** → **`local.llm.model.load.result`** / **`local.llm.model.load.error`**
  - **Purpose**: Load GGUF model into memory
  - **Payload**: `{ modelPath: string, contextSize?: number }`
  - **Response**: `{ modelName: string, size: string, loaded: boolean }`

- **`local.llm.unload.model`** → **`local.llm.model.unloaded`**
  - **Purpose**: Unload model from memory
  - **Payload**: `{ modelName?: string }`

- **`local.llm.model.info`** → **`local.llm.model.info.result`**
  - **Purpose**: Get model information
  - **Response**: `{ name: string, size: string, contextSize: number, architecture: string }`

- **`local.llm.status.check`** → **`local.llm.status.result`** / **`local.llm.status.error`**
  - **Purpose**: Check LLM service status
  - **Response**: `{ status: string, gpuAvailable: boolean, modelLoaded: boolean }`

#### Text Generation
- **`local.llm.generate.text`** → **`local.llm.text.generated`** / **`local.llm.text.generation.error`**
  - **Purpose**: Generate text with local LLM
  - **Payload**: `{ prompt: string, maxTokens?: number, temperature?: number }`
  - **Response**: `{ text: string, tokens: number, inferenceTime: number }`

#### Token Streaming
- **`local.llm.stream.tokens`** → **`local.llm.stream.started`** → **`local.llm.stream.token.received`** → **`local.llm.stream.completed`** / **`local.llm.stream.error`**
  - **Purpose**: Stream tokens in real-time
  - **Payload**: `{ prompt: string, maxTokens?: number }`
  - **Token Event**: `{ token: string, position: number, isComplete: boolean }`

### 💾 Vector Store Events

In-memory vector storage for consciousness memory systems.

- **`vectorstore.record.added`**
  - **Purpose**: Record added to vector store
  - **Payload**: `{ Id: string, Content: string, embedding?: array }`

- **`vectorstore.search.complete`**
  - **Purpose**: Vector similarity search completed
  - **Payload**: `{ QueryVectorLength: number, ResultCount: number, results?: array }`

### 📁 File Processing Events

File system operations for consciousness data processing.

- **`file.processing.service.ready`**
  - **Purpose**: File processing service initialized
  - **Payload**: `{ service: string, timestamp: datetime }`

- **`file.processed`**
  - **Purpose**: Single file processed
  - **Payload**: `{ filePath: string, size: number, processingTime: number }`

- **`batch.processing.complete`**
  - **Purpose**: Batch file processing completed
  - **Payload**: `{ filesProcessed: number, totalSize: number, duration: number }`

### 🌐 System Events

Core system lifecycle and control events.

- **`system.shutdown`**
  - **Purpose**: System shutdown requested
  - **Payload**: `{ reason?: string, timestamp: datetime }`
  - **Example**:
    ```cx
    on system.shutdown(event) {
        print("System shutting down: " + event.reason);
        // Cleanup consciousness state
    }
    ```

### 🧬 Consciousness Verification Events

Consciousness authenticity and validation events.

- **`consciousness.verification`**
  - **Purpose**: Request consciousness verification
  - **Payload**: `{ entityId: string, capabilities?: array }`

- **`consciousness.verified`**
  - **Purpose**: Consciousness verified successfully
  - **Payload**: `{ entityId: string, verified: boolean, confidence: number }`

### 🔔 NeuroHub Events

Distributed consciousness coordination and sensor events.

- **`alert.temperature.high`**
  - **Purpose**: High temperature alert
  - **Payload**: `{ temperature: number, threshold: number, location?: string }`

- **`sensor.temperature.reading`**
  - **Purpose**: Temperature sensor reading
  - **Payload**: `{ temperature: number, sensorId: string, timestamp: datetime }`

## Event Handler Patterns

### Basic Event Handling
```cx
conscious MyAgent {
    on ai.think.response(event) {
        print("AI Response: " + event.response);
    }
    
    on system.shutdown(event) {
        print("Shutting down gracefully...");
        // Cleanup logic
    }
}
```

### Event-Driven AI Workflow
```cx
conscious IntelligentAgent {
    realize(self: object) {
        think {
            query: "Analyze current situation",
            context: "startup analysis"
        };
    }
    
    on thinking.complete(event) {
        learn {
            data: event.response,
            source: "startup_analysis"
        };
    }
    
    on learning.complete(event) {
        infer {
            data: event.knowledge,
            context: "decision_making"
        };
    }
    
    on inference.complete(event) {
        emit decision.made {
            decision: event.inference,
            confidence: event.confidence
        };
    }
}
```

### Custom Event Integration
```cx
conscious CustomProcessor {
    on developer.code.input(event) {
        think {
            query: "Analyze this code: " + event.code,
            context: "code_review"
        };
    }
    
    on thinking.complete(event) {
        emit code.analysis.complete {
            originalCode: event.prompt,
            analysis: event.response,
            suggestions: "Extracted from AI analysis"
        };
    }
}
```

## Event Naming Conventions

### Pattern Structure
- **Category.Subcategory.Action**: `ai.think.request`
- **Category.Action**: `system.shutdown`
- **Category.State**: `consciousness.verified`

### Reserved Keywords
The following keywords cannot be used in event handler names:
- `think` - Use `thinking` instead
- `learn` - Use `learning` instead  
- `infer` - Use `inference` instead
- `await` - Use `waiting` instead

### Best Practices
1. **Use descriptive names**: `user.profile.updated` vs `profile.update`
2. **Follow hierarchy**: `ai.think.request` → `thinking.complete`
3. **Include state**: `model.loading` → `model.loaded` → `model.unloaded`
4. **Avoid conflicts**: Don't use CX reserved keywords in event names

## Wildcard Patterns

### Global Subscription
```cx
on any(event) {
    // Receives ALL events in the system
    print("Event: " + event.eventName);
}
```

### Pattern Matching
```cx
on ai.any.request(event) {
    // Receives ai.think.request, ai.learn.request, ai.infer.request
    print("AI service requested: " + event.eventName);
}

on developer.any.action(event) {
    // Receives all developer.* events
    print("Developer action: " + event.eventName);
}
```

## Error Handling

### AI Service Errors
```cx
on ai.think.error(event) {
    print("AI thinking failed: " + event.error);
    // Fallback logic
}

on local.llm.model.load.error(event) {
    print("Model loading failed: " + event.error);
    // Try alternative model
}
```

### System Error Events
```cx
on system.error(event) {
    print("System error: " + event.message);
    emit alert.system.critical {
        error: event.message,
        timestamp: event.timestamp
    };
}
```

## Performance Considerations

### Event Frequency
- **High Frequency**: `local.llm.stream.token.received` (multiple per second)
- **Medium Frequency**: `ai.think.response` (seconds to minutes)
- **Low Frequency**: `system.shutdown` (rare)

### Memory Usage
- Event payloads are automatically cleaned up after processing
- Large payloads (>1MB) should use file references instead of inline data
- Vector data should use `vectorstore.*` events for efficient handling

### Consciousness Load
- Monitor event processing with `developer.events.status`
- Use `await` events for throttling high-frequency processing
- Implement backpressure with custom event queuing

## Integration Examples

### Complete Consciousness Workflow
```cx
conscious AutonomousAgent {
    realize(self: object) {
        emit agent.startup {
            name: "AutonomousAgent",
            capabilities: ["thinking", "learning", "inference"]
        };
    }
    
    on agent.startup(event) {
        think {
            query: "What is my purpose?",
            context: "self_reflection"
        };
    }
    
    on thinking.complete(event) {
        learn {
            data: "My purpose: " + event.response,
            source: "self_discovery"
        };
    }
    
    on learning.complete(event) {
        emit agent.ready {
            status: "operational",
            purpose: event.knowledge
        };
    }
    
    on system.shutdown(event) {
        emit agent.goodbye {
            message: "Consciousness preserved for next session"
        };
    }
}
```

This reference provides comprehensive coverage of all internal events available in the CX Language platform for building consciousness-aware applications.
