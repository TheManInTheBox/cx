# AI Services Overview

## Introduction
Cx provides **6 production-ready AI services** through seamless integration with Microsoft Semantic Kernel and Azure OpenAI. All services are accessible via natural Cx syntax with automatic parameter marshalling and comprehensive error handling.

## Available AI Services

### 1. TextGeneration Service
**Location**: `Cx.AI.TextGeneration`  
**Purpose**: GPT-powered text generation, content creation, and structured responses

#### Key Methods
- `GenerateAsync(prompt)` - Basic text generation
- `GenerateAsync(prompt, options)` - Advanced generation with configuration

#### Configuration Options
```cx
{
    temperature: 0.7,      // Creativity (0.0-2.0)
    maxTokens: 500,        // Maximum response length
    topP: 1.0,            // Nucleus sampling
    frequencyPenalty: 0.0, // Repetition penalty
    presencePenalty: 0.0   // Topic diversity
}
```

#### Examples
```cx
using textGen from "Cx.AI.TextGeneration";

// Basic text generation
var story = textGen.GenerateAsync("Write a short story about a robot learning to paint");

// Structured responses for autonomous systems
var sentiment = textGen.GenerateAsync(
    "Rate sentiment 1-10 (1=negative, 10=positive). Number only:",
    userMessage
);

// Advanced configuration
var creative = textGen.GenerateAsync("Write a creative product description", {
    temperature: 0.9,
    maxTokens: 200,
    presencePenalty: 0.5
});
```

### 2. ChatCompletion Service  
**Location**: `Cx.AI.ChatCompletion`  
**Purpose**: Multi-turn conversations and contextual dialogue

#### Key Methods
- `SendMessageAsync(message)` - Single message
- `SendMessageAsync(message, options)` - Configured conversation

#### Examples
```cx
using chatBot from "Cx.AI.ChatCompletion";

// Single conversation turn
var response = chatBot.SendMessageAsync("Explain quantum computing simply");

// Contextual conversation with configuration
var answer = chatBot.SendMessageAsync("How does that relate to AI?", {
    temperature: 0.5,
    maxTokens: 300
});
```

### 3. TextEmbeddings Service
**Location**: `Cx.AI.TextEmbeddings`  
**Purpose**: Semantic similarity, text classification, and vector operations

#### Key Methods
- `GenerateEmbeddingAsync(text)` - Generate 1536-dimensional embedding
- `CalculateSimilarity(embedding1, embedding2)` - Cosine similarity (0.0-1.0)
- `FindMostSimilar(target, candidates)` - Best match from set

#### Examples
```cx
using embeddings from "Cx.AI.TextEmbeddings";

// Generate embeddings
var userEmbedding = embeddings.GenerateEmbeddingAsync(userMessage);
var urgentEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help critical");

// Calculate semantic similarity
var similarity = embeddings.CalculateSimilarity(userEmbedding, urgentEmbedding);

// Autonomous decision based on similarity
when (similarity > 0.8)
{
    emit "priority.urgent", { message: userMessage, score: similarity };
}

// Find best matching category
var categories = [
    embeddings.GenerateEmbeddingAsync("technical programming code"),
    embeddings.GenerateEmbeddingAsync("sales pricing purchase"),
    embeddings.GenerateEmbeddingAsync("support help assistance")
];

var bestMatch = embeddings.FindMostSimilar(userEmbedding, categories);
```

### 4. VectorDatabase Service
**Location**: `Cx.AI.VectorDatabase`  
**Purpose**: RAG workflows, knowledge management, and contextual search

#### Key Methods
- `IngestTextAsync(text)` - Add text to knowledge base
- `SearchAsync(query, options)` - Semantic search with relevance scores
- `AskAsync(question)` - RAG-style question answering
- `GetStatsAsync()` - Database statistics and health

#### Search Options
```cx
{
    maxResults: 10,    // Maximum search results
    threshold: 0.7     // Minimum relevance score
}
```

