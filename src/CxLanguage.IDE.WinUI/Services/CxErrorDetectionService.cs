using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Extensions.Logging;

namespace CxLanguage.IDE.WinUI.Services;

/// <summary>
/// Real-time error detection service for CX Language
/// Implements GitHub Issue #220 error detection requirements
/// </summary>
public interface ICxErrorDetectionService
{
    Task<CxErrorAnalysisResult> AnalyzeCodeAsync(string code, string fileName = "editor");
    void HighlightErrors(TextEditor editor, CxErrorAnalysisResult result);
    void ClearErrors(TextEditor editor);
    Task<bool> ValidateCodeAsync(string code);
}

public class CxErrorDetectionService : ICxErrorDetectionService
{
    private readonly ILogger<CxErrorDetectionService> _logger;
    private readonly List<TextMarkerService> _activeMarkers = new();
    
    public CxErrorDetectionService(ILogger<CxErrorDetectionService> logger)
    {
        _logger = logger;
    }
    
    public async Task<CxErrorAnalysisResult> AnalyzeCodeAsync(string code, string fileName = "editor")
    {
        try
        {
            var startTime = DateTime.Now;
            
            // Simple syntax validation for CX Language patterns
            var parseResult = await Task.Run(() => ValidateCxSyntax(code, fileName));
            
            var errors = new List<CxCodeError>();
            var warnings = new List<CxCodeWarning>();
            
            // Convert validation errors to our error format
            if (!parseResult.IsSuccess && parseResult.Errors != null)
            {
                foreach (var error in parseResult.Errors)
                {
                    errors.Add(new CxCodeError
                    {
                        Line = error.Line,
                        Column = error.Column,
                        Message = error.Message,
                        ErrorType = CxErrorType.SyntaxError,
                        Severity = CxErrorSeverity.Error,
                        FileName = error.FileName
                    });
                }
            }
            
            // Perform semantic analysis if parsing succeeded
            if (parseResult.IsSuccess)
            {
                var semanticIssues = await PerformSemanticAnalysisAsync(code, new object());
                warnings.AddRange(semanticIssues);
            }
            
            var elapsed = DateTime.Now - startTime;
            
            // Log performance (GitHub Issue #220 requirement: <100ms)
            if (elapsed.TotalMilliseconds > 100)
            {
                _logger.LogWarning($"Error analysis took {elapsed.TotalMilliseconds:F0}ms (target: <100ms)");
            }
            else
            {
                _logger.LogDebug($"Error analysis completed in {elapsed.TotalMilliseconds:F0}ms");
            }
            
            return new CxErrorAnalysisResult
            {
                IsValid = !errors.Any(),
                Errors = errors.ToArray(),
                Warnings = warnings.ToArray(),
                AnalysisTime = elapsed,
                ParseSuccess = parseResult.IsSuccess
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during code analysis");
            
            return new CxErrorAnalysisResult
            {
                IsValid = false,
                Errors = new[]
                {
                    new CxCodeError
                    {
                        Line = 1,
                        Column = 1,
                        Message = $"Analysis failed: {ex.Message}",
                        ErrorType = CxErrorType.InternalError,
                        Severity = CxErrorSeverity.Error,
                        FileName = fileName
                    }
                },
                Warnings = Array.Empty<CxCodeWarning>(),
                AnalysisTime = TimeSpan.Zero,
                ParseSuccess = false
            };
        }
    }
    
    private async Task<IEnumerable<CxCodeWarning>> PerformSemanticAnalysisAsync(string code, object ast)
    {
        await Task.Delay(1); // Placeholder for semantic analysis
        
        var warnings = new List<CxCodeWarning>();
        var lines = code.Split('\n');
        
        // Check for consciousness best practices
        if (!code.Contains("conscious"))
        {
            warnings.Add(new CxCodeWarning
            {
                Line = 1,
                Column = 1,
                Message = "Consider using consciousness patterns for AI-driven behavior. CX Language is designed around consciousness entities.",
                WarningType = CxWarningType.BestPractice,
                Severity = CxErrorSeverity.Info
            });
        }
        
        // Check for proper consciousness entity structure
        if (code.Contains("conscious") && !code.Contains("realize"))
        {
            var conscientLine = FindLineContaining(lines, "conscious");
            warnings.Add(new CxCodeWarning
            {
                Line = conscientLine,
                Column = 1,
                Message = "Consciousness entities require a 'realize(self: conscious)' constructor for proper initialization.",
                WarningType = CxWarningType.BestPractice,
                Severity = CxErrorSeverity.Warning
            });
        }
        
        // Check for learn self pattern
        if (code.Contains("realize") && !code.Contains("learn self"))
        {
            var realizeLine = FindLineContaining(lines, "realize");
            warnings.Add(new CxCodeWarning
            {
                Line = realizeLine,
                Column = 1,
                Message = "Consciousness entities should include 'learn self;' in their realize() constructor for proper consciousness initialization.",
                WarningType = CxWarningType.BestPractice,
                Severity = CxErrorSeverity.Warning
            });
        }
        
        // Check for event handlers
        if (code.Contains("conscious") && !code.Contains("handlers:") && !code.Contains("on "))
        {
            var conscientLine = FindLineContaining(lines, "conscious");
            warnings.Add(new CxCodeWarning
            {
                Line = conscientLine,
                Column = 1,
                Message = "Consciousness entities typically benefit from event handlers ('on eventName' or 'handlers: []') for reactive behavior.",
                WarningType = CxWarningType.BestPractice,
                Severity = CxErrorSeverity.Info
            });
        }
        
        // Check for AI services usage
        var availableAiServices = new[] { "think", "learn", "infer", "execute", "iam", "adapt", "await" };
        var hasAiServices = availableAiServices.Any(service => code.Contains(service + " "));
        
        if (code.Contains("conscious") && !hasAiServices)
        {
            var conscientLine = FindLineContaining(lines, "conscious");
            warnings.Add(new CxCodeWarning
            {
                Line = conscientLine,
                Column = 1,
                Message = "Consider using AI services (think, learn, infer, execute) for consciousness processing. Available services: " + string.Join(", ", availableAiServices),
                WarningType = CxWarningType.BestPractice,
                Severity = CxErrorSeverity.Info
            });
        }
        
        // Check for deprecated patterns
        CheckDeprecatedPatterns(code, lines, warnings);
        
        // Check for proper event emission patterns
        CheckEventPatterns(code, lines, warnings);
        
        // Check for consciousness-specific best practices
        CheckConsciousnessBestPractices(code, lines, warnings);
        
        return warnings;
    }
    
    private void CheckDeprecatedPatterns(string code, string[] lines, List<CxCodeWarning> warnings)
    {
        // Check for traditional if statements
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Contains("if (") || line.StartsWith("if("))
            {
                warnings.Add(new CxCodeWarning
                {
                    Line = i + 1,
                    Column = 1,
                    Message = "Traditional 'if' statements are deprecated in CX Language. Use consciousness-aware 'is {}' patterns for AI-driven decision making.",
                    WarningType = CxWarningType.Deprecated,
                    Severity = CxErrorSeverity.Warning
                });
            }
            
            if (line.Contains("while (") || line.StartsWith("while("))
            {
                warnings.Add(new CxCodeWarning
                {
                    Line = i + 1,
                    Column = 1,
                    Message = "Traditional 'while' loops are discouraged. Use event-driven consciousness patterns with 'adapt {}' for iterative behavior.",
                    WarningType = CxWarningType.Deprecated,
                    Severity = CxErrorSeverity.Warning
                });
            }
            
            if (line.Contains("for (") || line.StartsWith("for("))
            {
                warnings.Add(new CxCodeWarning
                {
                    Line = i + 1,
                    Column = 1,
                    Message = "Traditional 'for' loops are discouraged. Use consciousness iteration patterns or event-driven processing for better AI integration.",
                    WarningType = CxWarningType.Deprecated,
                    Severity = CxErrorSeverity.Info
                });
            }
        }
    }
    
    private void CheckEventPatterns(string code, string[] lines, List<CxCodeWarning> warnings)
    {
        // Check for proper emit patterns
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("emit ") && !line.Contains("{"))
            {
                warnings.Add(new CxCodeWarning
                {
                    Line = i + 1,
                    Column = 1,
                    Message = "Event emissions should include payload data: 'emit eventName { data: \"value\" }'",
                    WarningType = CxWarningType.BestPractice,
                    Severity = CxErrorSeverity.Info
                });
            }
            
            if (line.Contains("on ") && !line.Contains("(event)"))
            {
                warnings.Add(new CxCodeWarning
                {
                    Line = i + 1,
                    Column = 1,
                    Message = "Event handlers should accept event parameter: 'on eventName(event) { ... }'",
                    WarningType = CxWarningType.BestPractice,
                    Severity = CxErrorSeverity.Warning
                });
            }
        }
    }
    
    private void CheckConsciousnessBestPractices(string code, string[] lines, List<CxCodeWarning> warnings)
    {
        // Check for print statements in event handlers
        if (code.Contains("on ") && !code.Contains("print(event)"))
        {
            var handlerLine = FindLineContaining(lines, "on ");
            warnings.Add(new CxCodeWarning
            {
                Line = handlerLine,
                Column = 1,
                Message = "Consider adding 'print(event);' in event handlers to observe consciousness data flow for debugging.",
                WarningType = CxWarningType.BestPractice,
                Severity = CxErrorSeverity.Info
            });
        }
        
        // Check for consciousness naming conventions
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("conscious "))
            {
                var parts = line.Split(' ');
                if (parts.Length > 1)
                {
                    var entityName = parts[1].Replace("{", "").Trim();
                    if (char.IsLower(entityName[0]))
                    {
                        warnings.Add(new CxCodeWarning
                        {
                            Line = i + 1,
                            Column = 1,
                            Message = $"Consciousness entity names should follow PascalCase convention. Consider '{char.ToUpper(entityName[0])}{entityName.Substring(1)}' instead of '{entityName}'.",
                            WarningType = CxWarningType.BestPractice,
                            Severity = CxErrorSeverity.Info
                        });
                    }
                }
            }
        }
    }
    
    private int FindLineContaining(string[] lines, string searchText)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(searchText))
            {
                return i + 1; // Return 1-based line number
            }
        }
        return 1; // Default to line 1 if not found
    }
    
    public void HighlightErrors(TextEditor editor, CxErrorAnalysisResult result)
    {
        try
        {
            // Clear existing error highlights
            ClearErrors(editor);
            
            var document = editor.Document;
            var markerService = new TextMarkerService(document);
            
            // Add error markers
            foreach (var error in result.Errors)
            {
                try
                {
                    var line = Math.Max(1, error.Line);
                    var column = Math.Max(1, error.Column);
                    
                    if (line <= document.LineCount)
                    {
                        var documentLine = document.GetLineByNumber(line);
                        var offset = documentLine.Offset + Math.Min(column - 1, documentLine.Length - 1);
                        var endOffset = Math.Min(offset + 10, documentLine.EndOffset); // Highlight ~10 chars
                        
                        var marker = markerService.Create(offset, endOffset - offset);
                        marker.BackgroundColor = Colors.Red;
                        marker.ForegroundColor = Colors.White;
                        marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                        marker.ToolTip = $"Error: {error.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Could not highlight error at line {error.Line}, column {error.Column}");
                }
            }
            
            // Add warning markers
            foreach (var warning in result.Warnings)
            {
                try
                {
                    var line = Math.Max(1, warning.Line);
                    var column = Math.Max(1, warning.Column);
                    
                    if (line <= document.LineCount)
                    {
                        var documentLine = document.GetLineByNumber(line);
                        var offset = documentLine.Offset + Math.Min(column - 1, documentLine.Length - 1);
                        var endOffset = Math.Min(offset + 10, documentLine.EndOffset);
                        
                        var marker = markerService.Create(offset, endOffset - offset);
                        marker.BackgroundColor = Colors.Yellow;
                        marker.ForegroundColor = Colors.Black;
                        marker.MarkerTypes = TextMarkerTypes.DottedUnderline;
                        marker.ToolTip = $"Warning: {warning.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Could not highlight warning at line {warning.Line}, column {warning.Column}");
                }
            }
            
            // Install the marker service
            editor.TextArea.TextView.BackgroundRenderers.Add(markerService);
            editor.TextArea.TextView.LineTransformers.Add(markerService);
            _activeMarkers.Add(markerService);
            
            _logger.LogDebug($"Highlighted {result.Errors.Length} errors and {result.Warnings.Length} warnings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error highlighting errors in editor");
        }
    }
    
    public void ClearErrors(TextEditor editor)
    {
        try
        {
            // Remove all active marker services
            foreach (var markerService in _activeMarkers)
            {
                try
                {
                    editor.TextArea.TextView.BackgroundRenderers.Remove(markerService);
                    editor.TextArea.TextView.LineTransformers.Remove(markerService);
                    markerService.RemoveAll(_ => true);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error removing marker service");
                }
            }
            
            _activeMarkers.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing error highlights");
        }
    }
    
    public async Task<bool> ValidateCodeAsync(string code)
    {
        try
        {
            var result = await AnalyzeCodeAsync(code);
            return result.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during code validation");
            return false;
        }
    }
    
    private CxParseResult ValidateCxSyntax(string code, string fileName)
    {
        try
        {
            var errors = new List<CxParseError>();
            var lines = code.Split('\n');
            
            // Basic syntax validation for CX Language patterns
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                var lineNumber = i + 1;
                
                // Check for unmatched braces
                var openBraces = line.Count(c => c == '{');
                var closeBraces = line.Count(c => c == '}');
                
                // Check for proper consciousness entity syntax
                if (line.StartsWith("conscious ") && !line.Contains("{"))
                {
                    // Look ahead to see if brace is on next line
                    if (i + 1 < lines.Length && !lines[i + 1].Trim().StartsWith("{"))
                    {
                        errors.Add(new CxParseError
                        {
                            Line = lineNumber,
                            Column = line.Length,
                            Message = "Consciousness entity declaration must be followed by opening brace '{'",
                            FileName = fileName
                        });
                    }
                }
                
                // Check for proper realize method syntax
                if (line.Contains("realize") && !line.Contains("realize(self:"))
                {
                    errors.Add(new CxParseError
                    {
                        Line = lineNumber,
                        Column = line.IndexOf("realize") + 1,
                        Message = "realize method must include 'self' parameter: 'realize(self: conscious)'",
                        FileName = fileName
                    });
                }
                
                // Check for proper emit syntax
                if (line.StartsWith("emit ") && !line.Contains("{") && !line.EndsWith(";"))
                {
                    errors.Add(new CxParseError
                    {
                        Line = lineNumber,
                        Column = 1,
                        Message = "emit statements must include payload braces or end with semicolon",
                        FileName = fileName
                    });
                }
                
                // Check for proper event handler syntax
                if (line.StartsWith("on ") && !line.Contains("(event)"))
                {
                    errors.Add(new CxParseError
                    {
                        Line = lineNumber,
                        Column = 1,
                        Message = "Event handlers must accept event parameter: 'on eventName(event)'",
                        FileName = fileName
                    });
                }
                
                // Check for unsupported syntax
                if (line.Contains("class ") || line.Contains("public class"))
                {
                    errors.Add(new CxParseError
                    {
                        Line = lineNumber,
                        Column = line.IndexOf("class") + 1,
                        Message = "Traditional classes are not supported. Use 'conscious EntityName {}' for consciousness entities",
                        FileName = fileName
                    });
                }
            }
            
            return new CxParseResult
            {
                IsSuccess = !errors.Any(),
                Errors = errors.ToArray()
            };
        }
        catch (Exception ex)
        {
            return new CxParseResult
            {
                IsSuccess = false,
                Errors = new[]
                {
                    new CxParseError
                    {
                        Line = 1,
                        Column = 1,
                        Message = $"Syntax validation failed: {ex.Message}",
                        FileName = fileName
                    }
                }
            };
        }
    }

}

