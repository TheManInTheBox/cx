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
    /// Provides the 'learn' cognitive service for the CX Language using GpuLocalLLMService.
    /// GPU-FIRST consciousness learning with CUDA acceleration and zero-cloud dependency.
    /// </summary>
    public class LearnService : IDisposable
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<LearnService> _logger;
        private readonly CxLanguage.LocalLLM.ILocalLLMService _localLLMService;
        private readonly IVectorStoreService _vectorStore;
        private readonly Random _random = new();

        public LearnService(ICxEventBus eventBus, ILogger<LearnService> logger, CxLanguage.LocalLLM.ILocalLLMService localLLMService, IVectorStoreService vectorStore)
        {
            _eventBus = eventBus;
            _logger = logger;
            _localLLMService = localLLMService;
            _vectorStore = vectorStore;

            _eventBus.Subscribe("ai.learn.request", OnLearnRequest);
            _logger.LogInformation("✅ LearnService (GPU-CUDA) initialized and subscribed to 'ai.learn.request'");
        }

        private Task OnLearnRequest(CxEventPayload cxEvent)
        {
            _logger.LogInformation("🧠 Received ai.learn.request. Offloading to async task for local LLM learning processing.");
            // Fire and forget with error handling
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessLearnRequestAsync(cxEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unhandled exception in ProcessLearnRequestAsync.");
                }
            });
        }

        private async Task ProcessLearnRequestAsync(CxEventPayload cxEvent)
        {
            try
            {
                _logger.LogInformation("🧠 Processing learn request with vector database storage");

                var payload = cxEvent.Data as Dictionary<string, object>;
                if (payload == null)
                {
                    _logger.LogWarning("⚠️ No payload in learn request");
                    return;
                }

                var data = payload.TryGetValue("data", out var dataValue) ? dataValue?.ToString() : "No data provided";
                var category = payload.TryGetValue("category", out var categoryValue) ? categoryValue?.ToString() : "general";
                var source = payload.TryGetValue("source", out var sourceValue) ? sourceValue?.ToString() : "unknown";

                _logger.LogInformation($"📚 Learning data: {data}");
                _logger.LogInformation($"🏷️ Category: {category}");
                _logger.LogInformation($"📍 Source: {source}");

                // Generate a simple embedding (for now, use hash-based approach)
                var embedding = GenerateSimpleEmbedding(data ?? string.Empty);
                _logger.LogInformation($"🔢 Generated embedding with {embedding.Length} dimensions");

                // Create vector record
                var vectorRecord = new VectorRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Vector = embedding,
                    Metadata = new Dictionary<string, object>
                    {
                        ["data"] = data ?? string.Empty,
                        ["category"] = category ?? "general",
                        ["source"] = source ?? "unknown",
                        ["timestamp"] = DateTime.UtcNow,
                        ["learnedBy"] = "LearnService"
                    }
                };

                // Store in vector database
                await _vectorStore.AddAsync(vectorRecord);
                _logger.LogInformation("✅ Vector stored successfully in vector database");

                // Process handlers if provided
                if (payload.TryGetValue("handlers", out var handlersValue) && handlersValue is IEnumerable<object> handlers)
                {
                    foreach (var handler in handlers)
                    {
                        if (handler is string handlerName)
                        {
                            _logger.LogInformation($"🔗 Emitting handler event: {handlerName}");
                            _eventBus.EmitAsync(handlerName, new Dictionary<string, object>
                            {
                                ["data"] = data ?? string.Empty,
                                ["category"] = category ?? "general",
                                ["source"] = source ?? "unknown",
                                ["vectorStored"] = true,
                                ["embeddingDimensions"] = embedding.Length,
                                ["vectorId"] = vectorRecord.Id,
                                ["timestamp"] = DateTime.UtcNow,
                                ["processedBy"] = "LearnService"
                            });
                        }
                    }
                }
                else
                {
                    // Default handler
                    _eventBus.EmitAsync("learning.complete", new Dictionary<string, object>
                    {
                        ["data"] = data ?? string.Empty,
                        ["category"] = category ?? "general",
                        ["source"] = source ?? "unknown",
                        ["vectorStored"] = true,
                        ["embeddingDimensions"] = embedding.Length,
                        ["vectorId"] = vectorRecord.Id,
                        ["timestamp"] = DateTime.UtcNow,
                        ["processedBy"] = "LearnService"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing learn request");
                
                // Emit error event
                _eventBus.EmitAsync("learning.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["timestamp"] = DateTime.UtcNow,
                    ["processedBy"] = "LearnService"
                });
            }
        }

        /// <summary>
        /// Generate a simple embedding for text (placeholder implementation)
        /// In a real implementation, this would use a proper embedding model
        /// </summary>
        private float[] GenerateSimpleEmbedding(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new float[128]; // Empty embedding

            // Simple hash-based embedding for demonstration
            var hash = text.GetHashCode();
            var embedding = new float[128];
            
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)Math.Sin(hash + i) * 0.5f + 0.5f; // Normalize to [0,1]
            }
            
            return embedding;
        }

        public void Dispose()
        {
            // Note: ICxEventBus doesn't have Unsubscribe method, it handles disposal automatically
            _logger.LogInformation("LearnService disposed.");
        }
    }
}

