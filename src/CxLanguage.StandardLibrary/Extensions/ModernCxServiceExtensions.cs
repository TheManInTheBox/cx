using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

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

            // Register ChatClient
            services.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<global::Azure.AI.OpenAI.ChatClient>>();
                logger.LogInformation("🚀 Initializing Azure OpenAI ChatClient with Microsoft.Extensions.AI");
                var client = new global::Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint!), new global::Azure.AzureKeyCredential(apiKey!));
                return client.GetChatClient(chatDeploymentName);
            });

            // Register EmbeddingClient
            services.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<global::Azure.AI.OpenAI.EmbeddingClient>>();
                logger.LogInformation("🚀 Initializing Azure OpenAI EmbeddingClient with Microsoft.Extensions.AI");
                var client = new global::Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint!), new global::Azure.AzureKeyCredential(apiKey!));
                return client.GetEmbeddingClient(embeddingDeploymentName);
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
            services.AddModernAiServices(configuration);
            services.AddSingleton<CxLanguage.StandardLibrary.AI.Wait.AwaitService>();

            return services;
        }
    }
}
