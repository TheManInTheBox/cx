using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CxLanguage.Core.Events;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Services.VectorStore
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling - Memory Layer Vector Index Architect
    /// Issue #258: Semantic Search Service Implementation
    /// 
    /// Provides consciousness-aware semantic search with natural language processing,
    /// intelligent result ranking, and context-aware search capabilities.
    /// 
    /// Performance targets:
    /// - Sub-200ms for semantic search queries
    /// - Intelligent result ranking and scoring
    /// - Text snippet extraction from documents
    /// - Agent consciousness context integration
    /// </summary>
    public class SemanticSearchService : ISemanticSearchService
    {
        private readonly ILogger<SemanticSearchService> _logger;
        private readonly ICxEventBus _eventBus;
        private readonly IVectorStoreService _vectorStore;
        private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;

        // Performance tracking
        private long _totalSearches = 0;
        private double _averageSearchTimeMs = 0;
        private readonly object _metricsLock = new object();

        public SemanticSearchService(
            ILogger<SemanticSearchService> logger,
            ICxEventBus eventBus,
            IVectorStoreService vectorStore,
            IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
            _embeddingGenerator = embeddingGenerator;

            _logger.LogInformation("🔍 Dr. Marcus 'MemoryLayer' Sterling's Semantic Search Service initialized for Issue #258");
            
            // Register for semantic search events
            RegisterEventHandlers();
        }

        /// <summary>
        /// Issue #258: Core semantic search functionality with natural language processing.
        /// Optimized for sub-200ms performance with consciousness awareness.
        /// </summary>
        public async Task<SemanticSearchResult> SearchAsync(string query, SemanticSearchOptions? options = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var searchOptions = options ?? new SemanticSearchOptions();

            _logger.LogInformation("🔍 Starting semantic search for query: '{Query}'", query);

            try
            {
                // Emit search start event
                await _eventBus.EmitAsync("semantic.search.start", new Dictionary<string, object>
                {
                    ["query"] = query,
                    ["options"] = searchOptions,
                    ["timestamp"] = DateTimeOffset.UtcNow
                });

                // Perform vector search using existing vector store capabilities
                var vectorResults = await _vectorStore.SearchTextAsync(query, searchOptions.TopK * 2); // Get more results for filtering

                // Apply similarity threshold filtering
                var filteredResults = vectorResults
                    .Select((record, index) => new { Record = record, Index = index })
                    .Where(x => CalculateRelevanceScore(x.Record, query) >= searchOptions.SimilarityThreshold)
                    .Take(searchOptions.TopK)
                    .ToList();

                // Generate ranked results with snippets
                var rankedResults = new List<RankedVectorRecord>();
                
                for (int i = 0; i < filteredResults.Count; i++)
                {
                    var result = filteredResults[i];
                    var rankedRecord = new RankedVectorRecord
                    {
                        Record = result.Record,
                        SimilarityScore = CalculateRelevanceScore(result.Record, query),
                        Rank = i + 1,
                        SearchMetadata = new Dictionary<string, object>
                        {
                            ["search_timestamp"] = DateTimeOffset.UtcNow,
                            ["consciousness_aware"] = result.Record.Metadata.ContainsKey("consciousness_aware"),
                            ["source_type"] = result.Record.Metadata.GetValueOrDefault("source_file", "unknown")
                        }
                    };

                    // Generate snippet if requested
                    if (searchOptions.GenerateSnippets)
                    {
                        rankedRecord.Snippet = await GenerateSnippetAsync(result.Record, query, searchOptions.SnippetLength);
                    }

                    rankedResults.Add(rankedRecord);
                }

                stopwatch.Stop();

                // Create comprehensive search result
                var searchResult = new SemanticSearchResult
                {
                    Results = rankedResults,
                    Query = query,
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    TotalRecordsSearched = vectorResults.Count(),
                    ResultCount = rankedResults.Count,
                    SearchMetadata = new Dictionary<string, object>
                    {
                        ["search_type"] = "semantic_search",
                        ["consciousness_results"] = rankedResults.Count(r => r.SearchMetadata.GetValueOrDefault("consciousness_aware", false).Equals(true)),
                        ["performance_target"] = "200ms",
                        ["similarity_threshold"] = searchOptions.SimilarityThreshold
                    },
                    ResponseSummary = GenerateResponseSummary(rankedResults, query)
                };

                // Update performance metrics
                UpdateSearchMetrics(stopwatch.ElapsedMilliseconds);

                _logger.LogInformation("✅ Semantic search completed in {ElapsedMs}ms with {ResultCount} results (target: <200ms)", 
                    stopwatch.ElapsedMilliseconds, rankedResults.Count);

                if (stopwatch.ElapsedMilliseconds > 200)
                {
                    _logger.LogWarning("⚠️ Semantic search exceeded 200ms target: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                }

                // Emit completion event
                await _eventBus.EmitAsync("semantic.search.complete", new Dictionary<string, object>
                {
                    ["query"] = query,
                    ["processing_time_ms"] = stopwatch.ElapsedMilliseconds,
                    ["result_count"] = rankedResults.Count,
                    ["performance_target"] = "200ms",
                    ["consciousness_results"] = searchResult.SearchMetadata["consciousness_results"]
                });

                return searchResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Semantic search failed for query: '{Query}'", query);

                await _eventBus.EmitAsync("semantic.search.error", new Dictionary<string, object>
                {
                    ["query"] = query,
                    ["error"] = ex.Message,
                    ["processing_time_ms"] = stopwatch.ElapsedMilliseconds
                });

                throw;
            }
        }

        /// <summary>
        /// Issue #258: Agent-specific consciousness-aware semantic search.
        /// Includes agent context and consciousness state in search processing.
        /// </summary>
        public async Task<SemanticSearchResult> SearchWithContextAsync(string query, AgentContext agentContext, SemanticSearchOptions? options = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var searchOptions = options ?? new SemanticSearchOptions();

            _logger.LogInformation("🧠 Starting consciousness-aware search for agent '{AgentId}' with query: '{Query}'", 
                agentContext.AgentId, query);

            try
            {
                // Emit agent search start event
                await _eventBus.EmitAsync("semantic.search.agent.start", new Dictionary<string, object>
                {
                    ["query"] = query,
                    ["agent_id"] = agentContext.AgentId,
                    ["consciousness_state"] = agentContext.ConsciousnessState,
                    ["agent_objectives"] = agentContext.CurrentObjectives,
                    ["timestamp"] = DateTimeOffset.UtcNow
                });

                // Enhance query with consciousness context
                var contextEnhancedQuery = EnhanceQueryWithContext(query, agentContext);

                // Perform base semantic search
                var baseResult = await SearchAsync(contextEnhancedQuery, searchOptions);

                // Apply consciousness-aware re-ranking
                var contextAwareResults = ApplyConsciousnessReranking(baseResult.Results, agentContext, query);

                // Create context-aware result
                var contextResult = new SemanticSearchResult
                {
                    Results = contextAwareResults,
                    Query = query,
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    TotalRecordsSearched = baseResult.TotalRecordsSearched,
                    ResultCount = contextAwareResults.Count(),
                    SearchMetadata = new Dictionary<string, object>(baseResult.SearchMetadata)
                    {
                        ["search_type"] = "consciousness_aware_search",
                        ["agent_id"] = agentContext.AgentId,
                        ["consciousness_state"] = agentContext.ConsciousnessState,
                        ["context_enhanced"] = true,
                        ["agent_objectives_count"] = agentContext.CurrentObjectives.Count()
                    },
                    ResponseSummary = GenerateContextAwareResponseSummary(contextAwareResults, query, agentContext)
                };

                stopwatch.Stop();
                contextResult.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("✅ Consciousness-aware search completed in {ElapsedMs}ms for agent '{AgentId}'", 
                    stopwatch.ElapsedMilliseconds, agentContext.AgentId);

                // Emit agent search completion event
                await _eventBus.EmitAsync("semantic.search.agent.complete", new Dictionary<string, object>
                {
                    ["query"] = query,
                    ["agent_id"] = agentContext.AgentId,
                    ["processing_time_ms"] = stopwatch.ElapsedMilliseconds,
                    ["result_count"] = contextAwareResults.Count(),
                    ["consciousness_enhancement"] = true
                });

                return contextResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Consciousness-aware search failed for agent '{AgentId}'", agentContext.AgentId);

                await _eventBus.EmitAsync("semantic.search.agent.error", new Dictionary<string, object>
                {
                    ["query"] = query,
                    ["agent_id"] = agentContext.AgentId,
                    ["error"] = ex.Message,
                    ["processing_time_ms"] = stopwatch.ElapsedMilliseconds
                });

                throw;
            }
        }

        /// <summary>
        /// Issue #258: Generate meaningful text snippets from search results.
        /// Creates human-readable excerpts with highlighted relevant terms.
        /// </summary>
        public async Task<IEnumerable<SearchSnippet>> GenerateSnippetsAsync(IEnumerable<VectorRecord> results, string query, int snippetLength = 200)
        {
            var stopwatch = Stopwatch.StartNew();
            var snippets = new List<SearchSnippet>();

            try
            {
                var queryTerms = ExtractQueryTerms(query);

                foreach (var result in results)
                {
                    var snippet = await GenerateSnippetAsync(result, query, snippetLength);
                    var highlightedTerms = FindHighlightedTerms(snippet, queryTerms);

                    snippets.Add(new SearchSnippet
                    {
                        Text = snippet,
                        SourceRecordId = result.Id,
                        RelevanceScore = CalculateSnippetRelevance(snippet, query),
                        HighlightedTerms = highlightedTerms,
                        StartPosition = FindSnippetStartPosition(result.Content, snippet),
                        Length = snippet.Length
                    });
                }

                stopwatch.Stop();
                _logger.LogInformation("📝 Generated {SnippetCount} snippets in {ElapsedMs}ms", 
                    snippets.Count, stopwatch.ElapsedMilliseconds);

                return snippets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to generate snippets");
                throw;
            }
        }

        /// <summary>
        /// Issue #258: Get comprehensive search performance metrics.
        /// </summary>
        public Task<Dictionary<string, object>> GetSearchMetricsAsync()
        {
            lock (_metricsLock)
            {
                var metrics = new Dictionary<string, object>
                {
                    ["total_searches"] = _totalSearches,
                    ["average_search_time_ms"] = _averageSearchTimeMs,
                    ["performance_target_ms"] = 200,
                    ["service_type"] = "SemanticSearchService v1.0",
                    ["consciousness_aware"] = true,
                    ["natural_language_processing"] = true,
                    ["snippet_generation"] = true,
                    ["agent_context_support"] = true,
                    ["embedding_generator_available"] = _embeddingGenerator != null
                };

                _logger.LogInformation("📊 Semantic search metrics: {TotalSearches} searches, {AvgTime}ms average", 
                    _totalSearches, _averageSearchTimeMs);

                return Task.FromResult(metrics);
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Register event handlers for CX Language integration
        /// </summary>
        private void RegisterEventHandlers()
        {
            _logger.LogDebug("🔍 Registering semantic search event handlers");

            // Register handler for semantic.search events
            _eventBus.Subscribe("semantic.search", async (sender, eventName, payload) =>
            {
                _logger.LogInformation("🔍 Received semantic.search event with payload keys: {Keys}", 
                    payload != null ? string.Join(", ", payload.Keys) : "null");

                try
                {
                    if (payload?.TryGetValue("query", out var queryObj) == true && queryObj is string query)
                    {
                        _logger.LogInformation("🔍 Processing semantic search query: {Query}", query);
                        
                        var options = new SemanticSearchOptions();
                        
                        // Extract options if provided
                        if (payload.TryGetValue("options", out var optionsObj) && optionsObj is Dictionary<string, object> optionsDict)
                        {
                            if (optionsDict.TryGetValue("topK", out var topKObj) && topKObj is int topK)
                                options.TopK = topK;
                            if (optionsDict.TryGetValue("similarityThreshold", out var thresholdObj) && thresholdObj is double threshold)
                                options.SimilarityThreshold = threshold;
                            if (optionsDict.TryGetValue("generateSnippets", out var snippetsObj) && snippetsObj is bool generateSnippets)
                                options.GenerateSnippets = generateSnippets;
                            if (optionsDict.TryGetValue("snippetLength", out var lengthObj) && lengthObj is int snippetLength)
                                options.SnippetLength = snippetLength;
                            if (optionsDict.TryGetValue("includeMetadata", out var metadataObj) && metadataObj is bool includeMetadata)
                                options.IncludeMetadata = includeMetadata;
                        }

                        // Perform search
                        _logger.LogInformation("🔍 Starting semantic search with options: topK={TopK}, threshold={Threshold}", 
                            options.TopK, options.SimilarityThreshold);
                        var result = await SearchAsync(query, options);
                        _logger.LogInformation("🔍 Search completed with {ResultCount} results", result.ResultCount);

                        // Extract handlers for response
                        var handlers = new List<string>();
                        if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is object[] handlersArray)
                        {
                            handlers.AddRange(handlersArray.OfType<string>());
                        }
                        _logger.LogInformation("🔍 Found {HandlerCount} custom handlers: {Handlers}", 
                            handlers.Count, string.Join(", ", handlers));

                        // Emit completion events
                        foreach (var handler in handlers)
                        {
                            _logger.LogInformation("🔍 Emitting custom handler: {Handler}", handler);
                            await _eventBus.EmitAsync(handler, new Dictionary<string, object>
                            {
                                ["results"] = result.Results.ToList(),
                                ["query"] = query,
                                ["processing_time_ms"] = result.ProcessingTimeMs,
                                ["result_count"] = result.ResultCount,
                                ["consciousness_aware"] = true,
                                ["timestamp"] = DateTimeOffset.UtcNow
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error handling semantic.search event for query: {Query}", 
                        payload?.TryGetValue("query", out var q) == true ? q : "unknown");
                    
                    // Try to emit error to any custom handlers
                    if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                    {
                        var handlers = handlersArray.OfType<string>().ToList();
                        foreach (var handler in handlers)
                        {
                            try
                            {
                                await _eventBus.EmitAsync(handler.Replace(".complete", ".error"), new Dictionary<string, object>
                                {
                                    ["error"] = ex.Message,
                                    ["query"] = payload?.TryGetValue("query", out var qObj) == true ? qObj?.ToString() ?? "unknown" : "unknown",
                                    ["timestamp"] = DateTimeOffset.UtcNow
                                });
                            }
                            catch (Exception emitEx)
                            {
                                _logger.LogError(emitEx, "Failed to emit error event for handler: {Handler}", handler);
                            }
                        }
                    }
                }
                return true;
            });

            // Register handler for semantic.search.agent events
            _eventBus.Subscribe("semantic.search.agent", async (sender, eventName, payload) =>
            {
                try
                {
                    if (payload?.TryGetValue("query", out var queryObj) == true && queryObj is string query)
                    {
                        var options = new SemanticSearchOptions();
                        var agentContext = new AgentContext { AgentId = "unknown", ConsciousnessState = "unknown" };

                        // Extract options if provided
                        if (payload.TryGetValue("options", out var optionsObj) && optionsObj is Dictionary<string, object> optionsDict)
                        {
                            if (optionsDict.TryGetValue("topK", out var topKObj) && topKObj is int topK)
                                options.TopK = topK;
                            if (optionsDict.TryGetValue("similarityThreshold", out var thresholdObj) && thresholdObj is double threshold)
                                options.SimilarityThreshold = threshold;
                            if (optionsDict.TryGetValue("generateSnippets", out var snippetsObj) && snippetsObj is bool generateSnippets)
                                options.GenerateSnippets = generateSnippets;
                        }

                        // Extract agent context if provided
                        if (payload.TryGetValue("agentContext", out var agentContextObj) && agentContextObj is Dictionary<string, object> agentContextDict)
                        {
                            agentContext = new AgentContext
                            {
                                AgentId = agentContextDict.GetValueOrDefault("agentId", "unknown").ToString() ?? "unknown",
                                ConsciousnessState = agentContextDict.GetValueOrDefault("consciousnessState", "unknown").ToString() ?? "unknown"
                            };

                            if (agentContextDict.TryGetValue("currentObjectives", out var objectivesObj) && objectivesObj is object[] objectivesArray)
                            {
                                agentContext.CurrentObjectives = objectivesArray.OfType<string>().ToList();
                            }

                            if (agentContextDict.TryGetValue("memoryContext", out var memoryObj) && memoryObj is Dictionary<string, object> memoryDict)
                            {
                                agentContext.MemoryContext = memoryDict;
                            }
                        }

                        // Perform agent-aware search
                        var result = await SearchWithContextAsync(query, agentContext, options);

                        // Extract handlers for response
                        var handlers = new List<string>();
                        if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is object[] handlersArray)
                        {
                            handlers.AddRange(handlersArray.OfType<string>());
                        }

                        // Emit completion events
                        foreach (var handler in handlers)
                        {
                            await _eventBus.EmitAsync(handler, new Dictionary<string, object>
                            {
                                ["agent_id"] = agentContext.AgentId,
                                ["consciousness_state"] = agentContext.ConsciousnessState,
                                ["results"] = result.Results.ToList(),
                                ["processing_time_ms"] = result.ProcessingTimeMs,
                                ["consciousness_enhancement"] = true,
                                ["timestamp"] = DateTimeOffset.UtcNow
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling semantic.search.agent event");
                }
                return true;
            });

            // Register handler for semantic.search.metrics.request events
            _eventBus.Subscribe("semantic.search.metrics.request", async (sender, eventName, payload) =>
            {
                try
                {
                    var metrics = await GetSearchMetricsAsync();

                    // Extract handlers for response
                    var handlers = new List<string>();
                    if (payload?.TryGetValue("handlers", out var handlersObj) == true && handlersObj is object[] handlersArray)
                    {
                        handlers.AddRange(handlersArray.OfType<string>());
                    }

                    // Emit metrics completion events
                    foreach (var handler in handlers)
                    {
                        await _eventBus.EmitAsync(handler, new Dictionary<string, object>
                        {
                            ["total_searches"] = metrics.GetValueOrDefault("total_searches", 0L),
                            ["average_search_time_ms"] = metrics.GetValueOrDefault("average_search_time_ms", 0.0),
                            ["performance_target_ms"] = 200,
                            ["consciousness_aware"] = true,
                            ["natural_language_processing"] = true,
                            ["snippet_generation"] = true,
                            ["agent_context_support"] = true,
                            ["timestamp"] = DateTimeOffset.UtcNow
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling semantic.search.metrics.request event");
                }
                return true;
            });

            _logger.LogInformation("✅ Semantic search event handlers registered successfully");
        }

        /// <summary>
        /// Calculate relevance score for a vector record
        /// </summary>
        private double CalculateRelevanceScore(VectorRecord record, string query)
        {
            // Basic relevance calculation - can be enhanced with more sophisticated scoring
            var queryTerms = ExtractQueryTerms(query);
            var contentLower = record.Content.ToLowerInvariant();
            
            var termMatches = queryTerms.Count(term => contentLower.Contains(term.ToLowerInvariant()));
            var baseScore = (double)termMatches / queryTerms.Count;

            // Boost score for consciousness-aware records
            if (record.Metadata.ContainsKey("consciousness_aware"))
            {
                baseScore *= 1.1; // 10% boost for consciousness-aware content
            }

            // Boost score for recent content
            if (record.Metadata.TryGetValue("consciousness_processed_at", out var processedAt) 
                && processedAt is DateTimeOffset timestamp)
            {
                var daysSince = (DateTimeOffset.UtcNow - timestamp).TotalDays;
                if (daysSince < 7) // Recent content boost
                {
                    baseScore *= 1.05; // 5% boost for recent content
                }
            }

            return Math.Min(1.0, baseScore); // Cap at 1.0
        }

        /// <summary>
        /// Generate a relevant snippet from content
        /// </summary>
        private async Task<string> GenerateSnippetAsync(VectorRecord record, string query, int maxLength)
        {
            try
            {
                var content = record.Content;
                if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
                {
                    return content;
                }

                var queryTerms = ExtractQueryTerms(query);
                
                // Find the best position to extract snippet
                var bestPosition = FindBestSnippetPosition(content, queryTerms, maxLength);
                
                // Extract snippet around best position
                var start = Math.Max(0, bestPosition - maxLength / 2);
                var length = Math.Min(maxLength, content.Length - start);
                
                var snippet = content.Substring(start, length);
                
                // Clean up snippet boundaries
                snippet = CleanSnippetBoundaries(snippet);
                
                return await Task.FromResult(snippet);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate snippet for record {RecordId}", record.Id);
                return record.Content.Length > maxLength 
                    ? record.Content.Substring(0, maxLength) + "..."
                    : record.Content;
            }
        }

        /// <summary>
        /// Extract meaningful terms from query
        /// </summary>
        private List<string> ExtractQueryTerms(string query)
        {
            var terms = query.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(term => term.Length > 2) // Filter out short words
                .Select(term => term.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            return terms;
        }

        /// <summary>
        /// Find best position for snippet extraction
        /// </summary>
        private int FindBestSnippetPosition(string content, List<string> queryTerms, int maxLength)
        {
            var contentLower = content.ToLowerInvariant();
            var bestScore = 0;
            var bestPosition = 0;

            // Scan through content in chunks
            for (int i = 0; i < content.Length - maxLength; i += maxLength / 4)
            {
                var chunk = contentLower.Substring(i, Math.Min(maxLength, content.Length - i));
                var score = queryTerms.Count(term => chunk.Contains(term));
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = i;
                }
            }

            return bestPosition;
        }

        /// <summary>
        /// Clean snippet boundaries to avoid cutting words
        /// </summary>
        private string CleanSnippetBoundaries(string snippet)
        {
            if (string.IsNullOrEmpty(snippet))
                return snippet;

            // Find word boundaries
            var sentences = snippet.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length > 1)
            {
                // Return complete sentences
                return string.Join(". ", sentences.Take(sentences.Length - 1)) + ".";
            }

            // If no sentences, try to break at word boundaries
            var lastSpace = snippet.LastIndexOf(' ');
            if (lastSpace > snippet.Length * 0.8) // Only if we're not cutting too much
            {
                return snippet.Substring(0, lastSpace) + "...";
            }

            return snippet + "...";
        }

        /// <summary>
        /// Enhance query with agent context
        /// </summary>
        private string EnhanceQueryWithContext(string query, AgentContext agentContext)
        {
            var enhancedQuery = query;

            // Add agent objectives to query context
            if (agentContext.CurrentObjectives.Any())
            {
                var objectives = string.Join(" ", agentContext.CurrentObjectives);
                enhancedQuery = $"{query} {objectives}";
            }

            // Add consciousness state context
            if (!string.IsNullOrEmpty(agentContext.ConsciousnessState))
            {
                enhancedQuery = $"{enhancedQuery} {agentContext.ConsciousnessState}";
            }

            return enhancedQuery;
        }

        /// <summary>
        /// Apply consciousness-aware re-ranking of results
        /// </summary>
        private IEnumerable<RankedVectorRecord> ApplyConsciousnessReranking(
            IEnumerable<RankedVectorRecord> results, 
            AgentContext agentContext, 
            string originalQuery)
        {
            return results.Select(result =>
            {
                var newScore = result.SimilarityScore;

                // Boost based on agent objectives alignment
                foreach (var objective in agentContext.CurrentObjectives)
                {
                    if (result.Record.Content.ToLowerInvariant().Contains(objective.ToLowerInvariant()))
                    {
                        newScore *= 1.15; // 15% boost for objective alignment
                    }
                }

                // Boost based on consciousness state alignment
                if (!string.IsNullOrEmpty(agentContext.ConsciousnessState) &&
                    result.Record.Content.ToLowerInvariant().Contains(agentContext.ConsciousnessState.ToLowerInvariant()))
                {
                    newScore *= 1.1; // 10% boost for consciousness state alignment
                }

                result.SimilarityScore = Math.Min(1.0, newScore);
                return result;
            })
            .OrderByDescending(r => r.SimilarityScore)
            .Select((r, index) => { r.Rank = index + 1; return r; });
        }

        /// <summary>
        /// Generate response summary from ranked results
        /// </summary>
        private string GenerateResponseSummary(IEnumerable<RankedVectorRecord> results, string query)
        {
            if (!results.Any())
                return $"No results found for query: '{query}'";

            var topResult = results.First();
            var resultCount = results.Count();

            return $"Found {resultCount} relevant results for '{query}'. " +
                   $"Top result has {topResult.SimilarityScore:P0} relevance. " +
                   $"{results.Count(r => r.SearchMetadata.GetValueOrDefault("consciousness_aware", false).Equals(true))} results are consciousness-aware.";
        }

        /// <summary>
        /// Generate context-aware response summary
        /// </summary>
        private string GenerateContextAwareResponseSummary(
            IEnumerable<RankedVectorRecord> results, 
            string query, 
            AgentContext agentContext)
        {
            var baseSummary = GenerateResponseSummary(results, query);
            
            if (agentContext.CurrentObjectives.Any())
            {
                var objectiveMatches = results.Count(r => 
                    agentContext.CurrentObjectives.Any(obj => 
                        r.Record.Content.ToLowerInvariant().Contains(obj.ToLowerInvariant())));
                
                baseSummary += $" {objectiveMatches} results align with agent objectives.";
            }

            return baseSummary;
        }

        /// <summary>
        /// Calculate snippet relevance score
        /// </summary>
        private double CalculateSnippetRelevance(string snippet, string query)
        {
            var queryTerms = ExtractQueryTerms(query);
            var snippetLower = snippet.ToLowerInvariant();
            
            var termMatches = queryTerms.Count(term => snippetLower.Contains(term.ToLowerInvariant()));
            return (double)termMatches / queryTerms.Count;
        }

        /// <summary>
        /// Find highlighted terms in snippet
        /// </summary>
        private IEnumerable<string> FindHighlightedTerms(string snippet, List<string> queryTerms)
        {
            var snippetLower = snippet.ToLowerInvariant();
            return queryTerms.Where(term => snippetLower.Contains(term.ToLowerInvariant()));
        }

        /// <summary>
        /// Find snippet start position in original content
        /// </summary>
        private int FindSnippetStartPosition(string originalContent, string snippet)
        {
            if (string.IsNullOrEmpty(snippet) || snippet.Length < 10)
                return 0;

            var cleanSnippet = snippet.Replace("...", "").Trim();
            var position = originalContent.IndexOf(cleanSnippet, StringComparison.OrdinalIgnoreCase);
            return Math.Max(0, position);
        }

        /// <summary>
        /// Update search performance metrics
        /// </summary>
        private void UpdateSearchMetrics(double searchTimeMs)
        {
            lock (_metricsLock)
            {
                _totalSearches++;
                _averageSearchTimeMs = ((_averageSearchTimeMs * (_totalSearches - 1)) + searchTimeMs) / _totalSearches;
            }
        }

        #endregion
    }
}
