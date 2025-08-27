using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.Services.VectorStore;

namespace CxLanguage.StandardLibrary.Services.Document
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling - Memory Layer Vector Index Architect
    /// Consciousness-aware document ingestion service that processes text documents into searchable vector chunks.
    /// Integrates with CX Language event system and local vector database for seamless knowledge management.
    /// </summary>
    public class DocumentIngestionService : IDocumentIngestionService
    {
        private readonly ICxEventBus _eventBus;
        private readonly IVectorStoreService _vectorStore;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly ILogger<DocumentIngestionService> _logger;

        // Supported file formats for document ingestion
        private readonly HashSet<string> _supportedFormats = new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".md", ".json"
        };

        // Configuration for text chunking
        private const int DefaultChunkSize = 1000;
        private const int DefaultChunkOverlap = 200;
        
        public DocumentIngestionService(
            ICxEventBus eventBus,
            IVectorStoreService vectorStore,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            ILogger<DocumentIngestionService> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
            _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // NO AUTO HANDLERS - All handlers must be explicitly declared in CX programs
            // However, we still need to register our service methods so they can be called when events are emitted
            
            // Register this service instance for event handling
            try
            {
                _eventBus.Subscribe("document.ingest", HandleDocumentIngestAsync);
                _eventBus.Subscribe("document.batch.ingest", HandleDocumentBatchIngestAsync);
                _logger.LogInformation("✅ DocumentIngestionService event handlers registered");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not register DocumentIngestionService event handlers");
            }
            
            _logger.LogInformation("✅ DocumentIngestionService initialized with consciousness-aware processing");
        }

        /// <summary>
        /// Processes a single document into vector chunks.
        /// </summary>
        public async Task<int> IngestDocumentAsync(string filePath, Dictionary<string, object>? metadata = null)
        {
            try
            {
                _logger.LogInformation("🔄 Starting document ingestion: {FilePath}", filePath);

                // Validate file existence and format
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Document not found: {filePath}");
                }

                if (!IsFormatSupported(filePath))
                {
                    throw new NotSupportedException($"File format not supported: {Path.GetExtension(filePath)}");
                }

                // Read document content
                var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("⚠️ Document is empty: {FilePath}", filePath);
                    return 0;
                }

                // Extract content based on file format
                var extractedContent = await ExtractContentAsync(filePath, content);
                
                // Create text chunks
                var chunks = CreateTextChunks(extractedContent, DefaultChunkSize, DefaultChunkOverlap);
                _logger.LogInformation("📄 Created {ChunkCount} chunks from document: {FilePath}", chunks.Count, filePath);

                // Emit chunking complete event
                await _eventBus.EmitAsync("document.chunk.complete", new Dictionary<string, object>
                {
                    { "filePath", filePath },
                    { "chunkCount", chunks.Count },
                    { "originalLength", content.Length }
                });

                // Generate embeddings for each chunk
                var chunkTexts = chunks.Select(chunk => chunk.Content).ToList();
                var embeddingResult = await _embeddingGenerator.GenerateAsync(chunkTexts);
                var embeddings = embeddingResult.ToList();
                
                // Store vector records
                var vectorCount = 0;
                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    var embedding = i < embeddings.Count ? embeddings[i].Vector.ToArray() : Array.Empty<float>();
                    
                    // Prepare chunk metadata
                    var chunkMetadata = new Dictionary<string, object>
                    {
                        { "sourceFile", filePath },
                        { "chunkIndex", i },
                        { "chunkStart", chunk.StartPosition },
                        { "chunkEnd", chunk.EndPosition },
                        { "fileFormat", Path.GetExtension(filePath) }
                    };

                    // Add custom metadata if provided
                    if (metadata != null)
                    {
                        foreach (var kvp in metadata)
                        {
                            chunkMetadata[kvp.Key] = kvp.Value;
                        }
                    }

                    // Create and store vector record
                    var vectorRecord = new VectorRecord
                    {
                        Id = $"{Path.GetFileNameWithoutExtension(filePath)}_chunk_{i}_{Guid.NewGuid():N}",
                        Vector = embedding,
                        Content = chunk.Content,
                        Metadata = chunkMetadata,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await _vectorStore.AddAsync(vectorRecord);
                    vectorCount++;
                }

                // Emit vectorization complete event
                await _eventBus.EmitAsync("document.vector.complete", new Dictionary<string, object>
                {
                    { "filePath", filePath },
                    { "vectorCount", vectorCount },
                    { "chunkCount", chunks.Count }
                });

                _logger.LogInformation("✅ Document ingestion completed: {FilePath}, {VectorCount} vectors created", filePath, vectorCount);
                return vectorCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ingesting document: {FilePath}", filePath);
                
                // Emit error event
                await _eventBus.EmitAsync("document.ingest.error", new Dictionary<string, object>
                {
                    { "filePath", filePath },
                    { "error", ex.Message },
                    { "timestamp", DateTimeOffset.UtcNow }
                });
                
                throw;
            }
        }

        /// <summary>
        /// Processes multiple documents into vector chunks.
        /// </summary>
        public async Task<int> IngestDocumentBatchAsync(IEnumerable<string> filePaths, Dictionary<string, object>? metadata = null)
        {
            var totalVectors = 0;
            var processedFiles = 0;
            var errors = new List<string>();

            foreach (var filePath in filePaths)
            {
                try
                {
                    var vectorCount = await IngestDocumentAsync(filePath, metadata);
                    totalVectors += vectorCount;
                    processedFiles++;
                    
                    _logger.LogInformation("📄 Processed file {ProcessedFiles}: {FilePath} ({VectorCount} vectors)", processedFiles, filePath, vectorCount);
                }
                catch (Exception ex)
                {
                    errors.Add($"{filePath}: {ex.Message}");
                    _logger.LogError(ex, "❌ Failed to process file in batch: {FilePath}", filePath);
                }
            }

            // Emit batch completion event
            await _eventBus.EmitAsync("document.batch.ingest.complete", new Dictionary<string, object>
            {
                { "totalFiles", filePaths.Count() },
                { "processedFiles", processedFiles },
                { "totalVectors", totalVectors },
                { "errors", errors },
                { "timestamp", DateTimeOffset.UtcNow }
            });

            _logger.LogInformation("✅ Batch ingestion completed: {ProcessedFiles}/{TotalFiles} files, {TotalVectors} total vectors", 
                processedFiles, filePaths.Count(), totalVectors);

            return totalVectors;
        }

        /// <summary>
        /// Processes a directory recursively for supported document formats.
        /// </summary>
        public async Task<int> IngestDirectoryAsync(string directoryPath, bool recursive = true, Dictionary<string, object>? metadata = null)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            _logger.LogInformation("🔄 Starting directory ingestion: {DirectoryPath} (recursive: {Recursive})", directoryPath, recursive);

            // Find all supported files
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var allFiles = Directory.GetFiles(directoryPath, "*", searchOption);
            var supportedFiles = allFiles.Where(IsFormatSupported).ToList();

            _logger.LogInformation("📁 Found {SupportedFiles} supported files in directory: {DirectoryPath}", supportedFiles.Count, directoryPath);

            // Process all supported files
            var totalVectors = await IngestDocumentBatchAsync(supportedFiles, metadata);

            // Emit directory completion event
            await _eventBus.EmitAsync("document.directory.ingest.complete", new Dictionary<string, object>
            {
                { "directoryPath", directoryPath },
                { "recursive", recursive },
                { "supportedFiles", supportedFiles.Count },
                { "totalFiles", allFiles.Length },
                { "totalVectors", totalVectors },
                { "timestamp", DateTimeOffset.UtcNow }
            });

            return totalVectors;
        }

        /// <summary>
        /// Gets the supported file formats for document ingestion.
        /// </summary>
        public IEnumerable<string> GetSupportedFormats()
        {
            return _supportedFormats.ToList();
        }

        /// <summary>
        /// Validates if a file format is supported for ingestion.
        /// </summary>
        public bool IsFormatSupported(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return _supportedFormats.Contains(extension);
        }

        /// <summary>
        /// Extracts content from different document formats.
        /// </summary>
        private async Task<string> ExtractContentAsync(string filePath, string rawContent)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return extension switch
            {
                ".txt" => rawContent,
                ".md" => rawContent, // For now, treat markdown as plain text
                ".json" => await ExtractJsonContentAsync(rawContent),
                _ => rawContent
            };
        }

        /// <summary>
        /// Extracts meaningful content from JSON documents.
        /// </summary>
        private Task<string> ExtractJsonContentAsync(string jsonContent)
        {
            try
            {
                // Parse JSON and extract text values
                var document = JsonDocument.Parse(jsonContent);
                var textBuilder = new StringBuilder();
                
                ExtractTextFromJsonElement(document.RootElement, textBuilder);
                
                return Task.FromResult(textBuilder.ToString());
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to parse JSON, using raw content");
                return Task.FromResult(jsonContent);
            }
        }

        /// <summary>
        /// Recursively extracts text from JSON elements.
        /// </summary>
        private void ExtractTextFromJsonElement(JsonElement element, StringBuilder textBuilder)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    textBuilder.AppendLine(element.GetString());
                    break;
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        ExtractTextFromJsonElement(property.Value, textBuilder);
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        ExtractTextFromJsonElement(item, textBuilder);
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates text chunks from content with overlapping for better search results.
        /// </summary>
        private List<DocumentChunk> CreateTextChunks(string content, int chunkSize, int overlap)
        {
            var chunks = new List<DocumentChunk>();
            
            if (content.Length <= chunkSize)
            {
                chunks.Add(new DocumentChunk
                {
                    Content = content,
                    StartPosition = 0,
                    EndPosition = content.Length
                });
                return chunks;
            }

            var position = 0;
            var chunkIndex = 0;

            while (position < content.Length)
            {
                var endPosition = Math.Min(position + chunkSize, content.Length);
                var chunkContent = content.Substring(position, endPosition - position);

                chunks.Add(new DocumentChunk
                {
                    Content = chunkContent.Trim(),
                    StartPosition = position,
                    EndPosition = endPosition
                });

                // Move position with overlap
                position = Math.Max(position + chunkSize - overlap, position + 1);
                chunkIndex++;
                
                // Prevent infinite loop
                if (position >= content.Length)
                    break;
            }

            return chunks;
        }

        #region Event Handlers

        /// <summary>
        /// Handles 'document.ingest' event for single document processing.
        /// Expected payload: { path: string, metadata?: object }
        /// </summary>
        private async Task<bool> HandleDocumentIngestAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                if (!payload.TryGetValue("path", out var pathObj) || pathObj is not string filePath)
                {
                    _logger.LogError("❌ document.ingest requires 'path' parameter");
                    return false;
                }

                var metadata = payload.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> meta 
                    ? meta 
                    : null;

                await IngestDocumentAsync(filePath, metadata);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling document.ingest event");
                return false;
            }
        }

        /// <summary>
        /// Handles 'document.batch.ingest' event for batch document processing.
        /// Expected payload: { paths: string[], metadata?: object }
        /// </summary>
        private async Task<bool> HandleDocumentBatchIngestAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                if (!payload.TryGetValue("paths", out var pathsObj) || pathsObj is not IEnumerable<object> pathsList)
                {
                    _logger.LogError("❌ document.batch.ingest requires 'paths' parameter");
                    return false;
                }

                var filePaths = pathsList.OfType<string>().ToList();
                var metadata = payload.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> meta 
                    ? meta 
                    : null;

                await IngestDocumentBatchAsync(filePaths, metadata);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling document.batch.ingest event");
                return false;
            }
        }

        /// <summary>
        /// Handles 'document.directory.ingest' event for directory processing.
        /// Expected payload: { path: string, recursive?: bool, metadata?: object }
        /// </summary>
        private async Task<bool> HandleDirectoryIngestAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                if (!payload.TryGetValue("path", out var pathObj) || pathObj is not string directoryPath)
                {
                    _logger.LogError("❌ document.directory.ingest requires 'path' parameter");
                    return false;
                }

                var recursive = payload.TryGetValue("recursive", out var recursiveObj) && recursiveObj is bool rec ? rec : true;
                var metadata = payload.TryGetValue("metadata", out var metadataObj) && metadataObj is Dictionary<string, object> meta 
                    ? meta 
                    : null;

                await IngestDirectoryAsync(directoryPath, recursive, metadata);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling document.directory.ingest event");
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a chunk of text extracted from a document.
    /// </summary>
    public class DocumentChunk
    {
        public string Content { get; set; } = string.Empty;
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
    }
}
