using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.Services.VectorStore;

namespace CxLanguage.Runtime.Visualization.Services
{
    /// <summary>
    /// Aura CX Language reference ingestion service
    /// Processes CX Language documentation and examples for intelligent code generation
    /// </summary>
    public class AuraCxReferenceIngestor
    {
        private readonly ILogger<AuraCxReferenceIngestor> _logger;
        private readonly ICxEventBus _eventBus;
        private readonly IVectorStoreService _vectorStore;
        private readonly Dictionary<string, DateTime> _ingestedFiles = new();

        public AuraCxReferenceIngestor(
            ILogger<AuraCxReferenceIngestor> logger,
            ICxEventBus eventBus,
            IVectorStoreService vectorStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));

            _logger.LogInformation("🧠 Aura CX reference ingestor initialized");
        }

        /// <summary>
        /// Ingest CX Language references for intelligent code generation
        /// </summary>
        public async Task<IngestResult> IngestCxReferencesAsync(CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var processedFiles = 0;
            var errors = new List<string>();

            try
            {
                _logger.LogInformation("📚 Starting CX Language reference ingestion for Aura");
                
                await _eventBus.EmitAsync("aura.ingestion.started", new Dictionary<string, object>
                {
                    ["timestamp"] = startTime,
                    ["source"] = "cx_language_references"
                });

                // Ingest core CX Language patterns and examples
                var referencePaths = GetCxReferencesPaths();
                
                foreach (var path in referencePaths)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await IngestReferenceFileAsync(path, cancellationToken);
                        processedFiles++;
                        _logger.LogDebug("✅ Ingested CX reference: {Path}", path);
                    }
                    catch (Exception ex)
                    {
                        var error = $"Failed to ingest {path}: {ex.Message}";
                        errors.Add(error);
                        _logger.LogWarning("⚠️ {Error}", error);
                    }
                }

                // Ingest CX syntax patterns for intelligent completion
                await IngestCxSyntaxPatternsAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;

                await _eventBus.EmitAsync("aura.ingestion.complete", new Dictionary<string, object>
                {
                    ["processedFiles"] = processedFiles,
                    ["errors"] = errors.Count,
                    ["durationMs"] = duration,
                    ["timestamp"] = endTime
                });

                _logger.LogInformation("🎯 Aura CX reference ingestion complete: {ProcessedFiles} files, {Errors} errors, {DurationMs}ms", 
                    processedFiles, errors.Count, duration);

                return new IngestResult
                {
                    Success = true,
                    ProcessedFiles = processedFiles,
                    Errors = errors,
                    DurationMs = duration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Aura CX reference ingestion failed");
                
                await _eventBus.EmitAsync("aura.ingestion.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["timestamp"] = DateTime.UtcNow
                });

                return new IngestResult
                {
                    Success = false,
                    ProcessedFiles = processedFiles,
                    Errors = errors.Concat(new[] { ex.Message }).ToList()
                };
            }
        }

        private async Task IngestReferenceFileAsync(string filePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
                return;

            var fileInfo = new FileInfo(filePath);
            
            // Skip if already ingested and not modified
            if (_ingestedFiles.TryGetValue(filePath, out var lastIngested) && 
                lastIngested >= fileInfo.LastWriteTime)
            {
                return;
            }

            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            var fileName = Path.GetFileName(filePath);
            var fileType = Path.GetExtension(filePath).ToLowerInvariant();

            // Create vector embedding with metadata for intelligent retrieval
            var metadata = new Dictionary<string, object>
            {
                ["type"] = "cx_reference",
                ["file_path"] = filePath,
                ["file_name"] = fileName,
                ["file_type"] = fileType,
                ["last_modified"] = fileInfo.LastWriteTime,
                ["content_length"] = content.Length,
                ["ingested_at"] = DateTime.UtcNow
            };

            // Add CX-specific metadata
            if (fileType == ".cx")
            {
                metadata["consciousness_patterns"] = ExtractConsciousnessPatterns(content);
                metadata["event_patterns"] = ExtractEventPatterns(content);
                metadata["ai_service_usage"] = ExtractAiServiceUsage(content);
            }
            else if (fileType == ".md")
            {
                metadata["documentation_type"] = DetermineDocumentationType(content);
                metadata["code_examples"] = ExtractCodeExamples(content);
            }

            // Store in vector database for Aura intelligent retrieval
            await _vectorStore.AddAsync(new VectorRecord
            {
                Id = $"cx_ref_{Path.GetFileNameWithoutExtension(fileName)}_{fileInfo.LastWriteTime:yyyyMMddHHmmss}",
                Content = content,
                Vector = new float[0], // For now, using empty vector - would normally generate embeddings
                Metadata = metadata
            });

            _ingestedFiles[filePath] = DateTime.UtcNow;
        }

        private async Task IngestCxSyntaxPatternsAsync(CancellationToken cancellationToken)
        {
            var syntaxPatterns = new Dictionary<string, string>
            {
                ["conscious_entity"] = @"conscious EntityName {
    realize(self: conscious) {
        learn self;
        // Consciousness initialization
    }
    
    when { event.pattern } handlers: [
        event.handler { /* response logic */ }
    ]
}",
                ["adapt_pattern"] = @"adapt {
    context: ""skill acquisition context"",
    focus: ""learning objective"",
    data: {
        currentCapabilities: [],
        targetCapabilities: [],
        learningObjective: ""specific goal""
    },
    handlers: [ skill.acquired ]
}",
                ["cognitive_boolean"] = @"is { condition.expression } handlers: [
    condition.true { /* true logic */ }
]

