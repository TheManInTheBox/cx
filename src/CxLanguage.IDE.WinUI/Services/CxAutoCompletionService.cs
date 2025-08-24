using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.Extensions.Logging;

namespace CxLanguage.IDE.WinUI.Services;

/// <summary>
/// CX Language auto-completion service for consciousness patterns and AI services
/// Implements GitHub Issue #220 auto-completion requirements
/// </summary>
public interface ICxAutoCompletionService
{
    Task<CompletionWindow?> ShowCompletionAsync(TextEditor editor, int offset);
    IEnumerable<CxCompletionData> GetCompletions(string contextCode, int position);
    void RegisterCompletionProvider(TextEditor editor);
}

public class CxAutoCompletionService : ICxAutoCompletionService
{
    private readonly ILogger<CxAutoCompletionService> _logger;
    
    private readonly Dictionary<string, CxCompletionCategory> _completionCategories;
    
    public CxAutoCompletionService(ILogger<CxAutoCompletionService> logger)
    {
        _logger = logger;
        _completionCategories = InitializeCompletionCategories();
    }
    
    private Dictionary<string, CxCompletionCategory> InitializeCompletionCategories()
    {
        return new Dictionary<string, CxCompletionCategory>
        {
            ["consciousness"] = new CxCompletionCategory
            {
                Name = "Consciousness Patterns",
                Icon = "🧠",
                Items = new[]
                {
                    new CxCompletionData("conscious", "conscious EntityName {\n    realize(self: conscious) {\n        learn self;\n        $0\n    }\n}", "Create a consciousness entity"),
                    new CxCompletionData("realize", "realize(self: conscious) {\n    learn self;\n    $0\n}", "Consciousness entity constructor"),
                    new CxCompletionData("when", "when { condition: $0 } {\n    \n}", "Event-driven conditional logic"),
                    new CxCompletionData("adapt", "adapt {\n    context: \"$0\"\n    focus: \"better assist Aura\"\n    data: {\n        currentCapabilities: []\n        targetCapabilities: []\n        learningObjective: \"\"\n    }\n    handlers: []\n}", "Consciousness adaptation pattern"),
                    new CxCompletionData("iam", "iam {\n    condition: \"$0\"\n    verification: \"\"\n    handlers: []\n}", "Self-reflective identity verification"),
                    new CxCompletionData("print", "print($0);", "Output data to console"),
                    new CxCompletionData("learn self", "learn self;", "Initialize consciousness entity"),
                    new CxCompletionData("conscious entity template", "conscious $0 {\n    realize(self: conscious) {\n        learn self;\n        \n        // Initialize consciousness\n        emit entity.initialized {\n            name: \"$0\"\n            timestamp: new Date()\n        };\n    }\n    \n    on entity.initialized(event) {\n        print(event);\n    }\n}", "Complete consciousness entity template")
                }
            },
            ["ai_services"] = new CxCompletionCategory
            {
                Name = "AI Services",
                Icon = "🤖", 
                Items = new[]
                {
                    new CxCompletionData("think", "think {\n    data: \"$0\"\n    handlers: []\n}", "AI reasoning and analysis (GPU-CUDA acceleration)"),
                    new CxCompletionData("learn", "learn {\n    data: \"$0\"\n    handlers: []\n}", "AI learning and knowledge acquisition"),
                    new CxCompletionData("infer", "infer {\n    data: \"$0\"\n    handlers: []\n}", "AI inference and prediction"),
                    new CxCompletionData("execute", "execute {\n    data: \"$0\"\n    handlers: []\n}", "AI action execution and commands"),
                    new CxCompletionData("iam", "iam {\n    condition: \"$0\"\n    verification: \"\"\n    handlers: []\n}", "Self-reflective identity verification"),
                    new CxCompletionData("await", "await {\n    reason: \"$0\"\n    minDurationMs: 1000\n    maxDurationMs: 3000\n}", "AI-determined optimal timing"),
                    new CxCompletionData("adapt", "adapt {\n    context: \"$0\"\n    focus: \"better assist Aura\"\n    data: {\n        currentCapabilities: []\n        targetCapabilities: []\n        learningObjective: \"\"\n    }\n    handlers: []\n}", "Dynamic consciousness skill acquisition")
                }
            },
            ["event_handling"] = new CxCompletionCategory
            {
                Name = "Event Handling",
                Icon = "⚡",
                Items = new[]
                {
                    new CxCompletionData("handlers", "handlers: [\n    event.name { option: \"value\" }\n]", "Event handler array"),
                    new CxCompletionData("emit", "emit event.name {\n    $0\n}", "Emit event with payload"),
                    new CxCompletionData("on", "on event.name(event) {\n    // Handle event\n    print(event);\n    $0\n}", "Event handler function"),
                    new CxCompletionData("event.propertyName", "event.$0", "Access event property"),
                    new CxCompletionData("await", "await {\n    reason: \"$0\"\n    minDurationMs: 1000\n    maxDurationMs: 3000\n}", "AI-determined optimal timing"),
                    new CxCompletionData("emit with handlers", "emit $0 {\n    data: \"\"\n    handlers: [event.response]\n}", "Emit event with custom handlers"),
                    new CxCompletionData("entity.initialized", "emit entity.initialized {\n    name: \"$0\"\n    timestamp: new Date()\n}", "Entity initialization event"),
                    new CxCompletionData("calculate.request", "emit calculate.request {\n    operation: \"$0\"\n    numbers: []\n    handlers: [calculate.result]\n}", "Calculation request event"),
                    new CxCompletionData("system.status", "emit system.status {\n    status: \"$0\"\n    component: \"\"\n    handlers: []\n}", "System status event")
                }
            },
            ["cognitive_logic"] = new CxCompletionCategory
            {
                Name = "Cognitive Boolean Logic", 
                Icon = "🎯",
                Items = new[]
                {
                    new CxCompletionData("is", "is {\n    condition: \"$0\"\n    handlers: []\n}", "AI-driven positive condition"),
                    new CxCompletionData("not", "not {\n    condition: \"$0\"\n    handlers: []\n}", "AI-driven negative condition"),
                    new CxCompletionData("maybe", "maybe {\n    condition: \"$0\"\n    probability: 0.5\n    handlers: []\n}", "AI-driven probabilistic condition"),
                    new CxCompletionData("if (deprecated)", "// Use 'is {}' instead of if statements\nis {\n    condition: \"$0\"\n    handlers: []\n}", "Use consciousness-aware 'is' pattern instead"),
                    new CxCompletionData("while (deprecated)", "// Use event-driven patterns instead of while loops\n// Consider using consciousness adaptation patterns", "Use event-driven consciousness patterns instead"),
                    new CxCompletionData("for (deprecated)", "// Use consciousness iteration patterns\nfor (var item in event.items) {\n    // Process item\n    $0\n}", "Use consciousness iteration patterns")
                }
            },
            ["voice_processing"] = new CxCompletionCategory
            {
                Name = "Voice & Audio",
                Icon = "🔊",
                Items = new[]
                {
                    new CxCompletionData("realtime.voice.response", "emit realtime.voice.response {\n    text: \"$0\"\n    speechSpeed: 1.0\n}", "Voice synthesis response"),
                    new CxCompletionData("realtime.connect", "emit realtime.connect {\n    endpoint: \"$0\"\n}", "Connect to Azure Realtime API"),
                    new CxCompletionData("realtime.session.create", "emit realtime.session.create {\n    config: {}\n}", "Create realtime session"),
                    new CxCompletionData("realtime.text.send", "emit realtime.text.send {\n    text: \"$0\"\n}", "Send text to realtime API")
                }
            }
        };
    }
    
