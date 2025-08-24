using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using ScintillaNET;
using CxLanguage.Core.Ast;
using CxLanguage.Parser;

namespace CxLanguage.IDE.Windows;

/// <summary>
/// CX Language syntax highlighter using ANTLR4 parser integration
/// Implements GitHub Issue #220 syntax highlighting requirements
/// </summary>
public interface ICxSyntaxHighlighter
{
    void ConfigureEditor(Scintilla editor);
    void HighlightSyntax(Scintilla editor, AstNode ast);
    void HighlightErrors(Scintilla editor, ParseError[] errors);
}

public class CxSyntaxHighlighter : ICxSyntaxHighlighter
{
    private readonly ILogger<CxSyntaxHighlighter> _logger;
    
    // Scintilla style constants for CX Language
    private const int STYLE_DEFAULT = 0;
    private const int STYLE_KEYWORD = 1;
    private const int STYLE_CONSCIOUSNESS_KEYWORD = 2;
    private const int STYLE_AI_SERVICE = 3;
    private const int STYLE_STRING = 4;
    private const int STYLE_NUMBER = 5;
    private const int STYLE_COMMENT = 6;
    private const int STYLE_IDENTIFIER = 7;
    private const int STYLE_EVENT_NAME = 8;
    private const int STYLE_HANDLER = 9;
    private const int STYLE_EMIT = 10;
    private const int STYLE_ERROR = 11;
    
    public CxSyntaxHighlighter(ILogger<CxSyntaxHighlighter> logger)
    {
        _logger = logger;
    }
    
    public void ConfigureEditor(Scintilla editor)
    {
        // Configure Scintilla for CX Language
        editor.Lexer = Lexer.Container;
        
        // Configure styles for CX Language constructs
        ConfigureStyle(editor, STYLE_DEFAULT, Color.White, Color.FromArgb(40, 40, 60), false, false);
        ConfigureStyle(editor, STYLE_KEYWORD, Color.FromArgb(86, 156, 214), Color.FromArgb(40, 40, 60), true, false); // Blue keywords
        ConfigureStyle(editor, STYLE_CONSCIOUSNESS_KEYWORD, Color.FromArgb(197, 134, 192), Color.FromArgb(40, 40, 60), true, false); // Purple consciousness
        ConfigureStyle(editor, STYLE_AI_SERVICE, Color.FromArgb(78, 201, 176), Color.FromArgb(40, 40, 60), true, false); // Teal AI services
        ConfigureStyle(editor, STYLE_STRING, Color.FromArgb(206, 145, 120), Color.FromArgb(40, 40, 60), false, false); // Orange strings
        ConfigureStyle(editor, STYLE_NUMBER, Color.FromArgb(181, 206, 168), Color.FromArgb(40, 40, 60), false, false); // Green numbers
        ConfigureStyle(editor, STYLE_COMMENT, Color.FromArgb(106, 153, 85), Color.FromArgb(40, 40, 60), false, true); // Green comments
        ConfigureStyle(editor, STYLE_IDENTIFIER, Color.FromArgb(220, 220, 170), Color.FromArgb(40, 40, 60), false, false); // Yellow identifiers
        ConfigureStyle(editor, STYLE_EVENT_NAME, Color.FromArgb(255, 198, 109), Color.FromArgb(40, 40, 60), false, false); // Orange events
        ConfigureStyle(editor, STYLE_HANDLER, Color.FromArgb(156, 220, 254), Color.FromArgb(40, 40, 60), false, false); // Light blue handlers
        ConfigureStyle(editor, STYLE_EMIT, Color.FromArgb(255, 123, 114), Color.FromArgb(40, 40, 60), true, false); // Red emit statements
        ConfigureStyle(editor, STYLE_ERROR, Color.White, Color.FromArgb(255, 0, 0), false, false); // Error highlighting
        
        // Configure additional editor properties
        editor.CaretForeColor = Color.White;
        editor.CaretLineBackColor = Color.FromArgb(50, 50, 70);
        editor.CaretLineVisible = true;
        
        // Configure margins
        editor.Margins[0].Width = 40;
        editor.Margins[0].Type = MarginType.Number;
        editor.Margins[0].Sensitive = true;
        editor.Margins[0].Mask = 0;
        
        // Configure folding
        editor.SetProperty("fold", "1");
        editor.SetProperty("fold.compact", "1");
        editor.Margins[1].Type = MarginType.Symbol;
        editor.Margins[1].Mask = Marker.MaskFolders;
        editor.Margins[1].Sensitive = true;
        editor.Margins[1].Width = 20;
        
        // Configure indentation
        editor.IndentationGuides = IndentView.LookBoth;
        editor.TabWidth = 4;
        editor.UseTabs = false;
        
        // Configure bracket matching
        editor.StyleResetDefault();
        
        _logger.LogDebug("CX Language editor configured with syntax highlighting styles");
    }
    
