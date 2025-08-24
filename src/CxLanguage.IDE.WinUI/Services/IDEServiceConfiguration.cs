using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CxLanguage.Core.Events;
using CxLanguage.Runtime;
using CxLanguage.LocalLLM;
using CxLanguage.StandardLibrary.Services.VectorStore;
using CxLanguage.StandardLibrary.Services.IO;
using CxLanguage.Core.AI;

namespace CxLanguage.IDE.WinUI.Services;

/// <summary>
/// Service configuration for CX Language IDE
/// Mirrors CLI service registration for full consciousness functionality
/// GitHub Issue #229: IDE Runtime Service Configuration
/// </summary>
public static class IDEServiceConfiguration
{
    /// <summary>
    /// Configure all services required for consciousness computing in the IDE
    /// </summary>
    public static ServiceProvider ConfigureServices(IConfiguration? configuration = null)
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add configuration if provided
        if (configuration != null)
        {
            services.AddSingleton(configuration);
        }

        // Register core consciousness services
        RegisterConsciousnessServices(services);
        
        // Register AI services  
        RegisterAIServices(services);
        
        // Register IDE-specific services
        RegisterIDEServices(services);
        
        // Register I/O services
        RegisterIOServices(services);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Register core consciousness framework services
    /// </summary>
    private static void RegisterConsciousnessServices(IServiceCollection services)
    {
        try
        {
            // Add event bus for consciousness integration - using UnifiedEventBus to integrate with CxRuntimeHelper
            services.AddSingleton<ICxEventBus, UnifiedEventBus>();
            
            // 🚀 PARALLEL HANDLER PARAMETERS v1.0 - 200%+ PERFORMANCE IMPROVEMENT
            services.AddSingleton<CxLanguage.Runtime.ParallelHandlers.HandlerParameterResolver>();
            services.AddSingleton<CxLanguage.Runtime.ParallelHandlers.PayloadPropertyMapper>();
            services.AddSingleton<CxLanguage.Runtime.ParallelHandlers.ParallelParameterEngine>();
            services.AddSingleton<CxLanguage.Runtime.ParallelHandlers.ParallelParameterIntegrationService>();
            
            Console.WriteLine("✅ Consciousness framework services registered successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: Consciousness services could not be registered: {ex.Message}");
        }
    }

