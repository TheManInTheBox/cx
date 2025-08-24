using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScintillaNET;
using CxLanguage.Core;
using CxLanguage.Parser;
using CxLanguage.Compiler;

namespace CxLanguage.IDE.Windows;

/// <summary>
/// Main IDE form for CX Language development with advanced syntax highlighting
/// Implements GitHub Issue #220 acceptance criteria
/// </summary>
public partial class CxLanguageIDEForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CxLanguageIDEForm> _logger;
    private readonly ICxLanguageParser _parser;
    private readonly ICxCompilerService _compiler;
    private readonly ICxSyntaxHighlighter _syntaxHighlighter;
    private readonly ICxAutoCompleteProvider _autoComplete;
    
    private Scintilla _codeEditor;
    private TextBox _outputConsole;
    private StatusStrip _statusStrip;
    private ToolStripStatusLabel _statusLabel;
    private ToolStripStatusLabel _errorLabel;
    private Timer _syntaxTimer;
    private string _currentFilePath = "";
    
    // Performance monitoring
    private DateTime _lastSyntaxHighlightTime;
    
    public CxLanguageIDEForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<CxLanguageIDEForm>>();
        _parser = serviceProvider.GetRequiredService<ICxLanguageParser>();
        _compiler = serviceProvider.GetRequiredService<ICxCompilerService>();
        _syntaxHighlighter = serviceProvider.GetRequiredService<ICxSyntaxHighlighter>();
        _autoComplete = serviceProvider.GetRequiredService<ICxAutoCompleteProvider>();
        
        InitializeComponent();
        InitializeSyntaxTimer();
        LoadDefaultCode();
        
        _logger.LogInformation("🚀 CX Language IDE initialized with compiler integration");
    }
    
    private void InitializeComponent()
    {
        // Form setup
        Text = "CX Language IDE - Consciousness Computing Platform";
        Size = new Size(1400, 900);
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(30, 30, 50);
        
        // Create main layout
        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 600,
            BackColor = Color.FromArgb(30, 30, 50)
        };
        
        // Initialize code editor with ScintillaNET
        InitializeCodeEditor(splitContainer.Panel1);
        
        // Initialize output console
        InitializeOutputConsole(splitContainer.Panel2);
        
        // Create menu and toolbar
        InitializeMenuAndToolbar();
        
        // Create status bar
        InitializeStatusBar();
        
        Controls.Add(splitContainer);
        Controls.Add(_statusStrip);
    }
    
    private void InitializeCodeEditor(Control parent)
    {
        _codeEditor = new Scintilla
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 40, 60),
            ForeColor = Color.White
        };
        
        // Configure CX Language syntax highlighting
        _syntaxHighlighter.ConfigureEditor(_codeEditor);
        
        // Event handlers for real-time features
        _codeEditor.TextChanged += OnCodeChanged;
        _codeEditor.CharAdded += OnCharAdded;
        _codeEditor.KeyDown += OnKeyDown;
        
        parent.Controls.Add(_codeEditor);
    }
    
    private void InitializeOutputConsole(Control parent)
    {
        var consolePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 50) };
        
        var consoleLabel = new Label
        {
            Text = "💻 Console Output",
            ForeColor = Color.White,
            BackColor = Color.FromArgb(33, 150, 243),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 5, 5, 5)
        };
        
        _outputConsole = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BackColor = Color.FromArgb(40, 40, 60),
            ForeColor = Color.White,
            Font = new Font("Consolas", 10),
            ScrollBars = ScrollBars.Vertical,
            Text = "Console output will appear here...\r\n"
        };
        
        consolePanel.Controls.Add(_outputConsole);
        consolePanel.Controls.Add(consoleLabel);
        parent.Controls.Add(consolePanel);
    }
    
    private void InitializeMenuAndToolbar()
    {
        // Create menu strip
        var menuStrip = new MenuStrip { BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        
        // File menu
        var fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("New", null, OnNew) { ShortcutKeys = Keys.Control | Keys.N });
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Open...", null, OnOpen) { ShortcutKeys = Keys.Control | Keys.O });
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Save", null, OnSave) { ShortcutKeys = Keys.Control | Keys.S });
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Save As...", null, OnSaveAs));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Exit", null, OnExit));
        
        // Run menu
        var runMenu = new ToolStripMenuItem("Run");
        runMenu.DropDownItems.Add(new ToolStripMenuItem("▶️ Compile and Run", null, OnCompileAndRun) { ShortcutKeys = Keys.F5 });
        runMenu.DropDownItems.Add(new ToolStripMenuItem("🔧 Compile Only", null, OnCompileOnly) { ShortcutKeys = Keys.F6 });
        
        // Tools menu
        var toolsMenu = new ToolStripMenuItem("Tools");
        toolsMenu.DropDownItems.Add(new ToolStripMenuItem("🧠 Consciousness Patterns", null, OnShowConsciousnessPatterns));
        toolsMenu.DropDownItems.Add(new ToolStripMenuItem("📊 Performance Monitor", null, OnShowPerformanceMonitor));
        
        menuStrip.Items.Add(fileMenu);
        menuStrip.Items.Add(runMenu);
        menuStrip.Items.Add(toolsMenu);
        
        // Create toolbar
        var toolStrip = new ToolStrip { BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        toolStrip.Items.Add(new ToolStripButton("▶️ Run", null, OnCompileAndRun) { ToolTipText = "Compile and Run (F5)" });
        toolStrip.Items.Add(new ToolStripButton("🔧 Compile", null, OnCompileOnly) { ToolTipText = "Compile Only (F6)" });
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(new ToolStripButton("💾 Save", null, OnSave) { ToolTipText = "Save (Ctrl+S)" });
        toolStrip.Items.Add(new ToolStripButton("📁 Open", null, OnOpen) { ToolTipText = "Open (Ctrl+O)" });
        
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
    }
    
    private void InitializeStatusBar()
    {
        _statusStrip = new StatusStrip { BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        
        _statusLabel = new ToolStripStatusLabel("Ready - CX Language IDE with Compiler Integration")
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        
        _errorLabel = new ToolStripStatusLabel("✅ No Errors")
        {
            ForeColor = Color.LightGreen
        };
        
        var performanceLabel = new ToolStripStatusLabel("⚡ Syntax: 0ms")
        {
            ToolTipText = "Syntax highlighting performance"
        };
        
        _statusStrip.Items.Add(_statusLabel);
        _statusStrip.Items.Add(_errorLabel);
        _statusStrip.Items.Add(performanceLabel);
        _statusStrip.Tag = performanceLabel; // Store reference for updates
    }
    
    private void InitializeSyntaxTimer()
    {
        _syntaxTimer = new Timer { Interval = 200 }; // 200ms delay for performance
        _syntaxTimer.Tick += async (s, e) =>
        {
            _syntaxTimer.Stop();
            await PerformSyntaxHighlighting();
        };
    }
    
    private void LoadDefaultCode()
    {
        _codeEditor.Text = @"conscious calculator {
    realize(self: conscious) {
        learn self;
    }

    handlers: [
        calculate.request { operation: ""add"", numbers: [2, 2] }
    ]

    on calculate.request (event) {
        emit infer {
            data: event.payload,
            handlers: [ calculation.complete ]
        };
    }

    on calculation.complete (event) {
        emit learn {
            data: event.result,
            handlers: [ result.display ]
        };
    }

    on result.display (event) {
        print(event.result);
    }
}";
        
        // Trigger initial syntax highlighting
        _ = Task.Run(PerformSyntaxHighlighting);
    }
    
    private void OnCodeChanged(object sender, EventArgs e)
    {
        // Restart syntax highlighting timer for performance
        _syntaxTimer.Stop();
        _syntaxTimer.Start();
        
        // Update status
        _statusLabel.Text = $"Editing - {(_currentFilePath.Length > 0 ? Path.GetFileName(_currentFilePath) : "Untitled")}";
    }
    
    private void OnCharAdded(object sender, CharAddedEventArgs e)
    {
        // Trigger auto-completion
        var currentPos = _codeEditor.CurrentPosition;
        var currentLine = _codeEditor.LineFromPosition(currentPos);
        var lineText = _codeEditor.Lines[currentLine].Text;
        
        // Show auto-completion for consciousness patterns
        if (e.Char == ' ' || e.Char == '{' || e.Char == '.')
        {
            var suggestions = _autoComplete.GetSuggestions(lineText, currentPos);
            if (suggestions.Any())
            {
                ShowAutoComplete(suggestions);
            }
        }
    }
    
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Handle special key combinations
        if (e.Control && e.KeyCode == Keys.Space)
        {
            // Force show auto-completion
            var currentPos = _codeEditor.CurrentPosition;
            var currentLine = _codeEditor.LineFromPosition(currentPos);
            var lineText = _codeEditor.Lines[currentLine].Text;
            var suggestions = _autoComplete.GetSuggestions(lineText, currentPos);
            ShowAutoComplete(suggestions);
            e.Handled = true;
        }
    }
    
    private async Task PerformSyntaxHighlighting()
    {
        var startTime = DateTime.Now;
        
        try
        {
            await Task.Run(() =>
            {
                var code = _codeEditor.Text;
                if (string.IsNullOrWhiteSpace(code)) return;
                
                // Parse code for syntax errors
                var parseResult = _parser.Parse(code, _currentFilePath);
                
                // Update error status
                Invoke(() =>
                {
                    if (parseResult.IsSuccess)
                    {
                        _errorLabel.Text = "✅ No Errors";
                        _errorLabel.ForeColor = Color.LightGreen;
                        _syntaxHighlighter.HighlightSyntax(_codeEditor, parseResult.Value);
                    }
                    else
                    {
                        _errorLabel.Text = $"❌ {parseResult.Errors.Length} Error(s)";
                        _errorLabel.ForeColor = Color.IndianRed;
                        _syntaxHighlighter.HighlightErrors(_codeEditor, parseResult.Errors);
                    }
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during syntax highlighting");
            Invoke(() =>
            {
                _errorLabel.Text = "⚠️ Syntax Error";
                _errorLabel.ForeColor = Color.Orange;
            });
        }
        
        // Update performance metrics
        var elapsed = DateTime.Now - startTime;
        _lastSyntaxHighlightTime = DateTime.Now;
        
        if (_statusStrip.Tag is ToolStripStatusLabel perfLabel)
        {
            Invoke(() => perfLabel.Text = $"⚡ Syntax: {elapsed.TotalMilliseconds:F0}ms");
        }
        
        // Log performance if over 100ms (acceptance criteria)
        if (elapsed.TotalMilliseconds > 100)
        {
            _logger.LogWarning($"Syntax highlighting took {elapsed.TotalMilliseconds:F0}ms (target: <100ms)");
        }
    }
    
    private void ShowAutoComplete(IEnumerable<string> suggestions)
    {
        // Show auto-completion list (simplified implementation)
        _codeEditor.AutoCShow(0, string.Join(" ", suggestions));
    }
    
    // Event handlers
    private void OnNew(object sender, EventArgs e)
    {
        _codeEditor.Text = "";
        _currentFilePath = "";
        _statusLabel.Text = "Ready - New File";
    }
    
    private void OnOpen(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CX Files (*.cx)|*.cx|All Files (*.*)|*.*",
            Title = "Open CX Language File"
        };
        
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _codeEditor.Text = File.ReadAllText(dialog.FileName);
                _currentFilePath = dialog.FileName;
                _statusLabel.Text = $"Opened - {Path.GetFileName(_currentFilePath)}";
                _ = Task.Run(PerformSyntaxHighlighting);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private void OnSave(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            OnSaveAs(sender, e);
            return;
        }
        
        try
        {
            File.WriteAllText(_currentFilePath, _codeEditor.Text);
            _statusLabel.Text = $"Saved - {Path.GetFileName(_currentFilePath)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void OnSaveAs(object sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "CX Files (*.cx)|*.cx|All Files (*.*)|*.*",
            Title = "Save CX Language File"
        };
        
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                File.WriteAllText(dialog.FileName, _codeEditor.Text);
                _currentFilePath = dialog.FileName;
                _statusLabel.Text = $"Saved - {Path.GetFileName(_currentFilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private void OnExit(object sender, EventArgs e)
    {
        Close();
    }
    
    private async void OnCompileAndRun(object sender, EventArgs e)
    {
        await CompileAndExecute(runAfterCompile: true);
    }
    
    private async void OnCompileOnly(object sender, EventArgs e)
    {
        await CompileAndExecute(runAfterCompile: false);
    }
    
    private async Task CompileAndExecute(bool runAfterCompile)
    {
        _statusLabel.Text = "Compiling...";
        _outputConsole.Text = "🔧 Starting compilation...\r\n";
        
        try
        {
            var code = _codeEditor.Text;
            var compileResult = await _compiler.CompileAsync(code, _currentFilePath);
            
            if (compileResult.IsSuccess)
            {
                _outputConsole.AppendText("✅ Compilation successful!\r\n");
                
                if (runAfterCompile)
                {
                    _outputConsole.AppendText("▶️ Executing program...\r\n");
                    var output = await _compiler.ExecuteAsync(compileResult.Assembly);
                    _outputConsole.AppendText($"Output: {output}\r\n");
                    _statusLabel.Text = "Execution completed";
                }
                else
                {
                    _statusLabel.Text = "Compilation completed";
                }
            }
            else
            {
                _outputConsole.AppendText($"❌ Compilation failed:\r\n");
                foreach (var error in compileResult.Errors)
                {
                    _outputConsole.AppendText($"  {error}\r\n");
                }
                _statusLabel.Text = "Compilation failed";
            }
        }
        catch (Exception ex)
        {
            _outputConsole.AppendText($"💥 Error: {ex.Message}\r\n");
            _statusLabel.Text = "Error occurred";
            _logger.LogError(ex, "Error during compilation/execution");
        }
    }
    
    private void OnShowConsciousnessPatterns(object sender, EventArgs e)
    {
        MessageBox.Show("Consciousness pattern analysis coming soon!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void OnShowPerformanceMonitor(object sender, EventArgs e)
    {
        var perfInfo = $@"Performance Metrics:
• Last syntax highlight: {(_lastSyntaxHighlightTime == default ? "Not measured" : $"{(DateTime.Now - _lastSyntaxHighlightTime).TotalMilliseconds:F0}ms ago")}
• Target: <100ms response time
• Status: {(_lastSyntaxHighlightTime != default && (DateTime.Now - _lastSyntaxHighlightTime).TotalMilliseconds < 100 ? "✅ Meeting target" : "⚠️ Monitor performance")}";
        
        MessageBox.Show(perfInfo, "Performance Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _syntaxTimer?.Dispose();
        base.OnFormClosed(e);
    }
}
