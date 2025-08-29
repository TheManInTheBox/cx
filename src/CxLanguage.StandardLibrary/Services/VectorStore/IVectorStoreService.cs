using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Services.VectorStore
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling - Memory Layer Vector Index Architect
    /// Defines the contract for a consciousness-aware vector store service.
    /// Enhanced for Issue #252: Native embedding generation and FileService integration.
    /// Enhanced for Issue #255: File persistence with consciousness context preservation.
    /// </summary>
    public interface IVectorStoreService
    {
        /// <summary>
        /// Adds or updates a vector record in the store.
        /// </summary>
        /// <param name="record">The vector record to add or update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(VectorRecord record);

        /// <summary>
        /// Enhanced for Issue #252: Add text content directly with automatic embedding generation.
        /// Optimized for sub-50ms performance with consciousness context preservation.
        /// </summary>
        /// <param name="content">The text content to vectorize and store</param>
        /// <param name="metadata">Optional metadata to associate with the record</param>
        /// <returns>The generated vector record</returns>
        Task<VectorRecord> AddTextAsync(string content, Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Searches for similar vectors in the store.
        /// Enhanced for Issue #252: Optimized for sub-100ms performance.
        /// </summary>
        /// <param name="vector">The vector to search for.</param>
        /// <param name="topK">The number of similar records to return.</param>
        /// <returns>A collection of similar vector records.</returns>
        Task<IEnumerable<VectorRecord>> SearchAsync(float[] vector, int topK);

        /// <summary>
        /// Enhanced for Issue #252: Search using text query with automatic embedding generation.
        /// Supports consciousness-aware text-based queries.
        /// </summary>
        /// <param name="query">Text query to search for</param>
        /// <param name="topK">Number of results to return</param>
        /// <returns>Most similar vector records</returns>
        Task<IEnumerable<VectorRecord>> SearchTextAsync(string query, int topK = 5);

        /// <summary>
        /// Retrieves a vector record by its ID.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve.</param>
        /// <returns>The vector record, or null if not found.</returns>
        Task<VectorRecord?> GetAsync(string id);

        /// <summary>
        /// Enhanced for Issue #252: Process file content directly with FileService integration.
        /// Supports consciousness-aware document processing with automatic chunking.
        /// </summary>
        /// <param name="filePath">Path to the file to process</param>
        /// <param name="chunkSize">Size of text chunks for vectorization (default: 1000 chars)</param>
        /// <param name="metadata">Additional metadata to associate with the records</param>
        /// <returns>List of created vector records</returns>
        Task<IEnumerable<VectorRecord>> ProcessFileAsync(string filePath, int chunkSize = 1000, Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Enhanced for Issue #252: Get performance metrics for consciousness monitoring.
        /// </summary>
        /// <returns>Performance and consciousness metrics</returns>
        Task<Dictionary<string, object>> GetMetricsAsync();

        /// <summary>
        /// Enhanced for Issue #255: Save vector store to persistent storage.
        /// Implements file-based persistence with consciousness context preservation.
        /// </summary>
        /// <param name="baseDirectory">Base directory for storage (optional, uses default if null)</param>
        /// <returns>Success status and saved file information</returns>
        Task<Dictionary<string, object>> SaveToPersistentStorageAsync(string? baseDirectory = null);

        /// <summary>
        /// Enhanced for Issue #255: Load vector store from persistent storage.
        /// Implements automatic recovery with consciousness context restoration.
        /// </summary>
        /// <param name="baseDirectory">Base directory for storage (optional, uses default if null)</param>
        /// <returns>Success status and loaded record information</returns>
        Task<Dictionary<string, object>> LoadFromPersistentStorageAsync(string? baseDirectory = null);

        /// <summary>
        /// Enhanced for Issue #255: Enable/disable automatic persistence.
        /// Background persistence for real-time consciousness memory retention.
        /// </summary>
        /// <param name="enabled">Whether to enable automatic persistence</param>
        /// <param name="intervalSeconds">Persistence interval in seconds (default: 30)</param>
        /// <returns>Configuration status</returns>
        Task<bool> SetAutoPersistenceAsync(bool enabled, int intervalSeconds = 30);
    }
}

