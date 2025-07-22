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
        /// Calls a method on a user-defined class instance using reflection.
        /// This handles method resolution and parameter passing for CX user-defined classes.
        /// </summary>
        public static object? CallInstanceMethod(object instance, string methodName, object[] arguments)
        {
            if (instance == null)
            {
                Console.WriteLine($"[DEBUG] CallInstanceMethod: instance is null for method {methodName}");
                return null;
            }

            var instanceType = instance.GetType();
            Console.WriteLine($"[DEBUG] CallInstanceMethod: calling {methodName} on {instanceType.Name}");

            try
            {
                // Find methods with the given name
                var methods = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == methodName)
                    .ToArray();

                if (methods.Length == 0)
                {
                    Console.WriteLine($"[DEBUG] CallInstanceMethod: method {methodName} not found on {instanceType.Name}");
                    return null;
                }

                // Find the best matching method based on parameter count
                var method = methods.FirstOrDefault(m => m.GetParameters().Length == arguments.Length);
                if (method == null)
                {
                    // Try to find a method where we can fill in default values for optional parameters
                    method = methods.FirstOrDefault(m => 
                    {
                        var parameters = m.GetParameters();
                        // We can use this method if we have at least the required parameters
                        // and the remaining parameters have default values
                        if (parameters.Length >= arguments.Length)
                        {
                            // Check if all parameters beyond our argument count have default values
                            for (int i = arguments.Length; i < parameters.Length; i++)
                            {
                                if (!parameters[i].HasDefaultValue)
                                    return false;
                            }
                            return true;
                        }
                        return false;
                    });
                    
                    if (method != null)
                    {
                        Console.WriteLine($"[DEBUG] CallInstanceMethod: using method {method.Name} with {method.GetParameters().Length} parameters (padding optional params)");
                        
                        // Pad arguments with default values for optional parameters
                        var parameters = method.GetParameters();
                        var paddedArguments = new object[parameters.Length];
                        
                        // Copy provided arguments
                        for (int i = 0; i < arguments.Length; i++)
                        {
                            paddedArguments[i] = arguments[i];
                        }
                        
                        // Fill in default values for remaining parameters
                        for (int i = arguments.Length; i < parameters.Length; i++)
                        {
                            paddedArguments[i] = parameters[i].DefaultValue ?? Type.Missing;
                        }
                        
                        arguments = paddedArguments;
                    }
                    else
                    {
                        // Fallback: try the first method regardless of parameter count (original behavior)
                        method = methods[0];
                        Console.WriteLine($"[DEBUG] CallInstanceMethod: using first method {method.Name} with {method.GetParameters().Length} parameters (fallback)");
                    }
                }

                Console.WriteLine($"[DEBUG] CallInstanceMethod: invoking {method.Name} on {instanceType.Name}");
                var result = method.Invoke(instance, arguments);
                Console.WriteLine($"[DEBUG] CallInstanceMethod: method invoked successfully, result: {result ?? "null"}");
                Console.WriteLine($"[DEBUG] CallInstanceMethod: result type: {result?.GetType()?.FullName ?? "null"}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] CallInstanceMethod: exception during method call: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[DEBUG] CallInstanceMethod: inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[DEBUG] CallInstanceMethod: inner exception type: {ex.InnerException.GetType().Name}");
                    if (ex.InnerException.StackTrace != null)
                    {
                        Console.WriteLine($"[DEBUG] CallInstanceMethod: inner stack trace:\n{ex.InnerException.StackTrace}");
                    }
                }
                return null;
            }
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
            Console.WriteLine($"[DEBUG] Emitting event: {eventName} from {source}");
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
            
            // Create Action<CxEvent> to invoke the compiled static handler directly
            Action<CxEvent> action = (cxEvent) =>
            {
                try
                {
                    // Call the compiled handler, passing the structured CxEvent with proper payload
                    var task = handler(new CxEventPayload
                    {
                        EventName = cxEvent.name,
                        Data = cxEvent,  // This passes the CxEvent itself, which has the payload
                        Timestamp = cxEvent.timestamp,
                        Source = "Global"
                    });
                    task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Global handler execution failed: {ex.Message}");
                }
            };
            
            // Subscribe using ICxEventBus to pass structured CxEvent to the handler
            if (UnifiedEventBusRegistry.Instance is ICxEventBus bus)
            {
                bus.Subscribe(eventName, action);
            }
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
        Console.WriteLine($"[DEBUG] RegisterInstanceEventHandler called: {eventName} -> {instance?.GetType().Name ?? "null"}.{methodName}");
        
        if (instance == null)
        {
            Console.WriteLine($"[ERROR] Instance is null for event handler registration");
            return;
        }
        
        // Get the method from the instance type
        var instanceType = instance.GetType();
        Console.WriteLine($"[DEBUG] Looking for method {methodName} in type {instanceType.FullName}");
        
        var method = instanceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        
        if (method == null)
        {
            Console.WriteLine($"[ERROR] Event handler method {methodName} not found in {instanceType.Name}");
            // Try to list all available methods for debugging
            var allMethods = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            Console.WriteLine($"[DEBUG] Available methods in {instanceType.Name}:");
            foreach (var m in allMethods.Take(10))
            {
                Console.WriteLine($"[DEBUG]   - {m.Name}({string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
            }
            return;
        }            // Validate method signature for safety
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
            {
                Console.WriteLine($"[ERROR] Event handler method {methodName} must have exactly 1 parameter, found {parameters.Length}");
                return;
            }
            
            // Create an Action<CxEvent> to invoke the compiled instance method
            Action<CxEvent> cxHandler = (cxEvent) =>
            {
                Console.WriteLine($"[DEBUG] ACTION ENTRY: Handler {methodName} called with CxEvent: {cxEvent?.name ?? "null"}");
                try
                {
                    Console.WriteLine($"[DEBUG] Invoking instance method {methodName} with CxEvent: {cxEvent?.name ?? "null"}");
                    Console.WriteLine($"[DEBUG] Method parameter type: {parameters[0].ParameterType.FullName}");
                    Console.WriteLine($"[DEBUG] CxEvent type: {cxEvent?.GetType().FullName ?? "null"}");
                    Console.WriteLine($"[DEBUG] Instance type: {instance.GetType().FullName}");
                    Console.WriteLine($"[DEBUG] Method info: {method.DeclaringType?.FullName}.{method.Name}");
                    Console.WriteLine($"[DEBUG] Method is static: {method.IsStatic}");
                    Console.WriteLine($"[DEBUG] Parameter count: {parameters.Length}");
                    
                    // Check if the parameter type is compatible
                    if (!parameters[0].ParameterType.IsAssignableFrom(typeof(CxEvent)))
                    {
                        Console.WriteLine($"[ERROR] Parameter type mismatch: expected {parameters[0].ParameterType.FullName}, got {typeof(CxEvent).FullName}");
                        return;
                    }
                    
                    method.Invoke(instance, new object[] { cxEvent! });
                    Console.WriteLine($"[DEBUG] Instance method {methodName} invoked successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Instance handler {methodName} execution failed: {ex.Message}");
                    Console.WriteLine($"[ERROR] Exception type: {ex.GetType().FullName}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[ERROR] Inner exception: {ex.InnerException.Message}");
                        Console.WriteLine($"[ERROR] Inner exception type: {ex.InnerException.GetType().FullName}");
                        if (ex.InnerException.StackTrace != null)
                        {
                            Console.WriteLine($"[ERROR] Inner stack trace: {ex.InnerException.StackTrace}");
                        }
                    }
                }
            };
            
            // Subscribe using ICxEventBus to pass structured CxEvent to the handler
            if (UnifiedEventBusRegistry.Instance is ICxEventBus bus)
            {
                bus.Subscribe(eventName, instance, cxHandler);
                Console.WriteLine($"[INFO] Instance event handler registered: {eventName} -> {instanceType.Name}.{methodName}");
            }
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
                Console.WriteLine($"[DEBUG] Interpreting CX event handler {methodName} for event {eventName}");
                
                // Get the agent name from the instance
                var agentName = GetInstanceField(instance, "name")?.ToString() ?? "UnknownAgent";
                Console.WriteLine($"[DEBUG] Agent name: {agentName}");
                Console.WriteLine($"[DEBUG] Event object: name={cxEvent.name}, timestamp={cxEvent.timestamp}");
                Console.WriteLine($"[DEBUG] Payload data: {cxEvent.payload ?? "null"}");
                
                // Handle user action events
                if (eventName.Contains("user") && eventName.Contains("action"))
                {
                    Console.WriteLine($"🎯 {agentName} AGENT RESPONSE: User action detected - Processing user request");
                    Console.WriteLine($"📅 Event timestamp: {cxEvent.timestamp}");
                    return Task.CompletedTask;
                }
                
                // Handle alert events
                if (eventName.Contains("alert"))
                {
                    Console.WriteLine($"🚨 {agentName} AGENT RESPONSE: Alert detected - Taking defensive action");
                    Console.WriteLine($"📅 Event timestamp: {cxEvent.timestamp}");
                    return Task.CompletedTask;
                }
                
                // Handle command events (legacy support)
                if (eventName == "command.executed")
                {
                    Console.WriteLine($"✅ {agentName} COMMAND RESPONSE: Command completed successfully");
                    Console.WriteLine($"📅 Event timestamp: {cxEvent.timestamp}");
                    return Task.CompletedTask;
                }
                else if (eventName == "command.error")
                {
                    Console.WriteLine($"❌ {agentName} COMMAND RESPONSE: Command failed");
                    Console.WriteLine($"📅 Event timestamp: {cxEvent.timestamp}");
                    return Task.CompletedTask;
                }
                
                // Default handler for other events
                Console.WriteLine($"[DEBUG] {agentName} handled event: {eventName} at {cxEvent.timestamp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CX event handler interpretation failed: {ex.Message}");
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
                    Console.WriteLine($"[DEBUG] ConvertToEventHandler: payload.EventName = {payload.EventName}");
                    Console.WriteLine($"[DEBUG] ConvertToEventHandler: payload.Data type = {payload.Data?.GetType().FullName ?? "null"}");
                    Console.WriteLine($"[DEBUG] ConvertToEventHandler: handler type = {handler.GetType().FullName}");
                    Console.WriteLine($"[DEBUG] ConvertToEventHandler: handler target = {handler.Target?.GetType().FullName ?? "null"}");
                    
                    // Create CxEvent object from the payload
                    // Convert raw data object to dictionary for structured payload
                    var eventData = ConvertToEventData(payload.Data);
                    var cxEvent = new CxEvent
                    {
                        name = payload.EventName,
                        payload = eventData,
                        timestamp = payload.Timestamp
                    };
                    
                    Console.WriteLine($"[DEBUG] ConvertToEventHandler: About to call handler with cxEvent: {cxEvent.name}");
                    // Pass the CxEvent object to the compiled handler
                    await handler(cxEvent);
                    Console.WriteLine($"[DEBUG] ConvertToEventHandler: Handler completed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Event handler execution failed: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[ERROR] Inner exception: {ex.InnerException.Message}");
                        Console.WriteLine($"[ERROR] Inner exception type: {ex.InnerException.GetType().FullName}");
                    }
                    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                    throw;
                }
            };
        }

        /// <summary>
        /// Convert object data to dictionary for event payloads
        /// </summary>
        public static Dictionary<string, object?> ConvertToEventData(object? data)
        {
            Console.WriteLine($"[DEBUG] ConvertToEventData: input data type = {data?.GetType().FullName ?? "null"}");
            Console.WriteLine($"[DEBUG] ConvertToEventData: input data value = {data}");
            
            if (data == null)
            {
                Console.WriteLine("[DEBUG] ConvertToEventData: data is null, returning empty dictionary");
                return new Dictionary<string, object?>();
            }

            if (data is Dictionary<string, object?> dict)
            {
                Console.WriteLine($"[DEBUG] ConvertToEventData: data is already Dictionary with {dict.Count} entries");
                foreach (var kvp in dict)
                {
                    Console.WriteLine($"[DEBUG] ConvertToEventData: dictionary key '{kvp.Key}' = {kvp.Value}");
                }
                return dict;
            }

            // Convert object properties to dictionary using reflection
            Console.WriteLine("[DEBUG] ConvertToEventData: converting object properties to dictionary using reflection");
            var result = new Dictionary<string, object?>();
            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            Console.WriteLine($"[DEBUG] ConvertToEventData: found {properties.Length} properties");
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(data);
                    Console.WriteLine($"[DEBUG] ConvertToEventData: property '{prop.Name}' = {value}");
                    result[prop.Name] = value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Error converting property {prop.Name}: {ex.Message}");
                    result[prop.Name] = null;
                }
            }

            Console.WriteLine($"[DEBUG] ConvertToEventData: returning dictionary with {result.Count} entries");
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
                Console.WriteLine($"[DEBUG] GetInstanceField: instance is null for field {fieldName}");
                return null;
            }

            try
            {
                var instanceType = instance.GetType();
                Console.WriteLine($"[DEBUG] GetInstanceField: getting field {fieldName} from {instanceType.Name}");
                
                var field = instanceType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var value = field.GetValue(instance);
                    Console.WriteLine($"[DEBUG] GetInstanceField: field {fieldName} = {value ?? "null"}");
                    return value;
                }
                else
                {
                    Console.WriteLine($"[DEBUG] GetInstanceField: field {fieldName} not found in {instanceType.Name}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetInstanceField: exception getting field {fieldName}: {ex.Message}");
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
                Console.WriteLine($"[DEBUG] GetInstanceProperty: instance is null for {fieldName}.{propertyName}");
                return null;
            }

            try
            {
                Console.WriteLine($"[DEBUG] GetInstanceProperty: accessing {fieldName}.{propertyName}");
                
                // First get the field value (which should be the service instance)
                var serviceInstance = GetInstanceField(instance, fieldName);
                if (serviceInstance == null)
                {
                    Console.WriteLine($"[DEBUG] GetInstanceProperty: field {fieldName} is null");
                    return null;
                }

                // Then get the property from the service instance
                return GetObjectProperty(serviceInstance, propertyName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetInstanceProperty: exception getting {fieldName}.{propertyName}: {ex.Message}");
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
                Console.WriteLine($"[DEBUG] GetObjectProperty: instance is null for property {propertyName}");
                return null;
            }

            try
            {
                var instanceType = instance.GetType();
                Console.WriteLine($"[DEBUG] GetObjectProperty: getting property {propertyName} from {instanceType.Name}");
                
                // Special handling for array.length property
                if (propertyName == "length" && instance is Array array)
                {
                    var length = array.Length;
                    Console.WriteLine($"[DEBUG] GetObjectProperty: array length = {length}");
                    return length;
                }
                
                // Special handling for Dictionary<string, object> (CX object literals)
                if (instance is Dictionary<string, object> dict)
                {
                    if (dict.ContainsKey(propertyName))
                    {
                        var value = dict[propertyName];
                        Console.WriteLine($"[DEBUG] GetObjectProperty: dictionary key {propertyName} = {value ?? "null"}");
                        return value;
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] GetObjectProperty: dictionary key {propertyName} not found");
                        return $"[Key {propertyName} not found]";
                    }
                }
                
                // Special handling for KeyValuePair<string, object> (for dictionary iteration)
                if (instanceType.IsGenericType && instanceType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    var genericArgs = instanceType.GetGenericArguments();
                    if (genericArgs.Length == 2 && genericArgs[0] == typeof(string) && genericArgs[1] == typeof(object))
                    {
                        if (propertyName == "Key")
                        {
                            var keyProperty = instanceType.GetProperty("Key");
                            var keyValue = keyProperty?.GetValue(instance);
                            Console.WriteLine($"[DEBUG] GetObjectProperty: KeyValuePair Key = {keyValue ?? "null"}");
                            return keyValue;
                        }
                        else if (propertyName == "Value")
                        {
                            var valueProperty = instanceType.GetProperty("Value");
                            var valueValue = valueProperty?.GetValue(instance);
                            Console.WriteLine($"[DEBUG] GetObjectProperty: KeyValuePair Value = {valueValue ?? "null"}");
                            return valueValue;
                        }
                    }
                }
                
                // Regular property access for other objects
                var property = instanceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var getter = property.GetMethod;
                    if (getter != null)
                    {
                        var value = getter.Invoke(instance, null);
                        Console.WriteLine($"[DEBUG] GetObjectProperty: property {propertyName} = {value ?? "null"}");
                        return value;
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] GetObjectProperty: property {propertyName} has no getter on {instanceType.Name}");
                        return $"[Property {propertyName} has no getter]";
                    }
                }
                
                // Special handling for CxEvent - check payload dictionary for missing properties
                if (instanceType.Name == "CxEvent")
                {
                    Console.WriteLine($"[DEBUG] GetObjectProperty: CxEvent detected, checking payload for {propertyName}");
                    var payloadProperty = instanceType.GetProperty("payload");
                    if (payloadProperty != null)
                    {
                        var payload = payloadProperty.GetValue(instance);
                        Console.WriteLine($"[DEBUG] GetObjectProperty: CxEvent payload type = {payload?.GetType().Name ?? "null"}");
                        if (payload is Dictionary<string, object> payloadDict)
                        {
                            Console.WriteLine($"[DEBUG] GetObjectProperty: payload is Dictionary with {payloadDict.Count} entries");
                            foreach (var kvp in payloadDict)
                            {
                                Console.WriteLine($"[DEBUG] GetObjectProperty: payload key '{kvp.Key}' = {kvp.Value}");
                            }
                            
                            // First check if the property exists directly in the payload
                            if (payloadDict.ContainsKey(propertyName))
                            {
                                var value = payloadDict[propertyName];
                                Console.WriteLine($"[DEBUG] GetObjectProperty: CxEvent payload property {propertyName} = {value ?? "null"}");
                                return value;
                            }
                            
                            // If not found, check if there's a nested 'payload' property that contains the original data
                            if (payloadDict.ContainsKey("payload") && payloadDict["payload"] is Dictionary<string, object> nestedPayload)
                            {
                                Console.WriteLine($"[DEBUG] GetObjectProperty: checking nested payload for {propertyName}");
                                if (nestedPayload.ContainsKey(propertyName))
                                {
                                    var value = nestedPayload[propertyName];
                                    Console.WriteLine($"[DEBUG] GetObjectProperty: found {propertyName} in nested payload = {value ?? "null"}");
                                    return value;
                                }
                            }
                            
                            Console.WriteLine($"[DEBUG] GetObjectProperty: property {propertyName} not found in CxEvent payload or nested payload");
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] GetObjectProperty: CxEvent payload is not a Dictionary<string, object>");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] GetObjectProperty: CxEvent has no payload property");
                    }
                }
                
                Console.WriteLine($"[DEBUG] GetObjectProperty: property {propertyName} not found on {instanceType.Name}");
                return $"[Property {propertyName} not found]";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetObjectProperty: exception getting property {propertyName}: {ex.Message}");
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
    }
}
