using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CxLanguage.Runtime;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Linq;

namespace CxLanguage.StandardLibrary.Services
{
    /// <summary>
    /// Dr. Phoenix "StreamDX" Harper - Revolutionary Stream IDE Architecture
    /// Developer Terminal Service - Consciousness-Aware Interactive Development
    /// </summary>
    public class DeveloperTerminalService : IHostedService
    {
        private readonly ILogger<DeveloperTerminalService> _logger;
        private readonly ICxEventBus _eventBus;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _inputTask;
        private bool _isActive = false;
        private string _currentPrompt = "cx> ";

        public DeveloperTerminalService(ILogger<DeveloperTerminalService> logger, ICxEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛠️ Developer Terminal Service - Dr. Harper's Stream IDE Architecture");
            _logger.LogInformation("⌨️ Consciousness-aware interactive development terminal starting...");

            // Subscribe to terminal control events
            _eventBus.Subscribe("terminal.start", OnTerminalStart);
            _eventBus.Subscribe("terminal.stop", OnTerminalStop);
            _eventBus.Subscribe("terminal.prompt.set", OnPromptSet);
            _eventBus.Subscribe("terminal.command.execute", OnCommandExecute);
            _eventBus.Subscribe("system.shutdown", OnSystemShutdown);
            
            // Subscribe to AI response events for the behavior flow
            _eventBus.Subscribe("ai.think.response", OnAiThinkResponse);
            _eventBus.Subscribe("ai.think.needs_more_info", OnAiNeedsMoreInfo);

            _logger.LogInformation("✅ Developer Terminal Service registered for consciousness events");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔄 Developer Terminal Service stopping...");
            
            _isActive = false;
            _cancellationTokenSource.Cancel();
            
            if (_inputTask != null && !_inputTask.IsCompleted)
            {
                try
                {
                    _inputTask.Wait(TimeSpan.FromSeconds(2));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Terminal input task cleanup warning: {ex.Message}");
                }
            }

            _logger.LogInformation("✅ Developer Terminal Service stopped");
            return Task.CompletedTask;
        }

