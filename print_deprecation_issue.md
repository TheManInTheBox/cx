# Deprecate print() function in favor of consciousness-aware console I/O

## Issue Summary

Deprecate the legacy `print()` function in CX Language in favor of the new consciousness-aware console I/O patterns using `emit system.console.write` events.

## Motivation

The CX Language Basic I/O milestone (#17) has successfully implemented comprehensive consciousness-aware I/O operations. The legacy `print()` function is now inconsistent with the event-driven, consciousness-aware architecture that CX Language promotes.

## Changes Made

### 1. Documentation Updates
- ✅ Updated `.github/instructions/cx.instructions.md` to deprecate `print()`
- ✅ Replaced print examples with `emit system.console.write` patterns
- ✅ Added consciousness-aware console output documentation

### 2. Example Updates  
- ✅ Updated `examples/core_features/simple_test_with_visualization.cx`
- 🔄 Need to update remaining example files

### 3. New Recommended Patterns

#### Simple Text Output
```csharp
// Old (deprecated)
print("Hello, CX Language!");

// New (recommended)
emit system.console.write { text: "Hello, CX Language!" };
```

#### Colored Output
```csharp
emit system.console.write { 
    text: "Success!", 
    foregroundColor: "green" 
};
```

#### Object Serialization
```csharp
// Old (deprecated)
print(consciousEntity);

// New (recommended)
emit system.console.write { object: consciousEntity };
```

#### Positioned Output with Cursor Control
```csharp
emit system.console.write { 
    text: "Status: Active", 
    x: 20, y: 10, 
    foregroundColor: "cyan",
    hideCursor: true
};
```

## Implementation Status

- ✅ ConsoleService.cs implements comprehensive console I/O events
- ✅ FileService.cs implements file I/O operations  
- ✅ CxPrint.Print() continues to work for internal serialization
- ✅ Documentation updated with new patterns
- 🔄 Need to update remaining examples and wiki content

## Benefits

1. **Consciousness-Aware**: All I/O operations integrate with the event-driven consciousness architecture
2. **Enhanced Features**: Color support, cursor positioning, visibility control
3. **Cross-Platform**: Native .NET Console methods for maximum compatibility
4. **Consistent Architecture**: Aligns with CX Language's pure event-driven paradigm
5. **Future-Ready**: Extensible for advanced I/O capabilities

## Next Steps

1. Update remaining example files to use new patterns
2. Update wiki documentation 
3. Consider adding a deprecated `print()` function with warnings (optional)
4. Update any generated code templates

## Related Issues

- Milestone: CX Language Basic I/O (#17)
- Related: Enhanced consciousness entity serialization (#232)
- Related: I/O demonstration examples (#233)

This deprecation represents a major step forward in CX Language's evolution toward a fully consciousness-aware programming paradigm.
