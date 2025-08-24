using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.Core;

namespace CxLanguage.StandardLibrary.Services.IO
{
    /// <summary>
    /// CX Language Directory Service - Directory operations with consciousness integration
    /// Part of the Standard Library I/O Module
    /// </summary>
    public class DirectoryService : ModernAiServiceBase
    {
        private readonly ICxEventBus _eventBus;

        public DirectoryService(IServiceProvider serviceProvider, ICxEventBus eventBus, ILogger<DirectoryService> logger) 
            : base(serviceProvider, logger)
        {
            _eventBus = eventBus;
            _logger.LogDebug("📁 DirectoryService initialized with consciousness integration");
            
            // Subscribe to directory events
            _eventBus.Subscribe("directory.create.request", async (sender, eventName, data) => 
            {
                var eventPayload = new CxEventPayload(eventName, data ?? new Dictionary<string, object>());
                await HandleDirectoryCreateRequest(eventPayload);
                return true;
            });
            _eventBus.Subscribe("directory.list.request", async (sender, eventName, data) => 
            {
                var eventPayload = new CxEventPayload(eventName, data ?? new Dictionary<string, object>());
                await HandleDirectoryListRequest(eventPayload);
                return true;
            });
            _eventBus.Subscribe("directory.exists.request", async (sender, eventName, data) => 
            {
                var eventPayload = new CxEventPayload(eventName, data ?? new Dictionary<string, object>());
                await HandleDirectoryExistsRequest(eventPayload);
                return true;
            });
            _eventBus.Subscribe("directory.delete.request", async (sender, eventName, data) => 
            {
                var eventPayload = new CxEventPayload(eventName, data ?? new Dictionary<string, object>());
                await HandleDirectoryDeleteRequest(eventPayload);
                return true;
            });
            _eventBus.Subscribe("path.join.request", async (sender, eventName, data) => 
            {
                var eventPayload = new CxEventPayload(eventName, data ?? new Dictionary<string, object>());
                await HandlePathJoinRequest(eventPayload);
                return true;
            });
        }

