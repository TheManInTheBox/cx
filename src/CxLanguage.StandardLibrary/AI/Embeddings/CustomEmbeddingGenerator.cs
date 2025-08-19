using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;

namespace CxLanguage.StandardLibrary.AI.Embeddings;

/// <summary>
/// Custom embedding generator implementation that wraps Azure OpenAI for Microsoft.Extensions.AI compatibility
/// Provides production-ready embedding capabilities for the Aura Cognitive Framework
/// </summary>
public class CustomEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly AzureOpenAIClient _azureClient;
    private readonly string _deploymentName;
    private readonly ILogger<CustomEmbeddingGenerator> _logger;

    /// <summary>
    /// Metadata for the embedding generator
    /// </summary>
    public EmbeddingGeneratorMetadata Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the CustomEmbeddingGenerator
    /// </summary>
    /// <param name="azureClient">Azure OpenAI client</param>
    /// <param name="deploymentName">Deployment name for the embedding model</param>
    /// <param name="logger">Logger instance</param>
    public CustomEmbeddingGenerator(AzureOpenAIClient azureClient, string deploymentName, ILogger<CustomEmbeddingGenerator> logger)
    {
        _azureClient = azureClient ?? throw new ArgumentNullException(nameof(azureClient));
        _deploymentName = deploymentName ?? throw new ArgumentNullException(nameof(deploymentName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        Metadata = new EmbeddingGeneratorMetadata(GetType().Name);
        
        _logger.LogInformation("✅ CustomEmbeddingGenerator initialized with deployment: {DeploymentName}", deploymentName);
    }

    /// <summary>
    /// Generate embeddings for the provided input values
    /// </summary>
    /// <param name="values">Input text values to generate embeddings for</param>
    /// <param name="options">Embedding generation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated embeddings result</returns>
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, 
        EmbeddingGenerationOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inputList = values.ToList();
            _logger.LogInformation("🔢 Generating embeddings for {Count} input values", inputList.Count);

            // Get Azure OpenAI embedding client
            var embeddingClient = _azureClient.GetEmbeddingClient(_deploymentName);
            
            // Create embedding options
            var azureOptions = new OpenAI.Embeddings.EmbeddingGenerationOptions();
            
            // Generate embeddings using Azure OpenAI
            var response = await embeddingClient.GenerateEmbeddingsAsync(inputList, azureOptions, cancellationToken);
            
            _logger.LogInformation("✅ Successfully generated {Count} embeddings", response.Value.Count);
            
            // Convert Azure OpenAI embeddings to Microsoft.Extensions.AI format
            var embeddings = new List<Embedding<float>>();
            for (int i = 0; i < response.Value.Count; i++)
            {
                var azureEmbedding = response.Value[i];
                var embedding = new Embedding<float>(azureEmbedding.ToFloats().ToArray());
                embeddings.Add(embedding);
            }
            
            return new GeneratedEmbeddings<Embedding<float>>(embeddings)
            {
                Usage = new UsageDetails
                {
                    InputTokenCount = response.Value.Usage?.InputTokenCount,
                    TotalTokenCount = response.Value.Usage?.TotalTokenCount
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during embedding generation");
            throw;
        }
    }

    /// <summary>
    /// Gets a service instance (Microsoft.Extensions.AI interface requirement)
    /// </summary>
    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return this as TService;
    }

    /// <summary>
    /// Gets a service instance (Microsoft.Extensions.AI interface requirement)
    /// </summary>
    public object? GetService(Type serviceType, object? key = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        // Azure client is injected, so we don't dispose it here
        _logger.LogInformation("🧹 CustomEmbeddingGenerator disposed");
    }
}

