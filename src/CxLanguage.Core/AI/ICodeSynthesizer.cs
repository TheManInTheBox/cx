using CxLanguage.Core.Ast;
using CxLanguage.Core.Types;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Core.AI;

/// <summary>
/// Dynamic code synthesis capabilities for runtime adaptation
/// </summary>
public interface ICodeSynthesizer
{
    Task<CodeSynthesisResult> SynthesizeFunctionAsync(string specification, CodeSynthesisOptions? options = null);
    Task<CodeSynthesisResult> SynthesizeModuleAsync(string specification, CodeSynthesisOptions? options = null);
    Task<CodeSynthesisResult> SynthesizeClassAsync(string specification, CodeSynthesisOptions? options = null);
    Task<bool> AdaptCodePathAsync(string path, object context, AdaptationOptions? options = null);
}

/// <summary>
/// Result of code synthesis
/// </summary>
public class CodeSynthesisResult
{
    public bool IsSuccess { get; init; }
    public string? GeneratedCode { get; init; }
    public AstNode? Ast { get; init; }
    public Type? CompiledType { get; init; }
    public MethodInfo? CompiledMethod { get; init; }
    public string? ErrorMessage { get; init; }
    public CodeMetrics? Metrics { get; init; }

    public static CodeSynthesisResult Success(string code, AstNode? ast = null, 
        Type? type = null, MethodInfo? method = null, CodeMetrics? metrics = null) =>
        new() 
        { 
            IsSuccess = true, 
            GeneratedCode = code, 
            Ast = ast, 
            CompiledType = type, 
            CompiledMethod = method,
            Metrics = metrics 
        };

