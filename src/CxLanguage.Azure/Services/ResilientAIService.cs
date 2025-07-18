using CxLanguage.Core.AI;
using CxLanguage.Core.Telemetry;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Linq;

namespace CxLanguage.Azure.Services;

/// <summary>
/// Enhanced AI service wrapper with robust error handling, retry logic, and circuit breaker patterns
/// </summary>
public class ResilientAIService : IAiService
{
    private readonly IAiService _innerService;
    private readonly ILogger<ResilientAIService> _logger;
    private readonly CxTelemetryService? _telemetryService;
    private readonly ResilienceOptions _options;
    private readonly Dictionary<string, CircuitBreaker> _circuitBreakers;

    public ResilientAIService(
        IAiService innerService, 
        ILogger<ResilientAIService> logger,
        CxTelemetryService? telemetryService = null,
        ResilienceOptions? options = null)
    {
        _innerService = innerService;
        _logger = logger;
        _telemetryService = telemetryService;
        _options = options ?? new ResilienceOptions();
        _circuitBreakers = new Dictionary<string, CircuitBreaker>();
    }

    public async Task<AiResponse> GenerateTextAsync(string prompt, AiRequestOptions? options = null)
    {
        return await ExecuteWithResilienceAsync(
            "GenerateText",
            () => _innerService.GenerateTextAsync(prompt, options),
            prompt);
    }

    public async Task<AiResponse> AnalyzeAsync(string content, AiAnalysisOptions options)
    {
        return await ExecuteWithResilienceAsync(
            "Analyze",
            () => _innerService.AnalyzeAsync(content, options),
            content);
    }

    public async Task<AiStreamResponse> StreamGenerateTextAsync(string prompt, AiRequestOptions? options = null)
    {
        var circuitBreaker = GetCircuitBreaker("StreamGenerateText");
        
        if (circuitBreaker.IsOpen)
        {
            _logger.LogWarning("Circuit breaker is open for StreamGenerateText operation");
            throw new ServiceUnavailableException("AI streaming service is currently unavailable");
        }

        try
        {
            var result = await _innerService.StreamGenerateTextAsync(prompt, options);
            circuitBreaker.RecordSuccess();
            return result;
        }
        catch (Exception ex)
        {
            circuitBreaker.RecordFailure();
            _logger.LogError(ex, "Error in StreamGenerateTextAsync");
            _telemetryService?.TrackAiFunctionExecution("StreamGenerateText", prompt, TimeSpan.Zero, false, ex.Message);
            throw;
        }
    }

    public async Task<AiResponse[]> ProcessBatchAsync(string[] prompts, AiRequestOptions? options = null)
    {
        return await ExecuteWithBatchResilienceAsync(
            "ProcessBatch",
            () => _innerService.ProcessBatchAsync(prompts, options),
            string.Join(", ", prompts.Take(3)) + (prompts.Length > 3 ? "..." : ""));
    }

    public async Task<AiEmbeddingResponse> GenerateEmbeddingAsync(string text, AiRequestOptions? options = null)
    {
        return await ExecuteWithEmbeddingResilienceAsync(
            "GenerateEmbedding",
            () => _innerService.GenerateEmbeddingAsync(text, options),
            text);
    }

    public async Task<AiImageResponse> GenerateImageAsync(string prompt, AiImageOptions? options = null)
    {
        return await ExecuteWithImageResilienceAsync(
            "GenerateImage",
            () => _innerService.GenerateImageAsync(prompt, options),
            prompt);
    }

    public async Task<AiImageAnalysisResponse> AnalyzeImageAsync(string imageUrl, AiImageAnalysisOptions? options = null)
    {
        return await ExecuteWithImageAnalysisResilienceAsync(
            "AnalyzeImage",
            () => _innerService.AnalyzeImageAsync(imageUrl, options),
            imageUrl);
    }

