using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure;
using System;
using CxLanguage.StandardLibrary.AI.Chat;
using CxLanguage.StandardLibrary.AI.Embeddings;

namespace CxLanguage.StandardLibrary.Extensions
{
    /// <summary>
    /// Service collection extensions for modern CX Language AI services
    /// Complete replacement for heavy Semantic Kernel + Kernel Memory dependencies
    /// </summary>
    public static class ModernCxServiceExtensions
    {
        /// <summary>
        /// Add complete modern CX Language AI services using Microsoft.Extensions.AI
        /// Lightweight, performant replacement for the old heavy dependencies
        /// </summary>
        public static IServiceCollection AddModernCxAiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Validate configuration first
            ValidateAzureOpenAiConfiguration(configuration);

            // Add core modern AI services
            AddModernAiServices(services, configuration);

            // Add CX-specific AI services
            services.AddScoped<global::CxLanguage.StandardLibrary.AI.Wait.AwaitService>();
            services.AddScoped<global::CxLanguage.StandardLibrary.AI.Realtime.ModernRealtimeService>();
            services.AddScoped<global::CxLanguage.StandardLibrary.ExecuteService>();
            
            // Add Voice Processing Services
            services.AddSingleton<global::CxLanguage.StandardLibrary.Services.IVoiceInputService, global::CxLanguage.StandardLibrary.Services.VoiceInputService>();
            services.AddSingleton<global::CxLanguage.StandardLibrary.Services.IVoiceOutputService, global::CxLanguage.StandardLibrary.Services.VoiceOutputService>();
            
            // Add Event Bridges for Service Integration
            services.AddSingleton<global::CxLanguage.StandardLibrary.EventBridges.VoiceInputEventBridge>();
            services.AddSingleton<global::CxLanguage.StandardLibrary.EventBridges.VoiceOutputEventBridge>();
            services.AddSingleton<global::CxLanguage.StandardLibrary.EventBridges.AzureRealtimeEventBridge>();
            
            // Add Voice Service Initialization
            services.AddHostedService<global::CxLanguage.StandardLibrary.Services.VoiceServiceInitializer>();

            return services;
        }

