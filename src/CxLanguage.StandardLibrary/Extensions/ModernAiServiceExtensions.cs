using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.AI.Inference;
using Azure;
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

            // Register Azure AI Inference chat client
            services.AddSingleton<IChatClient>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<IChatClient>>();
                logger.LogInformation("🚀 Initializing Azure OpenAI ChatClient with Microsoft.Extensions.AI");
                logger.LogInformation("📍 Endpoint: {Endpoint}", endpoint);
                logger.LogInformation("🤖 Model: {DeploymentName}", deploymentName);

                // Use Azure OpenAI client directly with Microsoft.Extensions.AI
                var client = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                var chatClient = client.AsChatClient(deploymentName);
                
                logger.LogInformation("✅ Azure OpenAI ChatClient initialized successfully");
                return chatClient;
            });

            // Register our modern AI service
            services.AddSingleton<ModernAiService>();

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
            return services.AddModernAiServices(configuration);
        }
    }
}
