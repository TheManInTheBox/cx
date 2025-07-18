using CxLanguage.Core.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Concurrent;

namespace CxLanguage.Core.AI;

/// <summary>
/// Main implementation of the Agentic AI Runtime
/// Provides autonomous task planning, execution, and reasoning capabilities
/// </summary>
public class AgenticRuntime : IAgenticRuntime
{
    private readonly IAiService _aiService;
    private readonly IMultiModalAI _multiModalAI;
    private readonly ICodeSynthesizer _codeSynthesizer;
    private readonly ILogger<AgenticRuntime> _logger;
    private readonly ConcurrentDictionary<string, TaskPlan> _activePlans;
    private readonly ReasoningEngine _reasoningEngine;

    public AgenticRuntime(
        IAiService aiService,
        IMultiModalAI multiModalAI,
        ICodeSynthesizer codeSynthesizer,
        ILogger<AgenticRuntime> logger)
    {
        _aiService = aiService;
        _multiModalAI = multiModalAI;
        _codeSynthesizer = codeSynthesizer;
        _logger = logger;
        _activePlans = new ConcurrentDictionary<string, TaskPlan>();
        _reasoningEngine = new ReasoningEngine(aiService, logger);
    }

    public async Task<TaskPlanResult> PlanTaskAsync(string goal, TaskPlanningOptions? options = null)
    {
        var startTime = DateTime.UtcNow;
        options ??= new TaskPlanningOptions();

        try
        {
            _logger.LogInformation("Planning task with goal: {Goal}", goal);

            // Create initial plan
            var plan = new TaskPlan
            {
                Goal = goal,
                Status = TaskPlanStatus.Planning
            };

            _activePlans[plan.Id] = plan;

            // Use AI to decompose the goal into sub-tasks
            var planningPrompt = CreatePlanningPrompt(goal, options);
            
            var response = await _aiService.GenerateTextAsync(planningPrompt, new AiRequestOptions
            {
                Model = options.Model,
                Temperature = options.Temperature,
                MaxTokens = 2000,
                SystemPrompt = GetPlanningSystemPrompt()
            });

            if (!response.IsSuccess)
            {
                plan.Status = TaskPlanStatus.Failed;
                return TaskPlanResult.Failure($"Failed to generate plan: {response.ErrorMessage}");
            }

            // Parse the AI response into sub-tasks
            var subTasks = await ParsePlanningResponse(response.Content);
            plan.SubTasks.AddRange(subTasks);
            
            // Validate and optimize the plan
            var validationResult = await ValidatePlan(plan);
            if (!validationResult.IsValid)
            {
                plan.Status = TaskPlanStatus.Failed;
                return TaskPlanResult.Failure($"Plan validation failed: {validationResult.ErrorMessage}");
            }

            plan.Status = TaskPlanStatus.Ready;
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Successfully planned task {TaskId} with {SubTaskCount} sub-tasks in {Duration}ms", 
                plan.Id, plan.SubTasks.Count, duration.TotalMilliseconds);

            return TaskPlanResult.Success(plan, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error planning task: {Goal}", goal);
            return TaskPlanResult.Failure(ex.Message);
        }
    }

    public async Task<TaskExecutionResult> ExecuteTaskAsync(TaskPlan plan, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var executionLog = new List<string>();
        var results = new Dictionary<string, object>();

        try
        {
            _logger.LogInformation("Executing task plan: {PlanId} with {SubTaskCount} sub-tasks", 
                plan.Id, plan.SubTasks.Count);

            plan.Status = TaskPlanStatus.Executing;
            executionLog.Add($"Started execution of plan {plan.Id} at {startTime:yyyy-MM-dd HH:mm:ss}");

            // Execute sub-tasks with dependency resolution
            var executionOrder = ResolveDependencies(plan.SubTasks);
            
            foreach (var subTask in executionOrder)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    plan.Status = TaskPlanStatus.Cancelled;
                    executionLog.Add("Execution cancelled by user");
                    return TaskExecutionResult.Failure("Execution cancelled", executionLog);
                }

                var subTaskResult = await ExecuteSubTask(subTask, plan.Context, cancellationToken);
                
                if (subTaskResult.IsSuccess)
                {
                    subTask.Status = TaskStatus.Completed;
                    subTask.Result = subTaskResult.Result;
                    subTask.CompletedAt = DateTime.UtcNow;
                    
                    results[subTask.Id] = subTaskResult.Result ?? new object();
                    executionLog.Add($"✓ Completed sub-task: {subTask.Description}");
                }
                else
                {
                    subTask.Status = TaskStatus.Failed;
                    subTask.ErrorMessage = subTaskResult.ErrorMessage;
                    
                    executionLog.Add($"✗ Failed sub-task: {subTask.Description} - {subTaskResult.ErrorMessage}");
                    
                    // Decide whether to continue or abort based on task criticality
                    if (IsTaskCritical(subTask))
                    {
                        plan.Status = TaskPlanStatus.Failed;
                        var duration = DateTime.UtcNow - startTime;
                        return TaskExecutionResult.Failure($"Critical sub-task failed: {subTaskResult.ErrorMessage}", executionLog);
                    }
                }
            }

