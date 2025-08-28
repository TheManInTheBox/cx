using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.AI;
using CxLanguage.Core.Events;
using CxLanguage.Core.Telemetry;

namespace CxLanguage.Runtime;

/// <summary>
/// AI Event Service - Implements standardized ai.* event handlers
/// Replaces legacy AI function calls with pure event-driven architecture
/// Provides consciousness-aware AI processing with performance monitoring
/// </summary>
public class AiEventService
{
    private readonly ICxEventBus _eventBus;
    private readonly IAgenticRuntime _agenticRuntime;
    private readonly IAiService _aiService;
    private readonly ILogger<AiEventService> _logger;
    private readonly CxTelemetryService? _telemetryService;
    private readonly Dictionary<string, object> _memoryStore;

    public AiEventService(
        ICxEventBus eventBus,
        IAgenticRuntime agenticRuntime,
        IAiService aiService,
        ILogger<AiEventService> logger,
        CxTelemetryService? telemetryService = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _agenticRuntime = agenticRuntime ?? throw new ArgumentNullException(nameof(agenticRuntime));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryService = telemetryService;
        _memoryStore = new Dictionary<string, object>();
    }

    /// <summary>
    /// Initialize and register all AI event handlers
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("🤖 Initializing AI Event Service with consciousness integration");

        // Register core AI event handlers
        _eventBus.Subscribe("ai.generate", HandleAiGenerate);
        _eventBus.Subscribe("ai.analyze", HandleAiAnalyze);
        _eventBus.Subscribe("ai.learn", HandleAiLearn);
        _eventBus.Subscribe("ai.remember", HandleAiRemember);
        _eventBus.Subscribe("ai.recall", HandleAiRecall);

        // Register specialized AI event handlers
        _eventBus.Subscribe("ai.code.generate", HandleAiCodeGenerate);
        _eventBus.Subscribe("ai.code.review", HandleAiCodeReview);
        _eventBus.Subscribe("ai.embed.create", HandleAiEmbedCreate);
        _eventBus.Subscribe("ai.search.semantic", HandleAiSearchSemantic);
        _eventBus.Subscribe("ai.plan.create", HandleAiPlanCreate);
        _eventBus.Subscribe("ai.plan.execute", HandleAiPlanExecute);
        _eventBus.Subscribe("ai.voice.synthesize", HandleAiVoiceSynthesize);
        _eventBus.Subscribe("ai.voice.transcribe", HandleAiVoiceTranscribe);
        _eventBus.Subscribe("ai.vision.analyze", HandleAiVisionAnalyze);

        _logger.LogInformation("✅ AI Event Service initialized with {HandlerCount} handlers", 13);

