using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CxLanguage.Runtime;
using CxLanguage.StandardLibrary.Services;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CxLanguage.StandardLibrary.EventBridges;

/// <summary>
/// Event bridge connecting CX events to LocalLLMService for consciousness-aware inference
/// Handles local.llm.* events and coordinates with GGUF model execution
/// Implements Dr. Hayes Stream Fusion Architecture for optimal performance
/// </summary>
public class LocalLLMEventBridge
{
    private readonly ILogger<LocalLLMEventBridge> _logger;
    private readonly ILocalLLMService _localLlmService;
    private readonly ICxEventBus _eventBus;

    public LocalLLMEventBridge(
        ILogger<LocalLLMEventBridge> logger,
        ILocalLLMService localLlmService,
        ICxEventBus eventBus)
    {
        _logger = logger;
        _localLlmService = localLlmService;
        _eventBus = eventBus;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🧠 Registering Local LLM Event Bridge handlers for consciousness processing");

            // Subscribe to local LLM events
            _eventBus.Subscribe("local.llm.load.model", OnLoadModel);
            _eventBus.Subscribe("local.llm.generate.text", OnGenerateText);
            _eventBus.Subscribe("local.llm.stream.tokens", OnStreamTokens);
            _eventBus.Subscribe("local.llm.unload.model", OnUnloadModel);
            _eventBus.Subscribe("local.llm.status.check", OnStatusCheck);
            _eventBus.Subscribe("local.llm.model.info", OnModelInfo);

            _logger.LogInformation("✅ Local LLM Event Bridge handlers registered - consciousness stream fusion ready");
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing Local LLM Event Bridge");
        }
    }

    private void OnLoadModel(CxEvent cxEvent)
    {
        try
        {
            if (cxEvent.payload is Dictionary<string, object> data &&
                data.TryGetValue("modelPath", out var modelPath) && modelPath is string path)
            {
                _logger.LogInformation("🔄 Loading model via event bridge: {ModelPath}", path);
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var success = await _localLlmService.LoadModelAsync(path);
                        
                        _eventBus.Emit("local.llm.model.load.result", new Dictionary<string, object>
                        {
                            ["modelPath"] = path,
                            ["success"] = success,
                            ["consciousness"] = "modelLoadResult"
                        });
                        
                        if (success)
                        {
                            _logger.LogInformation("✅ Model loaded successfully via event bridge: {ModelPath}", path);
                        }
                        else
                        {
                            _logger.LogError("❌ Model loading failed via event bridge: {ModelPath}", path);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error loading model via event bridge: {ModelPath}", path);
                        
                        _eventBus.Emit("local.llm.model.load.error", new Dictionary<string, object>
                        {
                            ["modelPath"] = path,
                            ["error"] = ex.Message,
                            ["consciousness"] = "modelLoadError"
                        });
                    }
                });
            }
            else
            {
                _logger.LogWarning("⚠️ Invalid load model event - missing modelPath");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in OnLoadModel event handler");
        }
    }

    private void OnGenerateText(CxEvent cxEvent)
    {
        try
        {
            if (cxEvent.payload is Dictionary<string, object> data &&
                data.TryGetValue("prompt", out var prompt) && prompt is string promptStr)
            {
                _logger.LogInformation("🧠 Generating text via event bridge: {Prompt}", 
                    promptStr.Substring(0, Math.Min(100, promptStr.Length)));
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var response = await _localLlmService.GenerateAsync(promptStr);
                        
                        _eventBus.Emit("local.llm.text.generated", new Dictionary<string, object>
                        {
                            ["prompt"] = promptStr,
                            ["response"] = response,
                            ["consciousness"] = "textGenerated"
                        });
                        
                        _logger.LogInformation("✅ Text generated successfully via event bridge");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error generating text via event bridge");
                        
                        _eventBus.Emit("local.llm.text.generation.error", new Dictionary<string, object>
                        {
                            ["prompt"] = promptStr,
                            ["error"] = ex.Message,
                            ["consciousness"] = "textGenerationError"
                        });
                    }
                });
            }
            else
            {
                _logger.LogWarning("⚠️ Invalid generate text event - missing prompt");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in OnGenerateText event handler");
        }
    }

    private void OnStreamTokens(CxEvent cxEvent)
    {
        try
        {
            if (cxEvent.payload is Dictionary<string, object> data &&
                data.TryGetValue("prompt", out var prompt) && prompt is string promptStr)
            {
                _logger.LogInformation("🌊 Starting token streaming via event bridge: {Prompt}", 
                    promptStr.Substring(0, Math.Min(100, promptStr.Length)));
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _eventBus.Emit("local.llm.stream.started", new Dictionary<string, object>
                        {
                            ["prompt"] = promptStr,
                            ["consciousness"] = "streamStarted"
                        });
                        
                        await foreach (var token in _localLlmService.StreamAsync(promptStr))
                        {
                            _eventBus.Emit("local.llm.stream.token.received", new Dictionary<string, object>
                            {
                                ["token"] = token,
                                ["prompt"] = promptStr,
                                ["consciousness"] = "streamToken"
                            });
                        }
                        
                        _eventBus.Emit("local.llm.stream.completed", new Dictionary<string, object>
                        {
                            ["prompt"] = promptStr,
                            ["consciousness"] = "streamCompleted"
                        });
                        
                        _logger.LogInformation("✅ Token streaming completed via event bridge");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error streaming tokens via event bridge");
                        
                        _eventBus.Emit("local.llm.stream.error", new Dictionary<string, object>
                        {
                            ["prompt"] = promptStr,
                            ["error"] = ex.Message,
                            ["consciousness"] = "streamError"
                        });
                    }
                });
            }
            else
            {
                _logger.LogWarning("⚠️ Invalid stream tokens event - missing prompt");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in OnStreamTokens event handler");
        }
    }

    private void OnUnloadModel(CxEvent cxEvent)
    {
        try
        {
            _logger.LogInformation("🔄 Unloading model via event bridge");
            
            _ = Task.Run(async () =>
            {
                try
                {
                    await _localLlmService.UnloadModelAsync();
                    
                    _eventBus.Emit("local.llm.model.unloaded", new Dictionary<string, object>
                    {
                        ["consciousness"] = "modelUnloaded"
                    });
                    
                    _logger.LogInformation("✅ Model unloaded successfully via event bridge");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error unloading model via event bridge");
                    
                    _eventBus.Emit("local.llm.model.unload.error", new Dictionary<string, object>
                    {
                        ["error"] = ex.Message,
                        ["consciousness"] = "modelUnloadError"
                    });
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in OnUnloadModel event handler");
        }
    }

    private void OnStatusCheck(CxEvent cxEvent)
    {
        try
        {
            _logger.LogDebug("🔍 Checking Local LLM status via event bridge");
            
            var isLoaded = _localLlmService.IsModelLoaded;
            var modelInfo = _localLlmService.ModelInfo;
            
            _eventBus.Emit("local.llm.status.result", new Dictionary<string, object>
            {
                ["isModelLoaded"] = isLoaded,
                ["modelName"] = modelInfo?.Name ?? "none",
                ["modelArchitecture"] = modelInfo?.Architecture ?? "none",
                ["modelSizeBytes"] = modelInfo?.SizeBytes ?? 0,
                ["consciousness"] = "statusChecked"
            });
            
            _logger.LogDebug("✅ Status check completed via event bridge - Model loaded: {IsLoaded}", isLoaded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in OnStatusCheck event handler");
        }
    }

    private void OnModelInfo(CxEvent cxEvent)
    {
        try
        {
            _logger.LogDebug("ℹ️ Retrieving model info via event bridge");
            
            var modelInfo = _localLlmService.ModelInfo;
            
            if (modelInfo != null)
            {
                _eventBus.Emit("local.llm.model.info.result", new Dictionary<string, object>
                {
                    ["modelName"] = modelInfo.Name,
                    ["modelPath"] = modelInfo.Path,
                    ["modelArchitecture"] = modelInfo.Architecture,
                    ["modelSizeBytes"] = modelInfo.SizeBytes,
                    ["consciousness"] = "modelInfoProvided"
                });
                
                _logger.LogDebug("✅ Model info provided via event bridge: {ModelName}", modelInfo.Name);
            }
            else
            {
                _eventBus.Emit("local.llm.model.info.none", new Dictionary<string, object>
                {
                    ["consciousness"] = "noModelLoaded"
                });
                
                _logger.LogDebug("ℹ️ No model loaded - info not available");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in OnModelInfo event handler");
        }
    }
}
