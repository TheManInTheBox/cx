using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using CxCoreAI = CxLanguage.Core.AI;
using CxAzureAI = CxLanguage.Azure.Services;

namespace CxLanguage.Compiler.Modules;

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
        var azureOpenAIConfig = configuration.GetSection("AzureOpenAI");
        var endpoint = azureOpenAIConfig["Endpoint"];
        var apiKey = azureOpenAIConfig["ApiKey"];
        var deploymentName = azureOpenAIConfig["DeploymentName"] ?? "gpt-4o-mini";

        // Only add SK if we have valid configuration
        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey))
        {
            // Register Semantic Kernel
            services.AddSingleton<Kernel>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Kernel>>();
                
                var builder = Kernel.CreateBuilder();
                
                // Add Azure OpenAI chat completion
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: deploymentName,
                    endpoint: endpoint,
                    apiKey: apiKey);

                // Add Azure OpenAI text embedding generation
#pragma warning disable SKEXP0010
                builder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: "text-embedding-ada-002",
                    endpoint: endpoint,
                    apiKey: apiKey);
#pragma warning restore SKEXP0010
                
                // Add logging
                builder.Services.AddSingleton(serviceProvider.GetRequiredService<ILoggerFactory>());
                
                return builder.Build();
            });

            // Register the Azure OpenAI service directly since it implements CxCoreAI.IAiService
            services.AddSingleton<CxCoreAI.IAiService, CxAzureAI.AzureOpenAIService>();
        }

        return services;
    }
}
