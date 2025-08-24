using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CxLanguage.IDE.Windows;

/// <summary>
/// Auto-completion provider for CX Language consciousness patterns and AI services
/// Implements GitHub Issue #220 IntelliSense requirements
/// </summary>
public interface ICxAutoCompleteProvider
{
    IEnumerable<string> GetSuggestions(string lineText, int position);
    IEnumerable<string> GetConsciousnessPatterns();
    IEnumerable<string> GetAiServiceSuggestions();
    IEnumerable<string> GetEventPatterns();
}

public class CxAutoCompleteProvider : ICxAutoCompleteProvider
{
    private readonly ILogger<CxAutoCompleteProvider> _logger;
    
    // CX Language consciousness patterns
    private static readonly string[] ConsciousnessKeywords = {
        "conscious", "realize", "when", "adapt", "iam", "handlers", "emit"
    };
    
    // AI Service patterns
    private static readonly string[] AiServices = {
        "think", "learn", "infer", "search", "execute", "await"
    };
    
    // Common event patterns
    private static readonly string[] CommonEventPatterns = {
        "calculate.request", "calculation.complete", "result.display",
        "analysis.start", "analysis.complete", "data.processed",
        "user.input", "system.ready", "error.occurred",
        "task.start", "task.complete", "task.failed"
    };
    
