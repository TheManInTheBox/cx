using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using CxLanguage.Runtime;
using System.ComponentModel;

namespace CxLanguage.StandardLibrary.AI.VectorDatabase;

/// <summary>
/// Enterprise-grade vector database service powered by Kernel Memory
/// Provides advanced RAG capabilities for the CX AI-native programming language
/// </summary>
public class VectorDatabaseService : IDisposable
{
    private readonly ILogger<VectorDatabaseService> _logger;
    private readonly IKernelMemory _memory;
    private readonly bool _disposed = false;

    public VectorDatabaseService(ILogger<VectorDatabaseService> logger, IKernelMemory memory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        
        _logger.LogInformation("VectorDatabase service initialized with Kernel Memory");
    }

    /// <summary>
    /// Ingest a document into the vector database with advanced processing
    /// </summary>
    [Description("Ingests documents with enterprise-grade processing pipeline")]
    public async Task<DocumentIngestResult> IngestDocumentAsync(string filePath, VectorDatabaseOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Ingesting document: {FilePath}", filePath);
            
            options ??= new VectorDatabaseOptions();
            
            var documentId = Path.GetFileNameWithoutExtension(filePath) + "_" + Guid.NewGuid().ToString("N")[..8];
            
            // Convert Dictionary to TagCollection
            var tags = new TagCollection();
            if (options.Tags != null)
            {
                foreach (var tag in options.Tags)
                {
                    tags.Add(tag.Key, tag.Value);
                }
            }
            
            // Advanced document ingestion with chunking strategies
            await _memory.ImportDocumentAsync(
                filePath: filePath,
                documentId: documentId,
                tags: tags,
                index: options.IndexName ?? "default"
            );

            var result = new DocumentIngestResult
            {
                DocumentId = documentId,
                Status = "Success",
                ChunksCreated = 0, // Kernel Memory handles chunking internally
                IndexName = options.IndexName ?? "default",
                ProcessingTimeMs = 0
            };

            _logger.LogInformation("Document ingested successfully: {DocumentId}", documentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ingest document: {FilePath}", filePath);
            return new DocumentIngestResult
            {
                DocumentId = "",
                Status = $"Error: {ex.Message}",
                ChunksCreated = 0,
                IndexName = "",
                ProcessingTimeMs = 0
            };
        }
    }

    /// <summary>
    /// Ingest text content directly into the vector database
    /// </summary>
    [Description("Ingests text content with metadata")]
    public async Task<DocumentIngestResult> IngestTextAsync(string text, string documentId, VectorDatabaseOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Ingesting text content: {DocumentId}", documentId);
            
            options ??= new VectorDatabaseOptions();
            
            // Convert Dictionary to TagCollection
            var tags = new TagCollection();
            if (options.Tags != null)
            {
                foreach (var tag in options.Tags)
                {
                    tags.Add(tag.Key, tag.Value);
                }
            }
            
            // Import text as document
            await _memory.ImportTextAsync(
                text: text,
                documentId: documentId,
                tags: tags,
                index: options.IndexName ?? "default"
            );

            var result = new DocumentIngestResult
            {
                DocumentId = documentId,
                Status = "Success",
                ChunksCreated = 1,
                IndexName = options.IndexName ?? "default",
                ProcessingTimeMs = 0
            };

            _logger.LogInformation("Text ingested successfully: {DocumentId}", documentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ingest text: {DocumentId}", documentId);
            return new DocumentIngestResult
            {
                DocumentId = documentId,
                Status = $"Error: {ex.Message}",
                ChunksCreated = 0,
                IndexName = "",
                ProcessingTimeMs = 0
            };
        }
    }

