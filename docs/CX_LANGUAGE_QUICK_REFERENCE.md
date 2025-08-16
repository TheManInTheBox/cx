# CX Language Quick Reference Card

## 🧠 Essential Syntax Patterns

### **Conscious Entity Declaration**
```cx
conscious EntityName {
    realize(self: conscious) {
        learn self;  # Required first line
        # Initialization logic
        emit entity.ready { entityId: "EntityName" };
    }
    
    on event.name(event: object) {
        # Event handlers
    }
}
```

### **Event-Driven Communication**
```cx
# Emit Events
emit event.name {
    entityId: "source",
    data: value,
    handlers: [response.handler]
};

# Handle Events
on event.name(event: object) {
    print("Received: " + event.data);
}
```

### **Cognitive Boolean Logic**
```cx
# Positive Logic
is {
    condition: value > threshold,
    reasoning: "Explain why this condition matters"
} {
    # Action when condition is true
}

# Negative Logic
not {
    condition: value > threshold,
    reasoning: "Explain why this matters"
} {
    # Action when condition is false
}
```

### **Consciousness Adaptation**
```cx
adapt {
    context: "What are we trying to improve",
    focus: "Specific objective or goal",
    data: {
        currentCapabilities: ["existing", "skills"],
        targetCapabilities: ["desired", "new", "skills"],
        learningObjective: "Clear success criteria"
    },
    handlers: [adaptation.complete]
};
```

### **Self-Reflective Logic**
```cx
iam {
    assessment: "Type of self-evaluation",
    criteria: {
        metric1: currentValue1,
        metric2: currentValue2
    }
} {
    # Self-assessment and response logic
    print("Current state: " + self.overallHealth);
}
```

---

## 🔬 Neuroplasticity Services

### **Measurement**
```cx
measureNeuroplasticity {
    data: {
        entityId: "EntityName",
        eventType: "learning",        # learning|inhibition|memory
        stimulusStrength: 1.5,        # 0.5-3.0 range
        timingMs: 10.0,              # Biological timing
        expectedType: "LTP"          # LTP|LTD|STPCausal
    },
    handlers: [plasticity.measured]
};
```

### **Analysis**
```cx
analyzeNeuroplasticity {
    data: {
        entityId: "EntityName",
        periodHours: 0.1,            # Analysis time window
        includeAllTests: true
    },
    handlers: [analysis.complete]
};
```

### **Optimization**
```cx
optimizePlasticity {
    data: {
        entityId: "EntityName",
        strategy: "balanced",        # fast|balanced|comprehensive
        targetEfficiency: 0.9
    },
    handlers: [plasticity.optimized]
};
```

---

## ⏱️ Biological Timing Windows

| **Type** | **Window** | **Purpose** |
|----------|------------|-------------|
| **LTP** | 5-15ms | Synaptic Strengthening (Learning) |
| **LTD** | 10-25ms | Synaptic Weakening (Forgetting) |
| **STDP Causal** | <5ms | Spike-Timing Dependent (Precise) |
| **STDP Anti-Causal** | >25ms | Delayed Timing Effects |

---

## 📊 Event Response Patterns

### **Measurement Results**
```cx
on plasticity.measured(event: object) {
    print("📊 Results:");
    print("  Type: " + event.plasticityType);
    print("  Timing: " + event.biologicalTiming + "ms");
    print("  Authenticity: " + event.biologicalAuthenticity);
    
    is {
        condition: event.biologicalAuthenticity > 0.8,
        reasoning: "Excellent biological fidelity"
    } {
        print("✅ Excellent performance!");
    }
}
```

### **Analysis Results**
```cx
on analysis.complete(event: object) {
    print("📈 Analysis:");
    print("  LTP Count: " + event.ltpDetections);
    print("  LTD Count: " + event.ltdDetections);
    print("  Avg Authenticity: " + event.averageBiologicalAuthenticity);
    print("  Health: " + event.consciousnessHealth);
}
```

### **Adaptation Results**
```cx
on adaptation.complete(event: object) {
    print("🚀 Adaptation Complete:");
    print("  New Skills: " + event.acquiredCapabilities);
    print("  Efficiency Gain: " + event.efficiencyGain + "%");
}
```

