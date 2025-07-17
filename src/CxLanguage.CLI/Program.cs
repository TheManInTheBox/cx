using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using CxLanguage.Parser;
using CxLanguage.Compiler;
using CxLanguage.Azure.Services;

namespace CxLanguage.CLI;

/// <summary>
/// Main entry point for the Cx language CLI
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = CreateRootCommand();
        return await rootCommand.InvokeAsync(args);
    }

    static RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand("Cx Language - AI-Integrated Scripting Language");

        // Run command
        var runCommand = new Command("run", "Run a Cx script file");
        var fileArgument = new Argument<FileInfo>("file", "The Cx script file to run");
        runCommand.AddArgument(fileArgument);
        runCommand.SetHandler(RunScript, fileArgument);

        // Compile command
        var compileCommand = new Command("compile", "Compile a Cx script to .NET assembly");
        var sourceArg = new Argument<FileInfo>("source", "The Cx source file");
        var outputOption = new Option<FileInfo?>("--output", "Output assembly file");
        compileCommand.AddArgument(sourceArg);
        compileCommand.AddOption(outputOption);
        compileCommand.SetHandler(CompileScript, sourceArg, outputOption);

        // Parse command (for debugging)
        var parseCommand = new Command("parse", "Parse and display AST for a Cx script");
        var parseFileArg = new Argument<FileInfo>("file", "The Cx script file to parse");
        parseCommand.AddArgument(parseFileArg);
        parseCommand.SetHandler(ParseScript, parseFileArg);

        // Version command
        var versionCommand = new Command("version", "Show version information");
        versionCommand.SetHandler(ShowVersion);

        rootCommand.AddCommand(runCommand);
        rootCommand.AddCommand(compileCommand);
        rootCommand.AddCommand(parseCommand);
        rootCommand.AddCommand(versionCommand);

        return rootCommand;
    }

    static async Task RunScript(FileInfo file)
    {
        if (!file.Exists)
        {
            Console.Error.WriteLine($"Error: File '{file.FullName}' not found.");
            return;
        }

        try
        {
            // Setup services
            using var host = CreateHost();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Running Cx script: {FileName}", file.Name);

            // Read and parse the script
            var source = await File.ReadAllTextAsync(file.FullName);
            var parseResult = CxLanguage.Parser.CxLanguageParser.Parse(source, file.FullName);

            if (!parseResult.IsSuccess)
            {
                Console.Error.WriteLine("Parse errors:");
                foreach (var error in parseResult.Errors)
                {
                    Console.Error.WriteLine($"  Line {error.Line}, Column {error.Column}: {error.Message}");
                }
                return;
            }

            // Compile the script
            var compiler = new CxCompiler();
            var compilationResult = compiler.Compile(parseResult.Value!, Path.GetFileNameWithoutExtension(file.Name));

            if (!compilationResult.IsSuccess)
            {
                Console.Error.WriteLine($"Compilation error: {compilationResult.ErrorMessage}");
                return;
            }

            // Execute the compiled assembly
            var mainMethod = compilationResult.ProgramType!.GetMethod("Main");
            if (mainMethod != null)
            {
                try
                {
                    mainMethod.Invoke(null, null);
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    Console.Error.WriteLine($"Runtime error: {ex.InnerException?.Message ?? ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.Error.WriteLine($"Stack trace: {ex.InnerException.StackTrace}");
                    }
                }
            }
            else
            {
                Console.Error.WriteLine("No Main method found in compiled assembly.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error running script: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    static async Task CompileScript(FileInfo source, FileInfo? output)
    {
        if (!source.Exists)
        {
            Console.Error.WriteLine($"Error: Source file '{source.FullName}' not found.");
            return;
        }

        try
        {
            using var host = CreateHost();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Compiling Cx script: {FileName}", source.Name);

            // Read and parse the script
            var sourceCode = await File.ReadAllTextAsync(source.FullName);
            var parseResult = CxLanguage.Parser.CxLanguageParser.Parse(sourceCode, source.FullName);

            if (!parseResult.IsSuccess)
            {
                Console.Error.WriteLine("Parse errors:");
                foreach (var error in parseResult.Errors)
                {
                    Console.Error.WriteLine($"  Line {error.Line}, Column {error.Column}: {error.Message}");
                }
                return;
            }

            // Compile the script
            var compiler = new CxCompiler();
            var assemblyName = output?.Name ?? Path.ChangeExtension(source.Name, ".dll");
            var compilationResult = compiler.Compile(parseResult.Value!, Path.GetFileNameWithoutExtension(assemblyName));

            if (!compilationResult.IsSuccess)
            {
                Console.Error.WriteLine($"Compilation error: {compilationResult.ErrorMessage}");
                return;
            }

            Console.WriteLine($"Successfully compiled to: {assemblyName}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error compiling script: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    static async Task ParseScript(FileInfo file)
    {
        if (!file.Exists)
        {
            Console.Error.WriteLine($"Error: File '{file.FullName}' not found.");
            return;
        }

        try
        {
            var source = await File.ReadAllTextAsync(file.FullName);
            var parseResult = CxLanguage.Parser.CxLanguageParser.Parse(source, file.FullName);

            if (!parseResult.IsSuccess)
            {
                Console.WriteLine("Parse errors:");
                foreach (var error in parseResult.Errors)
                {
                    Console.WriteLine($"  Line {error.Line}, Column {error.Column}: {error.Message}");
                }
                return;
            }

            Console.WriteLine("Parse successful!");
            Console.WriteLine($"Program contains {parseResult.Value!.Statements.Count} statements");
            Console.WriteLine($"Imports: {parseResult.Value.Imports.Count}");

            // Display basic AST structure
            var astPrinter = new AstPrinter();
            var output = astPrinter.Print(parseResult.Value);
            Console.WriteLine("\nAST Structure:");
            Console.WriteLine(output);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing script: {ex.Message}");
        }
    }

    static Task ShowVersion()
    {
        var version = typeof(Program).Assembly.GetName().Version;
        Console.WriteLine($"Cx Language v{version}");
        Console.WriteLine("AI-Integrated Scripting Language for .NET");
        Console.WriteLine("Built with Azure OpenAI integration");
        return Task.CompletedTask;
    }

    static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(builder => builder.AddConsole());
                services.AddSingleton<IAiService, AzureOpenAIService>();
            })
            .Build();
    }
}

/// <summary>
/// Simple AST printer for debugging
/// </summary>
public class AstPrinter
{
    private int _indentLevel = 0;
    private const string IndentString = "  ";

    public string Print(CxLanguage.Core.Ast.AstNode node)
    {
        var result = new System.Text.StringBuilder();
        PrintNode(node, result);
        return result.ToString();
    }

    private void PrintNode(CxLanguage.Core.Ast.AstNode node, System.Text.StringBuilder result)
    {
        var indent = new string(' ', _indentLevel * 2);
        result.AppendLine($"{indent}{node.GetType().Name}");

        _indentLevel++;

        // Print basic node information
        if (node is CxLanguage.Core.Ast.FunctionDeclarationNode func)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Name: {func.Name}");
            result.AppendLine($"{new string(' ', _indentLevel * 2)}IsAsync: {func.IsAsync}");
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Parameters: {func.Parameters.Count}");
            
            // Print function body
            if (func.Body != null)
            {
                result.AppendLine($"{new string(' ', _indentLevel * 2)}Body:");
                _indentLevel++;
                PrintNode(func.Body, result);
                _indentLevel--;
            }
        }
        else if (node is CxLanguage.Core.Ast.BlockStatementNode block)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Statements: {block.Statements.Count}");
            foreach (var stmt in block.Statements)
            {
                PrintNode(stmt, result);
            }
        }
        else if (node is CxLanguage.Core.Ast.ExpressionStatementNode exprStmt)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Expression:");
            _indentLevel++;
            PrintNode(exprStmt.Expression, result);
            _indentLevel--;
        }
        else if (node is CxLanguage.Core.Ast.VariableDeclarationNode var)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Name: {var.Name}");
            if (var.Initializer != null)
            {
                result.AppendLine($"{new string(' ', _indentLevel * 2)}Initializer:");
                _indentLevel++;
                PrintNode(var.Initializer, result);
                _indentLevel--;
            }
        }
        else if (node is CxLanguage.Core.Ast.AssignmentExpressionNode assign)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Left:");
            _indentLevel++;
            PrintNode(assign.Left, result);
            _indentLevel--;
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Right:");
            _indentLevel++;
            PrintNode(assign.Right, result);
            _indentLevel--;
        }
        else if (node is CxLanguage.Core.Ast.CallExpressionNode call)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Callee:");
            _indentLevel++;
            PrintNode(call.Callee, result);
            _indentLevel--;
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Arguments: {call.Arguments.Count}");
            foreach (var arg in call.Arguments)
            {
                _indentLevel++;
                PrintNode(arg, result);
                _indentLevel--;
            }
        }
        else if (node is CxLanguage.Core.Ast.IdentifierNode id)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Name: {id.Name}");
        }
        else if (node is CxLanguage.Core.Ast.LiteralNode lit)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Value: {lit.Value}");
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Type: {lit.Type}");
        }
        else if (node is CxLanguage.Core.Ast.ProgramNode program)
        {
            result.AppendLine($"{new string(' ', _indentLevel * 2)}Statements: {program.Statements.Count}");
            foreach (var stmt in program.Statements)
            {
                PrintNode(stmt, result);
            }
        }

        _indentLevel--;
    }
}