    private async Task<AiResponse> ExecuteWithResilienceAsync(
        string operationName,
        Func<Task<AiResponse>> operation,
        string context)
    {
        var circuitBreaker = GetCircuitBreaker(operationName);
        var stopwatch = Stopwatch.StartNew();

        if (circuitBreaker.IsOpen)
        {
            _logger.LogWarning("Circuit breaker is open for {Operation}", operationName);
            return new AiResponse { IsSuccess = false, Error = "Service temporarily unavailable" };
        }

        circuitBreaker.RecordAttempt();

        Exception? lastException = null;

        for (int attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                _logger.LogDebug("Executing {Operation}, attempt {Attempt}/{MaxAttempts}", 
                    operationName, attempt, _options.MaxRetryAttempts);

                var result = await operation();
                
                if (result.IsSuccess)
                {
                    circuitBreaker.RecordSuccess();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, true);
                    return result;
                }
                else
                {
                    _logger.LogWarning("Operation {Operation} returned failure: {Error}", operationName, result.Error);
                    
                    // Don't retry for client errors (4xx equivalent)
                    if (IsClientError(result.Error))
                    {
                        circuitBreaker.RecordFailure();
                        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, result.Error);
                        return result;
                    }
                    
                    lastException = new InvalidOperationException(result.Error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for {Operation}: {Error}", 
                    attempt, operationName, ex.Message);

                // Don't retry for certain types of exceptions
                if (!IsRetryableException(ex))
                {
                    circuitBreaker.RecordFailure();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, ex.Message);
                    return new AiResponse { IsSuccess = false, Error = $"Non-retryable error: {ex.Message}" };
                }
            }

            if (attempt < _options.MaxRetryAttempts)
            {
                var delay = CalculateRetryDelay(attempt);
                _logger.LogInformation("Retrying {Operation} in {Delay}ms", operationName, delay.TotalMilliseconds);
                await Task.Delay(delay);
            }
        }

        // All retry attempts failed
        circuitBreaker.RecordFailure();
        var errorMessage = $"Operation failed after {_options.MaxRetryAttempts} attempts: {lastException?.Message}";
        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, errorMessage);
        return new AiResponse { IsSuccess = false, Error = errorMessage };
    }

    private async Task<AiEmbeddingResponse> ExecuteWithEmbeddingResilienceAsync(
        string operationName,
        Func<Task<AiEmbeddingResponse>> operation,
        string context)
    {
        var circuitBreaker = GetCircuitBreaker(operationName);
        var stopwatch = Stopwatch.StartNew();

        if (circuitBreaker.IsOpen)
        {
            _logger.LogWarning("Circuit breaker is open for {Operation}", operationName);
            return new AiEmbeddingResponse { IsSuccess = false, Error = "Service temporarily unavailable" };
        }

        Exception? lastException = null;

        for (int attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                var result = await operation();
                
                if (result.IsSuccess)
                {
                    circuitBreaker.RecordSuccess();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, true);
                    return result;
                }
                else
                {
                    if (IsClientError(result.Error))
                    {
                        circuitBreaker.RecordFailure();
                        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, result.Error);
                        return result;
                    }
                    
                    lastException = new InvalidOperationException(result.Error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for {Operation}", attempt, operationName);

                if (!IsRetryableException(ex))
                {
                    circuitBreaker.RecordFailure();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, ex.Message);
                    return new AiEmbeddingResponse { IsSuccess = false, Error = $"Non-retryable error: {ex.Message}" };
                }
            }

            if (attempt < _options.MaxRetryAttempts)
            {
                var delay = CalculateRetryDelay(attempt);
                await Task.Delay(delay);
            }
        }

        circuitBreaker.RecordFailure();
        var errorMessage = $"Operation failed after {_options.MaxRetryAttempts} attempts: {lastException?.Message}";
        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, errorMessage);
        return new AiEmbeddingResponse { IsSuccess = false, Error = errorMessage };
    }

    private async Task<AiImageResponse> ExecuteWithImageResilienceAsync(
        string operationName,
        Func<Task<AiImageResponse>> operation,
        string context)
    {
        var circuitBreaker = GetCircuitBreaker(operationName);
        var stopwatch = Stopwatch.StartNew();

        if (circuitBreaker.IsOpen)
        {
            _logger.LogWarning("Circuit breaker is open for {Operation}", operationName);
            return new AiImageResponse { IsSuccess = false, Error = "Service temporarily unavailable" };
        }

        Exception? lastException = null;

        for (int attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                var result = await operation();
                
                if (result.IsSuccess)
                {
                    circuitBreaker.RecordSuccess();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, true);
                    return result;
                }
                else
                {
                    if (IsClientError(result.Error))
                    {
                        circuitBreaker.RecordFailure();
                        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, result.Error);
                        return result;
                    }
                    
                    lastException = new InvalidOperationException(result.Error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for {Operation}", attempt, operationName);

                if (!IsRetryableException(ex))
                {
                    circuitBreaker.RecordFailure();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, ex.Message);
                    return new AiImageResponse { IsSuccess = false, Error = $"Non-retryable error: {ex.Message}" };
                }
            }

            if (attempt < _options.MaxRetryAttempts)
            {
                var delay = CalculateRetryDelay(attempt);
                await Task.Delay(delay);
            }
        }

        circuitBreaker.RecordFailure();
        var errorMessage = $"Operation failed after {_options.MaxRetryAttempts} attempts: {lastException?.Message}";
        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, errorMessage);
        return new AiImageResponse { IsSuccess = false, Error = errorMessage };
    }

    private async Task<AiImageAnalysisResponse> ExecuteWithImageAnalysisResilienceAsync(
        string operationName,
        Func<Task<AiImageAnalysisResponse>> operation,
        string context)
    {
        var circuitBreaker = GetCircuitBreaker(operationName);
        var stopwatch = Stopwatch.StartNew();

        if (circuitBreaker.IsOpen)
        {
            _logger.LogWarning("Circuit breaker is open for {Operation}", operationName);
            return new AiImageAnalysisResponse { IsSuccess = false, Error = "Service temporarily unavailable" };
        }

        Exception? lastException = null;

        for (int attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                var result = await operation();
                
                if (result.IsSuccess)
                {
                    circuitBreaker.RecordSuccess();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, true);
                    return result;
                }
                else
                {
                    if (IsClientError(result.Error))
                    {
                        circuitBreaker.RecordFailure();
                        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, result.Error);
                        return result;
                    }
                    
                    lastException = new InvalidOperationException(result.Error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for {Operation}", attempt, operationName);

                if (!IsRetryableException(ex))
                {
                    circuitBreaker.RecordFailure();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, ex.Message);
                    return new AiImageAnalysisResponse { IsSuccess = false, Error = $"Non-retryable error: {ex.Message}" };
                }
            }

            if (attempt < _options.MaxRetryAttempts)
            {
                var delay = CalculateRetryDelay(attempt);
                await Task.Delay(delay);
            }
        }

        circuitBreaker.RecordFailure();
        var errorMessage = $"Operation failed after {_options.MaxRetryAttempts} attempts: {lastException?.Message}";
        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, errorMessage);
        return new AiImageAnalysisResponse { IsSuccess = false, Error = errorMessage };
    }

    private async Task<AiResponse[]> ExecuteWithBatchResilienceAsync(
        string operationName,
        Func<Task<AiResponse[]>> operation,
        string context)
    {
        var circuitBreaker = GetCircuitBreaker(operationName);
        var stopwatch = Stopwatch.StartNew();

        if (circuitBreaker.IsOpen)
        {
            _logger.LogWarning("Circuit breaker is open for {Operation}", operationName);
            return new[] { new AiResponse { IsSuccess = false, Error = "Service temporarily unavailable" } };
        }

        Exception? lastException = null;

        for (int attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                var results = await operation();
                
                if (results.All(r => r.IsSuccess))
                {
                    circuitBreaker.RecordSuccess();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, true);
                    return results;
                }
                else
                {
                    var firstError = results.FirstOrDefault(r => !r.IsSuccess);
                    if (firstError != null && IsClientError(firstError.Error))
                    {
                        circuitBreaker.RecordFailure();
                        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, firstError.Error);
                        return results;
                    }
                    
                    lastException = new InvalidOperationException(firstError?.Error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for {Operation}", attempt, operationName);

                if (!IsRetryableException(ex))
                {
                    circuitBreaker.RecordFailure();
                    _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, ex.Message);
                    return new[] { new AiResponse { IsSuccess = false, Error = $"Non-retryable error: {ex.Message}" } };
                }
            }

            if (attempt < _options.MaxRetryAttempts)
            {
                var delay = CalculateRetryDelay(attempt);
                _logger.LogInformation("Retrying {Operation} in {Delay}ms", operationName, delay.TotalMilliseconds);
                await Task.Delay(delay);
            }
        }

        // All retry attempts failed
        circuitBreaker.RecordFailure();
        var errorMessage = $"Operation failed after {_options.MaxRetryAttempts} attempts: {lastException?.Message}";
        _telemetryService?.TrackAiFunctionExecution(operationName, context, stopwatch.Elapsed, false, errorMessage);
        return new[] { new AiResponse { IsSuccess = false, Error = errorMessage } };
    }

    private CircuitBreaker GetCircuitBreaker(string operationName)
    {
        if (!_circuitBreakers.TryGetValue(operationName, out var circuitBreaker))
        {
            circuitBreaker = new CircuitBreaker(_options.CircuitBreakerOptions);
            _circuitBreakers[operationName] = circuitBreaker;
        }
        return circuitBreaker;
    }

    private TimeSpan CalculateRetryDelay(int attempt)
    {
        return _options.RetryStrategy switch
        {
            RetryStrategy.Linear => TimeSpan.FromMilliseconds(_options.BaseDelayMs * attempt),
            RetryStrategy.Exponential => TimeSpan.FromMilliseconds(_options.BaseDelayMs * Math.Pow(2, attempt - 1)),
            RetryStrategy.Fixed => TimeSpan.FromMilliseconds(_options.BaseDelayMs),
            _ => TimeSpan.FromMilliseconds(_options.BaseDelayMs)
        };
    }

    private static bool IsRetryableException(Exception ex)
    {
        return ex switch
        {
            HttpRequestException => true,
            TaskCanceledException => true,
            TimeoutException => true,
            ServiceUnavailableException => true,
            ArgumentNullException => false,
            ArgumentException => false,
            InvalidOperationException when ex.Message.Contains("API key") => false,
            InvalidOperationException when ex.Message.Contains("authentication") => false,
            _ => true
        };
    }

    private static bool IsClientError(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return false;

        var clientErrorIndicators = new[]
        {
            "400", "401", "403", "404", "422", "429",
            "invalid", "unauthorized", "forbidden", "not found",
            "bad request", "authentication", "api key"
        };

        return clientErrorIndicators.Any(indicator => 
            errorMessage.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }
}

