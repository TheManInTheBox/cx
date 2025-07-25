using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CxLanguage.StandardLibrary.Core;
using CxLanguage.Runtime;
using System.ComponentModel;
using System.Linq;

namespace CxLanguage.StandardLibrary
{
    /// <summary>
    /// CX Execute Service - PowerShell Integration for Dynamic Information Gathering
    /// Enables consciousness entities to search for new knowledge locally and online
    /// </summary>
    public class ExecuteService : ModernAiServiceBase
    {
        private readonly PowerShellExecutor powershellExecutor;
        private readonly SearchEngine searchEngine;
        private readonly LocalFileSearcher localSearcher;
        
        public ExecuteService(IServiceProvider serviceProvider, ILogger<ExecuteService> logger)
            : base(serviceProvider, logger)
        {
            powershellExecutor = new PowerShellExecutor();
            searchEngine = new SearchEngine();
            localSearcher = new LocalFileSearcher();
            
            _logger.LogInformation("🔍 Execute Service initialized for dynamic knowledge gathering");
        }
        
        /// <summary>
        /// Gets the service name
        /// </summary>
        public override string ServiceName => "Execute";
        
        /// <summary>
        /// Gets the service version
        /// </summary>
        public override string Version => "1.0.0";
        
        /// <summary>
        /// Execute PowerShell command or search query from CX Language
        /// </summary>
        [Description("Execute PowerShell command or search for information locally and online")]
        public async Task<ExecuteResult> ExecuteAsync(
            [Description("Command to execute or search query")] object parameters)
        {
            try
            {
                var executeParams = ParseExecuteParameters(parameters);
                _logger.LogInformation($"🔍 Executing: {executeParams.Command ?? executeParams.Query}");
                
                ExecuteResult result = executeParams.Type switch
                {
                    ExecuteType.PowerShell => await ExecutePowerShellCommand(executeParams),
                    ExecuteType.LocalSearch => await SearchLocalFiles(executeParams),
                    ExecuteType.OnlineSearch => await SearchOnline(executeParams),
                    ExecuteType.SystemInfo => await GetSystemInformation(executeParams),
                    ExecuteType.NetworkInfo => await GetNetworkInformation(executeParams),
                    _ => await ExecutePowerShellCommand(executeParams) // Default
                };
                
                _logger.LogInformation($"✅ Execute complete: {result.Output?.Length ?? 0} characters returned");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Execute service error");
                return new ExecuteResult
                {
                    Success = false,
                    Error = ex.Message,
                    ExecutionTime = TimeSpan.Zero
                };
            }
        }
        
        private ExecuteParameters ParseExecuteParameters(object parameters)
        {
            var executeParams = new ExecuteParameters();
            
            if (parameters is string directCommand)
            {
                executeParams.Command = directCommand;
                executeParams.Type = ExecuteType.PowerShell;
                return executeParams;
            }
            
            var props = GetProperties(parameters);
            
            executeParams.Command = GetProperty(props, "command") as string;
            executeParams.Query = GetProperty(props, "query") as string;
            executeParams.Path = GetProperty(props, "path") as string;
            executeParams.SearchTerm = GetProperty(props, "searchTerm") as string;
            executeParams.Timeout = GetProperty(props, "timeout") as int? ?? 30000; // 30 second default
            executeParams.WorkingDirectory = GetProperty(props, "workingDirectory") as string;
            
            // Determine execute type
            if (!string.IsNullOrEmpty(executeParams.Command))
            {
                executeParams.Type = ExecuteType.PowerShell;
            }
            else if (!string.IsNullOrEmpty(executeParams.Query))
            {
                executeParams.Type = executeParams.Query.StartsWith("http") ? 
                    ExecuteType.OnlineSearch : ExecuteType.LocalSearch;
            }
            else if (!string.IsNullOrEmpty(executeParams.SearchTerm))
            {
                executeParams.Type = ExecuteType.LocalSearch;
            }
            else
            {
                executeParams.Type = ExecuteType.SystemInfo;
            }
            
            return executeParams;
        }
        