        private void OnTerminalStart(CxEvent eventData)
        {
            _logger.LogInformation("🚀 Developer Terminal - Starting consciousness-aware session");
            
            _isActive = true;
            
            // Display welcome message
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  🧠 CX LANGUAGE DEVELOPER TERMINAL - Dr. Harper's Stream IDE ║");
            Console.WriteLine("║  🎮 Consciousness-Aware Interactive Development Environment   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("💡 Available Commands:");
            Console.WriteLine("   /help          - Show all commands");
            Console.WriteLine("   /run <script>  - Execute CX script");
            Console.WriteLine("   /compile       - Compile current workspace");
            Console.WriteLine("   /debug         - Enable debug mode");
            Console.WriteLine("   /events        - Show event bus status");
            Console.WriteLine("   /clear         - Clear terminal");
            Console.WriteLine("   /exit          - Exit terminal");
            Console.WriteLine();
            Console.WriteLine("🧠 Type any CX code for immediate consciousness processing!");
            Console.WriteLine();

            // Start the input processing task
            _inputTask = Task.Run(async () => await ProcessInputAsync(_cancellationTokenSource.Token));
        }

        private async Task ProcessInputAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("✅ Terminal input loop started");
            
            while (_isActive && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.Write(_currentPrompt);
                    
                    // Simple synchronous readline - no complex async needed
                    var input = Console.ReadLine();

                    if (cancellationToken.IsCancellationRequested || string.IsNullOrEmpty(input))
                    {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        _logger.LogInformation("⌨️ Terminal input received: {Input}", input);
                        
                        // Check if it's a command or natural language
                        if (input.StartsWith("/"))
                        {
                            // Process as terminal command
                            ProcessCommand(input.Trim());
                        }
                        else
                        {
                            // Process as natural language
                            await ProcessNaturalLanguageInput(input.Trim());
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Terminal input loop cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in terminal input loop");
                    await Task.Delay(1000, cancellationToken);
                }
            }
            
            _logger.LogInformation("✅ Terminal input loop finished");
        }

        private void OnTerminalStop(CxEvent eventData)
        {
            _logger.LogInformation("🛑 Developer Terminal - Stopping consciousness session");
            _isActive = false;
            _cancellationTokenSource.Cancel();
        }

        private void OnPromptSet(CxEvent eventData)
        {
            var payload = eventData.payload as Dictionary<string, object>;
            if (payload != null && payload.ContainsKey("prompt"))
            {
                _currentPrompt = payload["prompt"]?.ToString() ?? "cx> ";
                _logger.LogDebug($"🎯 Terminal prompt updated: {_currentPrompt}");
            }
        }

        private void OnCommandExecute(CxEvent eventData)
        {
            var payload = eventData.payload as Dictionary<string, object>;
            if (payload != null && payload.ContainsKey("command"))
            {
                var command = payload["command"]?.ToString() ?? "";
                _logger.LogInformation($"⚡ Executing terminal command: {command}");
                ProcessCommand(command);
            }
        }

        private void OnSystemShutdown(CxEvent eventData)
        {
            _logger.LogInformation("🔄 System shutdown detected - gracefully stopping terminal");
            _isActive = false;
            _cancellationTokenSource.Cancel();
        }

        private async Task ProcessInputLine(string input)
        {
            try
            {
                if (input.StartsWith("/"))
                {
                    // Process terminal command
                    ProcessCommand(input);
                }
                else
                {
                    // Process as CX code - emit to consciousness framework
                    await _eventBus.EmitAsync("developer.code.input", new
                    {
                        code = input,
                        timestamp = DateTime.UtcNow,
                        source = "developer_terminal"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing input: {ex.Message}");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private void ProcessCommand(string command)
        {
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "/help":
                    ShowHelp();
                    break;

                case "/generate":
                    if (parts.Length > 1)
                    {
                        var naturalLanguage = string.Join(" ", parts.Skip(1));
                        _eventBus.EmitAsync("developer.natural.language.generate", new { 
                            input = naturalLanguage, 
                            timestamp = DateTime.UtcNow,
                            source = "terminal_command"
                        });
                        Console.WriteLine($"🧠 Generating CX code from: {naturalLanguage}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Usage: /generate <natural language description>");
                    }
                    break;

                case "/explain":
                    if (parts.Length > 1)
                    {
                        var cxCode = string.Join(" ", parts.Skip(1));
                        _eventBus.EmitAsync("developer.code.explain", new { 
                            code = cxCode, 
                            timestamp = DateTime.UtcNow 
                        });
                        Console.WriteLine($"🔍 Explaining CX code: {cxCode}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Usage: /explain <cx_code>");
                    }
                    break;

                case "/refactor":
                    if (parts.Length > 1)
                    {
                        var description = string.Join(" ", parts.Skip(1));
                        _eventBus.EmitAsync("developer.code.refactor", new { 
                            description = description, 
                            timestamp = DateTime.UtcNow 
                        });
                        Console.WriteLine($"🔧 Refactoring based on: {description}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Usage: /refactor <description>");
                    }
                    break;

                case "/pattern":
                    if (parts.Length > 1)
                    {
                        var intent = string.Join(" ", parts.Skip(1));
                        _eventBus.EmitAsync("developer.pattern.suggest", new { 
                            intent = intent, 
                            timestamp = DateTime.UtcNow 
                        });
                        Console.WriteLine($"🎯 Suggesting patterns for: {intent}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Usage: /pattern <intent>");
                    }
                    break;

                case "/voice":
                    if (parts.Length > 1)
                    {
                        var voiceCommand = string.Join(" ", parts.Skip(1));
                        _eventBus.EmitAsync("developer.voice.enable", new { 
                            command = voiceCommand, 
                            timestamp = DateTime.UtcNow 
                        });
                        Console.WriteLine($"🔊 Voice-enabling: {voiceCommand}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Usage: /voice <command>");
                    }
                    break;

                case "/consciousness":
                    if (parts.Length > 1)
                    {
                        var description = string.Join(" ", parts.Skip(1));
                        _eventBus.EmitAsync("developer.consciousness.add", new { 
                            description = description, 
                            timestamp = DateTime.UtcNow 
                        });
                        Console.WriteLine($"🧠 Adding consciousness features: {description}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Usage: /consciousness <description>");
                    }
                    break;

                case "/run":
                    if (parts.Length > 1)
                    {
                        var scriptPath = string.Join(" ", parts.Skip(1));
                        _eventBus.EmitAsync("developer.script.run", new { script = scriptPath });
                        Console.WriteLine($"🚀 Executing script: {scriptPath}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Usage: /run <script_path>");
                    }
                    break;

                case "/compile":
                    _eventBus.EmitAsync("developer.workspace.compile", new { timestamp = DateTime.UtcNow });
                    Console.WriteLine("🔧 Compiling workspace...");
                    break;

                case "/debug":
                    _eventBus.EmitAsync("developer.debug.toggle", new { enabled = true });
                    Console.WriteLine("🐛 Debug mode enabled");
                    break;

                case "/events":
                    _eventBus.EmitAsync("developer.events.status", new { request = "show_status" });
                    Console.WriteLine("📊 Event bus status requested");
                    break;

                case "/clear":
                    Console.Clear();
                    Console.WriteLine("🧠 CX Developer Terminal - Screen Cleared");
                    break;

                case "/exit":
                    Console.WriteLine("👋 Exiting developer terminal...");
                    _eventBus.EmitAsync("system.shutdown", new { reason = "developer_exit" });
                    break;

                default:
                    // Check if it's a natural language input (not starting with /)
                    if (!command.StartsWith("/"))
                    {
                        _ = ProcessNaturalLanguageInput(command);
                    }
                    else
                    {
                        Console.WriteLine($"❌ Unknown command: {cmd}");
                        Console.WriteLine("💡 Type /help for available commands");
                    }
                    break;
            }
        }

        private async Task ProcessNaturalLanguageInput(string input)
        {
            Console.WriteLine($"🧠 Processing natural language: {input}");
            
            // Send to AI for inference using the think service
            await _eventBus.EmitAsync("ai.think.request", new { 
                prompt = $"User input: {input}. Analyze this request and determine if you can complete it or if you need more information. If you need more info, specify exactly what additional details are required.",
                requestId = Guid.NewGuid().ToString(),
                timestamp = DateTime.UtcNow,
                source = "developer_terminal"
            });
        }

        private void OnAiThinkResponse(CxEvent eventData)
        {
            var payload = eventData.payload as Dictionary<string, object>;
            if (payload != null && payload.ContainsKey("result"))
            {
                var result = payload["result"]?.ToString() ?? "";

                // Clear the current line, print the response, and re-display the prompt.
                if (Console.IsOutputRedirected)
                {
                    Console.WriteLine($"🤖 AI Response: {result}");
                }
                else
                {
                    Console.Write("\r" + new string(' ', Console.BufferWidth > 0 ? Console.BufferWidth - 1 : 0) + "\r");
                    Console.WriteLine($"🤖 AI Response: {result}");
                    Console.Write(_currentPrompt);
                }
            }
        }

        private void OnAiNeedsMoreInfo(CxEvent eventData)
        {
            var payload = eventData.payload as Dictionary<string, object>;
            if (payload != null && payload.ContainsKey("question"))
            {
                var question = payload["question"]?.ToString() ?? "";
                _currentPrompt = "more info> ";

                // Clear the current line, print the question, and re-display the new prompt.
                if (Console.IsOutputRedirected)
                {
                    Console.WriteLine($"🤔 AI needs more info: {question}");
                }
                else
                {
                    Console.Write("\r" + new string(' ', Console.BufferWidth > 0 ? Console.BufferWidth - 1 : 0) + "\r");
                    Console.WriteLine($"🤔 AI needs more info: {question}");
                    Console.Write(_currentPrompt);
                }
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  🛠️ CX DEVELOPER TERMINAL - Dr. Harper's Stream IDE Commands  ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("� NATURAL LANGUAGE INPUT:");
            Console.WriteLine("   Just type natural language - it gets converted to CX code!");
            Console.WriteLine("   Example: create an agent that responds to user messages");
            Console.WriteLine("   Example: make the agent think about the input and respond");
            Console.WriteLine("   Example: add voice synthesis when the agent responds");
            Console.WriteLine();
            Console.WriteLine("�📝 DIRECT CX CODE INPUT:");
            Console.WriteLine("   Type CX code directly - it gets processed by consciousness!");
            Console.WriteLine("   Example: print(\"Hello Consciousness!\")");
            Console.WriteLine("   Example: emit user.message { text: \"test\" }");
            Console.WriteLine();
            Console.WriteLine("🧠 NATURAL LANGUAGE COMMANDS:");
            Console.WriteLine("   /generate <description>   - Generate CX code from natural language");
            Console.WriteLine("   /explain <cx_code>        - Explain CX code in natural language");
            Console.WriteLine("   /refactor <description>   - Refactor existing code based on description");
            Console.WriteLine("   /pattern <intent>         - Suggest optimal CX patterns for intent");
            Console.WriteLine("   /voice <command>          - Voice-enable existing functionality");
            Console.WriteLine("   /consciousness <desc>     - Add consciousness features to existing code");
            Console.WriteLine();
            Console.WriteLine("⚡ TERMINAL COMMANDS:");
            Console.WriteLine("   /help                     - Show this help");
            Console.WriteLine("   /run <script>             - Execute CX script file");
            Console.WriteLine("   /compile                  - Compile current workspace");
            Console.WriteLine("   /debug                    - Toggle debug mode");
            Console.WriteLine("   /events                   - Show event bus status");
            Console.WriteLine("   /clear                    - Clear terminal screen");
            Console.WriteLine("   /exit                     - Exit terminal and shutdown");
            Console.WriteLine();
            Console.WriteLine("🧠 CONSCIOUSNESS FEATURES:");
            Console.WriteLine("   - Natural language to CX code generation");
            Console.WriteLine("   - Real-time consciousness processing");
            Console.WriteLine("   - Event-driven architecture");
            Console.WriteLine("   - Hot reload capability");
            Console.WriteLine("   - Live debugging integration");
            Console.WriteLine("   - Voice-driven development");
            Console.WriteLine();
        }
    }
}
