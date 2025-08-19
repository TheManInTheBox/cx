using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CxLanguage.Core.Events;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Services.VectorStore
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling's high-performance, in-memory vector store.
    /// This service provides a zero-dependency, local-first vector database solution
    /// optimized for the Aura Cognitive Framework and consciousness-native processing.
    /// </summary>
    public class InMemoryVectorStoreService : IVectorStoreService
    {
        private readonly ILogger<InMemoryVectorStoreService> _logger;
        private readonly ICxEventBus _eventBus;

        /// <summary>
        /// The core in-memory data store. Using ConcurrentDictionary for thread-safe operations,
        /// which is essential for a multi-agent, event-driven environment.
        /// </summary>
        private readonly ConcurrentDictionary<string, VectorRecord> _vectorStore = new();

        public InMemoryVectorStoreService(ILogger<InMemoryVectorStoreService> logger, ICxEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
            _logger.LogInformation("🧠 Dr. Marcus 'MemoryLayer' Sterling's InMemoryVectorStoreService initialized.");
            _eventBus.EmitAsync("vectorstore.initialized", new Dictionary<string, object> { ["service"] = nameof(InMemoryVectorStoreService) });
        }

        /// <summary>
        /// Adds a new vector record to the in-memory store.
        /// </summary>
        /// <param name="record">The vector record to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task AddAsync(VectorRecord record)
        {
            if (string.IsNullOrEmpty(record.Id))
            {
                record.Id = Guid.NewGuid().ToString();
            }

            _vectorStore[record.Id] = record;
            _logger.LogDebug("Vector record added with ID: {RecordId}", record.Id);
            _eventBus.EmitAsync("vectorstore.record.added", new Dictionary<string, object> { ["Id"] = record.Id, ["Content"] = record.Content });
            
            return Task.CompletedTask;
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
        /// </summary>
        /// <param name="queryVector">The vector to compare against.</param>
        /// <param name="topK">The number of top results to return.</param>
        /// <returns>A collection of the most similar vector records.</returns>
        public Task<IEnumerable<VectorRecord>> SearchAsync(float[] queryVector, int topK = 5)
        {
            if (_vectorStore.IsEmpty)
            {
                _logger.LogWarning("SearchAsync called on an empty vector store.");
                return Task.FromResult(Enumerable.Empty<VectorRecord>());
            }

            var results = _vectorStore.Values
                .Select(record => new { Record = record, Similarity = CosineSimilarity(record.Vector, queryVector) })
                .OrderByDescending(x => x.Similarity)
                .Take(topK)
                .Select(x => x.Record);
            
            _logger.LogInformation("Search completed. Found {ResultCount} results.", results.Count());
            _eventBus.EmitAsync("vectorstore.search.complete", new Dictionary<string, object> { ["QueryVectorLength"] = queryVector.Length, ["ResultCount"] = results.Count() });

            return Task.FromResult(results);
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
    }
}

