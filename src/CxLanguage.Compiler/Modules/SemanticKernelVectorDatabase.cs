#pragma warning disable SKEXP0001 // Suppress experimental API warnings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Semantic Kernel-based vector database implementation for Cx language
/// Provides real embeddings and semantic search capabilities
/// </summary>
public class SemanticKernelVectorDatabase
{
    private readonly ILogger<SemanticKernelVectorDatabase> _logger;
    private readonly ISemanticTextMemory _memory;
    private readonly Kernel _kernel;

    public SemanticKernelVectorDatabase(
        ILogger<SemanticKernelVectorDatabase> logger,
        ISemanticTextMemory memory,
        Kernel kernel)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    }

    /// <summary>
    /// Ingests text into the vector database using Semantic Kernel memory
    /// </summary>
    public async Task<string> IngestAsync(string text, object? options = null)
    {
        try
        {
            var ingestOptions = ExtractIngestOptions(options);
            string collectionName = ingestOptions.Source ?? "default";
            
            // Generate a unique ID for this entry
            string id = Guid.NewGuid().ToString();
            
            // Convert metadata to additional text for better semantic search
            var metadataText = string.Empty;
            if (ingestOptions.Metadata != null)
            {
                var metadataJson = JsonSerializer.Serialize(ingestOptions.Metadata);
                metadataText = $" [Metadata: {metadataJson}]";
            }
            
            // Save to Semantic Kernel memory with real embeddings
            await _memory.SaveInformationAsync(
                collection: collectionName,
                text: text + metadataText,
                id: id,
                description: $"Ingested on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                additionalMetadata: JsonSerializer.Serialize(ingestOptions.Metadata ?? new Dictionary<string, object>())
            );
            
            _logger.LogInformation("Ingested text into collection {CollectionName} with id {Id} using Semantic Kernel", collectionName, id);
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting text with Semantic Kernel");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Searches the vector database for semantically similar content using Semantic Kernel
    /// </summary>
    public async Task<string> SearchAsync(string query, object? options = null)
    {
        try
        {
            var searchOptions = ExtractSearchOptions(options);
            var collectionsToSearch = searchOptions.Collections?.ToArray() ?? new[] { "default" };
            
            var allResults = new List<(MemoryQueryResult result, string collection)>();
            
            // Search each collection using Semantic Kernel memory
            foreach (var collectionName in collectionsToSearch)
            {
                var results = new List<MemoryQueryResult>();
                await foreach (var result in _memory.SearchAsync(
                    collection: collectionName,
                    query: query,
                    limit: searchOptions.Limit * 2, // Get more results to filter later
                    minRelevanceScore: searchOptions.SimilarityThreshold
                ))
                {
                    results.Add(result);
                }
                
                allResults.AddRange(results.Select(r => (result: r, collection: collectionName)));
            }
            
            // Sort all results by relevance score and take the top results
            var topResults = allResults
                .OrderByDescending(r => r.result.Relevance)
                .Take(searchOptions.Limit)
                .ToList();
            
            // Convert to string representation
            var resultText = $"Found {topResults.Count} results using Semantic Kernel:\n";
            foreach (var (result, collection) in topResults)
            {
                resultText += $"- Score: {result.Relevance:F3}, Collection: {collection}\n";
                resultText += $"  Text: {result.Metadata.Text}\n";
                if (searchOptions.IncludeMetadata && !string.IsNullOrEmpty(result.Metadata.AdditionalMetadata))
                {
                    resultText += $"  Metadata: {result.Metadata.AdditionalMetadata}\n";
                }
                resultText += "\n";
            }
            
            return resultText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vector database with Semantic Kernel");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Indexes a collection for optimized search (handled automatically by Semantic Kernel)
    /// </summary>
    public async Task<string> IndexAsync(string collectionName, object? options = null)
    {
        try
        {
            // With Semantic Kernel, indexing happens automatically on ingestion
            // This method provides feedback about the collection
            
            var results = new List<MemoryQueryResult>();
            await foreach (var result in _memory.SearchAsync(
                collection: collectionName,
                query: "test", // Minimal query to check if collection exists
                limit: 1,
                minRelevanceScore: 0.0
            ))
            {
                results.Add(result);
            }
            
            var hasContent = results.Any();
            var message = hasContent 
                ? $"Collection '{collectionName}' is indexed and ready for search with Semantic Kernel"
                : $"Collection '{collectionName}' is empty but indexed with Semantic Kernel";
            
            _logger.LogInformation("Indexed collection {CollectionName} with Semantic Kernel", collectionName);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing collection {CollectionName} with Semantic Kernel", collectionName);
            return $"Error indexing collection: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets collection statistics using Semantic Kernel
    /// </summary>
    public async Task<Dictionary<string, object>> GetCollectionStatsAsync(string collectionName)
    {
        try
        {
            // Get a sample of entries to estimate collection size
            var results = new List<MemoryQueryResult>();
            await foreach (var result in _memory.SearchAsync(
                collection: collectionName,
                query: "sample", // Generic query
                limit: 100,
                minRelevanceScore: 0.0
            ))
            {
                results.Add(result);
            }
            
            var stats = new Dictionary<string, object>
            {
                ["collection_name"] = collectionName,
                ["estimated_entries"] = results.Count,
                ["last_updated"] = DateTime.UtcNow,
                ["using_semantic_kernel"] = true,
                ["embedding_model"] = "text-embedding-ada-002" // Default for Azure OpenAI
            };
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collection stats for {CollectionName}", collectionName);
            return new Dictionary<string, object> { ["error"] = ex.Message };
        }
    }

    /// <summary>
    /// Extracts ingest options from a generic options object
    /// </summary>
    private IngestOptions ExtractIngestOptions(object? options)
    {
        var result = new IngestOptions();
        
        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("source", out var source) && source is string sourceStr)
            {
                result.Source = sourceStr;
            }
            
            if (optionsDict.TryGetValue("metadata", out var metadata) && metadata is Dictionary<string, object> metadataDict)
            {
                result.Metadata = metadataDict;
            }
        }
        
        return result;
    }

    /// <summary>
    /// Extracts search options from a generic options object
    /// </summary>
    private SearchOptions ExtractSearchOptions(object? options)
    {
        var result = new SearchOptions();
        
        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("collections", out var collections) && 
                collections is IEnumerable<object> collectionsList)
            {
                result.Collections = collectionsList
                    .Select(c => c?.ToString())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList()!;
            }
            
            if (optionsDict.TryGetValue("limit", out var limit) && limit is int limitInt)
            {
                result.Limit = limitInt;
            }
            
            if (optionsDict.TryGetValue("similarity_threshold", out var threshold) && 
                threshold is double thresholdDouble)
            {
                result.SimilarityThreshold = thresholdDouble;
            }
            
            if (optionsDict.TryGetValue("include_metadata", out var includeMetadata) && 
                includeMetadata is bool includeMetadataBool)
            {
                result.IncludeMetadata = includeMetadataBool;
            }
        }
        
        return result;
    }
}
