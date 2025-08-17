# CX Language Model Files

This directory contains local LLM model files used by the CX Language platform.

## Current Status: Intelligent Simulation + Real Model Support

The CX Language platform supports both intelligent simulation (for development) and real local LLM inference:

### Current Implementation
- **Intelligent Simulation**: High-quality simulated responses (active by default)
- **Real Model Support**: GGUF model integration with LLamaSharp (in development)
- **Hybrid Approach**: Automatic fallback from real models to simulation

## Model Files (Download Required for Real Inference)

Due to their large size, model files are not included in the git repository. Download them to enable real local inference:

### Recommended Models

1. **Phi-3-mini-4k-instruct (Primary Model)** 🚀
   - File: `phi-3-mini-4k-instruct-q4.gguf`
   - Size: ~2.4GB
   - Download from: [Hugging Face](https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf)
   - Used by: Primary inference, instruction following, consciousness processing
   - **Optimized for**: Fast inference, 4K context, instruction following

2. **Llama 3.2 1B Instruct (Lightweight Alternative)**
   - File: `llama-3.2-1b-instruct-q4_k_m.gguf`
   - Size: ~1GB
   - Download from: [Hugging Face](https://huggingface.co/microsoft/Llama-3.2-1B-Instruct-GGUF)
   - Used by: Lightweight testing, development

3. **Llama 3.2 3B Instruct (Production Alternative)**
   - File: `local_llm/llama-3.2-3b-instruct-q4_k_m.gguf`
   - Size: ~2GB  
   - Download from: [Hugging Face](https://huggingface.co/microsoft/Llama-3.2-3B-Instruct-GGUF)
   - Used by: Production inference with higher quality

## Directory Structure

```
models/
├── README.md (this file)
├── phi-3-mini-4k-instruct-q4.gguf (primary model - download first)
├── llama-3.2-1b-instruct-q4_k_m.gguf (lightweight alternative)
└── local_llm/
    └── llama-3.2-3b-instruct-q4_k_m.gguf (production alternative)
```

## Quick Setup (Download Phi-3 Model)

### PowerShell (Windows)
```powershell
# Navigate to models directory
cd models

# Download Phi-3-mini model using PowerShell (recommended for consciousness processing)
Invoke-WebRequest -Uri "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf/resolve/main/Phi-3-mini-4k-instruct-q4.gguf" -OutFile "phi-3-mini-4k-instruct-q4.gguf"

# Alternative: Use curl.exe directly (if available)
curl.exe -L "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf/resolve/main/Phi-3-mini-4k-instruct-q4.gguf" -o "phi-3-mini-4k-instruct-q4.gguf"

# Test with CX Language
cd ..
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/core_features/gpu_cuda_inference_demo.cx
```

### Linux/macOS
```bash
# Navigate to models directory
cd models

# Download Phi-3-mini model (recommended for consciousness processing)
curl -L "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf/resolve/main/Phi-3-mini-4k-instruct-q4.gguf" -o "phi-3-mini-4k-instruct-q4.gguf"

# Test with CX Language
cd ..
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/core_features/gpu_cuda_inference_demo.cx
```

## Real vs Simulation Mode

- **Simulation Mode**: High-quality predefined responses (no download required)
- **Real Model Mode**: Actual LLM inference with downloaded GGUF models
- **Automatic Detection**: Platform detects available models and switches modes automatically

## Setup Instructions

1. **Primary Model**: Download Phi-3-mini to `models/phi-3-mini-4k-instruct-q4.gguf` (recommended)
2. **Alternative Models**: Download Llama 1B or 3B models as backup options
3. **Test Integration**: Run the GPU-CUDA inference demo to verify setup
4. **Verify Real Models**: Check console output for "Real LLM Mode" vs "Simulation Mode"

## Why Phi-3-mini? 🚀

**Phi-3-mini-4k-instruct** is the optimal choice for consciousness processing:

✅ **Instruction Optimized**: Specifically fine-tuned for following complex instructions
✅ **4K Context Window**: Perfect for consciousness conversation threads
✅ **Fast Inference**: Optimized for quick response times with CUDA
✅ **Microsoft Quality**: Enterprise-grade model from Microsoft Research
✅ **Compact Size**: 2.4GB provides excellent performance/size ratio
✅ **CUDA Optimized**: Excellent GPU acceleration with LlamaSharp backend

## CUDA Acceleration ⚡

✅ **Fully CUDA-Enabled Platform**:
- **LlamaSharp CUDA12 Backend**: GPU-accelerated inference with CUDA 12
- **GPU Layer Offloading**: 32 layers automatically offloaded to GPU
- **TorchSharp Integration**: Additional GPU optimization for consciousness processing
- **Automatic Detection**: Platform detects CUDA availability and switches modes

**Performance with CUDA**:
- Sub-100ms inference latency for consciousness processing
- GPU memory utilization for large context windows
- Real-time token streaming with GPU acceleration
- Automatic fallback to CPU if GPU unavailable

## Troubleshooting Downloads

### PowerShell Download Issues
If you get "parameter not found" errors with `curl`:
```powershell
# PowerShell's curl is an alias - use full command:
Invoke-WebRequest -Uri "https://huggingface.co/..." -OutFile "filename.gguf"

# Or use curl.exe directly:
curl.exe -L "https://huggingface.co/..." -o "filename.gguf"
```

### Large File Downloads
For 1GB+ models, consider:
```powershell
# Enable progress display
$ProgressPreference = 'Continue'
Invoke-WebRequest -Uri "..." -OutFile "..." -UseBasicParsing

# Or download in chunks with resume capability
# Use a download manager like wget or aria2c if available
```

### Verification
After download, verify the model:
```powershell
# Check file size (should be ~2.4GB for Phi-3-mini)
Get-ChildItem "phi-3-mini-4k-instruct-q4.gguf" | Select-Object Name, Length

# Test with CX Language - should show "Real LLM Mode activated" with "Phi-3-mini-4k-instruct"
cd ..
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/core_features/gpu_cuda_inference_demo.cx
```

## Alternative: Git LFS

If you prefer to use Git LFS for model files:

```bash
git lfs track "*.gguf"
git lfs track "models/**"
```

However, for development purposes, downloading models separately is recommended to avoid large repository sizes.
