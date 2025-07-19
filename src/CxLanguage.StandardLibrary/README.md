# CX Language Standard Library

This standard library provides modular AI services for the CX programming language, replacing built-in AI functions with importable, extensible modules powered by Microsoft Semantic Kernel.

## Architecture Overview

### Core Components
- **ICxAiService**: Base interface for all AI services
- **CxAiServiceBase**: Abstract base class with common functionality
- **ServiceHealthStatus**: Health monitoring for all services
- **Microsoft Semantic Kernel 1.26.0**: Core AI orchestration framework

### Design Principles
- **Modular**: Each AI capability is a separate importable service
- **Consistent**: All services follow the same interface patterns
- **Extensible**: Easy to add new AI capabilities
- **Observable**: Built-in logging, telemetry, and health monitoring
- **Async-First**: All operations support async/await patterns

## Available AI Services

### ✅ 1. TextGenerationService
**Location**: `AI/TextGeneration/TextGenerationService.cs`
**Purpose**: General text generation and completion

**Key Methods**:
- `GenerateTextAsync(prompt, options)` - Generate text from prompt
- `GenerateStreamAsync(prompt, options)` - Streaming text generation
- `CompleteTextAsync(text, options)` - Text completion
- `GenerateBatchAsync(prompts, options)` - Batch processing

**Features**:
- Streaming and non-streaming generation
- Batch processing capabilities
- Temperature and creativity controls
- Token limit management
- Stop sequences and formatting options

### ✅ 2. ChatCompletionService
**Location**: `AI/ChatCompletion/ChatCompletionService.cs`
**Purpose**: Conversational AI with context management

**Key Methods**:
- `CompleteAsync(messages, options)` - Single chat completion
- `CompleteStreamAsync(messages, options)` - Streaming chat
- `ContinueConversationAsync(conversationId, message, options)` - Context-aware chat
- `StartConversationAsync(systemPrompt, options)` - New conversation

**Features**:
- Multi-turn conversation management
- System message support
- Function calling capabilities
- Conversation persistence
- Context length management

### ✅ 3. TextEmbeddingsService
**Location**: `AI/TextEmbeddings/TextEmbeddingsService.cs`
**Purpose**: Text embeddings and semantic search

**Key Methods**:
- `GenerateEmbeddingAsync(text, options)` - Single text embedding
- `GenerateBatchEmbeddingsAsync(texts, options)` - Batch embeddings
- `ComputeSimilarityAsync(embedding1, embedding2)` - Similarity calculation
- `FindSimilarAsync(query, embeddings, options)` - Semantic search

**Features**:
- Vector embeddings generation
- Similarity scoring and ranking
- Batch processing for efficiency
- Semantic search capabilities
- Multiple embedding model support

### ✅ 4. TextToImageService
**Location**: `AI/TextToImage/TextToImageService.cs`
**Purpose**: Image generation from text descriptions

**Key Methods**:
- `GenerateImageAsync(prompt, options)` - Single image generation
- `GenerateVariationsAsync(prompt, count, options)` - Multiple variations
- `EditImageAsync(imageUrl, prompt, options)` - Image editing
- `GenerateBatchAsync(prompts, options)` - Batch generation

**Features**:
- Multiple image sizes and formats
- Style and quality controls
- Image variations and editing
- Batch processing capabilities
- Custom aspect ratios and resolutions

### ✅ 5. ImageToTextService
**Location**: `AI/ImageToText/ImageToTextService.cs`
**Purpose**: Image analysis and description

**Key Methods**:
- `AnalyzeImageAsync(imageUrl, options)` - Detailed image analysis
- `AnalyzeImageDataAsync(imageData, mimeType, options)` - Binary data analysis
- `ExtractTextAsync(imageUrl, options)` - OCR text extraction
- `ClassifyImageAsync(imageUrl, categories, options)` - Image classification

**Features**:
- Comprehensive image analysis
- OCR text extraction
- Image classification
- Support for URLs and binary data
- Confidence scoring

### ✅ 6. TextToAudioService
**Location**: `AI/TextToAudio/TextToAudioService.cs`
**Purpose**: Audio generation from text

**Key Methods**:
- `GenerateAudioAsync(text, options)` - Text-to-audio conversion
- `GenerateWithExpressionAsync(text, expression, options)` - Expressive audio
- `GenerateBatchAsync(texts, options)` - Batch audio generation
- `GenerateStreamAsync(text, options)` - Streaming audio

**Features**:
- Multiple audio formats (WAV, MP3, OGG)
- Voice selection and customization
- Expression and emotion controls
- SSML support for advanced synthesis
- Batch and streaming processing

### ✅ 7. TextToSpeechService
**Location**: `AI/TextToSpeech/TextToSpeechService.cs`
**Purpose**: Advanced speech synthesis with zero-file MP3 streaming playback

**Key Methods**:
- `SpeakAsync(text, options)` - Pure memory MP3 streaming with NAudio playback
- `SynthesizeAsync(text, options)` - Basic speech synthesis (returns audio data)
- `SynthesizeStreamAsync(text, options)` - Streaming synthesis
- `SynthesizeConversationalAsync(text, context, options)` - Context-aware speech

**Features**:
- **Zero-File MP3 Streaming**: Direct memory-based audio playback using NAudio
- Real-time speech synthesis and immediate playback
- 70% smaller file sizes with MP3 format (vs WAV)
- Professional audio engine integration (WaveOutEvent)
- Conversational context adaptation
- Prosody and emotion controls
- Multiple voice models and SSML support

### ✅ 8. AudioToTextService
**Location**: `AI/AudioToText/AudioToTextService.cs`
**Purpose**: Audio transcription and analysis