    /// <summary>
    /// Perform hybrid search across the vector database
    /// </summary>
    [Description("Performs hybrid vector and text search")]
    public async Task<SearchResults> SearchAsync(string query, VectorSearchOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Performing vector search: {Query}", query);
            
            options ??= new VectorSearchOptions();
            
            var searchResult = await _memory.SearchAsync(
                query: query,
                index: options.IndexName ?? "default",
                limit: options.MaxResults,
                minRelevance: options.MinRelevance
            );

            var results = new SearchResults
            {
                Query = query,
                Results = searchResult.Results.Select(r => new SearchResult
                {
                    Content = r.Partitions.FirstOrDefault()?.Text ?? "",
                    Score = 0.8f, // Placeholder - Kernel Memory doesn't expose relevance score directly
                    DocumentId = r.Link,
                    Metadata = r.Partitions.FirstOrDefault()?.Tags?
                        .ToDictionary(t => t.Key, t => string.Join(", ", t.Value)) ?? new Dictionary<string, string>()
                }).ToList(),
                TotalResults = searchResult.Results.Count(),
                ProcessingTimeMs = 0
            };

            _logger.LogInformation("Search completed. Found {Count} results", results.Results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for query: {Query}", query);
            return new SearchResults
            {
                Query = query,
                Results = new List<SearchResult>(),
                TotalResults = 0,
                ProcessingTimeMs = 0,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Ask questions against the knowledge base with RAG
    /// </summary>
    [Description("Asks questions using RAG (Retrieval-Augmented Generation)")]
    public async Task<string> AskAsync(string question, RagOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Processing RAG question: {Question}", question);
            
            options ??= new RagOptions();
            
            var answer = await _memory.AskAsync(
                question: question,
                index: options.IndexName ?? "default"
            );

            _logger.LogInformation("RAG answer generated successfully");
            return answer.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG question failed: {Question}", question);
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Get memory usage and statistics
    /// </summary>
    [Description("Gets vector database statistics and usage information")]
    public Task<VectorDatabaseStats> GetStatsAsync(string? indexName = null)
    {
        try
        {
            // Note: Kernel Memory doesn't expose detailed stats directly
            // This is a placeholder for future implementation
            var stats = new VectorDatabaseStats
            {
                IndexName = indexName ?? "default",
                DocumentCount = 0,
                VectorCount = 0,
                StorageSize = 0,
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Retrieved vector database stats for index: {IndexName}", indexName ?? "default");
            return Task.FromResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stats for index: {IndexName}", indexName);
            return Task.FromResult(new VectorDatabaseStats
            {
                IndexName = indexName ?? "default",
                DocumentCount = 0,
                VectorCount = 0,
                StorageSize = 0,
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// Convert the service result to string for CX language compatibility
    /// </summary>
    public override string ToString()
    {
        return "VectorDatabaseService: Enterprise RAG with Kernel Memory";
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger.LogInformation("Disposing service: VectorDatabase");
            // IKernelMemory doesn't implement IDisposable directly
        }
    }
}

/// <summary>
/// Configuration options for vector database operations
/// </summary>
public class VectorDatabaseOptions
{
    public string? IndexName { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
    public string ChunkingStrategy { get; set; } = "auto";
    public bool ExtractImages { get; set; } = false;
    public bool PreserveMetadata { get; set; } = true;
}

/// <summary>
/// Options for vector search operations
/// </summary>
public class VectorSearchOptions
{
    public string? IndexName { get; set; }
    public int MaxResults { get; set; } = 10;
    public double MinRelevance { get; set; } = 0.0;
    public Dictionary<string, string>? Filters { get; set; }
}

/// <summary>
/// Options for RAG (Retrieval-Augmented Generation) operations
/// </summary>
public class RagOptions
{
    public string? IndexName { get; set; }
    public int MaxRelevantSources { get; set; } = 5;
    public double MinRelevance { get; set; } = 0.7;
    public string? SystemPrompt { get; set; }
}

/// <summary>
/// Result of document ingestion
/// </summary>
public class DocumentIngestResult
{
    public string DocumentId { get; set; } = "";
    public string Status { get; set; } = "";
    public int ChunksCreated { get; set; }
    public string IndexName { get; set; } = "";
    public long ProcessingTimeMs { get; set; }
}

/// <summary>
/// Search results container
/// </summary>
public class SearchResults
{
    public string Query { get; set; } = "";
    public List<SearchResult> Results { get; set; } = new();
    public int TotalResults { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Individual search result
/// </summary>
public class SearchResult
{
    public string Content { get; set; } = "";
    public float Score { get; set; }
    public string DocumentId { get; set; } = "";
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Vector database statistics
/// </summary>
public class VectorDatabaseStats
{
    public string IndexName { get; set; } = "";
    public int DocumentCount { get; set; }
    public long VectorCount { get; set; }
    public long StorageSize { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}
