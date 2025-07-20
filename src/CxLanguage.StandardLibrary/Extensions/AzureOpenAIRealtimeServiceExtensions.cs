using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CxLanguage.StandardLibrary.Events;
using CxLanguage.StandardLibrary.Services;

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
            // Register event bus
            services.AddSingleton<ICxEventBus, CxEventBus>();
            
            // Register Azure OpenAI Realtime service
            services.AddSingleton<AzureOpenAIRealtimeService>();
            
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
        private readonly AzureOpenAIRealtimeService _realtimeService;
        private readonly ICxEventBus _eventBus;
        
        public RealtimeEventHandler(
            AzureOpenAIRealtimeService realtimeService,
            ICxEventBus eventBus)
        {
            _realtimeService = realtimeService;
            _eventBus = eventBus;
            
            // Subscribe to CX events
            RegisterEventHandlers();
        }
        
        private void RegisterEventHandlers()
        {
            // Handle session control events
            _eventBus.Subscribe("realtime.session.create", async (payload) =>
            {
                var config = new RealtimeSessionConfig
                {
                    Model = "gpt-4o-realtime-preview-2024-10-01",
                    Voice = "alloy",
                    Instructions = "You are Aura, an enthusiastic programming assistant inspired by Animal from the Muppets. Use BEEP-BOOP frequently and help with programming tasks energetically.",
                    InputAudioFormat = "pcm16",
                    OutputAudioFormat = "pcm16",
                    Temperature = 0.8
                };
                
                await _realtimeService.StartRealtimeSessionAsync(config);
            });
            
            _eventBus.Subscribe("realtime.session.close", async (payload) =>
            {
                await _realtimeService.StopRealtimeSessionAsync("user_requested");
            });
            
            // Handle user message events
            _eventBus.Subscribe("realtime.user.message", async (payload) =>
            {
                if (payload is System.Text.Json.JsonElement json)
                {
                    if (json.TryGetProperty("content", out var content))
                    {
                        var messageContent = content.GetString();
                        var messageType = json.TryGetProperty("type", out var type) ? type.GetString() ?? "user_message" : "user_message";
                        
                        if (!string.IsNullOrEmpty(messageContent))
                        {
                            await _realtimeService.SendUserMessageAsync(messageContent, messageType);
                        }
                    }
                }
            });
            
            // Handle system message events
            _eventBus.Subscribe("realtime.system.message", async (payload) =>
            {
                if (payload is System.Text.Json.JsonElement json)
                {
                    if (json.TryGetProperty("content", out var content))
                    {
                        var systemMessage = content.GetString();
                        if (!string.IsNullOrEmpty(systemMessage))
                        {
                            // System messages would be handled differently in real implementation
                            await Task.CompletedTask;
                        }
                    }
                }
            });
        }
    }
}
