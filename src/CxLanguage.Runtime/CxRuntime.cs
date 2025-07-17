using CxLanguage.Azure.Services;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Runtime;

/// <summary>
/// Runtime environment for executing Cx language scripts
/// </summary>
public class CxRuntime
{
    private readonly IAiService _aiService;
    private readonly ILogger<CxRuntime> _logger;
    private readonly Dictionary<string, object> _globalVariables;
    private readonly CxRuntimeOptions _options;

    public CxRuntime(IAiService aiService, ILogger<CxRuntime> logger, CxRuntimeOptions? options = null)
    {
        _aiService = aiService;
        _logger = logger;
        _globalVariables = new Dictionary<string, object>();
        _options = options ?? new CxRuntimeOptions();
    }

    /// <summary>
    /// Built-in functions available to Cx scripts
    /// </summary>
    public class BuiltinFunctions
    {
        private readonly CxRuntime _runtime;

        public BuiltinFunctions(CxRuntime runtime)
        {
            _runtime = runtime;
        }

        public void Print(object? value)
        {
            Console.WriteLine(value?.ToString() ?? "null");
        }

        public async Task<string> AiGenerate(string prompt, object? options = null)
        {
            var requestOptions = ParseAiOptions(options);
            var response = await _runtime._aiService.GenerateTextAsync(prompt, requestOptions);
            
            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"AI generation failed: {response.ErrorMessage}");
            }

            return response.Content;
        }

        public async Task<string> AiAnalyze(string content, object options)
        {
            var analysisOptions = ParseAnalysisOptions(options);
            var response = await _runtime._aiService.AnalyzeAsync(content, analysisOptions);

            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"AI analysis failed: {response.ErrorMessage}");
            }

            return response.Content;
        }

        public async Task<string[]> AiParallel(string[] prompts, object? options = null)
        {
            var requestOptions = ParseAiOptions(options);
            var responses = await _runtime._aiService.ProcessBatchAsync(prompts, requestOptions);
            
            var results = new List<string>();
            foreach (var response in responses)
            {
                if (response.IsSuccess)
                {
                    results.Add(response.Content);
                }
                else
                {
                    throw new InvalidOperationException($"AI parallel processing failed: {response.ErrorMessage}");
                }
            }

            return results.ToArray();
        }

        public async IAsyncEnumerable<string> AiStream(string prompt, object? options = null)
        {
            var requestOptions = ParseAiOptions(options);
            
            using var streamResponse = await _runtime._aiService.StreamGenerateTextAsync(prompt, requestOptions);
            
            await foreach (var token in streamResponse.GetTokensAsync())
            {
                yield return token;
            }
        }

        private AiRequestOptions ParseAiOptions(object? options)
        {
            if (options == null)
                return new AiRequestOptions();

            // In a real implementation, you'd parse from a dictionary/object
            // For now, return default options
            return new AiRequestOptions();
        }

        private AiAnalysisOptions ParseAnalysisOptions(object options)
        {
            // In a real implementation, you'd parse the options object
            // For now, return a default analysis task
            return new AiAnalysisOptions
            {
                Task = "general_analysis",
                ResponseFormat = "text"
            };
        }
    }
}

/// <summary>
/// Runtime options for Cx execution
/// </summary>
public class CxRuntimeOptions
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxConcurrentAiCalls { get; set; } = 10;
    public bool EnableDebugMode { get; set; } = false;
    public Dictionary<string, object> EnvironmentVariables { get; set; } = new();
}

/// <summary>
/// Exception thrown during Cx runtime execution
/// </summary>
public class CxRuntimeException : Exception
{
    public int? Line { get; }
    public int? Column { get; }
    public string? SourceFile { get; }

    public CxRuntimeException(string message) : base(message) { }

    public CxRuntimeException(string message, Exception innerException) : base(message, innerException) { }

    public CxRuntimeException(string message, int? line, int? column, string? sourceFile) : base(message)
    {
        Line = line;
        Column = column;
        SourceFile = sourceFile;
    }
}

/// <summary>
/// Async context for managing concurrent AI operations
/// </summary>
public class AsyncContext
{
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public AsyncContext(int maxConcurrency)
    {
        _concurrencySemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation)
    {
        await _concurrencySemaphore.WaitAsync(_cancellationTokenSource.Token);
        try
        {
            return await operation(_cancellationTokenSource.Token);
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        _concurrencySemaphore?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
