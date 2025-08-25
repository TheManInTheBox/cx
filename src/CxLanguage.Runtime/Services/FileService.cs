using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime.Services
{
    /// <summary>
    /// Handles system.file.read and system.file.write events for file I/O operations with consciousness-aware patterns
    /// Supports both text and binary file operations, conscious entity serialization, and proper error handling
    /// </summary>
    public class FileService
    {
        private readonly ICxEventBus _eventBus;
        private readonly ILogger<FileService> _logger;
        
        public FileService(ICxEventBus eventBus, ILogger<FileService> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to file read and write events
            _eventBus.Subscribe("system.file.read", HandleFileReadAsync);
            _eventBus.Subscribe("system.file.write", HandleFileWriteAsync);
            
            _logger.LogInformation("FileService subscribed to file I/O events with consciousness-aware patterns");
        }

        /// <summary>
        /// Handler for 'system.file.read' event
        /// Supports payloads: { path: string, encoding: string?, handlers: array? }
        /// Reads file contents and invokes custom handlers with file data
        /// </summary>
        private async Task<bool> HandleFileReadAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Extract file path
                if (!payload.TryGetValue("path", out var pathObj) || pathObj is not string filePath || string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogError("system.file.read requires 'path' parameter");
                    await EmitErrorEvent("system.file.read.error", "Missing or invalid 'path' parameter", payload);
                    return false;
                }

                // Resolve relative paths to absolute paths
                filePath = Path.GetFullPath(filePath);
                _logger.LogDebug("Reading file: {FilePath}", filePath);

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    await EmitErrorEvent("system.file.read.error", $"File not found: {filePath}", payload);
                    return false;
                }

                // Extract encoding (default to UTF-8)
                var encoding = Encoding.UTF8;
                if (payload.TryGetValue("encoding", out var encodingObj) && encodingObj is string encodingName)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(encodingName);
                        _logger.LogDebug("Using encoding: {Encoding}", encodingName);
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning(ex, "Invalid encoding '{Encoding}', falling back to UTF-8", encodingName);
                        encoding = Encoding.UTF8;
                    }
                }

                try
                {
                    // Read file contents
                    var contents = await File.ReadAllTextAsync(filePath, encoding);
                    _logger.LogDebug("Successfully read file: {FilePath}, length: {Length}", filePath, contents.Length);

                    // Prepare result payload
                    var resultPayload = new Dictionary<string, object>
                    {
                        { "path", filePath },
                        { "contents", contents },
                        { "length", contents.Length },
                        { "encoding", encoding.EncodingName }
                    };

                    // Emit success event
                    await _eventBus.EmitAsync("system.file.read.success", resultPayload);

                    // Emit custom handlers if provided
                    if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                    {
                        foreach (var handlerObj in handlers)
                        {
                            if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                            {
                                await _eventBus.EmitAsync(handlerName, resultPayload);
                                _logger.LogDebug("Emitted handler event: {HandlerName}", handlerName);
                            }
                        }
                    }

                    return true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Access denied reading file: {FilePath}", filePath);
                    await EmitErrorEvent("system.file.read.error", $"Access denied: {ex.Message}", payload);
                    return false;
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "I/O error reading file: {FilePath}", filePath);
                    await EmitErrorEvent("system.file.read.error", $"I/O error: {ex.Message}", payload);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error handling system.file.read event");
                await EmitErrorEvent("system.file.read.error", $"Unexpected error: {ex.Message}", payload ?? new Dictionary<string, object>());
                return false;
            }
        }

        /// <summary>
        /// Handler for 'system.file.write' event
        /// Supports payloads: { path: string, content: string?, object: any?, encoding: string?, append: bool? }
        /// Writes content or serialized objects to files
        /// </summary>
        private async Task<bool> HandleFileWriteAsync(object? sender, string eventName, IDictionary<string, object>? payload)
        {
            try
            {
                payload ??= new Dictionary<string, object>();

                // Extract file path
                if (!payload.TryGetValue("path", out var pathObj) || pathObj is not string filePath || string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogError("system.file.write requires 'path' parameter");
                    await EmitErrorEvent("system.file.write.error", "Missing or invalid 'path' parameter", payload);
                    return false;
                }

                // Resolve relative paths to absolute paths
                filePath = Path.GetFullPath(filePath);
                _logger.LogDebug("Writing to file: {FilePath}", filePath);

                // Extract encoding (default to UTF-8)
                var encoding = Encoding.UTF8;
                if (payload.TryGetValue("encoding", out var encodingObj) && encodingObj is string encodingName)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(encodingName);
                        _logger.LogDebug("Using encoding: {Encoding}", encodingName);
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning(ex, "Invalid encoding '{Encoding}', falling back to UTF-8", encodingName);
                        encoding = Encoding.UTF8;
                    }
                }

                // Check append mode
                var appendMode = payload.TryGetValue("append", out var appendObj) && appendObj is bool append && append;
                
                string contentToWrite;

                // Determine content source - prioritize 'content' over 'object' or 'data'
                if (payload.TryGetValue("content", out var contentObj) && contentObj is not null)
                {
                    contentToWrite = contentObj.ToString() ?? string.Empty;
                    _logger.LogDebug("Writing text content, length: {Length}", contentToWrite.Length);
                }
                else if (payload.TryGetValue("object", out var objectObj) || payload.TryGetValue("data", out objectObj))
                {
                    // Serialize object to JSON with proper formatting
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true
                        };
                        
                        contentToWrite = JsonSerializer.Serialize(objectObj, options);
                        _logger.LogDebug("Serialized object to JSON, length: {Length}", contentToWrite.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to serialize object to JSON");
                        await EmitErrorEvent("system.file.write.error", $"JSON serialization failed: {ex.Message}", payload);
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("system.file.write requires either 'content', 'object', or 'data' parameter");
                    await EmitErrorEvent("system.file.write.error", "Missing 'content', 'object', or 'data' parameter", payload);
                    return false;
                }

                try
                {
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        _logger.LogDebug("Created directory: {Directory}", directory);
                    }

                    // Write file
                    if (appendMode)
                    {
                        await File.AppendAllTextAsync(filePath, contentToWrite, encoding);
                        _logger.LogDebug("Successfully appended to file: {FilePath}", filePath);
                    }
                    else
                    {
                        await File.WriteAllTextAsync(filePath, contentToWrite, encoding);
                        _logger.LogDebug("Successfully wrote file: {FilePath}", filePath);
                    }

                    // Prepare result payload
                    var resultPayload = new Dictionary<string, object>
                    {
                        { "path", filePath },
                        { "bytesWritten", encoding.GetByteCount(contentToWrite) },
                        { "encoding", encoding.EncodingName },
                        { "append", appendMode }
                    };

                    // Emit success event
                    await _eventBus.EmitAsync("system.file.write.success", resultPayload);

                    // Emit custom handlers if provided
                    if (payload.TryGetValue("handlers", out var handlersObj) && handlersObj is System.Collections.IEnumerable handlers)
                    {
                        foreach (var handlerObj in handlers)
                        {
                            if (handlerObj is string handlerName && !string.IsNullOrEmpty(handlerName))
                            {
                                await _eventBus.EmitAsync(handlerName, resultPayload);
                                _logger.LogDebug("Emitted handler event: {HandlerName}", handlerName);
                            }
                        }
                    }

                    return true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Access denied writing file: {FilePath}", filePath);
                    await EmitErrorEvent("system.file.write.error", $"Access denied: {ex.Message}", payload);
                    return false;
                }
                catch (DirectoryNotFoundException ex)
                {
                    _logger.LogError(ex, "Directory not found for file: {FilePath}", filePath);
                    await EmitErrorEvent("system.file.write.error", $"Directory not found: {ex.Message}", payload);
                    return false;
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "I/O error writing file: {FilePath}", filePath);
                    await EmitErrorEvent("system.file.write.error", $"I/O error: {ex.Message}", payload);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error handling system.file.write event");
                await EmitErrorEvent("system.file.write.error", $"Unexpected error: {ex.Message}", payload ?? new Dictionary<string, object>());
                return false;
            }
        }

        /// <summary>
        /// Helper method to emit error events with standardized format
        /// </summary>
        private async Task EmitErrorEvent(string eventName, string errorMessage, IDictionary<string, object> originalPayload)
        {
            var errorPayload = new Dictionary<string, object>
            {
                { "error", errorMessage },
                { "timestamp", DateTime.UtcNow },
                { "originalPayload", new Dictionary<string, object>(originalPayload) }
            };

            await _eventBus.EmitAsync(eventName, errorPayload);
        }
    }
}
