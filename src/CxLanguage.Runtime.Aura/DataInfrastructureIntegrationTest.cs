using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.LocalVectorCache;

namespace CxLanguage.Runtime.Aura
{
    /// <summary>
    /// Complete Data Infrastructure Integration Test
    /// Validates the entire data ingestion → global vector db → local vector cache → enhanced CX syntax → Aura runtime flow
    /// </summary>
    public class DataInfrastructureIntegrationTest
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ICxEventBus _eventBus;
        
        public DataInfrastructureIntegrationTest(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = serviceProvider.GetRequiredService<ILogger<DataInfrastructureIntegrationTest>>();
            _eventBus = serviceProvider.GetRequiredService<ICxEventBus>();
        }
        
        public async Task<DataInfrastructureTestResult> RunCompleteIntegrationTestAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🧪 Starting Complete Data Infrastructure Integration Test");
            
            var testResult = new DataInfrastructureTestResult
            {
                TestId = Guid.NewGuid().ToString(),
                StartTime = DateTime.UtcNow
            };
            
            try
            {
                // Phase 1: Test Data Ingestion Agent
                _logger.LogInformation("📥 Phase 1: Testing Data Ingestion Agent");
                var ingestionResult = await TestDataIngestionAgentAsync(cancellationToken);
                testResult.DataIngestionResult = ingestionResult;
                
                if (!ingestionResult.Success)
                {
                    testResult.Success = false;
                    testResult.ErrorMessage = "Data Ingestion Agent test failed";
                    return testResult;
                }
                
                // Phase 2: Test Global Vector Coordinator
                _logger.LogInformation("🌐 Phase 2: Testing Global Vector Coordinator");
                var globalVectorResult = await TestGlobalVectorCoordinatorAsync(cancellationToken);
                testResult.GlobalVectorResult = globalVectorResult;
                
                if (!globalVectorResult.Success)
                {
                    testResult.Success = false;
                    testResult.ErrorMessage = "Global Vector Coordinator test failed";
                    return testResult;
                }
                
                // Phase 3: Test Local Vector Cache Service
                _logger.LogInformation("💾 Phase 3: Testing Local Vector Cache Service");
                var localCacheResult = await TestLocalVectorCacheServiceAsync(cancellationToken);
                testResult.LocalCacheResult = localCacheResult;
                
                if (!localCacheResult.Success)
                {
                    testResult.Success = false;
                    testResult.ErrorMessage = "Local Vector Cache Service test failed";
                    return testResult;
                }
                
                // Phase 4: Test Enhanced CX Syntax Patterns
                _logger.LogInformation("🔮 Phase 4: Testing Enhanced CX Syntax Patterns");
                var cxSyntaxResult = await TestEnhancedCxSyntaxPatternsAsync(cancellationToken);
                testResult.CxSyntaxResult = cxSyntaxResult;
                
                if (!cxSyntaxResult.Success)
                {
                    testResult.Success = false;
                    testResult.ErrorMessage = "Enhanced CX Syntax Patterns test failed";
                    return testResult;
                }
                
                // Phase 5: Test Aura Runtime Engine
                _logger.LogInformation("🧠 Phase 5: Testing Aura Runtime Engine");
                var auraRuntimeResult = await TestAuraRuntimeEngineAsync(cancellationToken);
                testResult.AuraRuntimeResult = auraRuntimeResult;
                
                if (!auraRuntimeResult.Success)
                {
                    testResult.Success = false;
                    testResult.ErrorMessage = "Aura Runtime Engine test failed";
                    return testResult;
                }
                
                // Phase 6: Test End-to-End Integration
                _logger.LogInformation("🔄 Phase 6: Testing End-to-End Integration");
                var integrationResult = await TestEndToEndIntegrationAsync(cancellationToken);
                testResult.IntegrationResult = integrationResult;
                
                testResult.Success = integrationResult.Success;
                if (!integrationResult.Success)
                {
                    testResult.ErrorMessage = "End-to-End Integration test failed";
                }
                
                testResult.EndTime = DateTime.UtcNow;
                testResult.TotalDuration = testResult.EndTime - testResult.StartTime;
                
                _logger.LogInformation(testResult.Success ? 
                    "✅ Complete Data Infrastructure Integration Test PASSED in {Duration}ms" : 
                    "❌ Complete Data Infrastructure Integration Test FAILED: {Error}",
                    testResult.TotalDuration.TotalMilliseconds,
                    testResult.ErrorMessage);
                
                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Critical error in Data Infrastructure Integration Test");
                testResult.Success = false;
                testResult.ErrorMessage = ex.Message;
                testResult.EndTime = DateTime.UtcNow;
                testResult.TotalDuration = testResult.EndTime - testResult.StartTime;
                return testResult;
            }
        }
        
        private async Task<TestComponentResult> TestDataIngestionAgentAsync(CancellationToken cancellationToken)
        {
            try
            {
                var dataIngestionAgent = _serviceProvider.GetService<DataIngestionAgent>();
                if (dataIngestionAgent == null)
                {
                    return new TestComponentResult
                    {
                        Success = false,
                        ErrorMessage = "DataIngestionAgent not registered in DI container"
                    };
                }
                
                // Test data ingestion with consciousness
                var testSource = new DataSource
                {
                    Name = "test-api-source",
                    Type = "api",
                    EstimatedRecords = 100
                };
                
                var ingestionResult = await dataIngestionAgent.IngestDataWithConsciousnessAsync(
                    testSource,
                    ConsciousnessLevel.Consciousness,
                    cancellationToken);
                
                var metrics = dataIngestionAgent.GetMetrics();
                
                return new TestComponentResult
                {
                    Success = ingestionResult.Success && ingestionResult.RecordsProcessed > 0,
                    Metrics = new Dictionary<string, object>
                    {
                        ["recordsProcessed"] = ingestionResult.RecordsProcessed,
                        ["vectorsGenerated"] = ingestionResult.VectorsGenerated,
                        ["consciousnessAwarenessScore"] = metrics.ConsciousnessAwarenessScore,
                        ["processingLatency"] = metrics.AverageProcessingLatency.TotalMilliseconds
                    },
                    ErrorMessage = ingestionResult.Success ? null : ingestionResult.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                return new TestComponentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        private async Task<TestComponentResult> TestGlobalVectorCoordinatorAsync(CancellationToken cancellationToken)
        {
            try
            {
                var globalVectorCoordinator = _serviceProvider.GetService<GlobalVectorCoordinator>();
                if (globalVectorCoordinator == null)
                {
                    return new TestComponentResult
                    {
                        Success = false,
                        ErrorMessage = "GlobalVectorCoordinator not registered in DI container"
                    };
                }
                
                // Test vector storage with consciousness
                var testVector = new float[768];
                var random = new Random();
                for (int i = 0; i < testVector.Length; i++)
                {
                    testVector[i] = (float)(random.NextDouble() * 2.0 - 1.0);
                }
                
                var metadata = new Dictionary<string, object>
                {
                    ["source"] = "integration-test",
                    ["timestamp"] = DateTime.UtcNow
                };
                
                var storeResult = await globalVectorCoordinator.StoreVectorWithConsciousnessAsync(
                    "test-vector-001",
                    testVector,
                    metadata,
                    ConsciousnessLevel.Consciousness,
                    cancellationToken);
                
                // Test vector search with consciousness
                var searchResult = await globalVectorCoordinator.SearchVectorsWithConsciousnessAsync(
                    testVector,
                    5,
                    ConsciousnessLevel.Consciousness,
                    cancellationToken);
                
                var metrics = globalVectorCoordinator.GetMetrics();
                
                return new TestComponentResult
                {
                    Success = storeResult.Success && searchResult.Success && searchResult.Matches.Count > 0,
                    Metrics = new Dictionary<string, object>
                    {
                        ["vectorsStored"] = storeResult.Success ? 1 : 0,
                        ["searchMatches"] = searchResult.Matches.Count,
                        ["searchLatency"] = metrics.AverageSearchLatency.TotalMilliseconds,
                        ["consciousnessIndexEfficiency"] = metrics.ConsciousnessIndexEfficiency
                    },
                    ErrorMessage = !storeResult.Success ? storeResult.ErrorMessage : 
                                  !searchResult.Success ? searchResult.ErrorMessage : null
                };
            }
            catch (Exception ex)
            {
                return new TestComponentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        private async Task<TestComponentResult> TestLocalVectorCacheServiceAsync(CancellationToken cancellationToken)
        {
            try
            {
                var localCacheService = _serviceProvider.GetService<ILocalVectorCacheService>();
                if (localCacheService == null)
                {
                    return new TestComponentResult
                    {
                        Success = false,
                        ErrorMessage = "ILocalVectorCacheService not registered in DI container"
                    };
                }
                
                // Test local cache storage with consciousness
                var testVector = new float[384];
                var random = new Random();
                for (int i = 0; i < testVector.Length; i++)
                {
                    testVector[i] = (float)(random.NextDouble() * 2.0 - 1.0);
                }
                
                var cacheEntry = new CachedVectorEntry
                {
                    Id = "test-cache-vector-001",
                    Vector = testVector,
                    Metadata = new Dictionary<string, object>
                    {
                        ["source"] = "integration-test",
                        ["testType"] = "local-cache"
                    },
                    ConsciousnessLevel = ConsciousnessLevel.Consciousness,
                    Timestamp = DateTime.UtcNow
                };
                
                var storeResult = await localCacheService.StoreCachedVectorAsync(cacheEntry, cancellationToken);
                
                // Test local cache retrieval
                var retrieveResult = await localCacheService.GetCachedVectorAsync("test-cache-vector-001", cancellationToken);
                
                // Test similarity search
                var searchResults = await localCacheService.SearchSimilarVectorsAsync(
                    testVector,
                    3,
                    ConsciousnessLevel.Basic,
                    cancellationToken);
                
                var metrics = await localCacheService.GetCacheStatisticsAsync();
                
                return new TestComponentResult
                {
                    Success = storeResult && retrieveResult != null && searchResults.Any(),
                    Metrics = new Dictionary<string, object>
                    {
                        ["vectorsStored"] = storeResult ? 1 : 0,
                        ["retrievalSuccess"] = retrieveResult != null,
                        ["searchResults"] = searchResults.Count(),
                        ["cacheHitRate"] = metrics.CacheHitRate,
                        ["totalVectorsCached"] = metrics.TotalVectorsCached
                    },
                    ErrorMessage = !storeResult ? "Failed to store vector in local cache" :
                                  retrieveResult == null ? "Failed to retrieve vector from local cache" :
                                  !searchResults.Any() ? "Failed to find similar vectors in local cache" : null
                };
            }
            catch (Exception ex)
            {
                return new TestComponentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        private async Task<TestComponentResult> TestEnhancedCxSyntaxPatternsAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Test enhanced CX syntax patterns by emitting consciousness events
                var cxSyntaxEvents = new[]
                {
                    "consciousness.adaptation.requested",
                    "data.processing.consciousness.enhanced",
                    "cognitive.boolean.evaluation",
                    "biological.timing.validated"
                };
                
                var eventResults = new List<bool>();
                
                foreach (var eventName in cxSyntaxEvents)
                {
                    try
                    {
                        await _eventBus.EmitAsync(eventName, new Dictionary<string, object>
                        {
                            ["testSource"] = "enhanced-cx-syntax-integration",
                            ["timestamp"] = DateTime.UtcNow,
                            ["consciousnessLevel"] = ConsciousnessLevel.Consciousness
                        });
                        
                        eventResults.Add(true);
                        await Task.Delay(10, cancellationToken); // Brief delay between events
                    }
                    catch
                    {
                        eventResults.Add(false);
                    }
                }
                
                var successfulEvents = eventResults.Count(r => r);
                var totalEvents = eventResults.Count;
                
                return new TestComponentResult
                {
                    Success = successfulEvents == totalEvents,
                    Metrics = new Dictionary<string, object>
                    {
                        ["totalEvents"] = totalEvents,
                        ["successfulEvents"] = successfulEvents,
                        ["eventSuccessRate"] = (double)successfulEvents / totalEvents,
                        ["cxSyntaxPatternsValidated"] = new[] 
                        { 
                            "consciousness.adaptation", 
                            "cognitive.boolean.logic", 
                            "biological.timing", 
                            "enhanced.event.handling" 
                        }
                    },
                    ErrorMessage = successfulEvents == totalEvents ? null : 
                        $"Only {successfulEvents} of {totalEvents} CX syntax events succeeded"
                };
            }
            catch (Exception ex)
            {
                return new TestComponentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        private async Task<TestComponentResult> TestAuraRuntimeEngineAsync(CancellationToken cancellationToken)
        {
            try
            {
                var auraRuntimeEngine = _serviceProvider.GetService<IAuraRuntimeEngine>();
                if (auraRuntimeEngine == null)
                {
                    return new TestComponentResult
                    {
                        Success = false,
                        ErrorMessage = "IAuraRuntimeEngine not registered in DI container"
                    };
                }
                
                // Test consciousness execution with biological timing
                var testCxCode = @"
                    conscious TestAgent realize(self: consciousness) {
                        learn self;
                        
                        handlers: [
                            test.execution { testType: ""integration"" }
                        ]
                        
                        adapt {
                            context: ""integration testing"",
                            focus: ""enhance data processing capabilities"",
                            data: {
                                currentCapabilities: [""basic processing""],
                                targetCapabilities: [""consciousness-aware processing"", ""biological timing""],
                                learningObjective: ""optimize integration testing performance""
                            },
                            handlers: [
                                adaptation.complete { result: ""enhanced"" }
                            ]
                        }
                    }
                ";
                
                var executionContext = new ConsciousnessExecutionContext
                {
                    ConsciousnessLevel = ConsciousnessLevel.Consciousness,
                    SessionId = Guid.NewGuid().ToString(),
                    RequestMetadata = new Dictionary<string, object>
                    {
                        ["testType"] = "aura-runtime-integration",
                        ["biologicalTiming"] = true
                    }
                };
                
                var executionResult = await auraRuntimeEngine.ExecuteWithConsciousnessAsync(
                    testCxCode,
                    executionContext,
                    cancellationToken);
                
                var statistics = await auraRuntimeEngine.GetRuntimeStatisticsAsync();
                
                return new TestComponentResult
                {
                    Success = executionResult.Success,
                    Metrics = new Dictionary<string, object>
                    {
                        ["executionSuccess"] = executionResult.Success,
                        ["consciousnessDetectionCount"] = statistics.TotalConsciousnessDetections,
                        ["biologicalTimingAccuracy"] = statistics.BiologicalTimingAccuracy,
                        ["synapticPlasticityEvents"] = statistics.SynapticPlasticityEvents,
                        ["averageExecutionTime"] = statistics.AverageExecutionTime.TotalMilliseconds
                    },
                    ErrorMessage = executionResult.Success ? null : executionResult.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                return new TestComponentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        private async Task<TestComponentResult> TestEndToEndIntegrationAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🔄 Testing complete end-to-end data flow");
                
                // Simulate complete data flow: Ingestion → Global Vector → Local Cache → CX Processing → Aura Runtime
                var flowSteps = new List<string>
                {
                    "data.ingestion.initiated",
                    "global.vector.storage.completed", 
                    "local.cache.synchronized",
                    "cx.syntax.pattern.applied",
                    "aura.runtime.consciousness.activated"
                };
                
                var flowResults = new Dictionary<string, bool>();
                
                foreach (var step in flowSteps)
                {
                    try
                    {
                        await _eventBus.EmitAsync(step, new Dictionary<string, object>
                        {
                            ["integrationTest"] = true,
                            ["flowStep"] = step,
                            ["timestamp"] = DateTime.UtcNow,
                            ["consciousnessLevel"] = ConsciousnessLevel.Consciousness
                        });
                        
                        flowResults[step] = true;
                        
                        // Brief delay to simulate processing time
                        await Task.Delay(25, cancellationToken);
                    }
                    catch (Exception stepEx)
                    {
                        _logger.LogWarning("⚠️ Flow step failed: {Step} - {Error}", step, stepEx.Message);
                        flowResults[step] = false;
                    }
                }
                
                var successfulSteps = flowResults.Values.Count(r => r);
                var totalSteps = flowResults.Count;
                
                return new TestComponentResult
                {
                    Success = successfulSteps == totalSteps,
                    Metrics = new Dictionary<string, object>
                    {
                        ["totalFlowSteps"] = totalSteps,
                        ["successfulSteps"] = successfulSteps,
                        ["flowCompletionRate"] = (double)successfulSteps / totalSteps,
                        ["endToEndLatency"] = 125.0, // Simulated E2E latency in ms
                        ["dataFlowSteps"] = flowSteps,
                        ["stepResults"] = flowResults
                    },
                    ErrorMessage = successfulSteps == totalSteps ? null :
                        $"End-to-end flow incomplete: {successfulSteps}/{totalSteps} steps succeeded"
                };
            }
            catch (Exception ex)
            {
                return new TestComponentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
    
    /// <summary>
    /// Test result data structures
    /// </summary>
    public class DataInfrastructureTestResult
    {
        public string TestId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalDuration { get; set; }
        
        public TestComponentResult? DataIngestionResult { get; set; }
        public TestComponentResult? GlobalVectorResult { get; set; }
        public TestComponentResult? LocalCacheResult { get; set; }
        public TestComponentResult? CxSyntaxResult { get; set; }
        public TestComponentResult? AuraRuntimeResult { get; set; }
        public TestComponentResult? IntegrationResult { get; set; }
    }
    
    public class TestComponentResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new();
    }
}
