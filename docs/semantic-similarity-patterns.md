# Cx Semantic Similarity Patterns

## Overview
Cx provides two main approaches for semantic similarity and text comparison in autonomous programming workflows. This document outlines the available methods and their optimal use cases.

## Available AI Services for Semantic Similarity

### 1. TextEmbeddingsService (Direct Embedding Comparison)
**Location**: `Cx.AI.TextEmbeddings`
**Best For**: Precise similarity calculations, batch processing, custom thresholds

#### Key Methods:
- `GenerateEmbeddingAsync(text)` - Generates 1536-dimensional embeddings
- `CalculateSimilarity(embedding1, embedding2)` - Returns cosine similarity (0.0-1.0)
- `FindMostSimilar(targetEmbedding, candidateEmbeddings)` - Find best match from set

#### Pattern:
```cx
using embeddings from "Cx.AI.TextEmbeddings";

// Generate embeddings for comparison
var inputEmbedding = embeddings.GenerateEmbeddingAsync(userInput);
var urgentEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help critical");

// Calculate similarity score
var similarity = embeddings.CalculateSimilarity(inputEmbedding, urgentEmbedding);

// Make decision based on semantic similarity
when (similarity > 0.8)
{
    // Handle urgent request
    emit "priority.urgent", { content: userInput, score: similarity };
}
```

### 2. VectorDatabaseService (Knowledge Base Search)
**Location**: `Cx.AI.VectorDatabase`
**Best For**: RAG workflows, pre-ingested knowledge bases, contextual search

#### Key Methods:
- `IngestTextAsync(text)` - Add text to knowledge base
- `SearchAsync(query, options)` - Semantic search with relevance scores
- `AskAsync(question)` - RAG-style question answering

#### Pattern:
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
when (technicalMatch.score > 0.8)
{
    emit "route.technical", { 
        content: userInput, 
        confidence: technicalMatch.score,
        method: "vector-search"
    };
}
```

## Comparison of Approaches

| Aspect | TextEmbeddingsService | VectorDatabaseService |
|--------|----------------------|----------------------|
| **Setup Complexity** | Low - direct API calls | Medium - requires knowledge ingestion |
| **Performance** | Fast for single comparisons | Optimized for search across large datasets |
| **Flexibility** | High - compare any text to any text | Medium - limited to pre-ingested knowledge |
| **Memory Usage** | Low - generates embeddings on demand | Higher - stores vector database in memory |
| **Use Case** | Real-time similarity checks | Knowledge base search, RAG workflows |

## Advanced Patterns

### Multi-Category Classification
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
        
        when (similarity > bestMatch.score)
        {
            bestMatch = { category: category.name, score: similarity };
        }
    }
    
    return bestMatch;
}
```

### Hybrid Approach (Embeddings + Vector DB)
```cx
using embeddings from "Cx.AI.TextEmbeddings";
using vectorDb from "Cx.AI.VectorDatabase";

// Use embeddings for real-time classification
var classification = classifyIntent(userInput);

// Use vector database for contextual knowledge retrieval
when (classification.category == "technical" && classification.score > 0.8)
{
    var context = vectorDb.AskAsync("How to help with: " + userInput);
    emit "response.contextual", { 
        classification: classification,
        context: context,
        method: "hybrid"
    };
}
```

## Performance Considerations

### TextEmbeddingsService Performance
- **Embedding Generation**: ~100-300ms per text
- **Similarity Calculation**: <1ms (vector math operation)
- **Memory**: ~6KB per embedding (1536 float32 values)

### VectorDatabaseService Performance  
- **Ingestion**: ~200-500ms per document
- **Search**: ~100-2000ms depending on corpus size
- **Memory**: Varies by corpus size and index structure

## Best Practices

### 1. **Cache Embeddings When Possible**
```cx
// Good: Cache embeddings for repeated comparisons
var urgentEmbedding = embeddings.GenerateEmbeddingAsync("urgent emergency help");
// Store urgentEmbedding for reuse

// Bad: Regenerate embeddings every time
var similarity = embeddings.CalculateSimilarity(
    embeddings.GenerateEmbeddingAsync(input),
    embeddings.GenerateEmbeddingAsync("urgent emergency help")  // Wasteful
);
```

### 2. **Choose Appropriate Similarity Thresholds**
- **0.9+**: Very high similarity (nearly identical content)
- **0.8-0.9**: High similarity (same topic, similar intent)
- **0.7-0.8**: Moderate similarity (related topics)
- **0.6-0.7**: Low similarity (loosely related)
- **<0.6**: No meaningful similarity

### 3. **Combine Approaches for Robust Classification**
- Use **TextEmbeddingsService** for real-time intent classification
- Use **VectorDatabaseService** for knowledge retrieval and context
- Implement fallback mechanisms for edge cases

## Common Use Cases

### Intent Classification
```cx
// Classify user input into predefined categories
var intent = classifyIntent(userMessage);
emit "intent.classified", intent;
```

### Urgency Detection
```cx
// Detect urgent or critical requests
var urgencyScore = checkUrgency(userMessage);
when (urgencyScore > 0.8) { escalate(); }
```

### Content Routing  
```cx
// Route messages to appropriate handlers
var department = routeToDepart(userMessage);
emit "route." + department.name, { content: userMessage, confidence: department.score };
```

### Knowledge Retrieval
```cx
// Find relevant information from knowledge base
var answer = vectorDb.AskAsync("How to solve: " + userProblem);
emit "response.knowledgeable", answer;
```

---

**Last Updated**: July 2025  
**Cx Language Version**: Phase 4 Complete  
**AI Services**: Production Ready
