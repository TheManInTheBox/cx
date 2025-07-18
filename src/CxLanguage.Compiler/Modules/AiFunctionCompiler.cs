using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Runtime.CompilerServices; // Add this for TaskAwaiter<>
using CxLanguage.Core.AI;
using CxLanguage.Core.Ast;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CxLanguage.Compiler.Modules;

/// <summary>
/// AI function compiler for Cx language
/// Provides implementation for compiling AI function nodes
/// </summary>
public class AiFunctionCompiler
{
    private readonly FieldBuilder _aiServiceField;
    private readonly ILGenerator _il;
    private readonly TypeBuilder _programTypeBuilder;
    private readonly IServiceProvider _serviceProvider;

    public AiFunctionCompiler(
        FieldBuilder aiServiceField,
        ILGenerator il, 
        TypeBuilder programTypeBuilder,
        IServiceProvider serviceProvider)
    {
        _aiServiceField = aiServiceField;
        _il = il;
        _programTypeBuilder = programTypeBuilder;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Compiles a task function call
    /// </summary>
    public void CompileTaskFunction(object[] arguments)
    {
        // Get the IAiService field from the program
        _il.Emit(OpCodes.Ldarg_0); // this
        _il.Emit(OpCodes.Ldfld, _aiServiceField);
        
        // Check for null
        var notNullLabel = _il.DefineLabel();
        _il.Emit(OpCodes.Dup);
        _il.Emit(OpCodes.Brtrue_S, notNullLabel);
        
        // If null, return error message
        _il.Emit(OpCodes.Pop);
        _il.Emit(OpCodes.Ldstr, "AI service not available");
        _il.Emit(OpCodes.Ret);
        
        _il.MarkLabel(notNullLabel);
        
        // Push goal argument
        if (arguments.Length > 0)
        {
            _il.Emit(OpCodes.Ldarg_1); // Goal string
        }
        else
        {
            _il.Emit(OpCodes.Ldstr, "");
        }
        
        // Push options argument if provided
        if (arguments.Length > 1)
        {
            _il.Emit(OpCodes.Ldarg_2); // Options object
        }
        else
        {
            _il.Emit(OpCodes.Ldnull);
        }
        
        // Call the InvokeAIFunctionAsync method
        var invokeMethod = typeof(IAiService).GetMethod("InvokeAIFunctionAsync");
        if (invokeMethod != null)
        {
            _il.Emit(OpCodes.Callvirt, invokeMethod);
            
            // Get the Result property
            var resultProperty = typeof(Task<string>).GetProperty("Result");
            if (resultProperty != null && resultProperty.GetGetMethod() != null)
            {
                _il.Emit(OpCodes.Callvirt, resultProperty.GetGetMethod()!);
            }
            else
            {
                // Fallback - use GetAwaiter().GetResult() pattern
                var getAwaiterMethod = typeof(Task<string>).GetMethod("GetAwaiter");
                if (getAwaiterMethod != null)
                {
                    _il.Emit(OpCodes.Callvirt, getAwaiterMethod);
                    var getResultMethod = typeof(TaskAwaiter<string>).GetMethod("GetResult");
                    if (getResultMethod != null)
                    {
                        _il.Emit(OpCodes.Callvirt, getResultMethod);
                    }
                    else
                    {
                        // Fallback error handling
                        _il.Emit(OpCodes.Pop); // Pop the task from the stack
                        _il.Emit(OpCodes.Ldstr, "Error: Could not get result from Task<string>");
                    }
                }
                else
                {
                    // Fallback error handling
                    _il.Emit(OpCodes.Pop); // Pop the task from the stack
                    _il.Emit(OpCodes.Ldstr, "Error: Could not get awaiter from Task<string>");
                }
            }
        }
        else
        {
            // Fallback - load error message
            _il.Emit(OpCodes.Ldstr, "AI function invocation failed: method not found");
        }
        
        _il.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// Compiles a synthesize function call
    /// </summary>
    public void CompileSynthesizeFunction(object[] arguments)
    {
        // Similar to task function but with synthesize
        _il.Emit(OpCodes.Ldarg_0); // this
        _il.Emit(OpCodes.Ldfld, _aiServiceField);
        
        // Check for null
        var notNullLabel = _il.DefineLabel();
        _il.Emit(OpCodes.Dup);
        _il.Emit(OpCodes.Brtrue_S, notNullLabel);
        
        // If null, return error message
        _il.Emit(OpCodes.Pop);
        _il.Emit(OpCodes.Ldstr, "AI service not available");
        _il.Emit(OpCodes.Ret);
        
        _il.MarkLabel(notNullLabel);
        
        // Push specification argument
        if (arguments.Length > 0)
        {
            _il.Emit(OpCodes.Ldarg_1); // Specification string
        }
        else
        {
            _il.Emit(OpCodes.Ldstr, "");
        }
        
        // Push options argument if provided
        if (arguments.Length > 1)
        {
            _il.Emit(OpCodes.Ldarg_2); // Options object
        }
        else
        {
            _il.Emit(OpCodes.Ldnull);
        }
        
        // Call the InvokeAIFunctionAsync method with "synthesize"
        var invokeMethod = typeof(IAiService).GetMethod("InvokeAIFunctionAsync");
        if (invokeMethod != null)
        {
            _il.Emit(OpCodes.Ldstr, "synthesize");
            _il.Emit(OpCodes.Callvirt, invokeMethod!);
            
            // Get the Result property
            var resultProperty = typeof(Task<string>).GetProperty("Result");
            if (resultProperty?.GetGetMethod() != null)
            {
                _il.Emit(OpCodes.Callvirt, resultProperty.GetGetMethod()!);
            }
            else
            {
                // Fallback if Result property getter cannot be found
                _il.Emit(OpCodes.Pop);
                _il.Emit(OpCodes.Ldstr, "Result property not available");
            }
        }
        else
        {
            // Fallback if InvokeAIFunctionAsync method cannot be found
            _il.Emit(OpCodes.Pop); // Pop the options object
            _il.Emit(OpCodes.Ldstr, "InvokeAIFunctionAsync method not available");
        }
        
        _il.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// Compiles a reason function call
    /// </summary>
    public void CompileReasonFunction(object[] arguments)
    {
        // Similar to task function but with reason
        CompileAIFunction("reason", arguments);
    }

    /// <summary>
    /// Compiles a process function call
    /// </summary>
    public void CompileProcessFunction(object[] arguments)
    {
        // Similar to task function but with process
        CompileAIFunction("process", arguments);
    }

    /// <summary>
    /// Compiles a generate function call
    /// </summary>
    public void CompileGenerateFunction(object[] arguments)
    {
        // Similar to task function but with generate
        CompileAIFunction("generate", arguments);
    }

    /// <summary>
    /// Compiles an embed function call
    /// </summary>
    public void CompileEmbedFunction(object[] arguments)
    {
        // Similar to task function but with embed
        CompileAIFunction("embed", arguments);
    }

    /// <summary>
    /// Compiles an adapt function call
    /// </summary>
    public void CompileAdaptFunction(object[] arguments)
    {
        // Similar to task function but with adapt
        CompileAIFunction("adapt", arguments);
    }

    /// <summary>
    /// Generic method to compile an AI function call
    /// </summary>
    private void CompileAIFunction(string functionName, object[] arguments)
    {
        // Get the IAiService field from the program
        _il.Emit(OpCodes.Ldarg_0); // this
        _il.Emit(OpCodes.Ldfld, _aiServiceField);
        
        // Check for null
        var notNullLabel = _il.DefineLabel();
        _il.Emit(OpCodes.Dup);
        _il.Emit(OpCodes.Brtrue_S, notNullLabel);
        
        // If null, return error message
        _il.Emit(OpCodes.Pop);
        _il.Emit(OpCodes.Ldstr, $"AI service not available for {functionName}");
        _il.Emit(OpCodes.Ret);
        
        _il.MarkLabel(notNullLabel);
        
        // Load function name
        _il.Emit(OpCodes.Ldstr, functionName);
        
        // Create argument array
        _il.Emit(OpCodes.Ldc_I4, arguments.Length);
        _il.Emit(OpCodes.Newarr, typeof(object));
        
        // Load each argument into the array
        for (int i = 0; i < arguments.Length; i++)
        {
            _il.Emit(OpCodes.Dup);  // Duplicate array reference
            _il.Emit(OpCodes.Ldc_I4, i);  // Load index
            _il.Emit(OpCodes.Ldarg, i + 1);  // Load argument (1-based index since arg_0 is 'this')
            _il.Emit(OpCodes.Stelem_Ref);  // Store in array
        }
        
        // Push options argument (null for now)
        _il.Emit(OpCodes.Ldnull);
        
        // Call the InvokeAIFunctionAsync method
        var invokeMethod = typeof(IAiService).GetMethod("InvokeAIFunctionAsync");
        if (invokeMethod != null)
        {
            _il.Emit(OpCodes.Callvirt, invokeMethod!);
            
            // Get the Result property
            var resultProperty = typeof(Task<string>).GetProperty("Result");
            if (resultProperty?.GetGetMethod() != null)
            {
                _il.Emit(OpCodes.Callvirt, resultProperty.GetGetMethod()!);
            }
            else
            {
                // Fallback if Result property getter cannot be found
                _il.Emit(OpCodes.Pop);
                _il.Emit(OpCodes.Ldstr, "Result property not available");
            }
        }
        else
        {
            // Fallback if InvokeAIFunctionAsync method cannot be found
            _il.Emit(OpCodes.Pop); // Pop the options object
            _il.Emit(OpCodes.Pop); // Pop the arguments array
            _il.Emit(OpCodes.Pop); // Pop the function name
            _il.Emit(OpCodes.Ldstr, "InvokeAIFunctionAsync method not available");
        }
        
        _il.Emit(OpCodes.Ret);
    }
}
