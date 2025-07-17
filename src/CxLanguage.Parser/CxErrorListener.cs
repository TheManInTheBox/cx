using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;

namespace CxLanguage.Parser;

/// <summary>
/// Custom error listener for collecting parse errors
/// </summary>
public class CxErrorListener : BaseErrorListener
{
    public List<ParseError> Errors { get; } = new List<ParseError>();

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Errors.Add(new ParseError(line, charPositionInLine, msg));
    }
}