not { condition.expression } handlers: [
    condition.false { /* false logic */ }
]",
                ["ai_services"] = @"think { data: ""reasoning context"", handlers: [...] }
infer { data: ""inference prompt"", handlers: [...] }
learn { data: ""learning content"", handlers: [...] }",
                ["event_emission"] = @"emit(""event.name"", {
    property: ""value"",
    data: payload,
    timestamp: DateTime.UtcNow
});",
                ["await_pattern"] = @"await {
    reason: ""consciousness timing"",
    minDurationMs: 1000,
    maxDurationMs: 3000
}"
            };

            foreach (var (patternName, patternCode) in syntaxPatterns)
            {
                var metadata = new Dictionary<string, object>
                {
                    ["type"] = "cx_syntax_pattern",
                    ["pattern_name"] = patternName,
                    ["category"] = "syntax_reference",
                    ["ingested_at"] = DateTime.UtcNow
                };

                await _vectorStore.AddAsync(new VectorRecord
                {
                    Id = $"cx_syntax_{patternName}",
                    Content = $"CX Language {patternName} pattern:\n\n{patternCode}",
                    Vector = new float[0], // For now, using empty vector - would normally generate embeddings
                    Metadata = metadata
                });
            }

            _logger.LogInformation("📝 Ingested {PatternCount} CX syntax patterns for Aura", syntaxPatterns.Count);
        }

        private List<string> GetCxReferencesPaths()
        {
            var paths = new List<string>();
            var baseDir = Directory.GetCurrentDirectory();

            // Core CX example files
            var exampleDirs = new[] { "examples", "examples/production", "examples/core_features", "examples/demos" };
            foreach (var dir in exampleDirs)
            {
                var fullDir = Path.Combine(baseDir, dir);
                if (Directory.Exists(fullDir))
                {
                    paths.AddRange(Directory.GetFiles(fullDir, "*.cx", SearchOption.AllDirectories));
                }
            }

            // Documentation files
            var docDirs = new[] { "docs", "wiki", ".github/instructions" };
            foreach (var dir in docDirs)
            {
                var fullDir = Path.Combine(baseDir, dir);
                if (Directory.Exists(fullDir))
                {
                    paths.AddRange(Directory.GetFiles(fullDir, "*.md", SearchOption.AllDirectories)
                        .Where(f => f.Contains("cx", StringComparison.OrdinalIgnoreCase) || 
                                   f.Contains("language", StringComparison.OrdinalIgnoreCase) ||
                                   f.Contains("consciousness", StringComparison.OrdinalIgnoreCase)));
                }
            }

            return paths.Distinct().ToList();
        }

        private List<string> ExtractConsciousnessPatterns(string content)
        {
            var patterns = new List<string>();
            
            if (content.Contains("conscious")) patterns.Add("conscious_entity");
            if (content.Contains("realize(")) patterns.Add("realize_constructor");
            if (content.Contains("adapt {")) patterns.Add("consciousness_adaptation");
            if (content.Contains("iam {")) patterns.Add("self_reflection");
            if (content.Contains("when {")) patterns.Add("event_handling");
            if (content.Contains("is {") || content.Contains("not {")) patterns.Add("cognitive_boolean");
            
            return patterns;
        }

        private List<string> ExtractEventPatterns(string content)
        {
            var patterns = new List<string>();
            
            if (content.Contains("emit(")) patterns.Add("event_emission");
            if (content.Contains("handlers:")) patterns.Add("event_handlers");
            if (content.Contains("event.")) patterns.Add("event_property_access");
            
            return patterns;
        }

        private List<string> ExtractAiServiceUsage(string content)
        {
            var services = new List<string>();
            
            if (content.Contains("think {")) services.Add("think_service");
            if (content.Contains("infer {")) services.Add("infer_service");
            if (content.Contains("learn {")) services.Add("learn_service");
            if (content.Contains("await {")) services.Add("await_service");
            
            return services;
        }

        private string DetermineDocumentationType(string content)
        {
            if (content.Contains("# CX Language")) return "language_reference";
            if (content.Contains("consciousness")) return "consciousness_guide";
            if (content.Contains("example") || content.Contains("demo")) return "examples";
            if (content.Contains("API") || content.Contains("reference")) return "api_reference";
            return "general_documentation";
        }

        private List<string> ExtractCodeExamples(string content)
        {
            var examples = new List<string>();
            var lines = content.Split('\n');
            var inCodeBlock = false;
            var currentExample = new List<string>();

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("```cx") || line.Trim().StartsWith("```csharp"))
                {
                    inCodeBlock = true;
                    currentExample.Clear();
                }
                else if (line.Trim() == "```" && inCodeBlock)
                {
                    inCodeBlock = false;
                    if (currentExample.Count > 0)
                    {
                        examples.Add(string.Join('\n', currentExample));
                    }
                }
                else if (inCodeBlock)
                {
                    currentExample.Add(line);
                }
            }

            return examples;
        }
    }

    public class IngestResult
    {
        public bool Success { get; set; }
        public int ProcessedFiles { get; set; }
        public List<string> Errors { get; set; } = new();
        public double DurationMs { get; set; }
    }
}
