# CX Language Decoupled Event Architecture

## Overview

This project implements a decoupled event architecture for the CX Language framework, creating independence between the Aura Sensory Runtime Framework and CX Language of Consciousness while maintaining seamless communication.

## Key Components

### 1. Decoupled Event Interfaces

- **Runtime Events** (`CxLanguage.Runtime.Events.ICxEventBus`): For sensory/hardware processing
- **Core Events** (`CxLanguage.Core.Events.ICxEventBus`): For consciousness/cognitive processing

### 2. Adapter Pattern

The `AuraSensoryCxLanguageAdapter` bridges between the two event systems, implementing the adapter pattern to translate events and ensure they reach the appropriate handlers.

### 3. GPU Verification Demo

A demo application showcases the decoupled architecture while verifying GPU/CUDA capabilities using TorchSharp.

## Implementation Details

### Runtime Event Interface

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

### Core Event Interface

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

### Adapter Implementation

The adapter implements the Runtime interface and bridges events to the Core interface when appropriate:

```csharp
public class AuraSensoryCxLanguageAdapter : RuntimeEvents.ICxEventBus
{
    private readonly RuntimeEvents.ICxEventBus _runtimeEventBus;
    private readonly CoreEvents.ICxEventBus _coreEventBus;
    private readonly ILogger<AuraSensoryCxLanguageAdapter> _logger;

    // Constructor and methods...
    
    private bool ShouldForwardToCore(string eventName)
    {
        // Logic to determine which events should be forwarded to core
        return eventName.StartsWith("core.") || 
               eventName.StartsWith("inference.") || 
               eventName.StartsWith("gpu.");
    }
}
```

## GPU Verification Demo

The demo application shows:

1. Setting up the decoupled event architecture
2. Registering handlers in both systems
3. Checking GPU availability using TorchSharp
4. Emitting events through the adapter
5. Observing events propagating through both systems

### Running the Demo

```powershell
# Run from the root of the repository
.\run_gpu_verification_demo.ps1
```

## Documentation

- [Decoupled Event Architecture Integration Plan](./DECOUPLED_EVENT_ARCHITECTURE_INTEGRATION_PLAN.md) - Steps to integrate into the main codebase
- [Adapter Pattern Event Architecture](./ADAPTER_PATTERN_EVENT_ARCHITECTURE.md) - Details of the adapter pattern implementation
- [GPU TorchSharp Verification Guide](./GPU_TORCHSHARP_VERIFICATION_GUIDE.md) - Guide to the GPU verification demo

## Benefits

1. **Independence**: The Aura Sensory Runtime Framework and CX Language of Consciousness can evolve independently
2. **Clear Boundaries**: Each system has its own interface and implementation
3. **Event Bridging**: Events can flow between systems when needed
4. **Maintainability**: Simpler maintenance with decoupled systems
5. **Team Coordination**: Different teams can work on different parts of the system

## Integration Plan

See [Decoupled Event Architecture Integration Plan](./DECOUPLED_EVENT_ARCHITECTURE_INTEGRATION_PLAN.md) for detailed steps to integrate this approach into the main CX Language codebase.