#### Examples
```cx
using vectorDb from "Cx.AI.VectorDatabase";

// Knowledge ingestion (typically done at startup)
vectorDb.IngestTextAsync("Cx is an autonomous programming language with AI-native features");
vectorDb.IngestTextAsync("Event-driven architecture uses on, when, and emit keywords");
vectorDb.IngestTextAsync("Semantic similarity helps classify and route user messages");

// Semantic search
var results = vectorDb.SearchAsync("How does Cx handle events?", {
    maxResults: 3,
    threshold: 0.7
});

// RAG-style question answering
var answer = vectorDb.AskAsync("What are the key features of Cx language?");

// Database statistics
var stats = vectorDb.GetStatsAsync();
print("Knowledge base contains " + stats.documentCount + " documents");
```

### 5. ImageGeneration Service
**Location**: `Cx.AI.ImageGeneration`  
**Purpose**: DALL-E 3 powered image creation and visual content

#### Key Methods
- `GenerateAsync(prompt)` - Basic image generation  
- `GenerateAsync(prompt, options)` - Advanced image configuration

#### Configuration Options
```cx
{
    quality: "hd",         // "standard" or "hd"
    size: "1024x1024",    // "1024x1024", "1792x1024", "1024x1792" 
    style: "vivid"        // "vivid" or "natural"
}
```

#### Examples
```cx
using imageGen from "Cx.AI.ImageGeneration";

// Basic image generation
var image = imageGen.GenerateAsync("A robot painting a landscape");

// High-quality configuration
var artwork = imageGen.GenerateAsync("Abstract representation of autonomous programming", {
    quality: "hd",
    size: "1792x1024", 
    style: "vivid"
});

// Event-driven image creation
on "visualization.needed" (payload)
{
    var diagram = imageGen.GenerateAsync("Technical diagram showing " + payload.concept, {
        quality: "hd",
        style: "natural"
    });
    
    emit "visualization.created", {
        concept: payload.concept,
        imageUrl: diagram,
        timestamp: now()
    };
}
```

### 6. TextToSpeech Service
**Location**: `Cx.AI.TextToSpeech`  
**Purpose**: Audio generation and zero-file MP3 streaming

#### Key Methods
- `SpeakAsync(text)` - Direct audio playback (zero-file streaming)
- `GenerateAudioAsync(text, options)` - Generate audio data for processing

#### Voice Options
```cx
{
    voice: "alloy",    // "alloy", "echo", "fable", "onyx", "nova", "shimmer"
    speed: 1.0,       // Speech speed (0.25-4.0)
    format: "mp3"     // Audio format
}
```

#### Examples
```cx
using tts from "Cx.AI.TextToSpeech";

// Zero-file audio streaming (plays immediately)
tts.SpeakAsync("Welcome to Cx autonomous programming!");

// Custom voice and speed
tts.SpeakAsync("Processing autonomous agent response", {
    voice: "nova",
    speed: 0.8
});

// Event-driven audio notifications
on "alert.critical" (payload)
{
    tts.SpeakAsync("Critical alert: " + payload.message, {
        voice: "onyx",
        speed: 1.2
    });
}

// Generate audio data for further processing
var audioData = tts.GenerateAudioAsync("Status report complete", {
    voice: "alloy",
    format: "mp3"
});
```

## Multi-Service Integration Patterns

### Autonomous Content Pipeline
```cx
using textGen from "Cx.AI.TextGeneration";
using imageGen from "Cx.AI.ImageGeneration";
using tts from "Cx.AI.TextToSpeech";

class ContentCreationAgent
{
    function createMultiModalContent(topic)
    {
        // Generate text content
        var article = textGen.GenerateAsync(
            "Write a comprehensive article about " + topic,
            { temperature: 0.7, maxTokens: 800 }
        );
        
        // Create visual representation
        var image = imageGen.GenerateAsync(
            "Professional illustration representing " + topic,
            { quality: "hd", style: "natural" }
        );
        
        // Generate audio narration
        tts.SpeakAsync("New content created about " + topic);
        
        return {
            topic: topic,
            textContent: article,
            imageUrl: image,
            audioGenerated: true,
            timestamp: now()
        };
    }
}

var agent = new ContentCreationAgent();

// Event-driven content creation
on "content.request" (payload)
{
    var content = agent.createMultiModalContent(payload.topic);
    emit "content.created", content;
}
```

