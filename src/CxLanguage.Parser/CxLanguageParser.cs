using CxLanguage.Core;
using CxLanguage.Core.Ast;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CxLanguage.Parser;

/// <summary>
/// Wrapper class for ANTLR CxParser to provide high-level parsing interface
/// </summary>
public static class CxLanguageParser
{
    public static ParseResult<AstNode> Parse(string source, string fileName = "<unknown>")
    {
        try
        {
            // Create ANTLR input stream
            var input = new AntlrInputStream(source);
            var lexer = new CxLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new CxParser(tokens);
            
            // Add error listener to collect errors
            var errorListener = new CxErrorListener();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);
            
            // Parse the program
            var parseTree = parser.program();
            
            // Check for parsing errors
            if (errorListener.HasErrors)
            {
                return ParseResult<AstNode>.Failure(errorListener.Errors.ToArray());
            }
            
            // Build AST from parse tree
            var astBuilder = new AstBuilder();
            var ast = astBuilder.BuildAst(parseTree);
            
            return ParseResult<AstNode>.Success(ast);
        }
        catch (Exception ex)
        {
            return ParseResult<AstNode>.Failure(new[] { new ParseError(0, 0, ex.Message, fileName) });
        }
    }
}

/// <summary>
/// Result of a parsing operation
/// </summary>
public class ParseResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public ParseError[] Errors { get; private set; }
    
    private ParseResult(bool isSuccess, T? value, ParseError[] errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }
    
    public static ParseResult<T> Success(T value) => new(true, value, Array.Empty<ParseError>());
    public static ParseResult<T> Failure(ParseError[] errors) => new(false, default, errors);
}

/// <summary>
/// Represents a parsing error
/// </summary>
public class ParseError
{
    public int Line { get; }
    public int Column { get; }
    public string Message { get; }
    public string FileName { get; }
    
    public ParseError(int line, int column, string message, string fileName)
    {
        Line = line;
        Column = column;
        Message = message;
        FileName = fileName;
    }
    
    public override string ToString() => $"{FileName}({Line},{Column}): {Message}";
}