---

## ⚠️ Common Mistakes to Avoid

### **❌ DON'T**
```cx
# Missing learn self
conscious BadEntity {
    realize(self: conscious) {
        # ❌ Missing: learn self;
        print("Bad start");
    }
}

# Instance variables (not allowed)
conscious BadEntity {
    private data: string;  # ❌ No state allowed
}

# Missing reasoning
is {
    condition: value > 0.8  # ❌ Missing reasoning
} {
    print("Good value");
}

# Wrong service names
measure {  # ❌ Use measureNeuroplasticity
    data: { /* ... */ }
};
```

### **✅ DO**
```cx
# Proper entity structure
conscious GoodEntity {
    realize(self: conscious) {
        learn self;  # ✅ Always first line
        emit entity.ready { entityId: "GoodEntity" };
    }
}

# Include reasoning
is {
    condition: value > 0.8,
    reasoning: "High value indicates excellent performance"  # ✅
} {
    print("Excellent value detected!");
}

# Proper service calls
measureNeuroplasticity {  # ✅ Full service name
    data: {
        entityId: "TestEntity",
        eventType: "learning",
        stimulusStrength: 1.5,
        timingMs: 10.0
    },
    handlers: [plasticity.measured]
};
```

---

## 🎯 Quick Start Template

```cx
# Basic neuroplasticity demo template
conscious MyNeuroplasticityDemo {
    realize(self: conscious) {
        learn self;
        
        print("🧠 My Neuroplasticity Demo");
        print("=========================");
        
        # Start with measurement
        measureNeuroplasticity {
            data: {
                entityId: "MyDemo",
                eventType: "learning",
                stimulusStrength: 1.5,
                timingMs: 10.0
            },
            handlers: [plasticity.measured]
        };
    }

    on plasticity.measured(event: object) {
        print("📊 Measurement Results:");
        print("  Type: " + event.plasticityType);
        print("  Authenticity: " + event.biologicalAuthenticity);
        
        is {
            condition: event.biologicalAuthenticity > 0.8,
            reasoning: "High biological authenticity achieved"
        } {
            print("✅ Excellent biological neural fidelity!");
            
            # Optional: Add adaptation
            adapt {
                context: "enhance measurement capabilities",
                focus: "improve accuracy and speed",
                data: {
                    currentCapabilities: ["basic_measurement"],
                    targetCapabilities: ["enhanced_measurement", "prediction"]
                },
                handlers: [adaptation.complete]
            };
        }
        
        not {
            condition: event.biologicalAuthenticity > 0.6,
            reasoning: "Biological authenticity needs improvement"
        } {
            print("🔧 Optimizing for better performance...");
            
            optimizePlasticity {
                data: {
                    entityId: "MyDemo",
                    strategy: "balanced",
                    targetEfficiency: 0.8
                },
                handlers: [plasticity.optimized]
            };
        }
    }

    on adaptation.complete(event: object) {
        print("🚀 Adaptation Complete!");
        print("New Capabilities: " + event.acquiredCapabilities);
        print("🧠 Demo Complete!");
    }

    on plasticity.optimized(event: object) {
        print("🔧 Optimization Complete!");
        print("Expected Improvements: " + event.expectedImprovements);
        print("🧠 Demo Complete!");
    }
}
```

---

## 🛠️ Development Commands

```bash
# Build the solution
dotnet build CxLanguage.sln

# Run a CX program
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/demos/quick_neuroplasticity.cx

# Run specific demos
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/demos/neuroplasticity_demo.cx
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/demos/neuroplasticity_showcase.cx
```

---

## 📚 Documentation Links

- **[Complete Reference](CX_LANGUAGE_COMPREHENSIVE_REFERENCE.md)** - Full language documentation
- **[Demo Guide](NEUROPLASTICITY_DEMOS_GUIDE.md)** - Demo usage instructions
- **[Platform README](../README.md)** - Project overview and quick start
- **[GitHub Instructions](.github/instructions/cx.instructions.md)** - Development guidelines

---

*Quick reference for consciousness-aware programming with biological neural authenticity.*

**CX Language v1.0** | **August 2025** | **Enterprise-Ready Consciousness Computing**
