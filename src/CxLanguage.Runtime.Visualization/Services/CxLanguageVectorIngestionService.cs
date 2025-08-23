using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.Services.VectorStore;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Runtime.Visualization.Services;

/// <summary>
/// 🧠 CORE ENGINEERING TEAM - CX Language Vector Ingestion Service
/// Dr. Marcus "MemoryLayer" Sterling - Vector Index Architect
/// 
/// Ingests CX Language reference documentation into global vector database
/// on runtime start for consciousness-aware programming assistance.
/// </summary>
public class CxLanguageVectorIngestionService
{
    private readonly ILogger<CxLanguageVectorIngestionService> _logger;
    private readonly ICxEventBus _eventBus;
    private readonly IVectorStoreService? _vectorStore;

    // CX Language reference files to ingest
    private readonly List<string> _cxReferenceFiles = new()
    {
        "docs/CX_LANGUAGE_COMPREHENSIVE_REFERENCE.md",
        "docs/CX_LANGUAGE_QUICK_REFERENCE.md", 
        "docs/CX_LANGUAGE_INTERNAL_EVENTS_REFERENCE.md",
        ".github/copilot-instructions.md",
        ".github/instructions/cx.instructions.md",
        "src/CxLanguage.StandardLibrary/README.md",
        "wiki/CX_LANGUAGE_V1_0_COMPLETE_GUIDE.md",
        "examples/README.md"
    };

    public CxLanguageVectorIngestionService(
        ILogger<CxLanguageVectorIngestionService> logger,
        ICxEventBus eventBus,
        IVectorStoreService? vectorStore = null)
    {
        _logger = logger;
        _eventBus = eventBus;
        _vectorStore = vectorStore;
    }

    /// <summary>
    /// Ingests CX Language documentation into vector database for consciousness-aware assistance
    /// </summary>
    public async Task IngestCxLanguageReferencesAsync()
    {
        try
        {
            _logger.LogInformation("🧠 Starting CX Language reference ingestion into global vector database");
            
            // Emit ingestion start event
            await _eventBus.EmitAsync("cx.language.ingestion.start", new Dictionary<string, object>
            {
                ["service"] = nameof(CxLanguageVectorIngestionService),
                ["fileCount"] = _cxReferenceFiles.Count,
                ["timestamp"] = DateTime.Now.ToString("HH:mm:ss.fff")
            });

            int successCount = 0;
            int errorCount = 0;

            // Find workspace root directory
            var workspaceRoot = FindWorkspaceRoot();
            if (workspaceRoot == null)
            {
                _logger.LogWarning("⚠️ Could not find workspace root directory for CX Language ingestion");
                return;
            }

            _logger.LogInformation("📂 Workspace root: {WorkspaceRoot}", workspaceRoot);

            foreach (var relativePath in _cxReferenceFiles)
            {
                try
                {
                    var fullPath = Path.Combine(workspaceRoot, relativePath);
                    
                    if (!File.Exists(fullPath))
                    {
                        _logger.LogWarning("📄 File not found: {FilePath}", fullPath);
                        errorCount++;
                        continue;
                    }

                    _logger.LogInformation("📖 Ingesting: {RelativePath}", relativePath);
                    
                    // Read file content
                    var content = await File.ReadAllTextAsync(fullPath);
                    
                    // Skip empty files
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        _logger.LogWarning("📄 Skipping empty file: {RelativePath}", relativePath);
                        continue;
                    }

                    // Create chunks for large documents (break into 2000-character chunks with 200 character overlap)
                    var chunks = CreateDocumentChunks(content, 2000, 200);
                    
                    int chunkIndex = 0;
                    foreach (var chunk in chunks)
                    {
                        if (_vectorStore != null)
                        {
                            var vectorRecord = new VectorRecord
                            {
                                Id = $"cx_lang_{Path.GetFileNameWithoutExtension(relativePath)}_{chunkIndex:000}",
                                Vector = await GenerateSimpleEmbedding(chunk),
                                Metadata = new Dictionary<string, object>
                                {
                                    ["source"] = "cx_language_reference",
                                    ["file_path"] = relativePath,
                                    ["file_name"] = Path.GetFileName(relativePath),
                                    ["chunk_index"] = chunkIndex,
                                    ["chunk_count"] = chunks.Count,
                                    ["ingestion_timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                                    ["content_type"] = GetContentType(relativePath),
                                    ["content_preview"] = chunk.Length > 100 ? chunk.Substring(0, 100) + "..." : chunk
                                }
                            };

                            await _vectorStore.AddAsync(vectorRecord);
                            _logger.LogDebug("📦 Added chunk {ChunkIndex}/{ChunkCount} from {File}", 
                                chunkIndex + 1, chunks.Count, Path.GetFileName(relativePath));
                        }
                        
                        chunkIndex++;
                    }

                    _logger.LogInformation("✅ Successfully ingested {ChunkCount} chunks from {File}", 
                        chunks.Count, Path.GetFileName(relativePath));
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error ingesting {FilePath}", relativePath);
                    errorCount++;
                }
            }

            // Emit completion event
            await _eventBus.EmitAsync("cx.language.ingestion.complete", new Dictionary<string, object>
            {
                ["service"] = nameof(CxLanguageVectorIngestionService),
                ["successCount"] = successCount,
                ["errorCount"] = errorCount,
                ["totalFiles"] = _cxReferenceFiles.Count,
                ["timestamp"] = DateTime.Now.ToString("HH:mm:ss.fff"),
                ["status"] = errorCount == 0 ? "success" : "partial_success"
            });

            _logger.LogInformation("🎯 CX Language ingestion complete: {SuccessCount} success, {ErrorCount} errors", 
                successCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Critical error during CX Language reference ingestion");
            
            // Emit error event
            await _eventBus.EmitAsync("cx.language.ingestion.error", new Dictionary<string, object>
            {
                ["service"] = nameof(CxLanguageVectorIngestionService),
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.Now.ToString("HH:mm:ss.fff")
            });
        }
    }

    /// <summary>
    /// Finds the workspace root directory by looking for key files
    /// </summary>
    private string? FindWorkspaceRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var searchDir = currentDir;

        // Look for CxLanguage.sln file as workspace root indicator
        while (searchDir != null)
        {
            if (File.Exists(Path.Combine(searchDir, "CxLanguage.sln")))
            {
                return searchDir;
            }

            var parentDir = Directory.GetParent(searchDir);
            searchDir = parentDir?.FullName;
            
            // Prevent infinite loop
            if (parentDir == null || parentDir.FullName == searchDir)
                break;
        }

        _logger.LogWarning("🔍 Could not find workspace root from {CurrentDir}", currentDir);
        return null;
    }

