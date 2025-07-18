using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CxLanguage.Core.AI;

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
                
                // Add logging
                builder.Services.AddSingleton(serviceProvider.GetRequiredService<ILoggerFactory>());
                
                return builder.Build();
            });

            // Register a simple AI service that uses Semantic Kernel
            services.AddSingleton<IAiService, SimpleSemanticKernelAiService>();
        }

        return services;
    }
}

/// <summary>
/// Simple AI service implementation using Semantic Kernel
/// </summary>
public class SimpleSemanticKernelAiService : IAiService
{
    private readonly Kernel _kernel;
    private readonly ILogger<SimpleSemanticKernelAiService> _logger;

    public SimpleSemanticKernelAiService(Kernel kernel, ILogger<SimpleSemanticKernelAiService> logger)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AiResponse> GenerateTextAsync(string prompt, AiRequestOptions? options = null)
    {
        try
        {
            var result = await _kernel.InvokePromptAsync(prompt);
            var content = result.GetValue<string>() ?? "";
            
            return AiResponse.Success(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text with Semantic Kernel");
            return AiResponse.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<AiResponse> AnalyzeAsync(string content, AiAnalysisOptions options)
    {
        try
        {
            var prompt = $"Analyze the following content for {options.Task}:\n\n{content}";
            var result = await _kernel.InvokePromptAsync(prompt);
            var analysis = result.GetValue<string>() ?? "Analysis completed";
            
            return AiResponse.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing content with Semantic Kernel");
            return AiResponse.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<AiStreamResponse> StreamGenerateTextAsync(string prompt, AiRequestOptions? options = null)
    {
        // For now, return a simple implementation that yields the full response
        var response = await GenerateTextAsync(prompt, options);
        return new SimpleStreamResponse(response.Content);
    }

    public async Task<AiResponse[]> ProcessBatchAsync(string[] prompts, AiRequestOptions? options = null)
    {
        var tasks = prompts.Select(prompt => GenerateTextAsync(prompt, options));
        var results = await Task.WhenAll(tasks);
        return results;
    }

    public async Task<string> ProcessAsync(string input, string context = "", object? options = null)
    {
        var prompt = $"Process the following input: {input}";
        if (!string.IsNullOrEmpty(context))
        {
            prompt += $"\nContext: {context}";
        }

        var response = await GenerateTextAsync(prompt);
        return response.IsSuccess ? response.Content : response.ErrorMessage ?? "Processing failed";
    }
}

/// <summary>
/// Simple stream response implementation
/// </summary>
public class SimpleStreamResponse : AiStreamResponse
{
    private readonly string _content;

    public SimpleStreamResponse(string content)
    {
        _content = content;
    }

    public override async IAsyncEnumerable<string> GetTokensAsync()
    {
        await Task.CompletedTask;
        yield return _content;
    }

    public override void Dispose()
    {
        // Nothing to dispose
    }
}