        /// <summary>
        /// Helper to get properties from an object
        /// </summary>
        private Dictionary<string, object> GetProperties(object obj)
        {
            var result = new Dictionary<string, object>();
            
            if (obj == null) return result;
            
            foreach (var prop in obj.GetType().GetProperties())
            {
                var value = prop.GetValue(obj);
                if (value != null)
                {
                    result[prop.Name] = value;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Helper to get a property value
        /// </summary>
        private object? GetProperty(Dictionary<string, object> props, string name)
        {
            return props.ContainsKey(name) ? props[name] : null;
        }
        
        private async Task<ExecuteResult> ExecutePowerShellCommand(ExecuteParameters parameters)
        {
            _logger.LogInformation($"⚡ Executing PowerShell: {parameters.Command}");
            
            var startTime = DateTime.UtcNow;
            var result = await powershellExecutor.ExecuteAsync(
                parameters.Command ?? string.Empty, 
                parameters.WorkingDirectory,
                parameters.Timeout);
            
            return new ExecuteResult
            {
                Success = result.ExitCode == 0,
                Output = result.Output,
                Error = result.Error,
                ExitCode = result.ExitCode,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.PowerShell
            };
        }
        
        private async Task<ExecuteResult> SearchLocalFiles(ExecuteParameters parameters)
        {
            _logger.LogInformation($"📁 Searching local files: {parameters.Query ?? parameters.SearchTerm}");
            
            var startTime = DateTime.UtcNow;
            var searchTerm = parameters.Query ?? parameters.SearchTerm ?? string.Empty;
            var searchPath = parameters.Path ?? Environment.CurrentDirectory;
            
            var results = await localSearcher.SearchAsync(searchTerm, searchPath);
            var output = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            
            return new ExecuteResult
            {
                Success = true,
                Output = output,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.LocalSearch,
                SearchResults = results.Cast<object>().ToList()
            };
        }
        
        private async Task<ExecuteResult> SearchOnline(ExecuteParameters parameters)
        {
            _logger.LogInformation($"🌐 Searching online: {parameters.Query}");
            
            var startTime = DateTime.UtcNow;
            var results = await searchEngine.SearchAsync(parameters.Query ?? string.Empty);
            var output = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            
            return new ExecuteResult
            {
                Success = true,
                Output = output,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.OnlineSearch,
                SearchResults = results.Cast<object>().ToList()
            };
        }
        
        private Task<ExecuteResult> GetSystemInformation(ExecuteParameters parameters)
        {
            _logger.LogInformation("💻 Gathering system information");
            
            var startTime = DateTime.UtcNow;
            var systemInfo = new
            {
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                OSVersion = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                WorkingMemory = GC.GetTotalMemory(false),
                CurrentDirectory = Environment.CurrentDirectory,
                RuntimeVersion = Environment.Version.ToString(),
                CommandLine = Environment.CommandLine
            };
            
            var output = JsonSerializer.Serialize(systemInfo, new JsonSerializerOptions { WriteIndented = true });
            
            return Task.FromResult(new ExecuteResult
            {
                Success = true,
                Output = output,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.SystemInfo
            });
        }
        
        private async Task<ExecuteResult> GetNetworkInformation(ExecuteParameters parameters)
        {
            _logger.LogInformation("🌐 Gathering network information");
            
            var startTime = DateTime.UtcNow;
            var result = await powershellExecutor.ExecuteAsync(
                "Get-NetIPConfiguration | ConvertTo-Json", 
                null, 
                parameters.Timeout);
            
            return new ExecuteResult
            {
                Success = result.ExitCode == 0,
                Output = result.Output,
                Error = result.Error,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.NetworkInfo
            };
        }
        
        public class ExecuteParameters
        {
            public string? Command { get; set; }
            public string? Query { get; set; }
            public string? Path { get; set; }
            public string? SearchTerm { get; set; }
            public int Timeout { get; set; } = 30000;
            public string? WorkingDirectory { get; set; }
            public ExecuteType Type { get; set; }
        }
        
        public class ExecuteResult
        {
            public bool Success { get; set; }
            public string? Output { get; set; }
            public string? Error { get; set; }
            public int ExitCode { get; set; }
            public TimeSpan ExecutionTime { get; set; }
            public ExecuteType Type { get; set; }
            public List<object>? SearchResults { get; set; }
        }
        
        public enum ExecuteType
        {
            PowerShell,
            LocalSearch,
            OnlineSearch,
            SystemInfo,
            NetworkInfo
        }
    }
    
    /// <summary>
    /// PowerShell command executor
    /// </summary>
    public class PowerShellExecutor
    {
        public async Task<PowerShellResult> ExecuteAsync(string command, string? workingDirectory = null, int timeoutMs = 30000)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "pwsh.exe",
                Arguments = $"-Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
            };
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            
            using var process = new Process { StartInfo = processInfo };
            
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };
            
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
            var completed = process.HasExited;
            
            if (!completed)
            {
                process.Kill();
                return new PowerShellResult
                {
                    Output = outputBuilder.ToString(),
                    Error = "Command timed out",
                    ExitCode = -1
                };
            }
            
            return new PowerShellResult
            {
                Output = outputBuilder.ToString(),
                Error = errorBuilder.ToString(),
                ExitCode = process.ExitCode
            };
        }
        