    /// <summary>
    /// Creates text chunks with overlap for better vector search results
    /// </summary>
    private List<string> CreateDocumentChunks(string content, int chunkSize, int overlap)
    {
        var chunks = new List<string>();
        
        if (content.Length <= chunkSize)
        {
            chunks.Add(content);
            return chunks;
        }

        int position = 0;
        while (position < content.Length)
        {
            var remainingLength = content.Length - position;
            var currentChunkSize = Math.Min(chunkSize, remainingLength);
            
            var chunk = content.Substring(position, currentChunkSize);
            chunks.Add(chunk);
            
            // Move position forward, accounting for overlap
            position += chunkSize - overlap;
            
            // Ensure we don't skip the end of the document
            if (position >= content.Length - overlap)
                break;
        }

        return chunks;
    }

    /// <summary>
    /// Generates a simple embedding for text (hash-based for demonstration)
    /// In production, this would use Azure OpenAI embedding service
    /// </summary>
    private Task<float[]> GenerateSimpleEmbedding(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Task.FromResult(new float[384]); // Empty embedding

        // Simple hash-based embedding for demonstration
        var hash = text.GetHashCode();
        var embedding = new float[384]; // Common embedding dimension
        
        var random = new Random(hash);
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2.0 - 1.0); // Range [-1, 1]
        }
        
        // Normalize the vector
        var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= magnitude;
            }
        }
        
        return Task.FromResult(embedding);
    }

    /// <summary>
    /// Determines content type based on file extension and path
    /// </summary>
    private string GetContentType(string filePath)
    {
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        
        if (fileName.Contains("quick_reference"))
            return "quick_reference";
        if (fileName.Contains("comprehensive"))
            return "comprehensive_guide";
        if (fileName.Contains("instructions"))
            return "development_instructions";
        if (fileName.Contains("internal_events"))
            return "internal_events";
        if (fileName == "readme.md")
            return "readme";
        if (filePath.Contains("copilot-instructions"))
            return "copilot_instructions";
        
        return "documentation";
    }
}
