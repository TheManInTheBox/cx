# Semantic Similarity Patterns

## Overview
Cx provides two main approaches for semantic similarity and text comparison in autonomous programming workflows. This guide covers the available methods, their optimal use cases, and proven patterns for AI-native development.

## Available AI Services

### 1. TextEmbeddingsService (Direct Embedding Comparison)
**Location**: `Cx.AI.TextEmbeddings`  
**Best For**: Precise similarity calculations, batch processing, custom thresholds

#### Key Methods
- `GenerateEmbeddingAsync(text)` - Generates 1536-dimensional embeddings
- `CalculateSimilarity(embedding1, embedding2)` - Returns cosine similarity (0.0-1.0) 
- `FindMostSimilar(targetEmbedding, candidateEmbeddings)` - Find best match from set

#### Basic Pattern
```cx
using embeddings from "Cx.AI.TextEmbeddings";

// Generate embeddings for comparison
var inputEmbedding = embeddings.GenerateEmbeddingAsync(userInput);
var urgentEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help critical");

// Calculate similarity score
var similarity = embeddings.CalculateSimilarity(inputEmbedding, urgentEmbedding);

// Make decision based on semantic similarity
if (similarity > 0.8)
{
    emit "priority.urgent", { content: userInput, score: similarity };
}
```

### 2. VectorDatabaseService (Knowledge Base Search)
**Location**: `Cx.AI.VectorDatabase`  
**Best For**: RAG workflows, pre-ingested knowledge bases, contextual search

#### Key Methods
- `IngestTextAsync(text)` - Add text to knowledge base
- `SearchAsync(query, options)` - Semantic search with relevance scores
- `AskAsync(question)` - RAG-style question answering

#### Basic Pattern
```cx
using vectorDb from "Cx.AI.VectorDatabase";

// First, ingest knowledge (typically done once at startup)
vectorDb.IngestTextAsync("Technical support: programming, coding, debugging, software issues");
vectorDb.IngestTextAsync("Sales inquiries: pricing, purchasing, quotes, orders, costs");

// Then search for semantic matches
var technicalMatch = vectorDb.SearchAsync(userInput, {
    query: "technical support programming",
    maxResults: 1
});

// Route based on search relevance
if (technicalMatch.score > 0.8)
{
    emit "route.technical", { 
        content: userInput, 
        confidence: technicalMatch.score 
    };
}
```

## Advanced Patterns

### Multi-Category Intent Classification
This pattern demonstrates how to classify user input into multiple categories using semantic similarity:

```cx
using embeddings from "Cx.AI.TextEmbeddings";

function classifyIntent(userInput)
{
    var inputEmbedding = embeddings.GenerateEmbeddingAsync(userInput);
    
    // Define category embeddings
    var categories = [
        { name: "technical", pattern: "programming code software development debugging" },
        { name: "sales", pattern: "price cost buy purchase order pricing quote" },
        { name: "support", pattern: "help problem issue broken not working trouble" }
    ];
    
    var bestMatch = { category: "unknown", score: 0.0 };
    
    for (category in categories)
    {
        var categoryEmbedding = embeddings.GenerateEmbeddingAsync(category.pattern);
        var similarity = embeddings.CalculateSimilarity(inputEmbedding, categoryEmbedding);
        
        if (similarity > bestMatch.score)
        {
            bestMatch = { category: category.name, score: similarity };
        }
    }
    
    return bestMatch;
}

// Usage in event-driven system
on "user.message" (payload)
{
    var classification = classifyIntent(payload.content);
    emit "intent.classified", {
        original: payload.content,
        category: classification.category,
        confidence: classification.score
    };
}
```

### Hybrid Approach (Embeddings + Vector DB)
Combine both approaches for maximum flexibility:

```cx
using embeddings from "Cx.AI.TextEmbeddings";
using vectorDb from "Cx.AI.VectorDatabase";

// Use embeddings for real-time classification
var classification = classifyIntent(userInput);

// Use vector database for contextual knowledge retrieval
if (classification.category == "technical" && classification.score > 0.8)
{
    var context = vectorDb.AskAsync("How to help with: " + userInput);
    emit "response.contextual", { 
        classification: classification,
        context: context,
        method: "hybrid"
    };
}
```

### Urgency Detection System
A complete system for detecting and escalating urgent requests:

```cx
using embeddings from "Cx.AI.TextEmbeddings";
using textGen from "Cx.AI.TextGeneration";

class UrgencyDetector
{
    urgentPatterns: array;
    
    constructor()
    {
        this.urgentPatterns = [
            "urgent emergency help critical",
            "broken down error failure critical",
            "immediate assistance required asap",
            "production issue system failure outage"
        ];
    }
    
    function analyzeUrgency(message)
    {
        var messageEmbedding = embeddings.GenerateEmbeddingAsync(message);
        var maxUrgency = 0.0;
        var matchedPattern = "";
        
        for (pattern in this.urgentPatterns)
        {
            var patternEmbedding = embeddings.GenerateEmbeddingAsync(pattern);
            var similarity = embeddings.CalculateSimilarity(messageEmbedding, patternEmbedding);
            
            if (similarity > maxUrgency)
            {
                maxUrgency = similarity;
                matchedPattern = pattern;
            }
        }
        
        // Get additional context with AI analysis
        var aiAssessment = textGen.GenerateAsync(
            "Rate urgency level 1-10 based on: " + message + ". Respond with only the number.",
            { temperature: 0.1, maxTokens: 10 }
        );
        
        return {
            semanticUrgency: maxUrgency,
            aiUrgency: parseFloat(aiAssessment) / 10.0,
            matchedPattern: matchedPattern,
            finalScore: (maxUrgency + parseFloat(aiAssessment) / 10.0) / 2.0
        };
    }
}

// Usage in autonomous system
var detector = new UrgencyDetector();

on "support.request" (payload)
{
    var urgencyAnalysis = detector.analyzeUrgency(payload.message);
    
    when (urgencyAnalysis.finalScore > 0.8)
    {
        emit "escalation.urgent", {
            originalMessage: payload.message,
            urgencyScore: urgencyAnalysis.finalScore,
            semanticMatch: urgencyAnalysis.matchedPattern,
            aiConfirmation: urgencyAnalysis.aiUrgency,
            timestamp: now()
        };
    }
}
```

