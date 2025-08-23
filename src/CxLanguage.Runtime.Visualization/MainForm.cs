using System;
using System.Drawing;
using System.Windows.Forms;
using CxLanguage.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
    private System.Windows.Forms.Timer? _updateTimer;
    private int _realEventCount = 0;
    private DateTime _startTime = DateTime.Now;
    private ICxEventBus? _eventBus;

    public MainForm(ICxEventBus? eventBus = null)
    {
        // Set the event bus if provided
        _eventBus = eventBus;
        
        InitializeComponent();
        InitializeVisualization();
    }

    private void InitializeComponent()
    {
        // Form properties
        Text = "CX Consciousness Visualization";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(30, 30, 50);
        ForeColor = Color.White;

        // Title label
        _titleLabel = new Label
        {
            Text = "🧠 CX CONSCIOUSNESS VISUALIZATION",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 200, 255),
            Location = new Point(20, 20),
            Size = new Size(750, 40),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_titleLabel);

        // Status label
        _statusLabel = new Label
        {
            Text = "🟢 Consciousness System: ACTIVE",
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 255, 100),
            Location = new Point(20, 80),
            Size = new Size(750, 30),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_statusLabel);

        // Events label
        _eventsLabel = new Label
        {
            Text = "📊 Real-time Event Processing: 0 events (0.0s runtime)",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(200, 200, 200),
            Location = new Point(20, 120),
            Size = new Size(750, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_eventsLabel);

        // Event log text box
        _eventLogTextBox = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            BackColor = Color.FromArgb(20, 20, 30),
            ForeColor = Color.FromArgb(200, 200, 200),
            Font = new Font("Consolas", 9, FontStyle.Regular),
            Location = new Point(20, 160),
            Size = new Size(750, 380),
            Text = "🚀 CX Consciousness Visualization Starting...\r\n" +
                   "🔗 Event bus connection established\r\n" +
                   "✨ Neural pathways initialized\r\n" +
                   "🎯 Ready for consciousness monitoring\r\n\r\n"
        };
        Controls.Add(_eventLogTextBox);

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
        
        // Update status label to reflect connection state
        if (_eventBus != null)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = "🟢 Consciousness System: CONNECTED to CX Runtime";
                _statusLabel.ForeColor = Color.FromArgb(100, 255, 100);
            }
            LogEvent("⚡ Ready for real-time consciousness monitoring");
        }
        else
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = "🟡 Consciousness System: STANDALONE MODE (No CX Runtime)";
                _statusLabel.ForeColor = Color.FromArgb(255, 255, 100);
            }
            LogEvent("⚡ Ready for visualization (standalone mode)");
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
}
