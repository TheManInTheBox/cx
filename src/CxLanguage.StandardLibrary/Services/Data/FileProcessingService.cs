using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.Services.VectorStore;

namespace CxLanguage.StandardLibrary.Services.Data;

/// <summary>
/// 🗂️ Dr. Marcus "LocalLLM" Chen's File Processing Service
/// Comprehensive file ingestion and processing for the CX Language platform
/// Supports multiple formats: TXT, JSON, CSV, XML, and more
/// </summary>
public class FileProcessingService
{
    private readonly ILogger<FileProcessingService> _logger;
    private readonly ICxEventBus _eventBus;
    private readonly IVectorStoreService _vectorStore;

    public FileProcessingService(
        ILogger<FileProcessingService> logger, 
        ICxEventBus eventBus,
        IVectorStoreService vectorStore)
    {
        _logger = logger;
        _eventBus = eventBus;
        _vectorStore = vectorStore;
        _logger.LogInformation("🗂️ Dr. Marcus 'LocalLLM' Chen's FileProcessingService initialized");
        _eventBus.EmitAsync("file.processing.service.ready", new { service = nameof(FileProcessingService) });
    }

    /// <summary>
    /// Process a single file and extract text content for vector ingestion
    /// </summary>
    public async Task<FileProcessingResult> ProcessFileAsync(string filePath, FileProcessingOptions? options = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                var error = $"File not found: {filePath}";
                _logger.LogError(error);
                return FileProcessingResult.Failure(error);
            }

            var fileInfo = new FileInfo(filePath);
            var extension = fileInfo.Extension.ToLowerInvariant();
            
            _logger.LogInformation("🔍 Processing file: {FileName} ({Size} bytes, {Extension})", 
                fileInfo.Name, fileInfo.Length, extension);

            var result = extension switch
            {
                ".txt" => await ProcessTextFileAsync(filePath, options),
                ".json" => await ProcessJsonFileAsync(filePath, options),
                ".csv" => await ProcessCsvFileAsync(filePath, options),
                ".xml" => await ProcessXmlFileAsync(filePath, options),
                ".md" => await ProcessMarkdownFileAsync(filePath, options),
                ".log" => await ProcessLogFileAsync(filePath, options),
                _ => await ProcessGenericTextFileAsync(filePath, options)
            };

