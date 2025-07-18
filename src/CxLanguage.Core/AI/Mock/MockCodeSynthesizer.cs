using CxLanguage.Core.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Core.AI.Mock;

/// <summary>
/// Mock implementation of ICodeSynthesizer for testing and development
/// </summary>
public class MockCodeSynthesizer : ICodeSynthesizer
{
    private readonly ILogger<MockCodeSynthesizer> _logger;

    public MockCodeSynthesizer(ILogger<MockCodeSynthesizer> logger)
    {
        _logger = logger;
    }

    public async Task<CodeSynthesisResult> SynthesizeFunctionAsync(string specification, CodeSynthesisOptions? options = null)
    {
        _logger.LogInformation("Mock function synthesis for specification: {Specification}", specification);
        
        options ??= new CodeSynthesisOptions();
        
        var generatedCode = $@"// Generated function for: {specification}
// Language: {options.TargetLanguage ?? "cx"}

function generatedFunction()
{{
    // Mock implementation for: {specification}
    return ""Mock function synthesis result"";
}}";

        await Task.CompletedTask;
        return CodeSynthesisResult.Success(generatedCode, null, null, null, new CodeMetrics
        {
            LinesOfCode = 5,
            CyclomaticComplexity = 1,
            GenerationTime = TimeSpan.FromMilliseconds(100),
            ConfidenceScore = 0.95,
            QualityChecks = new List<string> { "Mock quality check passed" }
        });
    }

    public async Task<CodeSynthesisResult> SynthesizeModuleAsync(string specification, CodeSynthesisOptions? options = null)
    {
        _logger.LogInformation("Mock module synthesis for specification: {Specification}", specification);
        
        options ??= new CodeSynthesisOptions();
        
        var generatedCode = $@"// Generated module for: {specification}
// Language: {options.TargetLanguage ?? "cx"}

// Module: {specification}
var moduleData = ""Mock module data"";

function moduleFunction()
{{
    return moduleData;
}}";

        await Task.CompletedTask;
        return CodeSynthesisResult.Success(generatedCode, null, null, null, new CodeMetrics
        {
            LinesOfCode = 8,
            CyclomaticComplexity = 1,
            GenerationTime = TimeSpan.FromMilliseconds(150),
            ConfidenceScore = 0.90,
            QualityChecks = new List<string> { "Mock module quality check passed" }
        });
    }

    public async Task<CodeSynthesisResult> SynthesizeClassAsync(string specification, CodeSynthesisOptions? options = null)
    {
        _logger.LogInformation("Mock class synthesis for specification: {Specification}", specification);
        
        options ??= new CodeSynthesisOptions();
        
        var generatedCode = $@"// Generated class for: {specification}
// Language: {options.TargetLanguage ?? "cx"}

class GeneratedClass
{{
    var property;
    
    constructor(value)
    {{
        this.property = value;
    }}
    
    function method()
    {{
        return this.property;
    }}
}}";

        await Task.CompletedTask;
        return CodeSynthesisResult.Success(generatedCode, null, null, null, new CodeMetrics
        {
            LinesOfCode = 12,
            CyclomaticComplexity = 2,
            GenerationTime = TimeSpan.FromMilliseconds(200),
            ConfidenceScore = 0.88,
            QualityChecks = new List<string> { "Mock class quality check passed" }
        });
    }

    public async Task<bool> AdaptCodePathAsync(string path, object context, AdaptationOptions? options = null)
    {
        _logger.LogInformation("Mock code path adaptation for path: {Path}", path);
        
        options ??= new AdaptationOptions();
        
        // Mock adaptation always succeeds
        await Task.Delay(50); // Simulate processing time
        
        return true;
    }
}
