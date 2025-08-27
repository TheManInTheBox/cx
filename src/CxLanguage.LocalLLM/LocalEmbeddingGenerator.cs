using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using LLama.Common;
using LLama;

namespace CxLanguage.LocalLLM;

/// <summary>
/// 🧠 Marcus "LocalLLM" Chen - Local Embedding Generator
/// Local embedding generation using LlamaSharp for zero-cloud dependency operation
/// Supports GGUF embedding models for consciousness-aware document processing
/// </summary>
public class LocalEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>, IDisposable
{
    private readonly ILogger<LocalEmbeddingGenerator> _logger;
    private readonly string _modelPath;
    private LLamaWeights? _model;
    private LLamaEmbedder? _embedder;
    private bool _isLoaded = false;
    private bool _disposed = false;

    /// <summary>
    /// Metadata for the local embedding generator
    /// </summary>
    public EmbeddingGeneratorMetadata Metadata { get; }

    /// <summary>
    /// Initialize the local embedding generator with a GGUF embedding model
    /// </summary>
    /// <param name="modelPath">Path to the GGUF embedding model file</param>
    /// <param name="logger">Logger instance</param>
    public LocalEmbeddingGenerator(string modelPath, ILogger<LocalEmbeddingGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _modelPath = modelPath ?? throw new ArgumentNullException(nameof(modelPath));
        
        Metadata = new EmbeddingGeneratorMetadata("LocalLLM-Embeddings");
        
        _logger.LogInformation("🧩 LocalEmbeddingGenerator initialized with model: {ModelPath}", modelPath);
    }

    /// <summary>
    /// Load the embedding model for processing
    /// </summary>
    public async Task<bool> LoadModelAsync()
    {
        if (_isLoaded)
        {
            _logger.LogDebug("Model already loaded");
            return true;
        }

        if (!File.Exists(_modelPath))
        {
            _logger.LogError("❌ Embedding model file not found: {ModelPath}", _modelPath);
            return false;
        }

        try
        {
            _logger.LogInformation("🧩 Loading local embedding model from {ModelPath}...", _modelPath);

            // LlamaSharp model parameters for embedding generation
            var parameters = new ModelParams(_modelPath)
            {
                ContextSize = 512,         // Smaller context for embeddings
                GpuLayerCount = 32,        // GPU acceleration if available
                Seed = 1337,               // Reproducible embeddings
                UseMemorymap = true,       // Memory mapping for performance
                UseMemoryLock = false,     // Avoid memory locking issues
                Embeddings = true          // Enable embeddings mode
            };

            // Load model with embedding support - suppress console output
            await Task.Run(() =>
            {
                // Temporarily redirect console output to suppress llama_model_loader output
                var originalOut = Console.Out;
                var originalError = Console.Error;
                
                try
                {
                    // Completely suppress all output during model loading
                    Console.SetOut(TextWriter.Null);
                    Console.SetError(TextWriter.Null);
                    
                    _model = LLamaWeights.LoadFromFile(parameters);
                    _embedder = new LLamaEmbedder(_model, parameters);
                }
                finally
                {
                    // Restore console output
                    Console.SetOut(originalOut);
                    Console.SetError(originalError);
                }
            });

            _isLoaded = true;
            _logger.LogInformation("✅ Local embedding model loaded successfully");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to load local embedding model");
            return false;
        }
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
        if (!_isLoaded)
        {
            var loaded = await LoadModelAsync();
            if (!loaded)
            {
                throw new InvalidOperationException("Failed to load local embedding model");
            }
        }

        if (_embedder == null)
        {
            throw new InvalidOperationException("Embedding model not properly initialized");
        }

        try
        {
            var inputList = values.ToList();
            _logger.LogInformation("🔢 Generating local embeddings for {Count} input values", inputList.Count);

            var embeddings = new List<Embedding<float>>();
            int totalTokens = 0;

            foreach (var text in inputList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Generate embedding using LlamaSharp - suppress output
                var embeddingVector = await Task.Run(() => 
                {
                    // Suppress any potential LlamaSharp console output during embedding generation
                    var originalOut = Console.Out;
                    var originalError = Console.Error;
                    
                    try
                    {
                        Console.SetOut(TextWriter.Null);
                        Console.SetError(TextWriter.Null);
                        return _embedder.GetEmbeddings(text);
                    }
                    finally
                    {
                        Console.SetOut(originalOut);
                        Console.SetError(originalError);
                    }
                }, cancellationToken);
                
                // Convert to Microsoft.Extensions.AI format
                var embedding = new Embedding<float>(embeddingVector);
                embeddings.Add(embedding);

                // Estimate token count (rough approximation)
                totalTokens += text.Length / 4;
            }

            _logger.LogInformation("✅ Successfully generated {Count} local embeddings", embeddings.Count);

            return new GeneratedEmbeddings<Embedding<float>>(embeddings)
            {
                Usage = new UsageDetails
                {
                    InputTokenCount = totalTokens,
                    TotalTokenCount = totalTokens
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during local embedding generation");
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
        if (_disposed) return;

        try
        {
            _embedder?.Dispose();
            _model?.Dispose();
            _logger.LogInformation("🧹 LocalEmbeddingGenerator disposed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error during LocalEmbeddingGenerator disposal");
        }

        _disposed = true;
    }
}
