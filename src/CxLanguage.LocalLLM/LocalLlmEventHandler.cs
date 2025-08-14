using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.Runtime;

namespace CxLanguage.LocalLLM
{
    /// <summary>
    /// Integrates the GPU-accelerated Local LLM Service with the CX Event System
    /// Handles events for local LLM operations using consciousness-aware patterns
    /// </summary>
    public class LocalLlmEventHandler : IDisposable
    {
        private readonly ILogger<LocalLlmEventHandler> _logger;
        private readonly ICxEventBus _eventBus;
        private readonly GpuLocalLLMService _llmService;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the Local LLM Event Handler
        /// </summary>
        public LocalLlmEventHandler(
            ILogger<LocalLlmEventHandler> logger,
            ICxEventBus eventBus,
            GpuLocalLLMService llmService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));

            // Register event handlers
            _eventBus.Subscribe("llm.initialize", HandleInitializeEvent);
            _eventBus.Subscribe("llm.generate", HandleGenerateEvent);
            _eventBus.Subscribe("llm.dispose", HandleDisposeEvent);

            _logger.LogInformation("🔌 LocalLlmEventHandler initialized and subscribed to events");
        }

        /// <summary>
        /// Handle LLM initialization event
        /// </summary>
        private async Task<bool> HandleInitializeEvent(object? sender, string eventName, IDictionary<string, object>? data)
        {
            try
            {
                _logger.LogInformation("🚀 Initializing LocalLLM service");

                // Extract parameters from data
                if (data == null)
                {
                    data = new Dictionary<string, object>();
                }

                data.TryGetValue("useGpu", out var useGpuObj);
                data.TryGetValue("modelName", out var modelNameObj);

                bool useGpu = useGpuObj is bool b ? b : true;
                string modelName = modelNameObj?.ToString() ?? "default-model.gguf";

                // In a real implementation, this would actually load the model
                // For now, we'll just emit the initialized event
                await Task.Delay(500); // Simulate initialization time

                // Create result payload
                var resultPayload = new Dictionary<string, object>
                {
                    { "gpuAvailable", true }, // In a real implementation, this would come from _llmService
                    { "modelName", modelName },
                    { "useGpu", useGpu }
                };

                // Emit initialized event
                await _eventBus.EmitAsync("llm.initialized", resultPayload);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing LocalLLM service");
                await EmitErrorAsync("Failed to initialize LocalLLM service", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Handle LLM generation event
        /// </summary>
        private async Task<bool> HandleGenerateEvent(object? sender, string eventName, IDictionary<string, object>? data)
        {
            try
            {
                // Extract parameters from data
                if (data == null)
                {
                    data = new Dictionary<string, object>();
                }

                data.TryGetValue("prompt", out var promptObj);
                data.TryGetValue("maxTokens", out var maxTokensObj);

                string prompt = promptObj?.ToString() ?? "";
                int maxTokens = maxTokensObj is int mt ? mt : 100;

                _logger.LogInformation("🧠 Generating with prompt: {PromptStart}...", 
                    prompt.Length > 50 ? prompt.Substring(0, 50) + "..." : prompt);

                // Measure generation time
                var stopwatch = Stopwatch.StartNew();

                // Generate text using the GPU service
                string result = await _llmService.GenerateAsync(prompt);

                stopwatch.Stop();
                
                // Calculate tokens per second (in a real implementation, we would count actual tokens)
                int tokenCount = result.Split(' ').Length;
                double tokensPerSecond = tokenCount / (stopwatch.ElapsedMilliseconds / 1000.0);

                // Create result payload
                var resultPayload = new Dictionary<string, object>
                {
                    { "result", result },
                    { "prompt", prompt },
                    { "tokenCount", tokenCount },
                    { "generationTimeMs", stopwatch.ElapsedMilliseconds },
                    { "tokensPerSecond", tokensPerSecond }
                };

                // Emit generated event
                await _eventBus.EmitAsync("llm.generated", resultPayload);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during generation");
                await EmitErrorAsync("Failed to generate text", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Handle LLM dispose event
        /// </summary>
        private async Task<bool> HandleDisposeEvent(object? sender, string eventName, IDictionary<string, object>? data)
        {
            try
            {
                _logger.LogInformation("🧹 Disposing LocalLLM service");
                
                // In a real implementation, this would dispose the model and free resources
                // The actual Dispose() will happen when the DI container disposes this service
                
                // Emit disposed event
                await _eventBus.EmitAsync("llm.disposed", new Dictionary<string, object>());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing LocalLLM service");
                await EmitErrorAsync("Failed to dispose LocalLLM service", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Emit an error event
        /// </summary>
        private async Task EmitErrorAsync(string message, string details)
        {
            try
            {
                var errorPayload = new Dictionary<string, object>
                {
                    { "errorMessage", message },
                    { "errorDetails", details },
                    { "timestamp", DateTime.UtcNow }
                };

                await _eventBus.EmitAsync("llm.error", errorPayload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error emitting error event");
            }
        }

        /// <summary>
        /// Dispose of resources and unsubscribe from events
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Unsubscribe from events
                _eventBus.Unsubscribe("llm.initialize", HandleInitializeEvent);
                _eventBus.Unsubscribe("llm.generate", HandleGenerateEvent);
                _eventBus.Unsubscribe("llm.dispose", HandleDisposeEvent);

                _logger.LogInformation("🔌 LocalLlmEventHandler unsubscribed from events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing LocalLlmEventHandler");
            }

            _disposed = true;
        }
    }
}
