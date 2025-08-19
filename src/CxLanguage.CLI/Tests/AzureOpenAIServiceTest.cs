using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CxLanguage.Azure.Services;
using System.Text.Json;

namespace CxLanguage.CLI.Tests;

public class AzureOpenAIServiceTest
{
    public static async Task TestAzureOpenAIService()
    {
        // Create configuration
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<AzureOpenAIService>();

        // Create service
        var azureOpenAIService = new AzureOpenAIService(config, logger);

        // Test the service
                
        try
        {
            var result = await azureOpenAIService.GenerateTextAsync("Hello, Azure OpenAI! Please respond with a short greeting.");
            Console.WriteLine($"Azure OpenAI Response: {result.Content}");
            Console.WriteLine($"Success: {result.IsSuccess}");
            Console.WriteLine($"Tokens Used: {result.Usage?.TotalTokens}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

