using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CxLanguage.Runtime
{
    /// <summary>
    /// Provides runtime helper methods for executing complex operations
    /// that are difficult to generate directly in IL.
    /// </summary>
    public static class CxRuntimeHelper
    {
        /// <summary>
        /// Calls a method on a service instance dynamically using reflection.
        /// This method handles method overload resolution, optional parameters, and async Task results.
        /// Synchronous version to avoid Task.Result blocking issues.
        /// </summary>
        public static object? CallServiceMethod(object serviceInstance, string methodName, object[] arguments)
        {
            if (serviceInstance == null)
            {
                return $"[Error: Service instance is null. Cannot call method '{methodName}'.]";
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
    }
}