            if (result.IsSuccess)
            {
                _eventBus.EmitAsync("file.processed", new { 
                    filePath, 
                    extension, 
                    contentLength = result.ExtractedText?.Length ?? 0,
                    recordsGenerated = result.Records?.Count ?? 0
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file: {FilePath}", filePath);
            return FileProcessingResult.Failure($"Processing error: {ex.Message}");
        }
    }

    /// <summary>
    /// Process multiple files in batch
    /// </summary>
    public async Task<BatchProcessingResult> ProcessBatchAsync(IEnumerable<string> filePaths, FileProcessingOptions? options = null)
    {
        var results = new List<FileProcessingResult>();
        var totalFiles = 0;
        var successCount = 0;

        foreach (var filePath in filePaths)
        {
            totalFiles++;
            var result = await ProcessFileAsync(filePath, options);
            results.Add(result);
            
            if (result.IsSuccess)
            {
                successCount++;
                
                // Auto-ingest to vector store if enabled
                if (options?.AutoIngestToVectorStore == true && result.Records != null)
                {
                    foreach (var record in result.Records)
                    {
                        await _vectorStore.AddAsync(record);
                    }
                }
            }

            // Progress reporting
            if (totalFiles % 10 == 0)
            {
                _logger.LogInformation("📊 Batch progress: {Success}/{Total} files processed", 
                    successCount, totalFiles);
            }
        }

        _eventBus.EmitAsync("batch.processing.complete", new { 
            totalFiles, 
            successCount, 
            failureCount = totalFiles - successCount 
        });

        return new BatchProcessingResult
        {
            TotalFiles = totalFiles,
            SuccessCount = successCount,
            FailureCount = totalFiles - successCount,
            Results = results
        };
    }

    /// <summary>
    /// Process plain text files
    /// </summary>
    private async Task<FileProcessingResult> ProcessTextFileAsync(string filePath, FileProcessingOptions? options)
    {
        var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        var chunks = ChunkText(content, options?.ChunkSize ?? 1000, options?.ChunkOverlap ?? 100);
        
        var records = new List<VectorRecord>();
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        
        for (int i = 0; i < chunks.Count; i++)
        {
            records.Add(new VectorRecord
            {
                Id = $"{fileName}_chunk_{i}",
                Content = chunks[i],
                Metadata = new Dictionary<string, object>
                {
                    ["fileName"] = fileName,
                    ["filePath"] = filePath,
                    ["fileType"] = "text",
                    ["chunkIndex"] = i,
                    ["totalChunks"] = chunks.Count,
                    ["processedAt"] = DateTimeOffset.UtcNow
                }
            });
        }

        return FileProcessingResult.Success(content, records);
    }

    /// <summary>
    /// Process JSON files - extract text from all string values
    /// </summary>
    private async Task<FileProcessingResult> ProcessJsonFileAsync(string filePath, FileProcessingOptions? options)
    {
        var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        
        try
        {
            var jsonDocument = JsonDocument.Parse(jsonContent);
            var extractedText = ExtractTextFromJson(jsonDocument.RootElement);
            
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var record = new VectorRecord
            {
                Id = $"{fileName}_json",
                Content = extractedText,
                Metadata = new Dictionary<string, object>
                {
                    ["fileName"] = fileName,
                    ["filePath"] = filePath,
                    ["fileType"] = "json",
                    ["originalSize"] = jsonContent.Length,
                    ["processedAt"] = DateTimeOffset.UtcNow
                }
            };

            return FileProcessingResult.Success(extractedText, new List<VectorRecord> { record });
        }
        catch (JsonException ex)
        {
            return FileProcessingResult.Failure($"Invalid JSON format: {ex.Message}");
        }
    }

    /// <summary>
    /// Process CSV files - convert to structured text
    /// </summary>
    private async Task<FileProcessingResult> ProcessCsvFileAsync(string filePath, FileProcessingOptions? options)
    {
        var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
        
        if (lines.Length == 0)
        {
            return FileProcessingResult.Failure("Empty CSV file");
        }

        var extractedText = new StringBuilder();
        var headers = lines[0].Split(',');
        
        extractedText.AppendLine($"CSV Data with columns: {string.Join(", ", headers)}");
        
        // Process data rows (skip header)
        for (int i = 1; i < Math.Min(lines.Length, (options?.MaxCsvRows ?? 100) + 1); i++)
        {
            var values = lines[i].Split(',');
            
            for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
            {
                if (!string.IsNullOrWhiteSpace(values[j]))
                {
                    extractedText.AppendLine($"{headers[j]}: {values[j].Trim()}");
                }
            }
            extractedText.AppendLine(); // Separator between rows
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var record = new VectorRecord
        {
            Id = $"{fileName}_csv",
            Content = extractedText.ToString(),
            Metadata = new Dictionary<string, object>
            {
                ["fileName"] = fileName,
                ["filePath"] = filePath,
                ["fileType"] = "csv",
                ["rowCount"] = lines.Length - 1,
                ["columnCount"] = headers.Length,
                ["processedAt"] = DateTimeOffset.UtcNow
            }
        };

        return FileProcessingResult.Success(extractedText.ToString(), new List<VectorRecord> { record });
    }

    /// <summary>
    /// Process XML files - extract text content
    /// </summary>
    private async Task<FileProcessingResult> ProcessXmlFileAsync(string filePath, FileProcessingOptions? options)
    {
        var xmlContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        
        // Simple XML text extraction (removes tags, keeps content)
        var extractedText = System.Text.RegularExpressions.Regex.Replace(xmlContent, "<[^>]*>", " ");
        extractedText = System.Text.RegularExpressions.Regex.Replace(extractedText, @"\s+", " ").Trim();
        
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var record = new VectorRecord
        {
            Id = $"{fileName}_xml",
            Content = extractedText,
            Metadata = new Dictionary<string, object>
            {
                ["fileName"] = fileName,
                ["filePath"] = filePath,
                ["fileType"] = "xml",
                ["originalSize"] = xmlContent.Length,
                ["extractedSize"] = extractedText.Length,
                ["processedAt"] = DateTimeOffset.UtcNow
            }
        };

        return FileProcessingResult.Success(extractedText, new List<VectorRecord> { record });
    }

    /// <summary>
    /// Process Markdown files
    /// </summary>
    private async Task<FileProcessingResult> ProcessMarkdownFileAsync(string filePath, FileProcessingOptions? options)
    {
        var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        
        // Simple markdown processing - remove markdown syntax for better text extraction
        var cleanContent = System.Text.RegularExpressions.Regex.Replace(content, @"[#*`_~]", "");
        cleanContent = System.Text.RegularExpressions.Regex.Replace(cleanContent, @"\[([^\]]+)\]\([^\)]+\)", "$1");
        
        var chunks = ChunkText(cleanContent, options?.ChunkSize ?? 1000, options?.ChunkOverlap ?? 100);
        var records = new List<VectorRecord>();
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        
        for (int i = 0; i < chunks.Count; i++)
        {
            records.Add(new VectorRecord
            {
                Id = $"{fileName}_md_chunk_{i}",
                Content = chunks[i],
                Metadata = new Dictionary<string, object>
                {
                    ["fileName"] = fileName,
                    ["filePath"] = filePath,
                    ["fileType"] = "markdown",
                    ["chunkIndex"] = i,
                    ["totalChunks"] = chunks.Count,
                    ["processedAt"] = DateTimeOffset.UtcNow
                }
            });
        }

        return FileProcessingResult.Success(cleanContent, records);
    }

    /// <summary>
    /// Process log files
    /// </summary>
    private async Task<FileProcessingResult> ProcessLogFileAsync(string filePath, FileProcessingOptions? options)
    {
        var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
        
        // Extract meaningful log entries (filter out empty lines and basic formatting)
        var meaningfulLines = lines
            .Where(line => !string.IsNullOrWhiteSpace(line) && line.Length > 10)
            .Take(options?.MaxLogLines ?? 1000);
        
        var extractedText = string.Join("\n", meaningfulLines);
        
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var record = new VectorRecord
        {
            Id = $"{fileName}_log",
            Content = extractedText,
            Metadata = new Dictionary<string, object>
            {
                ["fileName"] = fileName,
                ["filePath"] = filePath,
                ["fileType"] = "log",
                ["totalLines"] = lines.Length,
                ["processedLines"] = meaningfulLines.Count(),
                ["processedAt"] = DateTimeOffset.UtcNow
            }
        };

        return FileProcessingResult.Success(extractedText, new List<VectorRecord> { record });
    }

    /// <summary>
    /// Process generic text files
    /// </summary>
    private async Task<FileProcessingResult> ProcessGenericTextFileAsync(string filePath, FileProcessingOptions? options)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return await ProcessTextFileAsync(filePath, options);
        }
        catch (Exception ex)
        {
            return FileProcessingResult.Failure($"Unable to read file as text: {ex.Message}");
        }
    }

