# Event-Driven Architecture

## Overview
Cx's event-driven architecture, powered by the **Aura cognitive framework**, enables autonomous systems to perceive, process, and respond to environmental stimuli in real-time. This architecture forms the foundation for building reactive, autonomous agents that can adapt and coordinate with each other.

## Core Keywords

### `on` - Event Receiver Functions
**Syntax**: `on "event.name" (payload) { ... }`  
**Purpose**: Defines an event receiver function that listens for specific events on the event bus. These can be defined globally or within class instances.
**Scope**: Global or class instance level

### `when` - Conditional Logic (Event Handlers Only)
**Syntax**: `when (condition) { ... }`  
**Purpose**: Declarative conditional blocks within event handlers for autonomous decision-making
**Scope**: **ONLY usable inside `on` event receiver blocks**

### `emit` - Event Emission
**Syntax**: `emit "event.name", payload;`  
**Purpose**: Publish events to the event bus for other agents to process
**Scope**: **Globally available** - can be used anywhere in the code (functions, classes, event handlers, standalone code)

### `if` - Regular Conditional Logic
**Syntax**: `if (condition) { ... }`  
**Purpose**: Standard conditional logic for functions and general code
**Scope**: **Used everywhere EXCEPT inside `on` blocks** (where `when` is preferred)

**Global Event Receivers:**
```cx
// Global event subscription - responds system-wide
on "user.message" (payload)
{
    print("Received message: " + payload.content);
}
```

**Class Instance Event Receivers:**
```cx
class AutonomousAgent
{
    name: string;
    
    constructor(agentName)
    {
        this.name = agentName;
    }
    
    // Instance-level event receiver
    on "task.assigned" (payload)
    {
        when (payload.assignedTo == this.name)
        {
            emit "task.accepted", { 
                agent: this.name, 
                taskId: payload.taskId 
            };
        }
    }
    
    // Another instance event receiver
    on "priority.urgent" (payload)
    {
        // This agent instance handles urgent events
        emit "agent.responding", { 
            agent: this.name, 
            urgentTask: payload 
        };
    }
}
```

// Multiple event handlers for the same event
on "system.startup" (payload)
{
    print("System starting up...");
}

on "system.startup" (payload)
{
    // Initialize autonomous agents
    emit "agents.initialize", { timestamp: now() };
}
```

### Conditional Logic: `when`
Declarative conditional blocks within event handlers for autonomous decision-making.

**Syntax**: `when (condition) { ... }`

```cx
on "sensor.reading" (payload)
{
    when (payload.temperature > 85.0)
    {
        emit "alert.overheating", { 
            temperature: payload.temperature,
            location: payload.sensor_id 
        };
    }
    
    when (payload.humidity > 0.8)
    {
        emit "alert.high-humidity", {
            humidity: payload.humidity,
            temperature: payload.temperature
        };
    }
}
```

### Event Emission: `emit`
Publish events to the bus for other agents to process. This is Cx's primary motor/action mechanism.

**Syntax**: `emit "event.name", payload;`

```cx
// Simple event emission
emit "notification.sent", { recipient: "admin", message: "System ready" };

// Complex payload
emit "user.profile.updated", {
    userId: 12345,
    changes: ["email", "preferences"],
    timestamp: now(),
    source: "autonomous-agent"
};
```

## Autonomous Agent Patterns

### Basic Sensory Agent
```cx
using textGen from "Cx.AI.TextGeneration";

// Agent perceives user input and classifies intent
on "user.input" (payload)
{
    // Structured AI response for reliable processing
    var intent = textGen.GenerateAsync(
        "Classify intent as: question, command, greeting, complaint, other. One word only:",
        payload.message
    );
    
    var sentiment = textGen.GenerateAsync(
        "Rate sentiment 1-10 (1=negative, 10=positive). Number only:",
        payload.message
    );
    
    // Emit structured perception data
    emit "perception.classified", {
        originalMessage: payload.message,
        intent: intent.toLowerCase(),
        sentiment: parseFloat(sentiment),
        userId: payload.userId,
        timestamp: now()
    };
}
```

### Multi-Stage Processing Pipeline
```cx
using embeddings from "Cx.AI.TextEmbeddings";
using textGen from "Cx.AI.TextGeneration";

