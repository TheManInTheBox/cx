using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CxLanguage.UnityServer.Core.Interpreter
{
    /// <summary>
    /// Hot-swappable interpreter manager for CX Language evolution
    /// Enables runtime language primitive updates without server downtime
    /// </summary>
    public interface IHotSwapInterpreterManager
    {
        Task InitializeAsync();
        Task LoadCoreInterpretersAsync();
        Task<IInterpreter> GetInterpreterAsync(string languageVersion);
        Task SwapInterpreterAsync(string interpreterName, IInterpreter newInterpreter);
        Task<bool> ValidateInterpreterAsync(IInterpreter interpreter);
        Task<List<string>> GetAvailableInterpretersAsync();
        Task<InterpreterMetrics> GetInterpreterMetricsAsync(string interpreterName);
    }

    /// <summary>
    /// Hot-swappable interpreter manager implementation
    /// Manages multiple interpreter versions and seamless runtime updates
    /// </summary>
    public class HotSwapInterpreterManager : IHotSwapInterpreterManager
    {
        private readonly ILogger<HotSwapInterpreterManager> _logger;
        private readonly IInterpreterFactory _interpreterFactory;
        private readonly IInterpreterValidator _validator;
        
        // Concurrent dictionary for thread-safe interpreter management
        private readonly ConcurrentDictionary<string, InterpreterContainer> _interpreters;
        private readonly ConcurrentDictionary<string, InterpreterMetrics> _metrics;
        
        // Version management
        private readonly ConcurrentDictionary<string, string> _versionMapping;
        private string _defaultVersion = "1.0";

        public HotSwapInterpreterManager(
            ILogger<HotSwapInterpreterManager> logger,
            IInterpreterFactory interpreterFactory,
            IInterpreterValidator validator)
        {
            _logger = logger;
            _interpreterFactory = interpreterFactory;
            _validator = validator;
            
            _interpreters = new ConcurrentDictionary<string, InterpreterContainer>();
            _metrics = new ConcurrentDictionary<string, InterpreterMetrics>();
            _versionMapping = new ConcurrentDictionary<string, string>();
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("🔄 Initializing Hot-Swap Interpreter Manager");
            
            // Initialize metrics collection
            foreach (var interpreter in _interpreters.Values)
            {
                _metrics.TryAdd(interpreter.Name, new InterpreterMetrics
                {
                    Name = interpreter.Name,
                    Version = interpreter.Version,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            _logger.LogInformation("✅ Hot-Swap Interpreter Manager initialized");
        }

        public async Task LoadCoreInterpretersAsync()
        {
            _logger.LogInformation("📦 Loading core CX Language interpreters");

            try
            {
                // Load standard CX Language interpreter
                var cxInterpreter = await _interpreterFactory.CreateCxInterpreterAsync("1.0");
                await RegisterInterpreterAsync("cx-standard", "1.0", cxInterpreter);
                
                // Load consciousness-aware interpreter
                var consciousnessInterpreter = await _interpreterFactory.CreateConsciousnessInterpreterAsync("1.0");
                await RegisterInterpreterAsync("cx-consciousness", "1.0", consciousnessInterpreter);
                
                // Load Unity-specific interpreter
                var unityInterpreter = await _interpreterFactory.CreateUnityInterpreterAsync("1.0");
                await RegisterInterpreterAsync("cx-unity", "1.0", unityInterpreter);
                
                // Load AI-enhanced interpreter
                var aiInterpreter = await _interpreterFactory.CreateAiInterpreterAsync("1.0");
                await RegisterInterpreterAsync("cx-ai", "1.0", aiInterpreter);
                
                // Set default version mapping
                _versionMapping.TryAdd("1.0", "cx-consciousness");
                _versionMapping.TryAdd("latest", "cx-consciousness");
                
                _logger.LogInformation("✅ Core interpreters loaded: {Count} interpreters", _interpreters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load core interpreters");
                throw;
            }
        }

        public async Task<IInterpreter> GetInterpreterAsync(string languageVersion)
        {
            try
            {
                // Resolve version to interpreter name
                var interpreterName = _versionMapping.GetValueOrDefault(languageVersion, _versionMapping[_defaultVersion]);
                
                if (_interpreters.TryGetValue(interpreterName, out var container))
                {
                    // Update usage metrics
                    await UpdateInterpreterMetricsAsync(interpreterName);
                    
                    _logger.LogDebug("🔄 Retrieved interpreter: {InterpreterName} for version: {Version}", 
                        interpreterName, languageVersion);
                    
                    return container.Interpreter;
                }
                
                // Fallback to default interpreter
                _logger.LogWarning("⚠️ Interpreter not found for version: {Version}, using default", languageVersion);
                var defaultInterpreterName = _versionMapping[_defaultVersion];
                return _interpreters[defaultInterpreterName].Interpreter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving interpreter for version: {Version}", languageVersion);
                throw;
            }
        }

        public async Task SwapInterpreterAsync(string interpreterName, IInterpreter newInterpreter)
        {
            _logger.LogInformation("🔄 Hot-swapping interpreter: {InterpreterName}", interpreterName);

            try
            {
                // Validate new interpreter
                var isValid = await _validator.ValidateAsync(newInterpreter);
                if (!isValid)
                {
                    throw new InvalidOperationException($"New interpreter validation failed: {interpreterName}");
                }
                
                // Create new container
                var newContainer = new InterpreterContainer
                {
                    Name = interpreterName,
                    Version = newInterpreter.Version,
                    Interpreter = newInterpreter,
                    LoadedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                // Perform atomic swap
                InterpreterContainer? oldContainer = null;
                _interpreters.AddOrUpdate(interpreterName, newContainer, (key, existing) =>
                {
                    oldContainer = existing;
                    existing.IsActive = false; // Mark old interpreter as inactive
                    return newContainer;
                });
                
                // Initialize new interpreter
                await newInterpreter.InitializeAsync();
                
                // Update metrics
                _metrics.AddOrUpdate(interpreterName, new InterpreterMetrics
                {
                    Name = interpreterName,
                    Version = newInterpreter.Version,
                    CreatedAt = DateTime.UtcNow,
                    SwapCount = oldContainer != null ? _metrics.GetValueOrDefault(interpreterName, new InterpreterMetrics()).SwapCount + 1 : 0
                }, (key, existing) =>
                {
                    existing.Version = newInterpreter.Version;
                    existing.SwapCount++;
                    existing.LastSwapAt = DateTime.UtcNow;
                    return existing;
                });
                
                // Gracefully dispose old interpreter after a delay
                if (oldContainer != null)
                {
                    _ = Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(async _ =>
                    {
                        try
                        {
                            await oldContainer.Interpreter.DisposeAsync();
                            _logger.LogInformation("🗑️ Disposed old interpreter: {InterpreterName}", interpreterName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Error disposing old interpreter: {InterpreterName}", interpreterName);
                        }
                    });
                }
                
                _logger.LogInformation("✅ Hot-swapped interpreter: {InterpreterName} to version: {Version}", 
                    interpreterName, newInterpreter.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to hot-swap interpreter: {InterpreterName}", interpreterName);
                throw;
            }
        }

        public async Task<bool> ValidateInterpreterAsync(IInterpreter interpreter)
        {
            try
            {
                return await _validator.ValidateAsync(interpreter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error validating interpreter");
                return false;
            }
        }

        public async Task<List<string>> GetAvailableInterpretersAsync()
        {
            await Task.CompletedTask;
            return new List<string>(_interpreters.Keys);
        }

        public async Task<InterpreterMetrics> GetInterpreterMetricsAsync(string interpreterName)
        {
            await Task.CompletedTask;
            return _metrics.GetValueOrDefault(interpreterName, new InterpreterMetrics { Name = interpreterName });
        }

        private async Task RegisterInterpreterAsync(string name, string version, IInterpreter interpreter)
        {
            var container = new InterpreterContainer
            {
                Name = name,
                Version = version,
                Interpreter = interpreter,
                LoadedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            _interpreters.TryAdd(name, container);
            
            // Initialize interpreter
            await interpreter.InitializeAsync();
            
            _logger.LogInformation("📦 Registered interpreter: {Name} version: {Version}", name, version);
        }

        private async Task UpdateInterpreterMetricsAsync(string interpreterName)
        {
            if (_metrics.TryGetValue(interpreterName, out var metrics))
            {
                metrics.UsageCount++;
                metrics.LastUsedAt = DateTime.UtcNow;
            }
            
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Container for interpreter instances with metadata
    /// </summary>
    public class InterpreterContainer
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public IInterpreter Interpreter { get; set; } = null!;
        public DateTime LoadedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Metrics for interpreter usage and performance
    /// </summary>
    public class InterpreterMetrics
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime? LastSwapAt { get; set; }
        public long UsageCount { get; set; }
        public int SwapCount { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public long TotalExecutions { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
    }

    /// <summary>
    /// Base interface for CX Language interpreters
    /// </summary>
    public interface IInterpreter : IAsyncDisposable
    {
        string Name { get; }
        string Version { get; }
        Task InitializeAsync();
        Task<CxExecutionResult> ExecuteAsync(CxAst ast, IActorModelRuntime runtime, CxExecutionContext context);
        Task<bool> CanExecuteAsync(CxAst ast);
        Task<CxValidationResult> ValidateAsync(CxAst ast);
    }

    /// <summary>
    /// Factory for creating specialized interpreters
    /// </summary>
    public interface IInterpreterFactory
    {
        Task<IInterpreter> CreateCxInterpreterAsync(string version);
        Task<IInterpreter> CreateConsciousnessInterpreterAsync(string version);
        Task<IInterpreter> CreateUnityInterpreterAsync(string version);
        Task<IInterpreter> CreateAiInterpreterAsync(string version);
        Task<IInterpreter> CreateCustomInterpreterAsync(string name, string version, InterpreterConfiguration config);
    }

    /// <summary>
    /// Validator for interpreter safety and compatibility
    /// </summary>
    public interface IInterpreterValidator
    {
        Task<bool> ValidateAsync(IInterpreter interpreter);
        Task<ValidationReport> GetValidationReportAsync(IInterpreter interpreter);
        Task<bool> IsCompatibleAsync(IInterpreter interpreter, string targetVersion);
    }

    /// <summary>
    /// Configuration for custom interpreter creation
    /// </summary>
    public class InterpreterConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, object> Settings { get; set; } = new();
        public List<string> EnabledFeatures { get; set; } = new();
        public List<string> DisabledFeatures { get; set; } = new();
        public string BaseInterpreter { get; set; } = "cx-standard";
    }

    /// <summary>
    /// Result of interpreter validation
    /// </summary>
    public class ValidationReport
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> SupportedFeatures { get; set; } = new();
        public string CompatibilityLevel { get; set; } = string.Empty;
    }
}