        private async Task HandleDirectoryCreateRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var recursive = data?.GetValueOrDefault("recursive")?.ToString()?.ToLower() == "true";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"📁 Creating directory: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ Directory path is required for create operation");
                    await _eventBus.EmitAsync("directory.create.error", new Dictionary<string, object>
                    {
                        ["error"] = "Directory path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (Directory.Exists(path))
                {
                    _logger.LogInformation($"ℹ️ Directory already exists: {path}");
                    await _eventBus.EmitAsync("directory.create.completed", new Dictionary<string, object>
                    {
                        ["path"] = path,
                        ["created"] = false,
                        ["existed"] = true,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var directoryInfo = Directory.CreateDirectory(path);
                _logger.LogInformation($"✅ Successfully created directory: {path}");

                await _eventBus.EmitAsync("directory.create.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["fullPath"] = directoryInfo.FullName,
                    ["created"] = true,
                    ["existed"] = false,
                    ["recursive"] = recursive,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating directory: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("directory.create.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleDirectoryListRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var pattern = data?.GetValueOrDefault("pattern")?.ToString() ?? "*";
                var recursive = data?.GetValueOrDefault("recursive")?.ToString()?.ToLower() == "true";
                var includeFiles = data?.GetValueOrDefault("includeFiles")?.ToString()?.ToLower() != "false";
                var includeDirectories = data?.GetValueOrDefault("includeDirectories")?.ToString()?.ToLower() != "false";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"📂 Listing directory: {path} (pattern: {pattern}, recursive: {recursive})");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ Directory path is required for list operation");
                    await _eventBus.EmitAsync("directory.list.error", new Dictionary<string, object>
                    {
                        ["error"] = "Directory path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (!Directory.Exists(path))
                {
                    _logger.LogError($"❌ Directory not found: {path}");
                    await _eventBus.EmitAsync("directory.list.error", new Dictionary<string, object>
                    {
                        ["error"] = $"Directory not found: {path}",
                        ["path"] = path,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var items = new List<Dictionary<string, object>>();
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                if (includeFiles)
                {
                    var files = Directory.GetFiles(path, pattern, searchOption);
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        items.Add(new Dictionary<string, object>
                        {
                            ["name"] = fileInfo.Name,
                            ["fullPath"] = fileInfo.FullName,
                            ["relativePath"] = Path.GetRelativePath(path, fileInfo.FullName),
                            ["type"] = "file",
                            ["size"] = fileInfo.Length,
                            ["extension"] = fileInfo.Extension,
                            ["created"] = fileInfo.CreationTimeUtc,
                            ["modified"] = fileInfo.LastWriteTimeUtc,
                            ["isReadOnly"] = fileInfo.IsReadOnly
                        });
                    }
                }

                if (includeDirectories)
                {
                    var directories = Directory.GetDirectories(path, pattern, searchOption);
                    foreach (var directory in directories)
                    {
                        var dirInfo = new DirectoryInfo(directory);
                        items.Add(new Dictionary<string, object>
                        {
                            ["name"] = dirInfo.Name,
                            ["fullPath"] = dirInfo.FullName,
                            ["relativePath"] = Path.GetRelativePath(path, dirInfo.FullName),
                            ["type"] = "directory",
                            ["created"] = dirInfo.CreationTimeUtc,
                            ["modified"] = dirInfo.LastWriteTimeUtc
                        });
                    }
                }

                _logger.LogInformation($"✅ Successfully listed {items.Count} items from {path}");

                await _eventBus.EmitAsync("directory.list.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["pattern"] = pattern,
                    ["recursive"] = recursive,
                    ["includeFiles"] = includeFiles,
                    ["includeDirectories"] = includeDirectories,
                    ["items"] = items,
                    ["count"] = items.Count,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error listing directory: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("directory.list.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleDirectoryExistsRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"🔍 Checking if directory exists: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ Directory path is required for exists check");
                    await _eventBus.EmitAsync("directory.exists.error", new Dictionary<string, object>
                    {
                        ["error"] = "Directory path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var exists = Directory.Exists(path);
                _logger.LogInformation($"✅ Directory exists check: {path} = {exists}");

                await _eventBus.EmitAsync("directory.exists.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["exists"] = exists,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking directory existence: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("directory.exists.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleDirectoryDeleteRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var recursive = data?.GetValueOrDefault("recursive")?.ToString()?.ToLower() == "true";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"🗑️ Deleting directory: {path} (recursive: {recursive})");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ Directory path is required for delete operation");
                    await _eventBus.EmitAsync("directory.delete.error", new Dictionary<string, object>
                    {
                        ["error"] = "Directory path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (!Directory.Exists(path))
                {
                    _logger.LogError($"❌ Directory not found for deletion: {path}");
                    await _eventBus.EmitAsync("directory.delete.error", new Dictionary<string, object>
                    {
                        ["error"] = $"Directory not found: {path}",
                        ["path"] = path,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                Directory.Delete(path, recursive);
                _logger.LogInformation($"✅ Successfully deleted directory: {path}");

                await _eventBus.EmitAsync("directory.delete.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["recursive"] = recursive,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting directory: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("directory.delete.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandlePathJoinRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var paths = data?.GetValueOrDefault("paths") as List<object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"🔗 Joining paths: {string.Join(", ", paths ?? new List<object>())}");

                if (paths == null || paths.Count == 0)
                {
                    _logger.LogError("❌ Path segments are required for join operation");
                    await _eventBus.EmitAsync("path.join.error", new Dictionary<string, object>
                    {
                        ["error"] = "Path segments are required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var pathStrings = paths.Select(p => p?.ToString() ?? "").Where(p => !string.IsNullOrEmpty(p)).ToArray();
                if (pathStrings.Length == 0)
                {
                    _logger.LogError("❌ At least one valid path segment is required");
                    await _eventBus.EmitAsync("path.join.error", new Dictionary<string, object>
                    {
                        ["error"] = "At least one valid path segment is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var joinedPath = Path.Combine(pathStrings);
                var normalizedPath = Path.GetFullPath(joinedPath);

                _logger.LogInformation($"✅ Successfully joined paths: {joinedPath}");

                await _eventBus.EmitAsync("path.join.completed", new Dictionary<string, object>
                {
                    ["paths"] = pathStrings,
                    ["joinedPath"] = joinedPath,
                    ["normalizedPath"] = normalizedPath,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error joining paths: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("path.join.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }
    }
}
