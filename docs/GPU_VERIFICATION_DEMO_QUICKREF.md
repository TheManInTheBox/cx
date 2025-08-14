# GPU Verification Demo Quick Reference

## Run the Demo

```powershell
./run_gpu_verification_demo.ps1
```

## Core Components

1. **Runtime Event Bus** (Aura Sensory Runtime Framework)
   - `CxLanguage.Runtime.Events.ICxEventBus`
   - `CxLanguage.Runtime.Events.LoggingEventBus`

2. **Core Event Bus** (CX Language of Consciousness)
   - `CxLanguage.Core.Events.ICxEventBus`
   - `CxLanguage.Core.Events.CxEventBus`

3. **Adapter** (Bridge)
   - `CxLanguage.AuraSensoryCxLanguageAdapter`

## Key Events

- `gpu.verification.start` - Initiates verification
- `gpu.verification.complete` - Reports results
- `system.any` - Captures all system events

## Dependency Setup

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

## GPU Verification

```csharp
// Check GPU availability
var gpuAvailable = torch.cuda.is_available();

if (gpuAvailable)
{
    // Get GPU info
    var cudaVersion = torch.cuda.get_compiled_version().ToString();
    var deviceName = torch.cuda.get_device_name(torch.cuda.current_device());
    
    // Test tensor operations
    var tensor = torch.randn(new long[] { 3, 3 }).cuda();
    var result = tensor.sum().cpu().ToString();
    
    // Test neural network
    var linearLayer = Linear(10, 5).cuda();
    var input = torch.randn(new long[] { 1, 10 }).cuda();
    var output = linearLayer.forward(input);
}
```

## Event Handling

```csharp
// Register event handler
eventBus.RegisterHandler("gpu.verification.complete", async (eventName, payload, sender) =>
{
    Console.WriteLine($"✅ GPU Verification Complete: {eventName}");
    
    if (payload.TryGetValue("gpuAvailable", out var gpuAvailable) && gpuAvailable is bool gpuAvailableBool)
    {
        Console.WriteLine($"🔍 GPU Available: {gpuAvailableBool}");
        
        if (gpuAvailableBool)
        {
            if (payload.TryGetValue("gpuName", out var gpuName))
            {
                Console.WriteLine($"🖥️ GPU: {gpuName}");
            }
        }
    }
    
    await Task.CompletedTask;
});
```

## Event Bridging

```csharp
// Bridge from Runtime to Core
if (ShouldForwardToCore(eventName))
{
    var corePayload = CreateCorePayload(eventName, payload, sender);
    await _coreEventBus.EmitAsync(eventName, corePayload);
}

// Bridge from Core to Runtime
_coreEventBus.RegisterHandler("system.any", async (coreEvent) =>
{
    var runtimePayload = CreateRuntimePayload(coreEvent);
    await _runtimeEventBus.EmitAsync(coreEvent.EventName, runtimePayload, this);
});
```
