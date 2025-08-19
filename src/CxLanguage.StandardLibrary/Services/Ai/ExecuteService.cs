using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using CxLanguage.StandardLibrary.Core;
using CxLanguage.Core.Events;
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
        /// Enhanced with advanced search query capabilities
        /// </summary>
        [Description("Execute PowerShell command or search for information locally and online")]
        public async Task<ExecuteResult> ExecuteAsync(object parameters)
        {
            // Parse generic parameters into ExecuteParameters
            var executeParams = ParseExecuteParameters(parameters);
            _logger.LogInformation($"🔍 Executing: {executeParams.Command ?? executeParams.Query}");

            try
            {
                ExecuteResult result = executeParams.Type switch
                {
                    ExecuteType.PowerShell => await ExecutePowerShellCommand(executeParams),
                    ExecuteType.LocalSearch => await SearchLocalFiles(executeParams),
                    ExecuteType.OnlineSearch => await SearchOnline(executeParams),
                    ExecuteType.WebApi => await ExecuteWebApiCommand(executeParams),
                    ExecuteType.SystemInfo => await GetSystemInformation(executeParams),
                    ExecuteType.NetworkInfo => await GetNetworkInformation(executeParams),
                    ExecuteType.AdvancedSearch => await ExecuteAdvancedSearch(executeParams),
                    ExecuteType.FileContentSearch => await SearchFileContents(executeParams),
                    ExecuteType.ProcessSearch => await SearchProcesses(executeParams),
                    ExecuteType.RegistrySearch => await SearchRegistry(executeParams),
                    ExecuteType.EventLogSearch => await SearchEventLogs(executeParams),
                    _ => await ExecutePowerShellCommand(executeParams)
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
            executeParams.Url = GetProperty(props, "url") as string;
            executeParams.Method = GetProperty(props, "method") as string ?? "GET";
            executeParams.Body = GetProperty(props, "body");
            executeParams.Path = GetProperty(props, "path") as string;
            executeParams.SearchTerm = GetProperty(props, "searchTerm") as string;
            executeParams.Timeout = GetProperty(props, "timeout") as int? ?? 30000; // 30 second default
            executeParams.WorkingDirectory = GetProperty(props, "workingDirectory") as string;
            
            // Determine execute type
            if (!string.IsNullOrEmpty(executeParams.Url))
            {
                executeParams.Type = ExecuteType.WebApi;
            }
            else if (!string.IsNullOrEmpty(executeParams.Command))
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
        
        private async Task<ExecuteResult> ExecuteAdvancedSearch(ExecuteParameters parameters)
        {
            _logger.LogInformation("🔍 Executing advanced search");
            
            if (string.IsNullOrWhiteSpace(parameters.Query))
            {
                return new ExecuteResult
                {
                    Success = false,
                    Error = "Advanced search requires a query parameter",
                    Type = ExecuteType.AdvancedSearch
                };
            }
            
            var startTime = DateTime.UtcNow;
            
            // Build a more sophisticated PowerShell search command
            var command = @$"
                $searchQuery = '{parameters.Query?.Replace("'", "''")}';
                $results = @();
                
                # Build search options based on query
                $searchTerms = $searchQuery -split '\s+';
                $filterExpr = ($searchTerms | ForEach-Object {{ [regex]::Escape($_) }}) -join '|';
                
                Write-Output ""Advanced search for: $searchQuery"";
                
                # First try searching with Where-Object for flexibility
                try {{
                    # Search for files matching pattern
                    $fileResults = Get-ChildItem -Path $PWD -Recurse -File -ErrorAction SilentlyContinue | 
                        Where-Object {{ $_.Name -match $filterExpr }} | 
                        Select-Object -First 30 |
                        ForEach-Object {{
                            [PSCustomObject]@{{
                                Type = 'File';
                                Name = $_.Name;
                                Path = $_.FullName;
                                LastModified = $_.LastWriteTime;
                                Size = $_.Length;
                            }}
                        }};
                    $results += $fileResults;
                }}
                catch {{
                    Write-Output ""File search error: $($_.Exception.Message)"";
                }}
                
                # Convert results to JSON
                $results | ConvertTo-Json;
            ";
            
            var result = await powershellExecutor.ExecuteAsync(command, parameters.WorkingDirectory, parameters.Timeout);
            
            // Process the results
            var searchResults = new List<SearchResult>();
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                try
                {
                    // Parse the JSON output
                    var jsonResults = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result.Output);
                    if (jsonResults != null)
                    {
                        foreach (var item in jsonResults)
                        {
                            var searchResult = new SearchResult
                            {
                                Type = item.TryGetValue("Type", out var type) ? type.GetString() ?? "" : "",
                                Title = item.TryGetValue("Name", out var name) ? name.GetString() ?? "" : "",
                                Path = item.TryGetValue("Path", out var path) ? path.GetString() ?? "" : "",
                                Description = item.TryGetValue("Path", out _) ? item["Path"].GetString() ?? "" : ""
                            };
                            
                            if (item.TryGetValue("Size", out var size) && size.TryGetInt64(out var sizeValue))
                            {
                                searchResult.Size = sizeValue;
                            }
                            
                            if (item.TryGetValue("LastModified", out var lastMod) && lastMod.TryGetDateTime(out var dateValue))
                            {
                                searchResult.LastModified = dateValue;
                            }
                            
                            searchResults.Add(searchResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing advanced search results");
                    
                    // Fallback - just add raw output as a result
                    searchResults.Add(new SearchResult
                    {
                        Title = "Advanced Search Results",
                        Description = result.Output,
                        Type = "Raw"
                    });
                }
            }
            
            return new ExecuteResult
            {
                Success = result.ExitCode == 0,
                Output = result.Output,
                Error = result.Error,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.AdvancedSearch,
                SearchResults = searchResults.Cast<object>().ToList()
            };
        }
        
        private async Task<ExecuteResult> SearchFileContents(ExecuteParameters parameters)
        {
            _logger.LogInformation("📄 Searching file contents");
            
            if (string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                return new ExecuteResult
                {
                    Success = false,
                    Error = "File content search requires a SearchTerm parameter",
                    Type = ExecuteType.FileContentSearch
                };
            }
            
            var startTime = DateTime.UtcNow;
            var searchPath = parameters.Path ?? ".";
            var searchTerm = parameters.SearchTerm.Replace("'", "''");
            
            var command = @$"
                $searchPath = '{searchPath}';
                $searchTerm = '{searchTerm}';
                
                Write-Output ""Searching for content: $searchTerm in $searchPath"";
                
                try {{
                    $results = Get-ChildItem -Path $searchPath -Recurse -File -ErrorAction SilentlyContinue |
                        Where-Object {{ $_.Extension -match '\.txt|\.log|\.md|\.json|\.xml|\.html|\.htm|\.cs|\.js|\.ts|\.py|\.ps1|\.sh|\.bat|\.cmd|\.sql|\.csv' }} |
                        Select-Object -First 100 |
                        ForEach-Object {{
                            $file = $_;
                            $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue;
                            if ($content -match $searchTerm) {{
                                $matchLines = @();
                                $lines = $content -split ""`n"";
                                for ($i = 0; $i -lt $lines.Length; $i++) {{
                                    if ($lines[$i] -match $searchTerm) {{
                                        $lineNum = $i + 1;
                                        $matchLines += ""Line $lineNum: $($lines[$i].Trim())"";
                                        if ($matchLines.Length -ge 5) {{ break; }}
                                    }}
                                }}
                                
                                [PSCustomObject]@{{
                                    Type = 'FileContent';
                                    Name = $file.Name;
                                    Path = $file.FullName;
                                    LastModified = $file.LastWriteTime;
                                    Size = $file.Length;
                                    MatchLines = $matchLines -join ""`n"";
                                    MatchCount = ($content | Select-String -Pattern $searchTerm -AllMatches).Matches.Count;
                                }}
                            }}
                        }} | 
                        Where-Object {{ $_ -ne $null }} |
                        Select-Object -First 20;
                    
                    $results | ConvertTo-Json -Depth 3;
                }}
                catch {{
                    Write-Output ""File content search error: $($_.Exception.Message)"";
                }}
            ";
            
            var result = await powershellExecutor.ExecuteAsync(command, parameters.WorkingDirectory, parameters.Timeout);
            
            // Process the results
            var searchResults = new List<SearchResult>();
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                try
                {
                    // Parse the JSON output
                    var jsonResults = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result.Output);
                    if (jsonResults != null)
                    {
                        foreach (var item in jsonResults)
                        {
                            var matchLines = item.TryGetValue("MatchLines", out var lines) ? lines.GetString() ?? "" : "";
                            var matchCount = item.TryGetValue("MatchCount", out var count) && count.TryGetInt32(out var countValue) ? countValue : 0;
                            
                            var searchResult = new SearchResult
                            {
                                Type = "File Content Match",
                                Title = item.TryGetValue("Name", out var name) ? name.GetString() ?? "" : "",
                                Path = item.TryGetValue("Path", out var path) ? path.GetString() ?? "" : "",
                                Description = $"Found {matchCount} matches:\n{matchLines}"
                            };
                            
                            if (item.TryGetValue("Size", out var size) && size.TryGetInt64(out var sizeValue))
                            {
                                searchResult.Size = sizeValue;
                            }
                            
                            if (item.TryGetValue("LastModified", out var lastMod) && lastMod.TryGetDateTime(out var dateValue))
                            {
                                searchResult.LastModified = dateValue;
                            }
                            
                            searchResults.Add(searchResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing file content search results");
                    
                    // Fallback - just add raw output as a result
                    searchResults.Add(new SearchResult
                    {
                        Title = "File Content Search Results",
                        Description = result.Output,
                        Type = "Raw"
                    });
                }
            }
            
            return new ExecuteResult
            {
                Success = result.ExitCode == 0,
                Output = result.Output,
                Error = result.Error,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.FileContentSearch,
                SearchResults = searchResults.Cast<object>().ToList()
            };
        }
        
        private async Task<ExecuteResult> SearchProcesses(ExecuteParameters parameters)
        {
            _logger.LogInformation("🔄 Searching running processes");
            
            var startTime = DateTime.UtcNow;
            var searchTerm = parameters.SearchTerm?.Replace("'", "''") ?? "";
            
            var command = @$"
                $searchTerm = '{searchTerm}';
                
                try {{
                    $processes = Get-Process | 
                        Where-Object {{ 
                            if ([string]::IsNullOrEmpty('$searchTerm')) {{ 
                                $true 
                            }} else {{ 
                                $_.ProcessName -match $searchTerm -or $_.Id -match $searchTerm 
                            }} 
                        }} |
                        Select-Object Id, ProcessName, CPU, WorkingSet, Path, StartTime |
                        Sort-Object -Property CPU -Descending;
                    
                    $processes | ConvertTo-Json;
                }}
                catch {{
                    Write-Output ""Process search error: $($_.Exception.Message)"";
                }}
            ";
            
            var result = await powershellExecutor.ExecuteAsync(command, parameters.WorkingDirectory, parameters.Timeout);
            
            // Process the results
            var searchResults = new List<SearchResult>();
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                try
                {
                    // Parse the JSON output
                    var jsonResults = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result.Output);
                    if (jsonResults != null)
                    {
                        foreach (var item in jsonResults)
                        {
                            var id = item.TryGetValue("Id", out var idVal) && idVal.TryGetInt32(out var idValue) ? idValue : 0;
                            var cpu = item.TryGetValue("CPU", out var cpuVal) && cpuVal.TryGetDouble(out var cpuValue) ? cpuValue : 0;
                            var workingSet = item.TryGetValue("WorkingSet", out var wsVal) && wsVal.TryGetInt64(out var wsValue) ? wsValue : 0;
                            var processStartTime = item.TryGetValue("StartTime", out var stVal) && stVal.TryGetDateTime(out var stValue) ? stValue.ToString() : "Unknown";
                            
                            var searchResult = new SearchResult
                            {
                                Type = "Process",
                                Title = item.TryGetValue("ProcessName", out var name) ? name.GetString() ?? "" : "",
                                Path = item.TryGetValue("Path", out var path) ? path.GetString() ?? "" : "",
                                Description = $"PID: {id}, CPU: {cpu:F2}, Memory: {workingSet / 1024 / 1024:F2} MB, Started: {processStartTime}"
                            };
                            
                            searchResult.Size = workingSet;
                            
                            searchResults.Add(searchResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing process search results");
                    
                    // Fallback - just add raw output as a result
                    searchResults.Add(new SearchResult
                    {
                        Title = "Process Search Results",
                        Description = result.Output,
                        Type = "Raw"
                    });
                }
            }
            
            return new ExecuteResult
            {
                Success = result.ExitCode == 0,
                Output = result.Output,
                Error = result.Error,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.ProcessSearch,
                SearchResults = searchResults.Cast<object>().ToList()
            };
        }
        
        private async Task<ExecuteResult> SearchRegistry(ExecuteParameters parameters)
        {
            _logger.LogInformation("🔑 Searching registry");
            
            if (string.IsNullOrWhiteSpace(parameters.Path) && string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                return new ExecuteResult
                {
                    Success = false,
                    Error = "Registry search requires either a Path or SearchTerm parameter",
                    Type = ExecuteType.RegistrySearch
                };
            }
            
            var startTime = DateTime.UtcNow;
            var registryPath = parameters.Path ?? "HKLM:\\SOFTWARE";
            var searchTerm = parameters.SearchTerm?.Replace("'", "''") ?? "";
            
            var command = @$"
                $registryPath = '{registryPath}';
                $searchTerm = '{searchTerm}';
                
                try {{
                    # Check if the registry path exists
                    if (-not (Test-Path $registryPath)) {{
                        Write-Output ""Registry path not found: $registryPath""
                        exit 1
                    }}
                    
                    # Get registry keys and values
                    $results = @();
                    
                    # Get direct child keys first
                    $keys = Get-ChildItem -Path $registryPath -ErrorAction SilentlyContinue;
                    foreach ($key in $keys) {{
                        if ([string]::IsNullOrEmpty('$searchTerm') -or $key.PSChildName -match $searchTerm) {{
                            $results += [PSCustomObject]@{{
                                Type = 'RegistryKey';
                                Name = $key.PSChildName;
                                Path = $key.PSPath;
                                LastModified = $key.LastWriteTime;
                                ValueCount = ($key | Get-ItemProperty).PSObject.Properties.Count - 4; # Subtract default PS properties
                            }}
                        }}
                    }}
                    
                    # Get values in the current key
                    if (Test-Path $registryPath) {{
                        $values = Get-ItemProperty -Path $registryPath -ErrorAction SilentlyContinue;
                        foreach ($prop in $values.PSObject.Properties) {{
                            # Skip default PS properties
                            if ($prop.Name -notmatch '^PS(Path|ParentPath|ChildName|Provider)$') {{
                                if ([string]::IsNullOrEmpty('$searchTerm') -or $prop.Name -match $searchTerm -or $prop.Value -match $searchTerm) {{
                                    $results += [PSCustomObject]@{{
                                        Type = 'RegistryValue';
                                        Name = $prop.Name;
                                        Path = $registryPath;
                                        Value = if ($prop.Value -is [array]) {{ ""[Array with $($prop.Value.Length) items]"" }} else {{ $prop.Value.ToString() }};
                                    }}
                                }}
                            }}
                        }}
                    }}
                    
                    # Limit results to a reasonable number
                    $results = $results | Select-Object -First 50;
                    $results | ConvertTo-Json;
                }}
                catch {{
                    Write-Output ""Registry search error: $($_.Exception.Message)"";
                }}
            ";
            
            var result = await powershellExecutor.ExecuteAsync(command, parameters.WorkingDirectory, parameters.Timeout);
            
            // Process the results
            var searchResults = new List<SearchResult>();
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                try
                {
                    // Parse the JSON output
                    var jsonResults = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result.Output);
                    if (jsonResults != null)
                    {
                        foreach (var item in jsonResults)
                        {
                            var isKey = item.TryGetValue("Type", out var type) && type.GetString() == "RegistryKey";
                            var title = item.TryGetValue("Name", out var name) ? name.GetString() ?? "" : "";
                            var path = item.TryGetValue("Path", out var pathVal) ? pathVal.GetString() ?? "" : "";
                            
                            string description;
                            if (isKey)
                            {
                                var valueCount = item.TryGetValue("ValueCount", out var countVal) && countVal.TryGetInt32(out var countValue) ? countValue : 0;
                                description = $"Registry Key with {valueCount} values";
                                
                                if (item.TryGetValue("LastModified", out var lastMod) && lastMod.TryGetDateTime(out var dateValue))
                                {
                                    description += $", Last Modified: {dateValue}";
                                }
                            }
                            else
                            {
                                var value = item.TryGetValue("Value", out var valueVal) ? valueVal.GetString() ?? "" : "";
                                description = $"Value: {value}";
                            }
                            
                            var searchResult = new SearchResult
                            {
                                Type = isKey ? "Registry Key" : "Registry Value",
                                Title = title,
                                Path = path,
                                Description = description
                            };
                            
                            searchResults.Add(searchResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing registry search results");
                    
                    // Fallback - just add raw output as a result
                    searchResults.Add(new SearchResult
                    {
                        Title = "Registry Search Results",
                        Description = result.Output,
                        Type = "Raw"
                    });
                }
            }
            
            return new ExecuteResult
            {
                Success = result.ExitCode == 0,
                Output = result.Output,
                Error = result.Error,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.RegistrySearch,
                SearchResults = searchResults.Cast<object>().ToList()
            };
        }
        
        private async Task<ExecuteResult> SearchEventLogs(ExecuteParameters parameters)
        {
            _logger.LogInformation("📋 Searching Windows Event Logs");
            
            var startTime = DateTime.UtcNow;
            var logName = parameters.Path ?? "Application";
            var searchTerm = parameters.SearchTerm?.Replace("'", "''") ?? "";
            var maxEvents = 50; // Limit to avoid overwhelming results
            
            var command = @$"
                $logName = '{logName}';
                $searchTerm = '{searchTerm}';
                $maxEvents = {maxEvents};
                
                try {{
                    $filter = @{{
                        LogName = $logName
                        MaxEvents = $maxEvents
                    }};
                    
                    # Add search filters if provided
                    if (-not [string]::IsNullOrEmpty('$searchTerm')) {{
                        # Try to parse as event ID first
                        if ($searchTerm -match '^\d+$') {{
                            $filter.ID = [int]$searchTerm;
                        }}
                        else {{
                            # Otherwise search in message content
                            # This will be done after retrieving events
                        }}
                    }}
                    
                    $events = Get-WinEvent -FilterHashtable $filter -ErrorAction SilentlyContinue;
                    
                    # Filter by message content if needed
                    if (-not [string]::IsNullOrEmpty('$searchTerm') -and -not ($searchTerm -match '^\d+$')) {{
                        $events = $events | Where-Object {{ $_.Message -match $searchTerm }};
                    }}
                    
                    $results = $events | Select-Object -First $maxEvents | ForEach-Object {{
                        [PSCustomObject]@{{
                            Type = 'EventLog';
                            LogName = $_.LogName;
                            EventID = $_.Id;
                            Level = $_.LevelDisplayName;
                            TimeCreated = $_.TimeCreated;
                            Source = $_.ProviderName;
                            Message = $_.Message.Substring(0, [Math]::Min(500, $_.Message.Length)) + 
                                     (($_.Message.Length -gt 500) ? ""..."" : """");
                        }}
                    }};
                    
                    $results | ConvertTo-Json -Depth 2;
                }}
                catch {{
                    Write-Output ""Event log search error: $($_.Exception.Message)"";
                }}
            ";
            
            var result = await powershellExecutor.ExecuteAsync(command, parameters.WorkingDirectory, parameters.Timeout);
            
            // Process the results
            var searchResults = new List<SearchResult>();
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                try
                {
                    // Parse the JSON output
                    var jsonResults = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result.Output);
                    if (jsonResults != null)
                    {
                        foreach (var item in jsonResults)
                        {
                            var eventId = item.TryGetValue("EventID", out var idVal) && idVal.TryGetInt32(out var idValue) ? idValue : 0;
                            var level = item.TryGetValue("Level", out var levelVal) ? levelVal.GetString() ?? "Information" : "Information";
                            var timeCreated = item.TryGetValue("TimeCreated", out var timeVal) && timeVal.TryGetDateTime(out var timeValue) ? timeValue.ToString() : "Unknown";
                            var source = item.TryGetValue("Source", out var sourceVal) ? sourceVal.GetString() ?? "" : "";
                            var message = item.TryGetValue("Message", out var msgVal) ? msgVal.GetString() ?? "" : "";
                            var logNameResult = item.TryGetValue("LogName", out var logVal) ? logVal.GetString() ?? "" : "";
                            
                            var searchResult = new SearchResult
                            {
                                Type = "Event Log",
                                Title = $"Event ID {eventId} ({level}) from {source}",
                                Path = logNameResult,
                                Description = $"Time: {timeCreated}\nMessage: {message}"
                            };
                            
                            if (item.TryGetValue("TimeCreated", out var tcVal) && tcVal.TryGetDateTime(out var tcValue))
                            {
                                searchResult.LastModified = tcValue;
                            }
                            
                            searchResults.Add(searchResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing event log search results");
                    
                    // Fallback - just add raw output as a result
                    searchResults.Add(new SearchResult
                    {
                        Title = "Event Log Search Results",
                        Description = result.Output,
                        Type = "Raw"
                    });
                }
            }
            
            return new ExecuteResult
            {
                Success = result.ExitCode == 0,
                Output = result.Output,
                Error = result.Error,
                ExecutionTime = DateTime.UtcNow - startTime,
                Type = ExecuteType.EventLogSearch,
                SearchResults = searchResults.Cast<object>().ToList()
            };
        }
        
        /// <summary>
        /// Execute Web API command with Google Custom Search integration
        /// </summary>
        private async Task<ExecuteResult> ExecuteWebApiCommand(ExecuteParameters parameters)
        {
            _logger.LogInformation($"🌐 Performing Web API request: {parameters.Method} {parameters.Url}");
            var startTime = DateTime.UtcNow;
            
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(parameters.Timeout) };
                
                // Add standard headers
                client.DefaultRequestHeaders.Add("User-Agent", "CX-Language/1.0");
                
                HttpResponseMessage response;
                
                if (string.Equals(parameters.Method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    var jsonContent = parameters.Body != null ? 
                        JsonSerializer.Serialize(parameters.Body) : "{}";
                    using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(parameters.Url, content);
                }
                else if (string.Equals(parameters.Method, "PUT", StringComparison.OrdinalIgnoreCase))
                {
                    var jsonContent = parameters.Body != null ? 
                        JsonSerializer.Serialize(parameters.Body) : "{}";
                    using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    response = await client.PutAsync(parameters.Url, content);
                }
                else
                {
                    // GET request (default)
                    response = await client.GetAsync(parameters.Url);
                }
                
                var output = await response.Content.ReadAsStringAsync();
                
                return new ExecuteResult
                {
                    Success = response.IsSuccessStatusCode,
                    Output = output,
                    Error = response.IsSuccessStatusCode ? null : $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                    ExitCode = response.IsSuccessStatusCode ? 0 : (int)response.StatusCode,
                    ExecutionTime = DateTime.UtcNow - startTime,
                    Type = ExecuteType.WebApi
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error executing Web API command");
                return new ExecuteResult
                {
                    Success = false,
                    Error = ex.Message,
                    ExitCode = -1,
                    ExecutionTime = DateTime.UtcNow - startTime,
                    Type = ExecuteType.WebApi
                };
            }
        }
        
        public class ExecuteParameters
        {
            public string? Command { get; set; }
            public string? Query { get; set; }
            public string? Url { get; set; }  // For Web API calls
            public string Method { get; set; } = "GET";
            public object? Body { get; set; }
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
            WebApi,  // Direct HTTP-based execution
            SystemInfo,
            NetworkInfo,
            AdvancedSearch,
            FileContentSearch,
            ProcessSearch,
            RegistrySearch,
            EventLogSearch
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

