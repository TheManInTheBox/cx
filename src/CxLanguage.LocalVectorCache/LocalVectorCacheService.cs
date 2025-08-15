using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.LocalVectorCache
{
    /// <summary>
    /// Dr. Marcus "LocalLLM" Chen - Local Vector Cache Service
    /// High-performance, consciousness-aware local vector caching with real-time synchronization
    /// Part of the Core Engineering Team's local execution excellence initiative
    /// </summary>
    public class LocalVectorCacheService : ILocalVectorCacheService, IDisposable
    {
        private readonly ILogger<LocalVectorCacheService> _logger;
        private readonly ICxEventBus _eventBus;
        
        // Consciousness-aware cache storage
        private readonly ConcurrentDictionary<string, CachedVectorEntry> _vectorCache = new();
        private readonly ConcurrentDictionary<string, ConsciousCacheMetrics> _cacheMetrics = new();
        private readonly Timer _cleanupTimer;
        private readonly Timer _syncTimer;
        
        // Cache configuration
        private readonly LocalVectorCacheConfig _config;
        private readonly SemaphoreSlim _persistenceLock = new(1, 1);
        private readonly string _cacheDirectory;
        
        // Consciousness processing components
        private readonly ConsciousnessCacheProcessor _consciousnessProcessor;
        private readonly VectorSimilarityEngine _similarityEngine;
        private readonly EdgeSyncCoordinator _edgeSyncCoordinator;
        
        public LocalVectorCacheService(
            ILogger<LocalVectorCacheService> logger,
            ICxEventBus eventBus,
            LocalVectorCacheConfig? config = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _config = config ?? new LocalVectorCacheConfig();
            
            // Initialize cache directory
            _cacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CxLanguage", "VectorCache");
            Directory.CreateDirectory(_cacheDirectory);
            
            // Initialize consciousness-aware components
            _consciousnessProcessor = new ConsciousnessCacheProcessor(_logger, _eventBus);
            _similarityEngine = new VectorSimilarityEngine(_logger);
            _edgeSyncCoordinator = new EdgeSyncCoordinator(_logger, _eventBus, _config.EdgeSyncEnabled);
            
            // Setup cleanup and sync timers
            _cleanupTimer = new Timer(PerformCleanup, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            _syncTimer = new Timer(PerformSync, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            
            _logger.LogInformation("🧠 LocalVectorCacheService initialized with consciousness awareness");
            _eventBus.EmitAsync("local.vector.cache.initialized", new Dictionary<string, object>
            {
                ["service"] = nameof(LocalVectorCacheService),
                ["cacheDirectory"] = _cacheDirectory,
                ["maxEntries"] = _config.MaxCacheEntries,
                ["edgeSyncEnabled"] = _config.EdgeSyncEnabled
            });
        }
        
        /// <summary>
        /// Stores a vector in the consciousness-aware local cache
        /// </summary>
        public async Task<string> StoreAsync(string key, float[] vector, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var entry = new CachedVectorEntry
                {
                    Key = key,
                    Vector = vector,
                    Metadata = metadata ?? new Dictionary<string, object>(),
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow,
                    AccessCount = 1,
                    ConsciousnessLevel = await _consciousnessProcessor.DetermineConsciousnessLevelAsync(vector, metadata)
                };
                
                // Store in memory cache
                _vectorCache.AddOrUpdate(key, entry, (k, existingEntry) =>
                {
                    existingEntry.Vector = vector;
                    existingEntry.Metadata = metadata ?? new Dictionary<string, object>();
                    existingEntry.LastAccessedAt = DateTime.UtcNow;
                    existingEntry.AccessCount++;
                    existingEntry.ConsciousnessLevel = entry.ConsciousnessLevel;
                    return existingEntry;
                });
                
                // Update metrics
                UpdateCacheMetrics(key, "store");
                
                // Persist to disk if enabled
                if (_config.PersistToDisk)
                {
                    await PersistEntryAsync(entry);
                }
                
                // Emit consciousness event
                await _eventBus.EmitAsync("local.vector.stored", new Dictionary<string, object>
                {
                    ["key"] = key,
                    ["vectorDimensions"] = vector.Length,
                    ["consciousnessLevel"] = entry.ConsciousnessLevel,
                    ["cacheSize"] = _vectorCache.Count
                });
                
                _logger.LogDebug("📦 Vector stored in local cache: {Key} (consciousness: {Level})", 
                    key, entry.ConsciousnessLevel);
                
                return $"Vector {key} stored successfully in local cache with consciousness level {entry.ConsciousnessLevel}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing vector in local cache: {Key}", key);
                throw;
            }
        }
        
        /// <summary>
        /// Retrieves a vector from the consciousness-aware local cache
        /// </summary>
        public async Task<CachedVectorEntry?> RetrieveAsync(string key)
        {
            try
            {
                if (_vectorCache.TryGetValue(key, out var entry))
                {
                    // Update access statistics
                    entry.LastAccessedAt = DateTime.UtcNow;
                    entry.AccessCount++;
                    
                    // Update metrics
                    UpdateCacheMetrics(key, "hit");
                    
                    // Emit consciousness event
                    await _eventBus.EmitAsync("local.vector.retrieved", new Dictionary<string, object>
                    {
                        ["key"] = key,
                        ["accessCount"] = entry.AccessCount,
                        ["consciousnessLevel"] = entry.ConsciousnessLevel,
                        ["cacheHit"] = true
                    });
                    
                    _logger.LogDebug("🎯 Cache hit for vector: {Key} (access count: {Count})", 
                        key, entry.AccessCount);
                    
                    return entry;
                }
                
                // Cache miss - try to load from disk
                if (_config.PersistToDisk)
                {
                    var diskEntry = await LoadEntryFromDiskAsync(key);
                    if (diskEntry != null)
                    {
                        _vectorCache.TryAdd(key, diskEntry);
                        UpdateCacheMetrics(key, "disk_hit");
                        
                        await _eventBus.EmitAsync("local.vector.retrieved", new Dictionary<string, object>
                        {
                            ["key"] = key,
                            ["accessCount"] = diskEntry.AccessCount,
                            ["consciousnessLevel"] = diskEntry.ConsciousnessLevel,
                            ["cacheHit"] = false,
                            ["diskHit"] = true
                        });
                        
                        _logger.LogDebug("💾 Disk hit for vector: {Key}", key);
                        return diskEntry;
                    }
                }
                
                // Complete miss
                UpdateCacheMetrics(key, "miss");
                
                await _eventBus.EmitAsync("local.vector.miss", new Dictionary<string, object>
                {
                    ["key"] = key,
                    ["cacheHit"] = false,
                    ["diskHit"] = false
                });
                
                _logger.LogDebug("❌ Cache miss for vector: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving vector from local cache: {Key}", key);
                throw;
            }
        }
        
        /// <summary>
        /// Searches for similar vectors using consciousness-aware similarity
        /// </summary>
        public async Task<IEnumerable<VectorSimilarityResult>> SearchSimilarAsync(
            float[] queryVector, 
            int topK = 5, 
            double minimumSimilarity = 0.7)
        {
            try
            {
                var queryConsciousness = await _consciousnessProcessor.DetermineConsciousnessLevelAsync(queryVector);
                
                var similarities = new List<VectorSimilarityResult>();
                
                foreach (var kvp in _vectorCache)
                {
                    var entry = kvp.Value;
                    var similarity = _similarityEngine.CalculateConsciousnessSimilarity(
                        queryVector, 
                        entry.Vector, 
                        queryConsciousness, 
                        entry.ConsciousnessLevel);
                    
                    if (similarity >= minimumSimilarity)
                    {
                        similarities.Add(new VectorSimilarityResult
                        {
                            Key = entry.Key,
                            Vector = entry.Vector,
                            Metadata = entry.Metadata,
                            Similarity = similarity,
                            ConsciousnessLevel = entry.ConsciousnessLevel,
                            LastAccessedAt = entry.LastAccessedAt
                        });
                    }
                }
                
                var results = similarities
                    .OrderByDescending(s => s.Similarity)
                    .ThenByDescending(s => s.ConsciousnessLevel)
                    .Take(topK)
                    .ToList();
                
                // Emit consciousness search event
                await _eventBus.EmitAsync("local.vector.search.complete", new Dictionary<string, object>
                {
                    ["queryConsciousness"] = queryConsciousness,
                    ["resultsCount"] = results.Count,
                    ["topSimilarity"] = results.FirstOrDefault()?.Similarity ?? 0.0,
                    ["searchLatencyMs"] = 0 // Would be measured in real implementation
                });
                
                _logger.LogDebug("🔍 Consciousness-aware search complete: {ResultCount} results", results.Count);
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error searching similar vectors");
                throw;
            }
        }
        
        /// <summary>
        /// Synchronizes local cache with global vector database
        /// </summary>
        public async Task<string> SyncWithGlobalAsync(string globalEndpoint)
        {
            try
            {
                var syncResult = await _edgeSyncCoordinator.SyncWithGlobalAsync(globalEndpoint, _vectorCache);
                
                await _eventBus.EmitAsync("local.vector.sync.complete", new Dictionary<string, object>
                {
                    ["globalEndpoint"] = globalEndpoint,
                    ["syncResult"] = syncResult.Status,
                    ["itemsSynced"] = syncResult.ItemsSynced,
                    ["syncDurationMs"] = syncResult.DurationMs
                });
                
                _logger.LogInformation("🌐 Global sync complete: {ItemsSynced} items synced", syncResult.ItemsSynced);
                
                return $"Sync complete: {syncResult.ItemsSynced} items synced with {globalEndpoint}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error syncing with global vector database");
                throw;
            }
        }
        
        /// <summary>
        /// Gets comprehensive cache statistics
        /// </summary>
        public async Task<LocalCacheStatistics> GetStatisticsAsync()
        {
            var stats = new LocalCacheStatistics
            {
                TotalEntries = _vectorCache.Count,
                TotalMemoryUsage = CalculateMemoryUsage(),
                HitRate = CalculateHitRate(),
                AverageConsciousnessLevel = _vectorCache.Values.Average(v => (double)v.ConsciousnessLevel),
                OldestEntry = _vectorCache.Values.Any() ? _vectorCache.Values.Min(v => v.CreatedAt) : DateTime.UtcNow,
                NewestEntry = _vectorCache.Values.Any() ? _vectorCache.Values.Max(v => v.CreatedAt) : DateTime.UtcNow,
                MostAccessedKey = _vectorCache.Values.Any() ? 
                    _vectorCache.Values.OrderByDescending(v => v.AccessCount).First().Key : 
                    string.Empty
            };
            
            await _eventBus.EmitAsync("local.vector.statistics.requested", new Dictionary<string, object>
            {
                ["totalEntries"] = stats.TotalEntries,
                ["memoryUsageMB"] = stats.TotalMemoryUsage / 1024 / 1024,
                ["hitRate"] = stats.HitRate,
                ["avgConsciousness"] = stats.AverageConsciousnessLevel
            });
            
            return stats;
        }
        
        /// <summary>
        /// Clears the local cache (with optional consciousness level filter)
        /// </summary>
        public async Task<string> ClearAsync(ConsciousnessLevel? levelFilter = null)
        {
            try
            {
                int clearedCount = 0;
                
                if (levelFilter.HasValue)
                {
                    var keysToRemove = _vectorCache
                        .Where(kvp => kvp.Value.ConsciousnessLevel == levelFilter.Value)
                        .Select(kvp => kvp.Key)
                        .ToList();
                    
                    foreach (var key in keysToRemove)
                    {
                        if (_vectorCache.TryRemove(key, out _))
                        {
                            clearedCount++;
                        }
                    }
                }
                else
                {
                    clearedCount = _vectorCache.Count;
                    _vectorCache.Clear();
                    _cacheMetrics.Clear();
                }
                
                await _eventBus.EmitAsync("local.vector.cache.cleared", new Dictionary<string, object>
                {
                    ["clearedCount"] = clearedCount,
                    ["levelFilter"] = levelFilter?.ToString() ?? "all",
                    ["remainingEntries"] = _vectorCache.Count
                });
                
                _logger.LogInformation("🧹 Local cache cleared: {ClearedCount} entries removed", clearedCount);
                
                return $"Cleared {clearedCount} entries from local cache";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error clearing local cache");
                throw;
            }
        }
        
        // Private helper methods
        
        private void UpdateCacheMetrics(string key, string operation)
        {
            _cacheMetrics.AddOrUpdate(key, 
                new ConsciousCacheMetrics { Key = key },
                (k, existing) =>
                {
                    switch (operation)
                    {
                        case "store":
                            existing.StoreCount++;
                            break;
                        case "hit":
                            existing.HitCount++;
                            break;
                        case "miss":
                            existing.MissCount++;
                            break;
                        case "disk_hit":
                            existing.DiskHitCount++;
                            break;
                    }
                    existing.LastOperation = operation;
                    existing.LastOperationAt = DateTime.UtcNow;
                    return existing;
                });
        }
        
        private async Task PersistEntryAsync(CachedVectorEntry entry)
        {
            await _persistenceLock.WaitAsync();
            try
            {
                var filePath = Path.Combine(_cacheDirectory, $"{entry.Key}.json");
                var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
            finally
            {
                _persistenceLock.Release();
            }
        }
        
        private async Task<CachedVectorEntry?> LoadEntryFromDiskAsync(string key)
        {
            await _persistenceLock.WaitAsync();
            try
            {
                var filePath = Path.Combine(_cacheDirectory, $"{key}.json");
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    return JsonSerializer.Deserialize<CachedVectorEntry>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error loading entry from disk: {Key}", key);
                return null;
            }
            finally
            {
                _persistenceLock.Release();
            }
        }
        
        private long CalculateMemoryUsage()
        {
            long totalBytes = 0;
            foreach (var entry in _vectorCache.Values)
            {
                totalBytes += entry.Vector.Length * sizeof(float);
                totalBytes += entry.Key.Length * sizeof(char);
                // Add metadata size estimation
                totalBytes += 1024; // Approximate metadata overhead
            }
            return totalBytes;
        }
        
        private double CalculateHitRate()
        {
            var totalHits = _cacheMetrics.Values.Sum(m => m.HitCount + m.DiskHitCount);
            var totalRequests = _cacheMetrics.Values.Sum(m => m.HitCount + m.DiskHitCount + m.MissCount);
            return totalRequests > 0 ? (double)totalHits / totalRequests : 0.0;
        }
        
        private void PerformCleanup(object? state)
        {
            try
            {
                if (_vectorCache.Count <= _config.MaxCacheEntries) return;
                
                var entriesToRemove = _vectorCache.Values
                    .OrderBy(v => v.LastAccessedAt)
                    .ThenBy(v => v.ConsciousnessLevel)
                    .Take(_vectorCache.Count - _config.MaxCacheEntries)
                    .Select(v => v.Key)
                    .ToList();
                
                foreach (var key in entriesToRemove)
                {
                    _vectorCache.TryRemove(key, out _);
                }
                
                _logger.LogDebug("🧹 Cache cleanup: {RemovedCount} entries removed", entriesToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during cache cleanup");
            }
        }
        
        private void PerformSync(object? state)
        {
            try
            {
                if (!_config.EdgeSyncEnabled) return;
                
                // Background sync logic would go here
                _logger.LogDebug("🌐 Background sync check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during background sync");
            }
        }
        
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _syncTimer?.Dispose();
            _persistenceLock?.Dispose();
            _consciousnessProcessor?.Dispose();
            _edgeSyncCoordinator?.Dispose();
            
            _logger.LogInformation("🧹 LocalVectorCacheService disposed");
        }
    }
}
