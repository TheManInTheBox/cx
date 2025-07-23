using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using CxLanguage.Core.AI;
using CxLanguage.Core.Telemetry;
using CxLanguage.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// AI function handler for Cx language
/// Implements the 7 core AI functions:
/// - task() - Task planning and execution
/// - synthesize() - Code generation
/// - reason() - Logical reasoning and decision making
/// - process() - Multi-modal data processing
/// - generate() - Content generation
/// - embed() - Vector embeddings
/// - adapt() - Self-optimization and adaptation
/// </summary>
public class AiFunctions
{
    private readonly IAgenticRuntime _agenticRuntime;
    private readonly ILogger<AiFunctions> _logger;
    private readonly CxTelemetryService? _telemetryService;
    private readonly CxJsonDeserializer _jsonDeserializer;

    public AiFunctions(IAgenticRuntime agenticRuntime, ILogger<AiFunctions> logger, CxTelemetryService? telemetryService = null)
    {
        _agenticRuntime = agenticRuntime ?? throw new ArgumentNullException(nameof(agenticRuntime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryService = telemetryService;
        _jsonDeserializer = new CxJsonDeserializer(logger as ILogger<CxJsonDeserializer>);
    }

    /// <summary>
    /// Implements the 'task' AI function
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> TaskAsync(string goal, object? options = null)
    {
        _logger.LogInformation("Executing task function with goal: {Goal}", goal);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Convert options to the appropriate type
            var taskOptions = ConvertOptions<TaskPlanningOptions>(options);
            
            // Call the agentic runtime to plan and execute the task
            var planResult = await _agenticRuntime.PlanTaskAsync(goal, taskOptions);
            if (!planResult.IsSuccess || planResult.Plan == null)
            {
                _logger.LogWarning("Task planning failed: {Error}", planResult.ErrorMessage);
                var errorMessage = $"Task planning failed: {planResult.ErrorMessage}";
                _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, errorMessage);
                return errorMessage;
            }

            // Execute the task plan
            var executionResult = await _agenticRuntime.ExecuteTaskAsync(planResult.Plan);
            if (!executionResult.IsSuccess)
            {
                _logger.LogWarning("Task execution failed: {Error}", executionResult.ErrorMessage);
                var errorMessage = $"Task execution failed: {executionResult.ErrorMessage}";
                _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, errorMessage);
                return errorMessage;
            }

            // Get the result
            var result = executionResult.Results.TryGetValue("summary", out var summary) 
                ? summary?.ToString() ?? "Task completed successfully"
                : "Task completed successfully";

            // Try to deserialize as JSON to native CX object
            var cxObject = _jsonDeserializer.TryDeserializeFromResponse(result);
            
            _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, true);
            return cxObject ?? result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing task function");
            var errorMessage = $"Error: {ex.Message}";
            _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, errorMessage);
            return errorMessage;
        }
    }

    /// <summary>
    /// Implements the 'synthesize' AI function for code generation
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> SynthesizeAsync(string specification, object? options = null)
    {
        _logger.LogInformation("Executing synthesize function with specification: {Spec}", specification);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Convert options to the appropriate type
            var codeOptions = ConvertOptions<CodeSynthesisOptions>(options);
            
            // Call the agentic runtime to synthesize code
            var result = await _agenticRuntime.SynthesizeCodeAsync(specification, codeOptions);
            
            if (result.IsSuccess && result.GeneratedCode != null)
            {
                // Try to deserialize as JSON to native CX object
                var cxObject = _jsonDeserializer.TryDeserializeFromResponse(result.GeneratedCode);
                _telemetryService?.TrackAiFunctionExecution("synthesize", specification, stopwatch.Elapsed, true);
                return cxObject ?? result.GeneratedCode;
            }
            else
            {
                _logger.LogWarning("Code synthesis failed: {Error}", result.ErrorMessage);
                var errorMessage = $"Code synthesis failed: {result.ErrorMessage}";
                _telemetryService?.TrackAiFunctionExecution("synthesize", specification, stopwatch.Elapsed, false, errorMessage);
                return errorMessage;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing synthesize function");
            var errorMessage = $"Error: {ex.Message}";
            _telemetryService?.TrackAiFunctionExecution("synthesize", specification, stopwatch.Elapsed, false, errorMessage);
            return errorMessage;
        }
    }

    /// <summary>
    /// Implements the 'reason' AI function for logical reasoning
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> ReasonAsync(string question, object? options = null)
    {
        _logger.LogInformation("Executing reason function with question: {Question}", question);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Convert options to AI invocation options
            var aiOptions = ConvertOptions<AIInvocationOptions>(options);
            
            // Use the agentic runtime to reason about the question
            var result = await _agenticRuntime.InvokeAIFunctionAsync("reason", new object[] { question, aiOptions });
            
            var response = result?.ToString() ?? "Reasoning failed: No response from AI service";
            
            // Try to deserialize as JSON to native CX object
            var cxObject = _jsonDeserializer.TryDeserializeFromResponse(response);
            
            _telemetryService?.TrackAiFunctionExecution("reason", question, stopwatch.Elapsed, result != null);
            return cxObject ?? response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing reason function");
            var errorMessage = $"Error: {ex.Message}";
            _telemetryService?.TrackAiFunctionExecution("reason", question, stopwatch.Elapsed, false, errorMessage);
            return errorMessage;
        }
    }

    /// <summary>
    /// Implements the 'process' AI function for multi-modal data processing
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> ProcessAsync(string input, string context, object? options = null)
    {
        _logger.LogInformation("Executing process function with input: {Input}", input);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Convert options to AI invocation options
            var aiOptions = ConvertOptions<AIInvocationOptions>(options);
            
            // Use the agentic runtime to process the input
            var result = await _agenticRuntime.InvokeAIFunctionAsync("process", new object[] { input, context, aiOptions });
            
            var response = result?.ToString() ?? "Processing failed: No response from AI service";
            
            // Try to deserialize as JSON to native CX object
            var cxObject = _jsonDeserializer.TryDeserializeFromResponse(response);
            
            _telemetryService?.TrackAiFunctionExecution("process", input, stopwatch.Elapsed, result != null);
            return cxObject ?? response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing process function");
            var errorMessage = $"Error: {ex.Message}";
            _telemetryService?.TrackAiFunctionExecution("process", input, stopwatch.Elapsed, false, errorMessage);
            return errorMessage;
        }
    }

    /// <summary>
    /// Implements the 'generate' AI function for content generation
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> GenerateAsync(string prompt, object? options = null)
    {
        _logger.LogInformation("Executing generate function with prompt: {Prompt}", prompt);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Convert options to AI invocation options
            var aiOptions = ConvertOptions<AIInvocationOptions>(options);
            
            // Use the agentic runtime to generate content
            var result = await _agenticRuntime.InvokeAIFunctionAsync("generate", new object[] { prompt, aiOptions });
            
            var response = result?.ToString() ?? "Generation failed: No response from AI service";
            
            // Try to deserialize as JSON to native CX object
            var cxObject = _jsonDeserializer.TryDeserializeFromResponse(response);
            
            _telemetryService?.TrackAiFunctionExecution("generate", prompt, stopwatch.Elapsed, result != null);
            return cxObject ?? response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing generate function");
            var errorMessage = $"Error: {ex.Message}";
            _telemetryService?.TrackAiFunctionExecution("generate", prompt, stopwatch.Elapsed, false, errorMessage);
            return errorMessage;
        }
    }

    /// <summary>
    /// Implements the 'embed' AI function for vector embeddings
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> EmbedAsync(string text, object? options = null)
    {
        _logger.LogInformation("Executing embed function with text of length: {Length}", text.Length);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Convert options to AI invocation options
            var aiOptions = ConvertOptions<AIInvocationOptions>(options);
            
            // Use the agentic runtime to generate embeddings
            var result = await _agenticRuntime.InvokeAIFunctionAsync("embed", new object[] { text, aiOptions });
            
            var response = result?.ToString() ?? "Embedding generation failed: No response from AI service";
            
            // Try to deserialize as JSON to native CX object
            var cxObject = _jsonDeserializer.TryDeserializeFromResponse(response);
            
            _telemetryService?.TrackAiFunctionExecution("embed", text, stopwatch.Elapsed, result != null);
            return cxObject ?? response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing embed function");
            var errorMessage = $"Error: {ex.Message}";
            _telemetryService?.TrackAiFunctionExecution("embed", text, stopwatch.Elapsed, false, errorMessage);
            return errorMessage;
        }
    }

    /// <summary>
    /// Implements the 'adapt' AI function for self-optimization
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> AdaptAsync(object functionOrCode, object? options = null)
    {
        _logger.LogInformation("Executing adapt function");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Convert options to AI invocation options
            var aiOptions = ConvertOptions<AIInvocationOptions>(options);
            
            // Use the agentic runtime to adapt the function
            var result = await _agenticRuntime.InvokeAIFunctionAsync("adapt", new object[] { functionOrCode, aiOptions });
            
            var response = result?.ToString() ?? "Adaptation failed: No response from AI service";
            
            // Try to deserialize as JSON to native CX object
            var cxObject = _jsonDeserializer.TryDeserializeFromResponse(response);
            
            _telemetryService?.TrackAiFunctionExecution("adapt", functionOrCode?.ToString() ?? "unknown", stopwatch.Elapsed, result != null);
            return cxObject ?? response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing adapt function");
            var errorMessage = $"Error: {ex.Message}";
            _telemetryService?.TrackAiFunctionExecution("adapt", functionOrCode?.ToString() ?? "unknown", stopwatch.Elapsed, false, errorMessage);
            return errorMessage;
        }
    }

    /// <summary>
    /// Converts a generic options object to the specified type
    /// </summary>
    private T ConvertOptions<T>(object? options) where T : class, new()
    {
        if (options == null)
        {
            return new T(); // Return a default instance instead of null
        }

        try
        {
            if (options is Dictionary<string, object> optionsDict)
            {
                var result = new T();
                var properties = typeof(T).GetProperties();

                foreach (var property in properties)
                {
                    if (optionsDict.TryGetValue(property.Name, out var value) && value != null)
                    {
                        try
                        {
                            property.SetValue(result, Convert.ChangeType(value, property.PropertyType));
                        }
                        catch
                        {
                            // If conversion fails, try to handle common cases
                            if (property.PropertyType == typeof(string))
                            {
                                property.SetValue(result, value.ToString());
                            }
                        }
                    }
                }
                
                return result;
            }
            
            // If options is already the correct type, return it
            if (options is T typedOptions)
            {
                return typedOptions;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error converting options to type {Type}", typeof(T).Name);
        }
        
        return new T(); // Return default instance if conversion fails
    }
    
    // Synchronous wrapper methods for IL generation
    // These methods call the actual async methods using GetAwaiter().GetResult()
    // Updated to use live Azure OpenAI service instead of mock responses
    
    public object Task(string goal, object? options = null)
    {
        _logger.LogInformation("Executing synchronous Task function with goal: {Goal}", goal);
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = TaskAsync(goal, options).GetAwaiter().GetResult();
            _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, true);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Task wrapper");
            _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, ex.Message);
            return $"[AI Task] {goal} - Error: {ex.Message}";
        }
    }
    
    public object Reason(string problem, object? options = null)
    {
        _logger.LogInformation("Executing synchronous Reason function with problem: {Problem}", problem);
        try
        {
            return ReasonAsync(problem, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Reason wrapper");
            return $"[AI Reason] {problem} - Error: {ex.Message}";
        }
    }
    
    public object Synthesize(string specification, object? options = null)
    {
        _logger.LogInformation("Executing synchronous Synthesize function with specification: {Specification}", specification);
        try
        {
            return SynthesizeAsync(specification, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Synthesize wrapper");
            return $"[AI Synthesize] {specification} - Error: {ex.Message}";
        }
    }
    
    public object Process(string input, string context, object? options = null)
    {
        _logger.LogInformation("Executing synchronous Process function with input: {Input} and context: {Context}", input, context);
        try
        {
            return ProcessAsync(input, context, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Process wrapper");
            return $"[AI Process] {input} (Context: {context}) - Error: {ex.Message}";
        }
    }
    
    public object Generate(string prompt, object? options = null)
    {
        _logger.LogInformation("Executing synchronous Generate function with prompt: {Prompt}", prompt);
        try
        {
            return GenerateAsync(prompt, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Generate wrapper");
            return $"[AI Generate] {prompt} - Error: {ex.Message}";
        }
    }
    
    public object Embed(string text, object? options = null)
    {
        _logger.LogInformation("Executing synchronous Embed function with text: {Text}", text);
        try
        {
            return EmbedAsync(text, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Embed wrapper");
            return $"[AI Embed] {text} - Error: {ex.Message}";
        }
    }
    
    public object Adapt(string content, object? options = null)
    {
        _logger.LogInformation("Executing synchronous Adapt function with content: {Content}", content);
        try
        {
            return AdaptAsync(content, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Adapt wrapper");
            return $"[AI Adapt] {content} - Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Implements the 'speak' AI function using Azure Realtime API
    /// Converts text to speech and emits realtime events
    /// </summary>
    public System.Threading.Tasks.Task<object> SpeakAsync(string text, object? options = null)
    {
        _logger.LogInformation("Executing speak function with text: {Text}", text);
        
        try
        {
            // For the base implementation, return a placeholder
            // The adapter will override this with the real implementation
            var result = new 
            { 
                success = true, 
                text = text,
                message = "Voice synthesis function called (base implementation)"
            };
            return System.Threading.Tasks.Task.FromResult<object>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in speak function");
            var errorResult = new { error = ex.Message, text = text };
            return System.Threading.Tasks.Task.FromResult<object>(errorResult);
        }
    }

    /// <summary>
    /// Implements the 'listen' AI function using Azure Realtime API
    /// Sets up voice input listening and emits realtime events
    /// </summary>
    public System.Threading.Tasks.Task<object> ListenAsync(string prompt = "Listening for voice input...", object? options = null)
    {
        _logger.LogInformation("Executing listen function with prompt: {Prompt}", prompt);
        
        try
        {
            // For the base implementation, return a placeholder
            // The adapter will override this with the real implementation
            var result = new 
            { 
                success = true, 
                prompt = prompt,
                message = "Voice listening function called (base implementation)"
            };
            return System.Threading.Tasks.Task.FromResult<object>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in listen function");
            var errorResult = new { error = ex.Message, prompt = prompt };
            return System.Threading.Tasks.Task.FromResult<object>(errorResult);
        }
    }

    /// <summary>
    /// Synchronous wrapper for speak function
    /// </summary>
    public object Speak(string text, object? options = null)
    {
        _logger.LogInformation("Executing synchronous Speak function with text: {Text}", text);
        try
        {
            return SpeakAsync(text, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Speak wrapper");
            return $"[AI Speak] {text} - Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Synchronous wrapper for listen function
    /// </summary>
    public object Listen(string prompt = "Listening for voice input...", object? options = null)
    {
        _logger.LogInformation("Executing synchronous Listen function with prompt: {Prompt}", prompt);
        try
        {
            return ListenAsync(prompt, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in synchronous Listen wrapper");
            return $"[AI Listen] {prompt} - Error: {ex.Message}";
        }
    }
}