    /// <summary>
    /// Extract text content from JSON recursively
    /// </summary>
    private string ExtractTextFromJson(JsonElement element, int depth = 0)
    {
        if (depth > 10) return ""; // Prevent infinite recursion
        
        var result = new StringBuilder();
        
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                result.AppendLine(element.GetString());
                break;
                
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    result.AppendLine($"{property.Name}: {ExtractTextFromJson(property.Value, depth + 1)}");
                }
                break;
                
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    result.AppendLine(ExtractTextFromJson(item, depth + 1));
                }
                break;
                
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                result.AppendLine(element.ToString());
                break;
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Intelligent text chunking with overlap
    /// </summary>
    private List<string> ChunkText(string text, int chunkSize, int overlapSize)
    {
        var chunks = new List<string>();
        
        if (string.IsNullOrEmpty(text) || text.Length <= chunkSize)
        {
            chunks.Add(text);
            return chunks;
        }

        var position = 0;
        while (position < text.Length)
        {
            var endPosition = Math.Min(position + chunkSize, text.Length);
            
            // Try to break at sentence or paragraph boundaries
            if (endPosition < text.Length)
            {
                var lastPeriod = text.LastIndexOf('.', endPosition - 1, Math.Min(100, endPosition - position));
                var lastNewline = text.LastIndexOf('\n', endPosition - 1, Math.Min(100, endPosition - position));
                
                var breakPoint = Math.Max(lastPeriod, lastNewline);
                if (breakPoint > position + chunkSize / 2)
                {
                    endPosition = breakPoint + 1;
                }
            }
            
            var chunk = text.Substring(position, endPosition - position).Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }
            
            position = endPosition - overlapSize;
            if (position <= 0) position = endPosition;
        }
        
        return chunks;
    }
}

/// <summary>
/// Configuration options for file processing
/// </summary>
public class FileProcessingOptions
{
    public int ChunkSize { get; set; } = 1000;
    public int ChunkOverlap { get; set; } = 100;
    public int MaxCsvRows { get; set; } = 100;
    public int MaxLogLines { get; set; } = 1000;
    public bool AutoIngestToVectorStore { get; set; } = true;
    public Dictionary<string, object> CustomMetadata { get; set; } = new();
}

/// <summary>
/// Result of file processing operation
/// </summary>
public class FileProcessingResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ExtractedText { get; init; }
    public List<VectorRecord>? Records { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();

    public static FileProcessingResult Success(string extractedText, List<VectorRecord> records) =>
        new() { IsSuccess = true, ExtractedText = extractedText, Records = records };

    public static FileProcessingResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Result of batch processing operation
/// </summary>
public class BatchProcessingResult
{
    public int TotalFiles { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<FileProcessingResult> Results { get; init; } = new();
    
    public double SuccessRate => TotalFiles > 0 ? (double)SuccessCount / TotalFiles : 0;
}
