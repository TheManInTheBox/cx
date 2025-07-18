# Vector Database Implementation Guide

This guide provides details on implementing the vector database capabilities for Phase 4 of the Cx language using Microsoft Semantic Kernel's memory functionality.

## Overview

The vector database functionality in Cx enables:
- Semantic search capabilities
- Data ingestion with metadata
- Vector embeddings for text
- Persistent agent memory
- Similarity calculations

## Core Components

### 1. VectorDatabaseService Class

Instead of building a custom vector database from scratch, we'll use Microsoft Semantic Kernel's built-in memory capabilities:

```csharp
public class VectorDatabaseService
{
    private readonly ILogger<VectorDatabaseService> _logger;
    private readonly IKernel _kernel;
    private readonly ISemanticTextMemory _memory;
    
    // Constructor
    public VectorDatabaseService(IKernel kernel, ILogger<VectorDatabaseService> logger)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memory = kernel.Memory;
    }

    // Core operations
    public async Task<string> IngestAsync(string text, object? options = null);
    public async Task<bool> IndexAsync(string collectionName, object? options = null);
    public async Task<List<SearchResult>> SearchAsync(string query, object? options = null);
    public async Task<double> SimilarityAsync(string text1, string text2, object? options = null);
}
```

### 2. SearchResult Class

```csharp
public class SearchResult
{
    public string Id { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 3. AgentMemory Class

```csharp
public class AgentMemory
{
    private readonly ILogger<AgentMemory> _logger;
    private readonly ISemanticTextMemory _memory;
    
    // Constructor
    public AgentMemory(IKernel kernel, ILogger<AgentMemory> logger)
    {
        _memory = kernel.Memory;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Core operations
    public async Task<string> Store(string collection, string key, string text, 
        Dictionary<string, object> metadata = null);
    public async Task<MemoryQueryResult?> Get(string collection, string key);
    public async Task<List<MemoryQueryResult>> Search(string collection, string query, 
        int limit = 5, double minRelevance = 0.7);
    public async Task<bool> Remove(string collection, string key);
}
```

## Implementation Details

### 1. Ingest Function

The `ingest()` function in Cx ingests text into the vector database using Semantic Kernel's memory store:

```cx
// Cx syntax
ingest("This is sample text to ingest", {
    source: "collection_name",
    metadata: { key1: "value1", key2: "value2" }
});
```

Implementation:

```csharp
// C# implementation using Semantic Kernel
public async Task<string> IngestAsync(string text, object? options = null)
{
    try
    {
        var ingestOptions = ExtractIngestOptions(options);
        string collectionName = ingestOptions.Source ?? "default";
        
        // Generate a unique ID for this entry
        string id = Guid.NewGuid().ToString();
        
        // Convert metadata to string dictionary for Semantic Kernel
        Dictionary<string, string> metadata = null;
        if (ingestOptions.Metadata != null)
        {
            metadata = ingestOptions.Metadata.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value?.ToString() ?? string.Empty
            );
        }
        
        // Save to Semantic Kernel memory
        await _memory.SaveInformationAsync(
            collection: collectionName,
            id: id,
            text: text,
            description: null,
            additionalMetadata: metadata
        );
        
        _logger.LogInformation("Ingested text into collection {CollectionName} with id {Id}", collectionName, id);
        return id;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error ingesting text");
        throw;
    }
}
```

### 2. Index Function

The `index()` function in Cx is simplified when using Semantic Kernel as it handles the embedding and indexing automatically:

```cx
// Cx syntax
index("collection_name", {
    embedding_model: "text-embedding-3-small",
    chunk_size: 512
});
```

Implementation:

```csharp
// C# implementation with Semantic Kernel
public async Task<bool> IndexAsync(string collectionName, object? options = null)
{
    try
    {
        var indexOptions = ExtractIndexOptions(options);
        
        // With Semantic Kernel, indexing happens automatically on ingestion
        // This method mostly serves as a configuration point
        
        _logger.LogInformation("Collection {CollectionName} configured with model {Model} and chunk size {ChunkSize}", 
            collectionName, 
            indexOptions.EmbeddingModel ?? "default", 
            indexOptions.ChunkSize);
        
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error configuring collection {CollectionName}", collectionName);
        throw;
    }
}
```

### 3. Search Function

The `search()` function performs semantic search using Semantic Kernel's memory search capabilities:

```cx
// Cx syntax
var results = search("How do neural networks work?", {
    collections: ["knowledge_base"],
    limit: 5,
    similarity_threshold: 0.8,
    include_metadata: true
});
```

Implementation:

```csharp
// C# implementation with Semantic Kernel
public async Task<List<SearchResult>> SearchAsync(string query, object? options = null)
{
    try
    {
        var searchOptions = ExtractSearchOptions(options);
        
        // Convert collections list to array
        var collectionsToSearch = searchOptions.Collections?.ToArray() ?? new[] { "default" };
        
        var results = new List<SearchResult>();
        
        // Search each collection using Semantic Kernel memory
        foreach (var collectionName in collectionsToSearch)
        {
            // Use Semantic Kernel's memory search
            var searchResults = _memory.SearchAsync(
                collection: collectionName,
                query: query,
                limit: searchOptions.Limit,
                minRelevanceScore: searchOptions.SimilarityThreshold
            );
            
            // Convert results to our format
            await foreach (var item in searchResults)
            {
                var result = new SearchResult
                {
                    Id = item.Id,
                    Text = item.Text,
                    Score = item.Relevance,
                    CollectionName = collectionName,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Add metadata if requested
                if (searchOptions.IncludeMetadata && !string.IsNullOrEmpty(item.AdditionalMetadata))
                {
                    try
                    {
                        // Parse metadata from Semantic Kernel's format
                        result.Metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                            item.AdditionalMetadata);
                    }
                    catch
                    {
                        // Fallback if not JSON format
                        result.Metadata = new Dictionary<string, object>
                        {
                            ["metadata"] = item.AdditionalMetadata
                        };
                    }
                }
                
                results.Add(result);
            }
        }
        
        // Sort and limit results (already done by Semantic Kernel, but ensuring consistency)
        return results
            .OrderByDescending(r => r.Score)
            .Take(searchOptions.Limit)
            .ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error searching");
        throw;
    }
}
```

### 4. Similarity Function

The `similarity()` function calculates semantic similarity between texts using Semantic Kernel's embedding service:

```cx
// Cx syntax
var score = similarity("Machine learning is interesting", "AI is fascinating");
```

Implementation:

```csharp
// C# implementation with Semantic Kernel
public async Task<double> SimilarityAsync(string text1, string text2, object? options = null)
{
    try
    {
        // Use Semantic Kernel's built-in embedding service
        var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGeneration>();
        
        // Generate embeddings
        var embeddings = await embeddingService.GenerateEmbeddingsAsync(new[] { text1, text2 });
        
        if (embeddings.Count < 2)
        {
            throw new Exception("Failed to generate embeddings for similarity calculation");
        }
        
        // Calculate cosine similarity using Semantic Kernel's vectors
        return CalculateCosineSimilarity(embeddings[0], embeddings[1]);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error calculating similarity");
        throw;
    }
}

