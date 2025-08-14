# Quick Reference: Adapter Pattern for CX Language Teams

## Overview

This quick reference guide explains how to use the adapter pattern to bridge between the Aura Sensory Runtime Framework and CX Language of Consciousness event systems.

## Key Concepts

1. **Two Event Systems**: Runtime (sensory) and Core (consciousness)
2. **Adapter Pattern**: Bridges between the two systems
3. **Event Forwarding**: Selective forwarding of events between systems

## Interfaces

### Runtime Event Interface

```csharp
// CxLanguage.Runtime.Events.ICxEventBus
Task<bool> EmitAsync(string eventName, IDictionary<string, object> payload = null, object sender = null);
string RegisterHandler(string eventName, Func<string, IDictionary<string, object>, object, Task> handler);
bool UnregisterHandler(string subscriptionId);
```

### Core Event Interface

```csharp
// CxLanguage.Core.Events.ICxEventBus
Task<bool> EmitAsync(string eventName, IDictionary<string, object> payload = null);
string RegisterHandler(string eventName, Func<CxEventPayload, Task> handler);
bool UnregisterHandler(string subscriptionId);
```

## Using the Adapter

### Service Registration

```csharp
// Add Runtime event bus (Aura Sensory Runtime Framework)
services.AddSingleton<RuntimeEvents.ICxEventBus, RuntimeEvents.LoggingEventBus>();

// Add Core event bus (CX Language of Consciousness)
services.AddSingleton<CoreEvents.ICxEventBus, CoreEvents.CxEventBus>();

// Add the adapter that bridges between the two systems
services.AddSingleton<RuntimeEvents.ICxEventBus>(provider => {
    var runtimeBus = provider.GetRequiredService<RuntimeEvents.ICxEventBus>();
    var coreBus = provider.GetRequiredService<CoreEvents.ICxEventBus>();
    var logger = provider.GetRequiredService<ILogger<AuraSensoryCxLanguageAdapter>>();
    return new AuraSensoryCxLanguageAdapter(runtimeBus, coreBus, logger);
});
```

### Emitting Events

```csharp
// Get the event bus through DI
var eventBus = serviceProvider.GetRequiredService<RuntimeEvents.ICxEventBus>();

// Emit an event
await eventBus.EmitAsync("event.name", new Dictionary<string, object>
{
    ["key1"] = "value1",
    ["key2"] = "value2"
});
```

### Registering Handlers

#### Runtime Handler

```csharp
// Runtime event handler
eventBus.RegisterHandler("event.name", async (eventName, data, sender) => {
    Console.WriteLine($"Runtime handler received: {eventName}");
    // Process event
    await Task.CompletedTask;
});
```

#### Core Handler

```csharp
// Core event handler
var coreBus = serviceProvider.GetRequiredService<CoreEvents.ICxEventBus>();
coreBus.RegisterHandler("event.name", async (payload) => {
    Console.WriteLine($"Core handler received: {payload.EventName}");
    // Process event
    await Task.CompletedTask;
});
```

## Event Naming Conventions

Events are automatically forwarded between systems based on naming conventions:

- `core.*`: Core events (forwarded to Core)
- `inference.*`: Inference events (forwarded to Core)
- `gpu.*`: GPU-related events (forwarded to Core)
- `sensory.*`: Sensory events (not forwarded, Runtime only)
- `hardware.*`: Hardware events (not forwarded, Runtime only)

## Common Usage Patterns

### GPU Processing Events

```csharp
// Emit GPU event
await eventBus.EmitAsync("gpu.check", new Dictionary<string, object>
{
    ["cudaAvailable"] = true,
    ["gpuCount"] = 1
});
```

### Inference Events

```csharp
// Emit inference event
await eventBus.EmitAsync("inference.complete", new Dictionary<string, object>
{
    ["model"] = "phi-3",
    ["duration"] = 120,
    ["tokens"] = 1024
});
```

### Sensory Processing Events

```csharp
// Emit sensory event (not forwarded to Core)
await eventBus.EmitAsync("sensory.vision", new Dictionary<string, object>
{
    ["image"] = "imageData",
    ["format"] = "jpeg"
});
```

## Best Practices

1. **Use the Right Interface**: Use the appropriate interface for your component:
   - Runtime components use `CxLanguage.Runtime.Events.ICxEventBus`
   - Core components use `CxLanguage.Core.Events.ICxEventBus`

2. **Naming Conventions**: Follow event naming conventions to ensure proper forwarding

3. **Error Handling**: Always include error handling in event handlers

4. **Asynchronous Processing**: Ensure all handlers properly use async/await

5. **Payload Design**: Keep payloads simple and serializable

## Debugging Tips

1. Debug logging is built into both event buses and the adapter
2. Use the `LoggingEventBus` to see detailed event flow during development
3. Set logging level to `Debug` to see all event activity

## Further Reading

- [Adapter Pattern Event Architecture](./ADAPTER_PATTERN_EVENT_ARCHITECTURE.md)
- [Decoupled Event Architecture Integration Plan](./DECOUPLED_EVENT_ARCHITECTURE_INTEGRATION_PLAN.md)
- [GPU TorchSharp Verification Guide](./GPU_TORCHSHARP_VERIFICATION_GUIDE.md)
