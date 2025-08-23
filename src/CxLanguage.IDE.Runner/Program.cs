using CxLanguage.IDE.DockedEditor;
using CxLanguage.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CxLanguage.IDE.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 CX Language Docked IDE - Starting...\n");
            
            // Set up dependency injection
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSingleton<ICxEventBus>(provider => 
            {
                var logger = provider.GetRequiredService<ILogger<CxEventBus>>();
                return new CxEventBus(logger);
            });
            services.AddSingleton<CXDockedIDE>();

            var serviceProvider = services.BuildServiceProvider();
            var ide = serviceProvider.GetRequiredService<CXDockedIDE>();
            
            await ide.StartAsync();
        }
    }
}
