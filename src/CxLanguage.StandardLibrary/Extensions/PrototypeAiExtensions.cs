using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using Azure;
using OpenAI.Chat;

namespace CxLanguage.StandardLibrary.Extensions
{
    /// <summary>
    /// Simple prototype for modern AI services without conflicting dependencies
    /// This is a proof of concept for replacing Semantic Kernel with Microsoft.Extensions.AI
    /// </summary>
    public static class PrototypeAiExtensions
    {
        /// <summary>
        /// Add modern AI services as a prototype (without removing SK yet)
        /// </summary>
        public static IServiceCollection AddPrototypeModernAi(this IServiceCollection services, IConfiguration configuration)
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

            // Register a simple modern AI service as proof of concept
            services.AddSingleton<IPrototypeAiService>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<IPrototypeAiService>>();
                
                // Create Azure OpenAI client
                var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                
                return new PrototypeAiService(client, deploymentName, logger);
            });

            return services;
        }
    }

    /// <summary>
    /// Simple interface for our prototype AI service
    /// </summary>
    public interface IPrototypeAiService
    {
        Task<string> ThinkAsync(string prompt);
        Task<string> LearnAsync(string data);
    }

    /// <summary>
    /// Prototype implementation using direct Azure OpenAI client
    /// </summary>
    public class PrototypeAiService : IPrototypeAiService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;
        private readonly ILogger<IPrototypeAiService> _logger;

        public PrototypeAiService(AzureOpenAIClient client, string deploymentName, ILogger<IPrototypeAiService> logger)
        {
            _client = client;
            _deploymentName = deploymentName;
            _logger = logger;
        }

        public async Task<string> ThinkAsync(string prompt)
        {
            _logger.LogInformation("🧠 Prototype: Processing think request with direct Azure OpenAI");
            
            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new List<OpenAI.Chat.ChatMessage>
            {
                OpenAI.Chat.ChatMessage.CreateUserMessage(prompt)
            };
            var result = await chatClient.CompleteChatAsync(messages);
            
            var response = result.Value.Content[0].Text;
            _logger.LogInformation("🧠 Prototype: Think completed - {Length} characters", response.Length);
            
            return response;
        }

        public async Task<string> LearnAsync(string data)
        {
            _logger.LogInformation("📚 Prototype: Processing learn request");
            
            var promptText = $"Analyze and summarize this learning data: {data}";
            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new List<OpenAI.Chat.ChatMessage>
            {
                OpenAI.Chat.ChatMessage.CreateUserMessage(promptText)
            };
            var result = await chatClient.CompleteChatAsync(messages);
            
            var insights = result.Value.Content[0].Text;
            _logger.LogInformation("📚 Prototype: Learn completed - generated insights");
            
            return insights;
        }
    }
}
