using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using CxLanguage.StandardLibrary.AI.VectorDatabase;
using Microsoft.KernelMemory.AI.AzureOpenAI;
using KMSearchResult = Microsoft.KernelMemory.SearchResult;

namespace CxLanguage.CLI.Extensions;

/// <summary>
/// Extension methods for Kernel Memory service registration
/// </summary>
public static class KernelMemoryServiceExtensions
{
    /// <summary>
    /// Add KernelMemory services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddKernelMemoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            Console.WriteLine("Vector Database: Starting service registration with KernelMemory 0.98.x");
            
            // Get Azure OpenAI configuration
            var azureOpenAISection = configuration.GetSection("AzureOpenAI");
            if (!azureOpenAISection.Exists())
            {
                Console.WriteLine("Vector Database: No AzureOpenAI configuration found, skipping KernelMemory registration");
                return services;
            }

            // For new KernelMemory API, we'll use a simpler approach with Azure OpenAI configuration
            var azureOpenAIConfig = new AzureOpenAIConfig
            {
                APIKey = azureOpenAISection["ApiKey"] ?? string.Empty,
                Endpoint = azureOpenAISection["Endpoint"] ?? string.Empty,
                Deployment = azureOpenAISection["DeploymentName"] ?? string.Empty,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };

            var kernelMemoryBuilder = new KernelMemoryBuilder()
                .WithAzureOpenAITextGeneration(azureOpenAIConfig)
                .WithAzureOpenAITextEmbeddingGeneration(azureOpenAIConfig);

            Console.WriteLine("Vector Database: Building KernelMemory with Azure OpenAI configuration");
            
            var kernelMemory = kernelMemoryBuilder.Build();
            Console.WriteLine("Vector Database: KernelMemory built successfully");

            // Register services
            services.AddSingleton<IKernelMemory>(kernelMemory);
            services.AddScoped<VectorDatabaseService>();
            
            Console.WriteLine("Vector Database services registered successfully");
            return services;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Vector Database: Failed to register services - {ex.Message}");
            Console.WriteLine($"Vector Database: Exception details: {ex}");
            return services;
        }
    }
}
