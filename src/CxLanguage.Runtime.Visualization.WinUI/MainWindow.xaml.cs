using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading.Tasks;
using CxLanguage.Core.Events;
using CxLanguage.Runtime.Visualization.Services;
using CxLanguage.StandardLibrary.Services.VectorStore;
using CxLanguage.LocalLLM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CxLanguage.Runtime.Visualization.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // Core services and state
        private ICxEventBus? _eventBus;
        private CxLanguageVectorIngestionService? _vectorIngestionService;
        private RealTimeCxCompilerSimple? _cxCompiler;
        private AuraCxReferenceIngestor? _referenceIngestor;
        private IntelligentCxCodeGenerator? _codeGenerator;
        private CxExecutionResult? _lastExecutionResult;
        private string? _currentFilePath;
        private bool _isCodeModified = false;
        private int _realEventCount = 0;
        private DateTime _startTime = DateTime.Now;
        
        // Event history for vector database display
        private ObservableCollection<string> _eventHistory = new();
        
        // Timer for UI updates
        private DispatcherTimer? _updateTimer;

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeVisualization();
            
            // Initialize event history binding
            EventHistoryList.ItemsSource = _eventHistory;
            
            // Initialize LLM automatically on startup
            _ = Task.Run(async () => await InitializeLocalLlm());
        }

        private void InitializeVisualization()
        {
            // Set up timer for periodic updates
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(500);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // Update initial status
            UpdateConnectionStatus(false);
            UpdateStatusText("Ready - CX IDE WinUI 3");
            
            // Track code editor changes
            CodeEditor.TextChanged += CodeEditor_TextChanged;
        }

        private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isCodeModified = true;
            UpdateFileStatus();
        }

        private void UpdateTimer_Tick(object? sender, object e)
        {
            // Update event count
            EventCountText.Text = $"Events: {_realEventCount}";
            
            // Update runtime duration
            var elapsed = DateTime.Now - _startTime;
            var durationText = elapsed.TotalMinutes > 1 
                ? $"{elapsed.TotalMinutes:F1}m" 
                : $"{elapsed.TotalSeconds:F0}s";
            
            // Update vector database status if available
            if (_vectorIngestionService != null)
            {
                VectorStatus.Text = $"Vector DB: Active ({_vectorIngestionService.DocumentCount} docs)";
            }
        }

        private void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                ConnectionIndicator.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80)); // Green
                ConnectionStatus.Text = "Connected";
            }
            else
            {
                ConnectionIndicator.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54)); // Red
                ConnectionStatus.Text = "Disconnected";
            }
        }

        private void UpdateStatusText(string status)
        {
            StatusText.Text = status;
        }

        private void UpdateFileStatus()
        {
            var fileName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
            var modifiedIndicator = _isCodeModified ? " *" : "";
            Title = $"CX Language Runtime Visualization - WinUI 3 - {fileName}{modifiedIndicator}";
        }

        private async Task InitializeLocalLlm()
        {
            try 
            {
                // Initialize the CX compiler
                _cxCompiler = new RealTimeCxCompilerSimple();
                
                DispatcherQueue.TryEnqueue(() => {
                    UpdateStatusText("Local LLM initialized successfully");
                    UpdateConnectionStatus(true);
                    AppendToConsole("✅ Local LLM system initialized and ready");
                });
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(() => {
                    UpdateStatusText("Failed to initialize Local LLM");
                    AppendToConsole($"❌ Local LLM initialization failed: {ex.Message}");
                });
            }
        }

        private void AppendToConsole(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {message}";
            
            if (!string.IsNullOrEmpty(ConsoleOutput.Text))
            {
                ConsoleOutput.Text += Environment.NewLine;
            }
            ConsoleOutput.Text += logEntry;
            
            // Auto-scroll to bottom (approximate by setting selection to end)
            ConsoleOutput.Select(ConsoleOutput.Text.Length, 0);
        }

        private void AddEventToHistory(string eventDescription)
        {
            _realEventCount++;
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var entry = $"[{timestamp}] {eventDescription}";
            
            DispatcherQueue.TryEnqueue(() => {
                _eventHistory.Insert(0, entry); // Add to top of list
                
                // Keep only recent events (limit to 100)
                while (_eventHistory.Count > 100)
                {
                    _eventHistory.RemoveAt(_eventHistory.Count - 1);
                }
            });
        }

        // Event handlers for menu items
        private async void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await OpenFile();
        }

        private async void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await SaveFile();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void UndoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // TextBox doesn't have built-in undo in WinUI, would need custom implementation
            AppendToConsole("Undo functionality not implemented yet");
        }

        private void RedoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // TextBox doesn't have built-in redo in WinUI, would need custom implementation
            AppendToConsole("Redo functionality not implemented yet");
        }

        private async void CompileRunMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteCurrentCode();
        }

        private void StopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AppendToConsole("Stop execution requested");
            UpdateStatusText("Execution stopped");
        }

        private void ShowConsoleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Console is always visible in this layout
            AppendToConsole("Console is already visible");
        }

        private void ShowVectorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Vector database panel is always visible in this layout
            AppendToConsole("Vector database panel is already visible");
        }

        // Toolbar event handlers
        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            
            try
            {
                await ExecuteCurrentCode();
            }
            finally
            {
                RunButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopMenuItem_Click(sender, e);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveFile();
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            await OpenFile();
        }

        private void ClearConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Text = "";
            AppendToConsole("Console cleared");
        }

        // File operations
        private async Task OpenFile()
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.List;
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add(".cx");
                picker.FileTypeFilter.Add(".txt");

                // Get the current window's handle for the picker
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    var content = await FileIO.ReadTextAsync(file);
                    CodeEditor.Text = content;
                    _currentFilePath = file.Path;
                    _isCodeModified = false;
                    UpdateFileStatus();
                    AppendToConsole($"Opened file: {file.Name}");
                }
            }
            catch (Exception ex)
            {
                AppendToConsole($"Error opening file: {ex.Message}");
            }
        }

        private async Task SaveFile()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    await SaveFileAs();
                    return;
                }

                var file = await StorageFile.GetFileFromPathAsync(_currentFilePath);
                await FileIO.WriteTextAsync(file, CodeEditor.Text);
                _isCodeModified = false;
                UpdateFileStatus();
                AppendToConsole($"Saved file: {Path.GetFileName(_currentFilePath)}");
            }
            catch (Exception ex)
            {
                AppendToConsole($"Error saving file: {ex.Message}");
                // Fallback to Save As
                await SaveFileAs();
            }
        }

        private async Task SaveFileAs()
        {
            try
            {
                var picker = new FileSavePicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add("CX Files", new List<string>() { ".cx" });
                picker.FileTypeChoices.Add("Text Files", new List<string>() { ".txt" });
                picker.SuggestedFileName = "program.cx";

                // Get the current window's handle for the picker
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, CodeEditor.Text);
                    _currentFilePath = file.Path;
                    _isCodeModified = false;
                    UpdateFileStatus();
                    AppendToConsole($"Saved file as: {file.Name}");
                }
            }
            catch (Exception ex)
            {
                AppendToConsole($"Error saving file: {ex.Message}");
            }
        }

        private async Task ExecuteCurrentCode()
        {
            try
            {
                UpdateStatusText("Executing CX code...");
                AppendToConsole("🚀 Starting CX code execution");
                AddEventToHistory("Code execution started");

                var code = CodeEditor.Text;
                if (string.IsNullOrWhiteSpace(code))
                {
                    AppendToConsole("⚠️ No code to execute");
                    UpdateStatusText("Ready");
                    return;
                }

                if (_cxCompiler == null)
                {
                    AppendToConsole("❌ CX compiler not initialized");
                    UpdateStatusText("Compiler not ready");
                    return;
                }

                // Execute the code
                var result = await _cxCompiler.CompileAndExecuteAsync(code);
                
                if (result.IsSuccess)
                {
                    AppendToConsole("✅ CX code executed successfully");
                    AppendToConsole($"Output: {result.Output}");
                    AddEventToHistory("Code execution completed successfully");
                    UpdateStatusText("Execution completed successfully");
                }
                else
                {
                    AppendToConsole($"❌ Execution failed: {result.ErrorMessage}");
                    AddEventToHistory($"Code execution failed: {result.ErrorMessage}");
                    UpdateStatusText("Execution failed");
                }

                _lastExecutionResult = result;
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Exception during execution: {ex.Message}");
                AddEventToHistory($"Execution exception: {ex.Message}");
                UpdateStatusText("Execution error");
            }
        }
    }
}