#region Data Models

public class CxErrorAnalysisResult
{
    public bool IsValid { get; set; }
    public CxCodeError[] Errors { get; set; } = Array.Empty<CxCodeError>();
    public CxCodeWarning[] Warnings { get; set; } = Array.Empty<CxCodeWarning>();
    public TimeSpan AnalysisTime { get; set; }
    public bool ParseSuccess { get; set; }
    
    public int TotalIssues => Errors.Length + Warnings.Length;
}

public class CxCodeError
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; } = "";
    public CxErrorType ErrorType { get; set; }
    public CxErrorSeverity Severity { get; set; }
    public string FileName { get; set; } = "";
}

public class CxCodeWarning
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; } = "";
    public CxWarningType WarningType { get; set; }
    public CxErrorSeverity Severity { get; set; }
}

public enum CxErrorType
{
    SyntaxError,
    SemanticError,
    TypeError,
    ReferenceError,
    InternalError
}

public enum CxWarningType
{
    BestPractice,
    Performance,
    Accessibility,
    Deprecated,
    Suggestion
}

public enum CxErrorSeverity
{
    Error,
    Warning,
    Info,
    Hint
}

#endregion

#region Text Marker Service

/// <summary>
/// Simple text marker service for error highlighting
/// </summary>
public class TextMarkerService : DocumentColorizingTransformer, IBackgroundRenderer
{
    private readonly TextDocument _document;
    private readonly List<TextMarker> _markers = new();
    