        public class PowerShellResult
        {
            public string Output { get; set; } = "";
            public string Error { get; set; } = "";
            public int ExitCode { get; set; }
        }
    }
    
    /// <summary>
    /// Local file search engine
    /// </summary>
    public class LocalFileSearcher
    {
        public async Task<List<SearchResult>> SearchAsync(string searchTerm, string searchPath)
        {
            var results = new List<SearchResult>();
            
            await Task.Run(() =>
            {
                try
                {
                    var files = Directory.GetFiles(searchPath, "*.*", SearchOption.AllDirectories)
                        .Where(f => Path.GetFileName(f).ToLower().Contains(searchTerm.ToLower()))
                        .Take(50); // Limit results
                    
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        results.Add(new SearchResult
                        {
                            Title = Path.GetFileName(file),
                            Path = file,
                            Size = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime,
                            Type = "File"
                        });
                    }
                    
                    // Search within file contents for text files
                    var textFiles = Directory.GetFiles(searchPath, "*.txt", SearchOption.AllDirectories)
                        .Concat(Directory.GetFiles(searchPath, "*.cs", SearchOption.AllDirectories))
                        .Concat(Directory.GetFiles(searchPath, "*.cx", SearchOption.AllDirectories))
                        .Take(20);
                    
                    foreach (var file in textFiles)
                    {
                        try
                        {
                            var content = File.ReadAllText(file);
                            if (content.ToLower().Contains(searchTerm.ToLower()))
                            {
                                results.Add(new SearchResult
                                {
                                    Title = $"Content match in {Path.GetFileName(file)}",
                                    Path = file,
                                    Description = GetContentSnippet(content, searchTerm),
                                    Type = "Content"
                                });
                            }
                        }
                        catch
                        {
                            // Skip files that can't be read
                        }
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new SearchResult
                    {
                        Title = "Search Error",
                        Description = ex.Message,
                        Type = "Error"
                    });
                }
            });
            
            return results;
        }
        
        private string GetContentSnippet(string content, string searchTerm)
        {
            var index = content.ToLower().IndexOf(searchTerm.ToLower());
            if (index == -1) return "";
            
            var start = Math.Max(0, index - 50);
            var length = Math.Min(100, content.Length - start);
            
            return content.Substring(start, length).Replace("\n", " ").Replace("\r", "");
        }
    }
    
    /// <summary>
    /// Online search engine (uses PowerShell to call web APIs)
    /// </summary>
    public class SearchEngine
    {
        private readonly PowerShellExecutor executor = new();
        
        public async Task<List<SearchResult>> SearchAsync(string query)
        {
            var results = new List<SearchResult>();
            
            // Use PowerShell to search GitHub repositories
            var gitHubCommand = $@"
                try {{
                    $response = Invoke-RestMethod -Uri 'https://api.github.com/search/repositories?q={Uri.EscapeDataString(query)}&sort=updated&per_page=5' -Headers @{{'User-Agent'='CX-Language'}}
                    $response.items | ForEach-Object {{
                        [PSCustomObject]@{{
                            title = $_.name
                            description = $_.description
                            url = $_.html_url
                            language = $_.language
                            updated = $_.updated_at
                        }}
                    }} | ConvertTo-Json
                }} catch {{
                    Write-Output 'GitHub API unavailable'
                }}
            ";
            
            var gitHubResult = await executor.ExecuteAsync(gitHubCommand);
            
            if (gitHubResult.ExitCode == 0 && !string.IsNullOrEmpty(gitHubResult.Output))
            {
                try
                {
                    var gitHubData = JsonSerializer.Deserialize<List<GitHubRepo>>(gitHubResult.Output);
                    foreach (var repo in gitHubData ?? new List<GitHubRepo>())
                    {
                        results.Add(new SearchResult
                        {
                            Title = repo.title ?? "Unknown",
                            Description = repo.description ?? "",
                            Url = repo.url ?? "",
                            Type = "GitHub Repository",
                            Language = repo.language ?? ""
                        });
                    }
                }
                catch
                {
                    // Fallback if JSON parsing fails
                    results.Add(new SearchResult
                    {
                        Title = "GitHub Search",
                        Description = gitHubResult.Output,
                        Type = "Raw"
                    });
                }
            }
            
            return results;
        }
        
        private class GitHubRepo
        {
            public string? title { get; set; }
            public string? description { get; set; }
            public string? url { get; set; }
            public string? language { get; set; }
            public string? updated { get; set; }
        }
    }
    
    /// <summary>
    /// Search result data structure
    /// </summary>
    public class SearchResult
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Path { get; set; } = "";
        public string Url { get; set; } = "";
        public string Type { get; set; } = "";
        public string Language { get; set; } = "";
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }
}
