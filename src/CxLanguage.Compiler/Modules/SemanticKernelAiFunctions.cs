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
    /// Always returns a native CX object (Dictionary<string, object>)
    /// </summary>
    public async Task<object> TaskAsync(string goal, object? options = null)
    {
        _logger.LogInformation("Executing task function with goal: {Goal}", goal);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
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
                    ["goal"] = goal,
                    ["status"] = "completed",
                    ["result"] = result.Content,
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "task",
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
                    ["goal"] = goal,
                    ["status"] = "failed",
                    ["error"] = result.ErrorMessage ?? "Unknown error",
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["function"] = "task",
                        ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        ["execution_time_ms"] = stopwatch.ElapsedMilliseconds
                    }
                };
                
                _telemetryService?.TrackAiFunctionExecution("task", goal, stopwatch.Elapsed, false, result.ErrorMessage);
                return errorObject;
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
