using System;
using System.Windows;

namespace CxLanguage.IDE.WinUI
{
    /// <summary>
    /// CX Language IDE WPF + DirectX Hybrid Application
    /// Provides consciousness computing development environment with GPU acceleration
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Application entry point for CX Language IDE
        /// </summary>
        public App()
        {
            // WPF will automatically call InitializeComponent() for partial classes
        }

        /// <summary>
        /// Handle application startup
        /// </summary>
        /// <param name="e">Startup event arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Application will automatically show MainWindow due to StartupUri in App.xaml
        }

        /// <summary>
        /// Handle unhandled exceptions
        /// </summary>
        /// <param name="sender">Source of the exception</param>
        /// <param name="e">Exception event arguments</param>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}", 
                           "CX Language IDE Error", 
                           MessageBoxButton.OK, 
                           MessageBoxImage.Error);
            
            e.Handled = true;
        }
    }
}