// Stage 1: Raw input processing
on "input.raw" (payload)
{
    // Basic classification and normalization
    var cleaned = payload.content.toLowerCase().trim();
    var intent = textGen.GenerateAsync("Classify intent:", cleaned);
    
    emit "input.preprocessed", {
        original: payload.content,
        cleaned: cleaned,
        intent: intent,
        source: payload.source
    };
}

// Stage 2: Semantic analysis
on "input.preprocessed" (payload)
{
    // Generate embeddings for semantic matching
    var inputEmbedding = embeddings.GenerateEmbeddingAsync(payload.cleaned);
    
    // Check similarity to known patterns
    var urgentEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help critical");
    var technicalEmbedding = embeddings.GenerateEmbeddingAsync("programming code software bug");
    
    var urgencyScore = embeddings.CalculateSimilarity(inputEmbedding, urgentEmbedding);
    var technicalScore = embeddings.CalculateSimilarity(inputEmbedding, technicalEmbedding);
    
    emit "input.analyzed", {
        content: payload.cleaned,
        intent: payload.intent,
        urgencyScore: urgencyScore,
        technicalScore: technicalScore,
        isUrgent: urgencyScore > 0.8,
        isTechnical: technicalScore > 0.75
    };
}

// Stage 3: Decision making and routing
on "input.analyzed" (payload)
{
    when (payload.isUrgent)
    {
        emit "escalation.urgent", {
            originalContent: payload.content,
            urgencyScore: payload.urgencyScore,
            reason: "High semantic similarity to urgent patterns"
        };
    }
    
    when (payload.isTechnical)
    {
        emit "route.technical", {
            content: payload.content,
            confidence: payload.technicalScore,
            intent: payload.intent
        };
    }
    
    when (!payload.isUrgent && !payload.isTechnical)
    {
        emit "route.general", {
            content: payload.content,
            intent: payload.intent
        };
    }
}
```

## Advanced Event Patterns

### Agent Communication and Coordination
```cx
using textGen from "Cx.AI.TextGeneration";

class AutonomousAgent
{
    id: string;
    role: string;
    
    constructor(agentId, agentRole)
    {
        this.id = agentId;
        this.role = agentRole;
    }
    
    function initialize()
    {
        // Register agent with coordination system
        emit "agent.registered", {
            id: this.id,
            role: this.role,
            capabilities: ["text-analysis", "decision-making"],
            status: "ready"
        };
    }
    
    function processTask(task)
    {
        var result = textGen.GenerateAsync(
            "As a " + this.role + ", analyze and respond to: " + task.description
        );
        
        return {
            agentId: this.id,
            taskId: task.id,
            result: result,
            confidence: this.assessConfidence(result)
        };
    }
    
    function assessConfidence(result)
    {
        var confidence = textGen.GenerateAsync(
            "Rate your confidence in this analysis 1-10. Number only:",
            result
        );
        return parseFloat(confidence);
    }
}

// Agent coordination system
var agents = [
    new AutonomousAgent("agent-1", "technical specialist"),
    new AutonomousAgent("agent-2", "user experience analyst"),
    new AutonomousAgent("agent-3", "business analyst")
];

// Initialize all agents
for (agent in agents)
{
    agent.initialize();
}

