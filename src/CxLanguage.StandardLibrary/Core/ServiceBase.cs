using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;

namespace CxLanguage.StandardLibrary.Core;

/// <summary>
/// Base class for CX classes that need service injection capabilities
/// This enables inheritance-based service access without explicit 'uses' declarations
/// </summary>
public abstract class ServiceBase
{
    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    protected readonly IServiceProvider _serviceProvider;
    
    /// <summary>
    /// Logger instance
    /// </summary>
    protected readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of ServiceBase with service provider injection
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    protected ServiceBase(IServiceProvider serviceProvider, ILogger<ServiceBase> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a service of the specified type
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service instance</returns>
    protected T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Get a service of the specified type, or null if not found
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service instance or null</returns>
    protected T? GetOptionalService<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }
}

/// <summary>
/// Enhanced service base with realtime AI capabilities by default
/// All CX classes inherit realtime thinking and communication as the base cognitive mode
/// Other specialized services are opt-in via interfaces
/// </summary>
public abstract class AiServiceBase : ServiceBase
{
    /// <summary>
    /// Initializes AI service base with realtime-first cognitive architecture
    /// </summary>
    protected AiServiceBase(IServiceProvider serviceProvider, ILogger<ServiceBase> logger)
        : base(serviceProvider, logger)
    {
    }

    // DEFAULT REALTIME COGNITIVE METHODS - Available to ALL classes
    // These are placeholders for the realtime-first architecture
    // In the future, these will connect to Azure OpenAI Realtime API

    /// <summary>
    /// Realtime thinking - accessible via self.ThinkAsync()
    /// Default cognitive mode for all CX classes
    /// </summary>
    public async Task<string> ThinkAsync(string input, object? options = null)
    {
        // Placeholder implementation - will be replaced with Azure OpenAI Realtime API
        _logger.LogInformation("ThinkAsync called with input: {Input}", input);
        await Task.Delay(10); // Simulate thinking
        return $"Thinking about: {input}";
    }

    /// <summary>
    /// Realtime communication - accessible via self.CommunicateAsync()
    /// Default communication mode for all CX classes
    /// </summary>
    public async Task CommunicateAsync(string message, object? options = null)
    {
        // Placeholder implementation - will be replaced with Azure OpenAI Realtime API
        _logger.LogInformation("CommunicateAsync called with message: {Message}", message);
        await Task.Delay(10); // Simulate communication
    }

    /// <summary>
    /// Realtime connection - accessible via self.ConnectAsync()
    /// Establishes realtime cognitive link
    /// </summary>
    public async Task<object> ConnectAsync(object? options = null)
    {
        // Placeholder implementation - will be replaced with Azure OpenAI Realtime API
        _logger.LogInformation("ConnectAsync called");
        await Task.Delay(10); // Simulate connection
        return new { status = "connected", mode = "realtime" };
    }

    /// <summary>
    /// Core text generation - accessible via self.GenerateAsync()
    /// Basic reasoning capability for all classes
    /// </summary>
    public async Task<string> GenerateAsync(string prompt, object? options = null)
    {
        try
        {
            // Try to use the actual TextGenerationService if available
            var textGenService = GetOptionalService<CxLanguage.StandardLibrary.AI.TextGeneration.TextGenerationService>();
            if (textGenService != null)
            {
                var result = await textGenService.GenerateAsync(prompt);
                return result.GeneratedText;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to use TextGenerationService, using placeholder");
        }
        
        // Fallback placeholder
        return $"Generated response for: {prompt}";
    }

    /// <summary>
    /// Core chat capability - accessible via self.ChatAsync()
    /// Conversational intelligence for all classes
    /// </summary>
    public async Task<string> ChatAsync(string message, object? options = null)
    {
        try
        {
            // Try to use the actual ChatCompletionService if available
            var chatService = GetOptionalService<CxLanguage.StandardLibrary.AI.ChatCompletion.ChatCompletionService>();
            if (chatService != null)
            {
                var result = await chatService.CompleteAsync(message);
                return result.Response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to use ChatCompletionService, using placeholder");
        }
        
        // Fallback placeholder
        return $"Chat response to: {message}";
    }

    // OPTIONAL SPECIALIZED METHODS - Only available when interfaces are implemented
    // These will throw meaningful errors if the required interfaces aren't implemented
    
    /// <summary>
    /// Text-to-speech - accessible via self.SpeakAsync() (requires ITextToSpeech)
    /// </summary>
    public async Task SpeakAsync(string text, object? options = null)
    {
        // This would require ITextToSpeech interface implementation
        _logger.LogInformation("SpeakAsync called with text: {Text}", text);
        await Task.Delay(10);
        // TODO: Check if class implements ITextToSpeech, throw error if not
    }

    /// <summary>
    /// Image generation - accessible via self.CreateImageAsync() (requires IImageGeneration)
    /// </summary>
    public async Task<string> CreateImageAsync(string prompt, object? options = null)
    {
        // This would require IImageGeneration interface implementation
        _logger.LogInformation("CreateImageAsync called with prompt: {Prompt}", prompt);
        await Task.Delay(10);
        // TODO: Check if class implements IImageGeneration, throw error if not
        return "placeholder-image-url";
    }
}
