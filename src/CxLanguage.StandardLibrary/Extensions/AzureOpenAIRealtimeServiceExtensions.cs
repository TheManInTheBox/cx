using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CxLanguage.StandardLibrary.AI.Realtime;
using CxLanguage.StandardLibrary.AI.AudioStreaming;
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
            
            // Register realtime audio streaming service for NAudio integration
            services.AddSingleton<RealtimeAudioStreamingService>();
            
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
        private readonly HashSet<string> _waitingAgents;
        
        public RealtimeEventHandler(
            RealtimeService realtimeService,
            ICxEventBus eventBus,
            ILogger<RealtimeEventHandler> logger)
        {
            _realtimeService = realtimeService;
            _eventBus = eventBus;
            _logger = logger;
            _waitingAgents = new HashSet<string>();
            
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
            
            // Handle direct audio streaming to NAudio (automatic playback)
            _eventBus.Subscribe("audio.stream.direct", async (cxEvent) =>
            {
                _logger.LogInformation("🎧 CX EVENT RECEIVED: audio.stream.direct - checking wait status");
                
                // Check if any agent is currently waiting before starting audio stream
                var isWaiting = _waitingAgents.Count > 0;
                if (isWaiting)
                {
                    _logger.LogInformation("⏸️ AUDIO BLOCKED: Agent is currently waiting, audio stream postponed");
                    
                    // Emit blocked event
                    _eventBus.Emit("audio.stream.blocked", new
                    {
                        reason = "agent_waiting",
                        waitingAgents = _waitingAgents.Count,
                        timestamp = DateTimeOffset.UtcNow
                    });
                    return;
                }
                
                _logger.LogInformation("🎧 AUDIO ALLOWED: No agents waiting, proceeding with audio stream");
                
                byte[]? audioData = null;
                
                // Extract audio data from payload
                if (cxEvent.payload is Dictionary<string, object> dict)
                {
                    if (dict.TryGetValue("audioData", out var audioValue))
                    {
                        audioData = audioValue as byte[];
                        _logger.LogInformation("🎵 Audio data extracted: {Size} bytes", audioData?.Length ?? 0);
                    }
                }
                
                if (audioData != null && audioData.Length > 0)
                {
                    try
                    {
                        // Stream audio directly to NAudio for immediate playback
                        await StreamToNAudioAsync(audioData);
                        
                        // Emit success events
                        _eventBus.Emit("naudio.playback.started", new
                        {
                            audioSize = audioData.Length,
                            format = "24kHz_16bit_mono_PCM",
                            timestamp = DateTimeOffset.UtcNow
                        });
                        
                        // Calculate approximate duration (24kHz 16-bit mono = 48KB/sec)
                        var durationMs = (audioData.Length / 48.0) * 1000.0;
                        
                        _eventBus.Emit("audio.streaming.complete", new
                        {
                            duration = durationMs,
                            status = "success",
                            bytesPlayed = audioData.Length,
                            timestamp = DateTimeOffset.UtcNow
                        });
                        
                        _logger.LogInformation("✅ NAudio streaming completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to stream audio to NAudio");
                        
                        _eventBus.Emit("audio.streaming.error", new
                        {
                            error = ex.Message,
                            timestamp = DateTimeOffset.UtcNow
                        });
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ audio.stream.direct event received but no valid audio data found");
                }
            });

            // Handle agent waiting status tracking for audio stream control
            _eventBus.Subscribe("agent.waiting.started", (cxEvent) =>
            {
                var agentId = "default_agent"; // Default agent ID
                
                // Try to extract agent identifier from payload
                if (cxEvent.payload is Dictionary<string, object> dict)
                {
                    if (dict.TryGetValue("agentId", out var id))
                    {
                        agentId = id?.ToString() ?? "default_agent";
                    }
                    else if (dict.TryGetValue("reason", out var reason))
                    {
                        agentId = reason?.ToString() ?? "default_agent";
                    }
                }
                
                _waitingAgents.Add(agentId);
                _logger.LogInformation("🔄 Agent waiting started: {AgentId} (Total waiting: {Count})", agentId, _waitingAgents.Count);
            });

            _eventBus.Subscribe("agent.waiting.completed", (cxEvent) =>
            {
                var agentId = "default_agent"; // Default agent ID
                
                // Try to extract agent identifier from payload
                if (cxEvent.payload is Dictionary<string, object> dict)
                {
                    if (dict.TryGetValue("agentId", out var id))
                    {
                        agentId = id?.ToString() ?? "default_agent";
                    }
                    else if (dict.TryGetValue("Reason", out var reason))
                    {
                        agentId = reason?.ToString() ?? "default_agent";
                    }
                }
                
                _waitingAgents.Remove(agentId);
                _logger.LogInformation("✅ Agent waiting completed: {AgentId} (Total waiting: {Count})", agentId, _waitingAgents.Count);
            });

            _eventBus.Subscribe("agent.waiting.failed", (cxEvent) =>
            {
                var agentId = "default_agent"; // Default agent ID
                
                // Try to extract agent identifier from payload
                if (cxEvent.payload is Dictionary<string, object> dict)
                {
                    if (dict.TryGetValue("agentId", out var id))
                    {
                        agentId = id?.ToString() ?? "default_agent";
                    }
                    else if (dict.TryGetValue("Reason", out var reason))
                    {
                        agentId = reason?.ToString() ?? "default_agent";
                    }
                }
                
                _waitingAgents.Remove(agentId);
                _logger.LogInformation("❌ Agent waiting failed: {AgentId} (Total waiting: {Count})", agentId, _waitingAgents.Count);
            });
        }
        
        // NAudio integration for direct audio streaming
        private async Task StreamToNAudioAsync(byte[] audioData)
        {
            _logger.LogInformation("🔊 Starting NAudio streaming for {Size} bytes", audioData.Length);
            
            try
            {
                // Create NAudio streaming service if not already exists
                using var memoryStream = new MemoryStream(audioData);
                using var rawSourceWaveStream = new NAudio.Wave.RawSourceWaveStream(memoryStream, new NAudio.Wave.WaveFormat(24000, 16, 1));
                
                // Create wave output for immediate playback
                using var waveOut = new NAudio.Wave.WaveOutEvent();
                waveOut.Init(rawSourceWaveStream);
                
                // Start playback
                waveOut.Play();
                _logger.LogInformation("🎵 NAudio playback started");
                
                // Wait for playback to complete
                while (waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                {
                    await Task.Delay(100);
                }
                
                _logger.LogInformation("✅ NAudio playback completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ NAudio streaming failed");
                throw;
            }
        }
    }
}
