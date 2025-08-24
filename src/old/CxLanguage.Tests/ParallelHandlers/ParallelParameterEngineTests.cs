// 🧪 PARALLEL HANDLER PARAMETERS v1.0 - COMPREHENSIVE TEST SUITE
// Validates 200%+ performance improvement implementation
// Tests integration with existing EventBusService Task.WhenAll foundation

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using CxLanguage.Core.Events;
using CxLanguage.Runtime.ParallelHandlers;

namespace CxLanguage.Tests.ParallelHandlers
{
    /// <summary>
    /// Comprehensive test suite for Parallel Handler Parameters v1.0
    /// Validates 200%+ performance improvement through parameter-based parallel execution.
    /// </summary>
    public class ParallelParameterEngineTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICxEventBus _eventBus;
        private readonly ParallelParameterEngine _engine;
        private readonly ILogger<ParallelParameterEngineTests> _logger;
        
        public ParallelParameterEngineTests()
        {
            // Set up test service container
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder => builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug));
            
            // Add event bus
            services.AddSingleton<ICxEventBus, CxLanguage.Runtime.CxEventBus>();
            
            // Add parallel parameter services
            services.AddSingleton<HandlerParameterResolver>();
            services.AddSingleton<PayloadPropertyMapper>();
            services.AddSingleton<ParallelParameterEngine>();
            
            _serviceProvider = services.BuildServiceProvider();
            _eventBus = _serviceProvider.GetRequiredService<ICxEventBus>();
            _engine = _serviceProvider.GetRequiredService<ParallelParameterEngine>();
            _logger = _serviceProvider.GetRequiredService<ILogger<ParallelParameterEngineTests>>();
        }
        
        /// <summary>
        /// Test basic parallel parameter execution with performance improvement validation.
        /// </summary>
        [Fact]
        public async Task TestBasicParallelParameterExecution()
        {
            // Arrange: Create AI service call with parallel handlers
            var aiServiceCall = new
            {
                serviceName = "think",
                prompt = "analyze data",
                handlers = new object[]
                {
                    new { parameterName = "analysis", handlerName = "analysis.complete" },
                    new { parameterName = "summary", handlerName = "summary.generated" },
                    new { parameterName = "metrics", handlerName = "metrics.calculated" }
                }
            };
            
            var sourcePayload = new { data = "test data", consciousness = "analytical" };
            var originalEventName = "think.completed";
            
            // Set up result handlers to simulate handler responses
            SetupTestHandlers();
            
            var executionStart = DateTime.UtcNow;
            
            // Act: Execute with parallel parameters
            var result = await _engine.ExecuteWithParametersAsync(aiServiceCall, originalEventName, sourcePayload);
            
            var executionTime = DateTime.UtcNow - executionStart;
            
            // Assert: Validate successful parallel execution
            Assert.True(result.Success, $"Execution should succeed, but got: {result.Message}");
            Assert.Equal(3, result.ParameterCount);
            Assert.True(result.ParameterResults.Count >= 3, "Should have results for all parameters");
            Assert.True(result.ExecutionTimeMs < 3000, "Parallel execution should be faster than sequential");
            Assert.True(result.PerformanceImprovement > 0, "Should show performance improvement");
            
            _logger.LogInformation("✅ Basic parallel parameter test completed successfully");
            _logger.LogInformation($"   • Parameters: {result.ParameterCount}");
            _logger.LogInformation($"   • Execution time: {result.ExecutionTimeMs}ms");
            _logger.LogInformation($"   • Performance improvement: {result.PerformanceImprovement}%");
        }
        
        /// <summary>
        /// Test performance improvement measurement accuracy.
        /// </summary>
        [Fact]
        public async Task TestPerformanceImprovementMeasurement()
        {
            // Arrange: Create service call with multiple parameters for higher improvement
            var aiServiceCall = new
            {
                serviceName = "analyze",
                handlers = new Dictionary<string, string>
                {
                    ["analysis"] = "analysis.complete",
                    ["summary"] = "summary.generated",
                    ["metrics"] = "metrics.calculated",
                    ["insights"] = "insights.discovered",
                    ["recommendations"] = "recommendations.ready"
                }
            };
            
            SetupTestHandlers();
            
            // Act: Execute and measure performance
            var result = await _engine.ExecuteWithParametersAsync(aiServiceCall, "analyze.completed", new { data = "test" });
            
            // Assert: Validate performance metrics
            Assert.True(result.Success, "Execution should succeed");
            Assert.Equal(5, result.ParameterCount);
            Assert.True(result.PerformanceImprovement >= 200, $"Should achieve 200%+ improvement, got {result.PerformanceImprovement}%");
            Assert.True(result.ExecutionTimeMs < 2000, "Should complete in under 2 seconds");
            
            _logger.LogInformation("✅ Performance improvement test completed");
            _logger.LogInformation($"   • Target: 200%+ improvement");
            _logger.LogInformation($"   • Achieved: {result.PerformanceImprovement}% improvement");
            _logger.LogInformation($"   • Parameters: {result.ParameterCount}");
        }
        
        /// <summary>
        /// Test consciousness context preservation during parallel execution.
        /// </summary>
        [Fact]
        public async Task TestConsciousnessContextPreservation()
        {
            // Arrange: Service call with consciousness context
            var aiServiceCall = new
            {
                consciousness = "analytical-thinking",
                handlers = new[]
                {
                    new { parameterName = "analysis", handlerName = "analysis.complete" },
                    new { parameterName = "insights", handlerName = "insights.discovered" }
                }
            };
            
            var sourcePayload = new 
            { 
                consciousness = "test-consciousness",
                contextData = "important context"
            };
            
            SetupTestHandlers();
            
            // Act: Execute with consciousness context
            var result = await _engine.ExecuteWithParametersAsync(aiServiceCall, "conscious.analysis", sourcePayload);
            
            // Assert: Validate consciousness preservation
            Assert.True(result.Success, "Execution should succeed");
            Assert.NotNull(result.EnhancedPayload);
            
            // Check that consciousness context is preserved in results
            var enhancedPayloadDict = result.EnhancedPayload as Dictionary<string, object>;
            Assert.NotNull(enhancedPayloadDict);
            Assert.True(enhancedPayloadDict.ContainsKey("consciousness"), "Consciousness should be preserved");
            
            _logger.LogInformation("✅ Consciousness context preservation test completed");
        }
        
        /// <summary>
        /// Test enhanced payload creation and result aggregation.
        /// </summary>
        [Fact]
        public async Task TestEnhancedPayloadAggregation()
        {
            // Arrange: Service call with varied parameter types
            var aiServiceCall = new
            {
                handlers = new Dictionary<string, string>
                {
                    ["textAnalysis"] = "text.analyzed",
                    ["numericMetrics"] = "metrics.calculated",
                    ["recommendations"] = "recommendations.generated"
                }
            };
            
            var sourcePayload = new 
            { 
                inputText = "test input",
                numericData = new[] { 1, 2, 3, 4, 5 },
                metadata = new { version = "1.0", type = "test" }
            };
            
            SetupTestHandlers();
            
            // Act: Execute and check payload aggregation
            var result = await _engine.ExecuteWithParametersAsync(aiServiceCall, "aggregation.test", sourcePayload);
            
            // Assert: Validate enhanced payload structure
            Assert.True(result.Success, "Execution should succeed");
            Assert.NotNull(result.EnhancedPayload);
            
            var enhancedPayloadDict = result.EnhancedPayload as Dictionary<string, object>;
            Assert.NotNull(enhancedPayloadDict);
            
            // Check for parallel execution metadata
            Assert.True(enhancedPayloadDict.ContainsKey("_parallelExecution"), "Should contain parallel execution metadata");
            
            var parallelMeta = enhancedPayloadDict["_parallelExecution"];
            Assert.NotNull(parallelMeta);
            
            _logger.LogInformation("✅ Enhanced payload aggregation test completed");
        }
        
        /// <summary>
        /// Test error handling and graceful degradation.
        /// </summary>
        [Fact]
        public async Task TestErrorHandlingAndGracefulDegradation()
        {
            // Arrange: Service call with invalid handler configuration
            var invalidServiceCall = new
            {
                handlers = new object[] { null, "", "invalid" }
            };
            
            // Act: Execute with invalid configuration
            var result = await _engine.ExecuteWithParametersAsync(invalidServiceCall, "error.test", new { data = "test" });
            
            // Assert: Should handle errors gracefully
            Assert.False(result.Success, "Should fail with invalid configuration");
            Assert.NotNull(result.Message);
            Assert.True(result.Message.Contains("validation failed") || result.Message.Contains("failed"), 
                $"Error message should be descriptive: {result.Message}");
            
            _logger.LogInformation("✅ Error handling test completed");
            _logger.LogInformation($"   • Error handled gracefully: {result.Message}");
        }
        
        /// <summary>
        /// Test execution statistics and monitoring capabilities.
        /// </summary>
        [Fact]
        public async Task TestExecutionStatisticsAndMonitoring()
        {
            // Arrange: Multiple executions for statistics
            var serviceCall1 = new { handlers = new[] { new { parameterName = "test1", handlerName = "test1.complete" } } };
            var serviceCall2 = new { handlers = new[] { new { parameterName = "test2", handlerName = "test2.complete" } } };
            
            SetupTestHandlers();
            
            // Act: Execute multiple times
            var result1 = await _engine.ExecuteWithParametersAsync(serviceCall1, "stats.test1", new { data = "test1" });
            var result2 = await _engine.ExecuteWithParametersAsync(serviceCall2, "stats.test2", new { data = "test2" });
            
            // Get statistics
            var statistics = _engine.GetExecutionStatistics();
            
            // Assert: Validate statistics collection
            Assert.True(statistics.TotalExecutions >= 2, "Should record multiple executions");
            Assert.True(statistics.TotalParametersProcessed >= 2, "Should count processed parameters");
            Assert.True(statistics.AverageExecutionTimeMs > 0, "Should track execution time");
            Assert.True(statistics.AveragePerformanceImprovement >= 0, "Should track performance improvement");
            
            _logger.LogInformation("✅ Execution statistics test completed");
            _logger.LogInformation($"   • Total executions: {statistics.TotalExecutions}");
            _logger.LogInformation($"   • Average execution time: {statistics.AverageExecutionTimeMs}ms");
            _logger.LogInformation($"   • Average performance improvement: {statistics.AveragePerformanceImprovement}%");
        }
        
        /// <summary>
        /// Set up test event handlers that provide AI service responses.
        /// </summary>
        private void SetupTestHandlers()
        {
            // Set up handlers that respond to test events
            _eventBus.Subscribe("analysis.complete", async (sender, eventName, payload) =>
            {
                // Real processing without artificial delays
                await _eventBus.EmitAsync("analysis.complete.result", new Dictionary<string, object>
                {
                    ["analysisResult"] = "Analysis completed successfully",
                    ["confidence"] = 0.95,
                    ["timestamp"] = DateTime.UtcNow
                });
                return true;
            });
            
            _eventBus.Subscribe("summary.generated", async (sender, eventName, payload) =>
            {
                // Real processing without artificial delays
                await _eventBus.EmitAsync("summary.generated.result", new Dictionary<string, object>
                {
                    ["summary"] = "Test summary generated",
                    ["wordCount"] = 250,
                    ["timestamp"] = DateTime.UtcNow
                });
                return true;
            });
            
            _eventBus.Subscribe("metrics.calculated", async (sender, eventName, payload) =>
            {
                await _eventBus.EmitAsync("metrics.calculated.result", new Dictionary<string, object>
                {
                    ["metrics"] = new { accuracy = 0.92, precision = 0.88, recall = 0.91 },
                    ["calculationTime"] = 200,
                    ["timestamp"] = DateTime.UtcNow
                });
                return true;
            });
            
            _eventBus.Subscribe("insights.discovered", async (sender, eventName, payload) =>
            {
                await _eventBus.EmitAsync("insights.discovered.result", new Dictionary<string, object>
                {
                    ["insights"] = new[] { "Pattern A detected", "Trend B identified", "Anomaly C found" },
                    ["insightCount"] = 3,
                    ["timestamp"] = DateTime.UtcNow
                });
                return true;
            });
            
            _eventBus.Subscribe("recommendations.ready", async (sender, eventName, payload) =>
            {
                await _eventBus.EmitAsync("recommendations.ready.result", new Dictionary<string, object>
                {
                    ["recommendations"] = new[] { "Recommendation 1", "Recommendation 2", "Recommendation 3" },
                    ["priority"] = "high",
                    ["timestamp"] = DateTime.UtcNow
                });
                return true;
            });
            
            // Generic test handlers
            _eventBus.Subscribe("test1.complete", async (sender, eventName, payload) =>
            {
                await _eventBus.EmitAsync("test1.complete.result", new Dictionary<string, object> { ["result"] = "test1 completed" });
                return true;
            });
            
            _eventBus.Subscribe("test2.complete", async (sender, eventName, payload) =>
            {
                await _eventBus.EmitAsync("test2.complete.result", new Dictionary<string, object> { ["result"] = "test2 completed" });
                return true;
            });
            
            _eventBus.Subscribe("text.analyzed", async (sender, eventName, payload) =>
            {
                await _eventBus.EmitAsync("text.analyzed.result", new Dictionary<string, object> 
                { 
                    ["result"] = "Text analysis complete",
                    ["sentiment"] = "positive",
                    ["entities"] = new[] { "entity1", "entity2" }
                });
                return true;
            });
            
            _eventBus.Subscribe("recommendations.generated", async (sender, eventName, payload) =>
            {
                await _eventBus.EmitAsync("recommendations.generated.result", new Dictionary<string, object> 
                { 
                    ["result"] = "Recommendations generated",
                    ["count"] = 5,
                    ["categories"] = new[] { "performance", "optimization", "security" }
                });
                return true;
            });
        }
    }
    
    /// <summary>
    /// Integration tests for the Parallel Parameter Integration Service.
    /// </summary>
    public class ParallelParameterIntegrationServiceTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICxEventBus _eventBus;
        private readonly ParallelParameterIntegrationService _integrationService;
        private readonly ILogger<ParallelParameterIntegrationServiceTests> _logger;
        
        public ParallelParameterIntegrationServiceTests()
        {
            // Set up test service container
            var services = new ServiceCollection();
            
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            services.AddSingleton<ICxEventBus, CxLanguage.Runtime.CxEventBus>();
            services.AddSingleton<HandlerParameterResolver>();
            services.AddSingleton<PayloadPropertyMapper>();
            services.AddSingleton<ParallelParameterEngine>();
            services.AddSingleton<ParallelParameterIntegrationService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _eventBus = _serviceProvider.GetRequiredService<ICxEventBus>();
            _integrationService = _serviceProvider.GetRequiredService<ParallelParameterIntegrationService>();
            _logger = _serviceProvider.GetRequiredService<ILogger<ParallelParameterIntegrationServiceTests>>();
        }
        
        /// <summary>
        /// Test integration with think service calls.
        /// </summary>
        [Fact]
        public async Task TestThinkServiceIntegration()
        {
            // Arrange: Think service call with handlers
            var thinkPayload = new Dictionary<string, object>
            {
                ["prompt"] = "analyze user behavior patterns",
                ["consciousness"] = "analytical",
                ["handlers"] = new object[]
                {
                    new { parameterName = "patterns", handlerName = "patterns.identified" },
                    new { parameterName = "insights", handlerName = "insights.generated" }
                }
            };
            
            // Set up result capture
            var resultsReceived = new List<Dictionary<string, object>>();
            _eventBus.Subscribe("parallel.result.enhanced", async (sender, eventName, payload) =>
            {
                resultsReceived.Add(new Dictionary<string, object>(payload ?? new Dictionary<string, object>()));
                return true;
            });
            
            // Act: Emit think request
            await _eventBus.EmitAsync("think.request", thinkPayload);
            
            // Wait for processing
            await Task.Delay(100);
            
            // Assert: Validate integration handled the request
            Assert.True(resultsReceived.Count > 0, "Should receive enhanced parallel results");
            
            var result = resultsReceived[0];
            Assert.True(result.ContainsKey("success"), "Should contain success indicator");
            Assert.True(result.ContainsKey("performanceImprovement"), "Should contain performance metrics");
            
            _logger.LogInformation("✅ Think service integration test completed");
        }
        
        /// <summary>
        /// Test integration statistics collection.
        /// </summary>
        [Fact]
        public async Task TestIntegrationStatisticsCollection()
        {
            // Arrange: Multiple service calls
            var thinkPayload = new Dictionary<string, object>
            {
                ["handlers"] = new[] { new { parameterName = "test", handlerName = "test.complete" } }
            };
            
            var learnPayload = new Dictionary<string, object>
            {
                ["handlers"] = new[] { new { parameterName = "knowledge", handlerName = "knowledge.acquired" } }
            };
            
            // Act: Execute multiple service calls
            await _eventBus.EmitAsync("think.request", thinkPayload);
            await _eventBus.EmitAsync("learn.request", learnPayload);
            
            // Wait for processing
            await Task.Delay(100);
            
            // Get statistics
            var statistics = _integrationService.GetIntegrationStatistics();
            var summary = _integrationService.GetPerformanceSummary();
            
            // Assert: Validate statistics collection
            Assert.True(statistics.Count > 0, "Should collect statistics for different event types");
            Assert.True(summary.TotalExecutions > 0, "Should track total executions");
            Assert.True(summary.TotalParametersProcessed > 0, "Should track parameter processing");
            
            _logger.LogInformation("✅ Integration statistics test completed");
            _logger.LogInformation($"   • Event types: {summary.TotalEventTypes}");
            _logger.LogInformation($"   • Total executions: {summary.TotalExecutions}");
            _logger.LogInformation($"   • Average performance improvement: {summary.AveragePerformanceImprovement}%");
        }
    }
}