// Helper method for cosine similarity between ReadOnlyMemory<float>
private static double CalculateCosineSimilarity(ReadOnlyMemory<float> embedding1, ReadOnlyMemory<float> embedding2)
{
    var span1 = embedding1.Span;
    var span2 = embedding2.Span;
    
    if (span1.Length != span2.Length)
        throw new ArgumentException("Embeddings must have the same dimension");
    
    float dotProduct = 0;
    float magnitude1 = 0;
    float magnitude2 = 0;
    
    for (int i = 0; i < span1.Length; i++)
    {
        dotProduct += span1[i] * span2[i];
        magnitude1 += span1[i] * span1[i];
        magnitude2 += span2[i] * span2[i];
    }
    
    magnitude1 = (float)Math.Sqrt(magnitude1);
    magnitude2 = (float)Math.Sqrt(magnitude2);
    
    if (magnitude1 == 0 || magnitude2 == 0)
        return 0;
    
    return dotProduct / (magnitude1 * magnitude2);
}
```

## Integration with Semantic Kernel

The vector database capabilities in Cx are designed to integrate seamlessly with Microsoft Semantic Kernel for advanced memory features:

```csharp
// Register services in dependency injection
services.AddSingleton<VectorDatabaseService>();
services.AddSingleton<AgentMemory>();

// Configure the kernel with memory
var builder = Kernel.CreateBuilder();

// Add memory store (in-memory for development)
builder.AddMemoryStore(new VolatileMemoryStore());

// Or use persistent memory store (for production)
builder.AddMemoryStore(new AzureAISearchMemoryStore(
    endpoint: "https://your-search-instance.search.windows.net",
    apiKey: "your-api-key"));

// Add embedding service
builder.AddAzureOpenAITextEmbeddingGeneration(
    deploymentName: "text-embedding-ada-002", 
    endpoint: "https://your-aoai-service.openai.azure.com/",
    apiKey: "your-api-key");

// Build the kernel
var kernel = builder.Build();

// Pass kernel to services
services.AddSingleton(kernel);
```
```

