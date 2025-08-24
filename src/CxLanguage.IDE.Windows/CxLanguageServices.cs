using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Ast;
using CxLanguage.Parser;
using CxLanguage.Compiler;

namespace CxLanguage.IDE.Windows;

/// <summary>
/// Parser service wrapper for IDE integration
/// </summary>
public interface ICxLanguageParser
{
    ParseResult<AstNode> Parse(string code, string fileName);
}

public class CxLanguageParserService : ICxLanguageParser
{
    private readonly ILogger<CxLanguageParserService> _logger;
    
    public CxLanguageParserService(ILogger<CxLanguageParserService> logger)
    {
        _logger = logger;
    }
    
    public ParseResult<AstNode> Parse(string code, string fileName)
    {
        try
        {
            var result = CxLanguageParser.Parse(code, fileName);
            _logger.LogDebug($"Parsed CX code: {(result.IsSuccess ? "Success" : $"{result.Errors.Length} errors")}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error parsing CX code in file: {fileName}");
            return ParseResult<AstNode>.Failure(new[] { new ParseError(0, 0, ex.Message, fileName) });
        }
    }
}

/// <summary>
/// Compiler service wrapper for IDE integration
/// </summary>
public interface ICxCompilerService
{
    Task<CompileResult> CompileAsync(string code, string fileName);
    Task<string> ExecuteAsync(Assembly assembly);
}

public class CxCompilerService : ICxCompilerService
{
    private readonly ILogger<CxCompilerService> _logger;
    private readonly ICxLanguageParser _parser;
    
    public CxCompilerService(ILogger<CxCompilerService> logger, ICxLanguageParser parser)
    {
        _logger = logger;
        _parser = parser;
    }
    
    public async Task<CompileResult> CompileAsync(string code, string fileName)
    {
        try
        {
            return await Task.Run(() =>
            {
                // Parse the code first
                var parseResult = _parser.Parse(code, fileName);
                if (!parseResult.IsSuccess)
                {
                    var errorMessages = parseResult.Errors.Select(e => e.ToString()).ToArray();
                    return CompileResult.Failure(errorMessages);
                }
                
                // Compile the AST
                if (parseResult.Value is ProgramNode program)
                {
                    var options = new CompilerOptions();
                    var compiler = new CxCompiler(fileName, options);
                    var compileResult = compiler.Compile(program, fileName, code);
                    
                    if (compileResult.IsSuccess)
                    {
                        _logger.LogInformation($"Successfully compiled CX program: {fileName}");
                        return CompileResult.Success(compileResult.Assembly);
                    }
                    else
                    {
                        _logger.LogWarning($"Compilation failed for {fileName}: {compileResult.ErrorMessage}");
                        return CompileResult.Failure(new[] { compileResult.ErrorMessage });
                    }
                }
                else
                {
                    return CompileResult.Failure(new[] { "Invalid program structure" });
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during compilation of {fileName}");
            return CompileResult.Failure(new[] { ex.Message });
        }
    }
    
    public async Task<string> ExecuteAsync(Assembly assembly)
    {
        try
        {
            return await Task.Run(() =>
            {
                // Find the program type and Run method
                var programType = assembly.GetTypes().FirstOrDefault(t => t.Name == "Program");
                if (programType == null)
                {
                    return "Error: Program type not found in assembly";
                }
                
                var runMethod = programType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
                if (runMethod == null)
                {
                    return "Error: Run method not found in Program type";
                }
                
                // Capture output
                var originalOut = Console.Out;
                using var stringWriter = new System.IO.StringWriter();
                Console.SetOut(stringWriter);
                
                try
                {
                    // Execute the program
                    var result = runMethod.Invoke(null, null);
                    var output = stringWriter.ToString();
                    
                    _logger.LogInformation("Successfully executed CX program");
                    return string.IsNullOrEmpty(output) ? "Program executed successfully (no output)" : output;
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during program execution");
            return $"Execution error: {ex.Message}";
        }
    }
}

/// <summary>
/// Compilation result wrapper
/// </summary>
public class CompileResult
{
    public bool IsSuccess { get; private set; }
    public Assembly? Assembly { get; private set; }
    public string[] Errors { get; private set; } = Array.Empty<string>();
    
    private CompileResult(bool isSuccess, Assembly? assembly, string[] errors)
    {
        IsSuccess = isSuccess;
        Assembly = assembly;
        Errors = errors;
    }
    
    public static CompileResult Success(Assembly assembly) => new(true, assembly, Array.Empty<string>());
    public static CompileResult Failure(string[] errors) => new(false, null, errors);
}