### Semantic Analysis and Response Pipeline
```cx
using embeddings from "Cx.AI.TextEmbeddings";
using vectorDb from "Cx.AI.VectorDatabase";
using chatBot from "Cx.AI.ChatCompletion";

class IntelligentResponseSystem
{
    function analyzeAndRespond(userQuery)
    {
        // Generate embedding for semantic analysis
        var queryEmbedding = embeddings.GenerateEmbeddingAsync(userQuery);
        
        // Search knowledge base for relevant context
        var context = vectorDb.SearchAsync(userQuery, {
            maxResults: 3,
            threshold: 0.7
        });
        
        // Generate contextual response
        var response = chatBot.SendMessageAsync(
            "User asks: " + userQuery + 
            " Relevant context: " + context.map(r => r.content).join(" "),
            { temperature: 0.6 }
        );
        
        return {
            query: userQuery,
            context: context,
            response: response,
            confidence: this.calculateConfidence(context)
        };
    }
    
    function calculateConfidence(searchResults)
    {
        when (searchResults.length == 0) { return 0.2; }
        
        var avgScore = searchResults.reduce((sum, r) => sum + r.score, 0) / searchResults.length;
        return Math.min(avgScore * 1.2, 1.0); // Boost and cap at 1.0
    }
}
```

### Real-Time Translation and Audio System
```cx
using textGen from "Cx.AI.TextGeneration";
using tts from "Cx.AI.TextToSpeech";

class MultilingualAgent
{
    supportedLanguages: array;
    
    constructor()
    {
        this.supportedLanguages = ["English", "Spanish", "French", "German", "Italian"];
    }
    
    function translateAndSpeak(text, targetLanguage)
    {
        // Generate translation
        var translation = textGen.GenerateAsync(
            "Translate this to " + targetLanguage + ": " + text,
            { temperature: 0.2 } // Low temperature for accurate translation
        );
        
        // Speak the translation
        tts.SpeakAsync(translation, {
            voice: this.selectVoiceForLanguage(targetLanguage),
            speed: 0.9
        });
        
        return {
            originalText: text,
            targetLanguage: targetLanguage,
            translation: translation,
            audioGenerated: true
        };
    }
    
    function selectVoiceForLanguage(language)
    {
        when (language.contains("Spanish")) { return "nova"; }
        when (language.contains("French")) { return "shimmer"; }
        return "alloy"; // Default voice
    }
}
```

## Performance Optimization

### Service Call Efficiency
```cx
// ✅ Good: Parallel service calls for independent operations
parallel 
{
    var sentiment = textGen.GenerateAsync("Rate sentiment:", message);
    var embedding = embeddings.GenerateEmbeddingAsync(message);
    var context = vectorDb.SearchAsync(message, { maxResults: 1 });
}

// ❌ Bad: Sequential service calls that could be parallel
var sentiment = textGen.GenerateAsync("Rate sentiment:", message);
var embedding = embeddings.GenerateEmbeddingAsync(message); // Could be parallel
var context = vectorDb.SearchAsync(message, { maxResults: 1 }); // Could be parallel
```

### Caching Strategies
```cx
class OptimizedAIService
{
    embeddingCache: object;
    responseCache: object;
    
    constructor()
    {
        this.embeddingCache = {};
        this.responseCache = {};
    }
    
    function getCachedEmbedding(text)
    {
        var key = this.hashText(text);
        when (!this.embeddingCache[key])
        {
            this.embeddingCache[key] = embeddings.GenerateEmbeddingAsync(text);
        }
        return this.embeddingCache[key];
    }
    
    function getCachedResponse(prompt, options)
    {
        var key = prompt + JSON.stringify(options);
        when (!this.responseCache[key])
        {
            this.responseCache[key] = textGen.GenerateAsync(prompt, options);
        }
        return this.responseCache[key];
    }
}
```

## Error Handling and Reliability

### Robust AI Service Integration
```cx
using textGen from "Cx.AI.TextGeneration";

class RobustAIService
{
    maxRetries: number;
    fallbackResponses: array;
    
    constructor()
    {
        this.maxRetries = 3;
        this.fallbackResponses = [
            "I'm processing your request...",
            "Please allow me a moment to respond...",
            "I'm experiencing some difficulty. Please try again."
        ];
    }
    
    function reliableGenerate(prompt, options, retryCount = 0)
    {
        try
        {
            return textGen.GenerateAsync(prompt, options);
        }
        catch (error)
        {
            when (retryCount < this.maxRetries)
            {
                // Exponential backoff
                var delay = Math.pow(2, retryCount) * 1000;
                setTimeout(() => this.reliableGenerate(prompt, options, retryCount + 1), delay);
            }
            else
            {
                // Use fallback response
                return this.fallbackResponses[retryCount % this.fallbackResponses.length];
            }
        }
    }
}
```

