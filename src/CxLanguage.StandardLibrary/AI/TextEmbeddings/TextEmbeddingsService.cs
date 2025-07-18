using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace CxLanguage.StandardLibrary.AI.TextEmbeddings;

/// <summary>
/// Provides vector embedding capabilities using Semantic Kernel
/// </summary>
public class TextEmbeddingsService : CxAiServiceBase
{
    private readonly ITextEmbeddingGenerationService _embeddingService;

    /// <summary>
    /// Initializes a new instance of the TextEmbeddingsService
    /// </summary>
    /// <param name="kernel">Semantic Kernel instance</param>
    /// <param name="logger">Logger instance</param>
    public TextEmbeddingsService(Kernel kernel, ILogger<TextEmbeddingsService> logger) 
        : base(kernel, logger)
    {
        _embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    }

    /// <summary>
    /// Gets the service name
    /// </summary>
    public override string ServiceName => "TextEmbeddings";
    
    /// <summary>
    /// Gets the service version
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Generate embeddings for a single text
    /// </summary>
    public async Task<TextEmbeddingResult> GenerateEmbeddingAsync(string text, TextEmbeddingOptions? options = null)
    {
        var result = new TextEmbeddingResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Generating embedding for text of length: {Length}", text.Length);

            var embeddings = await _embeddingService.GenerateEmbeddingAsync(text);

            result.IsSuccess = true;
            result.Embedding = embeddings.ToArray();
            result.Dimensions = embeddings.Length;
            result.InputText = text;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Embedding generation successful. Dimensions: {Dimensions}", result.Dimensions);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Generate embeddings for multiple texts in batch
    /// </summary>
    public async Task<BatchTextEmbeddingResult> GenerateEmbeddingsBatchAsync(
        IEnumerable<string> texts, 
        TextEmbeddingOptions? options = null)
    {
        var result = new BatchTextEmbeddingResult();
        var startTime = DateTimeOffset.UtcNow;
        var textList = texts.ToList();

        try
        {
            _logger.LogInformation("Generating embeddings for {Count} texts", textList.Count);

            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(textList);

            result.IsSuccess = true;
            result.Embeddings = embeddings.Select(e => e.ToArray()).ToList();
            result.InputTexts = textList;
            result.Count = embeddings.Count;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            _logger.LogInformation("Batch embedding generation successful. Generated {Count} embeddings", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch embeddings for {Count} texts", textList.Count);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Calculate cosine similarity between two embeddings
    /// </summary>
    public SimilarityResult CalculateSimilarity(float[] embedding1, float[] embedding2)
    {
        var result = new SimilarityResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            if (embedding1.Length != embedding2.Length)
            {
                throw new ArgumentException("Embeddings must have the same dimensions");
            }

            var dotProduct = 0.0f;
            var norm1 = 0.0f;
            var norm2 = 0.0f;

            for (int i = 0; i < embedding1.Length; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                norm1 += embedding1[i] * embedding1[i];
                norm2 += embedding2[i] * embedding2[i];
            }

            var similarity = dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));

            result.IsSuccess = true;
            result.Similarity = (float)similarity;
            result.Distance = 1.0f - result.Similarity;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating similarity between embeddings");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Find the most similar embeddings from a collection
    /// </summary>
    public SemanticSearchResult FindMostSimilar(
        float[] queryEmbedding, 
        IEnumerable<EmbeddingWithMetadata> candidateEmbeddings,
        int topK = 5)
    {
        var result = new SemanticSearchResult();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            var candidates = candidateEmbeddings.ToList();
            var similarities = new List<(float similarity, EmbeddingWithMetadata embedding)>();

            foreach (var candidate in candidates)
            {
                var similarityResult = CalculateSimilarity(queryEmbedding, candidate.Embedding);
                if (similarityResult.IsSuccess)
                {
                    similarities.Add((similarityResult.Similarity, candidate));
                }
            }

            var topResults = similarities
                .OrderByDescending(x => x.similarity)
                .Take(topK)
                .Select(x => new SimilarityMatch
                {
                    Similarity = x.similarity,
                    Distance = 1.0f - x.similarity,
                    Metadata = x.embedding.Metadata,
                    Text = x.embedding.Text
                })
                .ToList();

            result.IsSuccess = true;
            result.Matches = topResults;
            result.QueryEmbedding = queryEmbedding;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding most similar embeddings");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTimeOffset.UtcNow - startTime;
            return result;
        }
    }

    /// <summary>
    /// Synchronous wrapper for GenerateEmbeddingAsync
    /// </summary>
    public TextEmbeddingResult GenerateEmbedding(string text, TextEmbeddingOptions? options = null)
    {
        return GenerateEmbeddingAsync(text, options).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Represents an embedding with associated metadata
/// </summary>
public class EmbeddingWithMetadata
{
    /// <summary>
    /// The embedding vector values
    /// </summary>
    public float[] Embedding { get; set; } = Array.Empty<float>();
    
    /// <summary>
    /// The original text that was embedded
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional metadata associated with the embedding
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a similarity match result
/// </summary>
public class SimilarityMatch
{
    /// <summary>
    /// Similarity score (higher is more similar)
    /// </summary>
    public float Similarity { get; set; }
    
    /// <summary>
    /// Distance score (lower is more similar)
    /// </summary>
    public float Distance { get; set; }
    
    /// <summary>
    /// The matched text
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional metadata associated with the match
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Options for text embedding operations
/// </summary>
public class TextEmbeddingOptions : CxAiOptions
{
    /// <summary>
    /// Model to use for embedding generation
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Whether to normalize the embeddings
    /// </summary>
    public bool Normalize { get; set; } = true;
}

/// <summary>
/// Result from text embedding operations
/// </summary>
public class TextEmbeddingResult : CxAiResult
{
    /// <summary>
    /// The generated embedding vector
    /// </summary>
    public float[] Embedding { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Number of dimensions in the embedding
    /// </summary>
    public int Dimensions { get; set; }

    /// <summary>
    /// The input text that was embedded
    /// </summary>
    public string InputText { get; set; } = string.Empty;
}

/// <summary>
/// Result from batch text embedding operations
/// </summary>
public class BatchTextEmbeddingResult : CxAiResult
{
    /// <summary>
    /// The generated embedding vectors
    /// </summary>
    public List<float[]> Embeddings { get; set; } = new();

    /// <summary>
    /// The input texts that were embedded
    /// </summary>
    public List<string> InputTexts { get; set; } = new();

    /// <summary>
    /// Number of embeddings generated
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Result from similarity calculations
/// </summary>
public class SimilarityResult : CxAiResult
{
    /// <summary>
    /// Cosine similarity score (-1 to 1)
    /// </summary>
    public float Similarity { get; set; }

    /// <summary>
    /// Distance score (0 to 2)
    /// </summary>
    public float Distance { get; set; }
}

/// <summary>
/// Result from semantic search operations
/// </summary>
public class SemanticSearchResult : CxAiResult
{
    /// <summary>
    /// The query embedding used for search
    /// </summary>
    public float[] QueryEmbedding { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Ranked list of similarity matches
    /// </summary>
    public List<SimilarityMatch> Matches { get; set; } = new();
}
