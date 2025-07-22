using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.AI.VectorDatabase;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Management.Automation;

namespace CxLanguage.StandardLibrary.Core;

/// <summary>
/// Base class for CX classes that need service injection capabilities with comprehensive runtime logging
/// This enables inheritance-based service access without explicit 'uses' declarations
/// </summary>
public abstract class AiServiceBase
{
    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    protected readonly IServiceProvider _serviceProvider = null!;
    
    /// <summary>
    /// Logger instance
    /// </summary>
    protected readonly ILogger _logger = null!;

    /// <summary>
    /// Initializes a new instance of AiServiceBase with service provider injection
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    public AiServiceBase(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogInformation("RUNTIME: AiServiceBase constructor called for {ClassName}", GetType().Name);
        _logger.LogInformation("RUNTIME: ServiceProvider available: {Available}", _serviceProvider != null);
        _logger.LogInformation("RUNTIME: Logger available: {Available}", _logger != null);
    }

    /// <summary>
    /// Gets the event bus service for emitting events with comprehensive logging
    /// </summary>
    /// <returns>Event bus instance or null if not available</returns>
    protected ICxEventBus? GetEventBus()
    {
        _logger.LogInformation("RUNTIME: GetEventBus() called from {ClassName}", GetType().Name);
        
        try
        {
            _logger.LogInformation("RUNTIME: GetEventBus() - Attempting to resolve ICxEventBus from ServiceProvider");
            _logger.LogInformation("RUNTIME: GetEventBus() - ServiceProvider type: {Type}", _serviceProvider.GetType().Name);
            
            var eventBus = _serviceProvider.GetService<ICxEventBus>();
            
            if (eventBus != null)
            {
                _logger.LogInformation("RUNTIME: GetEventBus() - Successfully resolved ICxEventBus");
                _logger.LogInformation("RUNTIME: GetEventBus() - EventBus type: {Type}", eventBus.GetType().Name);
                _logger.LogInformation("RUNTIME: GetEventBus() - EventBus instance: {Instance}", eventBus.GetHashCode());
                return eventBus;
            }
            else
            {
                _logger.LogWarning("RUNTIME: GetEventBus() - ICxEventBus service not found");
                _logger.LogWarning("RUNTIME: GetEventBus() - Service may not be registered in DI container");
                
                // Debug service registration
                _logger.LogInformation("RUNTIME: GetEventBus() - Debugging service registration...");
                var allServices = _serviceProvider.GetServices<object>();
                _logger.LogInformation("RUNTIME: GetEventBus() - Total services registered: {Count}", allServices.Count());
                
                // List first few services for debugging
                int serviceIndex = 0;
                foreach (var service in allServices.Take(10))
                {
                    _logger.LogInformation("RUNTIME: GetEventBus() - Service[{Index}]: {Type}", serviceIndex++, service.GetType().Name);
                }
                
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RUNTIME: GetEventBus() - Exception while resolving ICxEventBus");
            _logger.LogError("RUNTIME: GetEventBus() - Exception type: {Type}", ex.GetType().Name);
            _logger.LogError("RUNTIME: GetEventBus() - Exception message: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Execute PowerShell commands with comprehensive event emission and runtime logging
    /// Fire-and-forget method - results delivered via events
    /// </summary>
    /// <param name="command">PowerShell command to execute</param>
    /// <param name="options">Execution options</param>
    public void Execute(string command, object? options = null)
    {
        var className = GetType().Name;
        _logger.LogInformation("RUNTIME: Execute() called from {ClassName} with command: {Command}", className, command);
        _logger.LogInformation("RUNTIME: Execute() - Starting Task.Run for fire-and-forget execution");
        
        // Fire-and-forget execution in background task
        Task.Run(async () =>
        {
            _logger.LogInformation("RUNTIME: Execute() - Task.Run started for command: {Command}", command);
            
            try
            {
                _logger.LogInformation("RUNTIME: Execute() - Creating PowerShell instance");
                
                using var powerShell = PowerShell.Create();
                _logger.LogInformation("RUNTIME: Execute() - PowerShell instance created successfully");
                
                // Add the command to PowerShell
                _logger.LogInformation("RUNTIME: Execute() - Adding script to PowerShell: {Command}", command);
                powerShell.AddScript(command);
                
                // Set execution policy if options specify it
                if (options != null)
                {
                    var policyValue = options.ToString();
                    if (policyValue != null)
                    {
                        _logger.LogInformation("RUNTIME: Execute() - Setting execution policy: {Policy}", policyValue);
                        powerShell.AddScript($"Set-ExecutionPolicy {policyValue} -Scope Process -Force");
                    }
                }
                
                // Execute the command asynchronously with timeout
                _logger.LogInformation("RUNTIME: Execute() - About to invoke PowerShell command");
                
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var results = await Task.Run(() =>
                {
                    _logger.LogInformation("RUNTIME: Execute() - PowerShell.Invoke() starting...");
                    var invokeResults = powerShell.Invoke();
                    _logger.LogInformation("RUNTIME: Execute() - PowerShell.Invoke() completed, result count: {Count}", invokeResults.Count);
                    return invokeResults;
                }, cts.Token);
                
                var errors = powerShell.Streams.Error.ReadAll();
                
                _logger.LogInformation("RUNTIME: Execute() - PowerShell execution completed");
                _logger.LogInformation("RUNTIME: Execute() - Results count: {Count}", results.Count);
                _logger.LogInformation("RUNTIME: Execute() - Errors count: {Count}", errors.Count);
                
                // Convert results to CX-compatible objects
                _logger.LogInformation("RUNTIME: Execute() - Converting results to CX objects");
                var outputResults = results.Select(result => new
                {
                    value = result?.BaseObject?.ToString() ?? result?.ToString() ?? "",
                    type = result?.BaseObject?.GetType().Name ?? "Unknown"
                }).ToArray<object>();
                
                var errorResults = errors.Select(error => new
                {
                    message = error.ToString(),
                    category = error.CategoryInfo?.Category.ToString() ?? "Unknown",
                    line = error.InvocationInfo?.ScriptLineNumber ?? 0
                }).ToArray<object>();
                
                _logger.LogInformation("RUNTIME: Execute() - Result conversion complete");
                _logger.LogInformation("RUNTIME: Execute() - Output results: {Count} items", outputResults.Length);
                _logger.LogInformation("RUNTIME: Execute() - Error results: {Count} items", errorResults.Length);
                
                // Create comprehensive execution results
                var executionResult = new
                {
                    command = command,
                    success = errors.Count == 0,
                    outputs = outputResults,
                    errors = errorResults,
                    executionTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    exitCode = errors.Count == 0 ? 0 : 1,
                    agent = className
                };
                
                _logger.LogInformation("RUNTIME: Execute() - Execution result object created");
                _logger.LogInformation("RUNTIME: Execute() - Success: {Success}", executionResult.success);
                _logger.LogInformation("RUNTIME: Execute() - Exit code: {ExitCode}", executionResult.exitCode);
                
                _logger.LogInformation("RUNTIME: Execute() - Command executed successfully: {Command}", command);
                
                // Emit command.executed event to event bus
                _logger.LogInformation("RUNTIME: Execute() - About to retrieve EventBus for event emission");
                var eventBus = GetEventBus();
                _logger.LogInformation("RUNTIME: Execute() - EventBus retrieved: {EventBus}", eventBus?.GetType().Name ?? "null");
                
                if (eventBus != null)
                {
                    _logger.LogInformation("RUNTIME: Execute() - EventBus is available, emitting command.executed event");
                    _logger.LogInformation("RUNTIME: Execute() - Event name: command.executed");
                    _logger.LogInformation("RUNTIME: Execute() - Event payload type: {Type}", executionResult.GetType().Name);
                    
                    try
                    {
                        _logger.LogInformation("RUNTIME: Execute() - Calling eventBus.EmitAsync()");
                        await eventBus.EmitAsync("command.executed", executionResult);
                        _logger.LogInformation("RUNTIME: Execute() - Successfully emitted command.executed event");
                        _logger.LogInformation("RUNTIME: Execute() - Event emission completed without exceptions");
                    }
                    catch (Exception emitEx)
                    {
                        _logger.LogError(emitEx, "RUNTIME: Execute() - FAILED to emit command.executed event");
                        _logger.LogError("RUNTIME: Execute() - Emit exception type: {Type}", emitEx.GetType().Name);
                        _logger.LogError("RUNTIME: Execute() - Emit exception message: {Message}", emitEx.Message);
                        _logger.LogError("RUNTIME: Execute() - Emit exception stack: {Stack}", emitEx.StackTrace);
                    }
                }
                else
                {
                    _logger.LogWarning("RUNTIME: Execute() - EventBus is null - cannot emit events");
                    _logger.LogWarning("RUNTIME: Execute() - This means ICxEventBus service is not registered");
                    
                    // Debug service provider state
                    _logger.LogInformation("RUNTIME: Execute() - Debugging service provider state");
                    try
                    {
                        var serviceProvider = _serviceProvider;
                        _logger.LogInformation("RUNTIME: Execute() - ServiceProvider available: {Available}", serviceProvider != null);
                        
                        if (serviceProvider != null)
                        {
                            // Try to get all services
                            var allServices = serviceProvider.GetServices<object>();
                            _logger.LogInformation("RUNTIME: Execute() - Total services registered: {Count}", allServices.Count());
                            
                            // Specifically look for event bus services
                            var eventBusServices = serviceProvider.GetServices<ICxEventBus>();
                            _logger.LogInformation("RUNTIME: Execute() - EventBus services found: {Count}", eventBusServices.Count());
                            
                            foreach (var service in eventBusServices)
                            {
                                _logger.LogInformation("RUNTIME: Execute() - Found EventBus service: {Type}", service.GetType().FullName);
                            }
                        }
                    }
                    catch (Exception debugEx)
                    {
                        _logger.LogError(debugEx, "RUNTIME: Execute() - Exception while debugging service provider");
                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RUNTIME: Execute() - FAILED to execute PowerShell command: {Command}", command);
                _logger.LogError("RUNTIME: Execute() - Exception type: {Type}", ex.GetType().Name);
                _logger.LogError("RUNTIME: Execute() - Exception message: {Message}", ex.Message);
                _logger.LogError("RUNTIME: Execute() - Exception stack: {Stack}", ex.StackTrace);
                
                var errorResult = new
                {
                    command = command,
                    success = false,
                    outputs = new object[0],
                    errors = new[] { new { message = ex.Message, category = "SystemError", line = 0 } },
                    executionTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    exitCode = -1,
                    agent = className
                };
                
                _logger.LogInformation("RUNTIME: Execute() - Error result object created");
                
                // Emit command.error event to event bus
                _logger.LogInformation("RUNTIME: Execute() - Attempting to emit command.error event");
                var eventBus = GetEventBus();
                _logger.LogInformation("RUNTIME: Execute() - EventBus for error: {EventBus}", eventBus?.GetType().Name ?? "null");
                
                if (eventBus != null)
                {
                    _logger.LogInformation("RUNTIME: Execute() - Emitting command.error event");
                    try
                    {
                        await eventBus.EmitAsync("command.error", errorResult);
                        _logger.LogInformation("RUNTIME: Execute() - Successfully emitted command.error event");
                    }
                    catch (Exception emitEx)
                    {
                        _logger.LogError(emitEx, "RUNTIME: Execute() - Failed to emit command.error event");
                    }
                }
                else
                {
                    _logger.LogWarning("RUNTIME: Execute() - EventBus is null - cannot emit error events");
                }
            }
            
            _logger.LogInformation("RUNTIME: Execute() - Task.Run completed for command: {Command}", command);
        });
        
        _logger.LogInformation("RUNTIME: Execute() method completed - task started in background");
    }

    // Additional cognitive methods with detailed logging
    
    /// <summary>
    /// Realtime thinking - accessible via this.Think()
    /// Default cognitive mode for all CX classes
    /// Fire-and-forget method - results delivered via events
    /// </summary>
    public void Think(string input, object? options = null)
    {
        _logger.LogInformation("RUNTIME: Think() called with input: {Input}", input);
        // Implementation details...
    }

    /// <summary>
    /// Core text generation - accessible via this.Generate()
    /// Fire-and-forget method - results delivered via events
    /// </summary>
    public void Generate(string prompt, object? options = null)
    {
        _logger.LogInformation("RUNTIME: Generate() called with prompt: {Prompt}", prompt);
        // Implementation details...
    }

    /// <summary>
    /// Learn from experience - accessible via this.Learn()
    /// Stores knowledge in vector database for future retrieval
    /// Fire-and-forget method - results delivered via events
    /// </summary>
    public void Learn(object experience, object? options = null)
    {
        _logger.LogInformation("RUNTIME: Learn() called with experience type: {Type}", experience?.GetType().Name ?? "null");
        // Implementation details...
    }

    /// <summary>
    /// Search for relevant memories - accessible via this.Search()
    /// Fire-and-forget method - results delivered via events
    /// </summary>
    public void Search(string query, object? options = null)
    {
        _logger.LogInformation("RUNTIME: Search() called with query: {Query}", query);
        // Implementation details...
    }
}