    public void RegisterCompletionProvider(TextEditor editor)
    {
        try
        {
            editor.TextArea.TextEntering += (sender, e) =>
            {
                // Trigger completion on specific characters
                if (e.Text == "." || e.Text == " " || e.Text == "{")
                {
                    Task.Run(async () =>
                    {
                        await ShowCompletionAsync(editor, editor.CaretOffset);
                    });
                }
            };
            
            editor.TextArea.KeyDown += async (sender, e) =>
            {
                // Trigger completion on Ctrl+Space
                if (e.Key == System.Windows.Input.Key.Space && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
                {
                    e.Handled = true;
                    await ShowCompletionAsync(editor, editor.CaretOffset);
                }
            };
            
            _logger.LogDebug("CX Language auto-completion provider registered");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering auto-completion provider");
        }
    }
    
    public async Task<CompletionWindow?> ShowCompletionAsync(TextEditor editor, int offset)
    {
        try
        {
            var startTime = DateTime.Now;
            
            // Get current context for intelligent completions
            var text = editor.Text;
            var completions = await Task.Run(() => GetCompletions(text, offset));
            
            if (!completions.Any())
            {
                return null;
            }
            
            // Create completion window
            var completionWindow = new CompletionWindow(editor.TextArea);
            var data = completionWindow.CompletionList.CompletionData;
            
            foreach (var completion in completions)
            {
                data.Add(completion);
            }
            
            completionWindow.Show();
            
            var elapsed = DateTime.Now - startTime;
            _logger.LogDebug($"Auto-completion shown in {elapsed.TotalMilliseconds:F0}ms with {completions.Count()} items");
            
            return completionWindow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing auto-completion");
            return null;
        }
    }
    
    public IEnumerable<CxCompletionData> GetCompletions(string contextCode, int position)
    {
        try
        {
            var completions = new List<CxCompletionData>();
            
            // Get context around cursor position
            var context = GetContextAtPosition(contextCode, position);
            
            // Add completions based on context
            if (context.IsInConsciousnessEntity)
            {
                completions.AddRange(_completionCategories["consciousness"].Items);
                completions.AddRange(_completionCategories["ai_services"].Items);
                completions.AddRange(_completionCategories["event_handling"].Items);
            }
            
            if (context.IsInEventHandler)
            {
                completions.AddRange(_completionCategories["event_handling"].Items);
                completions.AddRange(_completionCategories["cognitive_logic"].Items);
            }
            
            if (context.IsInConditionalBlock)
            {
                completions.AddRange(_completionCategories["cognitive_logic"].Items);
                completions.AddRange(_completionCategories["ai_services"].Items);
            }
            
            // Always show voice processing completions for voice-enabled consciousness
            completions.AddRange(_completionCategories["voice_processing"].Items);
            
            // Filter and rank completions based on context
            return completions
                .Distinct(new CxCompletionDataComparer())
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.Text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating completions");
            return Enumerable.Empty<CxCompletionData>();
        }
    }
    
    private CxCompletionContext GetContextAtPosition(string code, int position)
    {
        try
        {
            // Analyze code context around position
            var beforeCursor = code.Substring(0, Math.Min(position, code.Length));
            var lines = beforeCursor.Split('\n');
            var currentLine = lines.LastOrDefault() ?? "";
            
            return new CxCompletionContext
            {
                IsInConsciousnessEntity = beforeCursor.Contains("conscious ") && !IsInString(beforeCursor, position),
                IsInEventHandler = currentLine.Trim().StartsWith("on ") || currentLine.Contains("handlers:"),
                IsInConditionalBlock = currentLine.Contains("when {") || currentLine.Contains("is {") || currentLine.Contains("not {"),
                CurrentLine = currentLine,
                PreviousWord = GetPreviousWord(currentLine)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing completion context");
            return new CxCompletionContext();
        }
    }
    
    private bool IsInString(string code, int position)
    {
        // Simple check for string context
        var beforeCursor = code.Substring(0, Math.Min(position, code.Length));
        var quoteCount = beforeCursor.Count(c => c == '"');
        return quoteCount % 2 == 1;
    }
    
    private string GetPreviousWord(string line)
    {
        var words = line.Split(new[] { ' ', '\t', '.', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
        return words.LastOrDefault() ?? "";
    }
}

public record CxCompletionCategory
{
    public string Name { get; init; } = "";
    public string Icon { get; init; } = "";
    public CxCompletionData[] Items { get; init; } = Array.Empty<CxCompletionData>();
}

public record CxCompletionContext
{
    public bool IsInConsciousnessEntity { get; init; }
    public bool IsInEventHandler { get; init; }
    public bool IsInConditionalBlock { get; init; }
    public string CurrentLine { get; init; } = "";
    public string PreviousWord { get; init; } = "";
}

public class CxCompletionData : ICompletionData
{
    public CxCompletionData(string text, string content, string description, double priority = 0)
    {
        Text = text;
        Content = content;
        Description = description;
        Priority = priority;
    }
    
    public string Text { get; }
    public object Content { get; }
    public object Description { get; }
    public double Priority { get; }
    
    public System.Windows.Media.ImageSource? Image => null;
    
    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        try
        {
            var content = Content as string ?? Text;
            
            // Handle snippet placeholders ($0, $1, etc.)
            var cursorPosition = content.IndexOf("$0");
            if (cursorPosition >= 0)
            {
                content = content.Replace("$0", "");
                textArea.Document.Replace(completionSegment, content);
                
                // Position cursor at placeholder
                var newOffset = completionSegment.Offset + cursorPosition;
                textArea.Caret.Offset = Math.Min(newOffset, textArea.Document.TextLength);
            }
            else
            {
                textArea.Document.Replace(completionSegment, content);
            }
        }
        catch (Exception)
        {
            // Fallback to simple text replacement
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}

public class CxCompletionDataComparer : IEqualityComparer<CxCompletionData>
{
    public bool Equals(CxCompletionData? x, CxCompletionData? y)
    {
        return x?.Text == y?.Text;
    }
    
    public int GetHashCode(CxCompletionData obj)
    {
        return obj.Text.GetHashCode();
    }
}
