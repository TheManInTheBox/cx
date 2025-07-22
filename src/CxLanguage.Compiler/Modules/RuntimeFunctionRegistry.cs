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
            Console.WriteLine($"📚 REGISTRY: Starting registration for assembly: {assemblyName}");
            
            var assemblyInfo = new AssemblyInfo
            {
                AssemblyName = assemblyName,
                Assembly = assembly,
                ProgramType = programType
            };
            
            // Get all callable methods from the program type
            var methods = programType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            Console.WriteLine($"🔍 REGISTRY: Found {methods.Length} total methods in program type");
            
            int registeredCount = 0;
            foreach (var method in methods)
            {
                // Skip constructor, Run method, and system methods
                if (method.Name != "Run" && method.Name != ".ctor" && 
                    !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") &&
                    method.DeclaringType == programType)
                {
                    assemblyInfo.AvailableFunctions.Add(method.Name);
                    _registeredMethods[method.Name] = method;
                    registeredCount++;
                    
                    var parameters = method.GetParameters();
                    var returnType = method.ReturnType;
                    Console.WriteLine($"📋 REGISTRY: Registered function {method.Name}({string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}) -> {returnType.Name}");
                }
            }
            
            _registeredAssemblies[assemblyName] = assemblyInfo;
            Console.WriteLine($"✅ REGISTRY: Successfully registered {registeredCount} functions from assembly {assemblyName}");
            Console.WriteLine($"📊 REGISTRY: Total registered functions: {_registeredMethods.Count}");
        }
        catch (Exception ex)
        {
            // Log error but don't fail - this is a runtime enhancement
            Console.WriteLine($"❌ REGISTRY ERROR: Failed to register assembly {assemblyName}: {ex.Message}");
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
        Console.WriteLine($"🏗️ INSTANCE: Creating program instance for assembly: {assemblyName}");
        
        if (_registeredAssemblies.TryGetValue(assemblyName, out var assemblyInfo))
        {
            try
            {
                Console.WriteLine($"🔧 INSTANCE: Found assembly info for {assemblyName}");
                Console.WriteLine($"📋 INSTANCE: Program type: {assemblyInfo.ProgramType.Name}");
                
                var instance = Activator.CreateInstance(assemblyInfo.ProgramType, console, aiService, aiFunctions);
                assemblyInfo.ProgramInstance = instance;
                
                Console.WriteLine($"✅ INSTANCE: Successfully created program instance for {assemblyName}");
                return instance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ INSTANCE ERROR: Failed to create instance for {assemblyName}: {ex.Message}");
                Console.WriteLine($"📋 INSTANCE: Stack trace: {ex.StackTrace}");
                return null;
            }
        }
        
        Console.WriteLine($"❌ INSTANCE: No assembly info found for {assemblyName}");
        return null;
    }
    
    /// <summary>
    /// Execute a registered function with the given arguments
    /// </summary>
    public static object? ExecuteFunction(string functionName, object[] args)
    {
        Console.WriteLine($"🚀 EXECUTION: Attempting to execute function: {functionName} with {args.Length} arguments");
        
        if (_registeredMethods.TryGetValue(functionName, out var method))
        {
            try
            {
                Console.WriteLine($"✅ EXECUTION: Found function {functionName} in registry");
                
                // Handle static methods (built-in functions)
                if (method.IsStatic)
                {
                    Console.WriteLine($"🔧 EXECUTION: Executing static built-in function: {functionName}");
                    
                    // Log method details
                    var parameters = method.GetParameters();
                    Console.WriteLine($"📋 EXECUTION: Method signature: {method.ReturnType.Name} {functionName}({string.Join(", ", parameters.Select(p => p.ParameterType.Name))})");
                    
                    // Convert arguments to match parameter types
                    var convertedArgs = ConvertArguments(args, parameters);
                    
                    // Execute the static method
                    Console.WriteLine($"⚡ EXECUTION: Invoking static method {functionName}");
                    var result = method.Invoke(null, convertedArgs);
                    
                    Console.WriteLine($"🎉 EXECUTION SUCCESS: Static function {functionName} executed successfully");
                    Console.WriteLine($"📤 EXECUTION: Result type: {result?.GetType().Name ?? "null"}");
                    
                    return result;
                }
                
                // Handle instance methods (user-defined functions)
                var assemblyInfo = _registeredAssemblies.Values.FirstOrDefault(a => a.AvailableFunctions.Contains(functionName));
                if (assemblyInfo != null)
                {
                    Console.WriteLine($"🏗️ EXECUTION: Found assembly {assemblyInfo.AssemblyName} containing function {functionName}");
                    
                    // Create instance if needed
                    if (assemblyInfo.ProgramInstance == null)
                    {
                        Console.WriteLine($"🔧 EXECUTION: Creating program instance for {assemblyInfo.AssemblyName}");
                        assemblyInfo.ProgramInstance = CreateProgramInstance(assemblyInfo.AssemblyName, new object(), null, null);
                        if (assemblyInfo.ProgramInstance == null)
                        {
                            Console.WriteLine($"❌ EXECUTION: Failed to create program instance for {assemblyInfo.AssemblyName}");
                            return null;
                        }
                        Console.WriteLine($"✅ EXECUTION: Program instance created successfully");
                    }
                    
                    // Log method details
                    var parameters = method.GetParameters();
                    Console.WriteLine($"📋 EXECUTION: Method signature: {method.ReturnType.Name} {functionName}({string.Join(", ", parameters.Select(p => p.ParameterType.Name))})");
                    
                    // Convert arguments to match parameter types
                    var convertedArgs = ConvertArguments(args, parameters);
                    
                    // Execute the method
                    Console.WriteLine($"⚡ EXECUTION: Invoking method {functionName}");
                    var result = method.Invoke(assemblyInfo.ProgramInstance, convertedArgs);
                    
                    Console.WriteLine($"🎉 EXECUTION SUCCESS: Function {functionName} executed successfully");
                    Console.WriteLine($"📤 EXECUTION: Result type: {result?.GetType().Name ?? "null"}");
                    
                    return result;
                }
                else
                {
                    Console.WriteLine($"❌ EXECUTION: No assembly found containing function {functionName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXECUTION ERROR: Failed to execute function {functionName}: {ex.Message}");
                Console.WriteLine($"📋 EXECUTION: Stack trace: {ex.StackTrace}");
            }
        }
        else
        {
            Console.WriteLine($"❌ EXECUTION: Function {functionName} not found in registry");
            Console.WriteLine($"📋 EXECUTION: Available functions: {string.Join(", ", _registeredMethods.Keys)}");
        }
        
        // If we reach here, the function was not found or execution failed
        throw new Exception($"Function not found or execution failed: {functionName}");
    }

    /// <summary>
    /// Register a built-in static function for runtime access
    /// </summary>
    public static void RegisterBuiltInFunction(string functionName, MethodInfo method)
    {
        Console.WriteLine($"🔧 REGISTRY: Registering built-in function: {functionName}");
        _registeredMethods.TryAdd(functionName, method);
    }

    /// <summary>
    /// Register all built-in functions from CxRuntimeHelper
    /// </summary>
    public static void RegisterBuiltInFunctions()
    {
        // Use reflection to get the CxRuntimeHelper type from the Runtime assembly
        var runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "CxLanguage.Runtime");
        
        if (runtimeAssembly == null)
        {
            Console.WriteLine($"❌ REGISTRY: Could not find CxLanguage.Runtime assembly");
            return;
        }

        var runtimeHelperType = runtimeAssembly.GetType("CxLanguage.Runtime.CxRuntimeHelper");
        if (runtimeHelperType == null)
        {
            Console.WriteLine($"❌ REGISTRY: Could not find CxRuntimeHelper type in Runtime assembly");
            return;
        }

        var methods = runtimeHelperType.GetMethods(BindingFlags.Public | BindingFlags.Static);

        // Register namespace event functions
        var namespacedFunctions = new[]
        {
            "RegisterNamespacedAgent",
            "UnregisterNamespacedAgent", 
            "SubscribeToNamespacedEvent",
            "AutoSubscribeNamespacedAgent",
            "EmitNamespacedEvent",
            "GetNamespacedBusStatistics"
        };

        foreach (var functionName in namespacedFunctions)
        {
            var method = methods.FirstOrDefault(m => m.Name == functionName);
            if (method != null)
            {
                RegisterBuiltInFunction(functionName, method);
                Console.WriteLine($"✅ REGISTRY: Built-in function registered: {functionName}");
            }
            else
            {
                Console.WriteLine($"❌ REGISTRY: Built-in function not found: {functionName}");
            }
        }

        // Register utility functions with CX-friendly names
        var utilityFunctions = new Dictionary<string, string>
        {
            { "typeof", "GetTypeOf" }  // CX name -> C# method name
        };

        foreach (var (cxName, csharpMethodName) in utilityFunctions)
        {
            var method = methods.FirstOrDefault(m => m.Name == csharpMethodName);
            if (method != null)
            {
                RegisterBuiltInFunction(cxName, method);
                Console.WriteLine($"✅ REGISTRY: Built-in function registered: {cxName} -> {csharpMethodName}");
            }
            else
            {
                Console.WriteLine($"❌ REGISTRY: Built-in function not found: {cxName} -> {csharpMethodName}");
            }
        }
    }

    /// <summary>
    /// Convert CX arguments to match C# method parameter types
    /// </summary>
    private static object[] ConvertArguments(object[] args, ParameterInfo[] parameters)
    {
        if (args.Length != parameters.Length)
        {
            Console.WriteLine($"⚠️ ARGUMENT MISMATCH: Expected {parameters.Length} arguments, got {args.Length}");
            return args; // Return as-is and let reflection handle the error
        }

        var convertedArgs = new object[args.Length];
        
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var paramType = parameters[i].ParameterType;
            
            if (arg == null)
            {
                convertedArgs[i] = null!;
                continue;
            }
            
            var argType = arg.GetType();
            
            // Direct type match
            if (paramType.IsAssignableFrom(argType))
            {
                convertedArgs[i] = arg;
                continue;
            }
            
            // Handle object[] to string[] conversion
            if (paramType == typeof(string[]) && arg is object[] objectArray)
            {
                Console.WriteLine($"🔄 CONVERTING: object[] to string[] for parameter {i}");
                convertedArgs[i] = objectArray.Select(o => o?.ToString()).ToArray();
                continue;
            }
            
            // Handle other array conversions if needed
            if (paramType.IsArray && arg is object[] sourceArray)
            {
                var elementType = paramType.GetElementType();
                Console.WriteLine($"🔄 CONVERTING: object[] to {elementType?.Name}[] for parameter {i}");
                
                var targetArray = Array.CreateInstance(elementType!, sourceArray.Length);
                for (int j = 0; j < sourceArray.Length; j++)
                {
                    targetArray.SetValue(Convert.ChangeType(sourceArray[j], elementType!), j);
                }
                convertedArgs[i] = targetArray;
                continue;
            }
            
            // Default: try direct assignment
            convertedArgs[i] = arg;
        }
        
        return convertedArgs;
    }
}
