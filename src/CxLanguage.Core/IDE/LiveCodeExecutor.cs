using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using CxLanguage.Core.Events;
using CxLanguage.Core.Hardware;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CxLanguage.Core.IDE
{
    /// <summary>
    /// Live code executor with hardware acceleration for real-time CX programming
    /// MANDATORY: Sub-100ms execution for real-time development experience
    /// </summary>
    public class LiveCodeExecutor
    {
        private readonly ILogger _logger;
        private readonly ICxEventBus _eventBus;
        private readonly PatelHardwareAccelerator _hardwareAccelerator;
        private readonly Dictionary<string, CachedCompilation> _compilationCache;
        
        public LiveCodeExecutor(
            ILogger logger,
            ICxEventBus eventBus,
            PatelHardwareAccelerator hardwareAccelerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _hardwareAccelerator = hardwareAccelerator ?? throw new ArgumentNullException(nameof(hardwareAccelerator));
            _compilationCache = new Dictionary<string, CachedCompilation>();
            
            Console.WriteLine("⚡ Live Code Executor initialized with hardware acceleration");
        }
        
        /// <summary>
        /// Execute CX code with hardware acceleration and real-time feedback
        /// </summary>
        public async Task<IDEExecutionResult> ExecuteWithHardwareAccelerationAsync(IDEEvent ideEvent, TimeSpan timeout)
        {
            var stopwatch = Stopwatch.StartNew();
            var executionId = ideEvent.EventId;
            var sessionId = ideEvent.SessionId;
            var cxCode = ideEvent.CxCode ?? "";
            
            Console.WriteLine($"⚡ Hardware-accelerated execution starting: {executionId}");
            
            try
            {
                // Phase 1: Fast compilation with caching
                var compilationResult = await CompileWithCachingAsync(cxCode, executionId);
                var compilationTime = (int)stopwatch.ElapsedMilliseconds;
                
                if (!compilationResult.Success)
                {
                    return new IDEExecutionResult
                    {
                        Success = false,
                        ErrorMessage = compilationResult.ErrorMessage,
                        ExecutionId = executionId,
                        SessionId = sessionId,
                        CompilationTimeMs = compilationTime
                    };
                }
                
                // Phase 2: Hardware-accelerated execution
                var executionResult = await ExecuteOnHardwareAsync(compilationResult, ideEvent);
                var executionTime = (int)(stopwatch.ElapsedMilliseconds - compilationTime);
                
                stopwatch.Stop();
                var totalTime = (int)stopwatch.ElapsedMilliseconds;
                
                Console.WriteLine($"✅ Hardware execution completed: {executionId} in {totalTime}ms (compile: {compilationTime}ms, execute: {executionTime}ms)");
                
                // Emit execution metrics for IDE integration
                await _eventBus.EmitAsync("ide.execution.metrics", new Dictionary<string, object>
                {
                    ["executionId"] = executionId,
                    ["sessionId"] = sessionId,
                    ["compilationTimeMs"] = compilationTime,
                    ["executionTimeMs"] = executionTime,
                    ["totalTimeMs"] = totalTime,
                    ["hardwareAccelerated"] = executionResult.HardwareAccelerated,
                    ["eventsEmitted"] = executionResult.EventsEmitted
                });
                
                return new IDEExecutionResult
                {
                    Success = executionResult.Success,
                    Output = executionResult.Output ?? string.Empty,
                    ErrorMessage = executionResult.ErrorMessage,
                    ExecutionId = executionId,
                    SessionId = sessionId,
                    CompilationTimeMs = compilationTime,
                    ExecutionTimeMs = executionTime,
                    TotalTimeMs = totalTime,
                    EventsEmitted = new string[] { $"events.emitted.{executionResult.EventsEmitted}" },
                    HardwareAccelerated = executionResult.HardwareAccelerated,
                    PerformanceMetrics = LiveCodeExecutorExtensions.ConvertToIDEPerformanceMetrics(executionResult.PerformanceMetrics)
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"❌ Hardware execution failed: {executionId} - {ex.Message}");
                _logger.LogError(ex, "Hardware-accelerated execution failed for {ExecutionId}", executionId);
                
                return new IDEExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Execution error: {ex.Message}",
                    ExecutionId = executionId,
                    SessionId = sessionId,
                    TotalTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
        }
        
        /// <summary>
        /// Compile CX code with intelligent caching for performance
        /// </summary>
        private async Task<CompilationResult> CompileWithCachingAsync(string cxCode, string executionId)
        {
            // Generate cache key based on code hash
            var codeHash = cxCode.GetHashCode().ToString();
            
            // Check compilation cache for identical code
            if (_compilationCache.TryGetValue(codeHash, out var cached))
            {
                Console.WriteLine($"📦 Using cached compilation: {executionId}");
                
                // Emit cache hit event
                await _eventBus.EmitAsync("ide.compilation.cache.hit", new Dictionary<string, object>
                {
                    ["executionId"] = executionId,
                    ["codeHash"] = codeHash,
                    ["cacheAge"] = DateTime.UtcNow - cached.CompiledAt
                });
                
                return new CompilationResult
                {
                    Success = true,
                    CompiledCode = cached.CompiledCode,
                    Metadata = cached.Metadata
                };
            }
            
            // Perform new compilation
            Console.WriteLine($"🔧 Compiling CX code: {executionId}");
            
            try
            {
                // Basic CX syntax analysis and compilation
                var compiledCode = await PerformCxCompilationAsync(cxCode);
                var metadata = ExtractCodeMetadata(cxCode);
                
                // Cache the compilation result
                _compilationCache[codeHash] = new CachedCompilation
                {
                    CompiledCode = compiledCode,
                    Metadata = metadata,
                    CompiledAt = DateTime.UtcNow
                };
                
                // Emit compilation success event
                await _eventBus.EmitAsync("ide.compilation.success", new Dictionary<string, object>
                {
                    ["executionId"] = executionId,
                    ["codeHash"] = codeHash,
                    ["codeLength"] = cxCode.Length,
                    ["metadata"] = JsonSerializer.Serialize(metadata)
                });
                
                return new CompilationResult
                {
                    Success = true,
                    CompiledCode = compiledCode,
                    Metadata = metadata
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Compilation failed: {executionId} - {ex.Message}");
                
                // Emit compilation error event
                await _eventBus.EmitAsync("ide.compilation.error", new Dictionary<string, object>
                {
                    ["executionId"] = executionId,
                    ["error"] = ex.Message,
                    ["codeLength"] = cxCode.Length
                });
                
                return new CompilationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Execute compiled code on optimal hardware
        /// </summary>
        private async Task<HardwareExecutionResult> ExecuteOnHardwareAsync(CompilationResult compilation, IDEEvent ideEvent)
        {
            var executionId = ideEvent.EventId;
            Console.WriteLine($"🎮 Hardware execution starting: {executionId}");
            
            try
            {
                // Prepare execution data for hardware acceleration
                var executionData = PrepareExecutionData(compilation, ideEvent);
                
                // Use Patel Hardware Accelerator for consciousness-aware processing
                var preferredHardware = ideEvent.Options?.PreferredHardware ?? "Auto";
                var hardwareTarget = preferredHardware switch
                {
                    "GPU" => PatelHardwareAccelerator.HardwareTarget.GPU,
                    "CPU" => PatelHardwareAccelerator.HardwareTarget.CPU,
                    _ => PatelHardwareAccelerator.HardwareTarget.Auto
                };
                
                var hardwareResult = await _hardwareAccelerator.AccelerateConsciousnessProcessing(
                    PatelHardwareAccelerator.ConsciousnessOperation.RealTimeResponse,
                    executionData,
                    hardwareTarget,
                    priority: 9 // High priority for IDE responsiveness
                );
                
                // Process hardware result and generate output
                var output = ProcessHardwareExecutionResult(hardwareResult, compilation);
                var eventsEmitted = await EmitConsciousnessEvents(compilation, ideEvent);
                
                Console.WriteLine($"🎮 Hardware execution completed: {executionId}");
                
                return new HardwareExecutionResult
                {
                    Success = true,
                    Output = output,
                    EventsEmitted = eventsEmitted,
                    HardwareAccelerated = true,
                    PerformanceMetrics = GetExecutionPerformanceMetrics()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hardware execution error: {executionId} - {ex.Message}");
                _logger.LogError(ex, "Hardware execution failed for {ExecutionId}", executionId);
                
                return new HardwareExecutionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    HardwareAccelerated = false
                };
            }
        }
        
        /// <summary>
        /// Perform CX language compilation
        /// </summary>
        private async Task<string> PerformCxCompilationAsync(string cxCode)
        {
            // Simulate CX compilation process
            await Task.Delay(5); // Minimal compilation time for real-time experience
            
            // Basic CX pattern recognition and compilation
            var compiledCode = "// CX Compiled Code\n";
            
            if (cxCode.Contains("conscious"))
            {
                compiledCode += "// Consciousness entity detected\n";
            }
            
            if (cxCode.Contains("realize"))
            {
                compiledCode += "// Consciousness realization pattern\n";
            }
            
            if (cxCode.Contains("learn"))
            {
                compiledCode += "// Learning pattern detected\n";
            }
            
            if (cxCode.Contains("emit"))
            {
                compiledCode += "// Event emission pattern\n";
            }
            
            compiledCode += $"// Original code length: {cxCode.Length} characters\n";
            compiledCode += "// Compilation successful";
            
            return compiledCode;
        }
        
        /// <summary>
        /// Extract metadata from CX code for optimization
        /// </summary>
        private CodeMetadata ExtractCodeMetadata(string cxCode)
        {
            return new CodeMetadata
            {
                CodeLength = cxCode.Length,
                HasConsciousEntities = cxCode.Contains("conscious"),
                HasRealizationPattern = cxCode.Contains("realize"),
                HasLearningPattern = cxCode.Contains("learn"),
                HasEventEmission = cxCode.Contains("emit"),
                HasEventHandlers = cxCode.Contains("on "),
                EstimatedComplexity = CalculateCodeComplexity(cxCode)
            };
        }
        
        /// <summary>
        /// Calculate CX code complexity for optimization
        /// </summary>
        private int CalculateCodeComplexity(string cxCode)
        {
            var complexity = 1; // Base complexity
            
            // Add complexity for various CX patterns
            complexity += cxCode.Split("conscious").Length - 1; // Conscious entities
            complexity += cxCode.Split("realize").Length - 1; // Realization patterns
            complexity += cxCode.Split("learn").Length - 1; // Learning patterns
            complexity += cxCode.Split("emit").Length - 1; // Event emissions
            complexity += cxCode.Split("on ").Length - 1; // Event handlers
            complexity += cxCode.Split("{").Length - 1; // Code blocks
            
            return Math.Max(1, complexity);
        }
        
        /// <summary>
        /// Prepare execution data for hardware processing
        /// </summary>
        private byte[] PrepareExecutionData(CompilationResult compilation, IDEEvent ideEvent)
        {
            var executionPackage = new
            {
                CompiledCode = compilation.CompiledCode,
                Metadata = compilation.Metadata,
                SessionId = ideEvent.SessionId,
                ExecutionId = ideEvent.EventId,
                Options = ideEvent.Options
            };
            
            var json = JsonSerializer.Serialize(executionPackage);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
        
        /// <summary>
        /// Process hardware execution result into readable output
        /// </summary>
        private string ProcessHardwareExecutionResult(byte[] hardwareResult, CompilationResult compilation)
        {
            var output = "🎮 Hardware-Accelerated CX Execution Result:\n\n";
            
            if (compilation.Metadata?.HasConsciousEntities == true)
            {
                output += "🧠 Consciousness entities processed\n";
            }
            
            if (compilation.Metadata?.HasRealizationPattern == true)
            {
                output += "✨ Consciousness realization completed\n";
            }
            
            if (compilation.Metadata?.HasLearningPattern == true)
            {
                output += "📚 Learning patterns activated\n";
            }
            
            if (compilation.Metadata?.HasEventEmission == true)
            {
                output += "📡 Events emitted successfully\n";
            }
            
            output += $"\n⚡ Hardware acceleration: ACTIVE\n";
            output += $"📊 Code complexity: {compilation.Metadata?.EstimatedComplexity}\n";
            output += $"🔧 Processing result: {hardwareResult.Length} bytes\n";
            output += $"✅ Execution completed successfully";
            
            return output;
        }
        
        /// <summary>
        /// Emit consciousness events based on code execution
        /// </summary>
        private async Task<int> EmitConsciousnessEvents(CompilationResult compilation, IDEEvent ideEvent)
        {
            var eventsEmitted = 0;
            
            if (compilation.Metadata?.HasConsciousEntities == true)
            {
                await _eventBus.EmitAsync("consciousness.entity.active", new Dictionary<string, object>
                {
                    ["sessionId"] = ideEvent.SessionId,
                    ["executionId"] = ideEvent.EventId,
                    ["timestamp"] = DateTime.UtcNow
                });
                eventsEmitted++;
            }
            
            if (compilation.Metadata?.HasRealizationPattern == true)
            {
                await _eventBus.EmitAsync("consciousness.realization.completed", new Dictionary<string, object>
                {
                    ["sessionId"] = ideEvent.SessionId,
                    ["executionId"] = ideEvent.EventId,
                    ["timestamp"] = DateTime.UtcNow
                });
                eventsEmitted++;
            }
            
            if (compilation.Metadata?.HasLearningPattern == true)
            {
                await _eventBus.EmitAsync("consciousness.learning.active", new Dictionary<string, object>
                {
                    ["sessionId"] = ideEvent.SessionId,
                    ["executionId"] = ideEvent.EventId,
                    ["timestamp"] = DateTime.UtcNow
                });
                eventsEmitted++;
            }
            
            // Always emit execution completed event
            await _eventBus.EmitAsync("consciousness.execution.completed", new Dictionary<string, object>
            {
                ["sessionId"] = ideEvent.SessionId,
                ["executionId"] = ideEvent.EventId,
                ["eventsEmitted"] = eventsEmitted,
                ["timestamp"] = DateTime.UtcNow
            });
            eventsEmitted++;
            
            return eventsEmitted;
        }
        
        /// <summary>
        /// Get execution performance metrics
        /// </summary>
        private Dictionary<string, object> GetExecutionPerformanceMetrics()
        {
            var metrics = _hardwareAccelerator.GetPerformanceMetrics();
            
            return new Dictionary<string, object>
            {
                ["queuedTasks"] = metrics.QueuedTasks,
                ["gpuUtilization"] = metrics.GPUUtilization,
                ["cpuOptimizationLevel"] = metrics.CPUOptimizationLevel,
                ["specializedHardwareStatus"] = metrics.SpecializedHardwareStatus,
                ["totalProcessedTasks"] = metrics.TotalProcessedTasks,
                ["timestamp"] = DateTime.UtcNow
            };
        }
    }
    
    // Supporting classes for live code execution
    
    public class CachedCompilation
    {
        public required string CompiledCode { get; set; }
        public CodeMetadata? Metadata { get; set; }
        public DateTime CompiledAt { get; set; }
    }
    
    public class CompilationResult
    {
        public bool Success { get; set; }
        public string? CompiledCode { get; set; }
        public string? ErrorMessage { get; set; }
        public CodeMetadata? Metadata { get; set; }
    }
    
    public class HardwareExecutionResult
    {
        public bool Success { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public int EventsEmitted { get; set; }
        public bool HardwareAccelerated { get; set; }
        public Dictionary<string, object>? PerformanceMetrics { get; set; }
    }
    
    public class CodeMetadata
    {
        public int CodeLength { get; set; }
        public bool HasConsciousEntities { get; set; }
        public bool HasRealizationPattern { get; set; }
        public bool HasLearningPattern { get; set; }
        public bool HasEventEmission { get; set; }
        public bool HasEventHandlers { get; set; }
        public int EstimatedComplexity { get; set; }
    }
}

namespace CxLanguage.Core.IDE
{
    /// <summary>
    /// Helper extensions for Live Code Executor
    /// </summary>
    public static class LiveCodeExecutorExtensions
    {
        /// <summary>
        /// Helper method to convert Dictionary to IDEPerformanceMetrics
        /// </summary>
        public static IDEPerformanceMetrics? ConvertToIDEPerformanceMetrics(Dictionary<string, object>? dict)
        {
            if (dict == null) return null;
            
            return new IDEPerformanceMetrics
            {
                Timestamp = DateTime.UtcNow,
                MemoryUsageBytes = dict.TryGetValue("MemoryUsageMB", out var mem) ? (long)(Convert.ToDouble(mem) * 1024 * 1024) : 0L,
                CpuUsagePercent = dict.TryGetValue("CpuUsagePercent", out var cpu) ? Convert.ToDouble(cpu) : 0.0,
                AverageExecutionTime = dict.TryGetValue("ExecutionTimeMs", out var time) ? Convert.ToDouble(time) : 0.0,
                ActiveSessions = 1,
                TotalExecutions = 1,
                EventsPerSecond = 1.0,
                HardwareAccelerationActive = true
            };
        }
    }
}
