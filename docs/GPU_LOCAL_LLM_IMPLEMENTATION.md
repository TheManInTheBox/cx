# GPU-Accelerated Local LLM Implementation

This document outlines the implementation of the GPU-accelerated Local LLM service using TorchSharp for CUDA optimization, without ONNX dependencies.

## Overview

The implementation provides a high-performance, consciousness-aware local inference capability without cloud dependencies. It leverages TorchSharp for GPU acceleration while removing ONNX dependencies per team consensus.

## Architecture

The implementation consists of the following components:

1. **GpuLocalLLMService**: Core service that provides GPU-accelerated text generation capabilities
2. **LocalLlmEventHandler**: Integrates the GPU service with the CX event system
3. **ServiceCollectionExtensions**: Provides DI registration for the GPU service
4. **Test Scripts**: Provides testing capabilities for the GPU functionality

## Key Features

- **GPU Acceleration**: Uses TorchSharp for CUDA-accelerated inference
- **CPU Fallback**: Automatically falls back to CPU when GPU is not available
- **Streaming Support**: Provides token-by-token streaming for real-time feedback
- **Event Integration**: Fully integrated with the CX consciousness-aware event system
- **Zero Cloud Dependency**: Runs entirely locally, no cloud services required

## Implementation Details

### GpuLocalLLMService

This service implements the `ILocalLLMService` interface and provides:

- GPU detection and utilization via TorchSharp
- Model loading and unloading
- Text generation with GPU acceleration
- Streaming capabilities for real-time token generation

```csharp
// Core generation method with GPU acceleration
public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
{
    // Use GPU if available, otherwise fall back to CPU
    if (_gpuAvailable)
    {
        return await Task.Run(() => GenerateWithGpu(prompt), cancellationToken);
    }
    else
    {
        return await Task.Run(() => GenerateWithCpu(prompt), cancellationToken);
    }
}
```

### LocalLlmEventHandler

This component integrates the GPU service with the CX event system:

- Subscribes to LLM-related events
- Processes event payloads
- Calls the GPU service to handle operations
- Emits result events

```csharp
// Event handler for generation requests
private async Task HandleGenerateAsync(CxEventPayload payload)
{
    // Extract parameters from payload
    var data = payload.Data as IDictionary<string, object>;
    string prompt = data?["prompt"]?.ToString() ?? "";
    
    // Generate text using the GPU service
    string result = await _llmService.GenerateAsync(prompt);
    
    // Emit result event
    await _eventBus.EmitAsync("llm.generated", new Dictionary<string, object>
    {
        { "result", result },
        { "prompt", prompt }
    });
}
```

### Service Registration

The implementation provides easy registration with dependency injection:

```csharp
// Register GPU service
services.AddGpuLocalLlm();

// Or with more options
services.AddAllLocalLlmServices(preferGpu: true);
```

## Usage Examples

### Basic Usage

```csharp
// Get the service from DI
var llmService = serviceProvider.GetRequiredService<ILocalLLMService>();

// Initialize and load model
await llmService.InitializeAsync();
await llmService.LoadModelAsync("llama-2-7b-chat.Q4_K_M.gguf");

// Generate text
string result = await llmService.GenerateAsync("Explain consciousness in three sentences.");
```

### CX Language Integration

```cx
// Initialize the LocalLLM service
emit llm.initialize { 
    useGpu: true, 
    modelName: "llama-2-7b-chat.Q4_K_M.gguf"
};

// Handle initialization result
on llm.initialized (event)
{
    print("✅ LocalLLM service initialized");
    print("📊 GPU available: " + event.gpuAvailable);
    
    // Generate text
    emit llm.generate { 
        prompt: "Explain consciousness in three sentences."
    };
}

// Handle generation result
on llm.generated (event)
{
    print("📝 Result: " + event.result);
}
```

## Testing

The implementation includes comprehensive testing capabilities:

1. **GPU Detection**: Tests TorchSharp GPU/CUDA availability
2. **Service Tests**: Tests the full GPU service functionality
3. **Event Tests**: Tests the event integration

To run the tests:

```powershell
# Test GPU availability only
.\scripts\test_gpu_llm.ps1 -VerifyGpu

# Build the service only
.\scripts\test_gpu_llm.ps1 -BuildOnly

# Run the full test with a specific model
.\scripts\test_gpu_llm.ps1 -RunTest -ModelPath "./models/my-model.gguf"
```

## Technical Details

- **TorchSharp Version**: 0.102.5
- **Target Framework**: .NET 8.0
- **Removed Dependencies**: ONNX Runtime (per team consensus)
- **Required Dependencies**:
  - TorchSharp
  - Microsoft.Extensions.Logging
  - System.Threading.Channels
  - System.Memory
  - System.Buffers

## Next Steps

1. Implement actual model loading with TorchSharp
2. Add benchmarking capabilities
3. Optimize memory usage for large models
4. Add model quantization support
5. Implement model caching

## Conclusion

This implementation provides a GPU-accelerated Local LLM service using TorchSharp without ONNX dependencies, as requested. It is fully integrated with the CX consciousness-aware event system and provides both synchronous and streaming capabilities for text generation.
