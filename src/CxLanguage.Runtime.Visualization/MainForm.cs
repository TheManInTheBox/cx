using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CxLanguage.Core.Events;
using CxLanguage.Runtime.Visualization.Services;
using CxLanguage.StandardLibrary.Services.VectorStore;
using CxLanguage.LocalLLM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace CxLanguage.Runtime.Visualization;

/// <summary>
/// Main form for CX Consciousness Visualization
/// </summary>
public partial class MainForm : Form
{
    private Label? _titleLabel;
    private Label? _statusLabel;
    private Label? _eventsLabel;
    private TextBox? _eventLogTextBox;
    private TextBox? _developerInputTextBox;
    private Button? _sendButton;
    private System.Windows.Forms.Timer? _updateTimer;
    private int _realEventCount = 0;
    private DateTime _startTime = DateTime.Now;
    private ICxEventBus? _eventBus;
    private CxLanguageVectorIngestionService? _vectorIngestionService;
    private RealTimeCxCompilerSimple? _cxCompiler;
    private AuraCxReferenceIngestor? _referenceIngestor;
    private IntelligentCxCodeGenerator? _codeGenerator;
    private CxExecutionResult? _lastExecutionResult;

    // --- CX IDE (Preview) controls ---
    private RichTextBox? _codeEditor;
    private Button? _completeCodeButton;
    private Button? _generateCodeButton;
    private Button? _executeCodeButton;
    private TextBox? _promptTextBox;
    private RichTextBox? _executionResultsTextBox;

    // --- IDE chrome ---
    private MenuStrip? _menuStrip;
    private StatusStrip? _statusStrip;
    private ToolStripStatusLabel? _fileStatusLabel;
    private ToolStripStatusLabel? _runtimeStatusLabel;
    private string? _currentFilePath;

    public MainForm(ICxEventBus? eventBus = null)
    {
        // Set the event bus if provided
        _eventBus = eventBus;
        
        InitializeComponent();
        InitializeVisualization();
        
        // Initialize LLM automatically on startup
        _ = Task.Run(async () => await InitializeLocalLlm());
    }

    private void InitializeComponent()
    {
        // Form properties
        Text = "CX IDE";
        Size = new Size(1200, 720); // expanded to fit new docked layout
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(30, 30, 50);
        ForeColor = Color.White;
        WindowState = FormWindowState.Normal;
        TopMost = false;
        ShowInTaskbar = true;

        // MenuStrip (IDE primary controls)
        _menuStrip = new MenuStrip
        {
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(40, 40, 60),
            ForeColor = Color.White
        };

        // File menu
        var fileMenu = new ToolStripMenuItem("File");
        var miNew = new ToolStripMenuItem("New", null, (s, e) => NewFile()) { ShortcutKeys = Keys.Control | Keys.N };
        var miOpen = new ToolStripMenuItem("Open...", null, (s, e) => OpenFile()) { ShortcutKeys = Keys.Control | Keys.O };
        var miSave = new ToolStripMenuItem("Save", null, (s, e) => SaveFile()) { ShortcutKeys = Keys.Control | Keys.S };
        var miSaveAs = new ToolStripMenuItem("Save As...", null, (s, e) => SaveFileAs());
        var miExit = new ToolStripMenuItem("Exit", null, (s, e) => Close());
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { miNew, miOpen, miSave, miSaveAs, new ToolStripSeparator(), miExit });

        // Run menu
        var runMenu = new ToolStripMenuItem("Run");
        var miComplete = new ToolStripMenuItem("Complete Code", null, async (s, e) => await TriggerCodeCompletion()) { ShortcutKeys = Keys.Control | Keys.Space };
        var miExecute = new ToolStripMenuItem("Execute Program", null, async (s, e) => await ExecuteCurrentCode());
        runMenu.DropDownItems.AddRange(new ToolStripItem[] { miComplete, new ToolStripSeparator(), miExecute });

        // Help menu (placeholder)
        var helpMenu = new ToolStripMenuItem("Help");
        var miAbout = new ToolStripMenuItem("About CX IDE", null, (s, e) => MessageBox.Show("CX IDE - Primary editor for CX Language", "About", MessageBoxButtons.OK, MessageBoxIcon.Information));
        helpMenu.DropDownItems.Add(miAbout);

