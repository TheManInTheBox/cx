using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using Azure;
using CxLanguage.StandardLibrary.AI.Chat;
using CxLanguage.StandardLibrary.AI.Embeddings;
using CxLanguage.StandardLibrary.AI.Modern;

namespace CxLanguage.StandardLibrary.Extensions
{
    /// <summary>
    /// Extension methods for registering modern AI services with Microsoft.Extensions.AI
    /// Replaces heavy Semantic Kernel registration with lightweight, direct integration
    /// </summary>
    public static class ModernAiServiceExtensions
    {
        /// <summary>
        /// Add modern AI services using Microsoft.Extensions.AI
        /// </summary>
        public static IServiceCollection AddModernAiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Get Azure OpenAI configuration
            var azureConfig = configuration.GetSection("AzureOpenAI");
            var endpoint = azureConfig["Endpoint"];
            var apiKey = azureConfig["ApiKey"];
            var deploymentName = azureConfig["DeploymentName"] ?? "gpt-4";

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Azure OpenAI configuration is missing. Please set Endpoint and ApiKey in AzureOpenAI section.");
            }

            // Register custom chat client with Azure OpenAI integration
            services.AddSingleton<IChatClient>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<CustomChatClient>>();
                logger.LogInformation("🚀 Initializing CustomChatClient with Azure OpenAI");
                logger.LogInformation("📍 Endpoint: {Endpoint}", endpoint);
                logger.LogInformation("🤖 Model: {DeploymentName}", deploymentName);

                // Create Azure OpenAI client
                var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                var customChatClient = new CustomChatClient(azureClient, deploymentName, logger);
                
                logger.LogInformation("✅ CustomChatClient initialized successfully");
                return customChatClient;
            });

            // Register custom embedding generator
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<CustomEmbeddingGenerator>>();
                var embeddingDeploymentName = configuration["AzureOpenAI:EmbeddingDeploymentName"] ?? "text-embedding-3-small";
                
                logger.LogInformation("🧠 Initializing CustomEmbeddingGenerator with Azure OpenAI");
                logger.LogInformation("📍 Endpoint: {Endpoint}", endpoint);
                logger.LogInformation("🤖 Embedding Model: {DeploymentName}", embeddingDeploymentName);

                // Create Azure OpenAI client
                var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                var customEmbeddingGenerator = new CustomEmbeddingGenerator(azureClient, embeddingDeploymentName, logger);
                
                logger.LogInformation("✅ CustomEmbeddingGenerator initialized successfully");
                return customEmbeddingGenerator;
            });

            return services;
        }

        /// <summary>
        /// Add modern AI services as a replacement for Semantic Kernel + Kernel Memory
        /// This method provides a drop-in replacement with the same interface
        /// </summary>
        public static IServiceCollection ReplaceWithModernAi(this IServiceCollection services, IConfiguration configuration)
        {
            // Remove existing heavy dependencies (if any)
            // Note: In practice, you'd remove SK registrations here
            
            // Add our lightweight modern AI services
            return services.AddModernCxAiServices(configuration);
        }
    }
}
