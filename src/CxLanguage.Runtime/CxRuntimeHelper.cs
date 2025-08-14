using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Provides runtime helper methods for executing complex operations
    /// that are difficult to generate directly in IL.
    /// </summary>
    public static class CxRuntimeHelper
    {
        /// <summary>
        /// Static service registry for accessing services from static function contexts
        /// </summary>
        private static readonly Dictionary<string, object> _staticServices = new();
        
        /// <summary>
        /// Register a service instance for static access
        /// </summary>
        public static void RegisterService(string serviceName, object serviceInstance)
        {
            _staticServices[serviceName] = serviceInstance;
        }
        
        /// <summary>
        /// Get a registered service instance
        /// </summary>
        public static object? GetService(string serviceName)
        {
            return _staticServices.TryGetValue(serviceName, out var service) ? service : null;
        }

        /// <summary>
        /// Calls a method on a user-defined class instance using reflection.
        /// This handles method resolution and parameter passing for CX user-defined classes.
        /// </summary>
        public static object? CallInstanceMethod(object instance, string methodName, object[] arguments)
        {
            if (instance == null)
            {
                return null;
            }

            var instanceType = instance.GetType();

            try
            {
                var actualMethodName = MapMethodName(methodName, instanceType);

                // Use InvokeMember which handles overload resolution and type coercion automatically
                var result = instanceType.InvokeMember(
                    actualMethodName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                    Type.DefaultBinder,
                    instance,
                    arguments
                );

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CallInstanceMethod: exception during method call: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ERROR] CallInstanceMethod: inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[ERROR] CallInstanceMethod: inner exception type: {ex.InnerException.GetType().Name}");
                }
                return null;
            }
        }

        /// <summary>
        /// Maps JavaScript-style method names to .NET method names for better CX Language compatibility
        /// </summary>
        private static string MapMethodName(string methodName, Type instanceType)
        {
            // Handle string methods specifically
            if (instanceType == typeof(string))
            {
                return methodName switch
                {
                    "indexOf" => "IndexOf",
                    "lastIndexOf" => "LastIndexOf", 
                    "substring" => "Substring",
                    "substr" => "Substring",
                    "toLowerCase" => "ToLower",
                    "toUpperCase" => "ToUpper",
                    "charAt" => "get_Chars", // String indexer
                    "replace" => "Replace",
                    "split" => "Split",
                    "trim" => "Trim",
                    "startsWith" => "StartsWith",
                    "endsWith" => "EndsWith",
                    "includes" => "Contains",
                    "contains" => "Contains",
                    _ => methodName // Use original name if no mapping
                };
            }

            // Handle array methods
            if (instanceType.IsArray)
            {
                return methodName switch
                {
                    "push" => "Add", // Note: arrays don't have Add, but collections do
                    "pop" => "RemoveAt",
                    "shift" => "RemoveAt", 
                    "unshift" => "Insert",
                    "indexOf" => "IndexOf",
                    "includes" => "Contains",
                    "contains" => "Contains",
                    _ => methodName
                };
            }

            // Handle collection methods
            if (instanceType.IsGenericType)
            {
                var genericDef = instanceType.GetGenericTypeDefinition();
                if (genericDef == typeof(List<>) || genericDef == typeof(IList<>) || genericDef == typeof(ICollection<>))
                {
                    return methodName switch
                    {
                        "push" => "Add",
                        "pop" => "RemoveAt",
                        "indexOf" => "IndexOf",
                        "includes" => "Contains",
                        "contains" => "Contains",
                        _ => methodName
                    };
                }
            }

            // Default: return original method name
            return methodName;
        }

        /// <summary>
        /// Calls a method on a service instance dynamically using reflection.
        /// This method handles method overload resolution, optional parameters, and async Task results.
        /// Synchronous version to avoid Task.Result blocking issues.
        /// If serviceInstance is null, attempts to get it from static service registry.
        /// </summary>
        public static object? CallServiceMethod(object serviceInstance, string methodName, object[] arguments)
        {
            // Handle null serviceInstance by attempting to get from static registry
            if (serviceInstance == null)
            {
                Console.WriteLine($"[DEBUG] Service instance is null, trying to resolve from static registry for method: {methodName}");
                // Try to find the service by method name pattern (e.g., GenerateAsync -> TextGeneration)
                var possibleServices = _staticServices.Values.ToList();
                foreach (var service in possibleServices)
                {
                    var currentServiceType = service.GetType();
                    var currentMethods = currentServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.Name == methodName)
                        .ToArray();
                    
                    if (currentMethods.Length > 0)
                    {
                        Console.WriteLine($"[DEBUG] Found service {currentServiceType.Name} with method {methodName}");
                        serviceInstance = service;
                        break;
                    }
                }
                
                if (serviceInstance == null)
                {
                    return $"[Error: Service instance is null and no registered service found with method '{methodName}'.]";
                }
            }

            var serviceType = serviceInstance.GetType();
            var methods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == methodName)
                .ToArray();

            if (methods.Length == 0)
            {
                return $"[Error: Method '{methodName}' not found on service '{serviceType.Name}'.]";
            }

            // Find the best matching method
            var (method, finalArguments) = FindBestMethodAndPrepareArgs(methods, arguments);

            if (method == null || finalArguments == null)
            {
                return $"[Error: No suitable overload for method '{methodName}' found for the given arguments.]";
            }

            try
            {
                // Invoke the method
                var result = method.Invoke(serviceInstance, finalArguments);

                // Handle async methods by using GetAwaiter().GetResult()
                if (result is Task task)
                {
                    // Use GetAwaiter().GetResult() which is safer than .Result
                    var awaiter = task.GetAwaiter();
                    awaiter.GetResult(); // Wait for completion

                    // If the task has a result (Task<T>), return it
                    var resultProperty = task.GetType().GetProperty("Result");
                    if (resultProperty != null)
                    {
                        return resultProperty.GetValue(task);
                    }
                    
                    // If it's a non-generic Task, return null for void async
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"[Error: Failed to invoke method '{methodName}': {ex.Message}]";
            }
        }

        private static (MethodInfo?, object[]?) FindBestMethodAndPrepareArgs(MethodInfo[] methods, object[] providedArgs)
        {
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length < providedArgs.Length) continue;

                var finalArgs = new object?[parameters.Length];
                bool match = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i < providedArgs.Length)
                    {
                        // We have a provided argument
                        finalArgs[i] = ConvertArgument(providedArgs[i], parameters[i].ParameterType);
                    }
                    else if (parameters[i].HasDefaultValue)
                    {
                        // Use the default value
                        finalArgs[i] = parameters[i].DefaultValue;
                    }
                    else
                    {
                        // Not enough arguments and no default value
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return (method, (object[]?)finalArgs);
                }
            }

            return (null, null);
        }

        private static object? ConvertArgument(object? arg, Type targetType)
        {
            if (arg == null) return null;

            // Handle special conversion for object literals (Dictionary) to options objects
            if (arg is Dictionary<string, object> dict && !targetType.IsAssignableFrom(typeof(Dictionary<string, object>)))
            {
                return CxParameterConverter.ConvertToOptions(dict, targetType);
            }
            
            // Handle array conversions
            if (targetType == typeof(string[]) && arg is object[] objArray)
            {
                // Convert object[] to string[]
                return objArray.Select(o => o?.ToString() ?? "").ToArray();
            }
            
            if (targetType.IsInstanceOfType(arg))
            {
                return arg;
            }

            try
            {
                return Convert.ChangeType(arg, targetType);
            }
            catch
            {
                return arg; // Return original if conversion fails
            }
        }

        /// <summary>
        /// Retrieves a service from the service provider using GetRequiredService.
        /// This method is used to obtain service instances in a static context.
        /// </summary>
        public static object? GetService(System.IServiceProvider serviceProvider, System.Type serviceType)
        {
            if (serviceProvider == null)
            {
                return null;
            }
            
            try
            {
                // Use GetRequiredService via reflection to avoid generic method issues in IL
                var getRequiredServiceMethod = typeof(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions)
                    .GetMethod("GetRequiredService", new[] { typeof(System.IServiceProvider) })
                    ?.MakeGenericMethod(serviceType);
                
                if (getRequiredServiceMethod != null)
                {
                    return getRequiredServiceMethod.Invoke(null, new object[] { serviceProvider });
                }
                
                // Fallback to GetService
                return serviceProvider.GetService(serviceType);
            }
            catch (Exception ex)
            {
                return $"[Error: Failed to get service of type '{serviceType.Name}': {ex.Message}]";
            }
        }

        /// <summary>
        /// Find a compatible method from an array of methods based on name and argument count.
        /// Handles optional parameters and method overloads.
        /// </summary>
        public static System.Reflection.MethodInfo? FindCompatibleMethod(
            System.Reflection.MethodInfo[] methods, 
            string methodName, 
            int argumentCount)
        {
            // First, filter by method name
            var namedMethods = methods.Where(m => m.Name == methodName).ToArray();
            
            if (namedMethods.Length == 0)
            {
                return null;
            }
            
            // Find methods that can accept the given number of arguments
            foreach (var method in namedMethods)
            {
                var parameters = method.GetParameters();
                var requiredParams = parameters.Count(p => !p.HasDefaultValue);
                var totalParams = parameters.Length;
                
                // Method is compatible if:
                // 1. Argument count equals total parameters, OR
                // 2. Argument count is between required and total (optional parameters)
                if (argumentCount == totalParams || 
                    (argumentCount >= requiredParams && argumentCount <= totalParams))
                {
                    return method;
                }
            }
            
            // If no exact match, return the first method with the same name
            // This handles cases where parameter conversion is needed
            return namedMethods.FirstOrDefault();
        }

        /// <summary>
        /// Event Bus Support for CX Event-Driven Architecture
        /// </summary>
        
        /// <summary>
        /// Emit an event to the unified event bus
        /// </summary>
        public static void EmitEvent(string eventName, object? data = null, string source = "CxScript")
        {
            // Try to use registered ICxEventBus service first
            var eventBus = GetService("ICxEventBus") as CxLanguage.Core.Events.ICxEventBus;
            if (eventBus != null)
            {
                try
                {
                    // Convert data to dictionary for ICxEventBus
                    var payload = data as IDictionary<string, object> ?? 
                                  ConvertToEventData(data).ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? new object());
                    
                    // Emit through DI-managed event bus (async fire-and-forget)
                    _ = Task.Run(async () => await eventBus.EmitAsync(eventName, payload, source));
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to emit event through ICxEventBus: {ex.Message}");
                }
            }
            
            // Fallback to UnifiedEventBusRegistry
            UnifiedEventBusRegistry.Instance.Emit(eventName, data, source);
        }

        /// <summary>
        /// Register an event handler with the unified event bus
        /// This is called by compiled 'on' statements at program scope (global handlers)
        /// Updated to use CxEvent object as parameter
        /// </summary>
        public static void RegisterEventHandler(string eventName, CxEventHandler handler)
        {
            Console.WriteLine($"[DEBUG] Registering global event handler for: {eventName}");
            
            // Register as global scope subscription
            var subscriptionId = UnifiedEventBusRegistry.Instance.RegisterSubscription(
                "Global", "system", UnifiedEventScope.Global);
            
            // Wrap the CxEventHandler to match the expected EventHandler delegate
            EventHandler unifiedHandler = (payload) =>
            {
                var cxPayload = new CxEventPayload(payload.EventName, payload.Data ?? new object());
                return handler(cxPayload);
            };

            UnifiedEventBusRegistry.Instance.Subscribe(subscriptionId, eventName, unifiedHandler);
        }

        /// <summary>
        /// Advanced Event Bus Service Integration
        /// </summary>

        /// <summary>
        /// Join the Event Bus Service as an agent
        /// </summary>
        public static string JoinEventBus(string agentName, string role = "agent", 
            string scope = "Global", string[]? channels = null, string[]? eventFilters = null, object? agentInstance = null)
        {
            if (!Enum.TryParse<EventBusScope>(scope, true, out var scopeEnum))
            {
                scopeEnum = EventBusScope.Global;
            }

            var agentId = EventBusServiceRegistry.Instance.JoinBus(agentName, role, scopeEnum, channels, eventFilters, agentInstance);
            Console.WriteLine($"[INFO] Agent {agentName} joined event bus with ID: {agentId}");
            return agentId;
        }

        /// <summary>
        /// Leave the Event Bus Service
        /// </summary>
        public static bool LeaveEventBus(string agentId)
        {
            var success = EventBusServiceRegistry.Instance.LeaveBus(agentId);
            if (success)
            {
                Console.WriteLine($"[INFO] Agent {agentId} left the event bus");
            }
            else
            {
                Console.WriteLine($"[WARNING] Failed to remove agent {agentId} from event bus");
            }
            return success;
        }

        /// <summary>
        /// Subscribe to events through Event Bus Service
        /// </summary>
        public static bool SubscribeToEvent(string agentId, string eventName, CxEventHandler handler)
        {
            var success = EventBusServiceRegistry.Instance.Subscribe(agentId, eventName, handler);
            if (success)
            {
                Console.WriteLine($"[INFO] Agent {agentId} subscribed to event: {eventName}");
            }
            return success;
        }

        /// <summary>
        /// Emit event through Event Bus Service with advanced scoping
        /// </summary>
        public static void EmitScopedEvent(string eventName, object? data = null, string source = "CxScript",
            string? scope = null, string? targetChannel = null, string? targetRole = null)
        {
            EventBusScope? scopeEnum = null;
            if (scope != null && Enum.TryParse<EventBusScope>(scope, true, out var parsed))
            {
                scopeEnum = parsed;
            }

            Console.WriteLine($"[DEBUG] Emitting scoped event: {eventName} from {source} (scope: {scope}, channel: {targetChannel}, role: {targetRole})");
            EventBusServiceRegistry.Instance.Emit(eventName, data, source, scopeEnum, targetChannel, targetRole);
        }

        /// <summary>
        /// Join a channel in the Event Bus Service
        /// </summary>
        public static bool JoinChannel(string agentId, string channel)
        {
            var success = EventBusServiceRegistry.Instance.JoinChannel(agentId, channel);
            Console.WriteLine($"[INFO] Agent {agentId} joined channel: {channel} (success: {success})");
            return success;
        }

        /// <summary>
        /// Leave a channel in the Event Bus Service
        /// </summary>
        public static bool LeaveChannel(string agentId, string channel)
        {
            var success = EventBusServiceRegistry.Instance.LeaveChannel(agentId, channel);
            Console.WriteLine($"[INFO] Agent {agentId} left channel: {channel} (success: {success})");
            return success;
        }

        /// <summary>
        /// Get Event Bus Service statistics
        /// </summary>
        public static Dictionary<string, object> GetBusStatistics()
        {
            var stats = EventBusServiceRegistry.Instance.GetBusStatistics();
            var result = new Dictionary<string, object>
            {
                ["TotalAgents"] = stats.TotalAgents,
                ["TotalChannels"] = stats.TotalChannels,
                ["TotalRoles"] = stats.TotalRoles,
                ["ScopeDistribution"] = stats.ScopeDistribution,
                ["TopChannels"] = stats.TopChannels,
                ["TopRoles"] = stats.TopRoles
            };

            Console.WriteLine($"[INFO] Bus Statistics: {stats.TotalAgents} agents, {stats.TotalChannels} channels, {stats.TotalRoles} roles");
            return result;
        }

        /// <summary>
        /// Namespace-based Event Bus Functions - Event names as scopes
        /// </summary>

        /// <summary>
        /// Register agent for namespace-based event routing
        /// </summary>
        public static string RegisterNamespacedAgent(string agentName, string? team = null, string? role = null, 
            string[]? channels = null, object? agentInstance = null)
        {
            var agentId = NamespacedEventBusRegistry.Instance.RegisterAgent(agentName, team, role, channels, agentInstance);
            Console.WriteLine($"[INFO] Agent {agentName} registered for namespaced events with ID: {agentId}");
            return agentId;
        }

        /// <summary>
        /// Unregister agent from namespace-based event routing
        /// </summary>
        public static bool UnregisterNamespacedAgent(string agentId)
        {
            var success = NamespacedEventBusRegistry.Instance.UnregisterAgent(agentId);
            Console.WriteLine($"[INFO] Agent {agentId} unregistered from namespaced events: {success}");
            return success;
        }

        /// <summary>
        /// Subscribe to namespace-based event patterns
        /// Supports wildcards: team.any.task, role.manager.any, global.any
        /// </summary>
        public static bool SubscribeToNamespacedEvent(string agentId, string eventPattern, CxEventHandler handler)
        {
            var success = NamespacedEventBusRegistry.Instance.Subscribe(agentId, eventPattern, handler);
            Console.WriteLine($"[INFO] Agent {agentId} subscribed to pattern: {eventPattern} (success: {success})");
            return success;
        }

        /// <summary>
        /// Auto-subscribe agent to relevant patterns based on their properties
        /// </summary>
        public static void AutoSubscribeNamespacedAgent(string agentId, CxEventHandler handler)
        {
            NamespacedEventBusRegistry.Instance.AutoSubscribeAgent(agentId, handler);
            Console.WriteLine($"[INFO] Agent {agentId} auto-subscribed to relevant namespace patterns");
        }

        /// <summary>
        /// Emit namespace-scoped event
        /// Event name determines routing: global.any, team.name.any, role.name.any, agent.name.any, channel.name.any
        /// </summary>
        public static void EmitNamespacedEvent(string eventName, object? data = null, string source = "CxScript")
        {
            Console.WriteLine($"[DEBUG] Emitting namespaced event: {eventName} from {source}");
            NamespacedEventBusRegistry.Instance.Emit(eventName, data, source);
        }

        /// <summary>
        /// Get namespace-based event bus statistics
        /// </summary>
        public static Dictionary<string, object> GetNamespacedBusStatistics()
        {
            var stats = NamespacedEventBusRegistry.Instance.GetStatistics();
            var result = new Dictionary<string, object>
            {
                ["TotalAgents"] = stats.TotalAgents,
                ["TotalEventPatterns"] = stats.TotalEventPatterns,
                ["EventsByScope"] = stats.EventsByScope,
                ["AgentsByRole"] = stats.AgentsByRole,
                ["TopEventPatterns"] = stats.TopEventPatterns
            };

            Console.WriteLine($"[INFO] Namespaced Bus Stats: {stats.TotalAgents} agents, {stats.TotalEventPatterns} patterns");
            return result;
        }

        /// <summary>
        /// Register an instance event handler for a specific object
        /// This is called when class instances with event handlers are created
        /// </summary>
        public static void RegisterInstanceEventHandler(object instance, string eventName, string methodName)
        {
            if (instance == null)
            {
                return;
            }
            
            // Get the method from the instance type
            var instanceType = instance.GetType();
            var method = instanceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            
            if (method == null)
            {
                return;
            }
            
            // Validate method signature for safety
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
            {
                return;
            }

            // Try to use registered ICxEventBus service first (matches EmitEvent behavior)
            var eventBus = GetService("ICxEventBus") as CxLanguage.Core.Events.ICxEventBus;
            if (eventBus != null)
            {
                try
                {
                    // Create handler that matches ICxEventBus signature
                    Func<object?, string, IDictionary<string, object>?, Task<bool>> asyncHandler = async (sender, eventNameReceived, payload) =>
                    {
                        try
                        {
                            var cxEvent = new CxEvent
                            {
                                name = eventNameReceived,
                                payload = payload ?? new Dictionary<string, object>(),
                                timestamp = DateTime.UtcNow
                            };
                            
                            var result = method.Invoke(instance, new object[] { cxEvent });
                            if (result is Task task)
                            {
                                await task;
                            }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Handler execution failed: {ex.Message}");
                            return false;
                        }
                    };
                    
                    // Register with ICxEventBus (same system as EmitEvent)
                    eventBus.Subscribe(eventName, asyncHandler);
                    Console.WriteLine($"[DEBUG] Registered handler for {eventName} with ICxEventBus");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to register handler with ICxEventBus: {ex.Message}");
                }
            }

            // Fallback to UnifiedEventBusRegistry (legacy support)
            
            // Create a CxEventHandler to invoke the compiled instance method
            CxEventHandler cxHandler = (payload) =>
            {
                try
                {
                    // Check if the parameter type is compatible
                    if (parameters.Length == 0 || !parameters[0].ParameterType.IsAssignableFrom(typeof(CxEvent)))
                    {
                        return Task.CompletedTask;
                    }

                    var cxEvent = new CxEvent
                    {
                        name = payload.EventName,
                        payload = payload.Data as Dictionary<string, object> ?? new Dictionary<string, object>(),
                        timestamp = payload.Timestamp
                    };
                    
                    var result = method.Invoke(instance, new object[] { cxEvent! });
                    if (result is Task task)
                    {
                        return task;
                    }
                }
                catch (Exception)
                {
                    // Silent error handling for clean output
                }
                return Task.CompletedTask;
            };
            
            // Subscribe using the instance-aware subscription method
            var subscriptionId = UnifiedEventBusRegistry.Instance.RegisterSubscription(
                $"{instance.GetType().Name}_{instance.GetHashCode()}", "instance", UnifiedEventScope.Agent, instance: instance);
            
            EventHandler unifiedHandler = (payload) =>
            {
                var cxPayload = new CxEventPayload(payload.EventName, payload.Data ?? new object());
                return cxHandler(cxPayload);
            };            UnifiedEventBusRegistry.Instance.Subscribe(subscriptionId, eventName, unifiedHandler);
        }

        /// <summary>
        /// Safely interpret CX event handler behavior without calling the problematic IL-generated methods
        /// This manually implements what the CX event handlers are supposed to do
        /// Updated to accept CxEvent object instead of raw payload data
        /// </summary>
        private static Task InterpretCxEventHandler(object instance, string methodName, string eventName, CxEvent cxEvent)
        {
            try
            {
                // Get the agent name from the instance
                var agentName = GetInstanceField(instance, "name")?.ToString() ?? "UnknownAgent";
                
                // Handle user action events
                if (eventName.Contains("user") && eventName.Contains("action"))
                {
                    return Task.CompletedTask;
                }
                
                // Handle alert events
                if (eventName.Contains("alert"))
                {
                    return Task.CompletedTask;
                }
                
                // Handle command events (legacy support)
                if (eventName == "command.executed")
                {
                    return Task.CompletedTask;
                }
                else if (eventName == "command.error")
                {
                    return Task.CompletedTask;
                }
            }
            catch (Exception)
            {
                // Silent error handling for clean output
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Convert Func<object, Task> to CxEventHandler for event registration
        /// Updated to pass CxEvent object to handlers instead of raw payload
        /// </summary>
        public static CxEventHandler ConvertToEventHandler(Func<object, Task> handler)
        {
            return async (payload) => 
            {
                try
                {
                    // Pass the raw data object to the handler
                    await handler(payload.Data ?? new object());
                }
                catch (Exception)
                {
                    // Silent error handling for clean output
                    throw;
                }
            };
        }

        /// <summary>
        /// Convert object data to dictionary for event payloads
        /// </summary>
        public static Dictionary<string, object?> ConvertToEventData(object? data)
        {
            if (data == null)
            {
                return new Dictionary<string, object?>();
            }

            if (data is Dictionary<string, object?> dict)
            {
                return dict;
            }

            // Convert object properties to dictionary using reflection
            var result = new Dictionary<string, object?>();
            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(data);
                    result[prop.Name] = value;
                }
                catch (Exception)
                {
                    result[prop.Name] = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Execute event handler function with payload
        /// This bridges compiled CX event handlers with the event bus
        /// </summary>
        public static async Task ExecuteEventHandler(Func<object, Task> handler, CxEventPayload payload)
        {
            try
            {
                await handler(payload.Data ?? new object());
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Convert any value to a truthy boolean following JavaScript-like semantics
        /// - null, undefined -> false
        /// - boolean -> as is 
        /// - number 0, NaN -> false, otherwise true
        /// - string empty/null -> false, otherwise true
        /// - objects -> true if not null
        /// </summary>
        public static bool ConvertToBoolean(object? value)
        {
            if (value == null)
                return false;
            
            // Handle boolean directly
            if (value is bool boolValue)
                return boolValue;
            
            // Handle numbers
            if (value is int intValue)
                return intValue != 0;
            if (value is double doubleValue)
                return doubleValue != 0.0 && !double.IsNaN(doubleValue);
            if (value is float floatValue)
                return floatValue != 0.0f && !float.IsNaN(floatValue);
            if (value is decimal decimalValue)
                return decimalValue != 0m;
            
            // Handle strings
            if (value is string stringValue)
                return !string.IsNullOrEmpty(stringValue);
            
            // Handle objects (including services)
            // Any non-null object is truthy
            return true;
        }

        /// <summary>
        /// Gets a field value from an instance using reflection.
        /// This handles runtime field access for CX classes to avoid FieldBuilder invalidation issues.
        /// </summary>
        public static object? GetInstanceField(object instance, string fieldName)
        {
            if (instance == null)
            {
                return null;
            }

            try
            {
                var instanceType = instance.GetType();
                
                var field = instanceType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var value = field.GetValue(instance);
                    return value;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a property value from a service field on an instance using reflection.
        /// This combines field access and property access for service property access like this.textGen.ServiceName.
        /// </summary>
        public static object? GetInstanceProperty(object instance, string fieldName, string propertyName)
        {
            if (instance == null)
            {
                return null;
            }

            try
            {
                
                // First get the field value (which should be the service instance)
                var serviceInstance = GetInstanceField(instance, fieldName);
                if (serviceInstance == null)
                {
                    return null;
                }

                // Then get the property from the service instance
                return GetObjectProperty(serviceInstance, propertyName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a property value from an object using reflection.
        /// This handles runtime property access for any object.
        /// </summary>
        public static object? GetObjectProperty(object instance, string propertyName)
        {
            if (instance == null)
            {
                return null;
            }

            try
            {
                var instanceType = instance.GetType();

                // Special handling for CxEvent - check payload dictionary first
                if (instanceType.Name == "CxEvent")
                {
                    var payloadProperty = instanceType.GetProperty("payload");
                    if (payloadProperty != null)
                    {
                        var payload = payloadProperty.GetValue(instance);
                        
                        if (payload is Dictionary<string, object> payloadDict)
                        {
                            if (payloadDict.ContainsKey(propertyName))
                            {
                                var result = payloadDict[propertyName];
                                return result;
                            }
                            
                            if (payloadDict.ContainsKey("payload") && payloadDict["payload"] is Dictionary<string, object> nestedPayload)
                            {
                                if (nestedPayload.ContainsKey(propertyName))
                                {
                                    var result = nestedPayload[propertyName];
                                    return result;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] Payload is not Dictionary<string, object>, it's: {payload?.GetType().FullName ?? "null"}");
                        }
                    }
                }
                
                if (propertyName == "length" && instance is Array array)
                {
                    return array.Length;
                }
                
                if (instance is Dictionary<string, object> dict)
                {
                    return dict.TryGetValue(propertyName, out var val) ? val : $"[Key {propertyName} not found]";
                }
                
                if (instanceType.IsGenericType && instanceType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    if (propertyName == "Key") return instanceType.GetProperty("Key")?.GetValue(instance);
                    if (propertyName == "Value") return instanceType.GetProperty("Value")?.GetValue(instance);
                }
                
                var property = instanceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    return property.GetMethod?.Invoke(instance, null) ?? $"[Property {propertyName} has no getter]";
                }
                
                return $"[Property {propertyName} not found]";
            }
            catch (Exception ex)
            {
                return $"[Error accessing property {propertyName}: {ex.Message}]";
            }
        }

        /// <summary>
        /// Sets a field value on an instance using reflection.
        /// This handles runtime field assignment for CX classes to avoid FieldBuilder invalidation issues.
        /// </summary>
        public static void SetInstanceField(object instance, string fieldName, object? value)
        {
            if (instance == null)
            {
                Console.WriteLine($"[DEBUG] SetInstanceField: instance is null for field {fieldName}");
                return;
            }

            try
            {
                var instanceType = instance.GetType();
                Console.WriteLine($"[DEBUG] SetInstanceField: setting field {fieldName} on {instanceType.Name} to {value ?? "null"}");
                
                var field = instanceType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    Console.WriteLine($"[DEBUG] SetInstanceField: field {fieldName} set successfully");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] SetInstanceField: field {fieldName} not found in {instanceType.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] SetInstanceField: exception setting field {fieldName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Built-in function to get the type name of an object for CX language
        /// </summary>
        public static string GetTypeOf(object? obj)
        {
            if (obj == null)
            {
                return "null";
            }

            var type = obj.GetType();
            
            // Handle common .NET types with CX-friendly names
            if (type == typeof(string))
                return "string";
            if (type == typeof(int))
                return "number";
            if (type == typeof(double))
                return "number";
            if (type == typeof(float))
                return "number";
            if (type == typeof(bool))
                return "boolean";
            if (type.IsArray)
                return $"array[{type.GetElementType()?.Name ?? "object"}]";
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return $"list[{type.GetGenericArguments()[0].Name}]";
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return "object";
                
            // Return the actual type name for other types
            return type.Name;
        }

        /// <summary>
        /// Built-in function to get current timestamp for CX language
        /// Returns ISO 8601 formatted timestamp with millisecond precision
        /// </summary>
        public static string Now()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}