    private void ConfigureStyle(Scintilla editor, int style, Color foreground, Color background, bool bold, bool italic)
    {
        editor.Styles[style].ForeColor = foreground;
        editor.Styles[style].BackColor = background;
        editor.Styles[style].Bold = bold;
        editor.Styles[style].Italic = italic;
        editor.Styles[style].Font = "Consolas";
        editor.Styles[style].Size = 11;
    }
    
    public void HighlightSyntax(Scintilla editor, AstNode ast)
    {
        try
        {
            // Clear existing styling
            editor.StartStyling(0);
            editor.SetStyling(editor.TextLength, STYLE_DEFAULT);
            
            // Apply syntax highlighting based on AST
            if (ast is ProgramNode program)
            {
                HighlightProgram(editor, program);
            }
            
            _logger.LogDebug("Syntax highlighting applied successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying syntax highlighting");
        }
    }
    
    private void HighlightProgram(Scintilla editor, ProgramNode program)
    {
        var text = editor.Text;
        
        // Highlight keywords and constructs
        HighlightKeywords(editor, text);
        HighlightConsciousnessPatterns(editor, text);
        HighlightAiServices(editor, text);
        HighlightStringsAndNumbers(editor, text);
        HighlightComments(editor, text);
        HighlightEventPatterns(editor, text);
        
        // Highlight specific AST nodes
        foreach (var statement in program.Statements)
        {
            HighlightStatement(editor, statement);
        }
    }
    
    private void HighlightKeywords(Scintilla editor, string text)
    {
        var keywords = new[] { "var", "function", "if", "else", "while", "for", "return", "true", "false", "null", "object", "extends", "new" };
        
        foreach (var keyword in keywords)
        {
            HighlightPattern(editor, text, $@"\b{keyword}\b", STYLE_KEYWORD);
        }
    }
    
    private void HighlightConsciousnessPatterns(Scintilla editor, string text)
    {
        var consciousnessKeywords = new[] { "conscious", "realize", "when", "adapt", "iam", "handlers" };
        
        foreach (var keyword in consciousnessKeywords)
        {
            HighlightPattern(editor, text, $@"\b{keyword}\b", STYLE_CONSCIOUSNESS_KEYWORD);
        }
        
        // Highlight consciousness entity declarations
        HighlightPattern(editor, text, @"conscious\s+\w+\s*\{", STYLE_CONSCIOUSNESS_KEYWORD);
    }
    
    private void HighlightAiServices(Scintilla editor, string text)
    {
        var aiServices = new[] { "think", "learn", "infer", "search", "execute", "await" };
        
        foreach (var service in aiServices)
        {
            HighlightPattern(editor, text, $@"\b{service}\b", STYLE_AI_SERVICE);
        }
    }
    
    private void HighlightEventPatterns(Scintilla editor, string text)
    {
        // Highlight 'on' statements
        HighlightPattern(editor, text, @"\bon\s+[\w.]+\s*\([^)]*\)", STYLE_EVENT_NAME);
        
        // Highlight 'emit' statements
        HighlightPattern(editor, text, @"\bemit\b", STYLE_EMIT);
        
        // Highlight event names in emit statements
        HighlightPattern(editor, text, @"emit\s+([\w.]+)", STYLE_EVENT_NAME);
        
        // Highlight handler arrays
        HighlightPattern(editor, text, @"handlers:\s*\[", STYLE_HANDLER);
    }
    
