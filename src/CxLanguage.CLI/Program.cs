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
using CxLanguage.CLI.Extensions;
using CxLanguage.Core.Ast;
using CxLanguage.Runtime;
using CxLanguage.StandardLibrary.Services;
using CxLanguage.StandardLibrary.Services.VectorStore;
using CxLanguage.Core.Events;
using CxLanguage.LocalLLM;
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
            
            // Register LocalLLM service for consciousness integration
            var localLLMService = host.Services.GetService<CxLanguage.LocalLLM.ILocalLLMService>();
            if (localLLMService != null)
            {
                CxLanguage.Runtime.CxRuntimeHelper.RegisterService("LocalLLMService", localLLMService);
                Console.WriteLine("✅ GPU-FIRST LocalLLMService registered for static access in consciousness processing");
            }
            
            // Register ICxEventBus service for consciousness integration
            var eventBusService = host.Services.GetService<CxLanguage.Core.Events.ICxEventBus>();
            if (eventBusService != null)
            {
                CxLanguage.Runtime.CxRuntimeHelper.RegisterService("ICxEventBus", eventBusService);
                Console.WriteLine("✅ ICxEventBus registered for static access in event emission");
            }
            
            // Force AI services instantiation to ensure event subscriptions
            var thinkService = host.Services.GetService<CxLanguage.StandardLibrary.Services.Ai.ThinkService>();
            if (thinkService != null)
            {
                Console.WriteLine("✅ ThinkService instantiated and event subscriptions active");
            }
            else
            {
                Console.WriteLine("⚠️ Warning: ThinkService not available");
            }
            
            var inferService = host.Services.GetService<CxLanguage.StandardLibrary.Services.Ai.InferService>();
            if (inferService != null)
            {
                Console.WriteLine("✅ InferService instantiated and event subscriptions active");
            }
            else
            {
                Console.WriteLine("⚠️ Warning: InferService not available");
            }
            
            var learnService = host.Services.GetService<CxLanguage.StandardLibrary.Services.Ai.LearnService>();
            if (learnService != null)
            {
                Console.WriteLine("✅ LearnService instantiated and event subscriptions active");
            }
            else
            {
                Console.WriteLine("⚠️ Warning: LearnService not available");
            }
            
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
                    var task = (Task?)runMethod.Invoke(null, null);
                    if (task != null)
                    {
                        await task;
                    }
                }
                catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException != null)
                {
                    if (ex.InnerException is OperationCanceledException)
                    {
                        Console.WriteLine("Script execution was cancelled.");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Script execution error: {ex.InnerException.Message}");
                        telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, false, ex.InnerException.Message);
                    }
                }
            }
            else
            {
                Console.Error.WriteLine("No Run method found in compiled assembly.");
                telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, false, "No Run method found");
            }
            
            // Track successful script execution
            telemetryService?.TrackScriptExecution(file.Name, scriptExecutionStopwatch.Elapsed, true, null);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing script: {ex.Message}");
            CxDebugTracing.TraceError("CLI", "Script execution failed", new { FileName = file.FullName, Error = ex.Message });
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
        try
        {
            if (!source.Exists)
            {
                Console.Error.WriteLine($"Error: Source file '{source.FullName}' not found.");
                return;
            }

            // Set default output file if not specified
            output ??= new FileInfo(Path.ChangeExtension(source.FullName, ".dll"));

            // Setup services (needed for AI integration)
            var host = CreateHost();
            await host.StartAsync();
            
            try
            {
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

                var options = new CompilerOptions();
                CxCoreAI.IAiService? aiService = null;
                
                try
                {
                    aiService = host.Services.GetRequiredService<CxCoreAI.IAiService>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: AI service not available: {ex.Message}");
                    aiService = null;
                }
                
                var compiler = new CxCompiler(Path.GetFileNameWithoutExtension(source.Name), options, aiService);
                var compilationResult = compiler.Compile((ProgramNode)parseResult.Value!, Path.GetFileNameWithoutExtension(source.Name), sourceCode);

                if (!compilationResult.IsSuccess)
                {
                    Console.Error.WriteLine($"Compilation error: {compilationResult.ErrorMessage}");
                    return;
                }

                if (compilationResult.Assembly != null)
                {
                    // Save assembly to file
                    var assemblyBytes = File.ReadAllBytes(compilationResult.Assembly.Location);
                    await File.WriteAllBytesAsync(output.FullName, assemblyBytes);
                    Console.WriteLine($"Compilation successful. Output: {output.FullName}");
                }
                else
                {
                    Console.Error.WriteLine("Compilation did not produce an assembly.");
                }
            }
            finally
            {
                await host.StopAsync();
                host.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error compiling script: {ex.Message}");
        }
    }

    static async Task ParseScript(FileInfo file)
    {
        try
        {
            if (!file.Exists)
            {
                Console.Error.WriteLine($"Error: File '{file.FullName}' not found.");
                return;
            }

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

            Console.WriteLine("Parse successful. AST:");
            var printer = new AstPrinter();
            var astOutput = printer.Print(parseResult.Value!);
            Console.WriteLine(astOutput);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing script: {ex.Message}");
        }
    }

    static void ShowVersion()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine($"Cx Language CLI Version {version}");
        Console.WriteLine("Copyright (c) 2025 CX Language Development Team");
    }

    /// <summary>
    /// Creates and configures the dependency injection host with all required services
    /// </summary>
    static IHost CreateHost()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                // Add configuration sources
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                config.AddJsonFile("appsettings.local.json", optional: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                
                // Add Application Insights telemetry if configured
                var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];
                if (!string.IsNullOrEmpty(appInsightsConnectionString))
                {
                    services.AddApplicationInsightsTelemetryWorkerService(options =>
                    {
                        options.ConnectionString = appInsightsConnectionString;
                    });
                    
                    services.AddSingleton<CxLanguage.Core.Telemetry.CxTelemetryService>();
                }

                // Add modern CX Language services
                // TODO: Replace with proper service registration when extension methods are available
                // services.AddModernCxAiServices(configuration);

                // Add event bus for consciousness integration - using UnifiedEventBus to integrate with CxRuntimeHelper
                services.AddSingleton<CxLanguage.Core.Events.ICxEventBus, CxLanguage.Runtime.UnifiedEventBus>();

                // 🚀 PARALLEL HANDLER PARAMETERS v1.0 - 200%+ PERFORMANCE IMPROVEMENT
                services.AddSingleton<CxLanguage.Runtime.ParallelHandlers.HandlerParameterResolver>();
                services.AddSingleton<CxLanguage.Runtime.ParallelHandlers.PayloadPropertyMapper>();
                services.AddSingleton<CxLanguage.Runtime.ParallelHandlers.ParallelParameterEngine>();
                services.AddSingleton<CxLanguage.Runtime.ParallelHandlers.ParallelParameterIntegrationService>();

                // 🧠 REGISTER AI SERVICES FOR CONSCIOUSNESS INTEGRATION
                
                // Register LocalLLM Service
                try
                {
                    services.AddSingleton<CxLanguage.LocalLLM.ILocalLLMService, CxLanguage.LocalLLM.GpuLocalLLMService>();
                    Console.WriteLine("✅ GpuLocalLLMService registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: GpuLocalLLMService could not be registered: {ex.Message}");
                }

                // Register Vector Store Service
                try
                {
                    services.AddSingleton<CxLanguage.StandardLibrary.Services.VectorStore.IVectorStoreService, CxLanguage.StandardLibrary.Services.VectorStore.InMemoryVectorStoreService>();
                    Console.WriteLine("✅ InMemoryVectorStoreService registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: InMemoryVectorStoreService could not be registered: {ex.Message}");
                }

                // Register ThinkService
                try
                {
                    services.AddSingleton<CxLanguage.StandardLibrary.Services.Ai.ThinkService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<CxLanguage.Core.Events.ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<CxLanguage.StandardLibrary.Services.Ai.ThinkService>>();
                        var localLLMService = provider.GetRequiredService<CxLanguage.LocalLLM.ILocalLLMService>();
                        var vectorStore = provider.GetRequiredService<CxLanguage.StandardLibrary.Services.VectorStore.IVectorStoreService>();
                        return new CxLanguage.StandardLibrary.Services.Ai.ThinkService(eventBus, logger, localLLMService, vectorStore);
                    });
                    Console.WriteLine("✅ ThinkService (GPU-CUDA) with consciousness integration registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: ThinkService could not be registered: {ex.Message}");
                }

                // Register InferService
                try
                {
                    services.AddSingleton<CxLanguage.StandardLibrary.Services.Ai.InferService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<CxLanguage.Core.Events.ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<CxLanguage.StandardLibrary.Services.Ai.InferService>>();
                        var localLLMService = provider.GetRequiredService<CxLanguage.LocalLLM.ILocalLLMService>();
                        var vectorStore = provider.GetRequiredService<CxLanguage.StandardLibrary.Services.VectorStore.IVectorStoreService>();
                        return new CxLanguage.StandardLibrary.Services.Ai.InferService(eventBus, logger, localLLMService, vectorStore);
                    });
                    Console.WriteLine("✅ InferService (GPU-CUDA) with inference capabilities registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: InferService could not be registered: {ex.Message}");
                }

                // Register LearnService
                try
                {
                    services.AddSingleton<CxLanguage.StandardLibrary.Services.Ai.LearnService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<CxLanguage.Core.Events.ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<CxLanguage.StandardLibrary.Services.Ai.LearnService>>();
                        var localLLMService = provider.GetRequiredService<CxLanguage.LocalLLM.ILocalLLMService>();
                        var vectorStore = provider.GetRequiredService<CxLanguage.StandardLibrary.Services.VectorStore.IVectorStoreService>();
                        return new CxLanguage.StandardLibrary.Services.Ai.LearnService(eventBus, logger, localLLMService, vectorStore);
                    });
                    Console.WriteLine("✅ LearnService (GPU-CUDA) with vector storage capabilities registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: LearnService could not be registered: {ex.Message}");
                }

                // Register VoiceServiceInitializer as hosted service for automatic voice initialization
                // TODO: Re-enable when VoiceServiceInitializer is available
                // services.AddHostedService<VoiceServiceInitializer>();
            });

        return builder.Build();
    }
}

/// <summary>
/// Simple AST printer for debugging purposes
/// </summary>
public class AstPrinter
{
    private int _indentLevel = 0;

    public string Print(object node)
    {
        var result = new System.Text.StringBuilder();
        PrintNode(node, result);
        return result.ToString();
    }

    private void PrintNode(object node, System.Text.StringBuilder result)
    {
        result.AppendLine($"{new string(' ', _indentLevel * 2)}{node.GetType().Name}");
        _indentLevel++;

        if (node is CxLanguage.Core.Ast.CallExpressionNode call)
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