        _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, runMenu, helpMenu });
        Controls.Add(_menuStrip);

        // StatusStrip
        _statusStrip = new StatusStrip
        {
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(40, 40, 60),
            ForeColor = Color.White
        };
        _fileStatusLabel = new ToolStripStatusLabel("[No file]") { Spring = true };
        _runtimeStatusLabel = new ToolStripStatusLabel("Runtime: Unknown");
        _statusStrip.Items.AddRange(new ToolStripItem[] { _fileStatusLabel, _runtimeStatusLabel });
        Controls.Add(_statusStrip);

        // --- CX IDE (Primary) UI - LEFT DOCKED ---
        var ideLabel = new Label
        {
            Text = "� CX IDE (Primary)",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 220, 255),
            Location = new Point(20, 50),
            Size = new Size(560, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(ideLabel);

        _codeEditor = new RichTextBox
        {
            Location = new Point(20, 80),
            Size = new Size(560, 400),
            BackColor = Color.FromArgb(20, 20, 30),
            ForeColor = Color.FromArgb(230, 230, 230),
            Font = new Font("Consolas", 10, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle,
            DetectUrls = false,
            AcceptsTab = true,
            WordWrap = false
        };
        // Basic starter text with correct CX syntax
        _codeEditor.Text = "// Type CX code here and press Ctrl+Space or click Complete\n" +
                           "conscious Calculator {\n" +
                           "    realize(self: conscious) {\n" +
                           "        learn self;\n" +
                           "    }\n" +
                           "    \n" +
                           "    on calculate(event) {\n" +
                           "        emit calculation.result { value: 42 }\n" +
                           "    }\n" +
                           "}";
        _codeEditor.KeyDown += async (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
                await TriggerCodeCompletion();
            }
        };
        Controls.Add(_codeEditor);

        // --- RUNTIME ENVIRONMENT DATA - RIGHT DOCKED ---
        // Title label
        _titleLabel = new Label
        {
            Text = "🧠 CX RUNTIME ENVIRONMENT",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 200, 255),
            Location = new Point(600, 50),
            Size = new Size(580, 30),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_titleLabel);

        // Status label
        _statusLabel = new Label
        {
            Text = "🟢 Consciousness System: ACTIVE",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 255, 100),
            Location = new Point(600, 80),
            Size = new Size(580, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_statusLabel);

        // Events label
        _eventsLabel = new Label
        {
            Text = "� Real-time Event Processing: 0 events (0.0s runtime)",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.FromArgb(200, 200, 200),
            Location = new Point(600, 105),
            Size = new Size(520, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_eventsLabel);

        // Test button for debugging
        var testButton = new Button
        {
            Text = "🧪 Test Event",
            Size = new Size(100, 25),
            Location = new Point(1080, 105),
            BackColor = Color.FromArgb(0, 100, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        testButton.Click += async (s, e) => 
        {
            LogEvent("🧪 Manual test event triggered");
            _realEventCount++;
            
            if (_eventBus != null)
            {
                try
                {
                    var testPayload = new Dictionary<string, object>
                    {
                        { "source", "visualization_test" },
                        { "message", "Manual event from visualization UI" },
                        { "timestamp", DateTime.Now.ToString("HH:mm:ss.fff") }
                    };
                    
                    var emitResult = await _eventBus.EmitAsync("visualization.test.event", testPayload);
                    LogEvent($"🚀 Test event emitted: {emitResult}");
                }
                catch (Exception ex)
                {
                    LogEvent($"❌ Failed to emit test event: {ex.Message}");
                }
            }
        };
        Controls.Add(testButton);

        // Event log text box
        _eventLogTextBox = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            BackColor = Color.FromArgb(20, 20, 30),
            ForeColor = Color.FromArgb(200, 200, 200),
            Font = new Font("Consolas", 9, FontStyle.Regular),
            Location = new Point(600, 135),
            Size = new Size(580, 280),
            Text = "🚀 CX Consciousness Visualization Starting...\r\n" +
                   "🔗 Event bus connection established\r\n" +
                   "✨ Neural pathways initialized\r\n" +
                   "🎯 Ready for consciousness monitoring\r\n\r\n"
        };
        Controls.Add(_eventLogTextBox);

        // Developer input textbox
        var inputLabel = new Label
        {
            Text = "🔧 Developer Input:",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.FromArgb(150, 150, 150),
            Location = new Point(600, 425),
            Size = new Size(120, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(inputLabel);

        _developerInputTextBox = new TextBox
        {
            BackColor = Color.FromArgb(40, 40, 60),
            ForeColor = Color.FromArgb(255, 255, 255),
            Font = new Font("Consolas", 9, FontStyle.Regular),
            Location = new Point(600, 450),
            Size = new Size(490, 25),
            PlaceholderText = "Enter event name (e.g., system.shutdown, test.event)"
        };
        Controls.Add(_developerInputTextBox);

        // Send button for developer input
        _sendButton = new Button
        {
            Text = "📤 Send",
            Size = new Size(80, 25),
            Location = new Point(1100, 450),
            BackColor = Color.FromArgb(0, 150, 100),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _sendButton.Click += async (s, e) => await SendDeveloperInput();
        Controls.Add(_sendButton);

        // Handle Enter key in textbox
        _developerInputTextBox.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                await SendDeveloperInput();
            }
        };

        // --- IDE CONTROLS BOTTOM LEFT ---
        _completeCodeButton = new Button
        {
            Text = "✨ Complete",
            Size = new Size(120, 30),
            Location = new Point(20, 490),
            BackColor = Color.FromArgb(90, 60, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false // Disabled until LLM is initialized
        };
        _completeCodeButton.Click += async (s, e) => await TriggerCodeCompletion();
        Controls.Add(_completeCodeButton);

        // LLM is now automatically initialized on startup, no button needed

        // Generate Code button  
        _generateCodeButton = new Button
        {
            Text = "🤖 Generate",
            Size = new Size(120, 30),
            Location = new Point(280, 490),
            BackColor = Color.FromArgb(200, 60, 90),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _generateCodeButton.Click += async (s, e) => await GenerateCodeFromPrompt();
        Controls.Add(_generateCodeButton);

        // Execute Code button
        _executeCodeButton = new Button
        {
            Text = "▶️ Execute",
            Size = new Size(120, 30),
            Location = new Point(410, 490),
            BackColor = Color.FromArgb(0, 180, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _executeCodeButton.Click += async (s, e) => await ExecuteCurrentCode();
        Controls.Add(_executeCodeButton);

        // Prompt input for code generation
        var promptLabel = new Label
        {
            Text = "💭 Prompt:",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.FromArgb(150, 150, 150),
            Location = new Point(20, 530),
            Size = new Size(60, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(promptLabel);

        _promptTextBox = new TextBox
        {
            BackColor = Color.FromArgb(40, 40, 60),
            ForeColor = Color.FromArgb(255, 255, 255),
            Font = new Font("Consolas", 9, FontStyle.Regular),
            Location = new Point(85, 530),
            Size = new Size(495, 25),
            PlaceholderText = "Describe CX code to generate..."
        };
        Controls.Add(_promptTextBox);

        // Execution results display
        var resultsLabel = new Label
        {
            Text = "📊 Execution Results:",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.FromArgb(150, 150, 150),
            Location = new Point(20, 565),
            Size = new Size(120, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(resultsLabel);

        _executionResultsTextBox = new RichTextBox
        {
            Location = new Point(20, 590),
            Size = new Size(560, 80),
            BackColor = Color.FromArgb(25, 25, 35),
            ForeColor = Color.FromArgb(200, 255, 200),
            Font = new Font("Consolas", 9, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle,
            ReadOnly = true,
            Text = "No execution results yet..."
        };
        Controls.Add(_executionResultsTextBox);

        // Update timer
        _updateTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000, // 1 second
            Enabled = true
        };
        _updateTimer.Tick += UpdateTimer_Tick;
    }

    private void InitializeVisualization()
    {
        // Log initial startup
        LogEvent("🎮 CX Consciousness Visualization initialized");
        LogEvent("🧠 Connecting to CX Runtime Event Bus...");
        
        // Try to connect to the CX event bus
        InitializeCxEventConnection();
        
        // Initialize vector ingestion service
        InitializeVectorIngestionService();
        
        // Initialize real-time development services
        InitializeRealTimeDevelopmentServices();
        
        // Update status label to reflect connection state
        if (_eventBus != null)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = "🟢 Consciousness System: CONNECTED to CX Runtime";
                _statusLabel.ForeColor = Color.FromArgb(100, 255, 100);
            }
            LogEvent("⚡ Ready for real-time consciousness monitoring");
            _runtimeStatusLabel!.Text = "Runtime: Connected";
        }
        else
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = "🟡 Consciousness System: STANDALONE MODE (No CX Runtime)";
                _statusLabel.ForeColor = Color.FromArgb(255, 255, 100);
            }
            LogEvent("⚡ Ready for visualization (standalone mode)");
            _runtimeStatusLabel!.Text = "Runtime: Standalone";
        }
    }
    
    private void InitializeCxEventConnection()
    {
        try
        {
            // Use provided event bus if available, otherwise try to get from service registry
            if (_eventBus == null)
            {
                LogEvent("🔍 Attempting to get event bus from service registry...");
                var eventBusService = CxLanguage.Runtime.CxRuntimeHelper.GetService("ICxEventBus");
                LogEvent($"📋 Service registry returned: {eventBusService?.GetType()?.Name ?? "null"}");
                
                if (eventBusService is ICxEventBus eventBus)
                {
                    _eventBus = eventBus;
                    LogEvent("✅ Successfully cast to ICxEventBus interface");
                }
                else
                {
                    LogEvent("❌ Failed to cast to ICxEventBus interface");
                }
            }
            else
            {
                LogEvent("🔗 Using provided event bus instance");
            }
            
            if (_eventBus != null)
            {
                LogEvent($"📊 Event bus type: {_eventBus.GetType().Name}");
                LogEvent("🔔 Subscribing to ALL events...");
                
                // Subscribe to "any" to capture ALL events (CX Language wildcard)
                bool anySubscribed = _eventBus.Subscribe("any", OnCxEvent);
                LogEvent($"📡 CX 'any' wildcard subscription result: {anySubscribed}");
                
                // Also subscribe to system shutdown events specifically
                bool shutdownSubscribed = _eventBus.Subscribe("system.shutdown", OnSystemShutdown);
                LogEvent($"💀 System shutdown subscription result: {shutdownSubscribed}");
                
                // Subscribe to vector ingestion events
                bool ingestionStartSubscribed = _eventBus.Subscribe("cx.language.ingestion.start", OnVectorIngestionEvent);
                bool ingestionCompleteSubscribed = _eventBus.Subscribe("cx.language.ingestion.complete", OnVectorIngestionEvent);
                bool ingestionErrorSubscribed = _eventBus.Subscribe("cx.language.ingestion.error", OnVectorIngestionEvent);
                LogEvent($"📚 Vector ingestion subscriptions: start={ingestionStartSubscribed}, complete={ingestionCompleteSubscribed}, error={ingestionErrorSubscribed}");

                // Subscribe to Local LLM events for IDE completion
                bool llmGenSubscribed = _eventBus.Subscribe("llm.generated", OnLlmGeneratedEvent);
                bool llmErrSubscribed = _eventBus.Subscribe("llm.error", OnLlmErrorEvent);
                bool llmInitSubscribed = _eventBus.Subscribe("llm.initialized", OnLlmInitializedEvent);
                LogEvent($"🤖 LLM event subscriptions: generated={llmGenSubscribed}, error={llmErrSubscribed}, initialized={llmInitSubscribed}");
                
                // Subscribe to CX execution events for IDE results
                bool execStartSubscribed = _eventBus.Subscribe("cx.runtime.execution.started", OnCxExecutionEvent);
                bool execCompleteSubscribed = _eventBus.Subscribe("cx.runtime.execution.completed", OnCxExecutionEvent);
                bool execErrorSubscribed = _eventBus.Subscribe("cx.compiler.compilation.error", OnCxExecutionEvent);
                LogEvent($"⚡ CX execution subscriptions: started={execStartSubscribed}, completed={execCompleteSubscribed}, error={execErrorSubscribed}");
                
                if (anySubscribed)
                {
                    LogEvent("✅ Connected to CX Runtime Event Bus");
                    LogEvent("🔗 Monitoring ALL consciousness events in real-time");
                }
                else
                {
                    LogEvent("❌ Failed to subscribe to CX 'any' wildcard events");
                }
                
                // Get current statistics
                var stats = _eventBus.GetStatistics();
                LogEvent($"� Event bus statistics: {stats.Count} stat entries");
            }
            else
            {
                LogEvent("⚠️ CX Event Bus not available - running in standalone mode");
            }
        }
        catch (Exception ex)
        {
            LogEvent($"⚠️ Failed to connect to CX Event Bus: {ex.Message}");
            LogEvent($"🔍 Exception type: {ex.GetType().Name}");
            LogEvent("🔄 Running in standalone visualization mode");
        }
    }
    
    private void InitializeVectorIngestionService()
    {
        try
        {
            LogEvent("📚 Initializing CX Language vector ingestion service...");
            
            if (_eventBus != null)
            {
                // Try to get vector store service from runtime
                var vectorStoreService = CxLanguage.Runtime.CxRuntimeHelper.GetService("IVectorStoreService");
                IVectorStoreService? vectorStore = null;
                
                if (vectorStoreService is IVectorStoreService vs)
                {
                    vectorStore = vs;
                    LogEvent("✅ Vector store service available for ingestion");
                }
                else
                {
                    LogEvent("⚠️ Vector store service not available - using event-only logging");
                }
                
                // Create logger for the ingestion service
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var logger = loggerFactory.CreateLogger<CxLanguageVectorIngestionService>();
                
                // Initialize the vector ingestion service
                _vectorIngestionService = new CxLanguageVectorIngestionService(logger, _eventBus, vectorStore);
                
                // Start ingestion in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _vectorIngestionService.IngestCxLanguageReferencesAsync();
                    }
                    catch (Exception ex)
                    {
                        Invoke(() => LogEvent($"❌ Vector ingestion error: {ex.Message}"));
                    }
                });
                
                LogEvent("🚀 CX Language vector ingestion started in background");
            }
            else
            {
                LogEvent("⚠️ No event bus - skipping vector ingestion initialization");
            }
        }
        catch (Exception ex)
        {
            LogEvent($"❌ Error initializing vector ingestion service: {ex.Message}");
        }
    }
    
    private void InitializeRealTimeDevelopmentServices()
    {
        try
        {
            LogEvent("🎯 Initializing real-time CX development services...");
            
            if (_eventBus != null)
            {
                // Get services from dependency injection
                var vectorStoreService = CxLanguage.Runtime.CxRuntimeHelper.GetService("IVectorStoreService") as IVectorStoreService;
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                
                // Initialize RealTimeCxCompiler
                try
                {
                    var compilerLogger = loggerFactory.CreateLogger<RealTimeCxCompilerSimple>();
                    _cxCompiler = new RealTimeCxCompilerSimple(compilerLogger, _eventBus);
                    LogEvent("✅ Real-time CX compiler initialized");
                }
                catch (Exception ex)
                {
                    LogEvent($"⚠️ Failed to initialize CX compiler: {ex.Message}");
                }
                
                // Initialize LocalLLM service directly
                try
                {
                    var llmLogger = loggerFactory.CreateLogger<CxLanguage.LocalLLM.GpuLocalLLMService>();
                    var localLLMService = new CxLanguage.LocalLLM.GpuLocalLLMService(llmLogger);
                    
                    // Register the local LLM service for static access
                    CxLanguage.Runtime.CxRuntimeHelper.RegisterService("LocalLLMService", localLLMService);
                    LogEvent("✅ LocalLLM service initialized and registered directly");
                    
                    // Also create and register event handler for the LLM service
                    var handlerLogger = loggerFactory.CreateLogger<CxLanguage.LocalLLM.LocalLlmEventHandler>();
                    var handler = new CxLanguage.LocalLLM.LocalLlmEventHandler(handlerLogger, _eventBus, localLLMService);
                    LogEvent("✅ LocalLLM event handler initialized and registered");
                }
                catch (Exception ex)
                {
                    LogEvent($"⚠️ Failed to initialize LocalLLM service: {ex.Message}");
                }
                
                // Initialize AuraCxReferenceIngestor
                if (vectorStoreService != null)
                {
                    try
                    {
                        var ingestorLogger = loggerFactory.CreateLogger<AuraCxReferenceIngestor>();
                        _referenceIngestor = new AuraCxReferenceIngestor(ingestorLogger, _eventBus, vectorStoreService);
                        LogEvent("✅ Aura CX reference ingestor initialized");
                        
                        // Start reference ingestion in background
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _referenceIngestor.IngestCxReferencesAsync();
                            }
                            catch (Exception ex)
                            {
                                Invoke(() => LogEvent($"❌ Reference ingestion error: {ex.Message}"));
                            }
                        });
                        LogEvent("🚀 CX reference ingestion started in background");
                    }
                    catch (Exception ex)
                    {
                        LogEvent($"⚠️ Failed to initialize reference ingestor: {ex.Message}");
                    }
                }
                else
                {
                    LogEvent("⚠️ Vector store service not available - reference ingestor skipped");
                }
                
                // Initialize IntelligentCxCodeGenerator
                try
                {
                    var generatorLogger = loggerFactory.CreateLogger<IntelligentCxCodeGenerator>();
                    var llmService = CxLanguage.Runtime.CxRuntimeHelper.GetService("LocalLLMService") as ILocalLLMService;
                    if (llmService != null && vectorStoreService != null)
                    {
                        _codeGenerator = new IntelligentCxCodeGenerator(generatorLogger, _eventBus, llmService, vectorStoreService);
                        LogEvent("✅ Intelligent CX code generator initialized");
                    }
                    else
                    {
                        var missingServices = new List<string>();
                        if (llmService == null) missingServices.Add("LLM service");
                        if (vectorStoreService == null) missingServices.Add("Vector store service");
                        LogEvent($"⚠️ Missing services for code generator: {string.Join(", ", missingServices)}");
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"⚠️ Failed to initialize code generator: {ex.Message}");
                }
                
                LogEvent("🎮 Real-time CX development environment ready!");
            }
            else
            {
                LogEvent("⚠️ No event bus - skipping real-time development services initialization");
            }
        }
        catch (Exception ex)
        {
            LogEvent($"❌ Error initializing real-time development services: {ex.Message}");
        }
    }
    
    private Task<bool> OnCxEvent(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        try
        {
            // Increment real event counter
            _realEventCount++;
            
            // Log the real event
            this.Invoke(() =>
            {
                LogEvent($"🌟 CX Event #{_realEventCount}: {eventName}");
                LogEvent($"   📤 Sender: {sender?.GetType()?.Name ?? "null"}");
                
                // Log payload details if available
                if (payload != null && payload.Count > 0)
                {
                    LogEvent($"   📦 Payload: {payload.Count} properties");
                    foreach (var kvp in payload)
                    {
                        if (kvp.Key != "handlers") // Skip handlers to reduce noise
                        {
                            var valueStr = kvp.Value?.ToString() ?? "null";
                            if (valueStr.Length > 50) valueStr = valueStr.Substring(0, 50) + "...";
                            LogEvent($"      • {kvp.Key}: {valueStr}");
                        }
                    }
                }
                else
                {
                    LogEvent($"   📦 Payload: empty or null");
                }
                LogEvent(""); // Add spacing between events
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() => LogEvent($"⚠️ Error processing event {eventName}: {ex.Message}"));
        }
        
        return Task.FromResult(true);
    }

    private Task<bool> OnSystemShutdown(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        try
        {
            this.Invoke(() =>
            {
                LogEvent("💀 SYSTEM SHUTDOWN EVENT RECEIVED!");
                LogEvent("🔴 Initiating application termination...");
                
                if (payload != null && payload.ContainsKey("reason"))
                {
                    LogEvent($"📋 Shutdown reason: {payload["reason"]}");
                }
                
                LogEvent("⏱️ Shutting down in 2 seconds...");
            });
            
            // Give a brief moment for the log message to display, then terminate
            System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
            {
                this.Invoke(() =>
                {
                    LogEvent("🛑 Application terminating now!");
                    Application.Exit();
                });
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() => LogEvent($"⚠️ Error during shutdown: {ex.Message}"));
        }
        
        return Task.FromResult(true);
    }

    private Task<bool> OnVectorIngestionEvent(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        try
        {
            this.Invoke(() =>
            {
                switch (eventName)
                {
                    case "cx.language.ingestion.start":
                        LogEvent("📚 CX Language vector ingestion started");
                        if (payload != null && payload.ContainsKey("fileCount"))
                        {
                            LogEvent($"📄 Processing {payload["fileCount"]} CX Language reference files");
                        }
                        break;
                        
                    case "cx.language.ingestion.complete":
                        LogEvent("✅ CX Language vector ingestion completed!");
                        if (payload != null)
                        {
                            var successCount = payload.ContainsKey("successCount") ? payload["successCount"] : "unknown";
                            var errorCount = payload.ContainsKey("errorCount") ? payload["errorCount"] : "unknown";
                            var status = payload.ContainsKey("status") ? payload["status"] : "unknown";
                            LogEvent($"📊 Results: {successCount} success, {errorCount} errors, status: {status}");
                        }
                        LogEvent("🧠 Global vector database now contains CX Language knowledge!");
                        break;
                        
                    case "cx.language.ingestion.error":
                        LogEvent("❌ CX Language vector ingestion error!");
                        if (payload != null && payload.ContainsKey("error"))
                        {
                            LogEvent($"🚨 Error details: {payload["error"]}");
                        }
                        break;
                        
                    default:
                        LogEvent($"📚 Vector ingestion event: {eventName}");
                        break;
                }
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() => LogEvent($"⚠️ Error processing vector ingestion event {eventName}: {ex.Message}"));
        }
        
        return Task.FromResult(true);
    }

    private Task<bool> OnCxExecutionEvent(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        try
        {
            this.Invoke(() =>
            {
                switch (eventName)
                {
                    case "cx.runtime.execution.started":
                        LogEvent("⚡ CX execution started");
                        if (payload != null && payload.ContainsKey("executionId"))
                        {
                            LogEvent($"🆔 Execution ID: {payload["executionId"]}");
                        }
                        break;
                        
                    case "cx.runtime.execution.completed":
                        LogEvent("✅ CX execution completed successfully!");
                        if (payload != null)
                        {
                            var eventsEmitted = payload.ContainsKey("eventsEmitted") ? payload["eventsEmitted"] : "unknown";
                            var outputLength = payload.ContainsKey("outputLength") ? payload["outputLength"] : "unknown";
                            LogEvent($"📊 Results: {eventsEmitted} events emitted, {outputLength} chars output");
                            
                            // Update execution results with success message
                            var currentText = _executionResultsTextBox?.Text ?? "";
                            var newText = currentText + $"\n\n✅ Execution completed via events!\n📊 Events emitted: {eventsEmitted}\n📄 Output length: {outputLength} characters";
                            UpdateExecutionResults(newText);
                        }
                        break;
                        
                    case "cx.compiler.compilation.error":
                        LogEvent("❌ CX compilation/execution error!");
                        if (payload != null && payload.ContainsKey("error"))
                        {
                            var error = payload["error"].ToString();
                            LogEvent($"🚨 Error details: {error}");
                            
                            // Update execution results with error message
                            var currentText = _executionResultsTextBox?.Text ?? "";
                            var newText = currentText + $"\n\n❌ Execution failed!\n🚨 Error: {error}";
                            UpdateExecutionResults(newText);
                        }
                        break;
                        
                    default:
                        LogEvent($"⚡ CX execution event: {eventName}");
                        break;
                }
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() => LogEvent($"⚠️ Error processing CX execution event {eventName}: {ex.Message}"));
        }
        
        return Task.FromResult(true);
    }

    // --- LLM event handlers for IDE completion ---
    private Task<bool> OnLlmGeneratedEvent(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        try
        {
            if (payload == null)
            {
                this.Invoke(() => LogEvent("⚠️ LLM generated event received with null payload"));
                return Task.FromResult(false);
            }

            if (!payload.TryGetValue("result", out var resultObj) || resultObj == null)
            {
                this.Invoke(() => LogEvent("⚠️ LLM generated event missing result field"));
                return Task.FromResult(false);
            }

            var text = resultObj.ToString() ?? string.Empty;

            this.Invoke(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        LogEvent("⚠️ LLM returned empty result");
                        return;
                    }

                    if (_codeEditor == null)
                    {
                        LogEvent("⚠️ Code editor not available for completion insertion");
                        return;
                    }

                    // Insert the completion text at current cursor position
                    var pos = _codeEditor.SelectionStart;
                    _codeEditor.Select(pos, 0);
                    _codeEditor.SelectedText = text;
                    LogEvent("🧩 Code completion inserted successfully");
                }
                catch (Exception ex)
                {
                    LogEvent($"⚠️ Error inserting completion text: {ex.Message}");
                    ShowConsciousnessProcessingError("Code completion insertion", ex);
                }
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() => 
            {
                LogEvent($"⚠️ Error handling LLM result: {ex.Message}");
                ShowConsciousnessProcessingError("LLM result handling", ex);
            });
        }
        return Task.FromResult(true);
    }

    private Task<bool> OnLlmErrorEvent(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        try
        {
            if (payload == null)
            {
                this.Invoke(() => LogEvent("⚠️ LLM error event received with null payload"));
                return Task.FromResult(false);
            }

            if (!payload.TryGetValue("errorMessage", out var msg))
            {
                this.Invoke(() => LogEvent("⚠️ LLM error event missing errorMessage field"));
                return Task.FromResult(false);
            }

            var message = msg?.ToString() ?? "Unknown error";
            var details = payload.TryGetValue("errorDetails", out var det) ? det?.ToString() : null;

            this.Invoke(() =>
            {
                LogEvent($"❌ LLM error: {message}");
                if (!string.IsNullOrWhiteSpace(details))
                {
                    LogEvent($"   ↳ Details: {details}");
                }
                
                // Show error to user for better feedback
                MessageBox.Show(
                    $"The LLM encountered an error: {message}\n\nPlease check the log for details.",
                    "LLM Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() => LogEvent($"⚠️ Error handling LLM error event: {ex.Message}"));
        }
        return Task.FromResult(true);
    }

    private Task<bool> OnLlmInitializedEvent(object? sender, string eventName, IDictionary<string, object>? payload)
    {
        try
        {
            if (payload == null)
            {
                this.Invoke(() => LogEvent("⚠️ LLM initialized event received with null payload"));
                return Task.FromResult(false);
            }

            this.Invoke(() =>
            {
                payload.TryGetValue("modelName", out var m);
                payload.TryGetValue("gpuAvailable", out var g);
                payload.TryGetValue("modelLoaded", out var ml);
                
                var model = m?.ToString() ?? "unknown";
                var gpu = g?.ToString() ?? "unknown";
                var modelLoaded = ml != null && ml is bool b && b;
                
                LogEvent($"✅ Local LLM initialized successfully");
                LogEvent($"   ↳ Model: {model}");
                LogEvent($"   ↳ GPU Available: {gpu}");
                LogEvent($"   ↳ Model Loaded: {modelLoaded}");
                
                // Enable UI elements that depend on LLM being initialized
                if (_completeCodeButton != null)
                {
                    _completeCodeButton.Enabled = true;
                }
            });
        }
        catch (Exception ex)
        {
            this.Invoke(() => 
            {
                LogEvent($"⚠️ Error handling LLM initialized event: {ex.Message}");
                // Don't show error dialog here as this is background processing
            });
        }
        return Task.FromResult(true);
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.Now - _startTime;
        
        // Update events label with real metrics
        if (_eventsLabel != null)
        {
            _eventsLabel.Text = $"📊 Real CX Events: {_realEventCount} events ({elapsed.TotalSeconds:F1}s runtime)";
        }
    }

    private void LogEvent(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var logEntry = $"[{timestamp}] {message}\r\n";
        
        if (_eventLogTextBox != null)
        {
            _eventLogTextBox.AppendText(logEntry);
            _eventLogTextBox.SelectionStart = _eventLogTextBox.Text.Length;
            _eventLogTextBox.ScrollToCaret();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
        
        // Disconnect from event bus
        if (_eventBus != null)
        {
            try
            {
                // Unsubscribe from "any" wildcard
                _eventBus.Unsubscribe("any", OnCxEvent);
                _eventBus.Unsubscribe("system.shutdown", OnSystemShutdown);
                
                // Unsubscribe from vector ingestion events
                _eventBus.Unsubscribe("cx.language.ingestion.start", OnVectorIngestionEvent);
                _eventBus.Unsubscribe("cx.language.ingestion.complete", OnVectorIngestionEvent);
                _eventBus.Unsubscribe("cx.language.ingestion.error", OnVectorIngestionEvent);

                // Unsubscribe from LLM events
                _eventBus.Unsubscribe("llm.generated", OnLlmGeneratedEvent);
                _eventBus.Unsubscribe("llm.error", OnLlmErrorEvent);
                _eventBus.Unsubscribe("llm.initialized", OnLlmInitializedEvent);
                
                // Unsubscribe from CX execution events
                _eventBus.Unsubscribe("cx.runtime.execution.started", OnCxExecutionEvent);
                _eventBus.Unsubscribe("cx.runtime.execution.completed", OnCxExecutionEvent);
                _eventBus.Unsubscribe("cx.compiler.compilation.error", OnCxExecutionEvent);
                
                LogEvent("🔌 Disconnected from CX Event Bus");
            }
            catch (Exception ex)
            {
                LogEvent($"⚠️ Error disconnecting from event bus: {ex.Message}");
            }
        }
        
        LogEvent("🛑 CX Consciousness Visualization shutting down");
        base.OnFormClosing(e);
    }

    private async Task SendDeveloperInput()
    {
        if (_developerInputTextBox == null || string.IsNullOrWhiteSpace(_developerInputTextBox.Text))
            return;

        var input = _developerInputTextBox.Text.Trim();
        LogEvent($"🔧 Developer Input: {input}");

        if (_eventBus != null)
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "source", "developer_input" },
                    { "input", input },
                    { "timestamp", DateTime.Now.ToString("HH:mm:ss.fff") },
                    { "type", "manual_command" }
                };

                // Check if the input is a direct event emission
                if (input.StartsWith("system.") || input.StartsWith("timer.") || input.StartsWith("agent.") || 
                    input.StartsWith("consciousness.") || input.StartsWith("test.") || input.Contains(".shutdown"))
                {
                    // Special handling for system.shutdown
                    if (input.StartsWith("system.shutdown"))
                    {
                        payload["reason"] = "developer_command";
                        LogEvent("💀 System shutdown event - application will terminate!");
                    }
                    
                    // Emit the event directly as typed
                    var directEmitResult = await _eventBus.EmitAsync(input, payload);
                    LogEvent($"🎯 Event emitted: {input} → {directEmitResult}");
                    
                    _realEventCount += 1;
                }
                else
                {
                    // Emit as developer command for non-system events
                    var emitResult = await _eventBus.EmitAsync($"developer.input.{input.Replace(" ", "_").Replace(".", "_")}", payload);
                    LogEvent($"📤 Developer command emitted: {emitResult}");

                    await _eventBus.EmitAsync("developer.command", payload);
                    LogEvent("🚀 Command sent to consciousness system");

                    _realEventCount += 2;
                }
            }
            catch (Exception ex)
            {
                LogEvent($"❌ Failed to send developer input: {ex.Message}");
            }
        }
        else
        {
            LogEvent("⚠️ No event bus connection - developer input logged only");
        }

        // Clear the input box
        _developerInputTextBox.Text = "";
    }

    // --- Status message utilities ---
    private enum StatusMessageType
    {
        Info,
        Warning,
        Error,
        Success
    }
    
    private void ShowStatusMessage(string message, StatusMessageType type)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowStatusMessage(message, type)));
            return;
        }
        
        // Update status bar
        if (_runtimeStatusLabel != null)
        {
            _runtimeStatusLabel.Text = message;
            
            // Set color based on message type
            switch (type)
            {
                case StatusMessageType.Info:
                    _runtimeStatusLabel.ForeColor = Color.DarkBlue;
                    break;
                case StatusMessageType.Warning:
                    _runtimeStatusLabel.ForeColor = Color.DarkOrange;
                    break;
                case StatusMessageType.Error:
                    _runtimeStatusLabel.ForeColor = Color.DarkRed;
                    break;
                case StatusMessageType.Success:
                    _runtimeStatusLabel.ForeColor = Color.DarkGreen;
                    break;
            }
        }
        
        // Log the message as well
        LogEvent($"Status: {message}");
    }

    // --- CX IDE: trigger code completion via Local LLM events ---
    private async Task TriggerCodeCompletion()
    {
        if (_eventBus == null)
        {
            LogEvent("⚠️ No event bus connection - attempting to reinitialize event bus");
            InitializeCxEventConnection(); // Try to reinitialize event bus connection
            
            if (_eventBus == null)
            {
                LogEvent("❌ Event bus still unavailable - cannot request completion");
                ShowStatusMessage("Event bus unavailable. Cannot perform code completion.", StatusMessageType.Error);
                return;
            }
            LogEvent("✅ Event bus reinitialized successfully");
        }
        
        if (_codeEditor == null)
        {
            LogEvent("❌ Code editor not ready - cannot perform code completion");
            return;
        }

        // Check if the LLM service is ready
        bool isInitialized = IsLlmInitialized();
        if (!isInitialized)
        {
            LogEvent("⚠️ LLM service not initialized - attempting automatic initialization");
            ShowStatusMessage("Initializing LLM service...", StatusMessageType.Info);
            
            await InitializeLocalLlm();
            
            // Check again after attempting initialization
            isInitialized = IsLlmInitialized();
            if (!isInitialized)
            {
                LogEvent("❌ LLM service initialization failed - cannot perform code completion");
                ShowStatusMessage("LLM initialization failed. Cannot perform code completion.", StatusMessageType.Error);
                return;
            }
            LogEvent("✅ LLM service initialized successfully");
        }

        try
        {
            var caret = _codeEditor.SelectionStart;
            var codeBeforeCaret = _codeEditor.Text.Substring(0, Math.Min(caret, _codeEditor.Text.Length));

            var prompt = "You are the CX Language coding assistant. Continue the following CX source code. " +
                         "Return only the next lines of code (no explanations). If a block was started, continue it.\n\n" +
                         "CX code:\n" + codeBeforeCaret;

            var payload = new Dictionary<string, object>
            {
                ["prompt"] = prompt,
                ["maxTokens"] = 128,
                ["temperature"] = 0.2, // Lower temperature for more focused completions
                ["source"] = "cx.ide",
                ["mode"] = "completion"
            };

            LogEvent("🔮 Requesting CX code completion from Local LLM...");
            ShowStatusMessage("Generating code completion...", StatusMessageType.Info);
            await _eventBus.EmitAsync("llm.generate", payload);
        }
        catch (Exception ex)
        {
            LogEvent($"❌ Failed to request completion: {ex.Message}");
            if (ex.InnerException != null)
            {
                LogEvent($"   ↳ Inner exception: {ex.InnerException.Message}");
            }
            ShowConsciousnessProcessingError("Code completion failed", ex);
        }
    }
    
    // Helper method to check if LLM is initialized
    private bool IsLlmInitialized()
    {
        try
        {
            // Access the LocalLLM service through the runtime helper
            var localLLMService = CxLanguage.Runtime.CxRuntimeHelper.GetService("LocalLLMService") as CxLanguage.LocalLLM.ILocalLLMService;
            
            // Log detailed status for debugging
            if (localLLMService == null)
            {
                LogEvent("⚠️ LLM service not found in runtime helper");
                return false;
            }
            
            // Check if model is loaded and log status
            var modelLoaded = localLLMService.IsModelLoaded;
            LogEvent($"📊 LLM status check: Service available, Model loaded: {modelLoaded}");
            
            if (!modelLoaded)
            {
                // If model isn't loaded, try to initialize it now
                LogEvent("🔄 Model not loaded, attempting immediate initialization");
                _ = Task.Run(async () => await InitializeLocalLlm());
            }
            
            return modelLoaded;
        }
        catch (Exception ex)
        {
            LogEvent($"⚠️ Error checking LLM initialization status: {ex.Message}");
            return false;
        }
    }
    
    // Helper method to show a standardized error message
    private void ShowConsciousnessProcessingError(string operation, Exception ex)
    {
        LogEvent($"❌ Consciousness processing encountered an issue: {ex.Message}");
        LogEvent($"Stack trace: {ex.StackTrace}");
        
        MessageBox.Show(
            $"Consciousness processing encountered an issue: {ex.Message}\n\nPlease check the log for details.",
            "Consciousness Processing Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private async Task InitializeLocalLlm()
    {
        if (_eventBus == null)
        {
            LogEvent("⚠️ No event bus connection - attempting to reinitialize event bus");
            InitializeCxEventConnection(); // Try to reinitialize event bus connection
            
            if (_eventBus == null)
            {
                LogEvent("❌ Event bus connection still unavailable - cannot initialize LLM");
                return;
            }
        }

        try
        {
            LogEvent("🚀 Automatically initializing LLM service...");
            
            // Check for LocalLLM service availability in runtime
            var localLLMService = CxLanguage.Runtime.CxRuntimeHelper.GetService("LocalLLMService") as CxLanguage.LocalLLM.ILocalLLMService;
            if (localLLMService == null)
            {
                LogEvent("⚠️ LocalLLM service not found in runtime - attempting to create it now");
                
                // Create a new LocalLLM service instance
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var llmLogger = loggerFactory.CreateLogger<CxLanguage.LocalLLM.GpuLocalLLMService>();
                
                try 
                {
                    localLLMService = new CxLanguage.LocalLLM.GpuLocalLLMService(llmLogger);
                    
                    // Register it for static access
                    CxLanguage.Runtime.CxRuntimeHelper.RegisterService("LocalLLMService", localLLMService);
                    LogEvent("✅ Created and registered new LocalLLM service instance");
                    
                    // Also create and register event handler for the LLM service
                    var handlerLogger = loggerFactory.CreateLogger<CxLanguage.LocalLLM.LocalLlmEventHandler>();
                    try 
                    {
                        var handler = new CxLanguage.LocalLLM.LocalLlmEventHandler(handlerLogger, _eventBus, (CxLanguage.LocalLLM.GpuLocalLLMService)localLLMService);
                        LogEvent("✅ Created LocalLLM event handler");
                    }
                    catch (Exception ex)
                    {
                        LogEvent($"⚠️ Failed to create LocalLLM event handler: {ex.Message}");
                        LogEvent($"   ↳ Creating event handler manually...");
                        
                        // Subscribe to events manually if handler creation fails
                        _eventBus.Subscribe("llm.initialize", async (sender, eventName, data) => {
                            try {
                                await localLLMService.InitializeAsync();
                                var resultPayload = new Dictionary<string, object> {
                                    { "gpuAvailable", ((CxLanguage.LocalLLM.GpuLocalLLMService)localLLMService).IsGpuAvailable() },
                                    { "modelName", localLLMService.ModelInfo?.Name ?? "unknown" },
                                    { "modelLoaded", localLLMService.IsModelLoaded }
                                };
                                await _eventBus.EmitAsync("llm.initialized", resultPayload);
                                return true;
                            } catch (Exception initEx) {
                                LogEvent($"❌ LLM initialization failed: {initEx.Message}");
                                return false;
                            }
                        });
                        
                        _eventBus.Subscribe("llm.generate", async (sender, eventName, data) => {
                            try {
                                if (data?.TryGetValue("prompt", out var promptObj) == true) {
                                    string prompt = promptObj?.ToString() ?? "";
                                    string result = await localLLMService.GenerateAsync(prompt);
                                    var resultPayload = new Dictionary<string, object> { { "result", result } };
                                    await _eventBus.EmitAsync("llm.generated", resultPayload);
                                }
                                return true;
                            } catch (Exception genEx) {
                                LogEvent($"❌ LLM generation failed: {genEx.Message}");
                                return false;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"❌ Failed to create LocalLLM service: {ex.Message}");
                    return;
                }
            }
            else
            {
                LogEvent("✅ LocalLLM service found in runtime");
            }

            // Send event to initialize the LLM service
            var payload = new Dictionary<string, object>
            {
                ["useGpu"] = true,
                ["modelName"] = "Llama-3.2-3B-Instruct-Q4_K_M.gguf",
                ["source"] = "cx.ide"
            };
            LogEvent("⚙️ Initializing Local LLM via events (GPU, Llama-3.2-3B-Instruct-Q4_K_M.gguf)...");
            await _eventBus.EmitAsync("llm.initialize", payload);
            
            // Also try loading model directly if service is available
            if (localLLMService != null)
            {
                LogEvent("🔄 Attempting direct LLM initialization...");
                bool initResult = false;
                
                try
                {
                    // First try to find the model paths directly
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var workspaceRoot = Path.GetFullPath(Path.Combine(baseDir, ".."));
                    var parentDir = Directory.GetParent(workspaceRoot);
                    
                    // Log current directory information for debugging
                    LogEvent($"🔍 Current directory: {Environment.CurrentDirectory}");
                    LogEvent($"🔍 Base directory: {baseDir}");
                    LogEvent($"🔍 Initial workspace root: {workspaceRoot}");
                    
                    // Try to find workspace root by looking for characteristic files/folders
                    int searchDepth = 0;
                    const int maxDepth = 5;
                    
                    while (!Directory.Exists(Path.Combine(workspaceRoot, "models")) && 
                           !File.Exists(Path.Combine(workspaceRoot, "CxLanguage.sln")) &&
                           parentDir != null && searchDepth < maxDepth)
                    {
                        workspaceRoot = parentDir.FullName;
                        parentDir = Directory.GetParent(workspaceRoot);
                        searchDepth++;
                    }
                    
                    LogEvent($"🔍 Found workspace root: {workspaceRoot} (search depth: {searchDepth})");
                    
                    // If we still don't have a good path, try some hardcoded paths
                    if (!Directory.Exists(Path.Combine(workspaceRoot, "models")) &&
                        !File.Exists(Path.Combine(workspaceRoot, "CxLanguage.sln")))
                    {
                        string[] potentialRoots = {
                            @"C:\Users\a7qBIOyPiniwRue6UVvF\cx",
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cx")
                        };
                        
                        foreach (var root in potentialRoots)
                        {
                            LogEvent($"🔍 Trying hardcoded path: {root}");
                            if (Directory.Exists(root) && 
                                (Directory.Exists(Path.Combine(root, "models")) || 
                                 File.Exists(Path.Combine(root, "CxLanguage.sln"))))
                            {
                                workspaceRoot = root;
                                LogEvent($"✅ Using hardcoded workspace root: {workspaceRoot}");
                                break;
                            }
                        }
                    }
                    
                    // Check multiple model locations
                    var possibleModelPaths = new List<string>
                    {
                        Path.Combine(workspaceRoot, "models", "local_llm", "Llama-3.2-3B-Instruct-Q4_K_M.gguf"),
                        Path.Combine(workspaceRoot, "models", "local_llm", "Phi-3-mini-4k-instruct-q4.gguf"),
                        Path.Combine(workspaceRoot, "models", "Llama-3.2-3B-Instruct-Q4_K_M.gguf"),
                        Path.Combine(workspaceRoot, "models", "Phi-3-mini-4k-instruct-q4.gguf")
                    };
                    
                    string? modelPath = null;
                    
                    // Check each possible path
                    foreach (var path in possibleModelPaths)
                    {
                        LogEvent($"🔍 Checking model path: {path} - Exists: {File.Exists(path)}");
                        if (File.Exists(path))
                        {
                            modelPath = path;
                            LogEvent($"✅ Found GGUF model file: {modelPath}");
                            break;
                        }
                    }
                    
                    // If we didn't find any of our specific models, look for any .gguf file
                    if (modelPath == null)
                    {
                        try
                        {
                            var modelsDir = Path.Combine(workspaceRoot, "models");
                            if (Directory.Exists(modelsDir))
                            {
                                var ggufFiles = Directory.GetFiles(modelsDir, "*.gguf", SearchOption.AllDirectories);
                                if (ggufFiles.Length > 0)
                                {
                                    modelPath = ggufFiles[0];
                                    LogEvent($"✅ Found GGUF model file (search): {modelPath}");
                                }
                                else
                                {
                                    LogEvent($"⚠️ No .gguf files found in {modelsDir}");
                                }
                            }
                            else
                            {
                                // Try searching from current directory
                                var ggufFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.gguf", SearchOption.AllDirectories);
                                if (ggufFiles.Length > 0)
                                {
                                    modelPath = ggufFiles[0];
                                    LogEvent($"✅ Found GGUF model file (current dir search): {modelPath}");
                                }
                                else
                                {
                                    LogEvent($"⚠️ No .gguf files found anywhere");
                                }
                            }
                        }
                        catch (Exception searchEx)
                        {
                            LogEvent($"⚠️ Error searching for GGUF files: {searchEx.Message}");
                        }
                    }
                    
                    // Set the model path directly when initializing
                    if (modelPath != null)
                    {
                        LogEvent($"🔄 Initializing with direct model path: {modelPath}");
                        
                        // Use reflection to call a LoadModelAsync method if available
                        var methodInfo = localLLMService.GetType().GetMethod("LoadModelAsync", new[] { typeof(string), typeof(CancellationToken) });
                        if (methodInfo != null)
                        {
                            LogEvent("🔧 Found LoadModelAsync method, calling directly...");
                            var result = methodInfo.Invoke(localLLMService, new object[] { modelPath, CancellationToken.None });
                            if (result is Task<bool> task)
                            {
                                initResult = await task;
                            }
                            else
                            {
                                LogEvent("⚠️ Method invocation didn't return expected Task<bool>");
                                initResult = false;
                            }
                        }
                        else
                        {
                            // Fall back to InitializeAsync
                            initResult = await localLLMService.InitializeAsync();
                        }
                    }
                    else
                    {
                        // Use standard initialization
                        initResult = await localLLMService.InitializeAsync();
                    }
                    
                    LogEvent($"📊 Direct LLM initialization result: {initResult}");
                    
                    if (initResult)
                    {
                        LogEvent("✅ LLM service initialized successfully via direct call");
                        
                        // Check if we need to explicitly register this again for code generator
                        var codeGenService = CxLanguage.Runtime.CxRuntimeHelper.GetService("ILocalLLMService");
                        if (codeGenService == null)
                        {
                            CxLanguage.Runtime.CxRuntimeHelper.RegisterService("ILocalLLMService", localLLMService);
                            LogEvent("✅ Registered LLM service under ILocalLLMService interface name");
                        }
                        
                        // Initialize code generator now that LLM is available
                        if (_codeGenerator == null)
                        {
                            var vectorStoreService = CxLanguage.Runtime.CxRuntimeHelper.GetService("IVectorStoreService") as IVectorStoreService;
                            if (vectorStoreService != null)
                            {
                                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                                var generatorLogger = loggerFactory.CreateLogger<IntelligentCxCodeGenerator>();
                                _codeGenerator = new IntelligentCxCodeGenerator(generatorLogger, _eventBus, localLLMService, vectorStoreService);
                                LogEvent("✅ Intelligent CX code generator initialized after LLM initialization");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"⚠️ Direct LLM initialization error: {ex.Message}");
                    LogEvent($"⚠️ Error type: {ex.GetType().Name}");
                    
                    if (ex.InnerException != null)
                    {
                        LogEvent($"⚠️ Inner exception: {ex.InnerException.Message}");
                    }
                    
                    var fullPath = typeof(CxLanguage.LocalLLM.GpuLocalLLMService).Assembly.Location;
                    LogEvent($"📋 LocalLLM assembly location: {fullPath}");
                }
            }
        }
        catch (Exception ex)
        {
            LogEvent($"❌ Failed to automatically initialize LLM: {ex.Message}");
            LogEvent($"❌ Error type: {ex.GetType().Name}");
            
            if (ex.InnerException != null)
            {
                LogEvent($"❌ Inner exception: {ex.InnerException.Message}");
            }
            
            // Display error message to the user
            Invoke(new Action(() => {
                MessageBox.Show(
                    $"Failed to initialize Local LLM: {ex.Message}\n\nCode completion and generation features will not be available.",
                    "CX IDE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }));
        }
    }

    // --- File operations ---
    private void NewFile()
    {
        if (_codeEditor == null) return;
        _codeEditor.Clear();
        _codeEditor.Text = "// New CX file\nrealize(self: conscious) {\n    learn self;\n}\n";
        _currentFilePath = null;
        UpdateFileStatus();
    }

    private void OpenFile()
    {
        if (_codeEditor == null) return;
        using var ofd = new OpenFileDialog
        {
            Filter = "CX Files (*.cx)|*.cx|All Files (*.*)|*.*",
            Title = "Open CX File"
        };
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                var text = File.ReadAllText(ofd.FileName);
                _codeEditor.Text = text;
                _currentFilePath = ofd.FileName;
                UpdateFileStatus();
                LogEvent($"📂 Opened file: {_currentFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to open file: {ex.Message}", "Open File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void SaveFile()
    {
        if (_currentFilePath == null)
        {
            SaveFileAs();
            return;
        }
        try
        {
            File.WriteAllText(_currentFilePath, _codeEditor?.Text ?? string.Empty);
            UpdateFileStatus();
            LogEvent($"💾 Saved: {_currentFilePath}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to save file: {ex.Message}", "Save File", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveFileAs()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CX Files (*.cx)|*.cx|All Files (*.*)|*.*",
            Title = "Save CX File"
        };
        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                File.WriteAllText(sfd.FileName, _codeEditor?.Text ?? string.Empty);
                _currentFilePath = sfd.FileName;
                UpdateFileStatus();
                LogEvent($"💾 Saved As: {_currentFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to save file: {ex.Message}", "Save File As", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void UpdateFileStatus()
    {
        var name = _currentFilePath != null ? Path.GetFileName(_currentFilePath) : "[No file]";
        if (_fileStatusLabel != null) _fileStatusLabel.Text = name;
        Text = _currentFilePath != null ? $"CX IDE - {name}" : "CX IDE";
    }

    private async Task ExecuteCurrentCode()
    {
        if (_cxCompiler == null)
        {
            LogEvent("⚠️ CX compiler not initialized - falling back to event-based execution");
            await ExecuteCurrentCodeViaEvents();
            return;
        }
        
        if (string.IsNullOrWhiteSpace(_codeEditor?.Text))
        {
            LogEvent("⚠️ No CX code to execute");
            UpdateExecutionResults("⚠️ No CX code in editor. Please write or generate some code first.");
            return;
        }

        try
        {
            var cxCode = _codeEditor.Text;
            LogEvent($"▶️ Executing CX code via real-time compiler...");
            UpdateExecutionResults("▶️ Compiling and executing CX code...");
            
            var result = await _cxCompiler.CompileAndExecuteAsync(cxCode);
            _lastExecutionResult = result;
            
            if (result.Success)
            {
                LogEvent("✅ CX code execution completed successfully");
                var resultText = $"✅ Execution successful!\n\n" +
                               $"⏱️ Compilation Time: {result.CompilationTimeMs}ms\n" +
                               $"⚡ Execution Time: {result.ExecutionTimeMs}ms\n";
                
                if (!string.IsNullOrWhiteSpace(result.Output))
                {
                    resultText += $"\n📤 Output:\n{result.Output}";
                }
                
                if (result.EventsEmitted > 0)
                {
                    resultText += $"\n🌊 Events Emitted: {result.EventsEmitted}";
                }
                
                UpdateExecutionResults(resultText);
            }
            else
            {
                LogEvent($"❌ CX code execution failed: {result.ErrorMessage}");
                var errorText = $"❌ Execution failed!\n\n" +
                              $"Error: {result.ErrorMessage}\n";
                
                if (!string.IsNullOrWhiteSpace(result.Output))
                {
                    errorText += $"\nOutput before error:\n{result.Output}";
                }
                
                UpdateExecutionResults(errorText);
            }
        }
        catch (Exception ex)
        {
            LogEvent($"❌ Failed to execute code: {ex.Message}");
            UpdateExecutionResults($"❌ Execution failed: {ex.Message}");
            
            // Fallback to event-based execution
            LogEvent("🔄 Falling back to event-based execution...");
            await ExecuteCurrentCodeViaEvents();
        }
    }
    
    private async Task ExecuteCurrentCodeViaEvents()
    {
        if (_eventBus == null)
        {
            LogEvent("⚠️ No event bus connection - cannot execute code");
            UpdateExecutionResults("❌ Cannot execute code: No event bus connection available.");
            return;
        }
        var code = _codeEditor?.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            LogEvent("⚠️ Nothing to execute - editor is empty");
            UpdateExecutionResults("⚠️ Nothing to execute - editor is empty.");
            return;
        }
        try
        {
            var payload = new Dictionary<string, object>
            {
                ["source"] = "cx.ide",
                ["code"] = code,
                ["filePath"] = _currentFilePath ?? string.Empty,
                ["timestamp"] = DateTime.Now.ToString("O"),
                ["executionId"] = Guid.NewGuid().ToString()
            };
            LogEvent("🚀 Executing CX program via runtime events...");
            UpdateExecutionResults("🚀 Executing CX program via runtime events...\n\nNote: This is event-based execution mode.\nFor full compilation features, initialize the CX compiler service.");
            
            // Create a simple execution result to track progress
            _lastExecutionResult = new CxExecutionResult
            {
                Success = false,
                Output = "Execution in progress...",
                ExecutionTimeMs = 0,
                CompilationTimeMs = 0,
                EventsEmitted = 0,
                ErrorMessage = null
            };
            
            // Send the execution event
            await _eventBus.EmitAsync("cx.ide.execute", payload);
            
            // Update to indicate that event was sent
            var currentText = _executionResultsTextBox?.Text ?? "";
            UpdateExecutionResults(currentText + "\n\n✅ Event 'cx.ide.execute' sent to runtime.\nCheck the event log for processing results.");
        }
        catch (Exception ex)
        {
            LogEvent($"❌ Error executing code via events: {ex.Message}");
            UpdateExecutionResults($"❌ Error executing code via events: {ex.Message}");
        }
    }

    // --- Real-Time CX Development Workflow Methods ---
    
    private async Task GenerateCodeFromPrompt()
    {
        if (_codeGenerator == null)
        {
            LogEvent("⚠️ Code generator not initialized");
            UpdateExecutionResults("❌ Code generator not ready. Please initialize services first.");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(_promptTextBox?.Text))
        {
            LogEvent("⚠️ No prompt provided for code generation");
            UpdateExecutionResults("⚠️ Please enter a prompt describing the CX code you want to generate.");
            return;
        }

        try
        {
            var prompt = _promptTextBox.Text;
            LogEvent($"🤖 Generating CX code from prompt: {prompt}");
            UpdateExecutionResults("🤖 Generating CX code...");
            
            var generatedCodeResult = await _codeGenerator.GenerateCxCodeAsync(prompt);
            
            if (generatedCodeResult.Success && !string.IsNullOrWhiteSpace(generatedCodeResult.GeneratedCode))
            {
                var generatedCode = generatedCodeResult.GeneratedCode;
                
                // Update the code editor with generated code
                if (_codeEditor != null)
                {
                    _codeEditor.Text = generatedCode;
                }
                
                LogEvent("✅ CX code generation completed");
                UpdateExecutionResults($"✅ Code generated successfully!\n\nGenerated Code:\n{generatedCode}");
            }
            else
            {
                var errorMessage = generatedCodeResult.Error ?? "Code generation did not produce any output";
                LogEvent($"⚠️ Code generation failed: {errorMessage}");
                UpdateExecutionResults($"⚠️ Code generation failed: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            LogEvent($"❌ Failed to generate code: {ex.Message}");
            UpdateExecutionResults($"❌ Code generation failed: {ex.Message}");
        }
    }
    
    private void UpdateExecutionResults(string text)
    {
        if (_executionResultsTextBox != null)
        {
            _executionResultsTextBox.Text = text;
            _executionResultsTextBox.SelectionStart = _executionResultsTextBox.Text.Length;
            _executionResultsTextBox.ScrollToCaret();
        }
    }
}
