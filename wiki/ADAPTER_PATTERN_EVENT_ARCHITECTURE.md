# Adapter Pattern Implementation for Event Architecture

## Overview

This document outlines the implementation of the Adapter Pattern to create independence between the Aura Sensory Runtime Framework and CX Language of Consciousness. This architectural pattern allows these two systems to operate with different event interfaces while maintaining seamless communication.

## Problem Statement

The current CX Language codebase has two separate event systems with conflicting interfaces:

1. **Runtime Events** (`CxLanguage.Runtime.Events.ICxEventBus`)
   - Focused on hardware/sensory processing
   - Includes sender information in event payloads
   - Different method signatures for handlers

2. **Core Events** (`CxLanguage.Core.Events.ICxEventBus`)
   - Focused on consciousness/cognitive processing
   - Simplified event payload structure
   - Different method signatures for handlers

This dual interface creates confusion, coupling, and maintenance challenges.

## Solution: Adapter Pattern

The Adapter Pattern allows these two systems to operate independently while maintaining communication:

![Adapter Pattern Diagram](https://via.placeholder.com/800x400?text=Adapter+Pattern+Diagram)

### Key Components

1. **Core Event Interface** - For consciousness processing
2. **Runtime Event Interface** - For sensory/hardware processing
3. **Adapter** - Bridges between the two systems
4. **Event Forwarding Logic** - Determines which events cross the boundary

## Implementation Details

### 1. Core Event Interface

```csharp
namespace CxLanguage.Core.Events
{
    public interface ICxEventBus
    {
        Task<bool> EmitAsync(string eventName, IDictionary<string, object> payload = null);
        string RegisterHandler(string eventName, Func<CxEventPayload, Task> handler);
        bool UnregisterHandler(string subscriptionId);
    }

    public class CxEventPayload
    {
        public string EventName { get; set; }
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
```

### 2. Runtime Event Interface

```csharp
namespace CxLanguage.Runtime.Events
{
    public interface ICxEventBus
    {
        Task<bool> EmitAsync(string eventName, IDictionary<string, object> payload = null, object sender = null);
        string RegisterHandler(string eventName, Func<string, IDictionary<string, object>, object, Task> handler);
        bool UnregisterHandler(string subscriptionId);
    }
}
```

### 3. Adapter Implementation

```csharp
public class AuraSensoryCxLanguageAdapter : RuntimeEvents.ICxEventBus
{
    private readonly RuntimeEvents.ICxEventBus _runtimeEventBus;
    private readonly CoreEvents.ICxEventBus _coreEventBus;
    private readonly ILogger<AuraSensoryCxLanguageAdapter> _logger;

    public AuraSensoryCxLanguageAdapter(
        RuntimeEvents.ICxEventBus runtimeEventBus,
        CoreEvents.ICxEventBus coreEventBus,
        ILogger<AuraSensoryCxLanguageAdapter> logger)
    {
        _runtimeEventBus = runtimeEventBus;
        _coreEventBus = coreEventBus;
        _logger = logger;
    }

    public async Task<bool> EmitAsync(string eventName, IDictionary<string, object> payload = null, object sender = null)
    {
        try
        {
            _logger.LogDebug($"🔄 Bridging event: {eventName}");
            
            // Forward to runtime event bus
            var runtimeTask = _runtimeEventBus.EmitAsync(eventName, payload, sender);
            
            // Also emit to the core event bus if applicable
            if (ShouldForwardToCore(eventName))
            {
                var corePayload = CreateCorePayload(eventName, payload, sender);
                await _coreEventBus.EmitAsync(eventName, corePayload);
            }
            
            // Return result from runtime event bus
            return await runtimeTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error bridging event {eventName}");
            return false; false;
        }
    }

    public string RegisterHandler(string eventName, Func<string, IDictionary<string, object>, object, Task> handler)
    {
        // Register with the runtime event bus
        var subscriptionId = _runtimeEventBus.RegisterHandler(eventName, handler);
        
        // Also register with the core event bus if applicable
        if (ShouldForwardToCore(eventName))
        {
            _coreEventBus.RegisterHandler(eventName, async (corePayload) => {
                // Convert core payload back to runtime format and call the handler
                await handler(corePayload.EventName, corePayload.Data, null);
            });
        }
        
        return subscriptionId;
    }

    public bool UnregisterHandler(string subscriptionId)
    {
        // Only unregister from runtime bus for now
        // Core bus handlers are managed separately
        return _runtimeEventBus.UnregisterHandler(subscriptionId);
    }

    private bool ShouldForwardToCore(string eventName)
    {
        // Logic to determine which events should be forwarded to core
        // Could be based on event name prefix, category, etc.
        return eventName.StartsWith("core.") || 
               eventName.StartsWith("inference.") || 
               eventName.StartsWith("gpu.");
    }

    private IDictionary<string, object> CreateCorePayload(
        string eventName, 
        IDictionary<string, object> payload, 
        object sender)
    {
        var result = new Dictionary<string, object>(payload ?? new Dictionary<string, object>());
        
        // Add sender information if available
        if (sender != null)
        {
            result["sender"] = sender.ToString();
        }
        
        return result;
    }
}
```

### 4. Dependency Injection Registration

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

## Event Flow Patterns

### Runtime to Core

1. Event emitted through Runtime `ICxEventBus` interface
2. Adapter receives the event
3. Adapter determines if event should be forwarded to Core
4. If yes, Adapter converts payload and emits to Core event bus
5. Core event handlers receive the event

### Core to Runtime

1. Event emitted through Core `ICxEventBus` interface
2. Core event handlers receive the event
3. If a Runtime handler was registered for this event via the adapter, it will be called
4. The adapter converts the Core payload back to Runtime format
5. Runtime handler receives the event

## Benefits

1. **Decoupling** - Systems can evolve independently
2. **Clear Boundaries** - Explicit interfaces for each system
3. **Simplified Development** - Teams can work on their respective systems
4. **Improved Testing** - Each system can be tested in isolation
5. **Selective Propagation** - Only relevant events cross the boundary
6. **Maintainability** - Easier to understand and maintain

## Integration with GPU Verification

The adapter pattern is demonstrated in the GPU verification demo, which:

1. Sets up both event systems with their respective interfaces
2. Creates an adapter to bridge the systems
3. Registers handlers in both systems
4. Checks GPU availability using TorchSharp
5. Emits events through the adapter
6. Shows events propagating through both systems

## Next Steps

1. Update existing code to use the appropriate interfaces
2. Implement the adapter in the main codebase
3. Review event forwarding logic for specific use cases
4. Add tests for the adapter pattern
5. Document the new architecture for all teams