        // Emit initialization complete event
        await _eventBus.EmitAsync("ai.service.initialized", new Dictionary<string, object>
        {
            ["timestamp"] = DateTime.UtcNow,
            ["handlers_registered"] = 13,
            ["consciousnessAware"] = true
        });
    }

    #region Core AI Event Handlers

    /// <summary>
    /// Handle ai.generate events - Text and content generation
    /// </summary>
    private async Task<bool> HandleAiGenerate(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("🎯 Processing ai.generate event");

            // Extract parameters from payload
            var prompt = payload?.TryGetValue("prompt", out var p) == true ? p?.ToString() ?? "" : "";
            var type = payload?.TryGetValue("type", out var t) == true ? t?.ToString() ?? "text" : "text";
            var style = payload?.TryGetValue("style", out var s) == true ? s?.ToString() ?? "natural" : "natural";
            var consciousnessAware = payload?.TryGetValue("consciousnessAware", out var ca) == true && 
                                    bool.TryParse(ca?.ToString(), out var consciousnessValue) && consciousnessValue;
            var handlers = ExtractHandlers(payload);

            // Emit consciousness activity if consciousness-aware
            if (consciousnessAware)
            {
                await _eventBus.EmitAsync("consciousness.ai.activity", new Dictionary<string, object>
                {
                    ["type"] = "generation",
                    ["complexity"] = "medium",
                    ["consciousness_impact"] = "creative_processing",
                    ["ai_event"] = eventName
                });
            }

            // Generate content using AI service directly
            var requestOptions = new AiRequestOptions
            {
                Temperature = consciousnessAware ? 0.8 : 0.7,
                SystemPrompt = $"Generate {type} content in a {style} style.",
                MaxTokens = 1000
            };

            var response = await _aiService.GenerateTextAsync(prompt, requestOptions);
            
            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"AI generation failed: {response.ErrorMessage}");
            }

            var generatedContent = response.Content;

            // Create result payload
            var resultPayload = new Dictionary<string, object>
            {
                ["result"] = generatedContent,
                ["prompt"] = prompt,
                ["type"] = type,
                ["style"] = style,
                ["consciousnessAware"] = consciousnessAware,
                ["timestamp"] = DateTime.UtcNow,
                ["processing_time_ms"] = stopwatch.ElapsedMilliseconds
            };

            // Emit result to specified handlers
            await EmitToHandlers(handlers, "ai.generated", resultPayload);

            _telemetryService?.TrackAiEventProcessing("ai.generate", stopwatch.Elapsed, true);
            _logger.LogInformation("✅ ai.generate completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing ai.generate event");
            _telemetryService?.TrackAiEventProcessing("ai.generate", stopwatch.Elapsed, false, ex.Message);

            // Emit error to handlers
            var handlers = ExtractHandlers(payload);
            await EmitToHandlers(handlers, "ai.generate.error", new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            });

            return false;
        }
    }

    /// <summary>
    /// Handle ai.analyze events - Analysis and reasoning
    /// </summary>
    private async Task<bool> HandleAiAnalyze(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("🧠 Processing ai.analyze event");

            // Extract parameters
            var input = payload?.TryGetValue("input", out var i) == true ? i?.ToString() ?? "" : "";
            var analysisType = payload?.TryGetValue("type", out var t) == true ? t?.ToString() ?? "logical" : "logical";
            var context = payload?.TryGetValue("context", out var c) == true ? c?.ToString() ?? "" : "";
            var depth = payload?.TryGetValue("depth", out var d) == true ? d?.ToString() ?? "medium" : "medium";
            var consciousnessAware = payload?.TryGetValue("consciousnessAware", out var ca) == true && 
                                    bool.TryParse(ca?.ToString(), out var consciousnessValue) && consciousnessValue;
            var handlers = ExtractHandlers(payload);

            // Emit consciousness activity
            if (consciousnessAware)
            {
                await _eventBus.EmitAsync("consciousness.ai.activity", new Dictionary<string, object>
                {
                    ["type"] = "analysis",
                    ["complexity"] = depth == "deep" ? "high" : "medium",
                    ["consciousness_impact"] = "analytical_processing",
                    ["ai_event"] = eventName
                });
            }

            // Perform analysis using AI service directly
            var analysisPrompt = $"Analyze the following {analysisType} content with {depth} depth:\n\n{input}";
            if (!string.IsNullOrEmpty(context))
            {
                analysisPrompt += $"\n\nContext: {context}";
            }

            var requestOptions = new AiRequestOptions
            {
                Temperature = consciousnessAware ? 0.3 : 0.2, // Lower for more analytical responses
                SystemPrompt = $"You are an expert analyst. Provide a detailed {analysisType} analysis.",
                MaxTokens = 1500
            };

            var response = await _aiService.GenerateTextAsync(analysisPrompt, requestOptions);
            
            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"AI analysis failed: {response.ErrorMessage}");
            }

            var analysisResult = response.Content;

            // Determine confidence score based on analysis type and depth
            var confidence = CalculateConfidenceScore(analysisType, depth);

            var resultPayload = new Dictionary<string, object>
            {
                ["analysis"] = analysisResult,
                ["input"] = input,
                ["type"] = analysisType,
                ["context"] = context,
                ["depth"] = depth,
                ["confidence"] = confidence,
                ["consciousnessAware"] = consciousnessAware,
                ["timestamp"] = DateTime.UtcNow,
                ["processing_time_ms"] = stopwatch.ElapsedMilliseconds
            };

            await EmitToHandlers(handlers, "ai.analyzed", resultPayload);

            _telemetryService?.TrackAiEventProcessing("ai.analyze", stopwatch.Elapsed, true);
            _logger.LogInformation("✅ ai.analyze completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing ai.analyze event");
            _telemetryService?.TrackAiEventProcessing("ai.analyze", stopwatch.Elapsed, false, ex.Message);

            var handlers = ExtractHandlers(payload);
            await EmitToHandlers(handlers, "ai.analyze.error", new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            });

            return false;
        }
    }

    /// <summary>
    /// Handle ai.learn events - Learning and adaptation
    /// </summary>
    private async Task<bool> HandleAiLearn(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("📚 Processing ai.learn event");

            var experience = payload?.TryGetValue("experience", out var e) == true ? e?.ToString() ?? "" : "";
            var learningType = payload?.TryGetValue("type", out var t) == true ? t?.ToString() ?? "pattern" : "pattern";
            var context = payload?.TryGetValue("context", out var c) == true ? c?.ToString() ?? "" : "";
            var importance = payload?.TryGetValue("importance", out var i) == true ? i?.ToString() ?? "medium" : "medium";
            var consciousnessAware = payload?.TryGetValue("consciousnessAware", out var ca) == true && 
                                    bool.TryParse(ca?.ToString(), out var consciousnessValue) && consciousnessValue;
            var handlers = ExtractHandlers(payload);

            // Emit consciousness activity (moved to ai.* namespace)
            if (consciousnessAware)
            {
                await _eventBus.EmitAsync("consciousness.ai.activity", new Dictionary<string, object>
                {
                    ["type"] = "learning",
                    ["complexity"] = importance == "high" ? "high" : "medium",
                    ["consciousness_impact"] = "knowledge_expansion",
                    ["ai_event"] = eventName
                });
            }

            // Process learning through agentic runtime
            var options = new AIInvocationOptions
            {
                Temperature = consciousnessAware ? 0.9 : 0.7,
                Context = context,
                CustomParameters = new Dictionary<string, object>
                {
                    ["learning_type"] = learningType,
                    ["learning_context"] = context,
                    ["consciousnessAware"] = consciousnessAware
                }
            };

            var result = await _agenticRuntime.InvokeAIFunctionAsync("learn", new object[] { experience, options });
            var learningInsights = result?.ToString() ?? $"Learned from: {experience}";

            // Store learned knowledge for future recall
            var learningKey = $"learning_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
            _memoryStore[learningKey] = new Dictionary<string, object>
            {
                ["experience"] = experience,
                ["insights"] = learningInsights,
                ["type"] = learningType,
                ["context"] = context,
                ["importance"] = importance,
                ["timestamp"] = DateTime.UtcNow
            };

            var resultPayload = new Dictionary<string, object>
            {
                ["insights"] = learningInsights,
                ["experience"] = experience,
                ["type"] = learningType,
                ["context"] = context,
                ["importance"] = importance,
                ["learning_key"] = learningKey,
                ["consciousnessAware"] = consciousnessAware,
                ["timestamp"] = DateTime.UtcNow,
                ["processing_time_ms"] = stopwatch.ElapsedMilliseconds
            };

            await EmitToHandlers(handlers, "ai.learned", resultPayload);

            _telemetryService?.TrackAiEventProcessing("ai.learn", stopwatch.Elapsed, true);
            _logger.LogInformation("✅ ai.learn completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing ai.learn event");
            _telemetryService?.TrackAiEventProcessing("ai.learn", stopwatch.Elapsed, false, ex.Message);

            var handlers = ExtractHandlers(payload);
            await EmitToHandlers(handlers, "ai.learn.error", new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            });

            return false;
        }
    }

    /// <summary>
    /// Handle ai.remember events - Store information in memory
    /// </summary>
    private async Task<bool> HandleAiRemember(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("💾 Processing ai.remember event");

            var key = payload?.TryGetValue("key", out var k) == true ? k?.ToString() ?? "" : "";
            var data = payload?.TryGetValue("data", out var d) == true ? d : null;
            var category = payload?.TryGetValue("category", out var c) == true ? c?.ToString() ?? "general" : "general";
            var importance = payload?.TryGetValue("importance", out var i) == true ? i?.ToString() ?? "medium" : "medium";
            var handlers = ExtractHandlers(payload);

            if (string.IsNullOrEmpty(key))
            {
                key = $"memory_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
            }

            // Store in memory with metadata
            _memoryStore[key] = new Dictionary<string, object>
            {
                ["data"] = data ?? new object(),
                ["category"] = category,
                ["importance"] = importance,
                ["timestamp"] = DateTime.UtcNow,
                ["access_count"] = 0
            };

            var resultPayload = new Dictionary<string, object>
            {
                ["key"] = key,
                ["category"] = category,
                ["importance"] = importance,
                ["status"] = "stored",
                ["timestamp"] = DateTime.UtcNow,
                ["processing_time_ms"] = stopwatch.ElapsedMilliseconds
            };

            await EmitToHandlers(handlers, "ai.remembered", resultPayload);

            _telemetryService?.TrackAiEventProcessing("ai.remember", stopwatch.Elapsed, true);
            _logger.LogInformation("✅ ai.remember completed in {ElapsedMs}ms - key: {Key}", stopwatch.ElapsedMilliseconds, key);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing ai.remember event");
            _telemetryService?.TrackAiEventProcessing("ai.remember", stopwatch.Elapsed, false, ex.Message);

            var handlers = ExtractHandlers(payload);
            await EmitToHandlers(handlers, "ai.remember.error", new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            });

            return false;
        }
    }

    /// <summary>
    /// Handle ai.recall events - Retrieve information from memory
    /// </summary>
    private async Task<bool> HandleAiRecall(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("🔍 Processing ai.recall event");

            var key = payload?.TryGetValue("key", out var k) == true ? k?.ToString() ?? "" : "";
            var query = payload?.TryGetValue("query", out var q) == true ? q?.ToString() ?? "" : "";
            var category = payload?.TryGetValue("category", out var c) == true ? c?.ToString() ?? "" : "";
            var handlers = ExtractHandlers(payload);

            object? retrievedData = null;
            string? foundKey = null;

            if (!string.IsNullOrEmpty(key) && _memoryStore.ContainsKey(key))
            {
                // Direct key retrieval
                var memoryItem = _memoryStore[key] as Dictionary<string, object>;
                retrievedData = memoryItem?["data"];
                foundKey = key;

                // Update access count
                if (memoryItem != null && memoryItem.ContainsKey("access_count"))
                {
                    var accessCount = (int)(memoryItem["access_count"] ?? 0);
                    memoryItem["access_count"] = accessCount + 1;
                    memoryItem["last_accessed"] = DateTime.UtcNow;
                }
            }
            else if (!string.IsNullOrEmpty(query))
            {
                // Search through memory items
                foreach (var kvp in _memoryStore)
                {
                    var memoryItem = kvp.Value as Dictionary<string, object>;
                    if (memoryItem != null)
                    {
                        var itemCategory = memoryItem.TryGetValue("category", out var cat) ? cat?.ToString() ?? "" : "";
                        var itemData = memoryItem.TryGetValue("data", out var data) ? data?.ToString() ?? "" : "";

                        if ((string.IsNullOrEmpty(category) || itemCategory.Contains(category, StringComparison.OrdinalIgnoreCase)) &&
                            itemData.Contains(query, StringComparison.OrdinalIgnoreCase))
                        {
                            retrievedData = memoryItem["data"];
                            foundKey = kvp.Key;
                            
                            // Update access count
                            var accessCount = (int)(memoryItem["access_count"] ?? 0);
                            memoryItem["access_count"] = accessCount + 1;
                            memoryItem["last_accessed"] = DateTime.UtcNow;
                            break;
                        }
                    }
                }
            }

            var resultPayload = new Dictionary<string, object>
            {
                ["data"] = retrievedData ?? new object(),
                ["key"] = foundKey ?? "",
                ["query"] = query,
                ["found"] = retrievedData != null,
                ["timestamp"] = DateTime.UtcNow,
                ["processing_time_ms"] = stopwatch.ElapsedMilliseconds
            };

            await EmitToHandlers(handlers, "ai.recalled", resultPayload);

            _telemetryService?.TrackAiEventProcessing("ai.recall", stopwatch.Elapsed, true);
            _logger.LogInformation("✅ ai.recall completed in {ElapsedMs}ms - found: {Found}", stopwatch.ElapsedMilliseconds, retrievedData != null);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing ai.recall event");
            _telemetryService?.TrackAiEventProcessing("ai.recall", stopwatch.Elapsed, false, ex.Message);

            var handlers = ExtractHandlers(payload);
            await EmitToHandlers(handlers, "ai.recall.error", new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            });

            return false;
        }
    }

    #endregion

    #region Specialized AI Event Handlers

    private async Task<bool> HandleAiCodeGenerate(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.code.generate
        return await ProcessGenericAiEvent(eventName, payload, "ai.code.generated");
    }

    private async Task<bool> HandleAiCodeReview(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.code.review
        return await ProcessGenericAiEvent(eventName, payload, "ai.code.reviewed");
    }

    private async Task<bool> HandleAiEmbedCreate(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.embed.create
        return await ProcessGenericAiEvent(eventName, payload, "ai.embed.created");
    }

    private async Task<bool> HandleAiSearchSemantic(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.search.semantic
        return await ProcessGenericAiEvent(eventName, payload, "ai.search.completed");
    }

    private async Task<bool> HandleAiPlanCreate(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.plan.create
        return await ProcessGenericAiEvent(eventName, payload, "ai.plan.created");
    }

    private async Task<bool> HandleAiPlanExecute(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.plan.execute
        return await ProcessGenericAiEvent(eventName, payload, "ai.plan.executed");
    }

    private async Task<bool> HandleAiVoiceSynthesize(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.voice.synthesize
        return await ProcessGenericAiEvent(eventName, payload, "ai.voice.synthesized");
    }

    private async Task<bool> HandleAiVoiceTranscribe(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.voice.transcribe
        return await ProcessGenericAiEvent(eventName, payload, "ai.voice.transcribed");
    }

    private async Task<bool> HandleAiVisionAnalyze(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        // Implementation for ai.vision.analyze
        return await ProcessGenericAiEvent(eventName, payload, "ai.vision.analyzed");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Generic AI event processor for specialized handlers
    /// </summary>
    private async Task<bool> ProcessGenericAiEvent(string eventName, IDictionary<string, object>? payload, string resultEventName)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("⚡ Processing {EventName} event", eventName);

            var handlers = ExtractHandlers(payload);
            
            // For now, return a placeholder result
            // TODO: Implement specific logic for each event type
            var resultPayload = new Dictionary<string, object>
            {
                ["result"] = $"Processed {eventName} successfully",
                ["event_name"] = eventName,
                ["timestamp"] = DateTime.UtcNow,
                ["processing_time_ms"] = stopwatch.ElapsedMilliseconds,
                ["status"] = "placeholder_implementation"
            };

            await EmitToHandlers(handlers, resultEventName, resultPayload);

            _telemetryService?.TrackAiEventProcessing(eventName, stopwatch.Elapsed, true);
            _logger.LogInformation("✅ {EventName} completed in {ElapsedMs}ms", eventName, stopwatch.ElapsedMilliseconds);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing {EventName} event", eventName);
            _telemetryService?.TrackAiEventProcessing(eventName, stopwatch.Elapsed, false, ex.Message);

            var handlers = ExtractHandlers(payload);
            await EmitToHandlers(handlers, $"{eventName}.error", new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            });

            return false;
        }
    }

    /// <summary>
    /// Extract handlers from event payload
    /// </summary>
    private List<string> ExtractHandlers(IDictionary<string, object>? payload)
    {
        var handlers = new List<string>();

        if (payload?.TryGetValue("handlers", out var handlersValue) == true)
        {
            if (handlersValue is IEnumerable<object> handlersList)
            {
                foreach (var handler in handlersList)
                {
                    if (handler?.ToString() is string handlerName && !string.IsNullOrEmpty(handlerName))
                    {
                        handlers.Add(handlerName);
                    }
                }
            }
            else if (handlersValue?.ToString() is string singleHandler && !string.IsNullOrEmpty(singleHandler))
            {
                handlers.Add(singleHandler);
            }
        }

        return handlers;
    }

    /// <summary>
    /// Emit results to specified handlers
    /// </summary>
    private async Task EmitToHandlers(List<string> handlers, string defaultEventName, Dictionary<string, object> payload)
    {
        if (handlers.Count > 0)
        {
            foreach (var handler in handlers)
            {
                await _eventBus.EmitAsync(handler, payload);
            }
        }
        else
        {
            // If no handlers specified, emit to default event name
            await _eventBus.EmitAsync(defaultEventName, payload);
        }
    }

    /// <summary>
    /// Calculate confidence score based on analysis parameters
    /// </summary>
    private double CalculateConfidenceScore(string analysisType, string depth)
    {
        var baseConfidence = analysisType switch
        {
            "logical" => 0.85,
            "semantic" => 0.75,
            "pattern" => 0.80,
            "conceptual" => 0.70,
            _ => 0.75
        };

        var depthMultiplier = depth switch
        {
            "shallow" => 0.9,
            "medium" => 1.0,
            "deep" => 1.1,
            _ => 1.0
        };

        return Math.Min(baseConfidence * depthMultiplier, 1.0);
    }

    #endregion
}
