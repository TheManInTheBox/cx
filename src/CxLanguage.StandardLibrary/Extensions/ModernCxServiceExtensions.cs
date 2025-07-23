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
            if (string.IsNullOrEmpty(azureConfig["Endpoint"]) || string.IsNullOrEmpty(azureConfig["ApiKey"]))
            {
                throw new InvalidOperationException("Azure OpenAI configuration is missing. Please set Endpoint and ApiKey in AzureOpenAI section.");
            }
        }

        /// <summary>
        /// Add modern AI services using Microsoft.Extensions.AI
        /// </summary>
        public static IServiceCollection AddModernAiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Get Azure OpenAI configuration
            var azureConfig = configuration.GetSection("AzureOpenAI");
            var endpoint = azureConfig["Endpoint"];
            var apiKey = azureConfig["ApiKey"];
            var chatDeploymentName = azureConfig["DeploymentName"] ?? "gpt-4";
            var embeddingDeploymentName = azureConfig["EmbeddingDeploymentName"] ?? "text-embedding-ada-002";

            // Register IChatClient using our custom implementation
            services.AddSingleton<IChatClient>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<CustomChatClient>>();
                logger.LogInformation("🚀 Initializing CustomChatClient with Azure OpenAI");
                var client = new AzureOpenAIClient(new Uri(endpoint!), new AzureKeyCredential(apiKey!));
                return new CustomChatClient(client, chatDeploymentName, logger);
            });

            // Register IEmbeddingGenerator using our custom implementation  
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<CustomEmbeddingGenerator>>();
                logger.LogInformation("🚀 Initializing CustomEmbeddingGenerator with Azure OpenAI");
                var client = new AzureOpenAIClient(new Uri(endpoint!), new AzureKeyCredential(apiKey!));
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
