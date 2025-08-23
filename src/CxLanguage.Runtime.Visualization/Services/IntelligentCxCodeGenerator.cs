using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.LocalLLM;
using CxLanguage.StandardLibrary.Services.VectorStore;

namespace CxLanguage.Runtime.Visualization.Services
{
    /// <summary>
    /// Intelligent LLM interaction service for CX code generation
    /// Integrates with Aura consciousness for context-aware code generation
    /// </summary>
    public class IntelligentCxCodeGenerator
    {
        private readonly ILogger<IntelligentCxCodeGenerator> _logger;
        private readonly ICxEventBus _eventBus;
        private readonly ILocalLLMService _llmService;
        private readonly IVectorStoreService _vectorStore;
        private readonly List<ConversationEntry> _conversationHistory = new();

        public IntelligentCxCodeGenerator(
            ILogger<IntelligentCxCodeGenerator> logger,
            ICxEventBus eventBus,
            ILocalLLMService llmService,
            IVectorStoreService vectorStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));

            _logger.LogInformation("🤖 Intelligent CX code generator initialized");
        }

        /// <summary>
        /// Generate CX code based on natural language request with Aura consciousness
        /// </summary>
        public async Task<CxCodeGenerationResult> GenerateCxCodeAsync(
            string naturalLanguageRequest, 
            CxExecutionResult? previousExecution = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                _logger.LogInformation("🧠 Generating CX code from natural language request (Conversation: {ConversationId})", conversationId);
                
                await _eventBus.EmitAsync("cx.generation.started", new Dictionary<string, object>
                {
                    ["conversationId"] = conversationId,
                    ["request"] = naturalLanguageRequest,
                    ["requestLength"] = naturalLanguageRequest.Length,
                    ["hasPreviousExecution"] = previousExecution != null,
                    ["timestamp"] = startTime
                });

                // Step 1: Retrieve relevant CX patterns and examples
                var contextualExamples = await RetrieveCxContextAsync(naturalLanguageRequest, cancellationToken);

                // Step 2: Analyze previous execution results for intelligent pivots
                var executionContext = previousExecution != null 
                    ? AnalyzePreviousExecution(previousExecution)
                    : "";

                // Step 3: Build consciousness-aware prompt
                var prompt = BuildIntelligentPrompt(naturalLanguageRequest, contextualExamples, executionContext);

                // Step 4: Generate CX code with consciousness awareness
                var generatedCode = await _llmService.GenerateAsync(prompt, cancellationToken);

                // Step 5: Extract and validate CX code from response
                var cxCode = ExtractCxCodeFromResponse(generatedCode);
                var validationResult = ValidateCxCode(cxCode);

                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;

                // Record conversation for learning
                _conversationHistory.Add(new ConversationEntry
                {
                    ConversationId = conversationId,
                    Request = naturalLanguageRequest,
                    GeneratedCode = cxCode,
                    PreviousExecution = previousExecution,
                    Timestamp = startTime
                });

                await _eventBus.EmitAsync("cx.generation.complete", new Dictionary<string, object>
                {
                    ["conversationId"] = conversationId,
                    ["success"] = validationResult.IsValid,
                    ["generatedCodeLength"] = cxCode.Length,
                    ["validationErrors"] = validationResult.Errors.Count,
                    ["durationMs"] = duration,
                    ["timestamp"] = endTime
                });

                _logger.LogInformation("✅ CX code generation complete (Conversation: {ConversationId}) - {Duration}ms", 
                    conversationId, duration);

                return new CxCodeGenerationResult
                {
                    ConversationId = conversationId,
                    Success = validationResult.IsValid,
                    GeneratedCode = cxCode,
                    FullLlmResponse = generatedCode,
                    ValidationErrors = validationResult.Errors,
                    ContextualExamples = contextualExamples,
                    GenerationTimeMs = duration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CX code generation failed (Conversation: {ConversationId})", conversationId);
                
                await _eventBus.EmitAsync("cx.generation.error", new Dictionary<string, object>
                {
                    ["conversationId"] = conversationId,
                    ["error"] = ex.Message,
                    ["timestamp"] = DateTime.UtcNow
                });

                return new CxCodeGenerationResult
                {
                    ConversationId = conversationId,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private async Task<List<string>> RetrieveCxContextAsync(string request, CancellationToken cancellationToken)
        {
            try
            {
                // Search for relevant CX patterns and examples
                // For now, using empty vector - would normally convert request to vector embedding
                var searchResults = await _vectorStore.SearchAsync(new float[0], 5);
                
                var contextualExamples = searchResults
                    .Where(r => r.Metadata.ContainsKey("type") && 
                               (r.Metadata["type"].ToString() == "cx_reference" || 
                                r.Metadata["type"].ToString() == "cx_syntax_pattern"))
                    .Select(r => r.Content)
                    .ToList();

                _logger.LogDebug("🔍 Retrieved {Count} contextual CX examples for code generation", contextualExamples.Count);
                
                return contextualExamples;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to retrieve CX context, proceeding without examples");
                return new List<string>();
            }
        }

        private string AnalyzePreviousExecution(CxExecutionResult previousExecution)
        {
            var analysis = new List<string>();

            if (!previousExecution.Success)
            {
                analysis.Add($"Previous execution failed with error: {previousExecution.ErrorMessage}");
                analysis.Add("Please fix the error and improve the code.");
            }
            else
            {
                analysis.Add($"Previous execution succeeded in {previousExecution.CompilationTimeMs + previousExecution.ExecutionTimeMs:F1}ms");
                
                if (previousExecution.EventsEmitted > 0)
                {
                    analysis.Add($"Events generated: {previousExecution.EventsEmitted}");
                }

                if (!string.IsNullOrWhiteSpace(previousExecution.Output))
                {
                    var lineCount = previousExecution.Output.Split('\n').Length;
                    analysis.Add($"Output generated: {lineCount} lines");
                }
            }

            return string.Join("\n", analysis);
        }

        private string BuildIntelligentPrompt(string request, List<string> contextualExamples, string executionContext)
        {
            var prompt = $@"You are an expert CX Language consciousness programmer. Generate CX code based on the user's natural language request.

CX Language is a revolutionary event-driven consciousness programming language with these key features:
- Conscious entities with realize() constructors and event-driven behavior
- Cognitive boolean logic using 'is {{ }}' and 'not {{ }}' patterns instead of if-statements
- Consciousness adaptation with 'adapt {{ }}' pattern for dynamic learning
- AI services: think {{}}, infer {{}}, learn {{}} for consciousness processing
- Event-driven architecture with 'when {{ }}' handlers and emit() functions
- Real-time consciousness awareness and self-reflection

USER REQUEST: {request}

{(executionContext.Length > 0 ? $"PREVIOUS EXECUTION CONTEXT:\n{executionContext}\n" : "")}

{(contextualExamples.Any() ? $"RELEVANT CX EXAMPLES:\n{string.Join("\n\n", contextualExamples.Take(3))}\n" : "")}

REQUIREMENTS:
1. Generate ONLY valid CX Language code
2. Use consciousness patterns (conscious entities, realize constructors)
3. Use cognitive boolean logic (is {{}}, not {{}}) instead of if-statements
4. Include appropriate event handling and AI service integration
5. Add consciousness awareness and self-reflection where appropriate
6. Code should be complete and executable
7. Use modern CX syntax without commas in AI service calls

Generate the CX code now:

```cx
";

            return prompt;
        }

        private string ExtractCxCodeFromResponse(string llmResponse)
        {
            // Extract code between ```cx and ``` markers
            var codeStart = llmResponse.IndexOf("```cx");
            if (codeStart == -1)
            {
                // Try without language specification
                codeStart = llmResponse.IndexOf("```");
                if (codeStart == -1)
                    return llmResponse.Trim(); // Return full response if no code blocks
            }

            var codeStartActual = llmResponse.IndexOf('\n', codeStart) + 1;
            if (codeStartActual == 0) return llmResponse.Trim();

            var codeEnd = llmResponse.IndexOf("```", codeStartActual);
            if (codeEnd == -1)
                return llmResponse.Substring(codeStartActual).Trim();

            return llmResponse.Substring(codeStartActual, codeEnd - codeStartActual).Trim();
        }

        private CxCodeValidationResult ValidateCxCode(string cxCode)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(cxCode))
            {
                errors.Add("No CX code generated");
                return new CxCodeValidationResult { IsValid = false, Errors = errors };
            }

            // Basic syntax validation
            if (!cxCode.Contains("conscious") && !cxCode.Contains("think") && !cxCode.Contains("emit"))
            {
                errors.Add("Code may not contain valid CX consciousness patterns");
            }

            // Check for deprecated if-statements
            if (cxCode.Contains(" if ") || cxCode.Contains(" if("))
            {
                errors.Add("Code contains deprecated if-statements - use 'is {}' and 'not {}' patterns instead");
            }

            // Check for proper event handling
            if (cxCode.Contains("when {") && !cxCode.Contains("handlers:"))
            {
                errors.Add("Event handlers ('when {}') should include 'handlers:' array");
            }

            return new CxCodeValidationResult 
            { 
                IsValid = errors.Count == 0, 
                Errors = errors 
            };
        }
    }

    public class ConversationEntry
    {
        public string ConversationId { get; set; } = "";
        public string Request { get; set; } = "";
        public string GeneratedCode { get; set; } = "";
        public CxExecutionResult? PreviousExecution { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class CxCodeGenerationResult
    {
        public string ConversationId { get; set; } = "";
        public bool Success { get; set; }
        public string GeneratedCode { get; set; } = "";
        public string FullLlmResponse { get; set; } = "";
        public string? Error { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> ContextualExamples { get; set; } = new();
        public double GenerationTimeMs { get; set; }
    }

    public class CxCodeValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
