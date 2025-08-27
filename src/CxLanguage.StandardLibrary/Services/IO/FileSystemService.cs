using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;
using CxLanguage.StandardLibrary.Core;

namespace CxLanguage.StandardLibrary.Services.IO
{
    /// <summary>
    /// CX Language File System Service - Core file operations with consciousness integration
    /// Part of the Standard Library I/O Module
    /// </summary>
    public class FileSystemService : ModernAiServiceBase
    {
        private readonly ICxEventBus _eventBus;

        public FileSystemService(IServiceProvider serviceProvider, ICxEventBus eventBus, ILogger<FileSystemService> logger) 
            : base(serviceProvider, logger)
        {
            _eventBus = eventBus;
            _logger.LogInformation("🗂️ FileSystemService initialized with consciousness integration");
            
            // NO AUTO HANDLERS - All handlers must be explicitly declared in CX programs
        }

        private async Task HandleFileReadRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var encoding = data?.GetValueOrDefault("encoding")?.ToString() ?? "utf-8";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"📖 Reading file: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ File path is required for read operation");
                    await _eventBus.EmitAsync("file.read.error", new Dictionary<string, object>
                    {
                        ["error"] = "File path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (!File.Exists(path))
                {
                    _logger.LogError($"❌ File not found: {path}");
                    await _eventBus.EmitAsync("file.read.error", new Dictionary<string, object>
                    {
                        ["error"] = $"File not found: {path}",
                        ["path"] = path,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var content = await File.ReadAllTextAsync(path);
                _logger.LogInformation($"✅ Successfully read {content.Length} characters from {path}");

                await _eventBus.EmitAsync("file.read.completed", new Dictionary<string, object>
                {
                    ["content"] = content,
                    ["path"] = path,
                    ["encoding"] = encoding,
                    ["size"] = content.Length,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error reading file: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("file.read.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleFileWriteRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var content = data?.GetValueOrDefault("content")?.ToString();
                var encoding = data?.GetValueOrDefault("encoding")?.ToString() ?? "utf-8";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"✏️ Writing to file: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ File path is required for write operation");
                    await _eventBus.EmitAsync("file.write.error", new Dictionary<string, object>
                    {
                        ["error"] = "File path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (content == null)
                {
                    _logger.LogError("❌ Content is required for write operation");
                    await _eventBus.EmitAsync("file.write.error", new Dictionary<string, object>
                    {
                        ["error"] = "Content is required",
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

                await File.WriteAllTextAsync(path, content);
                _logger.LogInformation($"✅ Successfully wrote {content.Length} characters to {path}");

                await _eventBus.EmitAsync("file.write.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["encoding"] = encoding,
                    ["size"] = content.Length,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error writing file: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("file.write.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleFileExistsRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"🔍 Checking if file exists: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ File path is required for exists check");
                    await _eventBus.EmitAsync("file.exists.error", new Dictionary<string, object>
                    {
                        ["error"] = "File path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var exists = File.Exists(path);
                _logger.LogInformation($"✅ File exists check: {path} = {exists}");

                await _eventBus.EmitAsync("file.exists.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["exists"] = exists,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking file existence: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("file.exists.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleFileDeleteRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"🗑️ Deleting file: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ File path is required for delete operation");
                    await _eventBus.EmitAsync("file.delete.error", new Dictionary<string, object>
                    {
                        ["error"] = "File path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (!File.Exists(path))
                {
                    _logger.LogError($"❌ File not found for deletion: {path}");
                    await _eventBus.EmitAsync("file.delete.error", new Dictionary<string, object>
                    {
                        ["error"] = $"File not found: {path}",
                        ["path"] = path,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                File.Delete(path);
                _logger.LogInformation($"✅ Successfully deleted file: {path}");

                await _eventBus.EmitAsync("file.delete.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting file: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("file.delete.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleFileCopyRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var sourcePath = data?.GetValueOrDefault("sourcePath")?.ToString();
                var destinationPath = data?.GetValueOrDefault("destinationPath")?.ToString();
                var overwrite = data?.GetValueOrDefault("overwrite")?.ToString()?.ToLower() == "true";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"📋 Copying file: {sourcePath} → {destinationPath}");

                if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
                {
                    _logger.LogError("❌ Source and destination paths are required for copy operation");
                    await _eventBus.EmitAsync("file.copy.error", new Dictionary<string, object>
                    {
                        ["error"] = "Source and destination paths are required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (!File.Exists(sourcePath))
                {
                    _logger.LogError($"❌ Source file not found: {sourcePath}");
                    await _eventBus.EmitAsync("file.copy.error", new Dictionary<string, object>
                    {
                        ["error"] = $"Source file not found: {sourcePath}",
                        ["sourcePath"] = sourcePath,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                // Ensure destination directory exists
                var directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation($"📁 Created directory: {directory}");
                }

                File.Copy(sourcePath, destinationPath, overwrite);
                _logger.LogInformation($"✅ Successfully copied file: {sourcePath} → {destinationPath}");

                await _eventBus.EmitAsync("file.copy.completed", new Dictionary<string, object>
                {
                    ["sourcePath"] = sourcePath,
                    ["destinationPath"] = destinationPath,
                    ["overwrite"] = overwrite,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error copying file: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("file.copy.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleFileMoveRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var sourcePath = data?.GetValueOrDefault("sourcePath")?.ToString();
                var destinationPath = data?.GetValueOrDefault("destinationPath")?.ToString();
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"🚚 Moving file: {sourcePath} → {destinationPath}");

                if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
                {
                    _logger.LogError("❌ Source and destination paths are required for move operation");
                    await _eventBus.EmitAsync("file.move.error", new Dictionary<string, object>
                    {
                        ["error"] = "Source and destination paths are required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (!File.Exists(sourcePath))
                {
                    _logger.LogError($"❌ Source file not found: {sourcePath}");
                    await _eventBus.EmitAsync("file.move.error", new Dictionary<string, object>
                    {
                        ["error"] = $"Source file not found: {sourcePath}",
                        ["sourcePath"] = sourcePath,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                // Ensure destination directory exists
                var directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation($"📁 Created directory: {directory}");
                }

                File.Move(sourcePath, destinationPath);
                _logger.LogInformation($"✅ Successfully moved file: {sourcePath} → {destinationPath}");

                await _eventBus.EmitAsync("file.move.completed", new Dictionary<string, object>
                {
                    ["sourcePath"] = sourcePath,
                    ["destinationPath"] = destinationPath,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error moving file: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("file.move.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleFileAppendRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var content = data?.GetValueOrDefault("content")?.ToString();
                var encoding = data?.GetValueOrDefault("encoding")?.ToString() ?? "utf-8";
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"➕ Appending to file: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ File path is required for append operation");
                    await _eventBus.EmitAsync("file.append.error", new Dictionary<string, object>
                    {
                        ["error"] = "File path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (content == null)
                {
                    _logger.LogError("❌ Content is required for append operation");
                    await _eventBus.EmitAsync("file.append.error", new Dictionary<string, object>
                    {
                        ["error"] = "Content is required",
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

                await File.AppendAllTextAsync(path, content);
                _logger.LogInformation($"✅ Successfully appended {content.Length} characters to {path}");

                await _eventBus.EmitAsync("file.append.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["encoding"] = encoding,
                    ["size"] = content.Length,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error appending to file: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("file.append.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }

        private async Task HandleFileInfoRequest(CxEventPayload eventPayload)
        {
            try
            {
                var data = eventPayload.Data as Dictionary<string, object>;
                var path = data?.GetValueOrDefault("path")?.ToString();
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                _logger.LogInformation($"ℹ️ Getting file info: {path}");

                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("❌ File path is required for info operation");
                    await _eventBus.EmitAsync("file.info.error", new Dictionary<string, object>
                    {
                        ["error"] = "File path is required",
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                if (!File.Exists(path))
                {
                    _logger.LogError($"❌ File not found: {path}");
                    await _eventBus.EmitAsync("file.info.error", new Dictionary<string, object>
                    {
                        ["error"] = $"File not found: {path}",
                        ["path"] = path,
                        ["requestId"] = requestId,
                        ["timestamp"] = DateTime.UtcNow
                    });
                    return;
                }

                var fileInfo = new FileInfo(path);
                _logger.LogInformation($"✅ Successfully retrieved file info for: {path}");

                await _eventBus.EmitAsync("file.info.completed", new Dictionary<string, object>
                {
                    ["path"] = path,
                    ["name"] = fileInfo.Name,
                    ["fullName"] = fileInfo.FullName,
                    ["directoryName"] = fileInfo.DirectoryName ?? "",
                    ["extension"] = fileInfo.Extension,
                    ["size"] = fileInfo.Length,
                    ["created"] = fileInfo.CreationTimeUtc,
                    ["modified"] = fileInfo.LastWriteTimeUtc,
                    ["accessed"] = fileInfo.LastAccessTimeUtc,
                    ["isReadOnly"] = fileInfo.IsReadOnly,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting file info: {ex.Message}");
                var data = eventPayload.Data as Dictionary<string, object>;
                var requestId = data?.GetValueOrDefault("requestId")?.ToString() ?? Guid.NewGuid().ToString();

                await _eventBus.EmitAsync("file.info.error", new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["requestId"] = requestId,
                    ["timestamp"] = DateTime.UtcNow
                });
            }
        }
    }
}