## Performance Considerations

### TextEmbeddingsService Performance
- **Embedding Generation**: ~100-300ms per text
- **Similarity Calculation**: <1ms (vector math operation)
- **Memory**: ~6KB per embedding (1536 float32 values)
- **Throughput**: ~3-10 embeddings per second per service call

### VectorDatabaseService Performance  
- **Ingestion**: ~200-500ms per document
- **Search**: ~100-2000ms depending on corpus size
- **Memory**: Varies by corpus size and index structure
- **Throughput**: Optimized for large datasets, slower for single queries

## Best Practices

### 1. Cache Embeddings When Possible
```cx
// ✅ Good: Cache embeddings for repeated comparisons
var urgentEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help");
// Store urgentEmbedding for reuse across multiple comparisons

// ❌ Bad: Regenerate embeddings every time
var similarity = embeddings.CalculateSimilarity(
    embeddings.GenerateEmbeddingAsync(input),
    embeddings.GenerateEmbeddingAsync("urgent emergency help")  // Wasteful regeneration
);
```

### 2. Choose Appropriate Similarity Thresholds
- **0.9+**: Very high similarity (nearly identical content)
- **0.8-0.9**: High similarity (same topic, similar intent)
- **0.7-0.8**: Moderate similarity (related topics)
- **0.6-0.7**: Low similarity (loosely related)
- **<0.6**: No meaningful similarity

### 3. Combine Approaches for Robust Classification
- Use **TextEmbeddingsService** for real-time intent classification
- Use **VectorDatabaseService** for knowledge retrieval and context
- Implement fallback mechanisms for edge cases and low-confidence matches

### 4. Optimize for Your Use Case
- **Real-time systems**: Prefer TextEmbeddingsService with cached embeddings
- **Knowledge-heavy applications**: Use VectorDatabaseService with pre-ingested content
- **Hybrid scenarios**: Combine both approaches for maximum accuracy

## Comparison Matrix

| Aspect | TextEmbeddingsService | VectorDatabaseService |
|--------|----------------------|----------------------|
| **Setup Complexity** | Low - direct API calls | Medium - requires knowledge ingestion |
| **Performance** | Fast for single comparisons | Optimized for search across large datasets |
| **Flexibility** | High - compare any text to any text | Medium - limited to pre-ingested knowledge |
| **Memory Usage** | Low - generates embeddings on demand | Higher - stores vector database in memory |
| **Use Case** | Real-time similarity checks | Knowledge base search, RAG workflows |
| **Latency** | 100-300ms per comparison | 100-2000ms per search |
| **Accuracy** | High for direct comparisons | High for contextual understanding |
| **Scalability** | Good for moderate loads | Excellent for large knowledge bases |

## Common Implementation Patterns

### Intent Classification
```cx
var intent = classifyIntent(userMessage);
emit "intent.classified", intent;
```

### Urgency Detection  
```cx
var urgencyScore = analyzeUrgency(userMessage);
if (urgencyScore > 0.8) { escalateToHuman(); }
```

### Content Routing
```cx
var department = routeToDepartment(userMessage);
emit "route." + department.name, { content: userMessage, confidence: department.score };
```

### Knowledge Retrieval
```cx
var answer = vectorDb.AskAsync("How to solve: " + userProblem);
emit "response.knowledgeable", answer;
```

### Semantic Deduplication
```cx
function isDuplicate(newMessage, existingMessages)
{
    var newEmbedding = embeddings.GenerateEmbeddingAsync(newMessage);
    
    for (existing in existingMessages)
    {
        var existingEmbedding = embeddings.GenerateEmbeddingAsync(existing);
        var similarity = embeddings.CalculateSimilarity(newEmbedding, existingEmbedding);
        
        if (similarity > 0.95)
        {
            return true; // Likely duplicate
        }
    }
    
    return false;
}
```

## Error Handling and Edge Cases

### Robust Similarity Checking
```cx
function safeSimilarityCheck(text1, text2)
{
    try
    {
        var embedding1 = embeddings.GenerateEmbeddingAsync(text1);
        var embedding2 = embeddings.GenerateEmbeddingAsync(text2);
        return embeddings.CalculateSimilarity(embedding1, embedding2);
    }
    catch (error)
    {
        // Fallback to basic text comparison or return default value
        return 0.0;
    }
}
```

### Handling Empty or Invalid Input
```cx
function validateAndClassify(userInput)
{
    if (userInput == null || userInput.length < 3)
    {
        return { category: "invalid", score: 0.0 };
    }
    
    return classifyIntent(userInput);
}
```

---

**Next Steps**: 
- Explore [[Multi-Agent Coordination]] patterns
- Learn about [[Event-Driven Architecture]] 
- Check out [[Autonomous Programming Best Practices]]

**Related Examples**:
- [[Content Classification Pipeline]]
- [[Presence Detection System]]
- [[Climate Debate Demo]]
