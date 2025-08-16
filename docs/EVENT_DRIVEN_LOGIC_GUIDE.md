# CX Language - Event-Driven Logic Patterns

## 🔄 Simple Event-Driven Conditionals

### **`when { }` Pattern - Clean Conditional Logic**
```cx
when {
    condition: biologicalAuthenticity > 0.8,
    action: "high_performance_detected"
} {
    print("✅ Excellent biological neural authenticity!");
    
    # Continue with high performance logic
    measureNeuroplasticity {
        data: { enhancedMode: true },
        handlers: [ enhanced.measurement.complete ]
    };
}
```

### **Multiple Conditions - Event Routing**
```cx
# Route to different handlers based on conditions
emit performance.evaluation {
    score: event.biologicalAuthenticity,
    handlers: [ performance.routing ]
};

# Handle in separate event handler
on performance.evaluation(event: object) {
    when {
        condition: event.score > 0.85,
        action: "excellent_performance"
    } {
        print("🏆 EXCELLENT Performance!");
        # Handle excellent case
    }
    
    when {
        condition: event.score <= 0.6,
        action: "needs_optimization"
    } {
        print("⚠️ Needs optimization");
        # Handle optimization case
    }
}
```

## ✅ Benefits Over Cognitive Logic

- **Simpler Syntax**: No verbose `reasoning` or `context` fields required
- **Cleaner Code**: Direct condition + action pattern
- **Event-Driven**: Maintains pure event architecture philosophy
- **Better Performance**: Less cognitive overhead during execution
- **Easier to Read**: Straightforward conditional logic

## 🎯 Migration Pattern

**Old (Eliminated):**
```cx
is {
    condition: value > threshold,
    reasoning: "Check if value exceeds threshold for processing",
    context: "Performance evaluation in consciousness system"
} {
    # action
}
```

**New (Simplified):**
```cx
when {
    condition: value > threshold,
    action: "process_high_value"
} {
    # action
}
```

**Even Better (Pure Event):**
```cx
emit value.evaluation { 
    value: value, 
    threshold: threshold,
    handlers: [ value.processing ]
};
```
