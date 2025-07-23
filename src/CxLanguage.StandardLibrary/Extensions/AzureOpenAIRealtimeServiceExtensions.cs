using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CxLanguage.StandardLibrary.AI.Realtime;
using CxLanguage.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Extensions
{
    /// <summary>
    /// Service registration extensions for Azure OpenAI Realtime API
    /// </summary>
    public static class AzureOpenAIRealtimeServiceExtensions
    {
        /// <summary>
        /// Add Azure OpenAI Realtime API services to dependency injection
        /// </summary>
        public static IServiceCollection AddAzureOpenAIRealtimeServices(this IServiceCollection services)
        {
            // Register unified event bus as ICxEventBus for Azure integration
            services.AddSingleton<ICxEventBus>(_ => UnifiedEventBusRegistry.Instance);
            
            // Register Azure OpenAI Realtime service
            services.AddScoped<RealtimeService>();
            
            // Register event handlers for CX integration as singleton for immediate activation
            services.AddSingleton<RealtimeEventHandler>(provider =>
            {
                // Force instantiation and activation when requested
                var realtimeService = provider.GetRequiredService<RealtimeService>();
                var eventBus = provider.GetRequiredService<ICxEventBus>();
                var logger = provider.GetRequiredService<ILogger<RealtimeEventHandler>>();
                var handler = new RealtimeEventHandler(realtimeService, eventBus, logger);
                
                // Log activation
                try
                {
                    logger?.LogInformation("✅ Azure Realtime Event Handler activated via factory");
                }
                catch { /* Ignore logging errors */ }
                
                return handler;
            });
            
            return services;
        }
    }
    
    /// <summary>
    /// Event handler to bridge CX events with Azure OpenAI Realtime API
    /// </summary>
    public class RealtimeEventHandler
    {
        private readonly RealtimeService _realtimeService;
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<RealtimeEventHandler> _logger;
        
        public RealtimeEventHandler(
            RealtimeService realtimeService,
            ICxEventBus eventBus,
            ILogger<RealtimeEventHandler> logger)
        {
            _realtimeService = realtimeService;
            _eventBus = eventBus;
            _logger = logger;
            
            // Subscribe to CX events
            RegisterEventHandlers();
        }
        
        private void RegisterEventHandlers()
        {
            // Handle connection events
            _eventBus.Subscribe("realtime.connect", async (payload) =>
            {
                await _realtimeService.ConnectAsync();
            });
            
            _eventBus.Subscribe("realtime.disconnect", async (payload) =>
            {
                await _realtimeService.DisconnectAsync();
            });

            // Handle session control events
            _eventBus.Subscribe("realtime.session.create", async (payload) =>
            {
                var options = new RealtimeOptions
                {
                    MaxLatency = TimeSpan.FromMilliseconds(200)
                };
                
                var result = await _realtimeService.StartSessionAsync(options);
                
                if (result.IsSuccess)
                {
                    _eventBus.Emit("realtime.session.created", new { sessionId = result.SessionId });
                }
                else
                {
                    _eventBus.Emit("realtime.session.error", new { error = result.ErrorMessage });
                }
            });

            // Handle text message events
            _eventBus.Subscribe("realtime.text.send", (cxEvent) =>
            {
                string? text = null;
                
                // Handle Dictionary payload (from CX events)
                if (cxEvent.payload is Dictionary<string, object> dict)
                {
                    if (dict.TryGetValue("text", out var textValue))
                    {
                        text = textValue?.ToString();
                    }
                }
                // Handle JsonElement payload (legacy support)
                else if (cxEvent.payload is System.Text.Json.JsonElement json)
                {
                    if (json.TryGetProperty("text", out var textProperty))
                    {
                        text = textProperty.GetString();
                    }
                }
                
                if (!string.IsNullOrEmpty(text))
                {
                    _logger.LogInformation("🎯 CX EVENT RECEIVED: realtime.text.send with text: {Text}", text);
                    Task.Run(async () => await _realtimeService.SendTextAsync(text));
                }
                else
                {
                    _logger.LogWarning("❌ realtime.text.send event received but no text found in payload");
                }
            });

            // Handle audio message events  
            _eventBus.Subscribe("realtime.audio.send", (cxEvent) =>
            {
                if (cxEvent.payload is System.Text.Json.JsonElement json)
                {
                    if (json.TryGetProperty("audioData", out var audioProperty))
                    {
                        var audioBase64 = audioProperty.GetString();
                        if (!string.IsNullOrEmpty(audioBase64))
                        {
                            var audioData = Convert.FromBase64String(audioBase64);
                            Task.Run(async () => await _realtimeService.SendAudioAsync(audioData));
                        }
                    }
                }
            });
            
            _eventBus.Subscribe("realtime.audio.commit", (cxEvent) =>
            {
                Task.Run(async () => await _realtimeService.CommitAudioAsync());
            });
        }
    }
}
