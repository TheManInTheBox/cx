using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
            Console.WriteLine($"[DEBUG] Registered static service: {serviceName}");
        }
        
        /// <summary>
        /// Get a registered service instance
        /// </summary>
        public static object? GetService(string serviceName)
        {
            return _staticServices.TryGetValue(serviceName, out var service) ? service : null;
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
            
            // Handle array conversions for parallel operations
            if (targetType == typeof(string[]) && arg is object[] objArray)
            {
                // Convert object[] to string[] for parallel operations
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
        /// Executes a function in a background task (parallel execution).
        /// This provides the parallel keyword functionality.
        /// </summary>
        public static Task ExecuteParallel(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Start the action in a background task
            var task = Task.Run(action);
            
            // Don't wait for completion - let it run in background
            return task;
        }

        /// <summary>
        /// Executes a function in a background task and returns the result.
        /// This provides parallel execution for functions that return values.
        /// </summary>
        public static Task<T> ExecuteParallel<T>(Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            // Start the function in a background task
            return Task.Run(func);
        }

        /// <summary>
        /// Event Bus Support for CX Event-Driven Architecture
        /// </summary>
        
        /// <summary>
        /// Emit an event to the namespaced event bus
        /// </summary>
        public static void EmitEvent(string eventName, object? data = null, string source = "CxScript")
        {
            Console.WriteLine($"[DEBUG] Emitting event: {eventName} from {source}");
            NamespacedEventBusRegistry.Instance.Emit(eventName, data, source);
        }

        /// <summary>
        /// Register an event handler with the global event bus
        /// This is called by compiled 'on' statements
        /// </summary>
        public static void RegisterEventHandler(string eventName, CxEventHandler handler)
        {
            Console.WriteLine($"[DEBUG] Registering event handler for: {eventName}");
            GlobalEventBus.Instance.Subscribe(eventName, handler);
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
            Console.WriteLine($"[DEBUG] Registering instance event handler: {eventName} -> {instance.GetType().Name}.{methodName}");
            
            // Get the method from the instance type
            var instanceType = instance.GetType();
            var method = instanceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            
            if (method == null)
            {
                Console.WriteLine($"[ERROR] Event handler method {methodName} not found in {instanceType.Name}");
                return;
            }
            
            // Create a wrapper that calls the instance method
            CxEventHandler handler = async (payload) =>
            {
                try
                {
                    // Call the instance method with the payload data (not the whole payload object)
                    // The CX event handlers expect the raw data, not the CxEventPayload wrapper
                    var result = method.Invoke(instance, new[] { payload.Data });
                    
                    // If the method returns a Task, await it
                    if (result is Task task)
                    {
                        await task;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Instance event handler {instanceType.Name}.{methodName} failed: {ex.Message}");
                    Console.WriteLine($"[ERROR] Exception details: {ex}");
                }
            };
            
            // Register with the NamespacedEventBusService instead of GlobalEventBus
            // First register a temporary agent if needed
            var agentName = $"{instanceType.Name}_{instance.GetHashCode()}";
            var agentId = NamespacedEventBusRegistry.Instance.RegisterAgent(agentName);
            
            // Subscribe to the event pattern
            NamespacedEventBusRegistry.Instance.Subscribe(agentId, eventName, handler);
            Console.WriteLine($"[INFO] Instance event handler registered: {eventName} -> {instanceType.Name}.{methodName}");
        }

        /// <summary>
        /// Convert Func<object, Task> to CxEventHandler for event registration
        /// </summary>
        public static CxEventHandler ConvertToEventHandler(Func<object, Task> handler)
        {
            return async (payload) => 
            {
                try
                {
                    await handler(payload.Data ?? new object());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Event handler execution failed: {ex.Message}");
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
                return new Dictionary<string, object?>();

            if (data is Dictionary<string, object?> dict)
                return dict;

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
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Error converting property {prop.Name}: {ex.Message}");
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
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Event handler execution failed: {ex.Message}");
                throw;
            }
        }
    }
}