            plan.Status = TaskPlanStatus.Completed;
            var totalDuration = DateTime.UtcNow - startTime;
            
            executionLog.Add($"Completed execution of plan {plan.Id} in {totalDuration.TotalSeconds:F2} seconds");

            _logger.LogInformation("Successfully executed task plan {PlanId} in {Duration}ms", 
                plan.Id, totalDuration.TotalMilliseconds);

            return TaskExecutionResult.Success(results, executionLog, totalDuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing task plan: {PlanId}", plan.Id);
            plan.Status = TaskPlanStatus.Failed;
            executionLog.Add($"Execution failed with exception: {ex.Message}");
            return TaskExecutionResult.Failure(ex.Message, executionLog);
        }
    }

    public async Task<CodeSynthesisResult> SynthesizeCodeAsync(
        string specification, 
        CodeSynthesisOptions? options = null)
    {
        try
        {
            _logger.LogInformation("Synthesizing code from specification: {Spec}", 
                specification.Substring(0, Math.Min(100, specification.Length)));

            return await _codeSynthesizer.SynthesizeFunctionAsync(specification, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing code");
            return CodeSynthesisResult.Failure(ex.Message);
        }
    }

    public async Task<object?> InvokeAIFunctionAsync(
        string functionName, 
        object[] parameters, 
        AIInvocationOptions? options = null)
    {
        try
        {
            options ??= new AIInvocationOptions();
            
            _logger.LogInformation("Invoking AI function: {Function} with {ParamCount} parameters", 
                functionName, parameters.Length);

            var functionCallOptions = new FunctionCallOptions
            {
                Timeout = options.Timeout,
                Context = options.Context
            };

            var result = await _multiModalAI.ExecuteFunctionAsync(functionName, parameters, functionCallOptions);
            
            if (result.IsSuccess)
            {
                return result.Result;
            }
            else
            {
                _logger.LogWarning("AI function call failed: {Error}", result.ErrorMessage);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking AI function: {Function}", functionName);
            return null;
        }
    }

    private string CreatePlanningPrompt(string goal, TaskPlanningOptions options)
    {
        return $@"
Goal: {goal}

Please create a detailed execution plan by breaking down this goal into specific, actionable sub-tasks.

Requirements:
- Maximum {options.MaxSubTasks} sub-tasks
- Each sub-task should be specific and measurable
- Identify dependencies between sub-tasks
- Specify the type of each sub-task (DataAnalysis, CodeGeneration, FunctionCall, etc.)
- Include any necessary parameters for each sub-task

Respond with a JSON array of sub-tasks in this format:
[
  {{
    ""description"": ""Clear description of what needs to be done"",
    ""type"": ""TaskType"",
    ""dependencies"": [""id1"", ""id2""],
    ""parameters"": {{
      ""key1"": ""value1"",
      ""key2"": ""value2""
    }}
  }}
]

Available task types: DataAnalysis, CodeGeneration, FunctionCall, WebRequest, FileOperation, DatabaseQuery, AIInference, Custom
";
    }

    private string GetPlanningSystemPrompt()
    {
        return @"
You are an expert AI task planner. Your role is to decompose complex goals into executable sub-tasks.

Guidelines:
- Be specific and actionable in your task descriptions
- Consider dependencies and logical order
- Choose appropriate task types for each sub-task
- Keep the plan achievable and realistic
- Focus on creating value for the user

Always respond with valid JSON that can be parsed programmatically.
";
    }

    private async Task<List<SubTask>> ParsePlanningResponse(string response)
    {
        await Task.CompletedTask;
        try
        {
            // Extract JSON from the response (it might contain other text)
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']');
            
            if (jsonStart == -1 || jsonEnd == -1)
            {
                _logger.LogWarning("No JSON array found in planning response");
                return new List<SubTask>();
            }

            var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var planData = JsonSerializer.Deserialize<List<SubTaskData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var subTasks = new List<SubTask>();
            
            if (planData != null)
            {
                foreach (var data in planData)
                {
                    var subTask = new SubTask
                    {
                        Description = data.Description ?? "Unknown task",
                        Type = Enum.TryParse<TaskType>(data.Type, true, out var taskType) ? taskType : TaskType.Custom,
                        Dependencies = data.Dependencies ?? new List<string>(),
                        Parameters = data.Parameters ?? new Dictionary<string, object>()
                    };
                    
                    subTasks.Add(subTask);
                }
            }

            return subTasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing planning response");
            return new List<SubTask>();
        }
    }

    private async Task<ValidationResult> ValidatePlan(TaskPlan plan)
    {
        await Task.CompletedTask;
        // Basic validation logic
        if (plan.SubTasks.Count == 0)
        {
            return ValidationResult.Invalid("Plan contains no sub-tasks");
        }

        if (plan.SubTasks.Count > 20)
        {
            return ValidationResult.Invalid("Plan contains too many sub-tasks (max 20)");
        }

        // Check for circular dependencies
        if (HasCircularDependencies(plan.SubTasks))
        {
            return ValidationResult.Invalid("Plan contains circular dependencies");
        }

        return ValidationResult.Valid();
    }

    private List<SubTask> ResolveDependencies(List<SubTask> subTasks)
    {
        var resolved = new List<SubTask>();
        var remaining = new List<SubTask>(subTasks);

        while (remaining.Count > 0)
        {
            var next = remaining.FirstOrDefault(task => 
                task.Dependencies.All(dep => resolved.Any(r => r.Id == dep)));

            if (next == null)
            {
                // No task can be resolved, break circular dependency or add remaining tasks
                next = remaining.First();
            }

            resolved.Add(next);
            remaining.Remove(next);
        }

        return resolved;
    }

    private async Task<SubTaskExecutionResult> ExecuteSubTask(
        SubTask subTask, 
        Dictionary<string, object> context, 
        CancellationToken cancellationToken)
    {
        subTask.Status = TaskStatus.Running;
        subTask.StartedAt = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Executing sub-task: {Description} (Type: {Type})", 
                subTask.Description, subTask.Type);

            object? result = subTask.Type switch
            {
                TaskType.CodeGeneration => await ExecuteCodeGeneration(subTask, context),
                TaskType.DataAnalysis => await ExecuteDataAnalysis(subTask, context),
                TaskType.FunctionCall => await ExecuteFunctionCall(subTask, context),
                TaskType.AIInference => await ExecuteAIInference(subTask, context),
                TaskType.WebRequest => await ExecuteWebRequest(subTask, context),
                TaskType.FileOperation => await ExecuteFileOperation(subTask, context),
                TaskType.DatabaseQuery => await ExecuteDatabaseQuery(subTask, context),
                TaskType.Custom => await ExecuteCustomTask(subTask, context),
                _ => $"Executed {subTask.Type} task: {subTask.Description}"
            };

            return SubTaskExecutionResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sub-task: {Description}", subTask.Description);
            return SubTaskExecutionResult.Failure(ex.Message);
        }
    }

    private async Task<object> ExecuteCodeGeneration(SubTask subTask, Dictionary<string, object> context)
    {
        var specification = subTask.Parameters.GetValueOrDefault("specification")?.ToString() ?? subTask.Description;
        var options = new CodeSynthesisOptions
        {
            TargetLanguage = subTask.Parameters.GetValueOrDefault("language")?.ToString() ?? "cx",
            CompileImmediately = true
        };

        var result = await _codeSynthesizer.SynthesizeFunctionAsync(specification, options);
        return result.IsSuccess ? result.GeneratedCode ?? string.Empty : $"Failed: {result.ErrorMessage}";
    }

    private async Task<object> ExecuteDataAnalysis(SubTask subTask, Dictionary<string, object> context)
    {
        var data = subTask.Parameters.GetValueOrDefault("data")?.ToString() ?? "";
        var analysisType = subTask.Parameters.GetValueOrDefault("analysisType")?.ToString() ?? "general";

        var prompt = $"Analyze the following data with focus on {analysisType}:\n{data}";
        var response = await _aiService.GenerateTextAsync(prompt);
        
        return response.IsSuccess ? response.Content : $"Analysis failed: {response.ErrorMessage}";
    }

    private async Task<object> ExecuteFunctionCall(SubTask subTask, Dictionary<string, object> context)
    {
        var functionName = subTask.Parameters.GetValueOrDefault("functionName")?.ToString() ?? "";
        var parameters = subTask.Parameters.GetValueOrDefault("parameters") as object[] ?? Array.Empty<object>();

        if (string.IsNullOrEmpty(functionName))
        {
            return "Error: No function name specified";
        }

        var result = await _multiModalAI.ExecuteFunctionAsync(functionName, parameters);
        return result.IsSuccess ? result.Result ?? new object() : $"Function call failed: {result.ErrorMessage}";
    }

    private async Task<object> ExecuteAIInference(SubTask subTask, Dictionary<string, object> context)
    {
        var prompt = subTask.Parameters.GetValueOrDefault("prompt")?.ToString() ?? subTask.Description;
        var model = subTask.Parameters.GetValueOrDefault("model")?.ToString();

        var response = await _aiService.GenerateTextAsync(prompt, new AiRequestOptions { Model = model });
        return response.IsSuccess ? response.Content : $"AI inference failed: {response.ErrorMessage}";
    }

    private async Task<object> ExecuteWebRequest(SubTask subTask, Dictionary<string, object> context)
    {
        // TODO: Implement web request execution
        await Task.Delay(100);
        return $"Web request executed: {subTask.Description}";
    }

    private async Task<object> ExecuteFileOperation(SubTask subTask, Dictionary<string, object> context)
    {
        // TODO: Implement file operation execution
        await Task.Delay(100);
        return $"File operation executed: {subTask.Description}";
    }

    private async Task<object> ExecuteDatabaseQuery(SubTask subTask, Dictionary<string, object> context)
    {
        // TODO: Implement database query execution
        await Task.Delay(100);
        return $"Database query executed: {subTask.Description}";
    }

    private async Task<object> ExecuteCustomTask(SubTask subTask, Dictionary<string, object> context)
    {
        // TODO: Implement custom task execution
        await Task.Delay(100);
        return $"Custom task executed: {subTask.Description}";
    }

    private bool IsTaskCritical(SubTask subTask)
    {
        // Define logic to determine if a task is critical
        // For now, consider CodeGeneration and AIInference as critical
        return subTask.Type == TaskType.CodeGeneration || subTask.Type == TaskType.AIInference;
    }

    private bool HasCircularDependencies(List<SubTask> subTasks)
    {
        // Simple cycle detection using DFS
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var task in subTasks)
        {
            if (HasCycleDFS(task.Id, subTasks, visited, recursionStack))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasCycleDFS(string taskId, List<SubTask> subTasks, HashSet<string> visited, HashSet<string> recursionStack)
    {
        if (recursionStack.Contains(taskId))
        {
            return true; // Cycle detected
        }

        if (visited.Contains(taskId))
        {
            return false; // Already processed
        }

        visited.Add(taskId);
        recursionStack.Add(taskId);

        var task = subTasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            foreach (var dependency in task.Dependencies)
            {
                if (HasCycleDFS(dependency, subTasks, visited, recursionStack))
                {
                    return true;
                }
            }
        }

        recursionStack.Remove(taskId);
        return false;
    }
}

/// <summary>
/// Data structure for parsing sub-task JSON
/// </summary>
internal class SubTaskData
{
    public string? Description { get; set; }
    public string? Type { get; set; }
    public List<string>? Dependencies { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Validation result for task plans
/// </summary>
internal class ValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    public static ValidationResult Valid() => new() { IsValid = true };
    public static ValidationResult Invalid(string errorMessage) => new() { IsValid = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Result of sub-task execution
/// </summary>
internal class SubTaskExecutionResult
{
    public bool IsSuccess { get; init; }
    public object? Result { get; init; }
    public string? ErrorMessage { get; init; }

    public static SubTaskExecutionResult Success(object? result) => new() { IsSuccess = true, Result = result };
    public static SubTaskExecutionResult Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
