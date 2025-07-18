using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// Registry for runtime-injected functions from AI-generated code
/// Allows adapt() function to inject new functions into the current program context
/// </summary>
public static class RuntimeFunctionRegistry
{
    private static readonly ConcurrentDictionary<string, AssemblyInfo> _registeredAssemblies = new();
    private static readonly ConcurrentDictionary<string, MethodInfo> _registeredMethods = new();
    
    /// <summary>
    /// Information about a registered assembly
    /// </summary>
    public class AssemblyInfo
    {
        public string AssemblyName { get; set; } = string.Empty;
        public Assembly Assembly { get; set; } = null!;
        public Type ProgramType { get; set; } = null!;
        public object? ProgramInstance { get; set; }
        public List<string> AvailableFunctions { get; set; } = new();
    }
    
    /// <summary>
    /// Register an assembly with its functions for runtime access
    /// </summary>
    public static void RegisterAssembly(string assemblyName, Assembly assembly, Type programType)
    {
        try
        {
            var assemblyInfo = new AssemblyInfo
            {
                AssemblyName = assemblyName,
                Assembly = assembly,
                ProgramType = programType
            };
            
            // Get all callable methods from the program type
            var methods = programType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            
            foreach (var method in methods)
            {
                // Skip constructor, Run method, and system methods
                if (method.Name != "Run" && method.Name != ".ctor" && 
                    !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") &&
                    method.DeclaringType == programType)
                {
                    assemblyInfo.AvailableFunctions.Add(method.Name);
                    _registeredMethods[method.Name] = method;
                }
            }
            
            _registeredAssemblies[assemblyName] = assemblyInfo;
        }
        catch (Exception ex)
        {
            // Log error but don't fail - this is a runtime enhancement
            Console.WriteLine($"Warning: Failed to register assembly {assemblyName}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Check if a function is available in the runtime registry
    /// </summary>
    public static bool IsFunctionAvailable(string functionName)
    {
        return _registeredMethods.ContainsKey(functionName);
    }
    
    /// <summary>
    /// Get method info for a registered function
    /// </summary>
    public static MethodInfo? GetFunction(string functionName)
    {
        _registeredMethods.TryGetValue(functionName, out var method);
        return method;
    }
    
    /// <summary>
    /// Get all registered function names
    /// </summary>
    public static IEnumerable<string> GetAllFunctionNames()
    {
        return _registeredMethods.Keys;
    }
    
    /// <summary>
    /// Get all registered assemblies
    /// </summary>
    public static IEnumerable<AssemblyInfo> GetAllAssemblies()
    {
        return _registeredAssemblies.Values;
    }
    
    /// <summary>
    /// Create an instance of a program type for function execution
    /// </summary>
    public static object? CreateProgramInstance(string assemblyName, object console, object? aiService, object? aiFunctions)
    {
        if (_registeredAssemblies.TryGetValue(assemblyName, out var assemblyInfo))
        {
            try
            {
                var instance = Activator.CreateInstance(assemblyInfo.ProgramType, console, aiService, aiFunctions);
                assemblyInfo.ProgramInstance = instance;
                return instance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create instance for {assemblyName}: {ex.Message}");
                return null;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Execute a registered function with the given arguments
    /// </summary>
    public static object? ExecuteFunction(string functionName, object[] args)
    {
        if (_registeredMethods.TryGetValue(functionName, out var method))
        {
            try
            {
                // Find the assembly that contains this method
                var assemblyInfo = _registeredAssemblies.Values.FirstOrDefault(a => a.AvailableFunctions.Contains(functionName));
                if (assemblyInfo != null)
                {
                    // Create instance if needed
                    if (assemblyInfo.ProgramInstance == null)
                    {
                        assemblyInfo.ProgramInstance = CreateProgramInstance(assemblyInfo.AssemblyName, new object(), null, null);
                    }
                    
                    // Execute the method
                    return method.Invoke(assemblyInfo.ProgramInstance, args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to execute function {functionName}: {ex.Message}");
            }
        }
        
        // If we reach here, the function was not found
        throw new Exception($"Function not found: {functionName}");
    }
}
