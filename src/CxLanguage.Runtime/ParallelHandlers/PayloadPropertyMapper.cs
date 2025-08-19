// 🗺️ PAYLOAD PROPERTY MAPPER - RESULT AGGREGATION ENGINE
// Lead: Marcus "LocalLLM" Chen - Senior Local LLM Runtime Architect
// Issue #183: Runtime Framework - Parallel Handler Execution Engine

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CxLanguage.Runtime.ParallelHandlers
{
    /// <summary>
    /// Maps parallel handler execution results back to enhanced payload properties.
    /// Creates unified result objects with consciousness-aware property mapping.
    /// </summary>
    public class PayloadPropertyMapper
    {
        private readonly ILogger<PayloadPropertyMapper> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public PayloadPropertyMapper(ILogger<PayloadPropertyMapper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configure JSON serialization for consciousness objects
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
        
        /// <summary>
        /// Create enhanced payload by merging original payload with parallel handler results.
        /// </summary>
        /// <param name="originalPayload">Original event payload</param>
        /// <param name="handlerResults">Results from parallel handler execution</param>
        /// <returns>Enhanced payload with handler results mapped to properties</returns>
        public object CreateEnhancedPayload(object originalPayload, Dictionary<string, object> handlerResults)
        {
            if (originalPayload == null)
            {
                _logger.LogWarning("⚠️ Original payload is null, creating new enhanced payload");
                return CreateNewPayloadFromResults(handlerResults);
            }
            
            if (handlerResults == null || handlerResults.Count == 0)
            {
                _logger.LogDebug("ℹ️ No handler results to merge, returning original payload");
                return originalPayload;
            }
            
            _logger.LogDebug("🗺️ Creating enhanced payload with {ResultCount} handler results", handlerResults.Count);
            
            try
            {
                // Create enhanced payload by merging original and results
                var enhancedPayload = MergePayloadWithResults(originalPayload, handlerResults);
                
                _logger.LogInformation("✅ Enhanced payload created with {PropertyCount} total properties", 
                    GetPropertyCount(enhancedPayload));
                
                return enhancedPayload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to create enhanced payload, falling back to original");
                return originalPayload;
            }
        }
        
        /// <summary>
        /// Merge original payload with handler results using various strategies.
        /// </summary>
        private object MergePayloadWithResults(object originalPayload, Dictionary<string, object> handlerResults)
        {
            return originalPayload switch
            {
                // Dictionary-based payload (most common for CX events)
                IDictionary<string, object> dictPayload => MergeDictionaryPayload(dictPayload, handlerResults),
                
                // Anonymous object payload
                _ when IsAnonymousType(originalPayload.GetType()) => MergeAnonymousPayload(originalPayload, handlerResults),
                
                // Plain object payload
                _ => MergeObjectPayload(originalPayload, handlerResults)
            };
        }
        
        /// <summary>
        /// Merge dictionary-based payload with handler results.
        /// </summary>
        private Dictionary<string, object> MergeDictionaryPayload(
            IDictionary<string, object> originalDict, 
            Dictionary<string, object> handlerResults)
        {
            var mergedPayload = new Dictionary<string, object>(originalDict);
            
            // Add handler results as new properties
            foreach (var result in handlerResults)
            {
                var propertyName = result.Key;
                var propertyValue = ProcessResultValue(result.Value);
                
                // Handle property name conflicts
                if (mergedPayload.ContainsKey(propertyName))
                {
                    var conflictResolution = ResolvePropertyConflict(propertyName, mergedPayload[propertyName], propertyValue);
                    mergedPayload[conflictResolution.PropertyName] = conflictResolution.Value;
                    
                    if (conflictResolution.PropertyName != propertyName)
                    {
                        _logger.LogDebug("🔄 Resolved property conflict: '{OriginalName}' -> '{NewName}'", 
                            propertyName, conflictResolution.PropertyName);
                    }
                }
                else
                {
                    mergedPayload[propertyName] = propertyValue;
                }
            }
            
            // Add metadata about parallel execution
            mergedPayload["_parallelExecution"] = new
            {
                handlerCount = handlerResults.Count,
                executionMode = "parallel",
                timestamp = DateTime.UtcNow,
                resultProperties = handlerResults.Keys.ToArray()
            };
            
            return mergedPayload;
        }
        
        /// <summary>
        /// Merge anonymous object payload with handler results.
        /// </summary>
        private object MergeAnonymousPayload(object originalPayload, Dictionary<string, object> handlerResults)
        {
            // Convert anonymous object to dictionary for easier manipulation
            var originalDict = ConvertObjectToDictionary(originalPayload);
            var mergedDict = MergeDictionaryPayload(originalDict, handlerResults);
            
            // Return as ExpandoObject for dynamic property access
            var expandoObject = new System.Dynamic.ExpandoObject();
            var expandoDict = (IDictionary<string, object?>)expandoObject;
            
            foreach (var kvp in mergedDict)
            {
                expandoDict[kvp.Key] = kvp.Value;
            }
            
            return expandoObject;
        }
        
        /// <summary>
        /// Merge plain object payload with handler results.
        /// </summary>
        private object MergeObjectPayload(object originalPayload, Dictionary<string, object> handlerResults)
        {
            // For non-dictionary objects, create a wrapper object
            var wrapperPayload = new Dictionary<string, object>
            {
                ["originalPayload"] = originalPayload
            };
            
            return MergeDictionaryPayload(wrapperPayload, handlerResults);
        }
        
        /// <summary>
        /// Create new payload entirely from handler results.
        /// </summary>
        private Dictionary<string, object> CreateNewPayloadFromResults(Dictionary<string, object> handlerResults)
        {
            var payload = new Dictionary<string, object>();
            
            if (handlerResults != null)
            {
                foreach (var result in handlerResults)
                {
                    payload[result.Key] = ProcessResultValue(result.Value);
                }
            }
            
            // Add metadata
            payload["_parallelExecution"] = new
            {
                handlerCount = handlerResults?.Count ?? 0,
                executionMode = "parallel",
                timestamp = DateTime.UtcNow,
                createdFromResults = true
            };
            
            return payload;
        }
        
        /// <summary>
        /// Process handler result value for inclusion in enhanced payload.
        /// </summary>
        private object ProcessResultValue(object? resultValue)
        {
            if (resultValue == null)
                return new { value = (object?)null, type = "null" };
            
            try
            {
                // Handle different result types
                return resultValue switch
                {
                    // Primitive types
                    string or int or long or double or float or bool or DateTime => resultValue,
                    
                    // Collections
                    Array array => ProcessArrayResult(array),
                    IEnumerable<object> enumerable => ProcessEnumerableResult(enumerable),
                    
                    // Dictionaries
                    IDictionary<string, object> dict => ProcessDictionaryResult(dict),
                    
                    // Complex objects
                    _ => ProcessComplexObjectResult(resultValue)
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to process result value of type {ResultType}", 
                    resultValue.GetType().Name);
                
                return new
                {
                    value = resultValue.ToString(),
                    type = resultValue.GetType().Name,
                    processingError = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Process array result values.
        /// </summary>
        private object ProcessArrayResult(Array array)
        {
            var processedItems = new object[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                processedItems[i] = ProcessResultValue(array.GetValue(i));
            }
            
            return new
            {
                items = processedItems,
                count = array.Length,
                type = "array"
            };
        }
        
        /// <summary>
        /// Process enumerable result values.
        /// </summary>
        private object ProcessEnumerableResult(IEnumerable<object> enumerable)
        {
            var items = enumerable.Select(ProcessResultValue).ToArray();
            
            return new
            {
                items,
                count = items.Length,
                type = "enumerable"
            };
        }
        
        /// <summary>
        /// Process dictionary result values.
        /// </summary>
        private object ProcessDictionaryResult(IDictionary<string, object> dict)
        {
            var processedDict = new Dictionary<string, object>();
            
            foreach (var kvp in dict)
            {
                processedDict[kvp.Key] = ProcessResultValue(kvp.Value);
            }
            
            return processedDict;
        }
        
        /// <summary>
        /// Process complex object result values.
        /// </summary>
        private object ProcessComplexObjectResult(object complexObject)
        {
            try
            {
                // Try to serialize to JSON and back for consistent representation
                var json = JsonSerializer.Serialize(complexObject, _jsonOptions);
                var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
                
                return deserialized ?? new Dictionary<string, object> { ["value"] = complexObject.ToString() ?? "unknown", ["type"] = "complex_object" };
            }
            catch
            {
                // Fallback to property reflection
                return ConvertObjectToDictionary(complexObject);
            }
        }
        
        /// <summary>
        /// Resolve property name conflicts when merging payloads.
        /// </summary>
        private (string PropertyName, object Value) ResolvePropertyConflict(
            string originalPropertyName, 
            object originalValue, 
            object newValue)
        {
            // Strategy 1: Create array of values if they're different
            if (!ValuesAreEqual(originalValue, newValue))
            {
                return (originalPropertyName, new object[] { originalValue, newValue });
            }
            
            // Strategy 2: Keep new value if they're the same
            return (originalPropertyName, newValue);
        }
        
        /// <summary>
        /// Check if two values are equal for conflict resolution.
        /// </summary>
        private bool ValuesAreEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            
            try
            {
                // Use JSON comparison for complex objects
                var json1 = JsonSerializer.Serialize(value1, _jsonOptions);
                var json2 = JsonSerializer.Serialize(value2, _jsonOptions);
                return json1 == json2;
            }
            catch
            {
                // Fallback to ToString comparison
                return value1.ToString() == value2.ToString();
            }
        }
        
        /// <summary>
        /// Convert object to dictionary using reflection.
        /// </summary>
        private Dictionary<string, object> ConvertObjectToDictionary(object obj)
        {
            var dict = new Dictionary<string, object>();
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(obj);
                    dict[property.Name] = value ?? new object();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to get property value for {PropertyName}", property.Name);
                    dict[property.Name] = $"Error: {ex.Message}";
                }
            }
            
            return dict;
        }
        
        /// <summary>
        /// Check if type is anonymous.
        /// </summary>
        private bool IsAnonymousType(Type type)
        {
            return type.Name.Contains("AnonymousType") || 
                   (type.IsGenericType && type.Name.Contains("f__AnonymousType"));
        }
        
        /// <summary>
        /// Get property count for logging.
        /// </summary>
        private int GetPropertyCount(object obj)
        {
            return obj switch
            {
                IDictionary<string, object> dict => dict.Count,
                _ => obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Length
            };
        }
        
        /// <summary>
        /// Create diagnostic information about payload mapping.
        /// </summary>
        public PayloadMappingDiagnostics CreateDiagnostics(
            object originalPayload, 
            Dictionary<string, object> handlerResults, 
            object enhancedPayload)
        {
            return new PayloadMappingDiagnostics
            {
                OriginalPayloadType = originalPayload?.GetType().Name ?? "null",
                OriginalPropertyCount = GetPropertyCount(originalPayload ?? new object()),
                HandlerResultCount = handlerResults?.Count ?? 0,
                EnhancedPayloadType = enhancedPayload?.GetType().Name ?? "null",
                EnhancedPropertyCount = GetPropertyCount(enhancedPayload ?? new object()),
                MappingTimestamp = DateTime.UtcNow,
                HandlerResultKeys = handlerResults?.Keys.ToArray() ?? Array.Empty<string>()
            };
        }
    }
    
    /// <summary>
    /// Diagnostic information about payload mapping operations.
    /// </summary>
    public class PayloadMappingDiagnostics
    {
        public string OriginalPayloadType { get; set; } = string.Empty;
        public int OriginalPropertyCount { get; set; }
        public int HandlerResultCount { get; set; }
        public string EnhancedPayloadType { get; set; } = string.Empty;
        public int EnhancedPropertyCount { get; set; }
        public DateTime MappingTimestamp { get; set; }
        public string[] HandlerResultKeys { get; set; } = Array.Empty<string>();
    }
}

