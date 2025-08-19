using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CxLanguage.Core.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Core.AI.Mock
{
    /// <summary>
    /// Mock implementation of IAgenticRuntime for testing purposes
    /// </summary>
    public class MockAgenticRuntime : IAgenticRuntime
    {
        private readonly ILogger<MockAgenticRuntime> _logger;
        private readonly Random _random = new Random();

        public MockAgenticRuntime(ILogger<MockAgenticRuntime> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TaskPlanResult> PlanTaskAsync(string goal, TaskPlanningOptions? options = null)
        {
            _logger.LogInformation("Planning task: {Goal}", goal);
            
            var subtasks = new List<SubTask>
            {
                new SubTask { Id = "1", Description = "Analyze the goal", Status = TaskStatus.Completed },
                new SubTask { Id = "2", Description = "Create implementation plan", Status = TaskStatus.Completed },
                new SubTask { Id = "3", Description = "Execute implementation steps", Status = TaskStatus.Running }
            };
            
            var plan = new TaskPlan
            {
                Goal = goal,
                SubTasks = subtasks,
                Status = TaskPlanStatus.Executing
            };
            
            return Task.FromResult(new TaskPlanResult
            {
                IsSuccess = true,
                Plan = plan
            });
        }

        public Task<TaskExecutionResult> ExecuteTaskAsync(TaskPlan plan, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing task plan for goal: {Goal}", plan.Goal);
            
            // Simulate task execution
            var results = new Dictionary<string, object>
            {
                ["completed"] = true,
                ["steps"] = plan.SubTasks.Count,
                ["summary"] = $"Task completed successfully: {plan.Goal}"
            };
            
            var executionLog = new List<string>
            {
                $"Started execution of plan for: {plan.Goal}",
                $"Executed {plan.SubTasks.Count} subtasks",
                "All tasks completed successfully"
            };
            
            return Task.FromResult(
                TaskExecutionResult.Success(results, executionLog, TimeSpan.FromSeconds(2))
            );
        }

        public Task<CodeSynthesisResult> SynthesizeCodeAsync(string specification, CodeSynthesisOptions? options = null)
        {
            _logger.LogInformation("Synthesizing code for: {Spec}", specification);
            
            string codeText = "";
            
            if (specification.Contains("factorial"))
            {
                codeText = "function factorial(n)\n{\n    if (n <= 1)\n    {\n        return 1;\n    }\n    return n * factorial(n - 1);\n}";
            }
            else
            {
                codeText = "function example()\n{\n    // Code would be generated based on specification\n    // This is a mock implementation\n    print(\"Generated code example\");\n}";
            }
            
            // Use the proper static Success method or properties
            return Task.FromResult(CodeSynthesisResult.Success(codeText));
        }

        public Task<object?> InvokeAIFunctionAsync(string functionName, object[] parameters, AIInvocationOptions? options = null)
        {
            _logger.LogInformation("Invoking AI function: {Function}", functionName);
            
            // Implement mock responses for each AI function
            return functionName.ToLower() switch
            {
                "reason" => Task.FromResult<object?>(
                    "Reasoning analysis: An AI-native programming language provides seamless integration " +
                    "with AI models, reduces boilerplate code, enables autonomous workflows, and " +
                    "supports advanced capabilities like function introspection and self-modification."),
                
                "process" => Task.FromResult<object?>(
                    "Processed data successfully. The analysis shows positive sentiment with 87% confidence."),
                
                "generate" => Task.FromResult<object?>(
                    "Silicon thoughts in digital streams,\n" +
                    "Code and cognition interwoven dreams.\n" +
                    "Algorithms dance with human mind,\n" +
                    "In AI's embrace, new worlds we find."),
                
                "embed" => Task.FromResult<object?>(
                    $"Generated embedding vector with {_random.Next(768, 1536)} dimensions"),
                
                "adapt" => Task.FromResult<object?>(
                    "Function optimized. Improved performance by approximately 35%."),
                
                _ => Task.FromResult<object?>("Unknown AI function invoked")
            };
        }
    }
}

