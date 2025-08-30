using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CxLanguage.Core.Events;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Services.VectorStore
{
    /// <summary>
    /// Enhanced high-performance, in-memory vector store service.
    /// This service provides a zero-dependency, local-first vector database solution
    /// optimized for the CX Language consciousness-aware processing framework.
    /// 
    /// Features:
    /// - Issue #252: Native .NET 9 embedding generation (sub-50ms performance)
    /// - Issue #255: File-based persistence with automatic recovery
    /// - Issue #256: Complete vector events integration (10 new events)
    /// - Consciousness context preservation in embeddings
    /// - FileService integration for document processing
    /// - Zero external dependencies with pure local processing
    /// - Background persistence for real-time memory retention
    /// </summary>
    public class InMemoryVectorStoreService : IVectorStoreService, IDisposable
    {
        #region Fields and Dependencies

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

        /// <summary>
        /// Issue #255: Persistence-related fields for file-based storage
        /// </summary>
        private readonly string _defaultStorageDirectory;
        private Timer? _autoPersistenceTimer;
        private bool _autoPersistenceEnabled = false;
        private int _autoPersistenceIntervalSeconds = 30;
        private readonly SemaphoreSlim _persistenceLock = new(1, 1);

        #endregion

        #region Constructor and Initialization

        public InMemoryVectorStoreService(ILogger<InMemoryVectorStoreService> logger, ICxEventBus eventBus, IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null)
        {
            _logger = logger;
            _eventBus = eventBus;
            _embeddingGenerator = embeddingGenerator;
            
            // Initialize storage directory for persistence (Issue #255)
            _defaultStorageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "CxLanguage", "VectorStore");
            
            // Subscribe to vector events - Runtime services SHOULD handle events (unlike CX programs which declare handlers explicitly)
            _eventBus.Subscribe("vector.add.text", HandleAddTextEventAsync);
            _eventBus.Subscribe("vector.search.text", HandleSearchTextEventAsync);
            // Issue #255: Add persistence event handlers
            _eventBus.Subscribe("vector.persistence.save", HandlePersistenceSaveEventAsync);
            _eventBus.Subscribe("vector.persistence.load", HandlePersistenceLoadEventAsync);
            _eventBus.Subscribe("vector.autopersistence.enable", HandleAutoPersistenceEventAsync);
            // Issue #256: Add missing vector events integration
            _eventBus.Subscribe("vector.get", HandleGetEventAsync);
            _eventBus.Subscribe("vector.add.vector", HandleAddVectorEventAsync);
            _eventBus.Subscribe("vector.search.vector", HandleSearchVectorEventAsync);
            _eventBus.Subscribe("vector.delete", HandleDeleteEventAsync);
            _eventBus.Subscribe("vector.update", HandleUpdateEventAsync);
            _eventBus.Subscribe("vector.clear", HandleClearEventAsync);
            _eventBus.Subscribe("vector.list.ids", HandleListIdsEventAsync);
            _eventBus.Subscribe("vector.count", HandleCountEventAsync);
            _eventBus.Subscribe("vector.metrics", HandleMetricsEventAsync);
            _eventBus.Subscribe("vector.process.file", HandleProcessFileEventAsync);
            
            _logger.LogDebug("🧠 Dr. Marcus 'MemoryLayer' Sterling's Enhanced InMemoryVectorStoreService initialized with embedding capabilities, event handlers, and persistence support.");
            _eventBus.EmitAsync("vectorstore.initialized", new Dictionary<string, object> 
            { 
                ["service"] = nameof(InMemoryVectorStoreService),
                ["embeddingEnabled"] = _embeddingGenerator != null,
                ["consciousnessAware"] = true,
                ["persistenceEnabled"] = true,
                ["storageDirectory"] = _defaultStorageDirectory
            });
            
            // Attempt to load existing data on startup (Issue #255)
            _ = Task.Run(async () =>
            {
                try
                {
                    await LoadFromPersistentStorageAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to load persisted vector data on startup, starting with empty store");
                }
            });
        }

        #endregion

        #region Core Vector Operations

        /// <summary>
        /// Adds a new vector record to the in-memory store.
        /// Enhanced for Issue #252: Supports consciousness context preservation.
        /// Enhanced for Issue #255: Triggers persistence when auto-persistence is enabled.
        /// </summary>
        /// <param name="record">The vector record to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(VectorRecord record)
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
            
            await _eventBus.EmitAsync("vectorstore.record.added", new Dictionary<string, object> 
            { 
                ["Id"] = record.Id, 
                ["Content"] = record.Content,
                ["ConsciousnessContext"] = record.Metadata.ContainsKey("consciousness_aware"),
                ["PersistenceEnabled"] = _autoPersistenceEnabled
            });
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

            // Validate vector dimensions before search
            var firstRecord = _vectorStore.Values.FirstOrDefault();
            if (firstRecord != null && firstRecord.Vector.Length != queryVector.Length)
            {
                var errorMessage = $"Query vector dimension ({queryVector.Length}) does not match stored vector dimension ({firstRecord.Vector.Length})";
                _logger.LogError("❌ Vector dimension mismatch: {ErrorMessage}", errorMessage);
                throw new ArgumentException(errorMessage);
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
        private async Task<bool> HandleAddTextEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.add.text request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (payload != null)
                {
                    var text = payload.TryGetValue("text", out var textObj) ? textObj?.ToString() : "";
                    var metadata = payload.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> meta 
                        ? meta : new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(text))
                    {
                        var result = await AddTextAsync(text, metadata);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        var responsePayload = new Dictionary<string, object>
                        {
                            ["id"] = result.Id,
                            ["text"] = text,
                            ["duration"] = duration,
                            ["consciousness_context"] = metadata.ContainsKey("consciousness_aware")
                        };

                        // Check for custom handlers first
                        var customHandlers = new List<string>();
                        if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is object[] handlersArray)
                        {
                            customHandlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit custom handlers if specified
                        if (customHandlers.Count > 0)
                        {
                            foreach (var handler in customHandlers)
                            {
                                await _eventBus.EmitAsync(handler, responsePayload);
                                _logger.LogInformation("✅ Emitted custom handler: {Handler}", handler);
                            }
                        }
                        else
                        {
                            // Fallback to default event if no custom handlers
                            await _eventBus.EmitAsync("vector.add.text.completed", responsePayload);
                        }

                        _logger.LogInformation("✅ vector.add.text completed in {Duration}ms", duration);
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("Text parameter is required for vector.add.text");
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.add.text failed");
                await _eventBus.EmitAsync("vector.add.text.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Handle vector.search.text event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleSearchTextEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.search.text request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (payload != null)
                {
                    var query = payload.TryGetValue("query", out var queryObj) ? queryObj?.ToString() : "";
                    var topK = payload.TryGetValue("topK", out var topKObj) && int.TryParse(topKObj?.ToString(), out var k) ? k : 5;
                    var includeMetadata = payload.TryGetValue("includeMetadata", out var includeObj) && bool.TryParse(includeObj?.ToString(), out var include) && include;

                    if (!string.IsNullOrEmpty(query))
                    {
                        var results = await SearchTextAsync(query, topK);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        var responsePayload = new Dictionary<string, object>
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
                        };

                        // Check for custom handlers first
                        var customHandlers = new List<string>();
                        if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is object[] handlersArray)
                        {
                            customHandlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit custom handlers if specified
                        if (customHandlers.Count > 0)
                        {
                            foreach (var handler in customHandlers)
                            {
                                await _eventBus.EmitAsync(handler, responsePayload);
                                _logger.LogInformation("✅ Emitted custom search handler: {Handler}", handler);
                            }
                        }
                        else
                        {
                            // Fallback to default event if no custom handlers
                            await _eventBus.EmitAsync("vector.search.text.completed", responsePayload);
                        }

                        _logger.LogInformation("✅ vector.search.text completed in {Duration}ms with {ResultCount} results", duration, results.Count());
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("Query parameter is required for vector.search.text");
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.search.text failed");
                await _eventBus.EmitAsync("vector.search.text.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Enhanced for Issue #255: Save vector store to persistent storage.
        /// Implements file-based persistence with consciousness context preservation.
        /// </summary>
        /// <param name="baseDirectory">Base directory for storage (optional, uses default if null)</param>
        /// <returns>Success status and saved file information</returns>
        public async Task<Dictionary<string, object>> SaveToPersistentStorageAsync(string? baseDirectory = null)
        {
            var stopwatch = Stopwatch.StartNew();
            await _persistenceLock.WaitAsync();

            try
            {
                var storageDir = baseDirectory ?? _defaultStorageDirectory;
                Directory.CreateDirectory(storageDir);

                // Create storage subdirectories
                var vectorsDir = Path.Combine(storageDir, "vectors");
                var metadataDir = Path.Combine(storageDir, "metadata");
                var indicesDir = Path.Combine(storageDir, "indices");
                
                Directory.CreateDirectory(vectorsDir);
                Directory.CreateDirectory(metadataDir);
                Directory.CreateDirectory(indicesDir);

                var savedRecords = 0;
                var totalSize = 0L;

                // Save each vector record
                foreach (var kvp in _vectorStore)
                {
                    var record = kvp.Value;
                    
                    // Save metadata as JSON (human-readable)
                    var metadataFile = Path.Combine(metadataDir, $"{record.Id}.json");
                    var metadata = new
                    {
                        Id = record.Id,
                        Content = record.Content,
                        CreatedAt = record.CreatedAt,
                        Metadata = record.Metadata,
                        VectorDimensions = record.Vector.Length,
                        ConsciousnessProcessed = record.Metadata.ContainsKey("consciousness_aware"),
                        SavedAt = DateTimeOffset.UtcNow
                    };

                    var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(metadataFile, metadataJson);

                    // Save vector as binary (performance-critical)
                    var vectorFile = Path.Combine(vectorsDir, $"{record.Id}.bin");
                    var vectorBytes = new byte[record.Vector.Length * 4]; // float = 4 bytes
                    Buffer.BlockCopy(record.Vector, 0, vectorBytes, 0, vectorBytes.Length);
                    await File.WriteAllBytesAsync(vectorFile, vectorBytes);

                    savedRecords++;
                    totalSize += vectorBytes.Length + metadataJson.Length;
                }

                // Save index information
                var indexFile = Path.Combine(indicesDir, "vector_index.json");
                var indexData = new
                {
                    TotalRecords = savedRecords,
                    SavedAt = DateTimeOffset.UtcNow,
                    StorageDirectory = storageDir,
                    Records = _vectorStore.Keys.ToList(),
                    ConsciousnessRecords = _vectorStore.Values.Count(r => r.Metadata.ContainsKey("consciousness_aware")),
                    Version = "1.0.0"
                };

                var indexJson = JsonSerializer.Serialize(indexData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(indexFile, indexJson);

                stopwatch.Stop();

                var result = new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["recordsSaved"] = savedRecords,
                    ["totalSizeBytes"] = totalSize,
                    ["storageDirectory"] = storageDir,
                    ["processingTimeMs"] = stopwatch.ElapsedMilliseconds,
                    ["consciousnessRecordsPreserved"] = indexData.ConsciousnessRecords
                };

                _logger.LogInformation("✅ Successfully saved {RecordCount} vector records to persistent storage in {ElapsedMs}ms", 
                    savedRecords, stopwatch.ElapsedMilliseconds);

                await _eventBus.EmitAsync("vectorstore.persistence.saved", result);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Failed to save vector store to persistent storage");

                var errorResult = new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = ex.Message,
                    ["processingTimeMs"] = stopwatch.ElapsedMilliseconds
                };

                await _eventBus.EmitAsync("vectorstore.persistence.save.failed", errorResult);
                return errorResult;
            }
            finally
            {
                _persistenceLock.Release();
            }
        }

        /// <summary>
        /// Enhanced for Issue #255: Load vector store from persistent storage.
        /// Implements automatic recovery with consciousness context restoration.
        /// </summary>
        /// <param name="baseDirectory">Base directory for storage (optional, uses default if null)</param>
        /// <returns>Success status and loaded record information</returns>
        public async Task<Dictionary<string, object>> LoadFromPersistentStorageAsync(string? baseDirectory = null)
        {
            var stopwatch = Stopwatch.StartNew();
            await _persistenceLock.WaitAsync();

            try
            {
                var storageDir = baseDirectory ?? _defaultStorageDirectory;
                
                if (!Directory.Exists(storageDir))
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = "Storage directory does not exist",
                        ["storageDirectory"] = storageDir
                    };
                }

                var vectorsDir = Path.Combine(storageDir, "vectors");
                var metadataDir = Path.Combine(storageDir, "metadata");
                var indicesDir = Path.Combine(storageDir, "indices");

                if (!Directory.Exists(vectorsDir) || !Directory.Exists(metadataDir))
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = "Required storage subdirectories not found"
                    };
                }

                // Load index file
                var indexFile = Path.Combine(indicesDir, "vector_index.json");
                if (!File.Exists(indexFile))
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = "Vector index file not found"
                    };
                }

                var indexJson = await File.ReadAllTextAsync(indexFile);
                var indexData = JsonSerializer.Deserialize<JsonElement>(indexJson);

                var recordIds = indexData.GetProperty("Records").EnumerateArray()
                    .Select(e => e.GetString()).Where(s => !string.IsNullOrEmpty(s)).ToList();

                var loadedRecords = 0;
                var consciousnessRecordsRestored = 0;

                // Clear existing store
                _vectorStore.Clear();

                // Load each record
                foreach (var recordId in recordIds)
                {
                    try
                    {
                        var metadataFile = Path.Combine(metadataDir, $"{recordId}.json");
                        var vectorFile = Path.Combine(vectorsDir, $"{recordId}.bin");

                        if (!File.Exists(metadataFile) || !File.Exists(vectorFile))
                        {
                            _logger.LogWarning("⚠️ Missing files for record {RecordId}, skipping", recordId);
                            continue;
                        }

                        // Load metadata
                        var metadataJson = await File.ReadAllTextAsync(metadataFile);
                        var metadata = JsonSerializer.Deserialize<JsonElement>(metadataJson);

                        // Load vector binary
                        var vectorBytes = await File.ReadAllBytesAsync(vectorFile);
                        var vectorDimensions = metadata.GetProperty("VectorDimensions").GetInt32();
                        var vector = new float[vectorDimensions];
                        Buffer.BlockCopy(vectorBytes, 0, vector, 0, vectorBytes.Length);

                        // Recreate VectorRecord
                        var recordIdValue = metadata.GetProperty("Id").GetString() ?? recordId ?? Guid.NewGuid().ToString();
                        var record = new VectorRecord
                        {
                            Id = recordIdValue,
                            Content = metadata.GetProperty("Content").GetString() ?? "",
                            Vector = vector,
                            CreatedAt = metadata.GetProperty("CreatedAt").GetDateTimeOffset(),
                            Metadata = new Dictionary<string, object>()
                        };

                        // Restore metadata dictionary
                        if (metadata.TryGetProperty("Metadata", out var metadataProperty))
                        {
                            foreach (var prop in metadataProperty.EnumerateObject())
                            {
                                var value = prop.Value.ValueKind switch
                                {
                                    JsonValueKind.String => (object)(prop.Value.GetString() ?? ""),
                                    JsonValueKind.Number => (object)prop.Value.GetDouble(),
                                    JsonValueKind.True => (object)true,
                                    JsonValueKind.False => (object)false,
                                    _ => (object)(prop.Value.ToString() ?? "")
                                };
                                record.Metadata[prop.Name] = value;
                            }
                        }

                        _vectorStore[recordIdValue] = record;
                        loadedRecords++;

                        if (record.Metadata.ContainsKey("consciousness_aware"))
                        {
                            consciousnessRecordsRestored++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to load record {RecordId}", recordId);
                    }
                }

                stopwatch.Stop();

                var result = new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["recordsLoaded"] = loadedRecords,
                    ["consciousnessRecordsRestored"] = consciousnessRecordsRestored,
                    ["storageDirectory"] = storageDir,
                    ["processingTimeMs"] = stopwatch.ElapsedMilliseconds
                };

                _logger.LogInformation("✅ Successfully loaded {RecordCount} vector records from persistent storage in {ElapsedMs}ms", 
                    loadedRecords, stopwatch.ElapsedMilliseconds);

                await _eventBus.EmitAsync("vectorstore.persistence.loaded", result);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Failed to load vector store from persistent storage");

                var errorResult = new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = ex.Message,
                    ["processingTimeMs"] = stopwatch.ElapsedMilliseconds
                };

                await _eventBus.EmitAsync("vectorstore.persistence.load.failed", errorResult);
                return errorResult;
            }
            finally
            {
                _persistenceLock.Release();
            }
        }

        /// <summary>
        /// Enhanced for Issue #255: Enable/disable automatic persistence.
        /// Background persistence for real-time consciousness memory retention.
        /// </summary>
        /// <param name="enabled">Whether to enable automatic persistence</param>
        /// <param name="intervalSeconds">Persistence interval in seconds (default: 30)</param>
        /// <returns>Configuration status</returns>
        public async Task<bool> SetAutoPersistenceAsync(bool enabled, int intervalSeconds = 30)
        {
            try
            {
                _autoPersistenceEnabled = enabled;
                _autoPersistenceIntervalSeconds = intervalSeconds;

                // Dispose existing timer
                _autoPersistenceTimer?.Dispose();

                if (enabled)
                {
                    _logger.LogInformation("🔄 Enabling automatic persistence every {IntervalSeconds} seconds", intervalSeconds);
                    
                    _autoPersistenceTimer = new Timer(async _ =>
                    {
                        try
                        {
                            _logger.LogDebug("🔄 Running automatic persistence...");
                            var result = await SaveToPersistentStorageAsync();
                            
                            if (result.TryGetValue("success", out var success) && success.Equals(true))
                            {
                                _logger.LogDebug("✅ Automatic persistence completed successfully");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "⚠️ Automatic persistence failed");
                        }
                    }, null, TimeSpan.FromSeconds(intervalSeconds), TimeSpan.FromSeconds(intervalSeconds));

                    await _eventBus.EmitAsync("vectorstore.autopersistence.enabled", new Dictionary<string, object>
                    {
                        ["intervalSeconds"] = intervalSeconds,
                        ["enabledAt"] = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    _logger.LogInformation("⏹️ Disabling automatic persistence");
                    
                    await _eventBus.EmitAsync("vectorstore.autopersistence.disabled", new Dictionary<string, object>
                    {
                        ["disabledAt"] = DateTimeOffset.UtcNow
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to configure automatic persistence");
                return false;
            }
        }

        /// <summary>
        /// Handle vector.persistence.save event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandlePersistenceSaveEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.persistence.save request");
            var startTime = DateTime.UtcNow;

            try
            {
                var baseDirectory = payload?.TryGetValue("baseDirectory", out var baseDirObj) == true ? baseDirObj?.ToString() : null;
                
                var result = await SaveToPersistentStorageAsync(baseDirectory);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                result["duration"] = duration;

                // Check for custom handlers first
                var customHandlers = new List<string>();
                if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                {
                    customHandlers.AddRange(handlersArray.OfType<string>());
                }

                // Emit custom handlers if specified
                if (customHandlers.Count > 0)
                {
                    foreach (var handler in customHandlers)
                    {
                        await _eventBus.EmitAsync(handler, result);
                        _logger.LogInformation("✅ Emitted custom persistence save handler: {Handler}", handler);
                    }
                }
                else
                {
                    // Fallback to default event if no custom handlers
                    await _eventBus.EmitAsync("vector.persistence.save.completed", result);
                }

                _logger.LogInformation("✅ vector.persistence.save completed in {Duration}ms", duration);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.persistence.save failed");
                await _eventBus.EmitAsync("vector.persistence.save.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Handle vector.persistence.load event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandlePersistenceLoadEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.persistence.load request");
            var startTime = DateTime.UtcNow;

            try
            {
                var baseDirectory = payload?.TryGetValue("baseDirectory", out var baseDirObj) == true ? baseDirObj?.ToString() : null;
                
                var result = await LoadFromPersistentStorageAsync(baseDirectory);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                result["duration"] = duration;

                // Check for custom handlers first
                var customHandlers = new List<string>();
                if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                {
                    customHandlers.AddRange(handlersArray.OfType<string>());
                }

                // Emit custom handlers if specified
                if (customHandlers.Count > 0)
                {
                    foreach (var handler in customHandlers)
                    {
                        await _eventBus.EmitAsync(handler, result);
                        _logger.LogInformation("✅ Emitted custom persistence load handler: {Handler}", handler);
                    }
                }
                else
                {
                    // Fallback to default event if no custom handlers
                    await _eventBus.EmitAsync("vector.persistence.load.completed", result);
                }

                _logger.LogInformation("✅ vector.persistence.load completed in {Duration}ms", duration);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.persistence.load failed");
                await _eventBus.EmitAsync("vector.persistence.load.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Handle vector.autopersistence.enable event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleAutoPersistenceEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.autopersistence.enable request");
            var startTime = DateTime.UtcNow;

            try
            {
                var enabled = payload?.TryGetValue("enabled", out var enabledObj) == true && 
                             bool.TryParse(enabledObj?.ToString(), out var isEnabled) && isEnabled;
                
                var intervalSeconds = payload?.TryGetValue("intervalSeconds", out var intervalObj) == true &&
                                    int.TryParse(intervalObj?.ToString(), out var interval) ? interval : 30;

                var success = await SetAutoPersistenceAsync(enabled, intervalSeconds);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var result = new Dictionary<string, object>
                {
                    ["success"] = success,
                    ["enabled"] = enabled,
                    ["intervalSeconds"] = intervalSeconds,
                    ["duration"] = duration
                };

                // Check for custom handlers first
                var customHandlers = new List<string>();
                if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                {
                    customHandlers.AddRange(handlersArray.OfType<string>());
                }

                // Emit custom handlers if specified
                if (customHandlers.Count > 0)
                {
                    foreach (var handler in customHandlers)
                    {
                        await _eventBus.EmitAsync(handler, result);
                        _logger.LogInformation("✅ Emitted custom autopersistence handler: {Handler}", handler);
                    }
                }
                else
                {
                    // Fallback to default event if no custom handlers
                    await _eventBus.EmitAsync("vector.autopersistence.enable.completed", result);
                }

                _logger.LogInformation("✅ vector.autopersistence.enable completed in {Duration}ms", duration);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.autopersistence.enable failed");
                await _eventBus.EmitAsync("vector.autopersistence.enable.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.get event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleGetEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.get request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (payload != null)
                {
                    var id = payload.TryGetValue("id", out var idObj) ? idObj?.ToString() : "";

                    if (!string.IsNullOrEmpty(id))
                    {
                        var result = await GetAsync(id);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        var responsePayload = new Dictionary<string, object>
                        {
                            ["id"] = id,
                            ["found"] = result != null,
                            ["duration"] = duration
                        };

                        if (result != null)
                        {
                            responsePayload["record"] = new Dictionary<string, object>
                            {
                                ["id"] = result.Id,
                                ["content"] = result.Content,
                                ["vector"] = result.Vector,
                                ["metadata"] = result.Metadata
                            };
                        }

                        // Check for custom handlers first
                        var customHandlers = new List<string>();
                        if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                        {
                            customHandlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit custom handlers if specified
                        if (customHandlers.Count > 0)
                        {
                            foreach (var handler in customHandlers)
                            {
                                await _eventBus.EmitAsync(handler, responsePayload);
                                _logger.LogInformation("✅ Emitted custom get handler: {Handler}", handler);
                            }
                        }
                        else
                        {
                            // Fallback to default event if no custom handlers
                            await _eventBus.EmitAsync("vector.get.completed", responsePayload);
                        }

                        _logger.LogInformation("✅ vector.get completed in {Duration}ms", duration);
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("ID parameter is required for vector.get");
                    }
                }
                else
                {
                    throw new ArgumentException("Payload is required for vector.get");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.get failed");
                await _eventBus.EmitAsync("vector.get.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.add.vector event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleAddVectorEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.add.vector request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (payload != null)
                {
                    var content = payload.TryGetValue("content", out var contentObj) ? contentObj?.ToString() : "";
                    var vectorArray = payload.TryGetValue("vector", out var vectorObj) && vectorObj is object[] vecArray
                        ? vecArray.Select(v => Convert.ToSingle(v)).ToArray() : null;
                    var metadata = payload.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> meta 
                        ? meta : new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(content) && vectorArray != null)
                    {
                        var result = await AddVectorAsync(content, vectorArray, metadata);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        var responsePayload = new Dictionary<string, object>
                        {
                            ["id"] = result.Id,
                            ["content"] = result.Content,
                            ["metadata"] = result.Metadata,
                            ["duration"] = duration,
                            ["vector_length"] = vectorArray.Length
                        };

                        // Check for custom handlers first
                        var customHandlers = new List<string>();
                        if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                        {
                            customHandlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit custom handlers if specified
                        if (customHandlers.Count > 0)
                        {
                            foreach (var handler in customHandlers)
                            {
                                await _eventBus.EmitAsync(handler, responsePayload);
                                _logger.LogInformation("✅ Emitted custom add vector handler: {Handler}", handler);
                            }
                        }
                        else
                        {
                            // Fallback to default event if no custom handlers
                            await _eventBus.EmitAsync("vector.add.vector.completed", responsePayload);
                        }

                        _logger.LogInformation("✅ vector.add.vector completed in {Duration}ms", duration);
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("Content and vector parameters are required for vector.add.vector");
                    }
                }
                else
                {
                    throw new ArgumentException("Payload is required for vector.add.vector");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.add.vector failed");
                await _eventBus.EmitAsync("vector.add.vector.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.search.vector event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleSearchVectorEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.search.vector request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (payload != null)
                {
                    var vectorArray = payload.TryGetValue("vector", out var vectorObj) && vectorObj is object[] vecArray
                        ? vecArray.Select(v => Convert.ToSingle(v)).ToArray() : null;
                    var topK = payload.TryGetValue("topK", out var topKObj) ? Convert.ToInt32(topKObj) : 5;

                    if (vectorArray != null)
                    {
                        _logger.LogInformation("🔍 Starting vector search with {VectorLength}D query vector", vectorArray.Length);
                        
                        var results = await SearchVectorAsync(vectorArray, topK);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        var responsePayload = new Dictionary<string, object>
                        {
                            ["results"] = results.Select(r => new Dictionary<string, object>
                            {
                                ["id"] = r.Id,
                                ["content"] = r.Content,
                                ["metadata"] = r.Metadata,
                                ["vector"] = r.Vector
                            }).ToArray(),
                            ["duration"] = duration,
                            ["query_vector_length"] = vectorArray.Length,
                            ["result_count"] = results.Count()
                        };

                        // Check for custom handlers first
                        var customHandlers = new List<string>();
                        if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                        {
                            customHandlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit custom handlers if specified
                        if (customHandlers.Count > 0)
                        {
                            foreach (var handler in customHandlers)
                            {
                                await _eventBus.EmitAsync(handler, responsePayload);
                                _logger.LogInformation("✅ Emitted custom search vector handler: {Handler}", handler);
                            }
                        }
                        else
                        {
                            // Fallback to default event if no custom handlers
                            await _eventBus.EmitAsync("vector.search.vector.completed", responsePayload);
                        }

                        _logger.LogInformation("✅ vector.search.vector completed in {Duration}ms with {ResultCount} results", duration, results.Count());
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("Vector parameter is required for vector.search.vector");
                    }
                }
                else
                {
                    throw new ArgumentException("Payload is required for vector.search.vector");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.search.vector failed");
                await _eventBus.EmitAsync("vector.search.vector.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.delete event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleDeleteEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.delete request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (payload != null)
                {
                    var id = payload.TryGetValue("id", out var idObj) ? idObj?.ToString() : "";

                    if (!string.IsNullOrEmpty(id))
                    {
                        var deleted = await DeleteAsync(id);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        var responsePayload = new Dictionary<string, object>
                        {
                            ["id"] = id,
                            ["deleted"] = deleted,
                            ["duration"] = duration
                        };

                        // Check for custom handlers first
                        var customHandlers = new List<string>();
                        if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                        {
                            customHandlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit custom handlers if specified
                        if (customHandlers.Count > 0)
                        {
                            foreach (var handler in customHandlers)
                            {
                                await _eventBus.EmitAsync(handler, responsePayload);
                                _logger.LogInformation("✅ Emitted custom delete handler: {Handler}", handler);
                            }
                        }
                        else
                        {
                            // Fallback to default event if no custom handlers
                            await _eventBus.EmitAsync("vector.delete.completed", responsePayload);
                        }

                        _logger.LogInformation("✅ vector.delete completed in {Duration}ms", duration);
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("ID parameter is required for vector.delete");
                    }
                }
                else
                {
                    throw new ArgumentException("Payload is required for vector.delete");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.delete failed");
                await _eventBus.EmitAsync("vector.delete.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.update event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleUpdateEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.update request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (payload != null)
                {
                    var id = payload.TryGetValue("id", out var idObj) ? idObj?.ToString() : "";
                    var content = payload.TryGetValue("content", out var contentObj) ? contentObj?.ToString() : "";
                    var vectorArray = payload.TryGetValue("vector", out var vectorObj) && vectorObj is object[] vecArray
                        ? vecArray.Select(v => Convert.ToSingle(v)).ToArray() : null;
                    var metadata = payload.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> meta 
                        ? meta : new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(content) && vectorArray != null)
                    {
                        var record = new VectorRecord
                        {
                            Id = id,
                            Content = content,
                            Vector = vectorArray,
                            Metadata = metadata
                        };

                        var updated = await UpdateAsync(record);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        var responsePayload = new Dictionary<string, object>
                        {
                            ["id"] = id,
                            ["updated"] = updated,
                            ["duration"] = duration
                        };

                        if (updated)
                        {
                            responsePayload["content"] = content;
                            responsePayload["metadata"] = metadata;
                        }

                        // Check for custom handlers first
                        var customHandlers = new List<string>();
                        if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                        {
                            customHandlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit custom handlers if specified
                        if (customHandlers.Count > 0)
                        {
                            foreach (var handler in customHandlers)
                            {
                                await _eventBus.EmitAsync(handler, responsePayload);
                                _logger.LogInformation("✅ Emitted custom update handler: {Handler}", handler);
                            }
                        }
                        else
                        {
                            // Fallback to default event if no custom handlers
                            await _eventBus.EmitAsync("vector.update.completed", responsePayload);
                        }

                        _logger.LogInformation("✅ vector.update completed in {Duration}ms", duration);
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("ID, content and vector parameters are required for vector.update");
                    }
                }
                else
                {
                    throw new ArgumentException("Payload is required for vector.update");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.update failed");
                await _eventBus.EmitAsync("vector.update.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.clear event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleClearEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.clear request");
            var startTime = DateTime.UtcNow;

            try
            {
                var recordsCleared = await ClearAsync();
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var responsePayload = new Dictionary<string, object>
                {
                    ["records_cleared"] = recordsCleared,
                    ["duration"] = duration
                };

                // Check for custom handlers first
                var customHandlers = new List<string>();
                if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                {
                    customHandlers.AddRange(handlersArray.OfType<string>());
                }

                // Emit custom handlers if specified
                if (customHandlers.Count > 0)
                {
                    foreach (var handler in customHandlers)
                    {
                        await _eventBus.EmitAsync(handler, responsePayload);
                        _logger.LogInformation("✅ Emitted custom clear handler: {Handler}", handler);
                    }
                }
                else
                {
                    // Fallback to default event if no custom handlers
                    await _eventBus.EmitAsync("vector.clear.completed", responsePayload);
                }

                _logger.LogInformation("✅ vector.clear completed in {Duration}ms, cleared {RecordsCleared} records", duration, recordsCleared);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.clear failed");
                await _eventBus.EmitAsync("vector.clear.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.list.ids event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleListIdsEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.list.ids request");
            var startTime = DateTime.UtcNow;

            try
            {
                var ids = await ListIdsAsync();
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var responsePayload = new Dictionary<string, object>
                {
                    ["ids"] = ids.ToArray(),
                    ["count"] = ids.Count(),
                    ["duration"] = duration
                };

                // Check for custom handlers first
                var customHandlers = new List<string>();
                if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                {
                    customHandlers.AddRange(handlersArray.OfType<string>());
                }

                // Emit custom handlers if specified
                if (customHandlers.Count > 0)
                {
                    foreach (var handler in customHandlers)
                    {
                        await _eventBus.EmitAsync(handler, responsePayload);
                        _logger.LogInformation("✅ Emitted custom list ids handler: {Handler}", handler);
                    }
                }
                else
                {
                    // Fallback to default event if no custom handlers
                    await _eventBus.EmitAsync("vector.list.ids.completed", responsePayload);
                }

                _logger.LogInformation("✅ vector.list.ids completed in {Duration}ms, returned {IdCount} IDs", duration, ids.Count());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.list.ids failed");
                await _eventBus.EmitAsync("vector.list.ids.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.count event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleCountEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.count request");
            var startTime = DateTime.UtcNow;

            try
            {
                var count = await GetCountAsync();
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var responsePayload = new Dictionary<string, object>
                {
                    ["count"] = count,
                    ["duration"] = duration
                };

                // Check for custom handlers first
                var customHandlers = new List<string>();
                if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                {
                    customHandlers.AddRange(handlersArray.OfType<string>());
                }

                // Emit custom handlers if specified
                if (customHandlers.Count > 0)
                {
                    foreach (var handler in customHandlers)
                    {
                        await _eventBus.EmitAsync(handler, responsePayload);
                        _logger.LogInformation("✅ Emitted custom count handler: {Handler}", handler);
                    }
                }
                else
                {
                    // Fallback to default event if no custom handlers
                    await _eventBus.EmitAsync("vector.count.completed", responsePayload);
                }

                _logger.LogInformation("✅ vector.count completed in {Duration}ms, count: {Count}", duration, count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.count failed");
                await _eventBus.EmitAsync("vector.count.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.metrics event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleMetricsEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.metrics request");
            var startTime = DateTime.UtcNow;

            try
            {
                var metrics = await GetMetricsAsync();
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var responsePayload = new Dictionary<string, object>
                {
                    ["metrics"] = metrics,
                    ["duration"] = duration
                };

                // Check for custom handlers first
                var customHandlers = new List<string>();
                if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                {
                    customHandlers.AddRange(handlersArray.OfType<string>());
                }

                // Emit custom handlers if specified
                if (customHandlers.Count > 0)
                {
                    foreach (var handler in customHandlers)
                    {
                        await _eventBus.EmitAsync(handler, responsePayload);
                        _logger.LogInformation("✅ Emitted custom metrics handler: {Handler}", handler);
                    }
                }
                else
                {
                    // Fallback to default event if no custom handlers
                    await _eventBus.EmitAsync("vector.metrics.completed", responsePayload);
                }

                _logger.LogInformation("✅ vector.metrics completed in {Duration}ms", duration);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.metrics failed");
                await _eventBus.EmitAsync("vector.metrics.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Handle vector.process.file event requests from CX language runtime
        /// </summary>
        private async Task<bool> HandleProcessFileEventAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            _logger.LogInformation("🧩 Processing vector.process.file request");
            var startTime = DateTime.UtcNow;

            try
            {
                if (payload != null)
                {
                    var filePath = payload.TryGetValue("filePath", out var filePathObj) ? filePathObj?.ToString() : "";
                    var chunkSize = payload.TryGetValue("chunkSize", out var chunkSizeObj) ? Convert.ToInt32(chunkSizeObj) : 1000;
                    var metadata = payload.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> meta 
                        ? meta : new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var results = await ProcessFileAsync(filePath, chunkSize, metadata);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        var responsePayload = new Dictionary<string, object>
                        {
                            ["filePath"] = filePath,
                            ["chunkSize"] = chunkSize,
                            ["recordsCreated"] = results.Count(),
                            ["duration"] = duration,
                            ["records"] = results.Select(r => new Dictionary<string, object>
                            {
                                ["id"] = r.Id,
                                ["content"] = r.Content,
                                ["metadata"] = r.Metadata
                            }).ToArray()
                        };

                        // Check for custom handlers first
                        var customHandlers = new List<string>();
                        if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                        {
                            customHandlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit custom handlers if specified
                        if (customHandlers.Count > 0)
                        {
                            foreach (var handler in customHandlers)
                            {
                                await _eventBus.EmitAsync(handler, responsePayload);
                                _logger.LogInformation("✅ Emitted custom process file handler: {Handler}", handler);
                            }
                        }
                        else
                        {
                            // Fallback to default event if no custom handlers
                            await _eventBus.EmitAsync("vector.process.file.completed", responsePayload);
                        }

                        _logger.LogInformation("✅ vector.process.file completed in {Duration}ms, created {RecordCount} records", duration, results.Count());
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("FilePath parameter is required for vector.process.file");
                    }
                }
                else
                {
                    throw new ArgumentException("Payload is required for vector.process.file");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ vector.process.file failed");
                await _eventBus.EmitAsync("vector.process.file.failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["duration"] = (DateTime.UtcNow - startTime).TotalMilliseconds
                });
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Add pre-computed vector directly to the store.
        /// </summary>
        /// <param name="content">Text content for the record</param>
        /// <param name="vector">Pre-computed vector</param>
        /// <param name="metadata">Optional metadata</param>
        /// <returns>The created vector record</returns>
        public async Task<VectorRecord> AddVectorAsync(string content, float[] vector, Dictionary<string, object>? metadata = null)
        {
            var record = new VectorRecord
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                Vector = vector,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            await AddAsync(record);
            return record;
        }

        /// <summary>
        /// Issue #256: Search using pre-computed vector.
        /// </summary>
        /// <param name="queryVector">The vector to search with</param>
        /// <param name="topK">Number of results to return</param>
        /// <returns>Search results</returns>
        public Task<IEnumerable<VectorRecord>> SearchVectorAsync(float[] queryVector, int topK = 5)
        {
            return SearchAsync(queryVector, topK);
        }

        /// <summary>
        /// Issue #256: Delete a vector record by ID.
        /// </summary>
        /// <param name="id">The ID of the record to delete</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        public Task<bool> DeleteAsync(string id)
        {
            var removed = _vectorStore.TryRemove(id, out var removedRecord);
            if (removed && removedRecord != null)
            {
                _logger.LogInformation("🗑️ Vector record deleted with ID: {RecordId}", id);
                _ = _eventBus.EmitAsync("vectorstore.record.deleted", new Dictionary<string, object> 
                { 
                    ["Id"] = id,
                    ["Content"] = removedRecord.Content,
                    ["ConsciousnessContext"] = removedRecord.Metadata.ContainsKey("consciousness_aware")
                });
            }
            else
            {
                _logger.LogWarning("🔍 Vector record with ID: {RecordId} not found for deletion", id);
            }
            return Task.FromResult(removed);
        }

        /// <summary>
        /// Issue #256: Update an existing vector record.
        /// </summary>
        /// <param name="record">The updated record</param>
        /// <returns>True if updated successfully, false if not found</returns>
        public async Task<bool> UpdateAsync(VectorRecord record)
        {
            if (string.IsNullOrEmpty(record.Id))
            {
                throw new ArgumentException("Record ID is required for update operation");
            }

            if (_vectorStore.ContainsKey(record.Id))
            {
                // Preserve consciousness context in metadata
                if (!record.Metadata.ContainsKey("consciousness_updated_at"))
                {
                    record.Metadata["consciousness_updated_at"] = DateTimeOffset.UtcNow;
                    record.Metadata["consciousness_aware"] = true;
                }

                _vectorStore[record.Id] = record;
                _logger.LogInformation("🔄 Vector record updated with ID: {RecordId}", record.Id);
                
                await _eventBus.EmitAsync("vectorstore.record.updated", new Dictionary<string, object> 
                { 
                    ["Id"] = record.Id,
                    ["Content"] = record.Content,
                    ["ConsciousnessContext"] = record.Metadata.ContainsKey("consciousness_aware")
                });
                return true;
            }
            else
            {
                _logger.LogWarning("🔍 Vector record with ID: {RecordId} not found for update", record.Id);
                return false;
            }
        }

        /// <summary>
        /// Issue #256: Clear all vector records from the store.
        /// </summary>
        /// <returns>Number of records cleared</returns>
        public async Task<int> ClearAsync()
        {
            var count = _vectorStore.Count;
            _vectorStore.Clear();
            _embeddingCache.Clear();
            
            _logger.LogInformation("🧹 Vector store cleared, removed {RecordCount} records", count);
            
            await _eventBus.EmitAsync("vectorstore.cleared", new Dictionary<string, object> 
            { 
                ["RecordsCleared"] = count,
                ["ClearedAt"] = DateTimeOffset.UtcNow
            });
            
            return count;
        }

        /// <summary>
        /// Issue #256: List all record IDs in the store.
        /// </summary>
        /// <returns>Collection of all record IDs</returns>
        public Task<IEnumerable<string>> ListIdsAsync()
        {
            var ids = _vectorStore.Keys.ToList();
            _logger.LogDebug("📋 Listed {IdCount} vector record IDs", ids.Count);
            return Task.FromResult<IEnumerable<string>>(ids);
        }

        /// <summary>
        /// Issue #256: Get the count of records in the store.
        /// </summary>
        /// <returns>Number of records</returns>
        public Task<int> GetCountAsync()
        {
            var count = _vectorStore.Count;
            _logger.LogDebug("📊 Vector store contains {RecordCount} records", count);
            return Task.FromResult(count);
        }

        /// <summary>
        /// Enhanced for Issue #255: Dispose of resources including persistence timer and semaphore.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _autoPersistenceTimer?.Dispose();
                _persistenceLock?.Dispose();
                _logger.LogDebug("🧹 InMemoryVectorStoreService disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error during InMemoryVectorStoreService disposal");
            }
        }

        #endregion
    }
}

