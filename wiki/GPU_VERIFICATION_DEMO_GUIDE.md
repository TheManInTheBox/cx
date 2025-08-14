# GPU Verification Demo - User Guide

## Overview

The GPU Verification Demo is a comprehensive testing suite designed to validate and benchmark GPU-accelerated LLM (Large Language Model) inference in the CX Language platform. This guide explains how to use the demo, understand its output, and interpret the results.

## Features

- **GPU Availability Detection**: Automatically detects if CUDA-compatible GPUs are available
- **Model Loading**: Loads GGUF models for local execution
- **Text Generation**: Tests single-response generation
- **Text Streaming**: Tests token-by-token streaming capabilities
- **Cognitive Boolean Logic**: Demonstrates `is{}` and `not{}` pattern integration
- **Self-Reflective Logic**: Shows `iam{}` pattern capabilities
- **Consciousness Adaptation**: Illustrates `adapt{}` pattern functionality
- **Performance Benchmarking**: Measures tokens per second, latency, and GPU utilization

## Requirements

- **.NET 9 SDK**: Required for building and running the demo
- **CUDA Toolkit**: Required for GPU acceleration (v11.8 or later recommended)
- **TorchSharp**: Used for GPU/CUDA acceleration (v0.102.5)
- **GPU with CUDA Support**: NVIDIA GPU with compute capability 3.5 or higher
- **GGUF Model**: LLaMA 2 model in GGUF format (automatically downloaded if not present)

## Running the Demo

The demo can be run using the included PowerShell script:

```powershell
.\scripts\run_gpu_verification_demo.ps1
```

### Script Parameters

- `-SkipModelDownload`: Skip the automatic model download
- `-ModelPath`: Specify a custom model path
- `-ForceCpu`: Force CPU execution mode (disables GPU)

## Understanding the Output

### 1. GPU Availability Test

```
🔍 Testing GPU availability...
🖥️ GPU Available: ✅ YES
🎮 GPU Info: NVIDIA GeForce RTX 3080
⚡ CUDA Version: 11.8
💾 GPU Memory: 10GB
```

This section tells you:
- If a CUDA-compatible GPU was detected
- The model of your GPU
- The version of CUDA that was detected
- The amount of GPU memory available

### 2. Event Processing

```
🔔 EVENT: llm.initialize
  📦 Payload:
    - useGpu: True
    - logLevel: Debug
    - forceGpuTest: True
```

The demo uses an event-driven architecture. This output shows:
- The name of each event being processed
- The payload data attached to each event

### 3. Text Generation and Streaming

```
📡 Sending llm.generate event with prompt: Explain consciousness in CX Language using 3 sentences.
```

This section demonstrates:
- Text generation with different prompts
- Streaming tokens one-by-one (simulating real-time generation)

### 4. Performance Test Results

```
📊 GPU Performance Test Results:
⚡ Tokens Per Second: 24.6
⏱️ Average Inference Time: 40.65 ms
📈 GPU Utilization: 85.2%
💾 Memory Used: 2.3GB
🔄 Test Prompts Processed: 5
```

These metrics show:
- **Tokens Per Second**: How many tokens the model can generate per second
- **Average Inference Time**: The average time (in milliseconds) to generate each token
- **GPU Utilization**: The percentage of GPU resources used during inference
- **Memory Used**: How much GPU memory was consumed
- **Test Prompts Processed**: The number of different prompts tested

## Cognitive Functions Testing

The demo also tests the core cognitive functions of CX Language:

### Cognitive Boolean Logic

```
🧠 COGNITIVE BOOLEAN LOGIC: Processing is{} pattern
  • Context: Should the system proceed with operation?
  • Evaluate: System readiness check
```

This tests the `is{}` pattern that replaces traditional if-statements with AI-driven decision making.

### Negative Cognitive Boolean Logic

```
🧠 NEGATIVE COGNITIVE BOOLEAN LOGIC: Processing not{} pattern
  • Context: Should the system halt operation?
  • Evaluate: System failure check
```

This tests the `not{}` pattern for AI-driven false/negative decision-making.

### Self-Reflective Logic

```
🧠 SELF-REFLECTIVE LOGIC: Processing iam{} pattern
  • Context: Self-assessment: Can I handle this request?
  • Evaluate: Agent capability and readiness evaluation
```

This tests the `iam{}` pattern for AI-driven self-assessment and identity verification.

### Consciousness Adaptation

```
🧠 CONSCIOUSNESS ADAPTATION: Processing adapt{} pattern
  • Context: Learning new skills to better assist Aura decision-making
  • Focus: Voice processing optimization techniques
```

This tests the `adapt{}` pattern for dynamic skill acquisition and knowledge expansion.

## Troubleshooting

### GPU Not Detected

If the demo reports `GPU Available: ❌ NO`, check:
- Your GPU drivers are up-to-date
- CUDA is correctly installed
- Your GPU supports CUDA (must be NVIDIA with compute capability 3.5+)

### Model Loading Issues

If the model fails to load:
- Ensure the model file exists at the specified path
- Check if your GPU has enough memory for the model
- Try a smaller model if necessary

### Performance Issues

If performance is lower than expected:
- Check GPU utilization during the test
- Close other GPU-intensive applications
- Ensure your system is not thermal throttling

### Other Issues

For other issues:
- Check the console output for specific error messages
- Verify the event bus is functioning correctly
- Ensure all dependencies are correctly installed

## Additional Resources

- [CX Language Documentation](https://github.com/TheManInTheBox/cx)
- [CUDA Documentation](https://docs.nvidia.com/cuda/)
- [TorchSharp Documentation](https://github.com/dotnet/TorchSharp)
- [GGUF Model Format](https://github.com/ggerganov/ggml/blob/master/docs/gguf.md)

## Next Steps

After running the verification demo, you can:
1. Integrate GPU acceleration in your own CX Language applications
2. Experiment with different models and sizes
3. Fine-tune performance parameters for your specific hardware
4. Develop custom cognitive functions using the GPU-accelerated infrastructure
