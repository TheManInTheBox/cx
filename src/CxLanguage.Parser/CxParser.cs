using Antlr4.Runtime;
using CxLanguage.Core.Ast;

namespace CxLanguage.Parser;

/// <summary>
/// Main parser class for the Cx language
/// </summary>
public class CxLanguageParser
{
    public static ParseResult<ProgramNode> Parse(string source, string? fileName = null)
    {
        try
        {
            // Create ANTLR input stream
            var inputStream = new AntlrInputStream(source);
            
            // Create lexer
            var lexer = new CxLexer(inputStream);
            
            // Create token stream
            var tokenStream = new CommonTokenStream(lexer);
            
            // Create parser
            var parser = new CxParser(tokenStream);
            
            // Add error listener to collect parse errors
            var errorListener = new CxErrorListener();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);
            
            // Parse the program
            var parseTree = parser.program();
            
            // Check for parse errors
            if (errorListener.Errors.Count > 0)
            {
                return ParseResult<ProgramNode>.Failure(errorListener.Errors);
            }
            
            // Build AST from parse tree
            var astBuilder = new AstBuilder(fileName);
            var program = (ProgramNode)astBuilder.BuildAst(parseTree);
            
            return ParseResult<ProgramNode>.Success(program);
        }
        catch (Exception ex)
        {
            return ParseResult<ProgramNode>.Failure(new[] { new ParseError(0, 0, ex.Message) });
        }
    }
}

/// <summary>
/// Parse result container
/// </summary>
public class ParseResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<ParseError> Errors { get; }

    private ParseResult(bool isSuccess, T? value, IReadOnlyList<ParseError> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public static ParseResult<T> Success(T value) => new(true, value, Array.Empty<ParseError>());
    public static ParseResult<T> Failure(IEnumerable<ParseError> errors) => new(false, default, errors.ToArray());
}

/// <summary>
/// Parse error information
/// </summary>
public record ParseError(int Line, int Column, string Message);
