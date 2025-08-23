using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CxLanguage.Runtime.Visualization.Wpf.Services;
using CxLanguage.Runtime.Visualization.Wpf.ViewModels;

namespace CxLanguage.Runtime.Visualization.Wpf;

/// <summary>
/// WPF Application for CX Language Runtime Consciousness Peering Visualization
/// Provides 3D interactive visualization of consciousness network peering
/// </summary>
public partial class App : Application
{
    private IHost _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.AddSingleton<IConfiguration>(provider =>
                {
                    return new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();
                });

                // Services
                services.AddSingleton<IPeeringDataService, PeeringDataService>();
                services.AddSingleton<IConsciousnessNetworkService, ConsciousnessNetworkService>();
                services.AddSingleton<IVisualization3DEngine, Visualization3DEngine>();

                // ViewModels
                services.AddTransient<MainWindowViewModel>();

                // Windows
                services.AddTransient<MainWindow>();

                // Logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });
            })
            .Build();

        // Start services
        _ = _host.StartAsync();

        // Show main window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.StopAsync().Wait();
        _host?.Dispose();
        base.OnExit(e);
    }
}
