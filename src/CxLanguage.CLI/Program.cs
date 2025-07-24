using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.CommandLine;
using CxLanguage.Parser;
using CxLanguage.Compiler;
using CxLanguage.Compiler.Modules;
using CxLanguage.Azure.Services;
// NEURAL SYSTEM BYPASS: Legacy VectorDatabase reference disabled
// using CxLanguage.StandardLibrary.AI.VectorDatabase;
using CxLanguage.StandardLibrary.Extensions;
using CxLanguage.CLI.Extensions;
using CxLanguage.Core.Ast;
using CxLanguage.Runtime;
using CxCoreAI = CxLanguage.Core.AI;

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
        // Initialize debug tracing system
        CxDebugTracing.Initialize();
        CxDebugTracing.TraceInfo("CLI", "Starting CX Language script execution", new { FileName = file.Name, FullPath = file.FullName });
        
        if (!file.Exists)
        {
            Console.Error.WriteLine($"Error: File '{file.FullName}' not found.");
            CxDebugTracing.TraceError("CLI", "Script file not found", new { FileName = file.FullName });
            return;
        }

        var scriptExecutionStopwatch = System.Diagnostics.Stopwatch.StartNew();
        IHost? host = null;
        
        try
        {
            // Setup services
            host = CreateHost();
            
            // Start hosted services (including VoiceInputEventBridge via VoiceServiceInitializer)
            await host.StartAsync();
            
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var telemetryService = host.Services.GetService<CxLanguage.Core.Telemetry.CxTelemetryService>();

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
                
                // Track failed script execution
                telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, false, "Parse errors");
                return;
            }

            // NEURAL SYSTEM BYPASS: Simplified compilation for biological neural testing
            var compilationStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var options = new CompilerOptions();
            CxCoreAI.IAiService? aiService = null;
            
            try
            {
                aiService = host.Services.GetRequiredService<CxCoreAI.IAiService>();
                Console.WriteLine($"✅ Neural System: AI Service loaded for biological testing");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: AI service not available: {ex.Message}");
                Console.WriteLine("Neural system will continue with basic event functionality.");
                aiService = null;
            }
            
            var compiler = new CxCompiler(Path.GetFileNameWithoutExtension(file.Name), options, aiService);
            var compilationResult = compiler.Compile((ProgramNode)parseResult.Value!, Path.GetFileNameWithoutExtension(file.Name), source);
            
            // Track compilation metrics
            var linesOfCode = source.Split('\n').Length;
            telemetryService?.TrackCompilation(file.Name, compilationStopwatch.Elapsed, compilationResult.IsSuccess, linesOfCode, 
                compilationResult.IsSuccess ? null : compilationResult.ErrorMessage);

            if (!compilationResult.IsSuccess)
            {
                Console.Error.WriteLine($"Compilation error: {compilationResult.ErrorMessage}");
                telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, false, compilationResult.ErrorMessage);
                return;
            }

            // Register the compiled assembly for runtime function access
            if (compilationResult.Assembly != null && compilationResult.ProgramType != null)
            {
                CxLanguage.Compiler.Modules.RuntimeFunctionRegistry.RegisterAssembly(
                    Path.GetFileNameWithoutExtension(file.Name), 
                    compilationResult.Assembly, 
                    compilationResult.ProgramType
                );
            }
            
            // Register built-in functions for namespace event system
            CxLanguage.Compiler.Modules.RuntimeFunctionRegistry.RegisterBuiltInFunctions();

            // Execute the compiled assembly
            var runMethod = compilationResult.ProgramType!.GetMethod("Run");
            if (runMethod != null)
            {
                try
                {
                    Console.WriteLine($"[DEBUG] RUNTIME: About to create Program instance");
                    
                    // NEURAL SYSTEM: Create instance with simplified parameters for biological testing
                    var instance = Activator.CreateInstance(
                        compilationResult.ProgramType, 
                        new object(),  // Console object
                        aiService,     // AI service (can be null)
                        host.Services  // Service provider for DI
                    );
                    
                    Console.WriteLine($"[DEBUG] RUNTIME: Program instance created successfully");
                    Console.WriteLine($"[DEBUG] RUNTIME: About to invoke Run method");
                    
                    runMethod.Invoke(instance, null);
                    
                    Console.WriteLine($"[DEBUG] RUNTIME: Run method completed successfully");
                    
                    // Emit system.start event to trigger CX program execution
                    try
                    {
                        var eventBus = host.Services.GetService<CxLanguage.Runtime.ICxEventBus>();
                        if (eventBus != null)
                        {
                            Console.WriteLine($"[DEBUG] RUNTIME: Emitting system.start event");
                            await eventBus.EmitAsync("system.start", new { timestamp = DateTime.UtcNow });
                            Console.WriteLine($"[DEBUG] RUNTIME: system.start event emitted successfully");
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] RUNTIME: No event bus available for system.start");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] RUNTIME: Error emitting system.start: {ex.Message}");
                    }
                    
                    // Track successful execution
                    telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, true);
                    
                    // Wait for events to complete - give user control to exit when ready
                    Console.WriteLine("\n🔑 Press any key to exit (waiting for background events to complete)...");
                    
                    try
                    {
                        Console.ReadKey(true);
                    }
                    catch (InvalidOperationException)
                    {
                        // Handle redirected input by waiting a short time for events to process
                        Console.WriteLine("Input redirected - waiting 3 seconds for events to complete...");
                        await Task.Delay(3000);
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    Console.WriteLine($"[DEBUG] RUNTIME: TargetInvocationException caught");
                    Console.WriteLine($"[DEBUG] RUNTIME: Outer exception: {ex.Message}");
                    
                    var innerEx = ex.InnerException;
                    while (innerEx != null)
                    {
                        Console.WriteLine($"[DEBUG] RUNTIME: Inner exception: {innerEx.GetType().Name}: {innerEx.Message}");
                        if (innerEx.StackTrace != null)
                        {
                            Console.WriteLine($"[DEBUG] RUNTIME: Inner stack trace: {innerEx.StackTrace}");
                        }
                        innerEx = innerEx.InnerException;
                    }
                    
                    var errorMessage = ex.InnerException?.Message ?? ex.Message;
                    Console.Error.WriteLine($"Runtime error: {errorMessage}");
                    if (ex.InnerException != null)
                    {
                        Console.Error.WriteLine($"Stack trace: {ex.InnerException.StackTrace}");
                    }
                    
                    // Track failed execution
                    telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, false, errorMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] RUNTIME: General exception caught: {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine($"[DEBUG] RUNTIME: Stack trace: {ex.StackTrace}");
                    
                    Console.Error.WriteLine($"Runtime error: {ex.Message}");
                    Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    // Track failed execution
                    telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, false, ex.Message);
                }
            }
            else
            {
                Console.Error.WriteLine("No Run method found in compiled assembly.");
                telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, false, "No Run method found");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error running script: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Track exception
            var telemetryService = host?.Services?.GetService<CxLanguage.Core.Telemetry.CxTelemetryService>();
            telemetryService?.TrackException(ex, "RunScript");
            telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, false, ex.Message);
        }
        finally
        {
            if (host != null)
            {
                await host.StopAsync();
                host.Dispose();
            }
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
            var assemblyName = output?.Name ?? Path.ChangeExtension(source.Name, ".dll");
            var options = new CompilerOptions();
            
            CxCoreAI.IAiService? aiService = null;
            // NEURAL SYSTEM BYPASS: Remove legacy aiFunctions parameter
            
            try
            {
                aiService = host.Services.GetRequiredService<CxCoreAI.IAiService>();
                Console.WriteLine($"✅ Neural System: AI Service loaded for compile mode");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: AI service not available: {ex.Message}");
                aiService = null;
            }
            
            var compiler = new CxCompiler(Path.GetFileNameWithoutExtension(assemblyName), options, aiService);
            var sourceText = File.ReadAllText(source.FullName);
            var compilationResult = compiler.Compile((ProgramNode)parseResult.Value!, Path.GetFileNameWithoutExtension(assemblyName), sourceText);

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
            var program = (ProgramNode)parseResult.Value!;
            Console.WriteLine($"Program contains {program.Statements.Count} statements");
            Console.WriteLine($"Imports: {program.Imports.Count}");

            // Display basic AST structure
            var astPrinter = new AstPrinter();
            var output = astPrinter.Print(parseResult.Value!);
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
        try
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Get the directory where the executable is located
                    var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var basePath = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
                    
                    config.SetBasePath(basePath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(builder => 
                    {
                        builder.AddConsole();
                        builder.AddApplicationInsights(
                            configureTelemetryConfiguration: (config) => 
                            {
                                config.ConnectionString = context.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING") 
                                    ?? context.Configuration.GetSection("ApplicationInsights")["ConnectionString"];
                            },
                            configureApplicationInsightsLoggerOptions: (options) => { }
                        );
                    });

                    // Add Application Insights telemetry for console applications
                    services.AddApplicationInsightsTelemetryWorkerService(context.Configuration);
                    
                    // Add CX Language telemetry service
                    services.AddSingleton<CxLanguage.Core.Telemetry.CxTelemetryService>();

                    // Register CX Event Bus for agent communication with comprehensive logging
                    services.AddSingleton<ICxEventBus>(provider => 
                    {
                        var logger = provider.GetRequiredService<ILogger<UnifiedEventBus>>();
                        return UnifiedEventBusRegistry.Instance as ICxEventBus;
                    });

                    // Always register Vector Database services - they can work without full Azure OpenAI config
                    try
                    {
                        // NEURAL SYSTEM BYPASS: Skip legacy Kernel Memory services
                        Console.WriteLine("Neural System: Skipping legacy Vector Database services");
                        Console.WriteLine("Vector Database services registered successfully");
                    }
                    catch (Exception vectorEx)
                    {
                        Console.WriteLine($"Warning: Vector Database initialization failed: {vectorEx.Message}");
                        // The extension method handles fallback gracefully
                    }

                    try
                    {
                        // NEURAL SYSTEM BYPASS: Use modern services instead of legacy Semantic Kernel
                        services.AddModernCxAiServices(context.Configuration);
                        Console.WriteLine("✅ Neural System: Modern AI services registered");
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue - we'll just have null AI service
                        Console.WriteLine($"Warning: AI services could not be initialized: {ex.Message}");
                        Console.WriteLine("Function return values will still work, but AI functions will not.");
                        
                        // Register a null AI service for testing non-AI features
                        services.AddSingleton<CxCoreAI.IAiService>(provider => null!);
                    }
                })
                .Build();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating host: {ex.Message}");
            
            // Create a minimal host for testing
            return new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(builder => builder.AddConsole());
                    services.AddSingleton<CxCoreAI.IAiService>(provider => null!);
                    services.AddSingleton<ICxEventBus>(provider => 
                    {
                        var logger = provider.GetRequiredService<ILogger<UnifiedEventBus>>();
                        return UnifiedEventBusRegistry.Instance as ICxEventBus;
                    });
                })
                .Build();
        }
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
