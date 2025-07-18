using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using CxLanguage.Core.AI;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Semantic Kernel-based AI service for Cx language
/// Provides production-ready AI orchestration and planning
/// </summary>
public class SemanticKernelAiService : IAiService
{
    private readonly ILogger<SemanticKernelAiService> _logger;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public SemanticKernelAiService(
        ILogger<SemanticKernelAiService> logger,
        Kernel kernel)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    /// <summary>
    /// Generates text using the Semantic Kernel chat completion service
    /// </summary>
    public async Task<AiResponse> GenerateTextAsync(string prompt, AiRequestOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Generating text with Semantic Kernel for prompt: {Prompt}", prompt);

            // Create chat history with the prompt
            var chatHistory = new ChatHistory();
            
            // Add system message if provided
            if (!string.IsNullOrEmpty(options?.SystemPrompt))
            {
                chatHistory.AddSystemMessage(options.SystemPrompt);
            }
            
            // Add user message
            chatHistory.AddUserMessage(prompt);

            // Create OpenAI prompt execution settings
            var settings = new OpenAIPromptExecutionSettings
            {
                Temperature = options?.Temperature ?? 0.7,
                MaxTokens = options?.MaxTokens ?? 1000,
                TopP = 1.0,
                FrequencyPenalty = 0.0,
                PresencePenalty = 0.0
            };

            // Generate response
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                chatHistory, 
                settings);

            return new AiResponse
            {
                Content = response.Content ?? string.Empty,
                IsSuccess = true,
                Usage = response.Metadata?.ContainsKey("Usage") == true ? 
                    GetTokenUsage(response.Metadata["Usage"]) : null,
                Metadata = new Dictionary<string, object> { ["model"] = "semantic-kernel" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text with Semantic Kernel");
            return new AiResponse
            {
                Content = string.Empty,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Metadata = new Dictionary<string, object> { ["model"] = "semantic-kernel" }
            };
        }
    }

    /// <summary>
    /// Analyzes text using the Semantic Kernel
    /// </summary>
    public async Task<AiResponse> AnalyzeAsync(string content, AiAnalysisOptions options)
    {
        var analysisPrompt = $"Please analyze the following text and provide insights:\n\nTask: {options.Task}\nContent: {content}";
        var requestOptions = new AiRequestOptions
        {
            Temperature = 0.5,
            MaxTokens = 2000,
            SystemPrompt = "You are an expert analyst. Provide clear, structured analysis."
        };
        return await GenerateTextAsync(analysisPrompt, requestOptions);
    }

    /// <summary>
    /// Streams text generation using the Semantic Kernel
    /// </summary>
    public Task<AiStreamResponse> StreamGenerateTextAsync(string prompt, AiRequestOptions? options = null)
    {
        return Task.FromResult<AiStreamResponse>(new SemanticKernelStreamResponse(_chatCompletionService, prompt, options));
    }

    /// <summary>
    /// Processes multiple requests in batch using the Semantic Kernel
    /// </summary>
    public async Task<AiResponse[]> ProcessBatchAsync(string[] prompts, AiRequestOptions? options = null)
    {
        var tasks = prompts.Select(prompt => GenerateTextAsync(prompt, options));
        var results = await Task.WhenAll(tasks);
        return results.ToArray();
    }

    /// <summary>
    /// Extracts token usage from metadata
    /// </summary>
    private AiUsage GetTokenUsage(object? usage)
    {
        if (usage == null) return new AiUsage();
        
        try
        {
            // This is a simple implementation - in reality, you'd parse the usage metadata
            // from the OpenAI response format
            return new AiUsage();
        }
        catch
        {
            return new AiUsage();
        }
    }
}

/// <summary>
/// Semantic Kernel-based streaming response
/// </summary>
public class SemanticKernelStreamResponse : AiStreamResponse
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly string _prompt;
    private readonly AiRequestOptions? _options;

    public SemanticKernelStreamResponse(IChatCompletionService chatCompletionService, string prompt, AiRequestOptions? options)
    {
        _chatCompletionService = chatCompletionService;
        _prompt = prompt;
        _options = options;
    }

    public override async IAsyncEnumerable<string> GetTokensAsync()
    {
        var chatHistory = new ChatHistory();
        
        if (!string.IsNullOrEmpty(_options?.SystemPrompt))
        {
            chatHistory.AddSystemMessage(_options.SystemPrompt);
        }
        
        chatHistory.AddUserMessage(_prompt);

        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature = _options?.Temperature ?? 0.7,
            MaxTokens = _options?.MaxTokens ?? 1000
        };

        await foreach (var content in _chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory, settings))
        {
            if (!string.IsNullOrEmpty(content.Content))
            {
                yield return content.Content;
            }
        }
    }

    public override void Dispose()
    {
        // Nothing to dispose in this implementation
    }
}
