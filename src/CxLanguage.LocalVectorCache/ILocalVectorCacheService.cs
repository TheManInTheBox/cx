using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.LocalVectorCache
{
    /// <summary>
    /// Interface for consciousness-aware local vector caching service
    /// Part of the Core Engineering Team's local execution excellence initiative
    /// </summary>
    public interface ILocalVectorCacheService
    {
        /// <summary>
        /// Stores a vector in the consciousness-aware local cache
        /// </summary>
        Task<string> StoreAsync(string key, float[] vector, Dictionary<string, object>? metadata = null);
        
        /// <summary>
        /// Retrieves a vector from the consciousness-aware local cache
        /// </summary>
        Task<CachedVectorEntry?> RetrieveAsync(string key);
        
        /// <summary>
        /// Searches for similar vectors using consciousness-aware similarity
        /// </summary>
        Task<IEnumerable<VectorSimilarityResult>> SearchSimilarAsync(
            float[] queryVector, 
            int topK = 5, 
            double minimumSimilarity = 0.7);
        
        /// <summary>
        /// Synchronizes local cache with global vector database
        /// </summary>
        Task<string> SyncWithGlobalAsync(string globalEndpoint);
        
        /// <summary>
        /// Gets comprehensive cache statistics
        /// </summary>
        Task<LocalCacheStatistics> GetStatisticsAsync();
        
        /// <summary>
        /// Clears the local cache (with optional consciousness level filter)
        /// </summary>
        Task<string> ClearAsync(ConsciousnessLevel? levelFilter = null);
    }
    
    /// <summary>
    /// Consciousness levels for vector entries
    /// </summary>
    public enum ConsciousnessLevel
    {
        Basic = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4,
        Consciousness = 5
    }
    
    /// <summary>
    /// Cached vector entry with consciousness awareness
    /// </summary>
    public class CachedVectorEntry
    {
        public required string Key { get; set; }
        public required float[] Vector { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public int AccessCount { get; set; }
        public ConsciousnessLevel ConsciousnessLevel { get; set; }
    }
    
    /// <summary>
    /// Vector similarity search result
    /// </summary>
    public class VectorSimilarityResult
    {
        public required string Key { get; set; }
        public required float[] Vector { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public double Similarity { get; set; }
        public ConsciousnessLevel ConsciousnessLevel { get; set; }
        public DateTime LastAccessedAt { get; set; }
    }
    
    /// <summary>
    /// Local cache statistics
    /// </summary>
    public class LocalCacheStatistics
    {
        public int TotalEntries { get; set; }
        public long TotalMemoryUsage { get; set; }
        public double HitRate { get; set; }
        public double AverageConsciousnessLevel { get; set; }
        public DateTime OldestEntry { get; set; }
        public DateTime NewestEntry { get; set; }
        public string MostAccessedKey { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Cache metrics for consciousness tracking
    /// </summary>
    public class ConsciousCacheMetrics
    {
        public required string Key { get; set; }
        public int StoreCount { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public int DiskHitCount { get; set; }
        public string LastOperation { get; set; } = string.Empty;
        public DateTime LastOperationAt { get; set; }
    }
    
    /// <summary>
    /// Configuration for local vector cache
    /// </summary>
    public class LocalVectorCacheConfig
    {
        public int MaxCacheEntries { get; set; } = 10000;
        public bool PersistToDisk { get; set; } = true;
        public bool EdgeSyncEnabled { get; set; } = true;
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan SyncInterval { get; set; } = TimeSpan.FromSeconds(30);
        public double DefaultSimilarityThreshold { get; set; } = 0.7;
    }
    
    /// <summary>
    /// Edge sync result
    /// </summary>
    public class EdgeSyncResult
    {
        public string Status { get; set; } = string.Empty;
        public int ItemsSynced { get; set; }
        public long DurationMs { get; set; }
    }
}
