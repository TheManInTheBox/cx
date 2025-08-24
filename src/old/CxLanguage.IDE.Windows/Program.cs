using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CxLanguage.Core;
using CxLanguage.Parser;
using CxLanguage.Compiler;

namespace CxLanguage.IDE.Windows;

/// <summary>
/// CX Language IDE for Windows with advanced syntax highlighting and compiler integration
/// Implements GitHub Issue #220 acceptance criteria
/// </summary>
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        // Register CX Language services
        services.AddSingleton<ICxLanguageParser, CxLanguageParserService>();
        services.AddSingleton<ICxCompilerService, CxCompilerService>();
        services.AddSingleton<ICxSyntaxHighlighter, CxSyntaxHighlighter>();
        services.AddSingleton<ICxAutoCompleteProvider, CxAutoCompleteProvider>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Create and run the main IDE form
        var mainForm = new CxLanguageIDEForm(serviceProvider);
        Application.Run(mainForm);
    }
}
