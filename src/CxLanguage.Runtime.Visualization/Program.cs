using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows.Forms;
using CxLanguage.Core.Events;
using CxLanguage.Runtime; // UnifiedEventBus & CxRuntimeHelper
using CxLanguage.LocalLLM; // AddGpuLocalLlm & LocalLlmEventHandler
using CxLanguage.StandardLibrary.Services.VectorStore; // IVectorStoreService, InMemoryVectorStoreService
using CxLanguage.Runtime.Visualization.Services; // Real-time CX development services

namespace CxLanguage.Runtime.Visualization;

/// <summary>
/// Main application entry point for CX Consciousness Visualization
/// </summary>
public static class Program
{
    private static IHost? _host;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        try
        {
            // Build and start the host (CX runtime daemon inside this process)
            _host = CreateHostBuilder().Build();
            _host.StartAsync();

            // Resolve core runtime services
            var services = _host.Services;
            var eventBus = services.GetRequiredService<ICxEventBus>();
            var vectorStore = services.GetService<IVectorStoreService>();

            // Ensure Local LLM event handler is created so it subscribes to llm.* events
            services.GetService<LocalLlmEventHandler>();

            // Pre-load the LLM service before showing the main form to avoid race conditions
            var llmService = services.GetService<ILocalLLMService>();
            if (llmService != null)
            {
                try
                {
                    // Block and wait for the LLM to be fully initialized.
                    // This prevents the UI from starting until the LLM is ready.
                    llmService.InitializeAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to initialize Local LLM: {ex.Message}\n\nCode completion will not be available.", 
                        "CX Consciousness Visualization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Register commonly used services into CxRuntimeHelper for global/static access
            CxRuntimeHelper.RegisterService("ICxEventBus", eventBus);
            if (vectorStore != null)
            {
                CxRuntimeHelper.RegisterService("IVectorStoreService", vectorStore);
            }

            // Run the main form with an explicit event bus so UI is live-connected
            Application.Run(new MainForm(eventBus));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start application: {ex.Message}", "CX Consciousness Visualization", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _host?.StopAsync();
            _host?.Dispose();
        }
    }

    /// <summary>
    /// Creates and configures the application host with dependency injection
    /// </summary>
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Minimal logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // CX Runtime daemon services
                // - Unified Event Bus as the ICxEventBus implementation
                services.AddSingleton<ICxEventBus, UnifiedEventBus>();

                // - Vector store for ingestion and reasoning (in-memory for now)
                services.AddSingleton<IVectorStoreService, InMemoryVectorStoreService>();

                // - Local LLM (GPU-first) and its event handler (subscribes to llm.*)
                services.AddGpuLocalLlm();

                // - Real-time CX development services
                services.AddSingleton<RealTimeCxCompilerSimple>();
                services.AddSingleton<AuraCxReferenceIngestor>();
                services.AddSingleton<IntelligentCxCodeGenerator>();
            });
    }

    /// <summary>
    /// Gets the current host instance
    /// </summary>
    public static IHost? Current => _host;

    /// <summary>
    /// Gets a service from the current host
    /// </summary>
    public static T? GetService<T>() where T : class
        => Current?.Services?.GetService<T>();
}