    public static CodeSynthesisResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Options for code synthesis
/// </summary>
public class CodeSynthesisOptions
{
    public string? TargetLanguage { get; set; } = "cx";
    public string? Model { get; set; }
    public double Temperature { get; set; } = 0.3; // Lower for more deterministic code
    public bool CompileImmediately { get; set; } = true;
    public bool ValidateSyntax { get; set; } = true;
    public bool RunTests { get; set; } = false;
    public List<string> Dependencies { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Options for code path adaptation
/// </summary>
public class AdaptationOptions
{
    public string? Strategy { get; set; } = "performance"; // performance, accuracy, safety
    public double ConfidenceThreshold { get; set; } = 0.8;
    public bool BackupOriginal { get; set; } = true;
    public TimeSpan MaxAdaptationTime { get; set; } = TimeSpan.FromMinutes(2);
}

/// <summary>
/// Metrics for generated code
/// </summary>
public class CodeMetrics
{
    public int LinesOfCode { get; set; }
    public int CyclomaticComplexity { get; set; }
    public TimeSpan GenerationTime { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> QualityChecks { get; set; } = new();
}

/// <summary>
/// Runtime code synthesizer implementation
/// </summary>
public class RuntimeCodeSynthesizer : ICodeSynthesizer
{
    private readonly IAiService _aiService;
    private readonly ICodeGenerator _codeGenerator;
    private readonly ILogger<RuntimeCodeSynthesizer> _logger;

    public RuntimeCodeSynthesizer(
        IAiService aiService,
        ICodeGenerator codeGenerator,
        ILogger<RuntimeCodeSynthesizer> logger)
    {
        _aiService = aiService;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    public async Task<CodeSynthesisResult> SynthesizeFunctionAsync(
        string specification, 
        CodeSynthesisOptions? options = null)
    {
        options ??= new CodeSynthesisOptions();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Synthesizing function from specification: {Spec}", 
                specification.Substring(0, Math.Min(100, specification.Length)));

            // Generate code directly using AI service
            var codePrompt = CreateCodeGenerationPrompt(specification, options);
            
            var response = await _aiService.GenerateTextAsync(codePrompt, new AiRequestOptions
            {
                Model = options.Model,
                Temperature = options.Temperature,
                MaxTokens = 2000,
                SystemPrompt = GetCodeGenerationSystemPrompt()
            });

            if (!response.IsSuccess)
            {
                return CodeSynthesisResult.Failure($"Failed to generate code: {response.ErrorMessage}");
            }

            // Extract the generated code from the response
            var generatedCode = ExtractCodeFromResponse(response.Content);
            if (string.IsNullOrEmpty(generatedCode))
            {
                return CodeSynthesisResult.Failure("No valid code was generated");
            }

            var metrics = new CodeMetrics
            {
                LinesOfCode = generatedCode.Split('\n').Length,
                GenerationTime = DateTime.UtcNow - startTime,
                ConfidenceScore = 0.85 // TODO: Calculate actual confidence
            };

            // Parse and compile if requested
            AstNode? ast = null;
            MethodInfo? compiledMethod = null;
            
            if (options.CompileImmediately)
            {
                var compileResult = await _codeGenerator.CompileCodeAsync(generatedCode);
                if (compileResult.IsSuccess)
                {
                    ast = compileResult.Ast;
                    compiledMethod = compileResult.Method;
                }
            }

            return CodeSynthesisResult.Success(generatedCode, ast, null, compiledMethod, metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing function");
            return CodeSynthesisResult.Failure(ex.Message);
        }
    }

    public async Task<CodeSynthesisResult> SynthesizeModuleAsync(
        string specification, 
        CodeSynthesisOptions? options = null)
    {
        // Similar implementation to SynthesizeFunctionAsync but for modules
        return await SynthesizeFunctionAsync($"Create module: {specification}", options);
    }

    public async Task<CodeSynthesisResult> SynthesizeClassAsync(
        string specification, 
        CodeSynthesisOptions? options = null)
    {
        // Similar implementation to SynthesizeFunctionAsync but for classes
        return await SynthesizeFunctionAsync($"Create class: {specification}", options);
    }

    public async Task<bool> AdaptCodePathAsync(
        string path, 
        object context, 
        AdaptationOptions? options = null)
    {
        options ??= new AdaptationOptions();

        try
        {
            _logger.LogInformation("Adapting code path: {Path} with strategy: {Strategy}", 
                path, options.Strategy);

            // Analyze current performance/behavior using AI service directly
            var analysisPrompt = $@"
Analyze and suggest adaptations for the code path '{path}' with the following context:
{context}

Strategy: {options.Strategy}

Please provide specific recommendations for code improvements, optimizations, or adaptations.
";
            
            var response = await _aiService.GenerateTextAsync(analysisPrompt, new AiRequestOptions
            {
                Temperature = 0.3,
                MaxTokens = 1500
            });

            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to generate adaptation suggestions: {Error}", response.ErrorMessage);
                return false;
            }

            _logger.LogInformation("Code adaptation suggestions generated: {Suggestions}", 
                response.Content.Substring(0, Math.Min(200, response.Content.Length)));
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adapting code path");
            return false;
        }
    }

    private string CreateCodeGenerationPrompt(string specification, CodeSynthesisOptions options)
    {
        return $@"
Generate {options.TargetLanguage} code based on the following specification:

{specification}

Requirements:
- Target language: {options.TargetLanguage}
- Follow best practices for {options.TargetLanguage}
- Include appropriate error handling
- Write clean, readable code
- Add helpful comments

Please provide only the code implementation without additional explanations.
";
    }

    private string GetCodeGenerationSystemPrompt()
    {
        return @"
You are an expert code generator specializing in multiple programming languages.
Your goal is to generate high-quality, production-ready code based on specifications.

Guidelines:
- Write clean, maintainable code
- Follow language-specific best practices
- Include appropriate error handling
- Use meaningful variable and function names
- Add necessary imports/using statements
- Ensure code is ready to compile and run

Always respond with code only, wrapped in appropriate code block markers.
";
    }

    private string ExtractCodeFromResponse(string response)
    {
        // Extract code from markdown code blocks
        var codeBlockStart = response.IndexOf("```");
        if (codeBlockStart == -1) return response.Trim();

        var languageEnd = response.IndexOf('\n', codeBlockStart);
        if (languageEnd == -1) return response.Trim();

        var codeStart = languageEnd + 1;
        var codeBlockEnd = response.IndexOf("```", codeStart);
        
        if (codeBlockEnd == -1) 
        {
            return response.Substring(codeStart).Trim();
        }

        return response.Substring(codeStart, codeBlockEnd - codeStart).Trim();
    }
}

/// <summary>
/// Code generator interface
/// </summary>
public interface ICodeGenerator
{
    Task<CompileResult> CompileCodeAsync(string code);
}

/// <summary>
/// Compilation result
/// </summary>
public class CompileResult
{
    public bool IsSuccess { get; init; }
    public AstNode? Ast { get; init; }
    public MethodInfo? Method { get; init; }
    public string? ErrorMessage { get; init; }

    public static CompileResult Success(AstNode? ast = null, MethodInfo? method = null) =>
        new() { IsSuccess = true, Ast = ast, Method = method };

    public static CompileResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}
