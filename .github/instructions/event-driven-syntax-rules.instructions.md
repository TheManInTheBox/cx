---
applyTo: "**"
description: "CX Event-Driven Architecture - Critical Syntax Rules Quick Reference"
---

# CX Event-Driven Architecture - Syntax Quick Reference

## ⚠️ CRITICAL SCOPING RULES

### `if` keyword
- **Used for ALL conditional logic everywhere**
- Functions, classes, event handlers, standalone code
- Universal conditional statement - no context restrictions

### `emit` keyword  
- **Globally available everywhere**
- Can be used in: functions, classes, event handlers, standalone code
- Publishes events to the event bus

### `on` keyword
- **Defines event receiver functions**
- Can be used: globally or within class instances
- Creates event handlers that listen for specific events
- **Event names are UNQUOTED**: `on user.input (payload) { ... }`

## ✅ CORRECT PATTERNS

```cx
// Enhanced event receiver with embedded context in payload
on user.input (payload)  // ✅ UNQUOTED event name, single parameter
{
    // payload.context contains caller info, session, timestamp, processing chain
    // payload.data contains the actual event data and business logic
    print("Event from: " + payload.context.caller + " in session: " + payload.context.sessionId);
    print("Data: " + payload.data + ", Priority: " + payload.priority);
    
    // ✅ CORRECT: Use 'if' for ALL conditionals
    if (payload.priority > 5)
    {
        emit high.priority, payload;  // ✅ UNQUOTED event name, embedded context
    }
    
    if (payload.context.caller == "system")
    {
        // Enhanced context-aware processing
        emit escalation.needed, { 
            context: payload.context,
            data: {
                urgency: "high",
                originalData: payload.data,
                reason: "system-level-request"
            }
        };
    }
}

// ✅ BREAKTHROUGH: Extended Event Name Grammar Support
on support.tickets.new (payload)  // ✅ Keywords in event names working!
{
    if (payload.ticketId)
    {
        print("🎫 New ticket: " + payload.ticketId);
        emit alerts.high, { ticket: payload };
    }
}

on dev.tasks.assigned (payload)  // ✅ Multiple keywords supported
{
    if (payload.assignee)
    {
        print("📋 Task assigned to: " + payload.assignee);
        emit notifications.task, payload;
    }
}

// ✅ WILDCARD SUPPORT: 'any' keyword for cross-namespace matching
on any.critical (payload)  // ✅ Matches system.critical, alerts.critical, etc.
{
    if (payload.severity > 8)
    {
        print("🚨 CRITICAL EVENT from any namespace!");
        emit emergency.response, payload;
    }
}

// Enhanced function with context-aware event emission
function processData(data, requestContext)
{
    if (data.score > 0.8)         // ✅ CORRECT: 'if' everywhere
    {
        // Enhanced emit with embedded context
        emit data.processed, {
            context: requestContext,
            result: {
                data: data,
                confidence: data.score,
                processor: "processData"
            }
        };
        return "processed";
    }
    
    return "rejected";
}

// Enhanced class with instance event receiver  
class Agent
{
    name: string;
    
    constructor(agentName)
    {
        this.name = agentName;
    }
    
    // ✅ AUTO-REGISTRATION: Class event handlers automatically register with namespace bus
    on task.assigned (payload)   // ✅ CORRECT: single parameter with embedded context
    {
        if (payload.task.assignee == this.name)     // ✅ CORRECT: 'if' in event handlers
        {
            var responseContext = {
                caller: this.name,
                timestamp: "now",
                originalEvent: "task.assigned",
                sessionId: payload.context.sessionId,
                processingChain: payload.context.processingChain + " → " + this.name
            };
            
            emit task.accepted, {
                context: responseContext,
                result: {
                    agent: this.name,
                    task: payload.task,
                    acceptedAt: "now"
                }
            };
        }
    }
    
    // ✅ WILDCARD HANDLERS: Auto-register for all namespace patterns
    on support.any (payload)  // Matches support.tickets, support.users, etc.
    {
        if (payload.priority == "urgent")
        {
            print("🎯 " + this.name + " handling urgent support event");
            emit priority.escalation, payload;
        }
    }
}

// Enhanced standalone code with context tracking
var result = calculateSomething();
if (result.success)               // ✅ CORRECT: 'if' in standalone code
{
    var completionContext = {
        caller: "main-process",
        timestamp: "now",
        originalEvent: "calculation.started", 
        sessionId: "main-session"
    };
    
    emit calculation.done, {
        context: completionContext,
        result: {
            data: result.data,
            duration: result.processingTime,
            method: "calculateSomething"
        }
    };
}
```

## 🎯 MEMORY AID

**"if" = ALL conditionals EVERywhere**  
**"emit" = EVERywhere Means It's Totally allowed**  
**Event names = NO quotes needed (user.input, not "user.input")**
**Keywords supported = new, critical, assigned, tickets, tasks, support, dev, system, alerts**
**Wildcards = 'any' keyword matches ALL namespaces**
**Auto-registration = Class event handlers automatically register with namespace bus**

## 🔄 QUICK DECISION TREE

1. Do you need conditional logic?
   - **ALWAYS** → Use `if` (no exceptions!)

2. Do you want to publish an event?
   - **ANYWHERE** → Use `emit eventName, payload` (context embedded in payload)

3. Do you want to listen for events?
   - **ALWAYS** → Use `on eventName (payload) { ... }` (context in payload.context)
   - **Location** → Global level or inside classes (no quotes on event names)

4. Do you need context tracking?
   - **YES** → Use payload.context structure for caller, session, processing chain
   - **NO** → Use simple payload structure for basic event handling

5. Do you need wildcard matching?
   - **YES** → Use `any.critical` for ALL namespace critical events
   - **NO** → Use specific namespace patterns like `support.tickets.new`

6. Do you need auto-registration?
   - **YES** → Put `on` handlers inside class definitions for automatic namespace bus registration
   - **NO** → Use global `on` handlers for manual registration

## 📝 RECENT BREAKTHROUGHS

### ✅ COMPLETE: Extended Event Name Grammar
- **Keywords Supported**: `new`, `critical`, `assigned`, `tickets`, `tasks`, `support`, `dev`, `system`, `alerts`
- **Event Names**: `support.tickets.new`, `dev.tasks.assigned`, `system.critical`
- **Grammar**: `eventNamePart: IDENTIFIER | 'any' | 'agent' | 'new' | 'critical' | ...`

### ✅ COMPLETE: Auto-Registration System
- **Class-Based**: `on` handlers inside classes auto-register with namespace bus
- **Wildcard Support**: `any.critical` matches ALL namespace critical events
- **Zero Manual Setup**: No `RegisterNamespacedAgent()` calls needed

### ✅ COMPLETE: Native Emit Syntax
- **Working**: `emit support.tickets.new, { ticketId: "T-001" };`
- **Object Payloads**: Complex object literals passed correctly
- **Event Delivery**: Messages delivered to correct agent instances

---

**Remember**: These simplified rules eliminate complexity and potential errors while maintaining full event-driven architecture capabilities with production-ready auto-registration and wildcard matching.
