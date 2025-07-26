using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CxLanguage.Runtime;
using CxLanguage.StandardLibrary.Services.VectorStore;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Services.Ai
{
    /// <summary>
    /// Provides the 'think' cognitive service for the CX Language using local LLM.
    /// </summary>
    public class ThinkService : IDisposable
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<ThinkService> _logger;
        private readonly ILocalLLMService _localLLMService;
        private readonly IVectorStoreService _vectorStore;

        public ThinkService(ICxEventBus eventBus, ILogger<ThinkService> logger, ILocalLLMService localLLMService, IVectorStoreService vectorStore)
        {
            _eventBus = eventBus;
            _logger = logger;
            _localLLMService = localLLMService;
            _vectorStore = vectorStore;

            _eventBus.Subscribe("ai.think.request", OnThinkRequest);
            _logger.LogInformation("✅ ThinkService (Local LLM) initialized and subscribed to 'ai.think.request'");
        }

        private void OnThinkRequest(CxEvent cxEvent)
        {
            _logger.LogInformation("🧠 Received ai.think.request. Offloading to async task for local LLM processing.");
            // Fire and forget with error handling
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessThinkRequestAsync(cxEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unhandled exception in ProcessThinkRequestAsync.");
                }
            });
        }

        private async Task ProcessThinkRequestAsync(CxEvent cxEvent)
        {
            _logger.LogInformation("🧠 Processing think request with Local LLM...");

            string? prompt = null;
            List<object>? handlers = null;
            string? responseName = "thinking.complete"; // Default handler name

            if (cxEvent.payload is Dictionary<string, object> payload)
            {
                if (payload.TryGetValue("prompt", out var promptObj))
                {
                    prompt = promptObj?.ToString();
                }

                if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is List<object> handlersList)
                {
                    handlers = handlersList;
                    // Extract the primary handler name if it exists, otherwise default to thinking.complete
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

            if (string.IsNullOrWhiteSpace(prompt))
            {
                _logger.LogWarning("⚠️ Think request received without a valid prompt.");
                return;
            }

            try
            {
                // 🧠 MEMORY INTEGRATION: Search for relevant memories before thinking
                _logger.LogInformation("🔍 Searching memory for relevant context: {Prompt}", prompt);
                
                var memories = await SearchMemoryAsync(prompt);
                var memoryContext = "";
                
                if (memories.Any())
                {
                    _logger.LogInformation("💭 Found {Count} relevant memories", memories.Count());
                    memoryContext = "\n\nRelevant memories:\n" + string.Join("\n", memories.Select(m => $"- {m.Content}"));
                    
                    // Emit memory retrieval event
                    await _eventBus.EmitAsync("ai.memory.retrieved", new Dictionary<string, object>
                    {
                        { "prompt", prompt },
                        { "memoryCount", memories.Count() },
                        { "memories", memories.Select(m => m.Content).ToList() }
                    });
                }
                else
                {
                    _logger.LogInformation("💭 No relevant memories found");
                }
                
                // Enhance prompt with memory context
                var enhancedPrompt = prompt + memoryContext;

                var result = await _localLLMService.GenerateAsync(enhancedPrompt);

                _logger.LogInformation("✅ Think processing complete. Result: {Result}", result);
                
                // 🧠 MEMORY STORAGE: Store this interaction as a memory
                await StoreMemoryAsync(prompt, result);

                // Analyze if the AI needs more information
                var needsMoreInfo = await AnalyzeIfNeedsMoreInfo(result, prompt);

                var resultPayload = new Dictionary<string, object>
                {
                    { "result", result },
                    { "originalPrompt", prompt }
                };

                if (needsMoreInfo.needsMore)
                {
                    // AI needs more information - emit special event
                    await _eventBus.EmitAsync("ai.think.needs_more_info", new Dictionary<string, object>
                    {
                        { "question", needsMoreInfo.question },
                        { "originalPrompt", prompt },
                        { "partialResult", result }
                    });
                    _logger.LogInformation("AI needs more information: {Question}", needsMoreInfo.question);
                }
                else
                {
                    // AI has complete response - emit normal response
                    await _eventBus.EmitAsync("ai.think.response", resultPayload);
                    _logger.LogInformation("AI provided complete response");
                }

                // Always emit the primary completion event for backward compatibility
                await _eventBus.EmitAsync(responseName, resultPayload);
                _logger.LogInformation("Emitted primary event: {EventName}", responseName);

                // Process additional handlers if they exist
                if (handlers != null)
                {
                    foreach (var handler in handlers)
                    {
                        if (handler is string handlerName)
                        {
                            // Skip the primary handler since it was already emitted
                            if (handlerName == responseName) continue;
                            
                            await _eventBus.EmitAsync(handlerName, resultPayload);
                            _logger.LogInformation("Emitted additional handler event: {HandlerName}", handlerName);
                        }
                        else if (handler is Dictionary<string, object> handlerDict)
                        {
                            var handlerEventName = handlerDict.Keys.First();
                            // Skip the primary handler
                            if (handlerEventName == responseName) continue;

                            var handlerPayload = handlerDict.Values.First() as Dictionary<string, object> ?? new Dictionary<string, object>();
                            
                            // Combine original result with handler-specific payload
                            var combinedPayload = new Dictionary<string, object>(resultPayload);
                            foreach (var kvp in handlerPayload)
                            {
                                combinedPayload[kvp.Key] = kvp.Value;
                            }
                            
                            await _eventBus.EmitAsync(handlerEventName, combinedPayload);
                            _logger.LogInformation("Emitted additional handler event: {HandlerName}", handlerEventName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during think processing with Local LLM.");
                await _eventBus.EmitAsync("ai.think.error", new { error = ex.Message, prompt });
            }
        }

        private Task<(bool needsMore, string question)> AnalyzeIfNeedsMoreInfo(string aiResponse, string originalPrompt)
        {
            // Simple heuristic analysis - in a real implementation, you might use more sophisticated NLP
            var response = aiResponse.ToLower();
            
            // Check for common phrases that indicate the AI needs more information
            var needsMorePhrases = new[]
            {
                "need more information",
                "need more details",
                "can you provide more",
                "what specifically",
                "which type",
                "more context",
                "clarify",
                "specify",
                "what kind of",
                "i need to know"
            };

            foreach (var phrase in needsMorePhrases)
            {
                if (response.Contains(phrase))
                {
                    // Extract the question from the response
                    var sentences = aiResponse.Split('.', '?', '!');
                    var questionSentence = sentences.FirstOrDefault(s => s.ToLower().Contains(phrase))?.Trim();
                    
                    return Task.FromResult((true, questionSentence ?? aiResponse));
                }
            }

            // If response contains questions marks, it might be asking for clarification
            if (response.Contains("?"))
            {
                var sentences = aiResponse.Split('.', '!');
                var questionSentence = sentences.FirstOrDefault(s => s.Contains("?"))?.Trim();
                if (!string.IsNullOrEmpty(questionSentence))
                {
                    return Task.FromResult((true, questionSentence));
                }
            }

            return Task.FromResult((false, string.Empty));
        }

        /// <summary>
        /// Searches memory for relevant context based on the prompt
        /// </summary>
        private async Task<IEnumerable<VectorRecord>> SearchMemoryAsync(string prompt)
        {
            try
            {
                // Convert prompt to simple vector (in production, this would use proper embedding service)
                var searchVector = ConvertTextToVector(prompt);
                
                // Search for top 3 most relevant memories
                var memories = await _vectorStore.SearchAsync(searchVector, topK: 3);
                
                _logger.LogInformation("🔍 Memory search for '{Prompt}' returned {Count} results", 
                    prompt.Substring(0, Math.Min(50, prompt.Length)), memories.Count());
                
                return memories ?? Enumerable.Empty<VectorRecord>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error searching memory");
                return Enumerable.Empty<VectorRecord>();
            }
        }

        /// <summary>
        /// Stores the interaction as a memory for future reference
        /// </summary>
        private async Task StoreMemoryAsync(string prompt, string response)
        {
            try
            {
                var memoryContent = $"Prompt: {prompt}\nResponse: {response}";
                var memoryVector = ConvertTextToVector(memoryContent);
                
                var memory = new VectorRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Vector = memoryVector,
                    Content = memoryContent,
                    Metadata = new Dictionary<string, object>
                    {
                        { "type", "conversation" },
                        { "prompt", prompt },
                        { "response", response },
                        { "timestamp", DateTimeOffset.UtcNow.ToString() }
                    },
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _vectorStore.AddAsync(memory);
                
                _logger.LogInformation("💾 Stored memory: {Content}", 
                    memoryContent.Substring(0, Math.Min(100, memoryContent.Length)) + "...");
                
                // Emit memory storage event
                await _eventBus.EmitAsync("ai.memory.stored", new Dictionary<string, object>
                {
                    { "memoryId", memory.Id },
                    { "content", memoryContent },
                    { "timestamp", memory.CreatedAt }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing memory");
            }
        }

        /// <summary>
        /// Converts text to a simple vector (placeholder for proper embedding service)
        /// In production, this would use Azure OpenAI embedding service
        /// </summary>
        private float[] ConvertTextToVector(string text)
        {
            // Simple hash-based vector generation for demonstration
            // In production, this would use proper embeddings from Azure OpenAI
            var hash = text.GetHashCode();
            var random = new Random(hash);
            var vector = new float[384]; // Common embedding dimension
            
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)(random.NextDouble() * 2.0 - 1.0); // Range [-1, 1]
            }
            
            // Normalize the vector
            var magnitude = (float)Math.Sqrt(vector.Sum(x => x * x));
            if (magnitude > 0)
            {
                for (int i = 0; i < vector.Length; i++)
                {
                    vector[i] /= magnitude;
                }
            }
            
            return vector;
        }

        public void Dispose()
        {
            // _eventBus.Unsubscribe("ai.think.request", OnThinkRequest);
            _logger.LogInformation("ThinkService disposed and unsubscribed.");
        }
    }
}
