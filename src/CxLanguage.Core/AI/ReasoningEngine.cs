using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CxLanguage.Core.AI;

/// <summary>
/// Reasoning engine implementing the plan-execute-evaluate-refine loop
/// </summary>
public class ReasoningEngine
{
    private readonly IAiService _aiService;
    private readonly ILogger _logger;

    public ReasoningEngine(IAiService aiService, ILogger logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Execute the full reasoning loop for a given goal
    /// </summary>
    public async Task<ReasoningResult> ReasonAsync(string goal, ReasoningOptions? options = null)
    {
        options ??= new ReasoningOptions();
        var state = new ReasoningState
        {
            MaxIterations = options.MaxIterations
        };

        var startTime = DateTime.UtcNow;
        state.ReasoningLog.Add($"Starting reasoning for goal: {goal}");

        try
        {
            while (state.IterationCount < state.MaxIterations)
            {
                state.IterationCount++;
                _logger.LogInformation("Reasoning iteration {Iteration} for goal: {Goal}", 
                    state.IterationCount, goal);

                switch (state.CurrentPhase)
                {
                    case ReasoningPhase.Plan:
                        await ExecutePlanPhase(goal, state, options);
                        break;
                    case ReasoningPhase.Execute:
                        await ExecuteExecutionPhase(state, options);
                        break;
                    case ReasoningPhase.Evaluate:
                        await ExecuteEvaluationPhase(goal, state, options);
                        break;
                    case ReasoningPhase.Refine:
                        await ExecuteRefinementPhase(goal, state, options);
                        break;
                }

                // Check if we've reached a satisfactory solution
                if (await IsSolutionSatisfactory(goal, state, options))
                {
                    state.ReasoningLog.Add($"Satisfactory solution reached after {state.IterationCount} iterations");
                    break;
                }

                // Move to next phase
                state.CurrentPhase = GetNextPhase(state.CurrentPhase);
            }

            var duration = DateTime.UtcNow - startTime;
            state.ReasoningLog.Add($"Reasoning completed in {duration.TotalSeconds:F2} seconds");

            return ReasoningResult.Success(state, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reasoning process");
            state.ReasoningLog.Add($"Reasoning failed: {ex.Message}");
            return ReasoningResult.Failure(ex.Message, state);
        }
    }

    private async Task ExecutePlanPhase(string goal, ReasoningState state, ReasoningOptions options)
    {
        state.ReasoningLog.Add("Phase: Planning");

        var planningPrompt = CreatePlanningPrompt(goal, state);
        var response = await _aiService.GenerateTextAsync(planningPrompt, new AiRequestOptions
        {
            Model = options.Model,
            Temperature = options.PlanningTemperature,
            MaxTokens = 2000,
            SystemPrompt = GetPlanningSystemPrompt()
        });

        if (response.IsSuccess)
        {
            var plan = await ParsePlanningResponse(response.Content, goal);
            state.CurrentPlan = plan;
            state.Context["planning_response"] = response.Content;
            state.ReasoningLog.Add($"Generated plan with {plan?.SubTasks.Count ?? 0} sub-tasks");
        }
        else
        {
            state.ReasoningLog.Add($"Planning failed: {response.ErrorMessage}");
        }
    }

    private async Task ExecuteExecutionPhase(ReasoningState state, ReasoningOptions options)
    {
        state.ReasoningLog.Add("Phase: Execution");

        if (state.CurrentPlan == null)
        {
            state.ReasoningLog.Add("No plan available for execution");
            return;
        }

        // Simulate execution of plan steps
        var executionResults = new List<string>();
        
        foreach (var subTask in state.CurrentPlan.SubTasks)
        {
            var executionPrompt = CreateExecutionPrompt(subTask, state);
            var response = await _aiService.GenerateTextAsync(executionPrompt, new AiRequestOptions
            {
                Model = options.Model,
                Temperature = options.ExecutionTemperature,
                MaxTokens = 1000
            });

            if (response.IsSuccess)
            {
                executionResults.Add(response.Content);
                subTask.Status = TaskStatus.Completed;
                subTask.Result = response.Content;
            }
            else
            {
                executionResults.Add($"Failed: {response.ErrorMessage}");
                subTask.Status = TaskStatus.Failed;
                subTask.ErrorMessage = response.ErrorMessage;
            }
        }

        state.Context["execution_results"] = executionResults;
        state.ReasoningLog.Add($"Executed {executionResults.Count} plan steps");
    }

    private async Task ExecuteEvaluationPhase(string goal, ReasoningState state, ReasoningOptions options)
    {
        state.ReasoningLog.Add("Phase: Evaluation");

        var evaluationPrompt = CreateEvaluationPrompt(goal, state);
        var response = await _aiService.GenerateTextAsync(evaluationPrompt, new AiRequestOptions
        {
            Model = options.Model,
            Temperature = options.EvaluationTemperature,
            MaxTokens = 1500,
            SystemPrompt = GetEvaluationSystemPrompt()
        });

        if (response.IsSuccess)
        {
            var evaluation = await ParseEvaluationResponse(response.Content);
            state.Context["evaluation"] = evaluation;
            state.ReasoningLog.Add($"Evaluation completed with score: {evaluation.Score:F2}");
        }
        else
        {
            state.ReasoningLog.Add($"Evaluation failed: {response.ErrorMessage}");
        }
    }

    private async Task ExecuteRefinementPhase(string goal, ReasoningState state, ReasoningOptions options)
    {
        state.ReasoningLog.Add("Phase: Refinement");

        var refinementPrompt = CreateRefinementPrompt(goal, state);
        var response = await _aiService.GenerateTextAsync(refinementPrompt, new AiRequestOptions
        {
            Model = options.Model,
            Temperature = options.RefinementTemperature,
            MaxTokens = 2000,
            SystemPrompt = GetRefinementSystemPrompt()
        });

        if (response.IsSuccess)
        {
            var refinements = await ParseRefinementResponse(response.Content);
            state.Context["refinements"] = refinements;
            state.ReasoningLog.Add($"Generated {refinements.Count} refinements");

            // Apply refinements to the current plan
            await ApplyRefinements(state, refinements);
        }
        else
        {
            state.ReasoningLog.Add($"Refinement failed: {response.ErrorMessage}");
        }
    }

    private async Task<bool> IsSolutionSatisfactory(string goal, ReasoningState state, ReasoningOptions options)
    {
        if (state.Context.TryGetValue("evaluation", out var evalObj) && evalObj is EvaluationResult eval)
        {
            return eval.Score >= options.SatisfactionThreshold;
        }

        return false;
    }

    private ReasoningPhase GetNextPhase(ReasoningPhase currentPhase)
    {
        return currentPhase switch
        {
            ReasoningPhase.Plan => ReasoningPhase.Execute,
            ReasoningPhase.Execute => ReasoningPhase.Evaluate,
            ReasoningPhase.Evaluate => ReasoningPhase.Refine,
            ReasoningPhase.Refine => ReasoningPhase.Plan,
            _ => ReasoningPhase.Plan
        };
    }

    private string CreatePlanningPrompt(string goal, ReasoningState state)
    {
        var context = state.IterationCount > 1 ? GetContextSummary(state) : "";
        
        return $@"
Goal: {goal}

{context}

Create a detailed plan to achieve this goal. Consider:
1. What are the key steps needed?
2. What resources or information are required?
3. What are potential obstacles and how to overcome them?
4. How can progress be measured?

Provide a structured plan with specific, actionable steps.
";
    }

    private string CreateExecutionPrompt(SubTask subTask, ReasoningState state)
    {
        return $@"
Execute the following task step:
{subTask.Description}

Task Type: {subTask.Type}
Parameters: {JsonSerializer.Serialize(subTask.Parameters)}

Provide the result of executing this step, including any outputs, findings, or next actions needed.
";
    }

    private string CreateEvaluationPrompt(string goal, ReasoningState state)
    {
        var executionResults = state.Context.GetValueOrDefault("execution_results") as List<string> ?? new List<string>();
        var resultsText = string.Join("\n", executionResults);

        return $@"
Original Goal: {goal}

Execution Results:
{resultsText}

Evaluate how well the execution results achieve the original goal:

1. Completeness: How much of the goal was accomplished? (0-100%)
2. Quality: How well was it done? (0-100%)
3. Efficiency: Was it done in an optimal way? (0-100%)
4. Issues: What problems or gaps exist?
5. Overall Score: (0-100%)

Provide your evaluation in JSON format:
{{
  ""completeness"": 0-100,
  ""quality"": 0-100,
  ""efficiency"": 0-100,
  ""issues"": [""issue1"", ""issue2""],
  ""score"": 0-100,
  ""reasoning"": ""explanation""
}}
";
    }

    private string CreateRefinementPrompt(string goal, ReasoningState state)
    {
        var evaluation = state.Context.GetValueOrDefault("evaluation") as EvaluationResult;
        var issues = evaluation?.Issues ?? new List<string>();

        return $@"
Goal: {goal}
Current Issues: {string.Join(", ", issues)}
Current Score: {evaluation?.Score ?? 0:F2}

Based on the evaluation, suggest specific improvements to achieve the goal better:

1. What should be changed in the approach?
2. What additional steps are needed?
3. What should be done differently?
4. How can efficiency be improved?

Provide refinements as a JSON array:
[
  {{
    ""type"": ""modification"" | ""addition"" | ""removal"",
    ""target"": ""what to change"",
    ""action"": ""specific action to take"",
    ""reason"": ""why this improvement is needed""
  }}
]
";
    }

    private string GetContextSummary(ReasoningState state)
    {
        var summary = new List<string>();
        
        if (state.Context.TryGetValue("evaluation", out var evalObj) && evalObj is EvaluationResult eval)
        {
            summary.Add($"Previous attempt scored {eval.Score:F2}% with issues: {string.Join(", ", eval.Issues)}");
        }

        if (state.Context.TryGetValue("refinements", out var refObj) && refObj is List<Refinement> refinements)
        {
            summary.Add($"Applied {refinements.Count} refinements from previous iteration");
        }

        return summary.Count > 0 ? $"Context from previous iterations:\n{string.Join("\n", summary)}\n" : "";
    }

    private async Task<TaskPlan> ParsePlanningResponse(string response, string goal)
    {
        try
        {
            // This is a simplified parsing - in practice, you'd want more robust JSON extraction
            var plan = new TaskPlan { Goal = goal };
            
            // Extract main steps from the response
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.Trim().StartsWith("-") || line.Trim().StartsWith("1.") || 
                               line.Trim().StartsWith("2.") || line.Trim().StartsWith("3."))
                .Take(10);

            foreach (var line in lines)
            {
                var description = line.Trim().TrimStart('-', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ' ');
                if (!string.IsNullOrWhiteSpace(description))
                {
                    plan.SubTasks.Add(new SubTask
                    {
                        Description = description,
                        Type = TaskType.Custom
                    });
                }
            }

            return plan;
        }
        catch
        {
            return new TaskPlan { Goal = goal };
        }
    }

    private async Task<EvaluationResult> ParseEvaluationResponse(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            
            if (jsonStart != -1 && jsonEnd != -1)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var evalData = JsonSerializer.Deserialize<EvaluationData>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (evalData != null)
                {
                    return new EvaluationResult
                    {
                        Score = evalData.Score,
                        Completeness = evalData.Completeness,
                        Quality = evalData.Quality,
                        Efficiency = evalData.Efficiency,
                        Issues = evalData.Issues ?? new List<string>(),
                        Reasoning = evalData.Reasoning ?? ""
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse evaluation response");
        }

        return new EvaluationResult { Score = 50.0 }; // Default neutral score
    }

    private async Task<List<Refinement>> ParseRefinementResponse(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']');
            
            if (jsonStart != -1 && jsonEnd != -1)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var refinementData = JsonSerializer.Deserialize<List<RefinementData>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (refinementData != null)
                {
                    return refinementData.Select(r => new Refinement
                    {
                        Type = r.Type ?? "modification",
                        Target = r.Target ?? "",
                        Action = r.Action ?? "",
                        Reason = r.Reason ?? ""
                    }).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse refinement response");
        }

        return new List<Refinement>();
    }

    private async Task ApplyRefinements(ReasoningState state, List<Refinement> refinements)
    {
        if (state.CurrentPlan == null) return;

        foreach (var refinement in refinements)
        {
            switch (refinement.Type.ToLower())
            {
                case "addition":
                    state.CurrentPlan.SubTasks.Add(new SubTask
                    {
                        Description = refinement.Action,
                        Type = TaskType.Custom
                    });
                    break;
                case "modification":
                    // Find and modify existing sub-task
                    var existingTask = state.CurrentPlan.SubTasks
                        .FirstOrDefault(t => t.Description.Contains(refinement.Target, StringComparison.OrdinalIgnoreCase));
                    if (existingTask != null)
                    {
                        existingTask.Description = refinement.Action;
                        existingTask.Status = TaskStatus.Pending; // Reset status for re-execution
                    }
                    break;
                case "removal":
                    // Remove sub-task
                    var taskToRemove = state.CurrentPlan.SubTasks
                        .FirstOrDefault(t => t.Description.Contains(refinement.Target, StringComparison.OrdinalIgnoreCase));
                    if (taskToRemove != null)
                    {
                        state.CurrentPlan.SubTasks.Remove(taskToRemove);
                    }
                    break;
            }
        }
    }

    private string GetPlanningSystemPrompt()
    {
        return @"
You are an expert strategic planner. Create detailed, actionable plans that break down complex goals into manageable steps.

Guidelines:
- Be specific and concrete in your recommendations
- Consider dependencies and logical sequencing
- Identify potential risks and mitigation strategies
- Focus on measurable outcomes
- Keep plans realistic and achievable
";
    }

    private string GetEvaluationSystemPrompt()
    {
        return @"
You are an expert evaluator. Assess how well execution results achieve the stated goals.

Guidelines:
- Be objective and thorough in your assessment
- Consider both quantitative and qualitative factors
- Identify specific areas for improvement
- Provide actionable feedback
- Use the full 0-100 scale appropriately
";
    }

    private string GetRefinementSystemPrompt()
    {
        return @"
You are an expert improvement advisor. Suggest specific refinements to enhance goal achievement.

Guidelines:
- Focus on practical, actionable improvements
- Address root causes, not just symptoms
- Consider efficiency and effectiveness
- Provide clear reasoning for each suggestion
- Prioritize high-impact changes
";
    }
}

/// <summary>
/// Options for the reasoning process
/// </summary>
public class ReasoningOptions
{
    public int MaxIterations { get; set; } = 3;
    public double SatisfactionThreshold { get; set; } = 80.0;
    public string? Model { get; set; }
    public double PlanningTemperature { get; set; } = 0.7;
    public double ExecutionTemperature { get; set; } = 0.5;
    public double EvaluationTemperature { get; set; } = 0.3;
    public double RefinementTemperature { get; set; } = 0.6;
}

/// <summary>
/// Result of the reasoning process
/// </summary>
public class ReasoningResult
{
    public bool IsSuccess { get; init; }
    public ReasoningState? State { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }

    public static ReasoningResult Success(ReasoningState state, TimeSpan duration) =>
        new() { IsSuccess = true, State = state, Duration = duration };

    public static ReasoningResult Failure(string errorMessage, ReasoningState? state = null) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, State = state };
}

/// <summary>
/// Evaluation result structure
/// </summary>
public class EvaluationResult
{
    public double Score { get; set; }
    public double Completeness { get; set; }
    public double Quality { get; set; }
    public double Efficiency { get; set; }
    public List<string> Issues { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
}

/// <summary>
/// Refinement suggestion
/// </summary>
public class Refinement
{
    public string Type { get; set; } = string.Empty; // modification, addition, removal
    public string Target { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

// Internal data structures for JSON parsing
internal class EvaluationData
{
    public double Score { get; set; }
    public double Completeness { get; set; }
    public double Quality { get; set; }
    public double Efficiency { get; set; }
    public List<string>? Issues { get; set; }
    public string? Reasoning { get; set; }
}

internal class RefinementData
{
    public string? Type { get; set; }
    public string? Target { get; set; }
    public string? Action { get; set; }
    public string? Reason { get; set; }
}
