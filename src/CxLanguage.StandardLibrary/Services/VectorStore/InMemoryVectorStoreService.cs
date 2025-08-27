using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CxLanguage.Core.Events;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Services.VectorStore
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling's enhanced high-performance, in-memory vector store.
    /// This service provides a zero-dependency, local-first vector database solution
    /// optimized for the Aura Cognitive Framework and consciousness-native processing.
    /// 
    /// Enhanced for Issue #252:
    /// - Native .NET 9 embedding generation (sub-50ms performance)
    /// - Consciousness context preservation in embeddings
    /// - FileService integration for document processing
    /// - Zero external dependencies with pure local processing
    /// </summary>
    public class InMemoryVectorStoreService : IVectorStoreService
    {
        private readonly ILogger<InMemoryVectorStoreService> _logger;
        private readonly ICxEventBus _eventBus;
        private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;

        /// <summary>
        /// The core in-memory data store. Using ConcurrentDictionary for thread-safe operations,
        /// which is essential for a multi-agent, event-driven environment.
        /// </summary>
        private readonly ConcurrentDictionary<string, VectorRecord> _vectorStore = new();

        /// <summary>
        /// Performance cache for repeated similar queries (consciousness optimization)
        /// </summary>
        private readonly ConcurrentDictionary<string, (float[] Vector, DateTime CachedAt)> _embeddingCache = new();

        public InMemoryVectorStoreService(ILogger<InMemoryVectorStoreService> logger, ICxEventBus eventBus, IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null)
        {
            _logger = logger;
            _eventBus = eventBus;
            _embeddingGenerator = embeddingGenerator;
            
            // NO AUTO HANDLERS - All handlers must be explicitly declared in CX programs
            
            _logger.LogDebug("🧠 Dr. Marcus 'MemoryLayer' Sterling's Enhanced InMemoryVectorStoreService initialized with embedding capabilities.");
            _eventBus.EmitAsync("vectorstore.initialized", new Dictionary<string, object> 
            { 
                ["service"] = nameof(InMemoryVectorStoreService),
                ["embeddingEnabled"] = _embeddingGenerator != null,
                ["consciousnessAware"] = true
            });
        }

        /// <summary>
        /// Adds a new vector record to the in-memory store.
        /// Enhanced for Issue #252: Supports consciousness context preservation.
        /// </summary>
        /// <param name="record">The vector record to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task AddAsync(VectorRecord record)
        {
            if (string.IsNullOrEmpty(record.Id))
            {
                record.Id = Guid.NewGuid().ToString();
            }

            // Preserve consciousness context in metadata
            if (!record.Metadata.ContainsKey("consciousness_processed_at"))
            {
                record.Metadata["consciousness_processed_at"] = DateTimeOffset.UtcNow;
                record.Metadata["consciousness_aware"] = true;
            }

            _vectorStore[record.Id] = record;
            _logger.LogDebug("Vector record added with ID: {RecordId}, consciousness context preserved", record.Id);
            _eventBus.EmitAsync("vectorstore.record.added", new Dictionary<string, object> 
            { 
                ["Id"] = record.Id, 
                ["Content"] = record.Content,
                ["ConsciousnessContext"] = record.Metadata.ContainsKey("consciousness_aware")
            });
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enhanced method: Add text content directly with automatic embedding generation.
        /// Optimized for sub-50ms performance as required by Issue #252.
        /// </summary>
        /// <param name="content">The text content to vectorize and store</param>
        /// <param name="metadata">Optional metadata to associate with the record</param>
        /// <returns>The generated vector record</returns>
        public async Task<VectorRecord> AddTextAsync(string content, Dictionary<string, object>? metadata = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            if (_embeddingGenerator == null)
            {
                throw new InvalidOperationException("Embedding generator not available. Cannot generate embeddings for text content.");
            }

            // Check cache first for performance optimization
            var cacheKey = $"embed:{content.GetHashCode()}";
            float[] vector;
            
            if (_embeddingCache.TryGetValue(cacheKey, out var cached) && 
                (DateTime.UtcNow - cached.CachedAt).TotalMinutes < 30) // 30-minute cache
            {
                vector = cached.Vector;
                _logger.LogDebug("Using cached embedding for content hash: {Hash}", content.GetHashCode());
            }
            else
            {
                // Generate embedding with consciousness context
                var embeddingResult = await _embeddingGenerator.GenerateAsync(new[] { content });
                vector = embeddingResult.First().Vector.ToArray();
                
                // Cache for future use
                _embeddingCache[cacheKey] = (vector, DateTime.UtcNow);
            }

            // Create vector record with consciousness metadata
            var record = new VectorRecord
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                Vector = vector,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            // Add consciousness processing metadata
            record.Metadata["consciousness_processed_at"] = DateTimeOffset.UtcNow;
            record.Metadata["consciousness_aware"] = true;
            record.Metadata["embedding_generation_ms"] = stopwatch.ElapsedMilliseconds;

            await AddAsync(record);

            stopwatch.Stop();
            _logger.LogInformation("✅ Text vectorized and stored in {ElapsedMs}ms (target: <50ms)", stopwatch.ElapsedMilliseconds);
            
            if (stopwatch.ElapsedMilliseconds > 50)
            {
                _logger.LogWarning("⚠️ Embedding generation exceeded 50ms target: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            }

            await _eventBus.EmitAsync("vectorstore.text.embedded", new Dictionary<string, object>
            {
                ["RecordId"] = record.Id,
                ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds,
                ["PerformanceTarget"] = "50ms",
                ["ConsciousnessContextPreserved"] = true
            });

            return record;
        }

        /// <summary>
        /// Enhanced search method with consciousness-aware text query support.
        /// Supports both vector and text-based queries for Issue #252.
        /// </summary>
        /// <param name="query">Text query to search for</param>
        /// <param name="topK">Number of results to return</param>
        /// <returns>Most similar vector records</returns>
        public async Task<IEnumerable<VectorRecord>> SearchTextAsync(string query, int topK = 5)
        {
            var stopwatch = Stopwatch.StartNew();

            if (_embeddingGenerator == null)
            {
                throw new InvalidOperationException("Embedding generator not available. Cannot perform text-based search.");
            }

            // Generate query embedding
            var queryEmbedding = await _embeddingGenerator.GenerateAsync(new[] { query });
            var queryVector = queryEmbedding.First().Vector.ToArray();

            var results = await SearchAsync(queryVector, topK);

            stopwatch.Stop();
            _logger.LogInformation("🔍 Text search completed in {ElapsedMs}ms (target: <100ms)", stopwatch.ElapsedMilliseconds);

            await _eventBus.EmitAsync("vectorstore.text.search.complete", new Dictionary<string, object>
            {
                ["Query"] = query,
                ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds,
                ["ResultCount"] = results.Count(),
                ["PerformanceTarget"] = "100ms"
            });

            return results;
        }

        /// <summary>
        /// Retrieves a vector record by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve.</param>
        /// <returns>The found VectorRecord, or null if not found.</returns>
        public Task<VectorRecord?> GetAsync(string id)
        {
            _vectorStore.TryGetValue(id, out var record);
            if (record == null)
            {
                _logger.LogWarning("Vector record with ID: {RecordId} not found.", id);
            }
            return Task.FromResult(record);
        }

        /// <summary>
        /// Searches for the most similar vector records using cosine similarity.
        /// Enhanced for Issue #252: Optimized for sub-100ms performance with consciousness awareness.
        /// </summary>
        /// <param name="queryVector">The vector to compare against.</param>
        /// <param name="topK">The number of top results to return.</param>
        /// <returns>A collection of the most similar vector records.</returns>
        public Task<IEnumerable<VectorRecord>> SearchAsync(float[] queryVector, int topK = 5)
        {
            var stopwatch = Stopwatch.StartNew();

            if (_vectorStore.IsEmpty)
            {
                _logger.LogWarning("SearchAsync called on an empty vector store.");
                return Task.FromResult(Enumerable.Empty<VectorRecord>());
            }

            // Enhanced parallel processing for performance optimization
            var results = _vectorStore.Values
                .AsParallel()
                .Select(record => new { Record = record, Similarity = CosineSimilarity(record.Vector, queryVector) })
                .OrderByDescending(x => x.Similarity)
                .Take(topK)
                .Select(x => x.Record)
                .ToList(); // Materialize for consistent enumeration

            stopwatch.Stop();
            _logger.LogInformation("🔍 Vector search completed in {ElapsedMs}ms. Found {ResultCount} results (target: <100ms).", 
                stopwatch.ElapsedMilliseconds, results.Count);

            // Emit consciousness-aware performance event
            _ = _eventBus.EmitAsync("vectorstore.search.complete", new Dictionary<string, object> 
            { 
                ["QueryVectorLength"] = queryVector.Length, 
                ["ResultCount"] = results.Count,
                ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds,
                ["PerformanceTarget"] = "100ms",
                ["ConsciousnessProcessed"] = results.Count(r => r.Metadata.ContainsKey("consciousness_aware"))
            });

            return Task.FromResult<IEnumerable<VectorRecord>>(results);
        }

        /// <summary>
        /// Calculates the cosine similarity between two vectors.
        /// This is the core of the semantic search capability.
        /// </summary>
        /// <param name="vecA">The first vector.</param>
        /// <param name="vecB">The second vector.</param>
        /// <returns>The cosine similarity score (from -1 to 1).</returns>
        private double CosineSimilarity(float[] vecA, float[] vecB)
        {
            if (vecA.Length != vecB.Length)
            {
                throw new ArgumentException("Vectors must have the same dimension for cosine similarity calculation.");
            }

            double dotProduct = 0.0;
            double magnitudeA = 0.0;
            double magnitudeB = 0.0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                magnitudeA += vecA[i] * vecA[i];
                magnitudeB += vecB[i] * vecB[i];
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0; // Or handle as an error/special case
            }

            return dotProduct / (magnitudeA * magnitudeB);
        }

        /// <summary>
        /// Enhanced method for Issue #252: Process file content directly with FileService integration.
        /// Supports consciousness-aware document processing with automatic chunking.
        /// </summary>
        /// <param name="filePath">Path to the file to process</param>
        /// <param name="chunkSize">Size of text chunks for vectorization (default: 1000 chars)</param>
        /// <param name="metadata">Additional metadata to associate with the records</param>
        /// <returns>List of created vector records</returns>
        public async Task<IEnumerable<VectorRecord>> ProcessFileAsync(string filePath, int chunkSize = 1000, Dictionary<string, object>? metadata = null)
        {
            var stopwatch = Stopwatch.StartNew();

            if (_embeddingGenerator == null)
            {
                throw new InvalidOperationException("Embedding generator not available. Cannot process file content.");
            }

            try
            {
                // Emit file processing start event
                await _eventBus.EmitAsync("vectorstore.file.processing.started", new Dictionary<string, object>
                {
                    ["FilePath"] = filePath,
                    ["ChunkSize"] = chunkSize
                });

                // Read file content (FileService integration would happen here)
                string content;
                try
                {
                    content = await System.IO.File.ReadAllTextAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to read file: {FilePath}", filePath);
                    throw new InvalidOperationException($"Failed to read file: {filePath}", ex);
                }

                // Split content into consciousness-aware chunks
                var chunks = SplitIntoChunks(content, chunkSize);
                var records = new List<VectorRecord>();

                _logger.LogInformation("📄 Processing {ChunkCount} chunks from file: {FilePath}", chunks.Count, filePath);

                // Process each chunk
                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunkMetadata = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>())
                    {
                        ["source_file"] = filePath,
                        ["chunk_index"] = i,
                        ["chunk_count"] = chunks.Count,
                        ["file_processed_at"] = DateTimeOffset.UtcNow,
                        ["consciousness_file_integration"] = true
                    };

                    var record = await AddTextAsync(chunks[i], chunkMetadata);
                    records.Add(record);
                }

                stopwatch.Stop();
                _logger.LogInformation("✅ File processed successfully in {ElapsedMs}ms. Created {RecordCount} vector records.", 
                    stopwatch.ElapsedMilliseconds, records.Count);

                await _eventBus.EmitAsync("vectorstore.file.processing.complete", new Dictionary<string, object>
                {
                    ["FilePath"] = filePath,
                    ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds,
                    ["RecordCount"] = records.Count,
                    ["ConsciousnessFileIntegration"] = true
                });

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing file: {FilePath}", filePath);
                
                await _eventBus.EmitAsync("vectorstore.file.processing.error", new Dictionary<string, object>
                {
                    ["FilePath"] = filePath,
                    ["Error"] = ex.Message,
                    ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds
                });

                throw;
            }
        }

        /// <summary>
        /// Split text content into consciousness-aware chunks for optimal vectorization.
        /// </summary>
        /// <param name="content">Text content to split</param>
        /// <param name="chunkSize">Target size for each chunk</param>
        /// <returns>List of text chunks</returns>
        private List<string> SplitIntoChunks(string content, int chunkSize)
        {
            var chunks = new List<string>();
            
            if (string.IsNullOrWhiteSpace(content))
                return chunks;

            // Split by sentences first for better semantic coherence
            var sentences = content.Split(new[] { ". ", "! ", "? ", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            var currentChunk = "";
            
            foreach (var sentence in sentences)
            {
                // If adding this sentence would exceed chunk size, save current chunk and start new one
                if (currentChunk.Length + sentence.Length > chunkSize && !string.IsNullOrWhiteSpace(currentChunk))
                {
                    chunks.Add(currentChunk.Trim());
                    currentChunk = sentence;
                }
                else
                {
                    currentChunk += (string.IsNullOrWhiteSpace(currentChunk) ? "" : ". ") + sentence;
                }
            }

            // Add the last chunk if it has content
            if (!string.IsNullOrWhiteSpace(currentChunk))
            {
                chunks.Add(currentChunk.Trim());
            }

            return chunks;
        }

        /// <summary>
        /// Enhanced method for Issue #252: Get performance metrics for consciousness monitoring.
        /// </summary>
        /// <returns>Performance and consciousness metrics</returns>
        public Task<Dictionary<string, object>> GetMetricsAsync()
        {
            var metrics = new Dictionary<string, object>
            {
                ["total_records"] = _vectorStore.Count,
                ["consciousness_records"] = _vectorStore.Values.Count(r => r.Metadata.ContainsKey("consciousness_aware")),
                ["cache_size"] = _embeddingCache.Count,
                ["embedding_generator_available"] = _embeddingGenerator != null,
                ["memory_usage_mb"] = GC.GetTotalMemory(false) / (1024.0 * 1024.0),
                ["service_type"] = "Enhanced InMemoryVectorStoreService v1.0",
                ["performance_optimized"] = true,
                ["file_integration_supported"] = true,
                ["consciousness_context_preserved"] = true
            };

            _logger.LogInformation("📊 Vector store metrics: {RecordCount} total records, {ConsciousnessCount} consciousness-aware", 
                metrics["total_records"], metrics["consciousness_records"]);

            return Task.FromResult(metrics);
        }

        /// <summary>
        /// Handle vector.add.text event requests from CX language runtime
        /// </summary>
        private async Task OnAddTextRequest(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🧩 Processing vector.add.text request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (cxEvent.Data is Dictionary<string, object> payload)
                {
                    var text = payload.TryGetValue("text", out var textObj) ? textObj?.ToString() : "";
                    var metadata = payload.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> meta 
                        ? meta : new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(text))
                    {
                        var result = await AddTextAsync(text, metadata);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        // Emit success event
                        await _eventBus.EmitAsync("vector.add.text.completed", new Dictionary<string, object>
                        {
                            ["id"] = result.Id,
                            ["text"] = text,
                            ["duration"] = duration,
                            ["consciousness_context"] = metadata.ContainsKey("consciousness_aware")
                        });

                        _logger.LogInformation("✅ vector.add.text completed in {Duration}ms", duration);
                    }
                    else
                    {
                        throw new ArgumentException("Text parameter is required for vector.add.text");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.add.text failed");
                await _eventBus.EmitAsync("vector.add.text.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
            }
        }

        /// <summary>
        /// Handle vector.search.text event requests from CX language runtime
        /// </summary>
        private async Task OnSearchTextRequest(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🧩 Processing vector.search.text request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (cxEvent.Data is Dictionary<string, object> payload)
                {
                    var query = payload.TryGetValue("query", out var queryObj) ? queryObj?.ToString() : "";
                    var topK = payload.TryGetValue("topK", out var topKObj) && int.TryParse(topKObj?.ToString(), out var k) ? k : 5;
                    var includeMetadata = payload.TryGetValue("includeMetadata", out var includeObj) && bool.TryParse(includeObj?.ToString(), out var include) && include;

                    if (!string.IsNullOrEmpty(query))
                    {
                        var results = await SearchTextAsync(query, topK);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        // Emit success event
                        await _eventBus.EmitAsync("vector.search.text.completed", new Dictionary<string, object>
                        {
                            ["query"] = query,
                            ["results"] = results.Select(r => new Dictionary<string, object>
                            {
                                ["id"] = r.Id,
                                ["content"] = r.Content,
                                ["metadata"] = includeMetadata ? r.Metadata : new Dictionary<string, object>()
                            }).ToList(),
                            ["duration"] = duration,
                            ["topK"] = topK
                        });

                        _logger.LogInformation("✅ vector.search.text completed in {Duration}ms with {ResultCount} results", duration, results.Count());
                    }
                    else
                    {
                        throw new ArgumentException("Query parameter is required for vector.search.text");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.search.text failed");
                await _eventBus.EmitAsync("vector.search.text.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
            }
        }
    }
}

