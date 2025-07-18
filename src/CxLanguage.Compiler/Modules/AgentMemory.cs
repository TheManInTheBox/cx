using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CxLanguage.Core.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Memory and agent functions for Cx language
/// Provides persistent storage for agentic memory
/// </summary>
public class AgentMemory
{
    private readonly ILogger<AgentMemory> _logger;
    private readonly Dictionary<string, object> _memory = new();

    public AgentMemory(ILogger<AgentMemory> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Stores a value in the agent's memory
    /// </summary>
    public string Store(string key, object value)
    {
        try
        {
            _memory[key] = value;
            _logger.LogInformation("Stored value for key: {Key}", key);
            return $"Successfully stored value for key '{key}'";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing memory");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves a value from the agent's memory
    /// </summary>
    public object? Get(string key)
    {
        try
        {
            if (_memory.TryGetValue(key, out var value))
            {
                _logger.LogInformation("Retrieved value for key: {Key}", key);
                return value;
            }
            else
            {
                _logger.LogWarning("Key not found: {Key}", key);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memory");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Checks if a key exists in the agent's memory
    /// </summary>
    public bool Has(string key)
    {
        return _memory.ContainsKey(key);
    }

    /// <summary>
    /// Removes a value from the agent's memory
    /// </summary>
    public bool Remove(string key)
    {
        if (_memory.ContainsKey(key))
        {
            _memory.Remove(key);
            _logger.LogInformation("Removed key: {Key}", key);
            return true;
        }
        else
        {
            _logger.LogWarning("Attempt to remove non-existent key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Clears all values from the agent's memory
    /// </summary>
    public void Clear()
    {
        _memory.Clear();
        _logger.LogInformation("Cleared all memory");
    }

    /// <summary>
    /// Gets all keys in the agent's memory
    /// </summary>
    public List<string> Keys()
    {
        return new List<string>(_memory.Keys);
    }

    /// <summary>
    /// Updates a value in the agent's memory
    /// </summary>
    public string Update(string key, Func<object, object> updateFunc)
    {
        try
        {
            if (_memory.TryGetValue(key, out var value))
            {
                _memory[key] = updateFunc(value);
                _logger.LogInformation("Updated value for key: {Key}", key);
                return $"Successfully updated value for key '{key}'";
            }
            else
            {
                _logger.LogWarning("Key not found for update: {Key}", key);
                return $"Key '{key}' not found";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating memory");
            return $"Error: {ex.Message}";
        }
    }
}
