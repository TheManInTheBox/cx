using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.Core;

namespace CxLanguage.StandardLibrary.Services.IO
{
    /// <summary>
    /// CX Language JSON Service - JSON operations with consciousness integration
    /// Part of the Standard Library I/O Module
    /// </summary>
    public class JsonService : ModernAiServiceBase
    {
        private readonly ICxEventBus _eventBus;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonService(IServiceProvider serviceProvider, ICxEventBus eventBus, ILogger<JsonService> logger) 
            : base(serviceProvider, logger)
        {
            _eventBus = eventBus;
            _logger.LogInformation("📄 JsonService initialized with consciousness integration");
            
            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
            
            // NO AUTO HANDLERS - All handlers must be explicitly declared in CX programs
        }

        private async Task HandleJsonReadRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"📖 Reading JSON file: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ File path is required for JSON read operation");
                    await _eventBus.EmitAsync("json.read.error", new Dictionary<string, object>
                    {
                        ["error"] = "File path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (!File.Exists(path))
                {
                    _logger.LogError($"❌ JSON file not found: {path}");
                    await _eventBus.EmitAsync("json.read.error", new Dictionary<string, object>
                    {
                        ["error"] = $"JSON file not found: {path}",
                        ["path"] = path,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var jsonContent = await File.ReadAllTextAsync(path);
                var parsedData = JsonSerializer.Deserialize<object>(jsonContent, _jsonOptions);
                
                _logger.LogInformation($"✅ Successfully read and parsed JSON file: {path}");

                await _eventBus.EmitAsync("json.read.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["content"] = jsonContent,
                    ["data"] = parsedData ?? new object(),
                    ["size"] = jsonContent.Length,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"❌ JSON parsing error: {jsonEx.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.read.error", new Dictionary<string, object>
                {
                    ["error"] = $"JSON parsing error: {jsonEx.Message}",
                    ["type"] = "json_parse_error",
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error reading JSON file: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.read.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleJsonWriteRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var jsonData = data?.GetValueOrDefault("data");
                var indent = data?.GetValueOrDefault("indent")?.ToString()?.ToLower() != "false";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"✏️ Writing JSON to file: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ File path is required for JSON write operation");
                    await _eventBus.EmitAsync("json.write.error", new Dictionary<string, object>
                    {
                        ["error"] = "File path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (jsonData == null)
                {
                    _logger.LogError("❌ Data is required for JSON write operation");
                    await _eventBus.EmitAsync("json.write.error", new Dictionary<string, object>
                    {
                        ["error"] = "Data is required",
                        ["path"] = path,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation($"📁 Created directory: {directory}");
                }

                var options = new JsonSerializerOptions(_jsonOptions)
                {
                    WriteIndented = indent
                };

                var jsonContent = JsonSerializer.Serialize(jsonData, options);
                await File.WriteAllTextAsync(path, jsonContent);
                
                _logger.LogInformation($"✅ Successfully wrote JSON to file: {path}");

                await _eventBus.EmitAsync("json.write.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["size"] = jsonContent.Length,
                    ["indent"] = indent,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"❌ JSON serialization error: {jsonEx.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.write.error", new Dictionary<string, object>
                {
                    ["error"] = $"JSON serialization error: {jsonEx.Message}",
                    ["type"] = "json_serialization_error",
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error writing JSON file: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.write.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleJsonValidateRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var jsonContent = data?.GetValueOrDefault("content")?.ToString();
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation("🔍 Validating JSON content");

                if (string.IsNullOrEmpty(jsonContent))
                {
                    _logger.LogError("❌ JSON content is required for validation");
                    await _eventBus.EmitAsync("json.validate.error", new Dictionary<string, object>
                    {
                        ["error"] = "JSON content is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                try
                {
                    using (var doc = JsonDocument.Parse(jsonContent))
                    {
                        var isValid = true;
                        var jsonType = doc.RootElement.ValueKind.ToString();
                        
                        _logger.LogInformation($"✅ JSON is valid (type: {jsonType})");

                        await _eventBus.EmitAsync("json.validate.completed", new Dictionary<string, object>
                        {
                            ["valid"] = isValid,
                            ["type"] = jsonType,
                            ["size"] = jsonContent.Length,
                            ["requestId"] = requestId,
                            ["timestamp"] = DateTime.UtcNow
                        });
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogWarning($"❌ JSON validation failed: {jsonEx.Message}");

                    await _eventBus.EmitAsync("json.validate.completed", new Dictionary<string, object>
                    {
                        ["valid"] = false,
                        ["error"] = jsonEx.Message,
                        ["type"] = "invalid",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error validating JSON: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.validate.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleJsonParseRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var jsonContent = data?.GetValueOrDefault("content")?.ToString();
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation("📝 Parsing JSON content");

                if (string.IsNullOrEmpty(jsonContent))
                {
                    _logger.LogError("❌ JSON content is required for parsing");
                    await _eventBus.EmitAsync("json.parse.error", new Dictionary<string, object>
                    {
                        ["error"] = "JSON content is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var parsedData = JsonSerializer.Deserialize<object>(jsonContent, _jsonOptions);
                _logger.LogInformation("✅ Successfully parsed JSON content");

                await _eventBus.EmitAsync("json.parse.completed", new Dictionary<string, object>
                {
                    ["data"] = parsedData ?? new object(),
                    ["originalSize"] = jsonContent.Length,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"❌ JSON parsing error: {jsonEx.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.parse.error", new Dictionary<string, object>
                {
                    ["error"] = $"JSON parsing error: {jsonEx.Message}",
                    ["type"] = "json_parse_error",
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error parsing JSON: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.parse.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleJsonStringifyRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var objectData = data?.GetValueOrDefault("data");
                var indent = data?.GetValueOrDefault("indent")?.ToString()?.ToLower() != "false";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation("📝 Converting object to JSON string");

                if (objectData == null)
                {
                    _logger.LogError("❌ Data is required for JSON stringify operation");
                    await _eventBus.EmitAsync("json.stringify.error", new Dictionary<string, object>
                    {
                        ["error"] = "Data is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var options = new JsonSerializerOptions(_jsonOptions)
                {
                    WriteIndented = indent
                };

                var jsonContent = JsonSerializer.Serialize(objectData, options);
                _logger.LogInformation($"✅ Successfully converted object to JSON ({jsonContent.Length} characters)");

                await _eventBus.EmitAsync("json.stringify.completed", new Dictionary<string, object>
                {
                    ["content"] = jsonContent,
                    ["size"] = jsonContent.Length,
                    ["indent"] = indent,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"❌ JSON serialization error: {jsonEx.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.stringify.error", new Dictionary<string, object>
                {
                    ["error"] = $"JSON serialization error: {jsonEx.Message}",
                    ["type"] = "json_serialization_error",
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error converting to JSON: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("json.stringify.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }
    }
}
