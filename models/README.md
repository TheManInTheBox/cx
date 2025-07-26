# CX Language Model Files

This directory contains local LLM model files used by the CX Language platform.

## Model Files (Not Included in Git)

Due to their large size, model files are not included in the git repository. You'll need to download them separately:

### Required Models

1. **Llama 3.2 3B Instruct (Q4_K_M)**
   - File: `local_llm/llama-3.2-3b-instruct-q4_k_m.gguf`
   - Size: ~2GB
   - Download from: [Hugging Face](https://huggingface.co/microsoft/Llama-3.2-3B-Instruct-GGUF)
   - Used by: NativeGGUFInferenceEngine for local inference

2. **Llama 3.2 1B Instruct (Test Model)**
   - File: `llama-3.2-1b-instruct-test.gguf`
   - Size: ~1GB
   - Download from: [Hugging Face](https://huggingface.co/microsoft/Llama-3.2-1B-Instruct-GGUF)
   - Used by: Development and testing

## Directory Structure

```
models/
├── README.md (this file)
├── llama-3.2-1b-instruct-test.gguf (download separately)
└── local_llm/
    └── llama-3.2-3b-instruct-q4_k_m.gguf (download separately)
```

## Setup Instructions

1. Create the `local_llm` directory if it doesn't exist
2. Download the required GGUF model files from Hugging Face
3. Place them in the appropriate directories as shown above
4. The CX Language platform will automatically detect and use these models

## Alternative: Git LFS

If you prefer to use Git LFS for model files:

```bash
git lfs track "*.gguf"
git lfs track "models/**"
```

However, for development purposes, downloading models separately is recommended to avoid large repository sizes.
