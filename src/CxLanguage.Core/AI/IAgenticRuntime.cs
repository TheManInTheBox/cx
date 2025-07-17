using CxLanguage.Core.Ast;
using CxLanguage.Core.Types;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CxLanguage.Core.AI;

/// <summary>
/// Core AI runtime for Cx - Scripting Language for Agentic AI Runtime
/// Enables quality, intelligent, autonomous workflows through AI-native capabilities
/// </summary>
public interface IAgenticRuntime
{
    Task<TaskPlanResult> PlanTaskAsync(string goal, TaskPlanningOptions? options = null);
    Task<TaskExecutionResult> ExecuteTaskAsync(TaskPlan plan, CancellationToken cancellationToken = default);
    Task<CodeSynthesisResult> SynthesizeCodeAsync(string specification, CodeSynthesisOptions? options = null);
    Task<object?> InvokeAIFunctionAsync(string functionName, object[] parameters, AIInvocationOptions? options = null);
}

/// <summary>
/// Autonomous task planning and intelligent orchestration
/// </summary>
public class TaskPlan
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Goal { get; init; } = string.Empty;
    public List<SubTask> SubTasks { get; init; } = new();
    public Dictionary<string, object> Context { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public TaskPlanStatus Status { get; set; } = TaskPlanStatus.Created;
}

/// <summary>
/// Individual sub-task within a plan
/// </summary>
public class SubTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public TaskType Type { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Types of tasks that can be executed
/// </summary>
public enum TaskType
{
    DataAnalysis,
    CodeGeneration,
    FunctionCall,
    WebRequest,
    FileOperation,
    DatabaseQuery,
    AIInference,
    Custom
}

/// <summary>
/// Task execution status
/// </summary>
public enum TaskStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Task plan status
/// </summary>
public enum TaskPlanStatus
{
    Created,
    Planning,
    Ready,
    Executing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Result of task planning
/// </summary>
public class TaskPlanResult
{
    public bool IsSuccess { get; init; }
    public TaskPlan? Plan { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan PlanningDuration { get; init; }

    public static TaskPlanResult Success(TaskPlan plan, TimeSpan duration) =>
        new() { IsSuccess = true, Plan = plan, PlanningDuration = duration };

    public static TaskPlanResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Result of task execution
/// </summary>
public class TaskExecutionResult
{
    public bool IsSuccess { get; init; }
    public Dictionary<string, object> Results { get; init; } = new();
    public List<string> ExecutionLog { get; init; } = new();
    public string? ErrorMessage { get; init; }
    public TimeSpan ExecutionDuration { get; init; }

    public static TaskExecutionResult Success(Dictionary<string, object> results, 
        List<string> log, TimeSpan duration) =>
        new() { IsSuccess = true, Results = results, ExecutionLog = log, ExecutionDuration = duration };

    public static TaskExecutionResult Failure(string errorMessage, List<string> log) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, ExecutionLog = log };
}

/// <summary>
/// Options for task planning
/// </summary>
public class TaskPlanningOptions
{
    public int MaxSubTasks { get; set; } = 10;
    public TimeSpan MaxPlanningTime { get; set; } = TimeSpan.FromMinutes(5);
    public string? Model { get; set; }
    public double Temperature { get; set; } = 0.7;
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

/// <summary>
/// Reasoning phases for the AI runtime
/// </summary>
public enum ReasoningPhase
{
    Plan,
    Execute,
    Evaluate,
    Refine
}

/// <summary>
/// Reasoning loop state
/// </summary>
public class ReasoningState
{
    public ReasoningPhase CurrentPhase { get; set; } = ReasoningPhase.Plan;
    public TaskPlan? CurrentPlan { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public List<string> ReasoningLog { get; set; } = new();
    public int IterationCount { get; set; } = 0;
    public int MaxIterations { get; set; } = 3;
}