## Compiler Integration

To expose the vector database functions in Cx, the compiler needs visitor methods for each function:

```csharp
public object VisitIngestFunction(IngestFunctionNode node)
{
    // Extract parameters
    var text = Visit(node.Text);
    var options = node.Options != null ? Visit(node.Options) : null;
    
    // Call VectorDatabaseService.IngestAsync
    var vectorDatabaseService = _serviceProvider.GetRequiredService<VectorDatabaseService>();
    var result = vectorDatabaseService.IngestAsync(text.ToString(), options).GetAwaiter().GetResult();
    
    // Return the result
    return result;
}

public object VisitIndexFunction(IndexFunctionNode node)
{
    // Extract parameters
    var collectionName = Visit(node.CollectionName);
    var options = node.Options != null ? Visit(node.Options) : null;
    
    // Call VectorDatabaseService.IndexAsync
    var vectorDatabaseService = _serviceProvider.GetRequiredService<VectorDatabaseService>();
    var result = vectorDatabaseService.IndexAsync(collectionName.ToString(), options).GetAwaiter().GetResult();
    
    // Return the result
    return result;
}

public object VisitSearchFunction(SearchFunctionNode node)
{
    // Extract parameters
    var query = Visit(node.Query);
    var options = node.Options != null ? Visit(node.Options) : null;
    
    // Call VectorDatabaseService.SearchAsync
    var vectorDatabaseService = _serviceProvider.GetRequiredService<VectorDatabaseService>();
    var result = vectorDatabaseService.SearchAsync(query.ToString(), options).GetAwaiter().GetResult();
    
    // Return the result
    return result;
}
```

## Helper Functions

The implementation includes several helper functions:

```csharp
// Calculate cosine similarity between two embeddings
private double CalculateCosineSimilarity(float[] embedding1, float[] embedding2)
{
    if (embedding1.Length != embedding2.Length)
        throw new ArgumentException("Embeddings must have the same dimension");
    
    float dotProduct = 0;
    float magnitude1 = 0;
    float magnitude2 = 0;
    
    for (int i = 0; i < embedding1.Length; i++)
    {
        dotProduct += embedding1[i] * embedding2[i];
        magnitude1 += embedding1[i] * embedding1[i];
        magnitude2 += embedding2[i] * embedding2[i];
    }
    
    magnitude1 = (float)Math.Sqrt(magnitude1);
    magnitude2 = (float)Math.Sqrt(magnitude2);
    
    if (magnitude1 == 0 || magnitude2 == 0)
        return 0;
    
    return dotProduct / (magnitude1 * magnitude2);
}

// Generate embeddings using AI service
private async Task<float[]> GenerateEmbeddingAsync(string text)
{
    var options = new AiAnalysisOptions
    {
        Type = "embedding",
        Properties = new Dictionary<string, object>
        {
            ["model"] = "text-embedding-3-small",
            ["dimensions"] = 1536
        }
    };
    
    var response = await _aiService.AnalyzeAsync(text, options);
    
    if (!response.IsSuccess)
    {
        throw new Exception($"Failed to generate embedding: {response.ErrorMessage}");
    }
    
    // Extract the embedding from the response metadata
    if (response.Metadata.TryGetValue("embedding", out var embeddingObj) &&
        embeddingObj is float[] embedding)
    {
        return embedding;
    }
    
    throw new Exception("Failed to extract embedding from response");
}
```

## Next Steps

1. Implement vector database serialization for persistence
2. Add multi-embedding model support
3. Optimize search algorithms for large collections
4. Implement cross-collection search with relevance scoring
5. Add support for filtering by metadata in search results

## Example: Complete Agent Memory Integration

```csharp
// Example showing how to use AgentMemory for persistent context
public async Task StoreAgentContext(string agentId, Dictionary<string, object> context)
{
    var memory = _serviceProvider.GetRequiredService<AgentMemory>();
    
    foreach (var (key, value) in context)
    {
        await memory.Store(
            collection: $"agent_{agentId}_context",
            key: key,
            text: JsonSerializer.Serialize(value),
            metadata: new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["type"] = value.GetType().Name
            }
        );
    }
}

public async Task<Dictionary<string, object>> LoadAgentContext(string agentId)
{
    var memory = _serviceProvider.GetRequiredService<AgentMemory>();
    var context = new Dictionary<string, object>();
    
    var results = await memory.Search(
        collection: $"agent_{agentId}_context",
        query: "*",
        limit: 100,
        minRelevance: 0
    );
    
    foreach (var result in results)
    {
        context[result.Id] = JsonSerializer.Deserialize<object>(result.Text);
    }
    
    return context;
}
```