        /// <summary>
        /// Quick setup for development with default configuration
        /// </summary>
        public static IServiceCollection AddModernCxAiServicesWithDefaults(
            this IServiceCollection services,
            string azureOpenAiEndpoint,
            string deploymentName,
            string? apiKey = null)
        {
            // Create in-memory configuration
            var configDict = new Dictionary<string, string?>
            {
                ["AzureOpenAI:Endpoint"] = azureOpenAiEndpoint,
                ["AzureOpenAI:DeploymentName"] = deploymentName,
                ["AzureOpenAI:ApiKey"] = apiKey,
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            return services.AddModernCxAiServices(config);
        }

        /// <summary>
        /// Validate Azure OpenAI configuration
        /// </summary>
        public static void ValidateAzureOpenAiConfiguration(IConfiguration configuration)
        {
            var azureConfig = configuration.GetSection("AzureOpenAI");
            
            // Check for new per-service configuration structure
            var chatConfig = azureConfig.GetSection("Chat");
            var legacyConfig = azureConfig.GetSection("Legacy");
            
            bool hasNewConfig = !string.IsNullOrEmpty(chatConfig["Endpoint"]) && !string.IsNullOrEmpty(chatConfig["ApiKey"]);
            bool hasLegacyConfig = !string.IsNullOrEmpty(azureConfig["Endpoint"]) && !string.IsNullOrEmpty(azureConfig["ApiKey"]);
            bool hasLegacySection = !string.IsNullOrEmpty(legacyConfig["Endpoint"]) && !string.IsNullOrEmpty(legacyConfig["ApiKey"]);
            
            if (!hasNewConfig && !hasLegacyConfig && !hasLegacySection)
            {
                throw new InvalidOperationException("Azure OpenAI configuration is missing. Please set Endpoint and ApiKey in AzureOpenAI section or use the new per-service configuration structure.");
            }
        }

        /// <summary>
        /// Get Azure OpenAI configuration for a specific service type
        /// </summary>
        public static (string endpoint, string apiKey, string deploymentName) GetAzureOpenAIConfig(
            IConfiguration configuration, string serviceType, string defaultDeployment = "")
        {
            var azureConfig = configuration.GetSection("AzureOpenAI");
            
            // Try new per-service configuration first
            var serviceConfig = azureConfig.GetSection(serviceType);
            if (!string.IsNullOrEmpty(serviceConfig["Endpoint"]) && !string.IsNullOrEmpty(serviceConfig["ApiKey"]))
            {
                return (
                    serviceConfig["Endpoint"] ?? "",
                    serviceConfig["ApiKey"] ?? "",
                    serviceConfig["DeploymentName"] ?? defaultDeployment
                );
            }
            
            // Fall back to legacy configuration
            var legacyConfig = azureConfig.GetSection("Legacy");
            if (!string.IsNullOrEmpty(legacyConfig["Endpoint"]) && !string.IsNullOrEmpty(legacyConfig["ApiKey"]))
            {
                var deploymentKey = serviceType switch
                {
                    "Chat" => "DeploymentName",
                    "Embedding" => "EmbeddingDeploymentName", 
                    "Image" => "ImageDeploymentName",
                    "Realtime" => "RealtimeDeploymentName",
                    _ => "DeploymentName"
                };
                
                return (
                    legacyConfig["Endpoint"] ?? "",
                    legacyConfig["ApiKey"] ?? "",
                    legacyConfig[deploymentKey] ?? defaultDeployment
                );
            }
            
            // Final fallback to root-level legacy configuration
            var deploymentKey2 = serviceType switch
            {
                "Chat" => "DeploymentName",
                "Embedding" => "EmbeddingDeploymentName",
                "Image" => "ImageDeploymentName", 
                "Realtime" => "RealtimeDeploymentName",
                _ => "DeploymentName"
            };
            
            return (
                azureConfig["Endpoint"] ?? "",
                azureConfig["ApiKey"] ?? "",
                azureConfig[deploymentKey2] ?? defaultDeployment
            );
        }

        /// <summary>
        /// Add modern AI services using Microsoft.Extensions.AI
        /// </summary>
        public static IServiceCollection AddModernAiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Get Azure OpenAI configuration for chat service
            var (chatEndpoint, chatApiKey, chatDeploymentName) = GetAzureOpenAIConfig(configuration, "Chat", "gpt-4");
            var (embeddingEndpoint, embeddingApiKey, embeddingDeploymentName) = GetAzureOpenAIConfig(configuration, "Embedding", "text-embedding-ada-002");

            // Register IChatClient using our custom implementation
            services.AddSingleton<IChatClient>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<CustomChatClient>>();
                logger.LogInformation("🚀 Initializing CustomChatClient with Azure OpenAI");
                logger.LogInformation("🔗 Chat Endpoint: {Endpoint}", chatEndpoint);
                logger.LogInformation("📦 Chat Deployment: {Deployment}", chatDeploymentName);
                var client = new AzureOpenAIClient(new Uri(chatEndpoint), new AzureKeyCredential(chatApiKey));
                return new CustomChatClient(client, chatDeploymentName, logger);
            });

            // Register IEmbeddingGenerator using our custom implementation  
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<CustomEmbeddingGenerator>>();
                logger.LogInformation("🚀 Initializing CustomEmbeddingGenerator with Azure OpenAI");
                logger.LogInformation("🔗 Embedding Endpoint: {Endpoint}", embeddingEndpoint);
                logger.LogInformation("📦 Embedding Deployment: {Deployment}", embeddingDeploymentName);
                var client = new AzureOpenAIClient(new Uri(embeddingEndpoint), new AzureKeyCredential(embeddingApiKey));
                return new CustomEmbeddingGenerator(client, embeddingDeploymentName, logger);
            });

            return services;
        }

        /// <summary>
        /// Adds the modern CX services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddModernCxServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModernCxAiServices(configuration);
            services.AddSingleton<CxLanguage.StandardLibrary.AI.Wait.AwaitService>();

            return services;
        }
    }
}
