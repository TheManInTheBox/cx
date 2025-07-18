using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CxLanguage.Core.AI;
using CxLanguage.Core.Telemetry;
using CxLanguage.Core.Serialization;
using CxLanguage.Parser;
using CxLanguage.Compiler;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Semantic Kernel-powered AI function handler for Cx language
/// Implements the 7 core AI functions using Semantic Kernel:
/// - task() - Task planning and execution
/// - synthesize() - Code generation
/// - reason() - Logical reasoning and decision making
/// - process() - Multi-modal data processing
/// - generate() - Content generation
/// - embed() - Vector embeddings
/// - adapt() - Self-optimization and adaptation
/// </summary>
public class SemanticKernelAiFunctions
{
    private readonly IAiService _aiService;
    private readonly ILogger<SemanticKernelAiFunctions> _logger;
    private readonly CxTelemetryService? _telemetryService;
    private readonly CxJsonDeserializer _jsonDeserializer;
    private readonly Dictionary<string, Assembly> _compiledAssemblies;

    public SemanticKernelAiFunctions(
        IAiService aiService, 
        ILogger<SemanticKernelAiFunctions> logger, 
        CxTelemetryService? telemetryService = null)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryService = telemetryService;
        _jsonDeserializer = new CxJsonDeserializer(logger as ILogger<CxJsonDeserializer>);
        _compiledAssemblies = new Dictionary<string, Assembly>();
    }

    /// <summary>
    /// Implements the 'task' AI function using Semantic Kernel
    /// Handles specific AI tasks like text-to-image, text-to-speech, transcription, etc.
    /// Always returns a native CX object (Dictionary<string, object>)
    /// </summary>
    public async Task<object> TaskAsync(string goal, object? options = null)
    {
        _logger.LogInformation("Executing task function with goal: {Goal}", goal);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Parse the goal to determine the task type
            var taskType = DetermineTaskType(goal);
            
            switch (taskType)
            {
                case "text-to-image":
                    return await HandleTextToImageTask(goal, options, stopwatch);
                
                case "text-to-speech":
                    return await HandleTextToSpeechTask(goal, options, stopwatch);
                
                case "image-to-text":
                    return await HandleImageToTextTask(goal, options, stopwatch);
                
                case "transcribe-audio":
                    return await HandleTranscribeAudioTask(goal, options, stopwatch);
                
                case "general-task":
                default:
                    return await HandleGeneralTask(goal, options, stopwatch);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing task function");
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "task",
                ["goal"] = goal,
                ["status"] = "error",
                ["error"] = ex.Message,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "task",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, ex.Message);
            return errorObject;
        }
    }

    /// <summary>
    /// Determines the type of task based on the goal string
    /// </summary>
    private string DetermineTaskType(string goal)
    {
        var goalLower = goal.ToLowerInvariant();
        
        if (goalLower.Contains("generate image") || goalLower.Contains("create image") || goalLower.Contains("paint") || goalLower.Contains("draw"))
            return "text-to-image";
        
        if (goalLower.Contains("speak text") || goalLower.Contains("text to speech") || goalLower.Contains("say"))
            return "text-to-speech";
        
        if (goalLower.Contains("describe image") || goalLower.Contains("analyze image") || goalLower.Contains("image to text"))
            return "image-to-text";
        
        if (goalLower.Contains("transcribe") || goalLower.Contains("audio to text") || goalLower.Contains("speech to text"))
            return "transcribe-audio";
        
        return "general-task";
    }

    /// <summary>
    /// Handles text-to-image generation tasks
    /// </summary>
    private async Task<object> HandleTextToImageTask(string goal, object? options, Stopwatch stopwatch)
    {
        _logger.LogInformation("Handling text-to-image task: {Goal}", goal);
        
        // Extract the image description from the goal
        var imagePrompt = ExtractImagePrompt(goal);
        
        try
        {
            // First, use the AI service to generate a detailed image description
            var enhancedPrompt = $"""
                Based on this request: "{imagePrompt}"
                
                Create a detailed, artistic image description that would be perfect for generating a high-quality image. Include:
                - Visual style and composition
                - Colors, lighting, and mood
                - Specific details and elements
                - Artistic techniques or photography style
                
                Return only the enhanced image description, no other text.
                """;

            var requestOptions = new AiRequestOptions
            {
                Temperature = 0.8,
                MaxTokens = 500,
                SystemPrompt = "You are an expert prompt engineer for AI image generation. Create detailed, vivid descriptions that produce stunning visual results."
            };

            var promptResult = await _aiService.GenerateTextAsync(enhancedPrompt, requestOptions);
            
            string finalPrompt = imagePrompt; // Default to original prompt
            
            if (promptResult.IsSuccess)
            {
                finalPrompt = promptResult.Content;
                _logger.LogInformation("Enhanced prompt generated: {EnhancedPrompt}", finalPrompt);
            }
            else
            {
                _logger.LogWarning("Prompt enhancement failed, using original prompt: {Error}", promptResult.ErrorMessage);
            }
            
            // Now generate the actual image using DALL-E 3
            var imageOptions = new CxLanguage.Core.AI.AiImageOptions
            {
                Model = "dall-e-3",
                Size = "1024x1024",
                Quality = "standard",
                Style = "vivid",
                ResponseFormat = "url"
            };
            
            var imageResult = await _aiService.GenerateImageAsync(finalPrompt, imageOptions);
            
            if (imageResult.IsSuccess)
            {
                var cxObject = new Dictionary<string, object>
                {
                    ["type"] = "task",
                    ["task_type"] = "text-to-image",
                    ["goal"] = goal,
                    ["original_prompt"] = imagePrompt,
                    ["enhanced_prompt"] = finalPrompt,
                    ["status"] = "completed",
                    ["result"] = $"Image generated successfully using DALL-E 3",
                    ["image_url"] = imageResult.ImageUrl ?? "",
                    ["revised_prompt"] = imageResult.RevisedPrompt ?? finalPrompt,
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "task",
                        ["task_type"] = "text-to-image",
                        ["ai_model"] = "DALL-E 3",
                        ["prompt_enhancement"] = promptResult.IsSuccess ? "GPT-4" : "none",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds,
                        ["tokens_used"] = promptResult.Usage?.TotalTokens ?? 0,
                        ["image_size"] = imageOptions.Size,
                        ["image_quality"] = imageOptions.Quality,
                        ["image_style"] = imageOptions.Style
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, true);
                return cxObject;
            }
            else
            {
                _logger.LogError("DALL-E 3 image generation failed: {Error}", imageResult.ErrorMessage);
                var errorObject = new Dictionary<string, object>
                {
                    ["type"] = "task",
                    ["task_type"] = "text-to-image",
                    ["goal"] = goal,
                    ["original_prompt"] = imagePrompt,
                    ["enhanced_prompt"] = finalPrompt,
                    ["status"] = "failed",
                    ["error"] = imageResult.ErrorMessage ?? "Unknown error",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "task",
                        ["task_type"] = "text-to-image",
                        ["ai_model"] = "DALL-E 3",
                        ["prompt_enhancement"] = promptResult.IsSuccess ? "GPT-4" : "none",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, imageResult.ErrorMessage);
                return errorObject;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in text-to-image task processing");
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "task",
                ["task_type"] = "text-to-image",
                ["goal"] = goal,
                ["original_prompt"] = imagePrompt,
                ["status"] = "error",
                ["error"] = ex.Message,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "task",
                    ["task_type"] = "text-to-image",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, ex.Message);
            return errorObject;
        }
    }

    /// <summary>
    /// Handles text-to-speech tasks
    /// </summary>
    private Task<object> HandleTextToSpeechTask(string goal, object? options, Stopwatch stopwatch)
    {
        _logger.LogInformation("Handling text-to-speech task: {Goal}", goal);
        
        // Extract the text to speak from the goal
        var textToSpeak = ExtractTextToSpeak(goal);
        
        // For now, simulate speech generation
        var speechResponse = $"""
            [SPEECH GENERATION SIMULATION]
            
            Text to Speak: {textToSpeak}
            
            This would generate high-quality speech audio using Azure Cognitive Services.
            
            Generated Speech Details:
            - Voice: Neural voice (natural sounding)
            - Language: Auto-detected
            - Format: MP3
            - Quality: High fidelity
            - Duration: Estimated based on text length
            - AI Model: Azure Neural Text-to-Speech
            
            [Note: This is a simulation. Real implementation would return actual audio data or URL]
            """;

        var cxObject = new Dictionary<string, object>
        {
            ["type"] = "task",
            ["task_type"] = "text-to-speech",
            ["goal"] = goal,
            ["text"] = textToSpeak,
            ["status"] = "completed",
            ["result"] = speechResponse,
            ["audio_data"] = "[Base64 encoded audio data would be here]",
            ["metadata"] = new Dictionary<string, object>
            {
                ["function"] = "task",
                ["task_type"] = "text-to-speech",
                ["ai_model"] = "Azure Neural TTS",
                ["voice"] = "neural",
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
            }
        };
        
        _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, true);
        return System.Threading.Tasks.Task.FromResult<object>(cxObject);
    }

    /// <summary>
    /// Handles image-to-text tasks
    /// </summary>
    private Task<object> HandleImageToTextTask(string goal, object? options, Stopwatch stopwatch)
    {
        _logger.LogInformation("Handling image-to-text task: {Goal}", goal);
        
        // Extract the image description task from the goal
        var imageContext = ExtractImageContext(goal);
        
        // For now, simulate image analysis
        var analysisResponse = $"""
            [IMAGE ANALYSIS SIMULATION]
            
            Image Analysis Request: {imageContext}
            
            This would analyze the provided image using Azure Computer Vision.
            
            Generated Description:
            "The image shows a detailed scene with various elements. The AI vision model 
            would provide a comprehensive description of objects, people, scenery, colors, 
            composition, and other visual elements present in the image."
            
            Analysis Details:
            - Objects Detected: [List of detected objects]
            - Scene Type: [Landscape/Portrait/Indoor/Outdoor]
            - Dominant Colors: [Color palette]
            - Confidence: High
            - AI Model: Azure Computer Vision
            
            [Note: This is a simulation. Real implementation would analyze actual image data]
            """;

        var cxObject = new Dictionary<string, object>
        {
            ["type"] = "task",
            ["task_type"] = "image-to-text",
            ["goal"] = goal,
            ["image_context"] = imageContext,
            ["status"] = "completed",
            ["result"] = analysisResponse,
            ["description"] = "Detailed image description would be here",
            ["metadata"] = new Dictionary<string, object>
            {
                ["function"] = "task",
                ["task_type"] = "image-to-text",
                ["ai_model"] = "Azure Computer Vision",
                ["confidence"] = "high",
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
            }
        };
        
        _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, true);
        return System.Threading.Tasks.Task.FromResult<object>(cxObject);
    }

    /// <summary>
    /// Handles audio transcription tasks
    /// </summary>
    private Task<object> HandleTranscribeAudioTask(string goal, object? options, Stopwatch stopwatch)
    {
        _logger.LogInformation("Handling transcribe audio task: {Goal}", goal);
        
        // Extract the audio file info from the goal
        var audioContext = ExtractAudioContext(goal);
        
        // For now, simulate audio transcription
        var transcriptionResponse = $"""
            [AUDIO TRANSCRIPTION SIMULATION]
            
            Audio Transcription Request: {audioContext}
            
            This would transcribe the provided audio using Azure Speech Services.
            
            Generated Transcript:
            "This is a sample transcription of the audio content. The AI speech recognition 
            model would convert the spoken words into accurate text with proper punctuation 
            and formatting."
            
            Transcription Details:
            - Language: Auto-detected
            - Confidence: High
            - Duration: [Audio length]
            - Speaker Count: [Number of speakers]
            - AI Model: Azure Speech-to-Text
            
            [Note: This is a simulation. Real implementation would transcribe actual audio data]
            """;

        var cxObject = new Dictionary<string, object>
        {
            ["type"] = "task",
            ["task_type"] = "transcribe-audio",
            ["goal"] = goal,
            ["audio_context"] = audioContext,
            ["status"] = "completed",
            ["result"] = transcriptionResponse,
            ["transcript"] = "Sample transcript would be here",
            ["metadata"] = new Dictionary<string, object>
            {
                ["function"] = "task",
                ["task_type"] = "transcribe-audio",
                ["ai_model"] = "Azure Speech-to-Text",
                ["confidence"] = "high",
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
            }
        };
        
        _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, true);
        return System.Threading.Tasks.Task.FromResult<object>(cxObject);
    }

    /// <summary>
    /// Handles general task planning and execution
    /// </summary>
    private async Task<object> HandleGeneralTask(string goal, object? options, Stopwatch stopwatch)
    {
        _logger.LogInformation("Handling general task: {Goal}", goal);
        
        // Create a task-focused prompt
        var prompt = $"""
            You are a helpful AI assistant that excels at task planning and execution.
            
            Task: {goal}
            
            Please break down this task into clear, actionable steps and provide a comprehensive response.
            If the task involves code generation, provide working code examples.
            If the task involves analysis, provide detailed insights.
            
            Respond in a clear, structured format.
            """;

        var requestOptions = new AiRequestOptions
        {
            Temperature = 0.7,
            MaxTokens = 2000,
            SystemPrompt = "You are an expert task planner and executor."
        };

        var result = await _aiService.GenerateTextAsync(prompt, requestOptions);
        
        if (result.IsSuccess)
        {
            // Always return a native CX object
            var cxObject = new Dictionary<string, object>
            {
                ["type"] = "task",
                ["task_type"] = "general-task",
                ["goal"] = goal,
                ["status"] = "completed",
                ["result"] = result.Content,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "task",
                    ["task_type"] = "general-task",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, true);
            return cxObject;
        }
        else
        {
            _logger.LogWarning("Task execution failed: {Error}", result.ErrorMessage);
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "task",
                ["task_type"] = "general-task",
                ["goal"] = goal,
                ["status"] = "failed",
                ["error"] = result.ErrorMessage ?? "Unknown error",
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "task",
                    ["task_type"] = "general-task",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, result.ErrorMessage);
            return errorObject;
        }
    }

    /// <summary>
    /// Extracts the image prompt from a text-to-image goal
    /// </summary>
    private string ExtractImagePrompt(string goal)
    {
        var goalLower = goal.ToLowerInvariant();
        
        // Try to extract after "generate image:", "create image:", etc.
        var patterns = new[] { "generate image:", "create image:", "paint:", "draw:" };
        
        foreach (var pattern in patterns)
        {
            var index = goalLower.IndexOf(pattern);
            if (index >= 0)
            {
                return goal.Substring(index + pattern.Length).Trim();
            }
        }
        
        return goal; // Return the full goal if no pattern found
    }

    /// <summary>
    /// Extracts the text to speak from a text-to-speech goal
    /// </summary>
    private string ExtractTextToSpeak(string goal)
    {
        var goalLower = goal.ToLowerInvariant();
        
        // Try to extract after "speak text:", "say:", etc.
        var patterns = new[] { "speak text:", "say:", "text to speech:" };
        
        foreach (var pattern in patterns)
        {
            var index = goalLower.IndexOf(pattern);
            if (index >= 0)
            {
                return goal.Substring(index + pattern.Length).Trim();
            }
        }
        
        return goal; // Return the full goal if no pattern found
    }

    /// <summary>
    /// Extracts the image context from an image-to-text goal
    /// </summary>
    private string ExtractImageContext(string goal)
    {
        var goalLower = goal.ToLowerInvariant();
        
        // Try to extract after "describe image:", "analyze image:", etc.
        var patterns = new[] { "describe image:", "analyze image:", "image to text:" };
        
        foreach (var pattern in patterns)
        {
            var index = goalLower.IndexOf(pattern);
            if (index >= 0)
            {
                return goal.Substring(index + pattern.Length).Trim();
            }
        }
        
        return goal; // Return the full goal if no pattern found
    }

    /// <summary>
    /// Extracts the audio context from a transcribe audio goal
    /// </summary>
    private string ExtractAudioContext(string goal)
    {
        var goalLower = goal.ToLowerInvariant();
        
        // Try to extract after "transcribe:", "audio to text:", etc.
        var patterns = new[] { "transcribe:", "transcribe audio:", "audio to text:", "speech to text:" };
        
        foreach (var pattern in patterns)
        {
            var index = goalLower.IndexOf(pattern);
            if (index >= 0)
            {
                return goal.Substring(index + pattern.Length).Trim();
            }
        }
        
        return goal; // Return the full goal if no pattern found
    }

    /// <summary>
    /// Implements the 'synthesize' AI function for code generation using Semantic Kernel
    /// Always returns a native CX object (Dictionary<string, object>)
    /// </summary>
    public async Task<object> SynthesizeAsync(string specification, object? options = null)
    {
        _logger.LogInformation("Executing synthesize function with specification: {Spec}", specification);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var prompt = $"""
                You are an expert code generator and software synthesizer.
                
                Specification: {specification}
                
                Please generate clean, working code that meets the specification.
                Include proper error handling, comments, and follow best practices.
                If the specification is ambiguous, make reasonable assumptions and document them.
                
                Provide the complete, functional code solution.
                """;

            var requestOptions = new AiRequestOptions
            {
                Temperature = 0.3, // Lower temperature for more consistent code generation
                MaxTokens = 3000,
                SystemPrompt = "You are a skilled software developer and code synthesizer."
            };

            var result = await _aiService.GenerateTextAsync(prompt, requestOptions);
            
            if (result.IsSuccess)
            {
                // Always return a native CX object
                var cxObject = new Dictionary<string, object>
                {
                    ["type"] = "synthesize",
                    ["specification"] = specification,
                    ["status"] = "completed",
                    ["result"] = result.Content,
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "synthesize",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("synthesize", specification, stopwatch.Elapsed, true);
                return cxObject;
            }
            else
            {
                _logger.LogWarning("Code synthesis failed: {Error}", result.ErrorMessage);
                var errorObject = new Dictionary<string, object>
                {
                    ["type"] = "synthesize",
                    ["specification"] = specification,
                    ["status"] = "failed",
                    ["error"] = result.ErrorMessage ?? "Unknown error",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "synthesize",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("synthesize", specification, stopwatch.Elapsed, false, result.ErrorMessage);
                return errorObject;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing synthesize function");
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "synthesize",
                ["specification"] = specification,
                ["status"] = "error",
                ["error"] = ex.Message,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "synthesize",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("synthesize", specification, stopwatch.Elapsed, false, ex.Message);
            return errorObject;
        }
    }

    /// <summary>
    /// Implements the 'reason' AI function for logical reasoning using Semantic Kernel
    /// Always returns a native CX object (Dictionary<string, object>)
    /// </summary>
    public async Task<object> ReasonAsync(string question, object? options = null)
    {
        _logger.LogInformation("Executing reason function with question: {Question}", question);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var prompt = $"""
                You are an expert logical reasoner and critical thinker.
                
                Question/Problem: {question}
                
                Please provide a thorough analysis using logical reasoning:
                1. Break down the problem into components
                2. Identify key assumptions and constraints
                3. Apply logical principles and evidence
                4. Consider multiple perspectives
                5. Provide a well-reasoned conclusion
                
                Structure your response clearly with your reasoning process.
                """;

            var requestOptions = new AiRequestOptions
            {
                Temperature = 0.5,
                MaxTokens = 2500,
                SystemPrompt = "You are a logical reasoning expert with strong analytical skills."
            };

            var result = await _aiService.GenerateTextAsync(prompt, requestOptions);
            
            if (result.IsSuccess)
            {
                // Always return a native CX object
                var cxObject = new Dictionary<string, object>
                {
                    ["type"] = "reason",
                    ["question"] = question,
                    ["status"] = "completed",
                    ["result"] = result.Content,
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "reason",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("reason", question, stopwatch.Elapsed, true);
                return cxObject;
            }
            else
            {
                _logger.LogWarning("Reasoning failed: {Error}", result.ErrorMessage);
                var errorObject = new Dictionary<string, object>
                {
                    ["type"] = "reason",
                    ["question"] = question,
                    ["status"] = "failed",
                    ["error"] = result.ErrorMessage ?? "Unknown error",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "reason",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("reason", question, stopwatch.Elapsed, false, result.ErrorMessage);
                return errorObject;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing reason function");
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "reason",
                ["question"] = question,
                ["status"] = "error",
                ["error"] = ex.Message,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "reason",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("reason", question, stopwatch.Elapsed, false, ex.Message);
            return errorObject;
        }
    }

    /// <summary>
    /// Implements the 'process' AI function for multi-modal data processing using Semantic Kernel
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> ProcessAsync(string input, string context, object? options = null)
    {
        _logger.LogInformation("Executing process function with input: {Input}, context: {Context}", input, context);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var prompt = $"""
                You are an expert data processor capable of handling various types of input.
                
                Input: {input}
                Context: {context}
                
                Please process this input in the given context:
                1. Analyze the input format and content
                2. Apply appropriate processing techniques
                3. Consider the context for relevant transformations
                4. Provide structured output
                
                Deliver a comprehensive processed result.
                """;

            var requestOptions = new AiRequestOptions
            {
                Temperature = 0.4,
                MaxTokens = 2000,
                SystemPrompt = "You are a versatile data processor with expertise in multiple domains."
            };

            var result = await _aiService.GenerateTextAsync(prompt, requestOptions);
            
            if (result.IsSuccess)
            {
                // Always return a native CX object
                var cxObject = new Dictionary<string, object>
                {
                    ["type"] = "process",
                    ["input"] = input,
                    ["context"] = context,
                    ["status"] = "completed",
                    ["result"] = result.Content,
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "process",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("process", input, stopwatch.Elapsed, true);
                return cxObject;
            }
            else
            {
                _logger.LogWarning("Processing failed: {Error}", result.ErrorMessage);
                var errorObject = new Dictionary<string, object>
                {
                    ["type"] = "process",
                    ["input"] = input,
                    ["context"] = context,
                    ["status"] = "failed",
                    ["error"] = result.ErrorMessage ?? "Unknown error",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "process",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("process", input, stopwatch.Elapsed, false, result.ErrorMessage);
                return errorObject;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing process function");
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "process",
                ["input"] = input,
                ["context"] = context,
                ["status"] = "error",
                ["error"] = ex.Message,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "process",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("process", input, stopwatch.Elapsed, false, ex.Message);
            return errorObject;
        }
    }

    /// <summary>
    /// Implements the 'generate' AI function for content generation using Semantic Kernel
    /// Always returns a native CX object (Dictionary<string, object>)
    /// </summary>
    public async Task<object> GenerateAsync(string prompt, object? options = null)
    {
        _logger.LogInformation("Executing generate function with prompt: {Prompt}", prompt);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var enhancedPrompt = $"""
                You are a creative and skilled content generator.
                
                Generation Request: {prompt}
                
                Please create high-quality content that fulfills this request:
                1. Understand the intent and requirements
                2. Generate relevant, engaging content
                3. Ensure accuracy and coherence
                4. Follow appropriate style and format
                
                Provide the complete generated content.
                """;

            var requestOptions = new AiRequestOptions
            {
                Temperature = 0.8, // Higher temperature for more creative generation
                MaxTokens = 3000,
                SystemPrompt = "You are a versatile content generator with strong creative abilities."
            };

            var result = await _aiService.GenerateTextAsync(enhancedPrompt, requestOptions);
            
            if (result.IsSuccess)
            {
                // Always return a native CX object
                var cxObject = new Dictionary<string, object>
                {
                    ["type"] = "generate",
                    ["prompt"] = prompt,
                    ["status"] = "completed",
                    ["result"] = result.Content,
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "generate",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("generate", prompt, stopwatch.Elapsed, true);
                return cxObject;
            }
            else
            {
                _logger.LogWarning("Content generation failed: {Error}", result.ErrorMessage);
                var errorObject = new Dictionary<string, object>
                {
                    ["type"] = "generate",
                    ["prompt"] = prompt,
                    ["status"] = "failed",
                    ["error"] = result.ErrorMessage ?? "Unknown error",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "generate",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("generate", prompt, stopwatch.Elapsed, false, result.ErrorMessage);
                return errorObject;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing generate function");
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "generate",
                ["prompt"] = prompt,
                ["status"] = "error",
                ["error"] = ex.Message,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "generate",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("generate", prompt, stopwatch.Elapsed, false, ex.Message);
            return errorObject;
        }
    }

    /// <summary>
    /// Implements the 'embed' AI function for vector embeddings using Semantic Kernel
    /// Returns either a native CX object (if response contains JSON) or a string
    /// </summary>
    public async Task<object> EmbedAsync(string text, object? options = null)
    {
        _logger.LogInformation("Executing embed function with text: {Text}", text);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // For now, we'll use the AI service to provide embedding-related functionality
            // In a full implementation, this would connect to embedding services
            var prompt = $"""
                You are processing text for vector embedding analysis.
                
                Text: {text}
                
                Please analyze this text and provide:
                1. Key semantic features and concepts
                2. Important keywords and phrases
                3. Contextual meaning and themes
                4. Structural analysis
                
                This analysis will be used for similarity matching and semantic search.
                """;

            var requestOptions = new AiRequestOptions
            {
                Temperature = 0.3,
                MaxTokens = 1500,
                SystemPrompt = "You are an expert in text analysis and semantic understanding."
            };

            var result = await _aiService.GenerateTextAsync(prompt, requestOptions);
            
            if (result.IsSuccess)
            {
                // Always return a native CX object
                var cxObject = new Dictionary<string, object>
                {
                    ["type"] = "embed",
                    ["text"] = text,
                    ["status"] = "completed",
                    ["result"] = result.Content,
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "embed",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("embed", text, stopwatch.Elapsed, true);
                return cxObject;
            }
            else
            {
                _logger.LogWarning("Embedding analysis failed: {Error}", result.ErrorMessage);
                var errorObject = new Dictionary<string, object>
                {
                    ["type"] = "embed",
                    ["text"] = text,
                    ["status"] = "failed",
                    ["error"] = result.ErrorMessage ?? "Unknown error",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "embed",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("embed", text, stopwatch.Elapsed, false, result.ErrorMessage);
                return errorObject;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing embed function");
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "embed",
                ["text"] = text,
                ["status"] = "error",
                ["error"] = ex.Message,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "embed",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("embed", text, stopwatch.Elapsed, false, ex.Message);
            return errorObject;
        }
    }

    /// <summary>
    /// Implements the 'adapt' AI function for self-optimization using Semantic Kernel
    /// Generates CX code, compiles it, and makes it available to the current runtime
    /// Returns a native CX object with compilation and execution results
    /// </summary>
    public async Task<object> AdaptAsync(string content, object? options = null)
    {
        _logger.LogInformation("Executing adapt function with content: {Content}", content);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var prompt = $@"
Generate CX language code for the following request: {content}

CX Language Syntax Rules:
- Use 'function' keyword for function declarations: function functionName() {{ }}
- Use Allman-style braces (opening brace on new line)
- Variables declared with 'var' keyword: var variableName = value;
- Data types: strings (""text""), numbers (42, 3.14), booleans (true, false), null
- Control flow: if/else, while loops, for-in loops: for (item in collection) {{ }}
- AI functions: task(""prompt""), synthesize(""content""), reason(""problem""), process(""input""), generate(""spec""), embed(""text""), adapt(""request"")
- Use print() for output
- All statements end with semicolons
- Use double quotes for strings
- Function parameters are untyped: function name(param1, param2) {{ }}
- No type annotations or declarations
- Example valid CX code:
  function factorial(n)
  {{
      var result = 1;
      var i = 1;
      while (i <= n)
      {{
          result = result * i;
          i = i + 1;
      }}
      return result;
  }}

Instructions:
1. Return ONLY valid CX language code that follows the syntax above
2. Use proper CX syntax with 'function' keyword, Allman braces, and semicolons
3. Use native AI functions when appropriate for the request
4. Generate complete, runnable code that matches CX grammar
5. Don't include any explanatory text, just the code
6. Make sure all functions have proper opening/closing braces
7. Use 'var' for variable declarations
8. No type annotations - parameters and variables are untyped
9. Use while loops instead of for loops with counters
10. For arrays, use for-in loops: for (item in array) {{ }}

Request: {content}
";

            var requestOptions = new AiRequestOptions
            {
                Temperature = 0.4, // Lower temperature for more consistent code generation
                MaxTokens = 3000,
                SystemPrompt = "You are a CX language expert. Generate only valid CX code without any markdown or explanations."
            };

            var result = await _aiService.GenerateTextAsync(prompt, requestOptions);
            
            if (result.IsSuccess)
            {
                var generatedCode = result.Content.Trim();
                
                // Remove any markdown code blocks if present
                if (generatedCode.StartsWith("```"))
                {
                    var lines = generatedCode.Split('\n');
                    var codeLines = new List<string>();
                    bool inCodeBlock = false;
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("```"))
                        {
                            inCodeBlock = !inCodeBlock;
                            continue;
                        }
                        if (inCodeBlock)
                        {
                            codeLines.Add(line);
                        }
                    }
                    generatedCode = string.Join("\n", codeLines);
                }
                
                // Create a temporary file name for the generated code
                var tempFileName = $"adapted_code_{DateTime.UtcNow.Ticks}";
                
                // Attempt to compile and inject the generated CX code into current runtime
                var compilationResult = await CompileAndInjectCxCodeAsync(generatedCode, tempFileName);
                
                var cxObject = new Dictionary<string, object>
                {
                    ["type"] = "adapt",
                    ["content"] = content,
                    ["status"] = compilationResult.IsSuccess ? "completed" : "compilationFailed",
                    ["generatedCode"] = generatedCode,
                    ["generatedCodeLength"] = generatedCode.Length,
                    ["compilationSuccess"] = compilationResult.IsSuccess,
                    ["compilationError"] = compilationResult.ErrorMessage ?? "",
                    ["assemblyName"] = tempFileName,
                    ["hasProgramType"] = compilationResult.ProgramType != null,
                    ["injectedFunctions"] = compilationResult.InjectedFunctions ?? new List<string>(),
                    ["result"] = compilationResult.IsSuccess ? 
                        $"CX code successfully generated, compiled, and {compilationResult.InjectedFunctions?.Count ?? 0} function(s) injected into runtime" : 
                        $"Code generation successful but compilation failed: {compilationResult.ErrorMessage}",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["executionTimeMs"] = stopwatch.ElapsedMilliseconds,
                    ["codeLength"] = generatedCode.Length,
                    ["compilationSuccessful"] = compilationResult.IsSuccess,
                    ["runtimeInjection"] = compilationResult.IsSuccess
                };
                
                _telemetryService?.TrackAiFunctionExecution("adapt", content, stopwatch.Elapsed, true);
                return cxObject;
            }
            else
            {
                _logger.LogWarning("Code generation failed: {Error}", result.ErrorMessage);
                var errorObject = new Dictionary<string, object>
                {
                    ["type"] = "adapt",
                    ["content"] = content,
                    ["status"] = "failed",
                    ["error"] = result.ErrorMessage ?? "Unknown error",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "adapt",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("adapt", content, stopwatch.Elapsed, false, result.ErrorMessage);
                return errorObject;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing adapt function");
            var errorObject = new Dictionary<string, object>
            {
                ["type"] = "adapt",
                ["content"] = content,
                ["status"] = "error",
                ["error"] = ex.Message,
                ["metadata"] = new Dictionary<string, object>
                {
                    ["function"] = "adapt",
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                }
            };
            
            _telemetryService?.TrackAiFunctionExecution("adapt", content, stopwatch.Elapsed, false, ex.Message);
            return errorObject;
        }
    }

    /// <summary>
    /// Compiles CX code and injects it into the current runtime context
    /// </summary>
    private System.Threading.Tasks.Task<CompilationResult> CompileAndInjectCxCodeAsync(string cxCode, string tempFileName)
    {
        try
        {
            _logger.LogInformation("Compiling and injecting generated CX code: {Code}", cxCode.Substring(0, Math.Min(200, cxCode.Length)) + "...");
            
            // Parse the CX code
            var parseResult = CxLanguageParser.Parse(cxCode, tempFileName);
            
            if (!parseResult.IsSuccess || parseResult.Value == null)
            {
                var errorMessage = parseResult.Errors.Count > 0 
                    ? string.Join(", ", parseResult.Errors.Select(e => e.Message))
                    : "Unknown parse error";
                
                return System.Threading.Tasks.Task.FromResult(CompilationResult.Failure($"Failed to parse generated CX code: {errorMessage}"));
            }
            
            // Compile the CX code
            var compiler = new CxCompiler(tempFileName, new CompilerOptions(), _aiService, this);
            var compilationResult = compiler.Compile(parseResult.Value!, tempFileName, cxCode);
            
            if (!compilationResult.IsSuccess)
            {
                return System.Threading.Tasks.Task.FromResult(CompilationResult.Failure($"Failed to compile generated CX code: {compilationResult.ErrorMessage}"));
            }
            
            // Store the compiled assembly for potential future use
            if (compilationResult.Assembly != null)
            {
                _compiledAssemblies[tempFileName] = compilationResult.Assembly;
            }
            
            // RUNTIME INJECTION: Extract functions from generated assembly and make them available
            var injectedFunctions = new List<string>();
            var programType = compilationResult.ProgramType;
            if (programType != null && compilationResult.Assembly != null)
            {
                // Get all methods from the generated program type
                var methods = programType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                
                foreach (var method in methods)
                {
                    // Skip constructor, Run method, and system methods
                    if (method.Name != "Run" && method.Name != ".ctor" && 
                        !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") &&
                        method.DeclaringType == programType)
                    {
                        injectedFunctions.Add(method.Name);
                        _logger.LogInformation("Injected function into runtime: {FunctionName}", method.Name);
                    }
                }
                
                // Create a runtime delegate registry to make functions callable
                // This is a simplified approach - in a full implementation, we'd need to
                // integrate with the IL compiler's function table
                RuntimeFunctionRegistry.RegisterAssembly(tempFileName, compilationResult.Assembly, programType);
            }
            
            // Return enhanced compilation result with injection info
            return System.Threading.Tasks.Task.FromResult(CompilationResult.SuccessWithInjections(
                compilationResult.Assembly!, 
                compilationResult.ProgramType!, 
                injectedFunctions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling and injecting CX code");
            return System.Threading.Tasks.Task.FromResult(CompilationResult.Failure(ex.Message));
        }
    }

    /// <summary>
    /// Compiles CX code and makes it available to the current runtime
    /// </summary>
    private async Task<CompilationResult> CompileCxCodeAsync(string cxCode, string tempFileName)
    {
        try
        {
            _logger.LogInformation("Compiling generated CX code: {Code}", cxCode.Substring(0, Math.Min(200, cxCode.Length)) + "...");
            
            // Parse the CX code
            var parseResult = CxLanguageParser.Parse(cxCode, tempFileName);
            
            if (!parseResult.IsSuccess || parseResult.Value == null)
            {
                var errorMessage = parseResult.Errors.Count > 0 
                    ? string.Join(", ", parseResult.Errors.Select(e => e.Message))
                    : "Unknown parse error";
                
                return CompilationResult.Failure($"Failed to parse generated CX code: {errorMessage}");
            }
            
            // Compile the CX code
            var compiler = new CxCompiler(tempFileName, new CompilerOptions(), _aiService, this);
            var compilationResult = compiler.Compile(parseResult.Value!, tempFileName, cxCode);
            
            if (!compilationResult.IsSuccess)
            {
                return CompilationResult.Failure($"Failed to compile generated CX code: {compilationResult.ErrorMessage}");
            }
            
            // Store the compiled assembly for potential future use
            if (compilationResult.Assembly != null)
            {
                _compiledAssemblies[tempFileName] = compilationResult.Assembly;
            }
            
            // Execute the compiled code to make it available to the runtime
            var programType = compilationResult.ProgramType;
            if (programType != null)
            {
                // Create an instance of the program with required dependencies
                var program = Activator.CreateInstance(programType, new object(), _aiService, this);
                
                // Try to execute the Run method if it exists
                var runMethod = programType.GetMethod("Run");
                if (runMethod != null)
                {
                    await System.Threading.Tasks.Task.Run(() => runMethod.Invoke(program, new object[] { }));
                }
            }
            
            return compilationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling CX code");
            return CompilationResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Synchronous wrapper for task function
    /// </summary>
    public object Task(string goal, object? options = null)
    {
        return TaskAsync(goal, options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for synthesize function
    /// </summary>
    public object Synthesize(string specification, object? options = null)
    {
        return SynthesizeAsync(specification, options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for reason function
    /// </summary>
    public object Reason(string question, object? options = null)
    {
        return ReasonAsync(question, options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for process function
    /// </summary>
    public object Process(string input, string context, object? options = null)
    {
        return ProcessAsync(input, context, options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for generate function
    /// </summary>
    public object Generate(string prompt, object? options = null)
    {
        return GenerateAsync(prompt, options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for embed function
    /// </summary>
    public object Embed(string text, object? options = null)
    {
        return EmbedAsync(text, options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous wrapper for adapt function
    /// </summary>
    public object Adapt(string content, object? options = null)
    {
        return AdaptAsync(content, options).GetAwaiter().GetResult();
    }
}