#region Supporting Classes

public class ResilienceOptions
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int BaseDelayMs { get; set; } = 1000;
    public RetryStrategy RetryStrategy { get; set; } = RetryStrategy.Exponential;
    public CircuitBreakerOptions CircuitBreakerOptions { get; set; } = new();
}

public enum RetryStrategy
{
    Fixed,
    Linear,
    Exponential
}

public class CircuitBreakerOptions
{
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan OpenDuration { get; set; } = TimeSpan.FromMinutes(1);
    public int HalfOpenMaxCalls { get; set; } = 3;
}

public class CircuitBreaker
{
    private readonly CircuitBreakerOptions _options;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _halfOpenCalls = 0;
    private readonly object _lock = new object();

    public CircuitBreaker(CircuitBreakerOptions options)
    {
        _options = options;
    }

    public bool IsOpen
    {
        get
        {
            lock (_lock)
            {
                if (_state == CircuitBreakerState.Open && 
                    DateTime.UtcNow - _lastFailureTime >= _options.OpenDuration)
                {
                    _state = CircuitBreakerState.HalfOpen;
                    _halfOpenCalls = 0;
                }

                // In half-open state, limit the number of test calls
                if (_state == CircuitBreakerState.HalfOpen && _halfOpenCalls >= 1)
                {
                    return true; // Treat as open until the test call completes
                }

                return _state == CircuitBreakerState.Open;
            }
        }
    }

    public void RecordSuccess()
    {
        lock (_lock)
        {
            _failureCount = 0;
            if (_state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Closed;
                _halfOpenCalls = 0;
            }
        }
    }

    public void RecordFailure()
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Open;
                _halfOpenCalls = 0;
            }
            else if (_failureCount >= _options.FailureThreshold)
            {
                _state = CircuitBreakerState.Open;
            }
        }
    }

    public void RecordAttempt()
    {
        lock (_lock)
        {
            if (_state == CircuitBreakerState.HalfOpen)
            {
                _halfOpenCalls++;
            }
        }
    }
}

public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}

public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message) : base(message) { }
    public ServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}

#endregion
