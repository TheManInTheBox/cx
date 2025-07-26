// 🔧 HANDLER PARAMETER RESOLVER - PARAMETER EXTRACTION ENGINE
// Lead: Marcus "LocalLLM" Chen - Senior Local LLM Runtime Architect
// Issue #183: Runtime Framework - Parallel Handler Execution Engine

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CxLanguage.Runtime.ParallelHandlers
{
    /// <summary>
    /// Resolves handler parameters from AI service calls for parallel execution.
    /// Extracts parameter name to handler event mapping from consciousness service invocations.
    /// </summary>
    public class HandlerParameterResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HandlerParameterResolver> _logger;
        
        public HandlerParameterResolver(
            IServiceProvider serviceProvider,
            ILogger<HandlerParameterResolver> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Resolve handler parameters from AI service call expression.
        /// 
        /// Example CX syntax:
        /// think { 
        ///     prompt: "Analyze data",
        ///     handlers: [
        ///         analysis: analysis.complete,
        ///         report: report.generated,
        ///         metrics: metrics.calculated
        ///     ]
        /// }
        /// 
        /// Results in:
        /// {
        ///     "analysis" => "analysis.complete",
        ///     "report" => "report.generated", 
        ///     "metrics" => "metrics.calculated"
        /// }
        /// </summary>
        /// <param name="aiServiceCall">The AI service call expression object</param>
        /// <returns>Dictionary mapping parameter names to handler event names</returns>
        public Dictionary<string, string> ResolveHandlerParameters(object aiServiceCall)
        {
            if (aiServiceCall == null)
            {
                _logger.LogWarning("⚠️ Null AI service call provided to parameter resolver");
                return new Dictionary<string, string>();
            }
            
            _logger.LogDebug("🔍 Resolving handler parameters from AI service call: {ServiceCallType}", 
                aiServiceCall.GetType().Name);
            
            var handlerParameters = new Dictionary<string, string>();
            
            try
            {
                // Extract handlers from AI service call
                var handlersProperty = GetHandlersProperty(aiServiceCall);
                if (handlersProperty == null)
                {
                    _logger.LogDebug("ℹ️ No handlers property found in AI service call");
                    return handlerParameters;
                }
                
                var handlersValue = handlersProperty.GetValue(aiServiceCall);
                if (handlersValue == null)
                {
                    _logger.LogDebug("ℹ️ Handlers property is null");
                    return handlerParameters;
                }
                
                // Process different handler formats
                handlerParameters = handlersValue switch
                {
                    // Array of handler parameter objects: [{ parameterName: "analysis", handlerName: "analysis.complete" }]
                    Array handlerArray => ProcessHandlerArray(handlerArray),
                    
                    // List of handler parameter objects
                    IEnumerable<object> handlerList => ProcessHandlerList(handlerList),
                    
                    // Dictionary format: { "analysis": "analysis.complete", "report": "report.generated" }
                    IDictionary<string, object> handlerDict => ProcessHandlerDictionary(handlerDict),
                    
                    // Single handler object
                    object singleHandler => ProcessSingleHandler(singleHandler),
                    
                    _ => new Dictionary<string, string>()
                };
                
                _logger.LogInformation("✅ Resolved {ParameterCount} handler parameters for parallel execution", 
                    handlerParameters.Count);
                
                // Log resolved parameters for debugging
                foreach (var kvp in handlerParameters)
                {
                    _logger.LogDebug("📋 Parameter '{ParameterName}' -> Handler '{HandlerName}'", 
                        kvp.Key, kvp.Value);
                }
                
                return handlerParameters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to resolve handler parameters from AI service call");
                return new Dictionary<string, string>();
            }
        }
        
        /// <summary>
        /// Find the handlers property in the AI service call object.
        /// </summary>
        private PropertyInfo? GetHandlersProperty(object aiServiceCall)
        {
            var type = aiServiceCall.GetType();
            
            // Look for common handler property names
            var handlerPropertyNames = new[] { "handlers", "Handlers", "handlerParameters", "HandlerParameters" };
            
            foreach (var propertyName in handlerPropertyNames)
            {
                var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    _logger.LogDebug("🔍 Found handlers property: {PropertyName}", propertyName);
                    return property;
                }
            }
            
            // Check for anonymous type properties using reflection
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = property.GetValue(aiServiceCall);
                if (IsHandlerCollection(value))
                {
                    _logger.LogDebug("🔍 Found handlers collection in property: {PropertyName}", property.Name);
                    return property;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if an object represents a handler collection.
        /// </summary>
        private bool IsHandlerCollection(object? value)
        {
            if (value == null) return false;
            
            return value switch
            {
                Array => true,
                IEnumerable<object> => true,
                IDictionary<string, object> => true,
                _ => false
            };
        }
        
        /// <summary>
        /// Process handler array format.
        /// </summary>
        private Dictionary<string, string> ProcessHandlerArray(Array handlerArray)
        {
            var parameters = new Dictionary<string, string>();
            
            for (int i = 0; i < handlerArray.Length; i++)
            {
                var handler = handlerArray.GetValue(i);
                if (handler != null)
                {
                    var parameterMapping = ProcessHandlerObject(handler, i);
                    foreach (var kvp in parameterMapping)
                    {
                        parameters[kvp.Key] = kvp.Value;
                    }
                }
            }
            
            return parameters;
        }
        
        /// <summary>
        /// Process handler list format.
        /// </summary>
        private Dictionary<string, string> ProcessHandlerList(IEnumerable<object> handlerList)
        {
            var parameters = new Dictionary<string, string>();
            var index = 0;
            
            foreach (var handler in handlerList)
            {
                var parameterMapping = ProcessHandlerObject(handler, index);
                foreach (var kvp in parameterMapping)
                {
                    parameters[kvp.Key] = kvp.Value;
                }
                index++;
            }
            
            return parameters;
        }
        
        /// <summary>
        /// Process handler dictionary format.
        /// </summary>
        private Dictionary<string, string> ProcessHandlerDictionary(IDictionary<string, object> handlerDict)
        {
            var parameters = new Dictionary<string, string>();
            
            foreach (var kvp in handlerDict)
            {
                if (kvp.Value != null)
                {
                    parameters[kvp.Key] = kvp.Value.ToString() ?? kvp.Key;
                }
            }
            
            return parameters;
        }
        
        /// <summary>
        /// Process single handler object.
        /// </summary>
        private Dictionary<string, string> ProcessSingleHandler(object handler)
        {
            return ProcessHandlerObject(handler, 0);
        }
        
        /// <summary>
        /// Process individual handler object to extract parameter name and handler event name.
        /// </summary>
        private Dictionary<string, string> ProcessHandlerObject(object handler, int index)
        {
            var parameters = new Dictionary<string, string>();
            
            try
            {
                if (handler is string handlerString)
                {
                    // Simple string handler: "analysis.complete"
                    var paramName = $"param_{index}";
                    parameters[paramName] = handlerString;
                    return parameters;
                }
                
                // Complex handler object with parameter name and handler name
                var handlerType = handler.GetType();
                var properties = handlerType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                string? paramName2 = null;
                string? handlerName = null;
                
                foreach (var property in properties)
                {
                    var value = property.GetValue(handler)?.ToString();
                    if (value == null) continue;
                    
                    // Look for parameter name properties
                    if (property.Name.Equals("parameterName", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Equals("parameter", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        paramName2 = value;
                    }
                    
                    // Look for handler name properties
                    if (property.Name.Equals("handlerName", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Equals("handler", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Equals("eventName", StringComparison.OrdinalIgnoreCase))
                    {
                        handlerName = value;
                    }
                }
                
                // If we found both parameter and handler names, use them
                if (!string.IsNullOrEmpty(paramName2) && !string.IsNullOrEmpty(handlerName))
                {
                    parameters[paramName2] = handlerName;
                }
                else if (!string.IsNullOrEmpty(handlerName))
                {
                    // Use handler name as both parameter and handler
                    var inferredParameterName = InferParameterName(handlerName);
                    parameters[inferredParameterName] = handlerName;
                }
                else
                {
                    // Fallback: use object string representation
                    var fallbackParameterName = $"param_{index}";
                    parameters[fallbackParameterName] = handler.ToString() ?? fallbackParameterName;
                }
                
                return parameters;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to process handler object at index {Index}", index);
                return parameters;
            }
        }
        
        /// <summary>
        /// Infer parameter name from handler event name.
        /// Examples:
        /// "analysis.complete" -> "analysis"
        /// "report.generated" -> "report"
        /// "metrics.calculated" -> "metrics"
        /// </summary>
        private string InferParameterName(string handlerName)
        {
            if (string.IsNullOrEmpty(handlerName))
                return "unknown";
            
            // Split by dot and take the first part
            var parts = handlerName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[0];
            }
            
            // If no dots, use the whole name
            return handlerName;
        }
        
        /// <summary>
        /// Validate that resolved parameters are suitable for parallel execution.
        /// </summary>
        public bool ValidateParametersForParallelExecution(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                _logger.LogWarning("⚠️ No parameters provided for validation");
                return false;
            }
            
            // Check for duplicate parameter names
            var duplicateParameters = parameters.GroupBy(kvp => kvp.Key)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            
            if (duplicateParameters.Any())
            {
                _logger.LogError("❌ Duplicate parameter names found: {DuplicateParameters}", 
                    string.Join(", ", duplicateParameters));
                return false;
            }
            
            // Check for empty or null handler names
            var invalidHandlers = parameters.Where(kvp => string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();
            
            if (invalidHandlers.Any())
            {
                _logger.LogError("❌ Invalid handler names for parameters: {InvalidParameters}", 
                    string.Join(", ", invalidHandlers));
                return false;
            }
            
            _logger.LogInformation("✅ Parameters validated for parallel execution: {ParameterCount} handlers", 
                parameters.Count);
            
            return true;
        }
    }
}