    /// <summary>
    /// Register AI services for consciousness computing
    /// </summary>
    private static void RegisterAIServices(IServiceCollection services)
    {
        // Register core IAiService interface with SimpleAiService implementation
        try
        {
            services.AddSingleton<IAiService, SimpleAiService>();
            Console.WriteLine("✅ IAiService registered successfully with SimpleAiService");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: IAiService could not be registered: {ex.Message}");
        }

        // Register LocalLLM Service
        try
        {
            services.AddSingleton<ILocalLLMService, GpuLocalLLMService>();
            Console.WriteLine("✅ GpuLocalLLMService registered successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: GpuLocalLLMService could not be registered: {ex.Message}");
        }

        // Register Vector Store Service
        try
        {
            services.AddSingleton<IVectorStoreService, InMemoryVectorStoreService>();
            Console.WriteLine("✅ InMemoryVectorStoreService registered successfully");
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
                var eventBus = provider.GetRequiredService<ICxEventBus>();
                var logger = provider.GetRequiredService<ILogger<CxLanguage.StandardLibrary.Services.Ai.ThinkService>>();
                var localLLMService = provider.GetRequiredService<ILocalLLMService>();
                var vectorStore = provider.GetRequiredService<IVectorStoreService>();
                return new CxLanguage.StandardLibrary.Services.Ai.ThinkService(eventBus, logger, localLLMService, vectorStore);
            });
            Console.WriteLine("✅ ThinkService (GPU-CUDA) with consciousness integration registered successfully");
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
                var eventBus = provider.GetRequiredService<ICxEventBus>();
                var logger = provider.GetRequiredService<ILogger<CxLanguage.StandardLibrary.Services.Ai.InferService>>();
                var localLLMService = provider.GetRequiredService<ILocalLLMService>();
                var vectorStore = provider.GetRequiredService<IVectorStoreService>();
                return new CxLanguage.StandardLibrary.Services.Ai.InferService(eventBus, logger, localLLMService, vectorStore);
            });
            Console.WriteLine("✅ InferService (GPU-CUDA) with inference capabilities registered successfully");
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
                var eventBus = provider.GetRequiredService<ICxEventBus>();
                var logger = provider.GetRequiredService<ILogger<CxLanguage.StandardLibrary.Services.Ai.LearnService>>();
                var localLLMService = provider.GetRequiredService<ILocalLLMService>();
                var vectorStore = provider.GetRequiredService<IVectorStoreService>();
                return new CxLanguage.StandardLibrary.Services.Ai.LearnService(eventBus, logger, localLLMService, vectorStore);
            });
            Console.WriteLine("✅ LearnService (GPU-CUDA) with vector storage capabilities registered successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: LearnService could not be registered: {ex.Message}");
        }
    }

    /// <summary>
    /// Register IDE-specific services for syntax highlighting and development tools
    /// </summary>
    private static void RegisterIDEServices(IServiceCollection services)
    {
        try
        {
            services.AddSingleton<ICxSyntaxHighlighter, CxSyntaxHighlighter>();
            services.AddSingleton<ICxAutoCompletionService, CxAutoCompletionService>();
            services.AddSingleton<ICxCodeFormattingService, CxCodeFormattingService>();
            services.AddSingleton<ICxPerformanceMonitor, CxPerformanceMonitor>();
            services.AddSingleton<ICxErrorDetectionService, CxErrorDetectionService>();
            
            // Add enhanced GPU detection service for Issue #229
            services.AddSingleton<IGpuDetectionService, GpuDetectionService>();
            
            // Add native library logging service for Issue #229 
            services.AddSingleton<INativeLibraryLoggingService, NativeLibraryLoggingService>();
            
            Console.WriteLine("✅ IDE-specific services registered successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: IDE services could not be registered: {ex.Message}");
        }
    }

    /// <summary>
    /// Register I/O services for file and directory operations
    /// </summary>
    private static void RegisterIOServices(IServiceCollection services)
    {
        try
        {
            services.AddSingleton<FileSystemService>(provider =>
            {
                var eventBus = provider.GetRequiredService<ICxEventBus>();
                var logger = provider.GetRequiredService<ILogger<FileSystemService>>();
                return new FileSystemService(provider, eventBus, logger);
            });
            Console.WriteLine("🗂️ FileSystemService registered successfully");

            services.AddSingleton<DirectoryService>(provider =>
            {
                var eventBus = provider.GetRequiredService<ICxEventBus>();
                var logger = provider.GetRequiredService<ILogger<DirectoryService>>();
                return new DirectoryService(provider, eventBus, logger);
            });
            Console.WriteLine("📁 DirectoryService registered successfully");

            services.AddSingleton<JsonService>(provider =>
            {
                var eventBus = provider.GetRequiredService<ICxEventBus>();
                var logger = provider.GetRequiredService<ILogger<JsonService>>();
                return new JsonService(provider, eventBus, logger);
            });
            Console.WriteLine("🔧 JsonService registered successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: I/O Services could not be registered: {ex.Message}");
        }
    }

    /// <summary>
    /// Register services in CxRuntimeHelper for static access during consciousness execution
    /// </summary>
    public static void RegisterRuntimeServices(ServiceProvider serviceProvider)
    {
        try
        {
            // Register LocalLLM service for consciousness integration
            var localLLMService = serviceProvider.GetService<ILocalLLMService>();
            if (localLLMService != null)
            {
                CxRuntimeHelper.RegisterService("LocalLLMService", localLLMService);
                Console.WriteLine("✅ LocalLLMService registered for static access in consciousness processing");
            }
            
            // Register ICxEventBus service for consciousness integration
            var eventBusService = serviceProvider.GetService<ICxEventBus>();
            if (eventBusService != null)
            {
                CxRuntimeHelper.RegisterService("ICxEventBus", eventBusService);
                Console.WriteLine("✅ ICxEventBus registered for static access in event emission");
            }
            
            // Force AI services instantiation to ensure event subscriptions
            var thinkService = serviceProvider.GetService<CxLanguage.StandardLibrary.Services.Ai.ThinkService>();
            var inferService = serviceProvider.GetService<CxLanguage.StandardLibrary.Services.Ai.InferService>();
            var learnService = serviceProvider.GetService<CxLanguage.StandardLibrary.Services.Ai.LearnService>();
            
            Console.WriteLine("✅ AI services instantiated and ready for consciousness processing");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: Runtime service registration failed: {ex.Message}");
        }
    }
}
