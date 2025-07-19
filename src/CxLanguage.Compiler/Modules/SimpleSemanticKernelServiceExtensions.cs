
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using CxCoreAI = CxLanguage.Core.AI;
using CxAzureAI = CxLanguage.Azure.Services;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Configuration model for Azure OpenAI services
/// </summary>
public class AzureOpenAIConfiguration
{
    public List<AzureOpenAIService> Services { get; set; } = new();
    public string DefaultService { get; set; } = string.Empty;
    public Dictionary<string, string> ServiceSelection { get; set; } = new();
}

/// <summary>
/// Configuration model for individual Azure OpenAI service
/// </summary>
public class AzureOpenAIService
{
    public string Name { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-10-21";
    public string Region { get; set; } = string.Empty;
    public AzureOpenAIModels Models { get; set; } = new();
    public ModelApiVersions? ModelApiVersions { get; set; }
}

/// <summary>
/// Configuration model for Azure OpenAI model deployments with API version support
/// </summary>
public class AzureOpenAIModels
{
    public string? ChatCompletion { get; set; }
    public string? TextGeneration { get; set; }
    public string? TextEmbedding { get; set; }
    public string? TextToImage { get; set; }
    public string? TextToSpeech { get; set; }
    public string? AudioToText { get; set; }
    public string? ImageToText { get; set; }
}

/// <summary>
/// Configuration model for model-specific API versions
/// </summary>
public class ModelApiVersions
{
    public string ChatCompletion { get; set; } = "2024-10-21";
    public string TextGeneration { get; set; } = "2024-10-21";
    public string TextEmbedding { get; set; } = "2024-10-21";
    public string TextToImage { get; set; } = "2024-10-01";
    public string TextToSpeech { get; set; } = "2024-10-01";
    public string AudioToText { get; set; } = "2024-10-01";
    public string ImageToText { get; set; } = "2024-10-01";
}

/// <summary>
/// Simple extension methods for configuring Semantic Kernel services
/// </summary>
public static class SimpleSemanticKernelServiceExtensions
{
    /// <summary>
    /// Adds basic Semantic Kernel services to the service collection
    /// </summary>
    public static IServiceCollection AddSimpleSemanticKernelServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get Azure OpenAI configuration
        var azureOpenAIConfig = configuration.GetSection("AzureOpenAI").Get<AzureOpenAIConfiguration>();
        
        if (azureOpenAIConfig?.Services?.Any() == true)
        {
            // Helper method to get service configuration for a specific AI service type
            AzureOpenAIService GetServiceForType(string serviceType)
            {
                var selectedServiceName = azureOpenAIConfig.ServiceSelection?.GetValueOrDefault(serviceType) 
                                         ?? azureOpenAIConfig.DefaultService;
                
                return azureOpenAIConfig.Services.FirstOrDefault(s => s.Name == selectedServiceName)
                       ?? azureOpenAIConfig.Services.First();
            }

            // Register Semantic Kernel with multiple service support
            services.AddSingleton<Kernel>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Kernel>>();
                var builder = Kernel.CreateBuilder();

                // Chat Completion Service
                var chatService = GetServiceForType("ChatCompletion");
                if (!string.IsNullOrEmpty(chatService.Models.ChatCompletion))
                {
                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: chatService.Models.ChatCompletion,
                        endpoint: chatService.Endpoint,
                        apiKey: chatService.ApiKey);
                }

                // Text Embedding Service
                var embeddingService = GetServiceForType("TextEmbedding");
                if (!string.IsNullOrEmpty(embeddingService.Models.TextEmbedding))
                {
#pragma warning disable SKEXP0010
                    builder.AddAzureOpenAITextEmbeddingGeneration(
                        deploymentName: embeddingService.Models.TextEmbedding,
                        endpoint: embeddingService.Endpoint,
                        apiKey: embeddingService.ApiKey);
#pragma warning restore SKEXP0010
                }

                // Text-to-Image Service
                var imageService = GetServiceForType("TextToImage");
                if (!string.IsNullOrEmpty(imageService.Models.TextToImage))
                {
#pragma warning disable SKEXP0010
                    builder.AddAzureOpenAITextToImage(
                        deploymentName: imageService.Models.TextToImage,
                        endpoint: imageService.Endpoint,
                        apiKey: imageService.ApiKey);
#pragma warning restore SKEXP0010
                }

                // Audio-to-Text Service (Whisper)
                var audioToTextService = GetServiceForType("AudioToText");
                if (!string.IsNullOrEmpty(audioToTextService.Models.AudioToText))
                {
#pragma warning disable SKEXP0010
                    builder.AddAzureOpenAIAudioToText(
                        deploymentName: audioToTextService.Models.AudioToText,
                        endpoint: audioToTextService.Endpoint,
                        apiKey: audioToTextService.ApiKey);
#pragma warning restore SKEXP0010
                }
                else
                {
                    // Fallback to whisper if no specific deployment configured
#pragma warning disable SKEXP0010
                    builder.AddAzureOpenAIAudioToText(
                        deploymentName: "whisper",
                        endpoint: chatService.Endpoint,
                        apiKey: chatService.ApiKey);
#pragma warning restore SKEXP0010
                }

                // Text-to-Speech Service
                var ttsService = GetServiceForType("TextToSpeech");
                if (!string.IsNullOrEmpty(ttsService.Models.TextToSpeech))
                {
#pragma warning disable SKEXP0010
                    builder.AddAzureOpenAITextToAudio(
                        deploymentName: ttsService.Models.TextToSpeech,
                        endpoint: ttsService.Endpoint,
                        apiKey: ttsService.ApiKey);
#pragma warning restore SKEXP0010
                }

                // Add logging
                builder.Services.AddSingleton(serviceProvider.GetRequiredService<ILoggerFactory>());
                
                return builder.Build();
            });

            // Register the Azure OpenAI service directly since it implements CxCoreAI.IAiService
            services.AddSingleton<CxCoreAI.IAiService, CxAzureAI.AzureOpenAIService>();
            
            // Register all the individual AI services that can be imported
            services.AddSingleton<CxLanguage.StandardLibrary.AI.TextGeneration.TextGenerationService>();
            services.AddSingleton<CxLanguage.StandardLibrary.AI.ChatCompletion.ChatCompletionService>();
            services.AddSingleton<CxLanguage.StandardLibrary.AI.TextEmbeddings.TextEmbeddingsService>();
            services.AddSingleton<CxLanguage.StandardLibrary.AI.TextToImage.TextToImageService>();
            services.AddSingleton<CxLanguage.StandardLibrary.AI.ImageToText.ImageToTextService>();
            services.AddSingleton<CxLanguage.StandardLibrary.AI.TextToAudio.TextToAudioService>();
            services.AddSingleton<CxLanguage.StandardLibrary.AI.TextToSpeech.TextToSpeechService>();
            services.AddSingleton<CxLanguage.StandardLibrary.AI.AudioToText.AudioToTextService>();
            services.AddSingleton<CxLanguage.StandardLibrary.AI.Realtime.RealtimeService>();
        }

        return services;
    }
}
