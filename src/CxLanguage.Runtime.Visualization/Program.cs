using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows.Forms;

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
            // Build and start the host
            _host = CreateHostBuilder().Build();
            _host.StartAsync();

            // Run the main form
            Application.Run(new MainForm());
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