    private void HighlightStringsAndNumbers(Scintilla editor, string text)
    {
        // Highlight strings
        HighlightPattern(editor, text, @"""[^""]*""", STYLE_STRING);
        HighlightPattern(editor, text, @"'[^']*'", STYLE_STRING);
        
        // Highlight numbers
        HighlightPattern(editor, text, @"\b\d+\.?\d*\b", STYLE_NUMBER);
    }
    
    private void HighlightComments(Scintilla editor, string text)
    {
        // Highlight line comments
        HighlightPattern(editor, text, @"//.*$", STYLE_COMMENT);
        
        // Highlight block comments
        HighlightPattern(editor, text, @"/\*.*?\*/", STYLE_COMMENT);
    }
    
    private void HighlightPattern(Scintilla editor, string text, string pattern, int style)
    {
        try
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
                System.Text.RegularExpressions.RegexOptions.Multiline);
            
            foreach (System.Text.RegularExpressions.Match match in regex.Matches(text))
            {
                editor.StartStyling(match.Index);
                editor.SetStyling(match.Length, style);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error highlighting pattern: {pattern}");
        }
    }
    
    private void HighlightStatement(Scintilla editor, AstNode statement)
    {
        // Highlight specific AST node types with precise positioning
        switch (statement)
        {
            case ConsciousDeclarationNode conscious:
                HighlightConsciousDeclaration(editor, conscious);
                break;
            case OnStatementNode onStatement:
                HighlightOnStatement(editor, onStatement);
                break;
            case EmitStatementNode emitStatement:
                HighlightEmitStatement(editor, emitStatement);
                break;
        }
    }
    
    private void HighlightConsciousDeclaration(Scintilla editor, ConsciousDeclarationNode node)
    {
        // Highlight consciousness entity name specifically
        var text = editor.Text;
        var pattern = $@"conscious\s+{node.Name}\s*\{{";
        HighlightPattern(editor, text, pattern, STYLE_CONSCIOUSNESS_KEYWORD);
    }
    
    private void HighlightOnStatement(Scintilla editor, OnStatementNode node)
    {
        // Highlight event handler patterns
        var text = editor.Text;
        var eventPattern = node.EventName.Replace(".", @"\.");
        HighlightPattern(editor, text, $@"on\s+{eventPattern}", STYLE_EVENT_NAME);
    }
    
    private void HighlightEmitStatement(Scintilla editor, EmitStatementNode node)
    {
        // Highlight emit statements with event names
        var text = editor.Text;
        var eventPattern = node.EventName.Replace(".", @"\.");
        HighlightPattern(editor, text, $@"emit\s+{eventPattern}", STYLE_EMIT);
    }
    
    public void HighlightErrors(Scintilla editor, ParseError[] errors)
    {
        try
        {
            // Clear existing error indicators
            editor.IndicatorClearRange(0, editor.TextLength);
            
            // Configure error indicator
            editor.Indicators[0].Style = IndicatorStyle.Squiggle;
            editor.Indicators[0].ForeColor = Color.Red;
            
            foreach (var error in errors)
            {
                // Calculate position from line/column
                var line = Math.Max(0, error.Line - 1);
                var column = Math.Max(0, error.Column);
                
                if (line < editor.Lines.Count)
                {
                    var lineStart = editor.Lines[line].Position;
                    var lineLength = editor.Lines[line].Length;
                    var errorStart = Math.Min(lineStart + column, lineStart + lineLength);
                    var errorLength = Math.Min(10, lineLength - column); // Highlight up to 10 characters
                    
                    if (errorLength > 0)
                    {
                        editor.IndicatorFillRange(errorStart, errorLength);
                    }
                }
            }
            
            _logger.LogDebug($"Highlighted {errors.Length} syntax errors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error highlighting syntax errors");
        }
    }
}