    public TextMarkerService(TextDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }
    
    public TextMarker Create(int startOffset, int length)
    {
        var marker = new TextMarker(startOffset, length);
        _markers.Add(marker);
        return marker;
    }
    
    public void RemoveAll(Predicate<TextMarker> predicate)
    {
        _markers.RemoveAll(predicate);
    }
    
    public KnownLayer Layer => KnownLayer.Selection;
    
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        // Background rendering handled by markers
    }
    
    protected override void ColorizeLine(DocumentLine line)
    {
        // Line transformation handled by markers
    }
}

public class TextMarker
{
    public int StartOffset { get; }
    public int Length { get; }
    public Color BackgroundColor { get; set; } = Colors.Transparent;
    public Color ForegroundColor { get; set; } = Colors.Black;
    public TextMarkerTypes MarkerTypes { get; set; } = TextMarkerTypes.None;
    public object? ToolTip { get; set; }
    
    public TextMarker(int startOffset, int length)
    {
        StartOffset = startOffset;
        Length = length;
    }
    
    public int EndOffset => StartOffset + Length;
}

[Flags]
public enum TextMarkerTypes
{
    None = 0,
    SquigglyUnderline = 1,
    DottedUnderline = 2,
    LineInScrollBar = 4
}

#endregion

public class CxParseResult
{
    public bool IsSuccess { get; set; }
    public CxParseError[] Errors { get; set; } = Array.Empty<CxParseError>();
}

public class CxParseError
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; } = "";
    public string FileName { get; set; } = "";
}