    // Consciousness entity templates
    private static readonly Dictionary<string, string> ConsciousnessTemplates = new()
    {
        ["conscious"] = @"conscious entityName {
    realize(self: conscious) {
        learn self;
    }

    handlers: [
        event.name
    ]

    on event.name (event) {
        // Handle event
    }
}",
        ["realize"] = @"realize(self: conscious) {
    learn self;
}",
        ["handlers"] = @"handlers: [
    event.name,
    another.event
]",
        ["on"] = @"on event.name (event) {
    // Handle event
}",
        ["emit"] = @"emit event.name {
    data: ""value"",
    handlers: [ target.event ]
};",
        ["when"] = @"when {
    condition: ""expression"",
    action: ""response""
}",
        ["adapt"] = @"adapt {
    context: ""learning context"",
    focus: ""improvement area"",
    data: {
        currentCapabilities: [],
        targetCapabilities: [],
        learningObjective: """"
    },
    handlers: [ adaptation.complete ]
}",
        ["iam"] = @"iam {
    identity: ""role description"",
    capabilities: [],
    handlers: [ identity.verified ]
}"
    };
    
    // AI service templates
    private static readonly Dictionary<string, string> AiServiceTemplates = new()
    {
        ["think"] = @"think {
    data: ""input data"",
    handlers: [ thought.complete ]
}",
        ["learn"] = @"learn {
    data: ""learning material"",
    handlers: [ knowledge.acquired ]
}",
        ["infer"] = @"infer {
    data: ""input for inference"",
    handlers: [ inference.complete ]
}",
        ["search"] = @"search {
    query: ""search terms"",
    handlers: [ search.results ]
}",
        ["execute"] = @"execute {
    command: ""action to perform"",
    handlers: [ execution.complete ]
}",
        ["await"] = @"await {
    reason: ""waiting for condition"",
    minDurationMs: 1000,
    maxDurationMs: 5000,
    handlers: [ wait.complete ]
}"
    };
    
    public CxAutoCompleteProvider(ILogger<CxAutoCompleteProvider> logger)
    {
        _logger = logger;
    }
    
    public IEnumerable<string> GetSuggestions(string lineText, int position)
    {
        if (string.IsNullOrWhiteSpace(lineText))
            return Enumerable.Empty<string>();
        
        var suggestions = new List<string>();
        var currentWord = GetCurrentWord(lineText, position);
        var context = GetContext(lineText);
        
        try
        {
            // Context-aware suggestions
            switch (context)
            {
                case AutoCompleteContext.ConsciousDeclaration:
                    suggestions.AddRange(GetConsciousnessContextSuggestions(currentWord));
                    break;
                    
                case AutoCompleteContext.EventHandler:
                    suggestions.AddRange(GetEventHandlerSuggestions(currentWord));
                    break;
                    
                case AutoCompleteContext.AiService:
                    suggestions.AddRange(GetAiServiceContextSuggestions(currentWord));
                    break;
                    
                case AutoCompleteContext.HandlersArray:
                    suggestions.AddRange(GetHandlersArraySuggestions(currentWord));
                    break;
                    
                case AutoCompleteContext.EmitStatement:
                    suggestions.AddRange(GetEmitStatementSuggestions(currentWord));
                    break;
                    
                default:
                    // General suggestions
                    suggestions.AddRange(GetGeneralSuggestions(currentWord));
                    break;
            }
            
            // Filter suggestions based on current word
            if (!string.IsNullOrEmpty(currentWord))
            {
                suggestions = suggestions
                    .Where(s => s.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            _logger.LogDebug($"Generated {suggestions.Count} suggestions for context: {context}, word: '{currentWord}'");
            return suggestions.Distinct().OrderBy(s => s);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating auto-completion suggestions for line: {lineText}");
            return Enumerable.Empty<string>();
        }
    }
    
    private string GetCurrentWord(string lineText, int position)
    {
        if (string.IsNullOrEmpty(lineText) || position <= 0)
            return "";
        
        var start = position - 1;
        while (start >= 0 && (char.IsLetterOrDigit(lineText[start]) || lineText[start] == '.' || lineText[start] == '_'))
        {
            start--;
        }
        start++;
        
        var end = position;
        while (end < lineText.Length && (char.IsLetterOrDigit(lineText[end]) || lineText[end] == '.' || lineText[end] == '_'))
        {
            end++;
        }
        
        return start < end ? lineText.Substring(start, end - start) : "";
    }
    
    private AutoCompleteContext GetContext(string lineText)
    {
        var trimmed = lineText.Trim();
        
        if (trimmed.Contains("conscious ") && trimmed.Contains("{"))
            return AutoCompleteContext.ConsciousDeclaration;
            
        if (trimmed.StartsWith("on ") || trimmed.Contains(" on "))
            return AutoCompleteContext.EventHandler;
            
        if (trimmed.StartsWith("emit ") || trimmed.Contains(" emit "))
            return AutoCompleteContext.EmitStatement;
            
        if (trimmed.Contains("handlers:") || trimmed.Contains("handlers: ["))
            return AutoCompleteContext.HandlersArray;
            
        if (AiServices.Any(service => trimmed.Contains(service + " ")))
            return AutoCompleteContext.AiService;
            
        return AutoCompleteContext.General;
    }
    
    private IEnumerable<string> GetConsciousnessContextSuggestions(string currentWord)
    {
        yield return "realize(self: conscious) { learn self; }";
        yield return "handlers: [ event.name ]";
        
        foreach (var template in ConsciousnessTemplates.Where(t => t.Key != "conscious"))
        {
            yield return template.Key;
        }
    }
    
    private IEnumerable<string> GetEventHandlerSuggestions(string currentWord)
    {
        foreach (var eventPattern in CommonEventPatterns)
        {
            yield return eventPattern;
        }
        
        yield return "async";
        yield return "(event)";
    }
    
    private IEnumerable<string> GetAiServiceContextSuggestions(string currentWord)
    {
        foreach (var template in AiServiceTemplates)
        {
            yield return template.Value;
        }
        
        yield return "data: \"\"";
        yield return "handlers: [ event.name ]";
    }
    
    private IEnumerable<string> GetHandlersArraySuggestions(string currentWord)
    {
        foreach (var eventPattern in CommonEventPatterns)
        {
            yield return eventPattern;
        }
        
        yield return "event.name { option: \"value\" }";
    }
    
    private IEnumerable<string> GetEmitStatementSuggestions(string currentWord)
    {
        foreach (var eventPattern in CommonEventPatterns)
        {
            yield return eventPattern;
        }
        
        yield return "{ data: \"value\", handlers: [ target.event ] }";
    }
    
    private IEnumerable<string> GetGeneralSuggestions(string currentWord)
    {
        // Consciousness keywords
        foreach (var keyword in ConsciousnessKeywords)
        {
            yield return keyword;
        }
        
        // AI services
        foreach (var service in AiServices)
        {
            yield return service;
        }
        
        // Common patterns
        yield return "var name = value;";
        yield return "print(value);";
    }
    
    public IEnumerable<string> GetConsciousnessPatterns()
    {
        return ConsciousnessTemplates.Values;
    }
    
    public IEnumerable<string> GetAiServiceSuggestions()
    {
        return AiServiceTemplates.Values;
    }
    
    public IEnumerable<string> GetEventPatterns()
    {
        return CommonEventPatterns;
    }
}

public enum AutoCompleteContext
{
    General,
    ConsciousDeclaration,
    EventHandler,
    AiService,
    HandlersArray,
    EmitStatement
}
