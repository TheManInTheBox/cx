using Antlr4.Runtime;
using System.Collections.Generic;

namespace CxLanguage.Parser;

/// <summary>
/// Error listener for ANTLR parsing errors
/// </summary>
public class CxErrorListener : BaseErrorListener
{
    private readonly List<ParseError> _errors = new();
    
    public bool HasErrors => _errors.Count > 0;
    public IReadOnlyList<ParseError> Errors => _errors;
    
    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer, 
        IToken offendingSymbol, 
        int line, 
        int charPositionInLine, 
        string msg, 
        RecognitionException e)
    {
        _errors.Add(new ParseError(line, charPositionInLine, msg, "<input>"));
    }
}
