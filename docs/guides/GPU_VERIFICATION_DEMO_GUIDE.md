# GPU LLM Verification Demo Guide

## Overview

The GPU LLM Verification Demo demonstrates the integration of TorchSharp for GPU-accelerated LLM functionality with a decoupled event architecture. This demo showcases the separation between the Aura Sensory Runtime Framework and CX Language of Consciousness, allowing independent development while maintaining seamless integration.

## Key Features

1. **GPU Acceleration**: Utilizes TorchSharp v0.102.5 with CUDA support for GPU acceleration
2. **Decoupled Event Architecture**: Separates the Aura Sensory Runtime Framework and CX Language of Consciousness event systems
3. **Event Bridging**: Implements an adapter pattern to bridge events between the two systems
4. **Real-Time Verification**: Performs live verification of GPU availability and functionality
5. **Comprehensive Logging**: Captures all events flowing through the system

## Architecture

The demo implements a decoupled event architecture with three main components:

1. **Runtime Event Bus** (Aura Sensory Runtime Framework)
   - Interface: `CxLanguage.Runtime.Events.ICxEventBus`
   - Implementation: `CxLanguage.Runtime.Events.LoggingEventBus`
   - Purpose: Handles hardware-level events and sensory processing

2. **Core Event Bus** (CX Language of Consciousness)
   - Interface: `CxLanguage.Core.Events.ICxEventBus`
   - Implementation: `CxLanguage.Core.Events.CxEventBus`
   - Purpose: Handles high-level consciousness and cognitive processing

3. **Adapter** (Bridge between systems)
   - Class: `CxLanguage.AuraSensoryCxLanguageAdapter`
   - Purpose: Bridges events between the two event buses, providing seamless integration while maintaining separation

## Running the Demo

### Prerequisites

- .NET 9.0 SDK
- CUDA Toolkit (for GPU acceleration)
- Compatible NVIDIA GPU

### Running the Demo

Execute the PowerShell script to run the demo:

```powershell
./run_gpu_verification_demo.ps1
```

Or run manually with:

```powershell
dotnet run --project examples/GpuLlmVerificationDemo.csproj
```

## Expected Output

The demo will:

1. Initialize the decoupled event architecture
2. Register event handlers for GPU verification
3. Check for GPU availability using TorchSharp
4. Create and manipulate a tensor on the GPU
5. Create and test a small neural network on the GPU
6. Report the results through the event system

Example output:

```
==================================================
     CX LANGUAGE - GPU LLM VERIFICATION DEMO
==================================================
Demonstrating decoupled event architecture between
Aura Sensory Runtime Framework and CX Language of Consciousness
==================================================
🚀 Starting GPU LLM Verification Demo
📝 Demonstrating TorchSharp v0.102.5 with CUDA/GPU acceleration
🔗 Creating independence between Aura Sensory Runtime Framework and CX Language of Consciousness
📡 Registering event handlers
▶️ Starting GPU verification
📊 GPU Verification Started: gpu.verification.start
🔍 Checking GPU availability with TorchSharp...
✅ Found GPU: NVIDIA GeForce RTX 3080
⚡ CUDA Version: 12.1
🔢 GPU Count: 1
✅ Successfully created and computed with GPU tensor: -0.3524
✅ Successfully created and computed with GPU neural network
✅ GPU Verification Complete: gpu.verification.complete
🔍 GPU Available: True
🖥️ GPU: NVIDIA GeForce RTX 3080
⚡ CUDA Version: 12.1
🏁 GPU LLM Verification Demo completed
==================================================
Press any key to exit...
```

## Event Flow

1. `gpu.verification.start` - Initiates the GPU verification process
2. `gpu.verification.complete` - Indicates completion of the verification with results
3. Various system events - Captured for logging and monitoring

## Implementation Details

### Adapter Pattern

The `AuraSensoryCxLanguageAdapter` implements the adapter pattern to bridge between the two event systems:

```csharp
public class AuraSensoryCxLanguageAdapter : RuntimeEvents.ICxEventBus
{
    private readonly RuntimeEvents.ICxEventBus _runtimeEventBus;
    private readonly CoreEvents.ICxEventBus _coreEventBus;
    
    // Implementation details...
}
```

### Event Forwarding Logic

The adapter determines which events should be forwarded between systems:

```csharp
private bool ShouldForwardToCore(string eventName)
{
    // All system events go to core
    if (eventName.StartsWith("system."))
        return true;
        
    // User events go to core
    if (eventName.StartsWith("user."))
        return true;
    
    // More rules...
    
    return false;
}
```

### GPU Verification

The demo verifies GPU functionality through TorchSharp:

```csharp
var gpuAvailable = torch.cuda.is_available();

if (gpuAvailable)
{
    var cudaVersion = torch.cuda.get_compiled_version().ToString();
    var deviceCount = torch.cuda.device_count();
    var deviceName = torch.cuda.get_device_name(torch.cuda.current_device());
    
    // Create and manipulate a tensor on GPU
    var tensor = torch.randn(new long[] { 3, 3 }).cuda();
    var result = tensor.sum().cpu().ToString();
    
    // Create a small neural network
    var linearLayer = Linear(10, 5).cuda();
    var input = torch.randn(new long[] { 1, 10 }).cuda();
    var output = linearLayer.forward(input);
}
```

## Development Notes

- The adapter pattern allows independent development of both event systems
- Each team can maintain their own ICxEventBus implementation
- The adapter ensures seamless communication between the systems
- This approach supports the parallel development of both frameworks
- Future enhancements can be made to either system without affecting the other

## Troubleshooting

### No GPU Detected

If no GPU is detected:

1. Verify CUDA installation with `nvidia-smi`
2. Ensure the CUDA version is compatible with TorchSharp
3. Check that the GPU is not being used by other applications
4. Verify that the correct TorchSharp CUDA package is installed

### Event System Issues

If events are not flowing properly:

1. Check event registration in both event buses
2. Verify the adapter is properly forwarding events
3. Enable debug logging for more detailed information
4. Ensure the adapter is properly registered in the dependency injection container

## Next Steps

- Implement additional event patterns for specific use cases
- Enhance the adapter with more sophisticated routing logic
- Add performance monitoring for event processing
- Integrate with the broader CX Language framework
- Develop more comprehensive GPU acceleration features