## Configuration and Setup

### Azure OpenAI Configuration
Required `appsettings.json` structure:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4.1-nano",
    "EmbeddingDeploymentName": "text-embedding-3-small", 
    "ImageDeploymentName": "dall-e-3",
    "ApiKey": "your-api-key-here",
    "ApiVersion": "2024-10-21"
  }
}
```

### Multi-Service Configuration
For multi-region setups, see `appsettings.multiservice.example.json`:

```json
{
  "AzureOpenAI": {
    "DefaultService": "EastUS2",
    "ServiceSelection": {
      "TextEmbedding": "EastUS"
    },
    "Services": [
      {
        "Name": "EastUS",
        "Endpoint": "https://eastus-resource.openai.azure.com/",
        "ApiKey": "eastus-api-key",
        "Models": {
          "TextEmbedding": "text-embedding-3-small"
        }
      },
      {
        "Name": "EastUS2", 
        "Endpoint": "https://eastus2-resource.openai.azure.com/",
        "ApiKey": "eastus2-api-key",
        "Models": {
          "ChatCompletion": "gpt-4.1-nano",
          "TextGeneration": "gpt-4.1-nano"
        }
      }
    ]
  }
}
```

## Service Comparison Matrix

| Service | Primary Use Case | Latency | Output Type | Autonomous Suitability |
|---------|-----------------|---------|-------------|----------------------|
| **TextGeneration** | Content creation, structured responses | ~1-3s | Text | ⭐⭐⭐⭐⭐ Excellent |
| **ChatCompletion** | Conversational AI, contextual dialogue | ~1-4s | Text | ⭐⭐⭐⭐ Very Good |
| **TextEmbeddings** | Semantic similarity, classification | ~0.1-0.3s | Vector (1536d) | ⭐⭐⭐⭐⭐ Excellent |
| **VectorDatabase** | Knowledge retrieval, RAG workflows | ~0.1-2s | Search results | ⭐⭐⭐⭐ Very Good |
| **ImageGeneration** | Visual content, diagrams | ~10-30s | Image URL | ⭐⭐⭐ Good |
| **TextToSpeech** | Audio feedback, notifications | ~2-5s | Audio/MP3 | ⭐⭐⭐⭐ Very Good |

## Best Practices

### ✅ DO: AI Service Best Practices
1. **Use structured prompts** for predictable AI responses in autonomous systems
2. **Cache expensive operations** like embeddings and complex generations
3. **Handle errors gracefully** with fallback strategies and retry logic
4. **Monitor service performance** and response times
5. **Use appropriate services** for each use case (embeddings for similarity, not text generation)
6. **Combine services effectively** for multi-modal autonomous workflows
7. **Configure options properly** for your specific use case requirements

### ❌ DON'T: Common AI Service Mistakes
1. **Don't use text generation** for tasks better suited to embeddings or vector search
2. **Don't ignore error handling** in autonomous systems using AI services
3. **Don't generate embeddings repeatedly** for the same text
4. **Don't use high temperature** for structured responses that need consistency
5. **Don't skip configuration optimization** for performance-critical applications
6. **Don't create AI service loops** without termination conditions
7. **Don't forget to monitor costs** and usage patterns in production

## Usage Statistics and Monitoring

### Built-in Performance Tracking
All AI services include automatic performance monitoring:

```cx
// Service call timing and success rates are automatically tracked
var response = textGen.GenerateAsync("Generate response");
// Metrics: call duration, token usage, success/failure rates

var stats = getAIServiceStats();
print("Average response time: " + stats.avgResponseTime + "ms");
print("Success rate: " + (stats.successRate * 100) + "%");
print("Total API calls: " + stats.totalCalls);
```

---

**Next Steps**:
- Explore [[Semantic Similarity Patterns]] for advanced embedding usage
- Learn [[Event-Driven Architecture]] for AI-powered reactive systems  
- Master [[Autonomous Programming Best Practices]] for robust AI integration

**Related Examples**:
- [[Climate Debate Demo]] - Multi-service AI coordination
- [[Content Classification Pipeline]] - AI service chaining
- [[Presence Detection System]] - Real-time AI processing