**Key Methods**:
- `TranscribeAsync(audioData, options)` - Audio transcription
- `TranscribeWithSpeakersAsync(audioData, options)` - Speaker diarization
- `TranscribeStreamAsync(audioStream, options)` - Real-time transcription
- `AnalyzeAudioAsync(audioData, options)` - Audio feature analysis

**Features**:
- Multi-format audio support
- Speaker identification and diarization
- Real-time streaming transcription
- Audio feature extraction
- Language detection and confidence scoring

### ✅ 9. RealtimeService
**Location**: `AI/Realtime/RealtimeService.cs`
**Purpose**: Real-time AI interactions and streaming

**Key Methods**:
- `StartSessionAsync(options)` - Start real-time session
- `SendMessageStreamAsync(sessionId, message, options)` - Streaming messages
- `ProcessAudioStreamAsync(sessionId, audioStream, options)` - Real-time audio
- `CallFunctionAsync(sessionId, function, parameters, options)` - Function calling
- `CloseSessionAsync(sessionId)` - Close session

**Features**:
- Low-latency real-time communication
- Session management and persistence
- Real-time audio processing
- Function calling capabilities
- Connection health monitoring

## Usage Examples

### Import Syntax (Future)
```cx
// Import AI services
using TextGeneration from "cx.ai.textgeneration";
using ChatCompletion from "cx.ai.chatcompletion";
using TextEmbeddings from "cx.ai.textembeddings";
using TextToImage from "cx.ai.texttoimage";
using ImageToText from "cx.ai.imagetotext";
using TextToAudio from "cx.ai.texttoaudio";
using TextToSpeech from "cx.ai.texttospeech";
using AudioToText from "cx.ai.audiototext";
using Realtime from "cx.ai.realtime";
```

### Service Usage Pattern
```cx
// Create service instance
var textGen = new TextGeneration();

// Configure options
var options = {
    maxTokens: 100,
    temperature: 0.7,
    model: "gpt-4"
};

// Generate text
var result = await textGen.GenerateTextAsync("Write a story about", options);

if (result.IsSuccess)
{
    print(result.Text);
}
else
{
    print("Error: " + result.ErrorMessage);
}
```

## Project Structure

```
CxLanguage.StandardLibrary/
├── CxLanguage.StandardLibrary.csproj
├── Core/
│   └── ICxAiService.cs              # Base interfaces and classes
├── AI/
│   ├── TextGeneration/
│   │   └── TextGenerationService.cs
│   ├── ChatCompletion/
│   │   └── ChatCompletionService.cs
│   ├── TextEmbeddings/
│   │   └── TextEmbeddingsService.cs
│   ├── TextToImage/
│   │   └── TextToImageService.cs
│   ├── ImageToText/
│   │   └── ImageToTextService.cs
│   ├── TextToAudio/
│   │   └── TextToAudioService.cs
│   ├── TextToSpeech/
│   │   └── TextToSpeechService.cs
│   ├── AudioToText/
│   │   └── AudioToTextService.cs
│   └── Realtime/
│       └── RealtimeService.cs
└── README.md                        # This file
```

## Dependencies

- **.NET 8**: Target framework
- **Microsoft.SemanticKernel (1.26.0)**: Core AI orchestration
- **Microsoft.Extensions.Logging**: Logging infrastructure
- **Microsoft.Extensions.DependencyInjection**: Service container
- **CxLanguage.Core**: Core language runtime
- **CxLanguage.Runtime**: Language execution engine

## Integration Status

### ✅ Completed
- All 9 AI service implementations
- Consistent interface pattern
- Comprehensive feature sets
- Error handling and logging
- Health monitoring
- Async/await support

### 🔄 Next Steps
1. **Grammar Integration**: Update CX grammar to support import system
2. **Compiler Integration**: Implement service resolution and binding
3. **Runtime Integration**: Service instantiation and lifecycle management
4. **Testing**: Create comprehensive test suites
5. **Documentation**: API reference and usage examples

## Key Features

### Consistency
- All services implement `ICxAiService`
- Common patterns for options, results, and error handling
- Consistent async/await patterns
- Uniform logging and telemetry

### Extensibility
- Easy to add new AI services
- Pluggable architecture with Semantic Kernel
- Configurable service options
- Support for multiple AI providers

### Performance
- Async-first design
- Streaming capabilities where applicable
- Batch processing support
- Connection pooling and resource management

### Reliability
- Comprehensive error handling
- Health monitoring and diagnostics
- Timeout and retry mechanisms
- Resource cleanup and disposal

## Migration from Built-in Functions

### Before (Built-in Functions)
```cx
var result = task("Generate a story");
var embedding = embed("some text");
var response = reason("solve this problem");
```

### After (Standard Library)
```cx
using TextGeneration from "cx.ai.textgeneration";
using TextEmbeddings from "cx.ai.textembeddings";

var textGen = new TextGeneration();
var embeddings = new TextEmbeddings();

var story = await textGen.GenerateTextAsync("Generate a story");
var embedding = await embeddings.GenerateEmbeddingAsync("some text");
```

## Configuration

Services can be configured through options objects and dependency injection:

```cx
// Service-specific configuration
var options = new TextGenerationOptions()
{
    Model = "gpt-4",
    MaxTokens = 500,
    Temperature = 0.8,
    EnableStreaming = true
};

// Global service configuration
var config = new CxAiConfiguration()
{
    ApiKey = "your-api-key",
    Endpoint = "https://your-endpoint.com",
    DefaultTimeout = TimeSpan.FromSeconds(30)
};
```

This standard library provides a solid foundation for AI-native programming in CX, with comprehensive capabilities across text, image, audio, and real-time domains.
