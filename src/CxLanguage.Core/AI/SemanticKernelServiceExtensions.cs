using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace CxLanguage.Core.AI;

/// <summary>
/// Extension methods for configuring Semantic Kernel services
/// </summary>
public static class SemanticKernelServiceExtensions
{
    /// <summary>
    /// Adds Semantic Kernel services to the service collection
    /// </summary>
    public static IServiceCollection AddSemanticKernelServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get Azure OpenAI configuration
        var azureOpenAIConfig = configuration.GetSection("AzureOpenAI");
        var endpoint = azureOpenAIConfig["Endpoint"] ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is required");
        var apiKey = azureOpenAIConfig["ApiKey"] ?? throw new InvalidOperationException("AzureOpenAI:ApiKey is required");
        var deploymentName = azureOpenAIConfig["DeploymentName"] ?? "gpt-4o-mini";

        // Register Semantic Kernel
        services.AddSingleton<Kernel>(serviceProvider =>
        {
            var builder = Kernel.CreateBuilder();
            
            // Add Azure OpenAI chat completion
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey);
            
            // Add logging
            builder.Services.AddSingleton(serviceProvider.GetRequiredService<ILoggerFactory>());
            
            return builder.Build();
        });

        return services;
    }
}
