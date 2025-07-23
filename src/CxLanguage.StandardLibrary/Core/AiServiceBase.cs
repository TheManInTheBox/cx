using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.KernelMemory;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.AI.VectorDatabase;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;

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
    /// Unique Kernel Memory collection for this class instance
    /// </summary>
    protected readonly IKernelMemory? _instanceMemory = null!;

    /// <summary>
    /// Unique collection name for this instance's vector store
    /// </summary>
    protected readonly string _instanceCollectionName;

    /// <summary>
    /// Initializes a new instance of AiServiceBase with service provider injection
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    public AiServiceBase(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Create unique collection name for this instance
        var instanceId = Guid.NewGuid().ToString("N")[..8];
        _instanceCollectionName = $"{GetType().Name}_{instanceId}";
        
        // Get Kernel Memory service and set up instance-specific collection
        _instanceMemory = _serviceProvider.GetService<IKernelMemory>();
        
        _logger.LogInformation("RUNTIME: AiServiceBase constructor called for {ClassName}", GetType().Name);
        _logger.LogInformation("RUNTIME: ServiceProvider available: {Available}", _serviceProvider != null);
        _logger.LogInformation("RUNTIME: Logger available: {Available}", logger != null);
        _logger.LogInformation("RUNTIME: Instance memory collection: {CollectionName}", _instanceCollectionName);
        _logger.LogInformation("RUNTIME: Kernel Memory available: {Available}", _instanceMemory != null);
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
                var outputList = new List<object>();
                foreach (var result in results)
                {
                    outputList.Add(new
                    {
                        value = result?.BaseObject?.ToString() ?? result?.ToString() ?? "",
                        type = result?.BaseObject?.GetType().Name ?? "Unknown"
                    });
                }
                var outputResults = outputList.ToArray();
                
                var errorList = new List<object>();
                foreach (var error in errors)
                {
                    errorList.Add(new
                    {
                        message = error.ToString(),
                        category = error.CategoryInfo?.Category.ToString() ?? "Unknown",
                        line = error.InvocationInfo?.ScriptLineNumber ?? 0
                    });
                }
                var errorResults = errorList.ToArray();
                
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
        var className = GetType().Name;
        _logger.LogInformation("🧠 RUNTIME: Learn() called from {ClassName}", className);
        _logger.LogInformation("🧠 RUNTIME: Learn() - Experience type: {Type}", experience?.GetType().Name ?? "null");
        _logger.LogInformation("🧠 RUNTIME: Learn() - Experience content: {Content}", experience?.ToString() ?? "null");
        _logger.LogInformation("🧠 RUNTIME: Learn() - Options: {Options}", options?.ToString() ?? "null");
        
        // Fire-and-forget learning in background task with comprehensive instrumentation
        Task.Run(async () =>
        {
            _logger.LogInformation("🧠 RUNTIME: Learn() - Background task started");
            
            try
            {
                // Get the VectorDatabaseService
                _logger.LogInformation("🧠 RUNTIME: Learn() - Attempting to get VectorDatabaseService from DI container");
                var vectorService = _serviceProvider.GetService<VectorDatabaseService>();
                
                if (vectorService == null)
                {
                    _logger.LogWarning("🧠 RUNTIME: Learn() - VectorDatabaseService not found in DI container");
                    _logger.LogWarning("🧠 RUNTIME: Learn() - Available services:");
                    
                    var allServices = _serviceProvider.GetServices<object>();
                    foreach (var service in allServices.Take(10))
                    {
                        _logger.LogWarning("🧠 RUNTIME: Learn() - Service: {Type}", service.GetType().Name);
                    }
                    
                    // Still emit learning event even if no service
                    var learningEventBus = GetEventBus();
                    if (learningEventBus != null)
                    {
                        await learningEventBus.EmitAsync("ai.learning.failed", new
                        {
                            agent = className,
                            reason = "VectorDatabaseService not available",
                            experience = experience?.ToString() ?? "null",
                            timestamp = DateTime.UtcNow
                        });
                    }
                    return;
                }
                
                _logger.LogInformation("🧠 RUNTIME: Learn() - VectorDatabaseService found: {Type}", vectorService.GetType().Name);
                
                // Create document ID for the learning experience
                var documentId = $"{className}_learning_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}"[..32];
                _logger.LogInformation("🧠 RUNTIME: Learn() - Generated document ID: {DocumentId}", documentId);
                
                // Convert experience to text for vector database
                string textContent;
                if (experience is string strExp)
                {
                    textContent = strExp;
                }
                else
                {
                    // Serialize object to JSON-like string
                    textContent = System.Text.Json.JsonSerializer.Serialize(experience, new System.Text.Json.JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    });
                }
                
                _logger.LogInformation("🧠 RUNTIME: Learn() - Text content length: {Length} characters", textContent.Length);
                _logger.LogInformation("🧠 RUNTIME: Learn() - Text content preview: {Preview}...", 
                    textContent.Length > 100 ? textContent[..100] : textContent);
                
                // Create vector database options with metadata
                var vectorOptions = new VectorDatabaseOptions
                {
                    Tags = new Dictionary<string, string>
                    {
                        { "agent", className },
                        { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "learning_type", "cx_agent_learning" },
                        { "content_type", experience?.GetType().Name ?? "unknown" }
                    }
                };
                
                _logger.LogInformation("🧠 RUNTIME: Learn() - Calling vectorService.IngestTextAsync()");
                _logger.LogInformation("🧠 RUNTIME: Learn() - Vector options: {Options}", vectorOptions);
                
                var result = await vectorService.IngestTextAsync(textContent, documentId, vectorOptions);
                
                _logger.LogInformation("🧠 RUNTIME: Learn() - IngestTextAsync completed");
                _logger.LogInformation("🧠 RUNTIME: Learn() - Ingest result status: {Status}", result.Status);
                _logger.LogInformation("🧠 RUNTIME: Learn() - Ingest result document ID: {DocumentId}", result.DocumentId);
                
                // Emit learning complete event
                var learningCompleteEventBus = GetEventBus();
                if (learningCompleteEventBus != null)
                {
                    _logger.LogInformation("🧠 RUNTIME: Learn() - Emitting ai.learning.complete event");
                    await learningCompleteEventBus.EmitAsync("ai.learning.complete", new
                    {
                        agent = className,
                        topic = "Learning Operation",
                        knowledge = "Successfully stored experience in vector database",
                        documentId = result.DocumentId,
                        status = result.Status,
                        contentLength = textContent.Length,
                        timestamp = DateTime.UtcNow,
                        vectorOptions
                    });
                    _logger.LogInformation("🧠 RUNTIME: Learn() - Successfully emitted ai.learning.complete event");
                }
                else
                {
                    _logger.LogWarning("🧠 RUNTIME: Learn() - EventBus not available for learning complete event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🧠 RUNTIME: Learn() - EXCEPTION during learning process");
                _logger.LogError("🧠 RUNTIME: Learn() - Exception type: {Type}", ex.GetType().Name);
                _logger.LogError("🧠 RUNTIME: Learn() - Exception message: {Message}", ex.Message);
                _logger.LogError("🧠 RUNTIME: Learn() - Exception stack: {Stack}", ex.StackTrace);
                
                // Emit learning error event
                var learningErrorEventBus = GetEventBus();
                if (learningErrorEventBus != null)
                {
                    await learningErrorEventBus.EmitAsync("ai.learning.error", new
                    {
                        agent = className,
                        error = ex.Message,
                        experience = experience?.ToString() ?? "null",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            
            _logger.LogInformation("🧠 RUNTIME: Learn() - Background task completed");
        });
        
        _logger.LogInformation("🧠 RUNTIME: Learn() method completed - learning task started in background");
    }

    /// <summary>
    /// Search for relevant memories - accessible via this.Search()
    /// Fire-and-forget method - results delivered via events
    /// </summary>
    public void Search(string query, object? options = null)
    {
        var className = GetType().Name;
        _logger.LogInformation("🔍 RUNTIME: Search() called from {ClassName}", className);
        _logger.LogInformation("🔍 RUNTIME: Search() - Query: {Query}", query);
        _logger.LogInformation("🔍 RUNTIME: Search() - Options: {Options}", options?.ToString() ?? "null");
        
        // Fire-and-forget search in background task with comprehensive instrumentation
        Task.Run(async () =>
        {
            _logger.LogInformation("🔍 RUNTIME: Search() - Background task started");
            
            try
            {
                // Get the VectorDatabaseService
                _logger.LogInformation("🔍 RUNTIME: Search() - Attempting to get VectorDatabaseService from DI container");
                var vectorService = _serviceProvider.GetService<VectorDatabaseService>();
                
                if (vectorService == null)
                {
                    _logger.LogWarning("🔍 RUNTIME: Search() - VectorDatabaseService not found in DI container");
                    _logger.LogWarning("🔍 RUNTIME: Search() - Available services:");
                    
                    var allServices = _serviceProvider.GetServices<object>();
                    foreach (var service in allServices.Take(10))
                    {
                        _logger.LogWarning("🔍 RUNTIME: Search() - Service: {Type}", service.GetType().Name);
                    }
                    
                    // Still emit search event even if no service
                    var searchEventBus = GetEventBus();
                    if (searchEventBus != null)
                    {
                        await searchEventBus.EmitAsync("ai.search.failed", new
                        {
                            agent = className,
                            reason = "VectorDatabaseService not available",
                            query = query,
                            timestamp = DateTime.UtcNow
                        });
                    }
                    return;
                }
                
                _logger.LogInformation("🔍 RUNTIME: Search() - VectorDatabaseService found: {Type}", vectorService.GetType().Name);
                
                // Create search options
                var searchOptions = new VectorSearchOptions
                {
                    MaxResults = 10,
                    MinRelevance = 0.3f
                };
                
                _logger.LogInformation("🔍 RUNTIME: Search() - Calling vectorService.SearchAsync()");
                _logger.LogInformation("🔍 RUNTIME: Search() - Search options: MaxResults={MaxResults}, MinRelevance={MinRelevance}", 
                    searchOptions.MaxResults, searchOptions.MinRelevance);
                
                var searchResults = await vectorService.SearchAsync(query, searchOptions);
                
                _logger.LogInformation("🔍 RUNTIME: Search() - SearchAsync completed");
                _logger.LogInformation("🔍 RUNTIME: Search() - Found {Count} results", searchResults.TotalResults);
                _logger.LogInformation("🔍 RUNTIME: Search() - Processing time: {Time}ms", searchResults.ProcessingTimeMs);
                
                if (searchResults.Results.Any())
                {
                    _logger.LogInformation("🔍 RUNTIME: Search() - Top result preview: {Preview}...", 
                        searchResults.Results.First().Content.Length > 100 
                            ? searchResults.Results.First().Content[..100] 
                            : searchResults.Results.First().Content);
                }
                
                // Emit search complete event with detailed results
                var searchCompleteEventBus = GetEventBus();
                if (searchCompleteEventBus != null)
                {
                    _logger.LogInformation("🔍 RUNTIME: Search() - Emitting ai.search.complete event");
                    await searchCompleteEventBus.EmitAsync("ai.search.complete", new
                    {
                        agent = className,
                        query = query,
                        resultsCount = searchResults.TotalResults,
                        topResult = searchResults.Results.FirstOrDefault()?.Content ?? "No results",
                        processingTimeMs = searchResults.ProcessingTimeMs,
                        errorMessage = searchResults.ErrorMessage ?? "",
                        timestamp = DateTime.UtcNow,
                        allResults = searchResults.Results.Select(r => new { 
                            content = r.Content[..Math.Min(r.Content.Length, 200)], 
                            score = r.Score,
                            documentId = r.DocumentId 
                        }).ToList()
                    });
                    _logger.LogInformation("🔍 RUNTIME: Search() - Successfully emitted ai.search.complete event");
                }
                else
                {
                    _logger.LogWarning("🔍 RUNTIME: Search() - EventBus not available for search complete event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔍 RUNTIME: Search() - EXCEPTION during search process");
                _logger.LogError("🔍 RUNTIME: Search() - Exception type: {Type}", ex.GetType().Name);
                _logger.LogError("🔍 RUNTIME: Search() - Exception message: {Message}", ex.Message);
                _logger.LogError("🔍 RUNTIME: Search() - Exception stack: {Stack}", ex.StackTrace);
                
                // Emit search error event
                var searchErrorEventBus = GetEventBus();
                if (searchErrorEventBus != null)
                {
                    await searchErrorEventBus.EmitAsync("ai.search.error", new
                    {
                        agent = className,
                        error = ex.Message,
                        query = query,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            
            _logger.LogInformation("🔍 RUNTIME: Search() - Background task completed");
        });
        
        _logger.LogInformation("🔍 RUNTIME: Search() method completed - search task started in background");
    }

    /// <summary>
    /// Gets the unique collection name for this instance's vector store
    /// Used by the compiler to route AI service calls to instance-specific memory
    /// </summary>
    /// <returns>The unique collection name for this instance</returns>
    public string GetInstanceCollectionName()
    {
        return _instanceCollectionName;
    }

    /// <summary>
    /// Instance-specific think operation using this instance's unique Kernel Memory collection
    /// </summary>
    /// <param name="prompt">The thinking prompt</param>
    /// <param name="context">Additional context for the thought</param>
    /// <returns>Task representing the async operation</returns>
    protected async Task<string> ThinkAsync(string prompt, string? context = null)
    {
        if (_instanceMemory == null)
        {
            _logger.LogWarning("🧠 INSTANCE THINK: Kernel Memory not available for instance {Collection}", _instanceCollectionName);
            return "[THINK] Memory not available";
        }

        try
        {
            _logger.LogInformation("🧠 INSTANCE THINK: Starting for {Collection} - Prompt: {Prompt}", _instanceCollectionName, prompt);
            
            // Use instance-specific collection for thinking
            var fullPrompt = context != null ? $"{context}\n\n{prompt}" : prompt;
            var response = await _instanceMemory.AskAsync(fullPrompt, index: _instanceCollectionName);
            
            var result = response.Result ?? "[THINK] No response";
            
            // Store this thought in the instance's memory for learning
            var thoughtRecord = $"Thought: {prompt}\nContext: {context ?? "none"}\nResponse: {result}\nTimestamp: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
            var documentId = $"thought_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
            
            await _instanceMemory.ImportTextAsync(thoughtRecord, documentId: documentId, index: _instanceCollectionName);
            
            _logger.LogInformation("🧠 INSTANCE THINK: Completed for {Collection} - Result stored", _instanceCollectionName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🧠 INSTANCE THINK: Error for {Collection}", _instanceCollectionName);
            return $"[THINK ERROR] {ex.Message}";
        }
    }

    /// <summary>
    /// Instance-specific learn operation using this instance's unique Kernel Memory collection
    /// </summary>
    /// <param name="data">The data to learn</param>
    /// <param name="category">Category for the learning</param>
    /// <returns>Task representing the async operation</returns>
    protected async Task<string> LearnAsync(string data, string? category = null)
    {
        if (_instanceMemory == null)
        {
            _logger.LogWarning("📚 INSTANCE LEARN: Kernel Memory not available for instance {Collection}", _instanceCollectionName);
            return "[LEARN] Memory not available";
        }

        try
        {
            _logger.LogInformation("📚 INSTANCE LEARN: Starting for {Collection} - Data length: {Length}", _instanceCollectionName, data.Length);
            
            // Store learning data in instance-specific collection
            var learningRecord = $"Learning: {data}\nCategory: {category ?? "general"}\nTimestamp: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
            var documentId = $"learning_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
            
            await _instanceMemory.ImportTextAsync(learningRecord, documentId: documentId, index: _instanceCollectionName);
            
            _logger.LogInformation("📚 INSTANCE LEARN: Completed for {Collection} - Data stored", _instanceCollectionName);
            return $"[LEARN] Stored in {_instanceCollectionName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "📚 INSTANCE LEARN: Error for {Collection}", _instanceCollectionName);
            return $"[LEARN ERROR] {ex.Message}";
        }
    }

    /// <summary>
    /// Search this instance's personal memory
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum results to return</param>
    /// <returns>Search results from this instance's memory</returns>
    protected async Task<string[]> SearchInstanceMemoryAsync(string query, int limit = 5)
    {
        if (_instanceMemory == null)
        {
            _logger.LogWarning("🔍 INSTANCE SEARCH: Kernel Memory not available for instance {Collection}", _instanceCollectionName);
            return new[] { "[SEARCH] Memory not available" };
        }

        try
        {
            _logger.LogInformation("🔍 INSTANCE SEARCH: Searching {Collection} for: {Query}", _instanceCollectionName, query);
            
            var searchResult = await _instanceMemory.SearchAsync(query, index: _instanceCollectionName, limit: limit);
            
            var results = searchResult.Results
                .Select(r => r.Partitions.FirstOrDefault()?.Text ?? "No content")
                .ToArray();
            
            _logger.LogInformation("🔍 INSTANCE SEARCH: Found {Count} results in {Collection}", results.Length, _instanceCollectionName);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔍 INSTANCE SEARCH: Error searching {Collection}", _instanceCollectionName);
            return new[] { $"[SEARCH ERROR] {ex.Message}" };
        }
    }
}
