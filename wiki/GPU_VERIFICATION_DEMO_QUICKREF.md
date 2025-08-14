# GPU Verification Demo - Quick Reference Card

## 🚀 Run Commands

```powershell
# Standard Run (downloads model if needed)
.\scripts\run_gpu_verification_demo.ps1

# Run with custom model path
.\scripts\run_gpu_verification_demo.ps1 -ModelPath "C:\path\to\your\model.gguf"

# Force CPU mode (disable GPU)
.\scripts\run_gpu_verification_demo.ps1 -ForceCpu

# Skip model download
.\scripts\run_gpu_verification_demo.ps1 -SkipModelDownload
```

## 🔍 System Requirements

- **CPU**: 4+ cores recommended
- **RAM**: 8GB+ (16GB+ recommended for larger models)
- **GPU**: NVIDIA with CUDA support (compute capability 3.5+)
- **Disk**: 2GB+ free space for model storage
- **Software**: .NET 9 SDK, CUDA Toolkit 11.8+

## 📈 Performance Benchmarks

| GPU Model       | Tokens/Second | Inference Time | Memory Usage |
|-----------------|---------------|----------------|--------------|
| RTX 3080        | 24.6          | 40.65 ms       | 2.3 GB       |
| RTX 2070 SUPER  | 18.2          | 54.95 ms       | 2.1 GB       |
| GTX 1080 Ti     | 12.8          | 78.13 ms       | 2.0 GB       |
| CPU (12 core)   | 5.2           | 192.31 ms      | 4.2 GB       |

## 🧠 Cognitive Functions Tested

- **is{}**: Cognitive boolean logic
- **not{}**: Negative cognitive boolean logic
- **iam{}**: Self-reflective logic
- **adapt{}**: Consciousness adaptation

## 🔄 Workflow

1. **GPU Detection**: Automatic CUDA-compatible GPU detection
2. **Model Loading**: GGUF model loaded into memory
3. **Text Generation**: Single-response generation test
4. **Text Streaming**: Token-by-token streaming test
5. **Performance Test**: Multi-prompt benchmark with metrics
6. **Cognitive Tests**: Verification of CX cognitive functions
7. **Cleanup**: Model unloading and resource release

## 📝 Output Guide

- **✅**: Success indicators
- **❌**: Error or failure indicators
- **🔔**: Event notifications
- **📦**: Payload data
- **📊**: Performance metrics
- **🧠**: Cognitive function indicators
- **🚀**: Performance test indicators
- **⚡**: Tokens per second metrics
- **⏱️**: Timing information
- **📈**: Utilization metrics
- **💾**: Memory usage information

## 🔧 Common Issues

- **GPU Not Detected**: Update drivers, verify CUDA installation
- **Out of Memory**: Try smaller model or batch size
- **Slow Performance**: Close other GPU-intensive applications
- **Model Loading Fails**: Check file path, download integrity

## 📚 For Detailed Documentation

See the full guide at: `wiki/GPU_VERIFICATION_DEMO_GUIDE.md`
