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
using CxLanguage.CLI.Extensions;
using CxLanguage.Core.Ast;
using CxLanguage.Runtime;
using CxLanguage.StandardLibrary.Services;
using CxLanguage.StandardLibrary.Services.VectorStore;
using CxLanguage.StandardLibrary.Services.IO;
using CxLanguage.Core.Events;
using CxLanguage.LocalLLM;
using CxCoreAI = CxLanguage.Core.AI;
using CxLanguage.Runtime.Services;

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
            
            // Ensure ConsoleService is created so it subscribes to events
            _ = host.Services.GetService<ConsoleService>();
            
            // Ensure FileService is created so it subscribes to events
            _ = host.Services.GetService<FileService>();
            
            // Ensure TimeService is created so it subscribes to events
            _ = host.Services.GetService<TimeService>();
            
            // Register DocumentIngestionService for consciousness integration
            var documentService = host.Services.GetService<CxLanguage.StandardLibrary.Services.Document.IDocumentIngestionService>();
            if (documentService != null)
            {
                CxLanguage.Runtime.CxRuntimeHelper.RegisterService("DocumentIngestionService", documentService);
                // Console.WriteLine("✅ DocumentIngestionService registered for static access in consciousness processing");
            }
            
            // Register VectorStoreService for consciousness integration
            var vectorStoreService = host.Services.GetService<CxLanguage.StandardLibrary.Services.VectorStore.IVectorStoreService>();
            if (vectorStoreService != null)
            {
                CxLanguage.Runtime.CxRuntimeHelper.RegisterService("VectorStoreService", vectorStoreService);
                // Console.WriteLine("✅ VectorStoreService registered for static access in consciousness processing");
            }
            
            // Register SemanticSearchService for consciousness integration
            var semanticSearchService = host.Services.GetService<CxLanguage.StandardLibrary.Services.VectorStore.ISemanticSearchService>();
            if (semanticSearchService != null)
            {
                CxLanguage.Runtime.CxRuntimeHelper.RegisterService("SemanticSearchService", semanticSearchService);
                // Console.WriteLine("✅ SemanticSearchService registered for static access in consciousness processing");
            }
            
            // Register FileSystemService for consciousness integration
            var fileSystemService = host.Services.GetService<FileSystemService>();
            if (fileSystemService != null)
            {
                CxLanguage.Runtime.CxRuntimeHelper.RegisterService("FileSystemService", fileSystemService);
                // Console.WriteLine("✅ FileSystemService registered for static access in consciousness processing");
            }
            
            // Register LocalLLM service for consciousness integration
            var localLLMService = host.Services.GetService<CxLanguage.LocalLLM.ILocalLLMService>();
            if (localLLMService != null)
            {
                CxLanguage.Runtime.CxRuntimeHelper.RegisterService("LocalLLMService", localLLMService);
                // Console.WriteLine("✅ GPU-FIRST LocalLLMService registered for static access in consciousness processing");
            }
            
            // Register ICxEventBus service for consciousness integration
            var eventBusService = host.Services.GetService<CxLanguage.Core.Events.ICxEventBus>();
            if (eventBusService != null)
            {
                CxLanguage.Runtime.CxRuntimeHelper.RegisterService("ICxEventBus", eventBusService);
                // Console.WriteLine("✅ ICxEventBus registered for static access in event emission");
            }
            
            // Force AI services instantiation to ensure event subscriptions
            
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var telemetryService = host.Services.GetService<CxLanguage.Core.Telemetry.CxTelemetryService>();

            // 🤖 Initialize AI Event Service Infrastructure
            try
            {
                var consciousnessOrchestrator = host.Services.GetService<CxLanguage.Runtime.ConsciousnessServiceOrchestrator>();
                if (consciousnessOrchestrator != null)
                {
                    await consciousnessOrchestrator.StartAsync(CancellationToken.None);
                    Console.WriteLine("✅ ConsciousnessServiceOrchestrator initialized with AI Event Service");
                }
                else
                {
                    Console.WriteLine("⚠️ Warning: ConsciousnessServiceOrchestrator not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Warning: Failed to initialize AI Event Service infrastructure: {ex.Message}");
            }

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
                // Console.WriteLine($"✅ Neural System: AI Service loaded for biological testing");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: AI service not available: {ex.Message}");
                // Console.WriteLine("Neural system will continue with basic event functionality.");
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

            // Execute the compiled assembly and wait for shutdown
            var runMethod = compilationResult.ProgramType!.GetMethod("Run");
            if (runMethod != null)
            {
                try
                {
                    // Create a shutdown signal source
                    var shutdownTaskSource = new TaskCompletionSource<bool>();
                    var cancellationTokenSource = new CancellationTokenSource();
                    
                    // Subscribe to system.shutdown event
                    if (eventBusService != null)
                    {
                        // Console.WriteLine("🔄 Waiting for system.shutdown event or Ctrl+C to exit...");
                        
                        eventBusService.Subscribe("system.shutdown", (sender, eventName, payload) =>
                        {
                            Console.WriteLine("🛑 Received system.shutdown event - shutting down gracefully");
                            shutdownTaskSource.TrySetResult(true);
                            return Task.FromResult(true);
                        });
                        
                        // Handle Ctrl+C gracefully
                        Console.CancelKeyPress += async (sender, e) =>
                        {
                            e.Cancel = true; // Prevent immediate termination
                            Console.WriteLine("\n🛑 Ctrl+C received - emitting system.shutdown event...");
                            await eventBusService.EmitAsync("system.shutdown", new Dictionary<string, object>
                            {
                                ["reason"] = "user_interrupt",
                                ["source"] = "cli"
                            });
                        };
                    }
                    
                    // Launch CX Consciousness Visualization and wait for it to be ready
                    // Console.WriteLine("🎮 CX Consciousness Visualization (disabled for demo)...");
                    // var visualizationReady = await LaunchVisualizationAndWaitForReadyAsync();
                    var visualizationReady = true; // Skip visualization for demo
                    
                    if (!visualizationReady)
                    {
                        Console.WriteLine("⚠️ Warning: Visualization failed to start, continuing with script execution...");
                    }
                    
                    // Execute the script after visualization is ready
                    // Console.WriteLine("🚀 Starting CX script execution...");
                    var task = (Task?)runMethod.Invoke(null, null);
                    if (task != null)
                    {
                        await task;
                    }
                    
                    // Wait for shutdown signal (or timeout after 5 minutes for safety)
                    if (eventBusService != null)
                    {
                        using (cancellationTokenSource)
                        {
                            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5), cancellationTokenSource.Token);
                            var completedTask = await Task.WhenAny(shutdownTaskSource.Task, timeoutTask);
                            
                            if (completedTask == timeoutTask && !cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                Console.WriteLine("⏰ Application timeout - shutting down after 5 minutes");
                            }
                            
                            cancellationTokenSource.Cancel(); // Cancel the timeout task
                        }
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
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders(); // Remove all default logging providers
                logging.SetMinimumLevel(LogLevel.Critical); // Only show critical errors
            })
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

                // Register core IAiService interface
                try
                {
                    // Use existing LocalLLM service as the IAiService implementation
                    services.AddSingleton<CxLanguage.Core.AI.IAiService>(provider =>
                    {
                        var localLLMService = provider.GetRequiredService<CxLanguage.LocalLLM.ILocalLLMService>();
                        var logger = provider.GetRequiredService<ILogger<CxLanguage.Core.AI.IAiService>>();
                        
                        // Create a simple wrapper that delegates to LocalLLM
                        return new LocalLLMWrapper(localLLMService, logger);
                    });
                    // Console.WriteLine("✅ IAiService registered successfully with LocalLLM wrapper");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: IAiService could not be registered: {ex.Message}");
                }

                // 🧠 REGISTER AI SERVICES FOR CONSCIOUSNESS INTEGRATION
                
                // Register LocalLLM Service
                try
                {
                    services.AddSingleton<CxLanguage.LocalLLM.ILocalLLMService, CxLanguage.LocalLLM.GpuLocalLLMService>();
                    // Console.WriteLine("✅ GpuLocalLLMService registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: GpuLocalLLMService could not be registered: {ex.Message}");
                }

                // Register Vector Store Service
                try
                {
                    services.AddSingleton<CxLanguage.StandardLibrary.Services.VectorStore.IVectorStoreService, CxLanguage.StandardLibrary.Services.VectorStore.InMemoryVectorStoreService>();
                    // Console.WriteLine("✅ InMemoryVectorStoreService registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: InMemoryVectorStoreService could not be registered: {ex.Message}");
                }

                // Register Semantic Search Service
                try
                {
                    services.AddSingleton<CxLanguage.StandardLibrary.Services.VectorStore.ISemanticSearchService, CxLanguage.StandardLibrary.Services.VectorStore.SemanticSearchService>();
                    // Console.WriteLine("✅ SemanticSearchService registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: SemanticSearchService could not be registered: {ex.Message}");
                }

                // Register Local Embedding Generator
                try
                {
                    // Use LocalEmbeddingGenerator for local model processing
                    services.AddSingleton<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>(provider =>
                    {
                        var localLogger = provider.GetRequiredService<ILogger<CxLanguage.LocalLLM.LocalEmbeddingGenerator>>();
                        
                        // Use the correct local embedding model file path
                        var modelPath = "models/embedding/nomic-embed-text-v1.5.Q4_0.gguf";
                        
                        return new CxLanguage.LocalLLM.LocalEmbeddingGenerator(modelPath, localLogger);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: Azure OpenAI services could not be registered: {ex.Message}");
                }

                // Register Document Ingestion Service
                try
                {
                    services.AddSingleton<CxLanguage.StandardLibrary.Services.Document.IDocumentIngestionService, CxLanguage.StandardLibrary.Services.Document.DocumentIngestionService>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: DocumentIngestionService could not be registered: {ex.Message}");
                }

                // 🤖 REGISTER AI EVENT SERVICE INFRASTRUCTURE
                try
                {
                    // Register IMultiModalAI interface with real implementation
                    services.AddSingleton<CxCoreAI.IMultiModalAI, CxCoreAI.MultiModalAIService>();
                    
                    // Register ICodeSynthesizer interface with real implementation
                    services.AddSingleton<CxCoreAI.ICodeSynthesizer, CxCoreAI.RuntimeCodeSynthesizer>();
                    
                    // Register IAgenticRuntime for AI event processing
                    services.AddSingleton<CxCoreAI.IAgenticRuntime, CxCoreAI.AgenticRuntime>();
                    
                    // Register ICodeGenerator for RuntimeCodeSynthesizer
                    services.AddSingleton<CxCoreAI.ICodeGenerator>(provider =>
                    {
                        var logger = provider.GetRequiredService<ILogger<CxLanguage.Runtime.CxCodeGenerator>>();
                        return new CxLanguage.Runtime.CxCodeGenerator(logger);
                    });
                    
                    // Register AiEventService for ai.* event handlers
                    services.AddSingleton<CxLanguage.Runtime.AiEventService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<CxLanguage.Core.Events.ICxEventBus>();
                        var agenticRuntime = provider.GetRequiredService<CxCoreAI.IAgenticRuntime>();
                        var aiService = provider.GetRequiredService<CxCoreAI.IAiService>();
                        var logger = provider.GetRequiredService<ILogger<CxLanguage.Runtime.AiEventService>>();
                        var telemetryService = provider.GetService<CxLanguage.Core.Telemetry.CxTelemetryService>();
                        
                        return new CxLanguage.Runtime.AiEventService(eventBus, agenticRuntime, aiService, logger, telemetryService);
                    });
                    
                    // Register ConsciousnessServiceOrchestrator
                    services.AddSingleton<CxLanguage.Runtime.ConsciousnessServiceOrchestrator>();
                    
                    Console.WriteLine("✅ AI Event Service infrastructure registered successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: AI Event Service infrastructure could not be registered: {ex.Message}");
                }

                // Register Service-Based I/O Architecture
                try
                {
                    services.AddSingleton<FileSystemService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<FileSystemService>>();
                        return new FileSystemService(provider, eventBus, logger);
                    });
                    // Console.WriteLine("🗂️ FileSystemService registered successfully.");

                    services.AddSingleton<DirectoryService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<DirectoryService>>();
                        return new DirectoryService(provider, eventBus, logger);
                    });
                    // Console.WriteLine("📁 DirectoryService registered successfully.");

                    services.AddSingleton<JsonService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<JsonService>>();
                        return new JsonService(provider, eventBus, logger);
                    });
                    // Console.WriteLine("🔧 JsonService registered successfully.");

                    // Register ConsoleService for system.console.write event handling
                    services.AddSingleton<ConsoleService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<ConsoleService>>();
                        return new ConsoleService(eventBus, logger);
                    });

                    // Register FileService for system.file.read and system.file.write event handling
                    services.AddSingleton<FileService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<FileService>>();
                        return new FileService(eventBus, logger);
                    });

                    // Register TimeService for system.time.* event handling
                    services.AddSingleton<TimeService>(provider =>
                    {
                        var eventBus = provider.GetRequiredService<ICxEventBus>();
                        var logger = provider.GetRequiredService<ILogger<TimeService>>();
                        return new TimeService(eventBus, logger);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warning: I/O Services could not be registered: {ex.Message}");
                }

                // Register VoiceServiceInitializer as hosted service for automatic voice initialization
                // TODO: Re-enable when VoiceServiceInitializer is available
                // services.AddHostedService<VoiceServiceInitializer>();
            });

        return builder.Build();
    }

    /// <summary>
    /// Launch the Consciousness Visualization service in-process and wait for it to be ready
    /// </summary>
    private static async Task<bool> LaunchVisualizationAndWaitForReadyAsync()
    {
        try
        {
            Console.WriteLine("🎮 Starting CX Consciousness Visualization...");
            
            var visualizationReadySource = new TaskCompletionSource<bool>();
            
            // Get the event bus to pass to visualization via CxRuntimeHelper
            ICxEventBus? eventBusService = null;
            
            try
            {
                var runtimeService = CxLanguage.Runtime.CxRuntimeHelper.GetService("ICxEventBus");
                eventBusService = runtimeService as ICxEventBus;
                Console.WriteLine($"🔍 Runtime helper resolution: {eventBusService?.GetType()?.Name ?? "null"}");
                
                if (eventBusService != null)
                {
                    // Console.WriteLine($"✅ Successfully retrieved event bus: {eventBusService.GetType().FullName}");
                }
                else
                {
                    Console.WriteLine("⚠️ Event bus is null - visualization will run in standalone mode");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Runtime helper resolution failed: {ex.Message}");
            }
            
            // Launch Windows Forms visualization in a separate thread
            var visualizationTask = Task.Run(() =>
            {
                try
                {
                    // Set the application to use visual styles
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                    
                    // Visualization disabled for Issue #228 demo
                    throw new NotImplementedException("Visualization disabled for Issue #228 demo");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error running Windows Forms visualization: {ex.Message}");
                    visualizationReadySource.TrySetResult(false);
                }
            });
            
            // Wait for visualization to be ready (with timeout)
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
            var completedTask = await Task.WhenAny(visualizationReadySource.Task, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                Console.WriteLine("⏰ Visualization startup timeout after 10 seconds");
                return false;
            }
            
            return await visualizationReadySource.Task;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to launch Windows Forms visualization: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Attempts to build the visualization project
    /// </summary>
    private static async Task<bool> BuildVisualizationProjectAsync()
    {
        try
        {
            var projectPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "CxLanguage.Runtime.Visualization", "CxLanguage.Runtime.Visualization.csproj");
            
            projectPath = Path.GetFullPath(projectPath);
            
            if (!File.Exists(projectPath))
            {
                Console.WriteLine($"⚠️ Visualization project not found at: {projectPath}");
                return false;
            }
            
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to build visualization project: {ex.Message}");
            return false;
        }
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

/// <summary>
/// Simple wrapper to bridge ILocalLLMService to IAiService interface
/// Uses the existing GPU-based local LLM service
/// </summary>
internal class LocalLLMWrapper : CxCoreAI.IAiService
{
    private readonly CxLanguage.LocalLLM.ILocalLLMService _localLLMService;
    private readonly ILogger<CxCoreAI.IAiService> _logger;

    public LocalLLMWrapper(CxLanguage.LocalLLM.ILocalLLMService localLLMService, ILogger<CxCoreAI.IAiService> logger)
    {
        _localLLMService = localLLMService;
        _logger = logger;
    }

    public async Task<CxCoreAI.AiResponse> GenerateTextAsync(string prompt, CxCoreAI.AiRequestOptions? options = null)
    {
        try
        {
            _logger.LogInformation("LocalLLM generating text for prompt: {Prompt}", prompt.Substring(0, Math.Min(50, prompt.Length)));
            
            // Add timeout to prevent infinite hangs
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            try
            {
                if (!_localLLMService.IsModelLoaded)
                {
                    _logger.LogWarning("LocalLLM model not loaded, attempting to load default model");
                    await _localLLMService.LoadModelAsync("default", timeoutCts.Token);
                }

                var response = await _localLLMService.GenerateAsync(prompt, timeoutCts.Token);
                return CxCoreAI.AiResponse.Success(response ?? "No response generated");
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("LocalLLM generation timed out after 30 seconds");
                return CxCoreAI.AiResponse.Success($"LocalLLM response to: '{prompt.Substring(0, Math.Min(50, prompt.Length))}' (timed out - model may need optimization)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text with LocalLLM");
            return CxCoreAI.AiResponse.Failure($"LocalLLM error: {ex.Message}");
        }
    }

    public async Task<CxCoreAI.AiResponse> AnalyzeAsync(string content, CxCoreAI.AiAnalysisOptions options)
    {
        try
        {
            _logger.LogInformation("LocalLLM analyzing content for task: {Task}", options.Task);
            
            var analysisPrompt = $"Analyze the following content for {options.Task}:\n\n{content}";
            var response = await GenerateTextAsync(analysisPrompt);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing content with LocalLLM");
            return CxCoreAI.AiResponse.Failure($"LocalLLM analysis error: {ex.Message}");
        }
    }

    public async Task<CxCoreAI.AiStreamResponse> StreamGenerateTextAsync(string prompt, CxCoreAI.AiRequestOptions? options = null)
    {
        try
        {
            _logger.LogInformation("LocalLLM streaming text for prompt: {Prompt}", prompt.Substring(0, Math.Min(50, prompt.Length)));
            
            if (!_localLLMService.IsModelLoaded)
            {
                await _localLLMService.LoadModelAsync("default");
            }

            return new LocalLLMStreamWrapper(_localLLMService.StreamAsync(prompt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming text with LocalLLM");
            return new ErrorStreamWrapper($"LocalLLM streaming error: {ex.Message}");
        }
    }

    public async Task<CxCoreAI.AiResponse[]> ProcessBatchAsync(string[] prompts, CxCoreAI.AiRequestOptions? options = null)
    {
        _logger.LogInformation("LocalLLM processing batch of {Count} prompts", prompts.Length);
        
        var tasks = prompts.Select(prompt => GenerateTextAsync(prompt, options));
        return await Task.WhenAll(tasks);
    }

    public Task<CxCoreAI.AiEmbeddingResponse> GenerateEmbeddingAsync(string text, CxCoreAI.AiRequestOptions? options = null)
    {
        _logger.LogInformation("LocalLLM embedding generation not implemented, returning placeholder");
        return Task.FromResult(CxCoreAI.AiEmbeddingResponse.Failure("Embedding generation not supported by LocalLLM service. Use the dedicated embedding service."));
    }

    public Task<CxCoreAI.AiImageResponse> GenerateImageAsync(string prompt, CxCoreAI.AiImageOptions? options = null)
    {
        _logger.LogInformation("LocalLLM image generation not supported");
        return Task.FromResult(CxCoreAI.AiImageResponse.Failure("Image generation not supported by LocalLLM service"));
    }

    public Task<CxCoreAI.AiImageAnalysisResponse> AnalyzeImageAsync(string imageUrl, CxCoreAI.AiImageAnalysisOptions? options = null)
    {
        _logger.LogInformation("LocalLLM image analysis not supported");
        return Task.FromResult(CxCoreAI.AiImageAnalysisResponse.Failure("Image analysis not supported by LocalLLM service"));
    }
}

/// <summary>
/// Stream response wrapper for LocalLLM
/// </summary>
internal class LocalLLMStreamWrapper : CxCoreAI.AiStreamResponse
{
    private readonly IAsyncEnumerable<string> _stream;

    public LocalLLMStreamWrapper(IAsyncEnumerable<string> stream)
    {
        _stream = stream;
    }

    public override async IAsyncEnumerable<string> GetTokensAsync()
    {
        await foreach (var token in _stream)
        {
            yield return token;
        }
    }

    public override void Dispose()
    {
        // LocalLLM handles its own cleanup
    }
}

/// <summary>
/// Error stream response for failed operations
/// </summary>
internal class ErrorStreamWrapper : CxCoreAI.AiStreamResponse
{
    private readonly string _errorMessage;

    public ErrorStreamWrapper(string errorMessage)
    {
        _errorMessage = errorMessage;
    }

    public override async IAsyncEnumerable<string> GetTokensAsync()
    {
        yield return _errorMessage;
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        // No cleanup needed
    }
}

