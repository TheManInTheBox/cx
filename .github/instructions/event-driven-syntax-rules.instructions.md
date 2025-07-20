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
// Event receiver with conditional logic
on user.input (payload)  // ✅ UNQUOTED event name
{
    // Handle incoming event
    print("Received: " + payload.data);
    
    // ✅ CORRECT: Use 'if' for ALL conditionals
    if (payload.priority > 5)
    {
        emit high.priority, payload;  // ✅ UNQUOTED event name
    }
    
    if (payload.type == "urgent")
    {
        // More conditional logic
        emit escalation.needed, { urgency: "high" };
    }
}

// Function with conditional logic  
function processData(data)
{
    if (data.score > 0.8)         // ✅ CORRECT: 'if' everywhere
    {
        emit data.processed, data;   // ✅ CORRECT: 'emit' anywhere, unquoted
        return "processed";
    }
    
    return "rejected";
}

// Class with instance event receiver
class Agent
{
    on task.assigned (payload)   // ✅ CORRECT: unquoted event name
    {
        if (payload.target == this.name)  // ✅ CORRECT: 'if' in event handlers
        {
            emit task.accepted, payload;   // ✅ CORRECT: unquoted emit
        }
    }
}

// Standalone code
var result = calculateSomething();
if (result.success)               // ✅ CORRECT: 'if' in standalone code
{
    emit calculation.done, result;  // ✅ CORRECT: unquoted emit anywhere
}
```

## 🎯 MEMORY AID

**"if" = ALL conditionals EVERywhere**  
**"emit" = EVERywhere Means It's Totally allowed**  
**Event names = NO quotes needed (user.input, not "user.input")**

## 🔄 QUICK DECISION TREE

1. Do you need conditional logic?
   - **ALWAYS** → Use `if` (no exceptions!)

2. Do you want to publish an event?
   - **ANYWHERE** → Use `emit eventName, payload` (no quotes on event name)

3. Do you want to listen for events?
   - **Global level** → Use `on eventName (payload) { ... }` (no quotes)
   - **Class instance** → Use `on eventName (payload) { ... }` inside class (no quotes)

## 📝 RECENT SIMPLIFICATIONS

### ✅ REMOVED: `when` keyword
- **Old**: `when (condition) { ... }` inside event handlers
- **New**: `if (condition) { ... }` everywhere (simpler!)

### ✅ REMOVED: Quoted event names  
- **Old**: `on "user.input" (payload) { ... }`
- **New**: `on user.input (payload) { ... }` (cleaner!)

---

**Remember**: These simplified rules eliminate complexity and potential errors while maintaining full event-driven architecture capabilities.
