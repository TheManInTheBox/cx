using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CxLanguage.Runtime.Visualization.Wpf.ViewModels;

namespace CxLanguage.Runtime.Visualization.Wpf;

/// <summary>
/// Main Window for CX Language Runtime Consciousness Peering Visualization
/// Features 3D interactive visualization of consciousness network peering
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        
        InitializeComponent();
        
        Title = "CX Language Runtime - Consciousness Peering Visualization";
        Width = 1200;
        Height = 800;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        // Set the 3D models container in the visualization engine
        ViewModel.SetModels3DContainer(Models3D);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ViewModel.Dispose();
    }
}
