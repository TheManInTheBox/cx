using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CxLanguage.StandardLibrary.Core;

/// <summary>
/// Base interface for all CX standard library AI services
/// </summary>
public interface ICxAiService : IDisposable
{
    /// <summary>
    /// Service name for registration and discovery
    /// </summary>
    string ServiceName { get; }
    
    /// <summary>
    /// Service version for compatibility checking
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Whether the service is currently available
    /// </summary>
    bool IsAvailable { get; }
    
    /// <summary>
    /// Initialize the service with configuration
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Health check for the service
    /// </summary>
    Task<ServiceHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Health status for AI services
/// </summary>
public enum ServiceHealthStatus
{
    /// <summary>
    /// Service is operating normally
    /// </summary>
    Healthy,
    
    /// <summary>
    /// Service is experiencing performance issues but still functional
    /// </summary>
    Degraded,
    
    /// <summary>
    /// Service is not functioning properly
    /// </summary>
    Unhealthy,
    
    /// <summary>
    /// Service status cannot be determined
    /// </summary>
    Unknown
}

/// <summary>
/// Base options class for all AI service operations
/// </summary>
public abstract class CxAiOptions
{
    /// <summary>
    /// Timeout for the operation
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// Additional metadata for the operation
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Whether to enable telemetry for this operation
    /// </summary>
    public bool EnableTelemetry { get; set; } = true;
}

/// <summary>
/// Base result class for all AI service operations
/// </summary>
public abstract class CxAiResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Execution time for the operation
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }
    
    /// <summary>
    /// Additional metadata from the operation
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Base implementation for CX AI services
/// </summary>
public abstract class CxAiServiceBase : ICxAiService
{
    /// <summary>
    /// The Semantic Kernel instance used by this service
    /// </summary>
    protected readonly Kernel _kernel;
    
    /// <summary>
    /// Logger instance for this service
    /// </summary>
    protected readonly ILogger _logger;
    
    /// <summary>
    /// Flag indicating whether this service has been disposed
    /// </summary>
    protected bool _disposed;

    /// <summary>
    /// Initializes a new instance of the CxAiServiceBase class
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <param name="logger">Logger instance</param>
    protected CxAiServiceBase(Kernel kernel, ILogger logger)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Service name for registration and discovery
    /// </summary>
    public abstract string ServiceName { get; }
    
    /// <summary>
    /// Service version for compatibility checking
    /// </summary>
    public abstract string Version { get; }
    
    /// <summary>
    /// Whether the service is currently available
    /// </summary>
    public virtual bool IsAvailable => !_disposed && _kernel != null;

    /// <summary>
    /// Initialize the service with configuration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing service: {ServiceName} v{Version}", ServiceName, Version);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Health check for the service
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The health status of the service</returns>
    public virtual async Task<ServiceHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_disposed) return ServiceHealthStatus.Unhealthy;
            if (_kernel == null) return ServiceHealthStatus.Unhealthy;
            
            // Basic health check - can be overridden by specific services
            await Task.CompletedTask;
            return ServiceHealthStatus.Healthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for service: {ServiceName}", ServiceName);
            return ServiceHealthStatus.Unhealthy;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">true if the method is being called from Dispose(); false if being called from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _logger.LogInformation("Disposing service: {ServiceName}", ServiceName);
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
