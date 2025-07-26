using CxLanguage.StandardLibrary.EventBridges;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CxLanguage.StandardLibrary.Services;

/// <summary>
/// Service initialization for voice processing components
/// Ensures event bridges are activated on startup
/// </summary>
public class VoiceServiceInitializer : IHostedService
{
    private readonly ILogger<VoiceServiceInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public VoiceServiceInitializer(
        ILogger<VoiceServiceInitializer> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🚀 Initializing Voice Processing Services");
            
            // Activate the voice input event bridge
            var voiceInputEventBridge = _serviceProvider.GetRequiredService<VoiceInputEventBridge>();
            _logger.LogInformation("✅ Voice Input Event Bridge activated");
            
            // Activate the voice output event bridge
            var voiceOutputEventBridge = _serviceProvider.GetRequiredService<VoiceOutputEventBridge>();
            _ = Task.Run(async () => await voiceOutputEventBridge.InitializeAsync());
            _logger.LogInformation("✅ Voice Output Event Bridge activated");
            
            // Activate the Azure Realtime event bridge
            var azureRealtimeEventBridge = _serviceProvider.GetRequiredService<AzureRealtimeEventBridge>();
            _ = Task.Run(async () => await azureRealtimeEventBridge.InitializeAsync());
            _logger.LogInformation("✅ Azure Realtime Event Bridge activated");
            
            // Activate the Local LLM event bridge for IL-generated inference
            var localLLMEventBridge = _serviceProvider.GetRequiredService<LocalLLMEventBridge>();
            _ = Task.Run(async () => await localLLMEventBridge.InitializeAsync());
            _logger.LogInformation("✅ Local LLM Event Bridge activated - IL-generated inference ready");
            
            // Activate the Await event bridge for timing operations
            var awaitEventBridge = _serviceProvider.GetRequiredService<AwaitEventBridge>();
            _ = Task.Run(async () => await awaitEventBridge.InitializeAsync());
            _logger.LogInformation("✅ Await Event Bridge activated - await timing operations ready");
            
            _logger.LogInformation("🎤 Voice Processing Services initialized successfully");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize Voice Processing Services");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔇 Voice Processing Services stopped");
        return Task.CompletedTask;
    }
}
