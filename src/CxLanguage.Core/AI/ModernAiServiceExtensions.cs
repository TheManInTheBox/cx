using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure.Identity;

namespace CxLanguage.Core.AI;

/// <summary>
/// Extension methods for configuring modern Microsoft.Extensions.AI services
/// Replaces heavy Semantic Kernel with lightweight direct AI integration
/// </summary>
public static class ModernAiServiceExtensions
{
    /// <summary>
    /// Adds modern AI services to the service collection using Microsoft.Extensions.AI
    /// </summary>
    public static IServiceCollection AddModernAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get Azure OpenAI configuration
        var azureOpenAiConfig = configuration.GetSection("AzureOpenAI");
        var endpoint = azureOpenAiConfig["Endpoint"];
        var deploymentName = azureOpenAiConfig["DeploymentName"];
        var apiKey = azureOpenAiConfig["ApiKey"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI configuration is missing. Please configure Endpoint and DeploymentName.");
        }

        // Add Azure OpenAI client as singleton
        services.AddSingleton<AzureOpenAIClient>(provider =>
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                return new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            }
            else
            {
                // Use managed identity when API key is not provided
                return new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
            }
        });

        // Add Azure OpenAI ChatClient directly - simpler and more reliable
        services.AddSingleton<OpenAI.Chat.ChatClient>(provider =>
        {
            var azureClient = provider.GetRequiredService<AzureOpenAIClient>();
            var logger = provider.GetRequiredService<ILogger<OpenAI.Chat.ChatClient>>();
            
            logger.LogInformation("🚀 Initializing OpenAI ChatClient with deployment: {DeploymentName}", deploymentName);
            
            return azureClient.GetChatClient(deploymentName);
        });

        // Add embedding client directly 
        var embeddingDeploymentName = azureOpenAiConfig["EmbeddingDeploymentName"] ?? "text-embedding-3-small";
        services.AddSingleton<OpenAI.Embeddings.EmbeddingClient>(provider =>
        {
            var azureClient = provider.GetRequiredService<AzureOpenAIClient>();
            var logger = provider.GetRequiredService<ILogger<OpenAI.Embeddings.EmbeddingClient>>();
            
            logger.LogInformation("📊 Initializing embedding client with deployment: {EmbeddingDeploymentName}", embeddingDeploymentName);
            
            return azureClient.GetEmbeddingClient(embeddingDeploymentName);
        });

        return services;
    }

    /// <summary>
    /// Configuration validation helper
    /// </summary>
    public static void ValidateAzureOpenAiConfiguration(IConfiguration configuration)
    {
        var azureOpenAiConfig = configuration.GetSection("AzureOpenAI");
        
        if (string.IsNullOrEmpty(azureOpenAiConfig["Endpoint"]))
        {
            throw new InvalidOperationException("AzureOpenAI:Endpoint is required");
        }
        
        if (string.IsNullOrEmpty(azureOpenAiConfig["DeploymentName"]))
        {
            throw new InvalidOperationException("AzureOpenAI:DeploymentName is required");
        }
    }
}
