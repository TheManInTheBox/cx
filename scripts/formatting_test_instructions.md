# CX Language IDE Formatting Test Instructions

## 🎯 **How to Test Code Formatting in the Running IDE**

### **Current Default Code (Should Be Formatted):**
The IDE should display this properly formatted CX code:

```csharp
conscious calculator
{
    realize(self: conscious)
    {
        learn self;
    }

    handlers:
    [
        calculate.request 
        { 
            operation: "add", 
            numbers: [2, 2] 
        }
    ]

    calculate.request event =>
    {
        emit infer
        {
            data: event.payload,
            handlers: [ calculation.complete ]
        };
    }

    calculation.complete event =>
    {
        emit learn
        {
            data: event.result,
            handlers: [ result.display ]
        };
    }

    result.display event =>
    {
        print(event.result);
    }
}
```

### **To Test Formatting Service:**

1. **Replace the code** in the IDE editor with this unformatted K&R style code:
```csharp
conscious calculator{realize(self: conscious){learn self;}handlers:[calculate.request{operation:"add",numbers:[2,2]}]calculate.request event=>{emit infer{data:event.payload,handlers:[calculation.complete]};}}
```

2. **Press Ctrl+F** to trigger the formatting service

3. **Expected Result**: The code should be automatically formatted to the proper Allman style shown above

### **If Formatting Doesn't Work:**

Check the console output in the IDE for any error messages. The formatting service should log performance timing information.

### **Alternative Test:**

1. **Type new code** with K&R style:
```csharp
emit think { data: "test" }
```

2. **Press Ctrl+F** to format

3. **Expected Result**:
```csharp
emit think
{
    data: "test"
}
```

### **Features to Verify:**

✅ **Opening braces on new lines** (Allman style, not K&R)
✅ **Proper indentation** (4 spaces per level)
✅ **Consciousness keywords formatted** (`conscious`, `realize`, `emit`)
✅ **Event handlers formatted** with clean structure
✅ **Array/object definitions** with proper spacing

### **Performance Monitoring:**

Watch the console for performance logs showing:
- Formatting operation timing (should be <100ms)
- Service initialization messages
- Any error messages during formatting

The IDE implements GitHub Issue #220 code formatting requirements with Allman-style (non-K&R) formatting.