// Task distribution
on "task.incoming" (payload)
{
    // Determine best agent for task
    var taskType = textGen.GenerateAsync(
        "Categorize this task: technical, user-experience, business, or general. One word:",
        payload.description
    );
    
    when (taskType.contains("technical"))
    {
        emit "task.assigned", {
            taskId: payload.id,
            assignedTo: "agent-1",
            task: payload
        };
    }
    
    when (taskType.contains("user"))
    {
        emit "task.assigned", {
            taskId: payload.id,
            assignedTo: "agent-2", 
            task: payload
        };
    }
    
    when (taskType.contains("business"))
    {
        emit "task.assigned", {
            taskId: payload.id,
            assignedTo: "agent-3",
            task: payload
        };
    }
}

// Task processing by assigned agents
on "task.assigned" (payload)
{
    // Find assigned agent and process task
    for (agent in agents)
    {
        when (agent.id == payload.assignedTo)
        {
            var result = agent.processTask(payload.task);
            emit "task.completed", result;
        }
    }
}
```

### Real-Time Adaptive Response System
```cx
using textGen from "Cx.AI.TextGeneration";
using embeddings from "Cx.AI.TextEmbeddings";

// Environmental monitoring
on "environment.change" (payload)
{
    // Analyze the nature of the change
    var changeType = textGen.GenerateAsync(
        "Classify this environmental change: user-behavior, system-performance, external-factor, or error. One word:",
        payload.description
    );
    
    var severity = textGen.GenerateAsync(
        "Rate severity 1-5 (1=minor, 5=critical). Number only:",
        payload.description
    );
    
    emit "environment.analyzed", {
        originalChange: payload,
        changeType: changeType.toLowerCase(),
        severity: parseFloat(severity),
        requiresAdaptation: parseFloat(severity) >= 3.0
    };
}

// Autonomous adaptation
on "environment.analyzed" (payload)
{
    when (payload.requiresAdaptation)
    {
        // Generate adaptation strategy
        var strategy = textGen.GenerateAsync(
            "Given " + payload.changeType + " change with severity " + payload.severity +
            "/5, what adaptation strategy should be used: increase-monitoring, adjust-thresholds, escalate-human, or maintain-course?",
            { temperature: 0.3 }
        );
        
        emit "adaptation.strategy", {
            environmentChange: payload,
            proposedStrategy: strategy,
            confidence: this.calculateStrategyConfidence(strategy, payload)
        };
    }
}

// Strategy execution
on "adaptation.strategy" (payload)
{
    when (payload.proposedStrategy.contains("increase-monitoring"))
    {
        emit "monitoring.increase", {
            reason: payload.environmentChange,
            duration: "1-hour",
            intensity: "high"
        };
    }
    
    when (payload.proposedStrategy.contains("adjust-thresholds"))
    {
        emit "thresholds.adjust", {
            reason: payload.environmentChange,
            adjustment: "adaptive",
            temporary: true
        };
    }
    
    when (payload.proposedStrategy.contains("escalate-human"))
    {
        emit "escalation.human", {
            reason: "Autonomous system requires human intervention",
            context: payload.environmentChange,
            urgency: "high"
        };
    }
}
```

## Error Handling in Event-Driven Systems

### Robust Error Recovery
```cx
using textGen from "Cx.AI.TextGeneration";

// Error detection and classification
on "system.error" (payload)
{
    try
    {
        var errorType = textGen.GenerateAsync(
            "Classify error type: recoverable, requires-restart, data-corruption, or external-dependency. One word:",
            payload.errorMessage
        );
        
        var severity = textGen.GenerateAsync(
            "Rate error severity 1-5. Number only:",
            payload.errorMessage
        );
        
        emit "error.classified", {
            originalError: payload,
            errorType: errorType.toLowerCase(),
            severity: parseFloat(severity),
            timestamp: now()
        };
    }
    catch (analysisError)
    {
        // Fallback to basic error handling
        emit "error.unclassified", {
            originalError: payload,
            analysisError: analysisError.toString(),
            fallbackAction: "basic-recovery"
        };
    }
}

