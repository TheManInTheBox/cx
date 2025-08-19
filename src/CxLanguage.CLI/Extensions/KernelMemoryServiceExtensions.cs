// NEURAL SYSTEM BYPASS: Temporarily disabled for biological neural testing
// Legacy Kernel Memory references removed for compiler compatibility
/*
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
                        
            // Get Azure OpenAI configuration
            var azureOpenAISection = configuration.GetSection("AzureOpenAI");
            if (!azureOpenAISection.Exists())
            {
                Console.WriteLine("Vector Database: No AzureOpenAI configuration found, skipping KernelMemory registration");
                return services;
            }

            // Handle multi-service configuration format
            string apiKey = string.Empty;
            string endpoint = string.Empty;
            string embeddingDeployment = string.Empty;
            string chatDeployment = string.Empty;
            
            var servicesSection = azureOpenAISection.GetSection("Services");
            if (servicesSection.Exists())
            {
                // Multi-service configuration
                var defaultService = azureOpenAISection["DefaultService"] ?? "EastUS";
                var embeddingService = azureOpenAISection.GetSection("ServiceSelection")["TextEmbedding"] ?? defaultService;
                
                // Find the embedding service configuration
                foreach (var serviceSection in servicesSection.GetChildren())
                {
                    var serviceName = serviceSection["Name"];
                    if (serviceName == embeddingService)
                    {
                        apiKey = serviceSection["ApiKey"] ?? string.Empty;
                        endpoint = serviceSection["Endpoint"] ?? string.Empty;
                        embeddingDeployment = serviceSection.GetSection("Models")["TextEmbedding"] ?? string.Empty;
                        chatDeployment = serviceSection.GetSection("Models")["ChatCompletion"] ?? string.Empty;
                                                break;
                    }
                }
            }
            else
            {
                // Legacy single-service configuration
                apiKey = azureOpenAISection["ApiKey"] ?? string.Empty;
                endpoint = azureOpenAISection["Endpoint"] ?? string.Empty;
                embeddingDeployment = azureOpenAISection["EmbeddingDeploymentName"] ?? azureOpenAISection["DeploymentName"] ?? string.Empty;
                chatDeployment = azureOpenAISection["DeploymentName"] ?? string.Empty;
            }

            // Create separate configs for embeddings and text generation
            var embeddingConfig = new AzureOpenAIConfig
            {
                APIKey = apiKey,
                Endpoint = endpoint,
                Deployment = embeddingDeployment,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };

            var textGenConfig = new AzureOpenAIConfig
            {
                APIKey = apiKey,
                Endpoint = endpoint,
                Deployment = chatDeployment,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            };

            var kernelMemoryBuilder = new KernelMemoryBuilder()
                .WithAzureOpenAITextGeneration(textGenConfig)
                .WithAzureOpenAITextEmbeddingGeneration(embeddingConfig);

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
*/

namespace CxLanguage.CLI.Extensions
{
    // NEURAL SYSTEM: Empty placeholder for legacy Kernel Memory extensions
}

