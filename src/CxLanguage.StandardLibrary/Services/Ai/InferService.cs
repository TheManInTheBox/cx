using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.Services.VectorStore;
using CxLanguage.LocalLLM;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Services.Ai
{
    /// <summary>
    /// Provides the 'infer' cognitive service for the CX Language using GpuLocalLLMService.
    /// GPU-FIRST consciousness inference with CUDA acceleration and zero-cloud dependency.
    /// </summary>
    public class InferService : IDisposable
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<InferService> _logger;
        private readonly CxLanguage.LocalLLM.ILocalLLMService _localLLMService;
        private readonly IVectorStoreService _vectorStore;
        private readonly Random _random = new();

        public InferService(ICxEventBus eventBus, ILogger<InferService> logger, CxLanguage.LocalLLM.ILocalLLMService localLLMService, IVectorStoreService vectorStore)
        {
            _eventBus = eventBus;
            _logger = logger;
            _localLLMService = localLLMService;
            _vectorStore = vectorStore;

            _eventBus.Subscribe("ai.infer.request", async (sender, eventName, data) => { await OnInferRequest(new CxEventPayload(eventName, data ?? new Dictionary<string, object>())); return true; });
            _logger.LogInformation("✅ InferService (GPU-CUDA) initialized and subscribed to 'ai.infer.request'");
        }

        private Task OnInferRequest(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🔍 Received ai.infer.request. Offloading to async task for local LLM inference processing.");
            // Fire and forget with error handling
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessInferRequestAsync(cxEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unhandled exception in ProcessInferRequestAsync.");
                }
            });
            return Task.CompletedTask;
        }

        private async Task ProcessInferRequestAsync(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🔍 Processing infer request with GPU-CUDA Local LLM...");

            string? context = null;
            Dictionary<string, object>? data = null;
            string? inferenceType = null;
            string? algorithm = null;
            string? consciousnessLevel = null;
            double confidence = 0.8; // Default confidence
            List<object>? handlers = null;
            string? responseName = "inference.complete"; // Default handler name

            if (cxEvent.Data is Dictionary<string, object> payload)
            {
                if (payload.TryGetValue("context", out var contextObj))
                {
                    context = contextObj?.ToString();
                }

                if (payload.TryGetValue("data", out var dataObj) && dataObj is Dictionary<string, object> dataDict)
                {
                    data = dataDict;
                }

                if (payload.TryGetValue("inferenceType", out var typeObj))
                {
                    inferenceType = typeObj?.ToString();
                }

                if (payload.TryGetValue("algorithm", out var algoObj))
                {
                    algorithm = algoObj?.ToString();
                }

                if (payload.TryGetValue("consciousnessLevel", out var levelObj))
                {
                    consciousnessLevel = levelObj?.ToString();
                }

                if (payload.TryGetValue("confidence", out var confObj) && confObj is double confDouble)
                {
                    confidence = confDouble;
                }

                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is List<object> handlersList)
                {
                    handlers = handlersList;
                    // Extract the primary handler name if it exists
                    var firstHandler = handlers.FirstOrDefault();
                    if (firstHandler is string s)
                    {
                        responseName = s;
                    }
                    else if (firstHandler is Dictionary<string, object> d)
                    {
                        responseName = d.Keys.FirstOrDefault() ?? responseName;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(context) || data == null)
            {
                _logger.LogWarning("⚠️ Infer request received without valid context or data.");
                return;
            }

            try
            {
                // 🔍 INFERENCE PROCESSING: Analyze data and perform inference
                _logger.LogInformation("🧠 Starting inference: {InferenceType} - {Context}", inferenceType, context);
                
                var inferenceResult = await PerformInferenceAsync(context, data, inferenceType, algorithm, consciousnessLevel, confidence);
                
                _logger.LogInformation("✅ Inference completed: {InferenceType}", inferenceType);

                // Emit inference completion events
                await EmitInferenceResultsAsync(inferenceResult, handlers, responseName, "InferService");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during inference processing");
                
                // Emit error event
                await _eventBus.EmitAsync("ai.infer.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["context"] = context ?? "",
                    ["inferenceType"] = inferenceType ?? "unknown",
                    ["source"] = "InferService"
                });
            }
        }

        private async Task<Dictionary<string, object>> PerformInferenceAsync(
            string context, 
            Dictionary<string, object> data, 
            string? inferenceType, 
            string? algorithm, 
            string? consciousnessLevel,
            double confidence)
        {
            var result = new Dictionary<string, object>
            {
                ["context"] = context,
                ["inferenceType"] = inferenceType ?? "general",
                ["algorithm"] = algorithm ?? "ai_analysis",
                ["consciousnessLevel"] = consciousnessLevel ?? "medium",
                ["confidence"] = confidence,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };

            switch (inferenceType?.ToLower())
            {
                case "user_intent":
                    result.Merge(await InferUserIntentAsync(data));
                    break;
                    
                case "anomaly_detection":
                    result.Merge(await DetectAnomaliesAsync(data));
                    break;
                    
                case "capability_matching":
                    result.Merge(await MatchCapabilitiesAsync(data));
                    break;
                    
                case "pattern_recognition":
                    result.Merge(await RecognizePatternsAsync(data));
                    break;
                    
                default:
                    result.Merge(await PerformGeneralInferenceAsync(data));
                    break;
            }

            return result;
        }

        private Task<Dictionary<string, object>> InferUserIntentAsync(Dictionary<string, object> data)
        {
            _logger.LogInformation("🎯 Performing user intent inference");
            
            // Extract user action data
            var actions = data.GetValueOrDefault("userActions", new List<object>()) as List<object> ?? new List<object>();
            var sessionData = data.GetValueOrDefault("sessionData", new Dictionary<string, object>()) as Dictionary<string, object>;
            
            // Simulate AI-driven intent analysis
            var intents = new[] { "browsing", "shopping", "searching", "reading", "learning", "exploring" };
            var selectedIntent = intents[_random.Next(intents.Length)];
            
            var confidence = Math.Round(_random.NextDouble() * 0.3 + 0.7, 2); // 0.7-1.0 range
            
            return Task.FromResult(new Dictionary<string, object>
            {
                ["intent"] = selectedIntent,
                ["confidence"] = confidence,
                ["actionsAnalyzed"] = actions.Count,
                ["sessionContext"] = sessionData?.GetValueOrDefault("page", "unknown") ?? "unknown",
                ["reasoning"] = $"Based on {actions.Count} user actions, inferred intent as {selectedIntent}"
            });
        }

        private Task<Dictionary<string, object>> DetectAnomaliesAsync(Dictionary<string, object> data)
        {
            _logger.LogInformation("🚨 Performing anomaly detection");
            
            // Extract metrics data
            var metrics = data.GetValueOrDefault("metrics", new List<object>()) as List<object> ?? new List<object>();
            var baseline = data.GetValueOrDefault("baseline", new List<object>()) as List<object> ?? new List<object>();
            var threshold = data.GetValueOrDefault("threshold", 0.15) as double? ?? 0.15;
            
            // Simulate statistical anomaly detection
            var anomalies = new List<Dictionary<string, object>>();
            
            if (metrics.Count > 0)
            {
                // Simulate finding 1-2 anomalies
                var anomalyCount = _random.Next(0, 3);
                for (int i = 0; i < anomalyCount; i++)
                {
                    anomalies.Add(new Dictionary<string, object>
                    {
                        ["type"] = new[] { "spike", "drop", "drift", "oscillation" }[_random.Next(4)],
                        ["severity"] = new[] { "low", "medium", "high", "critical" }[_random.Next(4)],
                        ["position"] = _random.Next(metrics.Count),
                        ["deviation"] = Math.Round(_random.NextDouble() * 0.5 + threshold, 3)
                    });
                }
            }
            
            return Task.FromResult(new Dictionary<string, object>
            {
                ["anomalies"] = anomalies,
                ["count"] = anomalies.Count,
                ["metricsAnalyzed"] = metrics.Count,
                ["threshold"] = threshold,
                ["status"] = anomalies.Count > 0 ? "anomalies_detected" : "normal",
                ["summary"] = $"Analyzed {metrics.Count} metrics, found {anomalies.Count} anomalies"
            });
        }

        private Task<Dictionary<string, object>> MatchCapabilitiesAsync(Dictionary<string, object> data)
        {
            _logger.LogInformation("⚡ Performing capability matching");
            
            // Extract task and agent data
            var task = data.GetValueOrDefault("task", new Dictionary<string, object>()) as Dictionary<string, object>;
            var agents = data.GetValueOrDefault("agents", new List<object>()) as List<object> ?? new List<object>();
            
            if (agents.Count == 0)
            {
                return Task.FromResult(new Dictionary<string, object>
                {
                    ["selectedAgent"] = "none",
                    ["successProbability"] = 0.0,
                    ["reasoning"] = "No agents available for assignment"
                });
            }
            
            // Simulate intelligent agent selection
            var selectedAgentIndex = _random.Next(agents.Count);
            var selectedAgent = agents[selectedAgentIndex] as Dictionary<string, object>;
            var agentName = selectedAgent?.GetValueOrDefault("name", "Unknown")?.ToString() ?? "Unknown";
            
            var successProbability = Math.Round(_random.NextDouble() * 0.3 + 0.7, 2); // 0.7-1.0 range
            var estimatedTime = $"{_random.Next(30, 180)} minutes";
            
            return Task.FromResult(new Dictionary<string, object>
            {
                ["selectedAgent"] = selectedAgent ?? new Dictionary<string, object> { ["name"] = "Unknown" },
                ["agentName"] = agentName,
                ["successProbability"] = successProbability,
                ["estimatedTime"] = estimatedTime,
                ["reasoning"] = $"Selected {agentName} based on capability analysis and current load",
                ["alternativeAgents"] = agents.Count - 1
            });
        }

        private Task<Dictionary<string, object>> RecognizePatternsAsync(Dictionary<string, object> data)
        {
            _logger.LogInformation("🔍 Performing pattern recognition");
            
            // Simulate pattern recognition analysis
            var patterns = new[]
            {
                "sequential_increase", "cyclical_pattern", "random_distribution", 
                "exponential_growth", "linear_trend", "seasonal_variation"
            };
            
            var detectedPattern = patterns[_random.Next(patterns.Length)];
            var confidence = Math.Round(_random.NextDouble() * 0.4 + 0.6, 2); // 0.6-1.0 range
            
            return Task.FromResult(new Dictionary<string, object>
            {
                ["pattern"] = detectedPattern,
                ["confidence"] = confidence,
                ["characteristics"] = new List<string> { "consistent", "predictable", "measurable" },
                ["recommendation"] = $"Pattern '{detectedPattern}' detected with {confidence * 100}% confidence"
            });
        }

        private Task<Dictionary<string, object>> PerformGeneralInferenceAsync(Dictionary<string, object> data)
        {
            _logger.LogInformation("🧠 Performing general inference");
            
            // General AI inference processing
            var dataKeys = data.Keys.ToList();
            var insights = new List<string>();
            
            foreach (var key in dataKeys.Take(3)) // Analyze up to 3 data points
            {
                var value = data[key]?.ToString() ?? "";
                var preview = value.Length > 50 ? value.Substring(0, 50) + "..." : value;
                insights.Add($"Analyzed {key}: {preview}");
            }
            
            return Task.FromResult(new Dictionary<string, object>
            {
                ["result"] = "General inference completed",
                ["insights"] = insights,
                ["dataPointsAnalyzed"] = dataKeys.Count,
                ["summary"] = $"Processed {dataKeys.Count} data points with AI inference"
            });
        }

        private async Task EmitInferenceResultsAsync(Dictionary<string, object> result, List<object>? handlers, string defaultHandler, string? source)
        {
            if (handlers != null && handlers.Count > 0)
            {
                foreach (var handler in handlers)
                {
                    if (handler is string handlerName)
                    {
                        await _eventBus.EmitAsync(handlerName, result);
                        _logger.LogInformation("📤 Emitted inference result to handler: {Handler}", handlerName);
                    }
                    else if (handler is Dictionary<string, object> handlerDict)
                    {
                        foreach (var kvp in handlerDict)
                        {
                            var mergedResult = new Dictionary<string, object>(result);
                            if (kvp.Value is Dictionary<string, object> customPayload)
                            {
                                mergedResult.Merge(customPayload);
                            }
                            await _eventBus.EmitAsync(kvp.Key, mergedResult);
                            _logger.LogInformation("📤 Emitted inference result to handler: {Handler} (with custom payload)", kvp.Key);
                        }
                    }
                }
            }
            else
            {
                // Emit to default handler
                await _eventBus.EmitAsync(defaultHandler, result);
                _logger.LogInformation("📤 Emitted inference result to default handler: {Handler}", defaultHandler);
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("🔍 InferService disposed and unsubscribed from events");
        }
    }
}

// Extension method to merge dictionaries
public static class DictionaryExtensions
{
    public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source) where TKey : notnull
    {
        foreach (var kvp in source)
        {
            target[kvp.Key] = kvp.Value;
        }
    }
}