// Autonomous error recovery
on "error.classified" (payload)
{
    when (payload.errorType == "recoverable" && payload.severity <= 3.0)
    {
        emit "recovery.automatic", {
            error: payload.originalError,
            strategy: "retry-with-backoff",
            maxAttempts: 3
        };
    }
    
    when (payload.errorType == "requires-restart")
    {
        emit "recovery.restart", {
            error: payload.originalError,
            component: payload.originalError.component,
            graceful: payload.severity <= 3.0
        };
    }
    
    when (payload.severity >= 4.0)
    {
        emit "escalation.critical", {
            error: payload.originalError,
            classification: payload,
            requiresImmediate: true
        };
    }
}

// Recovery execution
on "recovery.automatic" (payload)
{
    var attempts = 0;
    var maxAttempts = payload.maxAttempts || 3;
    
    function attemptRecovery()
    {
        attempts++;
        
        try
        {
            // Attempt recovery operation
            var result = executeRecovery(payload.error);
            emit "recovery.successful", {
                error: payload.error,
                attempts: attempts,
                result: result
            };
        }
        catch (recoveryError)
        {
            when (attempts < maxAttempts)
            {
                // Wait and retry
                setTimeout(attemptRecovery, 1000 * attempts); // Exponential backoff
            }
            else
            {
                emit "recovery.failed", {
                    originalError: payload.error,
                    recoveryAttempts: attempts,
                    finalError: recoveryError.toString()
                };
            }
        }
    }
    
    attemptRecovery();
}
```

## Performance Considerations

### Event Bus Optimization
```cx
// Efficient event filtering and routing
on "high-frequency.sensor" (payload)
{
    // Sample high-frequency data to prevent overload
    when (payload.timestamp % 5 == 0) // Process every 5th event
    {
        emit "sensor.sampled", {
            data: payload,
            samplingRate: "1-in-5",
            timestamp: now()
        };
    }
}

// Batch processing for efficiency
var batchBuffer = [];
var batchSize = 10;
var batchTimeout = 5000; // 5 seconds

on "data.item" (payload)
{
    batchBuffer.push(payload);
    
    when (batchBuffer.length >= batchSize)
    {
        emit "data.batch", {
            items: batchBuffer,
            batchSize: batchBuffer.length,
            timestamp: now()
        };
        
        batchBuffer = []; // Clear buffer
    }
}

// Timeout-based batch processing
setTimeout(function()
{
    when (batchBuffer.length > 0)
    {
        emit "data.batch", {
            items: batchBuffer,
            batchSize: batchBuffer.length,
            reason: "timeout",
            timestamp: now()
        };
        
        batchBuffer = [];
    }
}, batchTimeout);
```

## Testing Event-Driven Systems

### Event Testing Patterns
```cx
using textGen from "Cx.AI.TextGeneration";

class EventSystemTester
{
    capturedEvents: array;
    expectedEvents: array;
    
    constructor()
    {
        this.capturedEvents = [];
        this.expectedEvents = [];
    }
    
    function startCapture()
    {
        // Capture all events for testing
        on "*" (payload) // Wildcard event listener
        {
            this.capturedEvents.push({
                eventName: currentEventName(),
                payload: payload,
                timestamp: now()
            });
        }
    }
    
    function testEventFlow(initialEvent, expectedOutcomes)
    {
        this.expectedEvents = expectedOutcomes;
        this.capturedEvents = [];
        
        // Trigger initial event
        emit initialEvent.name, initialEvent.payload;
        
        // Wait for event processing
        setTimeout(function()
        {
            this.validateEventFlow();
        }, 2000);
    }
    
