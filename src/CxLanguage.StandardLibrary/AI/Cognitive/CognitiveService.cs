using CxLanguage.StandardLibrary.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using CxLanguage.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.AI.Cognitive
{
    /// <summary>
    /// Cognitive service that handles AI cognitive functions like think, learn, etc.
    /// Processes cognitive requests using Kernel Memory and stores results in vector store
    /// with support for instance-specific routing to personalized memory collections
    /// </summary>
    public class CognitiveService : AiServiceBase
    {
        private readonly IKernelMemory? _memory;
        private readonly ICxEventBus? _eventBus;

        /// <summary>
        /// Initializes a new instance of the CognitiveService
        /// </summary>
        /// <param name="serviceProvider">Service provider for dependency injection</param>
        /// <param name="logger">Logger instance</param>
        public CognitiveService(IServiceProvider serviceProvider, ILogger<CognitiveService> logger) 
            : base(serviceProvider, logger)
        {
            _logger.LogInformation("🧠 Initializing CognitiveService with instance-specific routing");
            
            // Get Kernel Memory service for vector operations
            _memory = _serviceProvider.GetService<IKernelMemory>();
            
            // Get event bus for cognitive event handling
            _eventBus = _serviceProvider.GetService<ICxEventBus>();
            
            _logger.LogInformation("🧠 CognitiveService initialized - Memory: {MemoryAvailable}, EventBus: {EventBusAvailable}", 
                _memory != null, _eventBus != null);
                
            RegisterEventHandlers();
        }

        /// <summary>
        /// Register event handlers for cognitive operations
        /// </summary>
        private void RegisterEventHandlers()
        {
            if (_eventBus == null)
            {
                _logger.LogWarning("⚠️ Event bus not available for cognitive service registration");
                return;
            }

            // Register handlers for AI cognitive requests with instance-specific routing
            _eventBus.Subscribe("ai.think.request", async (eventData) => await HandleThinkRequest(eventData));
            _eventBus.Subscribe("ai.learn.request", async (eventData) => await HandleLearnRequest(eventData));
            
            _logger.LogInformation("✅ CognitiveService event handlers registered for instance-specific cognitive operations");
        }

        /// <summary>
        /// Handle think requests using Kernel Memory with instance-specific routing
        /// </summary>
        private async Task HandleThinkRequest(CxEvent eventData)
        {
            try
            {
                _logger.LogInformation("🧠 Processing think request with instance routing");
                
                var payload = ExtractPayload(eventData);
                var source = ExtractSource(eventData) ?? "unknown";
                
                // Check for instance collection routing information
                var instanceCollection = ExtractInstanceCollection(payload);
                string prompt;
                
                if (instanceCollection != null)
                {
                    _logger.LogInformation("🧠 INSTANCE ROUTING: Using collection {Collection} for think request", instanceCollection);
                    
                    // Extract prompt from originalPayload if present, otherwise use direct prompt
                    if (payload is Dictionary<string, object> dict && dict.TryGetValue("originalPayload", out var originalPayload))
                    {
                        prompt = ExtractPrompt(originalPayload);
                    }
                    else
                    {
                        prompt = ExtractPrompt(payload);
                    }
                }
                else
                {
                    _logger.LogInformation("🧠 GLOBAL ROUTING: Using default collection for think request");
                    prompt = ExtractPrompt(payload);
                }
                
                if (_memory == null)
                {
                    _logger.LogWarning("⚠️ Kernel Memory not available for think request");
                    EmitCognitiveResponse("thinking.complete", new { error = "Memory service unavailable" }, source);
                    return;
                }

                // Use instance-specific collection if available, otherwise use default
                string response;
                if (instanceCollection != null)
                {
                    _logger.LogInformation("🧠 Querying instance collection: {Collection}", instanceCollection);
                    var memoryResponse = await _memory.AskAsync(prompt, index: instanceCollection);
                    response = memoryResponse.Result ?? "No response generated";
                }
                else
                {
                    _logger.LogInformation("🧠 Querying default collection");
                    var memoryResponse = await _memory.AskAsync(prompt);
                    response = memoryResponse.Result ?? "No response generated";
                }
                
                _logger.LogInformation("🧠 Think request completed: {Result}", response.Substring(0, Math.Min(100, response.Length)));

                // Store the thinking process in the appropriate vector memory collection
                var thinkingContent = SerializePayload(new { prompt, response });
                var thinkingPattern = $"Thinking process: {thinkingContent}, source='{source}', timestamp={DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
                var documentId = $"think_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
                
                if (instanceCollection != null)
                {
                    _logger.LogInformation("💾 Storing thinking pattern in instance collection: {Collection}", instanceCollection);
                    await _memory.ImportTextAsync(thinkingPattern, documentId: documentId, index: instanceCollection);
                }
                else
                {
                    _logger.LogInformation("💾 Storing thinking pattern in default collection");
                    await _memory.ImportTextAsync(thinkingPattern, documentId: documentId);
                }

                // Emit the cognitive response
                EmitCognitiveResponse("thinking.complete", new { 
                    prompt = prompt,
                    result = response,
                    source = source,
                    vectorStored = true,
                    documentId = documentId,
                    instanceCollection = instanceCollection
                }, source);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing think request");
                EmitCognitiveResponse("thinking.error", new { error = ex.Message }, "cognitive_service");
            }
        }

        /// <summary>
        /// Handle learn requests using Kernel Memory with instance-specific routing
        /// </summary>
        private async Task HandleLearnRequest(CxEvent eventData)
        {
            try
            {
                _logger.LogInformation("📚 Processing learn request with instance routing");
                
                var payload = ExtractPayload(eventData);
                var source = ExtractSource(eventData) ?? "unknown";
                
                // Check for instance collection routing information
                var instanceCollection = ExtractInstanceCollection(payload);
                string data;
                
                if (instanceCollection != null)
                {
                    _logger.LogInformation("📚 INSTANCE ROUTING: Using collection {Collection} for learn request", instanceCollection);
                    
                    // Extract data from originalPayload if present, otherwise use direct data
                    if (payload is Dictionary<string, object> dict && dict.TryGetValue("originalPayload", out var originalPayload))
                    {
                        data = ExtractData(originalPayload);
                    }
                    else
                    {
                        data = ExtractData(payload);
                    }
                }
                else
                {
                    _logger.LogInformation("📚 GLOBAL ROUTING: Using default collection for learn request");
                    data = ExtractData(payload);
                }
                
                if (_memory == null)
                {
                    _logger.LogWarning("⚠️ Kernel Memory not available for learn request");
                    EmitCognitiveResponse("learning.complete", new { error = "Memory service unavailable" }, source);
                    return;
                }

                // Serialize the data to JSON if it's a complex object
                var learningContent = SerializePayload(data);
                var learningData = $"Learning data: content='{learningContent}', source='{source}', timestamp={DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
                var documentId = $"learn_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
                
                if (instanceCollection != null)
                {
                    _logger.LogInformation("💾 Storing learning data in instance collection: {Collection}", instanceCollection);
                    await _memory.ImportTextAsync(learningData, documentId: documentId, index: instanceCollection);
                }
                else
                {
                    _logger.LogInformation("💾 Storing learning data in default collection");
                    await _memory.ImportTextAsync(learningData, documentId: documentId);
                }
                
                _logger.LogInformation("📚 Learning data stored: {DocumentId}", documentId);

                // Emit the cognitive response
                EmitCognitiveResponse("learning.complete", new { 
                    data = data,
                    source = source,
                    vectorStored = true,
                    documentId = documentId,
                    instanceCollection = instanceCollection
                }, source);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing learn request");
                EmitCognitiveResponse("learning.error", new { error = ex.Message }, "cognitive_service");
            }
        }

        /// <summary>
        /// Extract instance collection name from event data for routing to instance-specific memory
        /// </summary>
        private string? ExtractInstanceCollection(object eventData)
        {
            if (eventData is Dictionary<string, object> dict)
            {
                return dict.TryGetValue("_instanceCollection", out var collection) ? collection?.ToString() : null;
            }
            
            return null;
        }

        /// <summary>
        /// Extract payload data from event data object
        /// </summary>
        private object ExtractPayload(CxEvent eventData)
        {
            return eventData.payload ?? new { };
        }

        /// <summary>
        /// Extract prompt from payload
        /// </summary>
        private string ExtractPrompt(object payload)
        {
            if (payload is Dictionary<string, object> dict)
            {
                return dict.TryGetValue("prompt", out var prompt) ? prompt?.ToString() ?? "" : "";
            }
            
            return payload?.ToString() ?? "";
        }

        /// <summary>
        /// Extract data from payload for learning operations
        /// </summary>
        private string ExtractData(object payload)
        {
            if (payload is Dictionary<string, object> dict)
            {
                return dict.TryGetValue("data", out var data) ? data?.ToString() ?? "" : "";
            }
            
            return payload?.ToString() ?? "";
        }

        /// <summary>
        /// Extract source from event data
        /// </summary>
        private string? ExtractSource(CxEvent eventData)
        {
            if (eventData.payload is Dictionary<string, object> dict)
            {
                return dict.TryGetValue("source", out var source) ? source?.ToString() : null;
            }
            
            return null;
        }

        /// <summary>
        /// Emit cognitive response event
        /// </summary>
        private void EmitCognitiveResponse(string eventName, object data, string source)
        {
            if (_eventBus != null)
            {
                _eventBus.Emit(eventName, data);
                _logger.LogInformation("✅ Emitted cognitive response: {EventName}", eventName);
            }
        }

        /// <summary>
        /// Serializes an object to a JSON string for Kernel Memory ingestion.
        /// Handles primitive types by returning their string representation.
        /// </summary>
        private string SerializeForMemory(object data)
        {
            if (data is string s)
            {
                return s;
            }

            if (data.GetType().IsPrimitive || data is decimal)
            {
                return data.ToString() ?? string.Empty;
            }

            try
            {
                return System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to serialize object for memory ingestion. Using ToString() as fallback.");
                return data.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Serializes the given payload object to a JSON string for memory ingestion.
        /// </summary>
        private string SerializePayload(object payload)
        {
            if (payload is string s)
            {
                return s;
            }
            
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to serialize payload to JSON. Falling back to ToString().");
                return payload.ToString() ?? string.Empty;
            }
        }
    }
}
