using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.AI.Realtime;
using CxLanguage.Runtime;
using System;
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
            
            // Register event handlers for CX integration
            services.AddSingleton<RealtimeEventHandler>();
            
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
        
        public RealtimeEventHandler(
            RealtimeService realtimeService,
            ICxEventBus eventBus)
        {
            _realtimeService = realtimeService;
            _eventBus = eventBus;
            
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
                    await _eventBus.EmitAsync("realtime.session.created", new { sessionId = result.SessionId });
                }
                else
                {
                    await _eventBus.EmitAsync("realtime.session.error", new { error = result.ErrorMessage });
                }
            });

            // Handle text message events
            _eventBus.Subscribe("realtime.text.send", async (payload) =>
            {
                if (payload is System.Text.Json.JsonElement json)
                {
                    if (json.TryGetProperty("text", out var textProperty))
                    {
                        var text = textProperty.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            await _realtimeService.SendTextAsync(text);
                        }
                    }
                }
            });

            // Handle audio message events  
            _eventBus.Subscribe("realtime.audio.send", async (payload) =>
            {
                if (payload is System.Text.Json.JsonElement json)
                {
                    if (json.TryGetProperty("audioData", out var audioProperty))
                    {
                        var audioBase64 = audioProperty.GetString();
                        if (!string.IsNullOrEmpty(audioBase64))
                        {
                            var audioData = Convert.FromBase64String(audioBase64);
                            await _realtimeService.SendAudioAsync(audioData);
                        }
                    }
                }
            });
            
            _eventBus.Subscribe("realtime.audio.commit", async (payload) =>
            {
                await _realtimeService.CommitAudioAsync();
            });
        }
    }
}