    function validateEventFlow()
    {
        var results = {
            expectedCount: this.expectedEvents.length,
            actualCount: this.capturedEvents.length,
            matches: [],
            mismatches: []
        };
        
        for (expected in this.expectedEvents)
        {
            var found = this.findMatchingEvent(expected);
            
            when (found)
            {
                results.matches.push({
                    expected: expected,
                    actual: found
                });
            }
            else
            {
                results.mismatches.push({
                    expected: expected,
                    reason: "Event not found"
                });
            }
        }
        
        emit "test.results", {
            testType: "event-flow",
            results: results,
            passed: results.mismatches.length == 0
        };
    }
    
    function findMatchingEvent(expectedEvent)
    {
        for (captured in this.capturedEvents)
        {
            when (captured.eventName == expectedEvent.name)
            {
                // Use AI to validate payload similarity
                var similarity = textGen.GenerateAsync(
                    "Are these payloads similar? Expected: " + JSON.stringify(expectedEvent.payload) +
                    " Actual: " + JSON.stringify(captured.payload) + " Answer YES or NO:",
                    { temperature: 0.1 }
                );
                
                when (similarity == "YES")
                {
                    return captured;
                }
            }
        }
        
        return null;
    }
}
```

## Best Practices for Event-Driven Architecture

### ✅ DO: Event-Driven Best Practices
1. **Use descriptive event names** with dot notation: `user.message.received`
2. **Include timestamps** and source information in event payloads
3. **Handle events asynchronously** to prevent blocking other agents
4. **Implement error handling** within event handlers
5. **Use structured payloads** for reliable processing
6. **Monitor event flow** and performance metrics
7. **Test event chains** to ensure proper system behavior
8. **Implement circuit breakers** for high-frequency events

### ❌ DON'T: Common Event-Driven Pitfalls
1. **Don't create infinite event loops** without termination conditions
2. **Don't block event handlers** with long-running synchronous operations
3. **Don't ignore failed events** without proper error handling
4. **Don't emit events from within the same event handler** without careful consideration
5. **Don't use generic event names** that make debugging difficult
6. **Don't skip event validation** and payload structure verification
7. **Don't create too many event types** that fragment the system
8. **Don't forget to clean up** event listeners and resources

## Event Naming Conventions

### Hierarchical Event Names
- `user.login.successful`
- `user.login.failed`
- `system.startup.complete`
- `agent.task.completed`
- `environment.change.detected`
- `error.recovery.attempted`
- `escalation.human.required`

### Event Payload Standards
```cx
// Standard event payload structure
{
    timestamp: "2025-07-19T10:30:00Z",
    source: "autonomous-agent-1", 
    version: "1.0",
    correlationId: "unique-trace-id",
    data: {
        // Actual event data
    },
    metadata: {
        // Additional context
    }
}
```

## Integration with AI Services

### AI-Enhanced Event Processing
```cx
using textGen from "Cx.AI.TextGeneration";
using embeddings from "Cx.AI.TextEmbeddings";

// AI-powered event classification
on "event.raw" (payload)
{
    var eventCategory = textGen.GenerateAsync(
        "Classify this event: user-action, system-event, error, notification, or data-update. One word:",
        payload.description
    );
    
    var priority = textGen.GenerateAsync(
        "Priority 1-5 (1=low, 5=critical). Number only:",
        payload.description  
    );
    
    // Generate embedding for semantic similarity
    var eventEmbedding = embeddings.GenerateEmbeddingAsync(payload.description);
    
    emit "event.classified", {
        originalEvent: payload,
        category: eventCategory.toLowerCase(),
        priority: parseFloat(priority),
        embedding: eventEmbedding,
        needsImmediate: parseFloat(priority) >= 4.0
    };
}
```

---

**Next Steps**:
- Explore [[Semantic Similarity Patterns]] for AI-powered event analysis
- Learn [[Multi-Agent Coordination]] for complex event-driven systems
- Master [[Autonomous Programming Best Practices]] for robust event handlers

**Related Examples**:
- [[Presence Detection System]] - Real-time event processing
- [[Climate Debate Demo]] - Multi-agent event coordination
- [[Content Classification Pipeline]] - Event-driven AI processing
