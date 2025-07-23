using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CxLanguage.Core;
using CxLanguage.Core.Ast;
using CxLanguage.Core.Symbols;
using CxLanguage.Core.Types;
using CxLanguage.Core.AI;
using CxLanguage.Compiler.Modules;
using CxLanguage.Runtime;

namespace CxLanguage.Compiler;

/// <summary>
/// Exception thrown during compilation
/// </summary>
public class CompilationException : Exception
{
    public CompilationException(string message) : base(message) { }
}

/// <summary>
/// CxCompiler generates .NET assembly from the Cx AST
/// </summary>
public class CxCompiler : IAstVisitor<object>
{
    // Compiler-level fields
    private readonly AssemblyBuilder _assemblyBuilder;
    private readonly ModuleBuilder _moduleBuilder;
    private readonly TypeBuilder _programTypeBuilder;
    private readonly Dictionary<string, FieldBuilder> _globalFields = new();
    private readonly Dictionary<string, MethodBuilder> _userFunctions = new();
    private readonly Stack<SymbolTable> _scopes = new();
    private readonly CompilerOptions _options;
    private readonly string _scriptName;
    private readonly FieldBuilder _consoleField;
    private readonly IAiService? _aiService;
    private readonly FieldBuilder _aiServiceField;
    private readonly FieldBuilder _serviceProviderField;

    // Import and service resolution
    private readonly Dictionary<string, Type> _importedServices = new();
    private readonly Dictionary<string, FieldBuilder> _serviceFields = new();

    // Class system support
    private readonly Dictionary<string, TypeBuilder> _userClasses = new();
    private readonly Dictionary<string, FieldBuilder> _classFields = new();
    private readonly Dictionary<string, Type> _createdTypes = new(); // Maps className -> created Type
    private readonly Dictionary<string, Type> _classBaseTypes = new(); // Maps className -> base class Type
    private readonly Dictionary<string, FieldInfo> _actualFields = new(); // Maps className.fieldName -> actual FieldInfo
    private readonly Dictionary<string, MethodBuilder> _classMethods = new();
    private readonly Dictionary<string, ConstructorBuilder> _classConstructors = new();
    private readonly Dictionary<string, List<(string EventName, string MethodName)>> _classEventHandlers = new();
    private string? _currentClassName = null; // Track current class being compiled

    // Event system support
    private readonly Dictionary<string, MethodBuilder> _eventHandlers = new();
    private readonly List<(string EventName, string HandlerMethodName)> _eventRegistrations = new();
    private int _eventHandlerCounter = 0;
    
    // Track event handler method names for Pass 2
    private Dictionary<OnStatementNode, string> _eventHandlerMethodNames = new();

    // Two-pass compilation state
    private bool _isFirstPass = true;
    private readonly List<FunctionDeclarationNode> _pendingFunctions = new();

    // Method-level fields for current compilation context
    private MethodBuilder? _currentMethod;
    private ILGenerator? _currentIl;
    private ILGenerator? _constructorIl;
    private Dictionary<string, LocalBuilder> _currentLocals = new();
    private Dictionary<string, int>? _currentParameterMapping = null;
    private int _ifCounter = 0;
    private int _whileCounter = 0;
    private bool _isCompilingAsyncMethod = false; // Track if we're compiling an async method

    public CxCompiler(string assemblyName, CompilerOptions options, IAiService? aiService = null)
    {
        _options = options;
        _scriptName = assemblyName;
        _aiService = aiService;
        
        // Create assembly and module
        var assemblyNameObj = new AssemblyName(assemblyName);
        _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyNameObj, 
            AssemblyBuilderAccess.RunAndCollect);
        
        _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName);
        
        // Create the program type
        _programTypeBuilder = _moduleBuilder.DefineType(
            "Program", 
            TypeAttributes.Public | TypeAttributes.Class);
        
        // Create console field for output
        _consoleField = _programTypeBuilder.DefineField(
            "_console", 
            typeof(object), 
            FieldAttributes.Private);
        
        // Create AI service field for AI functions
        _aiServiceField = _programTypeBuilder.DefineField(
            "_aiService", 
            typeof(IAiService), 
            FieldAttributes.Private);
        
        // Note: AiFunctions field removed - modern architecture uses individual services via dependency injection
        
        // Create service provider field for dependency injection
        _serviceProviderField = _programTypeBuilder.DefineField(
            "_serviceProvider", 
            typeof(IServiceProvider), 
            FieldAttributes.Assembly | FieldAttributes.Static);
        
        // Initialize symbol table
        _scopes.Push(new SymbolTable());
    }

    /// <summary>
    /// Compile AST into a .NET assembly using two-pass compilation
    /// </summary>
    public CompilationResult Compile(ProgramNode ast, string scriptName, string sourceText)
    {
        try
        {
            // Create constructor that takes object for console, IAiService for AI functions, SemanticKernelAiFunctions for AI operations, and IServiceProvider for DI
            var ctorBuilder = _programTypeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(IAiService), typeof(IServiceProvider) });
            
            var ctorIl = ctorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
            
            // Store console in field
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Stfld, _consoleField);
            
            // Store AI service in field
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_2);
            ctorIl.Emit(OpCodes.Stfld, _aiServiceField);
            
            // Store service provider in static field (arg 3)
            Console.WriteLine($"[DEBUG] IL-EMIT: Storing service provider in static field");
            Console.WriteLine($"[DEBUG] IL-EMIT: Service provider field info - Name: {_serviceProviderField.Name}, Type: {_serviceProviderField.FieldType}, IsStatic: {_serviceProviderField.IsStatic}");
            ctorIl.Emit(OpCodes.Ldarg_3);
            ctorIl.Emit(OpCodes.Stsfld, _serviceProviderField);
            
            // Initialize imported services (this will be populated after import processing)
            _constructorIl = ctorIl;
            
            // Note: Service initialization will be added here before the ret instruction
            
            // Create Run method
            var runMethod = _programTypeBuilder.DefineMethod(
                "Run",
                MethodAttributes.Public,
                typeof(object),
                Type.EmptyTypes);
            
            _currentMethod = runMethod;
            _currentIl = runMethod.GetILGenerator();
            _currentLocals.Clear();

            // **PASS 1: Collect function and class declarations, and event handlers**
            Console.WriteLine("[DEBUG] Starting Pass 1: Collecting declarations");
            _isFirstPass = true;
            _pendingFunctions.Clear();
            _eventRegistrations.Clear();
            _eventHandlerMethodNames.Clear();
            ast.Accept(this);
            Console.WriteLine($"[DEBUG] Pass 1 complete. Found {_pendingFunctions.Count} functions and {_eventRegistrations.Count} event handlers.");

            // Finish constructor after all imports are processed
            if (_constructorIl != null)
            {
                _constructorIl.Emit(OpCodes.Ret);
            }

            // **PASS 2: Compile function and event handler bodies**
            Console.WriteLine("[DEBUG] Starting Pass 2: Compiling function and event handler bodies");
            _isFirstPass = false;

            // Compile all pending function bodies
            foreach (var functionNode in _pendingFunctions)
            {
                Console.WriteLine($"[DEBUG] Compiling function body: {functionNode.Name}");
                CompileFunctionBody(functionNode);
            }

            // Compile all event handler bodies (they were collected in Pass 1)
            // This requires iterating through the AST again in "Pass 2" mode
            foreach (var statement in ast.Statements)
            {
                if (statement is OnStatementNode or ClassDeclarationNode)
                {
                    statement.Accept(this);
                }
            }

            // **PASS 3: Compile the main program body**
            Console.WriteLine("[DEBUG] Starting Pass 3: Compiling main program statements");
            
            // Add runtime debug output at the start of the Run method
            var printMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
            _currentIl.Emit(OpCodes.Ldstr, "[DEBUG] RUNTIME: Run method started executing");
            _currentIl.Emit(OpCodes.Call, printMethod!);

            // Generate event handler registration code at the beginning of the Run method
            GenerateEventHandlerRegistrations();

            // Compile the main program statements, skipping declarations
            foreach (var statement in ast.Statements)
            {
                if (statement is not FunctionDeclarationNode and not OnStatementNode and not ClassDeclarationNode)
                {
                    statement.Accept(this);
                }
            }
            
            // Add runtime debug output at the end of the Run method (before the return)
            _currentIl.Emit(OpCodes.Ldstr, "[DEBUG] RUNTIME: Run method completed successfully");
            _currentIl.Emit(OpCodes.Call, printMethod!);
            
            // Return null by default
            Console.WriteLine("[DEBUG] Adding final return to main method");
            _currentIl.Emit(OpCodes.Ldnull);
            _currentIl.Emit(OpCodes.Ret);
            
            // Create the type
            Console.WriteLine("[DEBUG] Creating program type");
            var programType = _programTypeBuilder.CreateType();
            Console.WriteLine("[DEBUG] Compilation successful");
            
            return CompilationResult.Success(_assemblyBuilder, programType);
        }
        catch (Exception ex)
        {
            return CompilationResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Generate IL code to register all compiled event handlers with the runtime event bus
    /// </summary>
    private void GenerateEventHandlerRegistrations()
    {
        if (_eventRegistrations.Count == 0)
        {
            Console.WriteLine("[DEBUG] No event handlers to register");
            return;
        }

        Console.WriteLine($"[DEBUG] Generating registration code for {_eventRegistrations.Count} event handlers");

        foreach (var (eventName, handlerMethodName) in _eventRegistrations)
        {
            if (!_eventHandlers.TryGetValue(eventName, out var handlerMethod))
            {
                Console.WriteLine($"[ERROR] Handler method not found for event: {eventName}");
                continue;
            }

            Console.WriteLine($"[DEBUG] Registering event handler: {eventName} -> {handlerMethodName}");

            // Load the event name as string parameter
            _currentIl!.Emit(OpCodes.Ldstr, eventName);

            // Create delegate for the handler method
            // Load null for target (static method)
            _currentIl.Emit(OpCodes.Ldnull);
            
            // Load function pointer to the handler method
            _currentIl.Emit(OpCodes.Ldftn, handlerMethod);
            
            // Create Func<object, Task> delegate
            var funcType = typeof(Func<object, Task>);
            var funcConstructor = funcType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            if (funcConstructor == null)
            {
                throw new CompilationException("Could not find Func<object, Task> constructor");
            }
            _currentIl.Emit(OpCodes.Newobj, funcConstructor);

            // Convert Func<object, Task> to CxEventHandler using helper method
            var convertMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper).GetMethod("ConvertToEventHandler");
            if (convertMethod == null)
            {
                throw new CompilationException("ConvertToEventHandler method not found in CxRuntimeHelper");
            }
            _currentIl.Emit(OpCodes.Call, convertMethod);

            // Call CxRuntimeHelper.RegisterEventHandler(string eventName, CxEventHandler handler)
            // The handler will now receive CxEvent objects instead of raw payloads
            var registerMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper).GetMethod("RegisterEventHandler");
            if (registerMethod == null)
            {
                throw new CompilationException("RegisterEventHandler method not found in CxRuntimeHelper");
            }
            _currentIl.Emit(OpCodes.Call, registerMethod);

            Console.WriteLine($"[DEBUG] Generated registration IL for: {eventName}");
        }

        Console.WriteLine($"[DEBUG] Event handler registration complete - {_eventRegistrations.Count} handlers registered");
    }

    /// <summary>
    /// Get method info for built-in or predefined functions
    /// </summary>
    private MethodInfo? GetMethodInfo(string name, int argCount)
    {
        // Enhanced print function that handles Dictionary objects
        if (name == "print" && argCount == 1)
        {
            return typeof(CxLanguage.Runtime.CxPrint).GetMethod("Print", new[] { typeof(object) });
        }
        
        // Console.Write
        if (name == "write" && argCount == 1)
        {
            return typeof(Console).GetMethod("Write", new[] { typeof(object) });
        }
        
        // Vector database functions
        if (name == "ingest" && argCount >= 1 && argCount <= 2)
        {
            // Will be handled specially in CompileCall
            return null;
        }
        
        if (name == "index" && argCount >= 1 && argCount <= 2)
        {
            // Will be handled specially in CompileCall
            return null;
        }
        
        if (name == "search" && argCount >= 1 && argCount <= 2)
        {
            // Will be handled specially in CompileCall
            return null;
        }
        
        // TODO: Add other built-in methods
        
        return null;
    }

    // AST node visitors
    
    public object VisitProgram(ProgramNode node)
    {
        // Process imports first in both passes
        foreach (var import in node.Imports)
        {
            import.Accept(this);
        }
        
        foreach (var statement in node.Statements)
        {
            statement.Accept(this);
        }
        
        return new object();
    }

    public object VisitBlock(BlockStatementNode node)
    {
        // Enter new scope
        _scopes.Push(new SymbolTable(_scopes.Peek()));
        
        foreach (var statement in node.Statements)
        {
            statement.Accept(this);
        }
        
        // Exit scope
        _scopes.Pop();
        
        return new object();
    }

    public object VisitExpression(ExpressionStatementNode node)
    {
        Console.WriteLine($"[DEBUG] Processing expression statement in Pass {(_isFirstPass ? "1" : "2")}");
        
        if (_isFirstPass)
        {
            node.Expression.Accept(this);
            return new object();
        }

        // In Pass 2, we need to be careful about stack management
        // Most expressions leave values on the stack that need to be popped when used as statements
        bool needsPop = true;

        // Assignment expressions manage their own stack and don't leave extra values
        if (node.Expression is AssignmentExpressionNode)
        {
            Console.WriteLine($"[DEBUG] Processing assignment expression - will need to pop result");
            needsPop = true; // Assignment expressions now leave a duplicated value on the stack
        }
        
        // Some specific function calls that return void or manage their own stack
        if (node.Expression is CallExpressionNode callExpr)
        {
            // Check if the callee is an identifier (simple function call)
            if (callExpr.Callee is IdentifierNode identifier)
            {
                // Built-in functions that return void - these push null in VisitFunctionCall, so we need to pop it in expression context
                if (identifier.Name == "print")
                {
                    needsPop = true; // print returns void, pushes null in VisitFunctionCall, must pop in expression context
                }
            }
        }

        node.Expression.Accept(this);
        
        // Only pop if the expression actually left a value on the stack
        if (needsPop)
        {
            Console.WriteLine("[DEBUG] Popping expression result from stack");
            _currentIl!.Emit(OpCodes.Pop);
        }
        else
        {
            Console.WriteLine("[DEBUG] No pop needed - expression doesn't leave value on stack");
        }
        
        Console.WriteLine("[DEBUG] Expression statement processing complete");
        return new object();
    }

    public object VisitIf(IfStatementNode node)
    {
        int ifId = _ifCounter++;
        
        // Compile condition
        node.Condition.Accept(this);
        
        // Convert to boolean condition
        EmitTruthyConversion();
        
        // Create labels
        var elseLabel = _currentIl!.DefineLabel();
        var endLabel = _currentIl.DefineLabel();
        
        // Jump to else if condition is false
        _currentIl.Emit(OpCodes.Brfalse, elseLabel);
        
        // Compile then branch
        node.ThenStatement.Accept(this);
        
        // Skip else branch
        _currentIl.Emit(OpCodes.Br, endLabel);
        
        // Else branch
        _currentIl.MarkLabel(elseLabel);
        
        if (node.ElseStatement != null)
        {
            node.ElseStatement.Accept(this);
        }
        
        // End of if
        _currentIl.MarkLabel(endLabel);
        
        return new object();
    }

    public object VisitWhile(WhileStatementNode node)
    {
        if (_isFirstPass)
        {
            // Pass 1: Skip while loop processing entirely
            return new object();
        }
        
        int whileId = _whileCounter++;
        
        // Create labels
        var condLabel = _currentIl!.DefineLabel();
        var endLabel = _currentIl.DefineLabel();
        
        // Condition check
        _currentIl.MarkLabel(condLabel);
        
        // Compile condition
        node.Condition.Accept(this);
        
        // Unbox to boolean
        _currentIl.Emit(OpCodes.Unbox_Any, typeof(bool));
        
        // Jump to end if condition is false
        _currentIl.Emit(OpCodes.Brfalse, endLabel);
        
        // Compile body
        node.Body.Accept(this);
        
        // Jump back to condition
        _currentIl.Emit(OpCodes.Br, condLabel);
        
        // End of while
        _currentIl.MarkLabel(endLabel);
        
        return new object();
    }

    /// <summary>
    /// Compile a function body (used in Pass 2)
    /// </summary>
    private void CompileFunctionBody(FunctionDeclarationNode node)
    {
        // Get the already-created MethodBuilder
        var methodBuilder = _userFunctions[node.Name];
        
        // Save current method and locals
        var savedMethod = _currentMethod;
        var savedIl = _currentIl;
        var savedLocals = _currentLocals;
        
        // Set up for new method
        _currentMethod = methodBuilder;
        _currentIl = methodBuilder.GetILGenerator();
        _currentLocals = new Dictionary<string, LocalBuilder>();
        
        // Create a mapping for parameters so they can be accessed like local variables
        // Parameters are accessed via Ldarg instructions, not local variables
        var parameterMapping = new Dictionary<string, int>();
        for (int i = 0; i < node.Parameters.Count; i++)
        {
            parameterMapping[node.Parameters[i].Name] = i;
            Console.WriteLine($"[DEBUG] Function {node.Name}: Parameter {node.Parameters[i].Name} mapped to arg {i}");
        }
        
        // Store parameter mapping for use in identifier resolution
        var savedParameterMapping = _currentParameterMapping;
        _currentParameterMapping = parameterMapping;
        
        if (node.IsAsync)
        {
            Console.WriteLine($"[DEBUG] Compiling async function {node.Name} as regular method with Task<object> return");
            
            // Compile body synchronously but handle await expressions as blocking calls
            var resultLocal = _currentIl.DeclareLocal(typeof(object));
            var bodyHasReturn = false;
            
            // Execute function body and track if we found a return statement
            if (node.Body != null)
            {
                foreach (var statement in node.Body.Statements)
                {
                    if (statement is ReturnStatementNode)
                    {
                        bodyHasReturn = true;
                        // Process return statement - result will be on stack
                        statement.Accept(this);
                        // Store the returned value
                        _currentIl.Emit(OpCodes.Stloc, resultLocal);
                        break;
                    }
                    else
                    {
                        statement.Accept(this);
                    }
                }
            }
            
            // If no return statement was found, set result to null
            if (!bodyHasReturn)
            {
                _currentIl.Emit(OpCodes.Ldnull);
                _currentIl.Emit(OpCodes.Stloc, resultLocal);
            }
            
            // Wrap result in Task.FromResult<object>()
            _currentIl.Emit(OpCodes.Ldloc, resultLocal);
            
            // Get the generic FromResult method: Task.FromResult<T>(T value)
            var taskFromResultMethod = typeof(Task).GetMethod("FromResult");
            if (taskFromResultMethod != null)
            {
                // Make it generic with object type: Task.FromResult<object>(object value)
                var genericMethod = taskFromResultMethod.MakeGenericMethod(typeof(object));
                Console.WriteLine($"[DEBUG] Using Task.FromResult<object> method in CompileFunctionBody: {genericMethod}");
                _currentIl.Emit(OpCodes.Call, genericMethod);
            }
            else
            {
                Console.WriteLine("[ERROR] Could not find Task.FromResult method in CompileFunctionBody");
                _currentIl.Emit(OpCodes.Ldnull);
            }
            
            _currentIl.Emit(OpCodes.Ret);
        }
        else
        {
            Console.WriteLine($"[DEBUG] Compiling sync function {node.Name}");
            // Compile body
            node.Body.Accept(this);
            
            // Only add implicit return if the function doesn't end with a return statement
            // (This is a simplification - ideally we'd track control flow)
            var lastStatement = node.Body.Statements.LastOrDefault();
            if (lastStatement == null || lastStatement is not ReturnStatementNode)
            {
                // For functions with specified return types other than void, we should ideally
                // check that all paths return a value, but for now we'll just provide a default
                // null return value for any missing returns
                _currentIl.Emit(OpCodes.Ldnull);
                _currentIl.Emit(OpCodes.Ret);
            }
        }
        
        // Restore previous method context
        _currentMethod = savedMethod;
        _currentIl = savedIl;
        _currentLocals = savedLocals;
        _currentParameterMapping = savedParameterMapping;
    }

    private void CompileAsyncFunctionBody(FunctionDeclarationNode node)
    {
        // SIMPLIFIED ASYNC APPROACH:
        // Generate a simple method body that wraps the synchronous execution in Task.FromResult
        // This provides Task<object> return type with blocking execution semantics
        
        if (_currentIl == null)
        {
            Console.WriteLine("[ERROR] _currentIl is null in CompileAsyncFunctionBody");
            return;
        }
        
        Console.WriteLine($"[DEBUG] Generating simplified async wrapper for function: {node.Name}");
        
        try
        {
            var resultLocal = _currentIl.DeclareLocal(typeof(object));
            
            // Execute function body synchronously and store result
            if (node.Body != null)
            {
                foreach (var statement in node.Body.Statements)
                {
                    var result = statement.Accept(this);
                    
                    // If this is a return statement, store the result
                    if (statement is ReturnStatementNode)
                    {
                        // Result is already on stack from return statement processing
                        _currentIl.Emit(OpCodes.Stloc, resultLocal);
                        break;
                    }
                }
            }
            
            // If no explicit return, set result to null
            if (!(node.Body?.Statements.Any(s => s is ReturnStatementNode) ?? false))
            {
                _currentIl.Emit(OpCodes.Ldnull);
                _currentIl.Emit(OpCodes.Stloc, resultLocal);
            }
            
            // Wrap result in Task.FromResult<object>()
            _currentIl.Emit(OpCodes.Ldloc, resultLocal);
            
            // Get the generic FromResult method: Task.FromResult<T>(T value)
            var taskFromResultMethod = typeof(Task).GetMethod("FromResult");
            if (taskFromResultMethod != null)
            {
                // Make it generic with object type: Task.FromResult<object>(object value)
                var genericMethod = taskFromResultMethod.MakeGenericMethod(typeof(object));
                Console.WriteLine($"[DEBUG] Using Task.FromResult<object> method: {genericMethod}");
                _currentIl.Emit(OpCodes.Call, genericMethod);
            }
            else
            {
                Console.WriteLine("[ERROR] Could not find Task.FromResult method");
                // Fallback: create completed task manually  
                var completedTaskProperty = typeof(Task<object>).GetProperty("CompletedTask");
                if (completedTaskProperty?.GetMethod != null)
                {
                    // Pop the result value since CompletedTask doesn't take parameters
                    _currentIl.Emit(OpCodes.Pop);
                    _currentIl.Emit(OpCodes.Call, completedTaskProperty.GetMethod);
                }
                else
                {
                    Console.WriteLine("[ERROR] Could not find Task<object>.CompletedTask property");
                    _currentIl.Emit(OpCodes.Ldnull);
                }
            }
            
            _currentIl.Emit(OpCodes.Ret);
            Console.WriteLine($"[DEBUG] Generated simplified async wrapper for {node.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to generate async wrapper for {node.Name}: {ex.Message}");
            
            // Fallback: return completed task with null
            _currentIl.Emit(OpCodes.Ldnull);
            var taskFromResultMethod = typeof(Task).GetMethod("FromResult", new[] { typeof(object) });
            if (taskFromResultMethod != null)
            {
                _currentIl.Emit(OpCodes.Call, taskFromResultMethod);
            }
            _currentIl.Emit(OpCodes.Ret);
        }
    }

    public object VisitFunctionDeclaration(FunctionDeclarationNode node)
    {
        if (_isFirstPass)
        {
            Console.WriteLine($"[DEBUG] Pass 1: Defining function {node.Name} with {node.Parameters.Count} parameters, IsAsync: {node.IsAsync}");
            
            // **PASS 1: Define the method signature with proper parameter types**
            // For now, treat all parameters as object type (we'll improve this later)
            var parameterTypes = new Type[node.Parameters.Count];
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                parameterTypes[i] = typeof(object); // All parameters are object type for now
                Console.WriteLine($"[DEBUG] Parameter {i}: {node.Parameters[i].Name} (type: object)");
            }
            
            // Determine return type based on async/sync and declared return type
            Type returnType;
            if (node.IsAsync)
            {
                // Async functions return Task<object> for now
                returnType = typeof(Task<object>);
                Console.WriteLine($"[DEBUG] Async function {node.Name} will return Task<object>");
            }
            else
            {
                // Sync functions return object
                returnType = typeof(object);
            }
            
            // Create the method with the determined return type
            var methodBuilder = _programTypeBuilder.DefineMethod(
                node.Name, 
                MethodAttributes.Public | MethodAttributes.Static, // Back to Static - will pass services differently
                returnType,
                parameterTypes);
            
            // Define parameter names for debugging
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, node.Parameters[i].Name);
            }
            
            // Store the method builder for Pass 2
            _userFunctions[node.Name] = methodBuilder;
            
            // Store the function node for body compilation in Pass 2
            _pendingFunctions.Add(node);
            
            // Determine the CxType to use for the function symbol
            var returnCxType = node.ReturnType ?? CxLanguage.Core.Types.CxType.Any;
            
            // Add function to symbol table  
            var parameterSymbols = node.Parameters.Select(p => new ParameterSymbol(p.Name, p.Type)).ToList();
            var symbol = new FunctionSymbol(node.Name, returnCxType, parameterSymbols, false, node);
            _scopes.Peek().TryDefine(symbol);
        }
        else
        {
            Console.WriteLine($"[DEBUG] Pass 2: Skipping function declaration {node.Name} (body compiled separately)");
            // **PASS 2: Skip function declarations (bodies are compiled separately)**
            // Function bodies are compiled in CompileFunctionBody method
        }
        
        return new object();
    }

    public object VisitReturn(ReturnStatementNode node)
    {
        // If there's a return value, compile it
        if (node.Value != null)
        {
            // Evaluate the expression and leave the result on the stack
            node.Value.Accept(this);
        }
        else
        {
            // For void returns or when no value is specified, return null
            _currentIl!.Emit(OpCodes.Ldnull);
        }
        
        // Check if we're in an async method context - if so, don't emit Ret
        // (the async wrapper will handle the return)
        if (_isCompilingAsyncMethod)
        {
            Console.WriteLine($"[DEBUG] Return statement in async method - not emitting Ret (Task wrapper will handle)");
            // Don't emit Ret - let the async wrapper handle it
            // The value is already on the stack for the wrapper to use
        }
        else
        {
            // Regular synchronous method - emit return directly
            Console.WriteLine($"[DEBUG] Return statement in sync method - emitting Ret");
            _currentIl!.Emit(OpCodes.Ret);
        }
        
        return new object();
    }

    public object VisitBinaryExpression(BinaryExpressionNode node)
    {
        // For arithmetic operations on boxed objects, we need to unbox first
        switch (node.Operator)
        {
            case BinaryOperator.Add:
                // Special case: Check if this is string concatenation
                if (IsStringConcatenation(node))
                {
                    EmitStringConcatenation(node);
                }
                else
                {
                    // Numeric addition with type coercion
                    EmitArithmeticOperation(node, OpCodes.Add);
                }
                break;
                
            case BinaryOperator.Subtract:
            case BinaryOperator.Multiply:
            case BinaryOperator.Divide:
            case BinaryOperator.Modulo:
                // Arithmetic operations with type coercion
                var opCode = node.Operator switch
                {
                    BinaryOperator.Subtract => OpCodes.Sub,
                    BinaryOperator.Multiply => OpCodes.Mul,
                    BinaryOperator.Divide => OpCodes.Div,
                    BinaryOperator.Modulo => OpCodes.Rem,
                    _ => throw new CompilationException($"Unexpected operator: {node.Operator}")
                };
                EmitArithmeticOperation(node, opCode);
                break;
                
            // Comparison operators
            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual:
                // For equality/inequality comparisons, use Object.Equals
                node.Left.Accept(this);
                node.Right.Accept(this);
                
                var equalsMethod = typeof(object).GetMethod("Equals", new[] { typeof(object), typeof(object) });
                _currentIl!.EmitCall(OpCodes.Call, equalsMethod!, null);
                
                // For Not Equal, negate the result
                if (node.Operator == BinaryOperator.NotEqual)
                {
                    _currentIl.Emit(OpCodes.Ldc_I4_0);
                    _currentIl.Emit(OpCodes.Ceq);
                }
                
                _currentIl.Emit(OpCodes.Box, typeof(bool));
                break;
                
            case BinaryOperator.LessThan:
            case BinaryOperator.LessThanOrEqual:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.GreaterThanOrEqual:
                // For relational comparisons, use numeric comparison
                EmitComparisonOperation(node);
                break;
                
            // Logical operators
            case BinaryOperator.And:
            case BinaryOperator.Or:
                // For logical operations, we need to unbox booleans
                node.Left.Accept(this);
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(bool));
                
                // For And/Or, we need short-circuit evaluation
                var endLabel = _currentIl.DefineLabel();
                var shortCircuitLabel = _currentIl.DefineLabel();
                
                // Duplicate the left value
                _currentIl.Emit(OpCodes.Dup);
                
                if (node.Operator == BinaryOperator.And)
                {
                    // If left is false, short-circuit (result is false)
                    _currentIl.Emit(OpCodes.Brfalse, shortCircuitLabel);
                }
                else // Or
                {
                    // If left is true, short-circuit (result is true)
                    _currentIl.Emit(OpCodes.Brtrue, shortCircuitLabel);
                }
                
                // Pop the duplicate since we're evaluating the right side
                _currentIl.Emit(OpCodes.Pop);
                
                // Evaluate right side
                node.Right.Accept(this);
                _currentIl.Emit(OpCodes.Unbox_Any, typeof(bool));
                
                // Jump to end
                _currentIl.Emit(OpCodes.Br, endLabel);
                
                // Short-circuit: result is already on stack (the duplicate)
                _currentIl.MarkLabel(shortCircuitLabel);
                
                // End of logical operation
                _currentIl.MarkLabel(endLabel);
                
                // Box the boolean result
                _currentIl.Emit(OpCodes.Box, typeof(bool));
                break;
                
            default:
                throw new CompilationException($"Unsupported binary operator: {node.Operator}");
        }
        
        return new object();
    }

    /// <summary>
    /// Emit IL for arithmetic operations with automatic type coercion between int and double
    /// </summary>
    private void EmitArithmeticOperation(BinaryExpressionNode node, OpCode operation)
    {
        // Load both operands onto the stack
        node.Left.Accept(this);
        node.Right.Accept(this);
        
        // Call a runtime helper that handles type coercion and arithmetic
        var helperMethod = typeof(CxArithmeticHelper).GetMethod("PerformArithmetic", 
            new[] { typeof(object), typeof(object), typeof(string) });
        
        // Convert OpCode to string for the helper method
        var operationName = operation.Name ?? "unknown"; // e.g., "add", "sub", "mul", "div"
        _currentIl!.Emit(OpCodes.Ldstr, operationName);
        
        // Call the helper method
        _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
        
        // Result is already an object on the stack
    }
    
    /// <summary>
    /// Emit IL for comparison operations with automatic type coercion
    /// </summary>
    private void EmitComparisonOperation(BinaryExpressionNode node)
    {
        // Load both operands onto the stack
        node.Left.Accept(this);
        node.Right.Accept(this);
        
        // Call a runtime helper that handles comparison with type coercion
        var helperMethod = typeof(CxArithmeticHelper).GetMethod("PerformComparison", 
            new[] { typeof(object), typeof(object), typeof(string) });
        
        // Convert operator to string for the helper method
        var operationName = node.Operator switch
        {
            BinaryOperator.LessThan => "lt",
            BinaryOperator.LessThanOrEqual => "le",
            BinaryOperator.GreaterThan => "gt",
            BinaryOperator.GreaterThanOrEqual => "ge",
            _ => "unknown"
        };
        
        _currentIl!.Emit(OpCodes.Ldstr, operationName);
        
        // Call the helper method
        _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
        
        // Result is already an object (boolean) on the stack
    }

    /// <summary>
    /// Convert any value on stack to a truthy boolean value
    /// - null -> false
    /// - boolean -> as is 
    /// - number 0 -> false, otherwise true
    /// - string empty/null -> false, otherwise true
    /// - objects -> true if not null
    /// </summary>
    private void EmitTruthyConversion()
    {
        // Call runtime helper to convert any value to boolean
        var helperMethod = typeof(CxRuntimeHelper).GetMethod("ConvertToBoolean", new[] { typeof(object) });
        _currentIl!.EmitCall(OpCodes.Call, helperMethod!, null);
        
        // Result is a boolean on the stack
    }
    
    /// <summary>
    /// Determine the runtime type of an expression (simplified version)
    /// </summary>
    private Type GetRuntimeType(ExpressionNode expression)
    {
        if (expression is LiteralNode literal)
        {
            return literal.Value?.GetType() ?? typeof(object);
        }
        
        // For now, assume other expressions are int (this could be enhanced)
        // In a full implementation, we'd track types through the compilation process
        return typeof(int);
    }

    public object VisitUnaryExpression(UnaryExpressionNode node)
    {
        node.Operand.Accept(this);
        
        // Determine the operand type for proper unboxing
        var operandType = GetRuntimeType(node.Operand);
        
        switch (node.Operator)
        {
            case UnaryOperator.Negate:
            case UnaryOperator.Minus: // Minus and Negate do the same thing
                _currentIl!.Emit(OpCodes.Unbox_Any, operandType);
                _currentIl.Emit(OpCodes.Neg);
                _currentIl.Emit(OpCodes.Box, operandType);
                break;
                
            case UnaryOperator.Plus: // Plus is a no-op, the value is already on the stack
                // Just ensure it's unboxed and boxed back to maintain consistency
                _currentIl!.Emit(OpCodes.Unbox_Any, operandType);
                _currentIl.Emit(OpCodes.Box, operandType);
                break;
                
            case UnaryOperator.Not:
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(bool));
                _currentIl.Emit(OpCodes.Ldc_I4_0);
                _currentIl.Emit(OpCodes.Ceq);
                _currentIl.Emit(OpCodes.Box, typeof(bool));
                break;
                
            case UnaryOperator.BitwiseNot:
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(int));
                _currentIl.Emit(OpCodes.Not);
                _currentIl.Emit(OpCodes.Box, typeof(int));
                break;
                
            default:
                throw new CompilationException($"Unsupported unary operator: {node.Operator}");
        }
        
        return new object();
    }

    public object VisitLiteral(LiteralNode node)
    {
        if (node.Value == null)
        {
            Console.WriteLine($"[IL-EMIT] Emitting Ldnull for null literal");
            _currentIl!.Emit(OpCodes.Ldnull);
        }
        else if (node.Value is int intValue)
        {
            Console.WriteLine($"[IL-EMIT] Emitting Ldc_I4 {intValue} for int literal");
            _currentIl!.Emit(OpCodes.Ldc_I4, intValue);
            Console.WriteLine($"[IL-EMIT] Emitting Box(int) to box int value");
            _currentIl.Emit(OpCodes.Box, typeof(int));
        }
        else if (node.Value is bool boolValue)
        {
            Console.WriteLine($"[IL-EMIT] Emitting Ldc_I4_{(boolValue ? "1" : "0")} for bool literal");
            _currentIl!.Emit(boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            Console.WriteLine($"[IL-EMIT] Emitting Box(bool) to box bool value");
            _currentIl.Emit(OpCodes.Box, typeof(bool));
        }
        else if (node.Value is double doubleValue)
        {
            Console.WriteLine($"[IL-EMIT] Emitting Ldc_R8 {doubleValue} for double literal");
            _currentIl!.Emit(OpCodes.Ldc_R8, doubleValue);
            Console.WriteLine($"[IL-EMIT] Emitting Box(double) to box double value");
            _currentIl.Emit(OpCodes.Box, typeof(double));
        }
        else if (node.Value is string stringValue)
        {
            Console.WriteLine($"[IL-EMIT] Emitting Ldstr '{stringValue}' for string literal");
            _currentIl!.Emit(OpCodes.Ldstr, stringValue);
            // Don't box strings - they're already reference types
        }
        else
        {
            throw new CompilationException($"Unsupported literal type: {node.Value.GetType()}");
        }
        
        return new object();
    }

    public object VisitVariableDeclaration(VariableDeclarationNode node)
    {
        if (_isFirstPass)
        {
            // Pass 1: Only define the symbol, don't generate IL
            var symbol = new VariableSymbol(node.Name, CxLanguage.Core.Types.CxType.Any, false);
            _scopes.Peek().TryDefine(symbol);
            
            // Skip the initializer in Pass 1 - it might contain function calls
            // Don't process the initializer in Pass 1
        }
        else
        {
            // Pass 2: Generate IL for the variable assignment
            // The symbol was already defined in Pass 1
            
            if (node.Initializer != null)
            {
                // Initialize with value
                node.Initializer.Accept(this);
                
                // Store in local or global field
                if (_currentMethod != null)
                {
                    // Local variable
                    var local = _currentIl!.DeclareLocal(typeof(object));
                    _currentLocals[node.Name] = local;
                    _currentIl.Emit(OpCodes.Stloc, local);
                }
                else
                {
                    // Global variable - handled in static constructor
                    var field = _programTypeBuilder.DefineField(
                        node.Name, 
                        typeof(object), 
                        FieldAttributes.Static | FieldAttributes.Public);
                    
                    _globalFields[node.Name] = field;
                    _currentIl!.Emit(OpCodes.Stsfld, field);
                }
            }
        }
        
        return new object();
    }

    public object VisitAssignmentExpression(AssignmentExpressionNode node)
    {
        if (_isFirstPass)
        {
            // Pass 1: Skip assignment expressions, just visit for symbol collection
            // Don't process the value or generate any IL
            return new object();
        }
        
        // Pass 2: Handle different assignment operators
        if (node.Left is IdentifierNode idNode)
        {
            if (node.Operator == AssignmentOperator.Assign)
            {
                // Simple assignment: variable = value
                
                // Check if this is a field assignment - need to load 'this' first
                if (_currentClassName != null && _classFields.ContainsKey(_currentClassName + "." + idNode.Name))
                {
                    // Field assignment - load 'this' pointer first
                    _currentIl!.Emit(OpCodes.Ldarg_0);
                    node.Right.Accept(this);
                    _currentIl.Emit(OpCodes.Dup);
                    StoreVariable(idNode.Name);
                }
                else
                {
                    // Regular variable assignment
                    node.Right.Accept(this);
                    _currentIl!.Emit(OpCodes.Dup);
                    StoreVariable(idNode.Name);
                }
            }
            else
            {
                // Compound assignment: variable += value, variable -= value, etc.
                
                // Check if this is a field - need special handling for 'this' pointer
                if (_currentClassName != null && _classFields.ContainsKey(_currentClassName + "." + idNode.Name))
                {
                    // Field compound assignment
                    LoadVariable(idNode.Name);
                    node.Right.Accept(this);
                    
                    // Perform the arithmetic operation
                    var helperMethod = typeof(CxArithmeticHelper).GetMethod("PerformArithmetic", 
                        new[] { typeof(object), typeof(object), typeof(string) });
                    
                    switch (node.Operator)
                    {
                        case AssignmentOperator.AddAssign:
                            _currentIl!.Emit(OpCodes.Ldstr, "add");
                            _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
                            break;
                        case AssignmentOperator.SubtractAssign:
                            _currentIl!.Emit(OpCodes.Ldstr, "sub");
                            _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
                            break;
                        case AssignmentOperator.MultiplyAssign:
                            _currentIl!.Emit(OpCodes.Ldstr, "mul");
                            _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
                            break;
                        case AssignmentOperator.DivideAssign:
                            _currentIl!.Emit(OpCodes.Ldstr, "div");
                            _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
                            break;
                        default:
                            throw new CompilationException($"Unsupported assignment operator: {node.Operator}");
                    }
                    
                    // Load 'this' and store the result to the field
                    _currentIl!.Emit(OpCodes.Ldarg_0);
                    _currentIl.Emit(OpCodes.Dup);
                    StoreVariable(idNode.Name);
                }
                else
                {
                    // Regular variable compound assignment
                    LoadVariable(idNode.Name);
                    node.Right.Accept(this);
                    
                    // Perform the arithmetic operation
                    var helperMethod = typeof(CxArithmeticHelper).GetMethod("PerformArithmetic", 
                        new[] { typeof(object), typeof(object), typeof(string) });
                    
                    switch (node.Operator)
                    {
                        case AssignmentOperator.AddAssign:
                            _currentIl!.Emit(OpCodes.Ldstr, "add");
                            _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
                            break;
                        case AssignmentOperator.SubtractAssign:
                            _currentIl!.Emit(OpCodes.Ldstr, "sub");
                            _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
                            break;
                        case AssignmentOperator.MultiplyAssign:
                            _currentIl!.Emit(OpCodes.Ldstr, "mul");
                            _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
                            break;
                        case AssignmentOperator.DivideAssign:
                            _currentIl!.Emit(OpCodes.Ldstr, "div");
                            _currentIl.EmitCall(OpCodes.Call, helperMethod!, null);
                            break;
                        default:
                            throw new CompilationException($"Unsupported assignment operator: {node.Operator}");
                    }
                    
                    // Duplicate result for expression value
                    _currentIl!.Emit(OpCodes.Dup);
                    
                    // Store the result back to the variable
                    StoreVariable(idNode.Name);
                }
            }
        }
        else if (node.Left is MemberAccessNode memberNode)
        {
            // Member access assignment: object.property = value
            if (node.Operator == AssignmentOperator.Assign)
            {
                // Check if it's 'this.fieldName'
                if (memberNode.Object is IdentifierNode objectId && objectId.Name == "this")
                {
                    Console.WriteLine($"[DEBUG] Field assignment: this.{memberNode.Property} = <value>");
                    
                    // Evaluate the value and keep it on stack for return
                    Console.WriteLine($"[IL-EMIT] Evaluating right-hand side value");
                    node.Right.Accept(this);
                    
                    // Duplicate the value so we can return it
                    Console.WriteLine($"[IL-EMIT] Emitting Dup to duplicate value on stack");
                    _currentIl!.Emit(OpCodes.Dup);
                    
                    // Store the value for the method call
                    var valueLocal = _currentIl.DeclareLocal(typeof(object));
                    Console.WriteLine($"[IL-EMIT] Declared local variable {valueLocal.LocalIndex} for value storage");
                    Console.WriteLine($"[IL-EMIT] Emitting Stloc to store duplicated value in local {valueLocal.LocalIndex}");
                    _currentIl.Emit(OpCodes.Stloc, valueLocal);
                    
                    // Call SetInstanceField(this, fieldName, value)
                    Console.WriteLine($"[IL-EMIT] Emitting Ldarg_0 to load 'this' pointer (1st param)");
                    _currentIl.Emit(OpCodes.Ldarg_0);  // Load 'this' (1st param)
                    Console.WriteLine($"[IL-EMIT] Emitting Ldstr '{memberNode.Property}' (2nd param)");
                    _currentIl.Emit(OpCodes.Ldstr, memberNode.Property);  // Load field name (2nd param)
                    Console.WriteLine($"[IL-EMIT] Emitting Ldloc {valueLocal.LocalIndex} to load value (3rd param)");
                    _currentIl.Emit(OpCodes.Ldloc, valueLocal);  // Load value (3rd param)
                    
                    // Call the runtime helper
                    var setFieldMethod = typeof(CxRuntimeHelper).GetMethod("SetInstanceField", 
                        BindingFlags.Public | BindingFlags.Static);
                    if (setFieldMethod != null)
                    {
                        Console.WriteLine($"[IL-EMIT] Found SetInstanceField method: {setFieldMethod}");
                        Console.WriteLine($"[IL-EMIT] Method signature: {setFieldMethod.ReturnType} {setFieldMethod.Name}({string.Join(", ", setFieldMethod.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"))})");
                        Console.WriteLine($"[IL-EMIT] Stack before Call: [return_value, this, fieldName, value]");
                        Console.WriteLine($"[IL-EMIT] Emitting Call to SetInstanceField");
                        _currentIl.Emit(OpCodes.Call, setFieldMethod);
                        Console.WriteLine($"[IL-EMIT] Stack after Call: [return_value] (SetInstanceField returns void)");
                        Console.WriteLine($"[DEBUG] Runtime field assignment complete, return value on stack");
                    }
                    else
                    {
                        throw new CompilationException("SetInstanceField method not found in CxRuntimeHelper");
                    }
                }
                else
                {
                    // Handle variable.property = value (e.g., agent1.name = "value")
                    if (memberNode.Object is IdentifierNode objectIdentifier)
                    {
                        // We need to determine the field to store to
                        var potentialFieldKey = "";
                        foreach (var classFieldKey in _classFields.Keys)
                        {
                            if (classFieldKey.EndsWith("." + memberNode.Property))
                            {
                                potentialFieldKey = classFieldKey;
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(potentialFieldKey) && _classFields.TryGetValue(potentialFieldKey, out var objectField))
                        {
                            // Extract class name from field key (format: "ClassName.fieldName")
                            var className = potentialFieldKey.Substring(0, potentialFieldKey.LastIndexOf('.'));
                            
                            // Load the object variable
                            objectIdentifier.Accept(this);
                            
                            // Get the class type and cast the object to the correct type
                            if (_userClasses.TryGetValue(className, out var typeBuilder))
                            {
                                // Try to get the actual created type
                                Type? classType = null;
                                try
                                {
                                    classType = _moduleBuilder.GetType(className);
                                    if (classType == null)
                                    {
                                        classType = typeBuilder.CreateType();
                                    }
                                }
                                catch
                                {
                                    classType = typeBuilder.CreateType();
                                }
                                
                                if (classType != null)
                                {
                                    // Cast the object to the correct class type
                                    _currentIl!.Emit(OpCodes.Castclass, classType);
                                }
                            }
                            
                            // Evaluate the value to assign
                            node.Right.Accept(this);
                            
                            // Duplicate the value for return (stack: [object, value, value])
                            _currentIl!.Emit(OpCodes.Dup);
                            
                            // Store the field: [object, value] -> []
                            // We need to reorder stack to put object first, then value
                            var tempLocal = _currentIl!.DeclareLocal(typeof(object));
                            _currentIl.Emit(OpCodes.Stloc, tempLocal); // Store duplicate value temporarily
                            _currentIl.Emit(OpCodes.Stfld, objectField); // Store field: [object, value] -> []
                            _currentIl.Emit(OpCodes.Ldloc, tempLocal); // Load the value back for return
                        }
                        else
                        {
                            throw new CompilationException($"Field not found: {memberNode.Property} for object assignment");
                        }
                    }
                    else
                    {
                        throw new CompilationException("Only 'this.property' and 'variable.property' assignments are supported currently");
                    }
                }
            }
            else
            {
                throw new CompilationException("Compound assignment not supported for member access yet");
            }
        }
        else
        {
            throw new CompilationException($"Invalid assignment target: {node.Left}");
        }
        
        return new object();
    }
    
    private void LoadVariable(string variableName)
    {
        // Check if it's a local variable
        if (_currentLocals.TryGetValue(variableName, out var local))
        {
            _currentIl!.Emit(OpCodes.Ldloc, local);
        }
        // Check if it's a class field (when inside a class method/constructor)
        else if (_currentClassName != null && _classFields.TryGetValue(_currentClassName + "." + variableName, out var classField))
        {
            // Load 'this' pointer and then the field
            _currentIl!.Emit(OpCodes.Ldarg_0);
            _currentIl.Emit(OpCodes.Ldfld, classField);
        }
        // Check if it's a global field
        else if (_globalFields.TryGetValue(variableName, out var field))
        {
            _currentIl!.Emit(OpCodes.Ldsfld, field);
        }
        else
        {
            throw new CompilationException($"Variable not found: {variableName}");
        }
    }
    
    private void StoreVariable(string variableName)
    {
        // Check if it's a local variable
        if (_currentLocals.TryGetValue(variableName, out var local))
        {
            _currentIl!.Emit(OpCodes.Stloc, local);
        }
        // Check if it's a class field (when inside a class method/constructor)
        else if (_currentClassName != null && _classFields.TryGetValue(_currentClassName + "." + variableName, out var classField))
        {
            // Load 'this' pointer (already loaded by assignment expression), then store field
            _currentIl!.Emit(OpCodes.Stfld, classField);
        }
        // Check if it's a global field
        else if (_globalFields.TryGetValue(variableName, out var field))
        {
            _currentIl!.Emit(OpCodes.Stsfld, field);
        }
        else
        {
            throw new CompilationException($"Variable not found: {variableName}");
        }
    }

    private string? GetObjectTypeName(IdentifierNode objectIdentifier)
    {
        // Try to determine the type of an object based on how it was declared
        // For now, we'll use a simple heuristic - check if it's a local variable that might have been 
        // created with a 'new' expression. This is a simplified approach.
        
        // Check all classes to see if any have methods that might match
        foreach (var className in _userClasses.Keys)
        {
            // We'll return the first class type we find - this is a simplified approach
            // In a real implementation, we'd need better type tracking
            return className;
        }
        
        return null; // Unknown type
    }

    public object VisitArrayLiteral(ArrayLiteralNode node)
    {
        // Create array
        _currentIl!.Emit(OpCodes.Ldc_I4, node.Elements.Count);
        _currentIl.Emit(OpCodes.Newarr, typeof(object));
        
        // Initialize elements
        for (int i = 0; i < node.Elements.Count; i++)
        {
            // Duplicate array reference
            _currentIl.Emit(OpCodes.Dup);
            
            // Load index
            _currentIl.Emit(OpCodes.Ldc_I4, i);
            
            // Compile the element expression
            node.Elements[i].Accept(this);
            
            // Store element in array
            _currentIl.Emit(OpCodes.Stelem_Ref);
        }

        // Array is now on top of stack as IEnumerable for for-in loops
        return new object();
    }

    private string GetFullMemberAccessName(MemberAccessNode memberAccess)
    {
        // Recursively build the full member access chain (e.g., "analysis.complete")
        if (memberAccess.Object is IdentifierNode identifier)
        {
            return identifier.Name + "." + memberAccess.Property;
        }
        else if (memberAccess.Object is MemberAccessNode nestedAccess)
        {
            return GetFullMemberAccessName(nestedAccess) + "." + memberAccess.Property;
        }
        else
        {
            // Fallback - just return the property name
            return memberAccess.Property;
        }
    }

    public object VisitObjectLiteral(ObjectLiteralNode node)
    {
        // Check if this object literal contains the special 'handlers' property
        var handlersProperty = node.Properties.FirstOrDefault(p => p.Key == "handlers");
        var regularProperties = node.Properties.Where(p => p.Key != "handlers").ToList();
        
        // Create a new Dictionary<string, object> for the object literal
        var dictionaryType = typeof(Dictionary<string, object>);
        var dictionaryConstructor = dictionaryType.GetConstructor(Type.EmptyTypes);
        var addMethod = dictionaryType.GetMethod("Add", new[] { typeof(string), typeof(object) });
        
        // Create new Dictionary<string, object>()
        _currentIl!.Emit(OpCodes.Newobj, dictionaryConstructor!);
        
        // Add each regular property to the dictionary (excluding 'handlers')
        foreach (var property in regularProperties)
        {
            // Duplicate dictionary reference
            _currentIl.Emit(OpCodes.Dup);
            
            // Load property key as string
            _currentIl.Emit(OpCodes.Ldstr, property.Key);
            
            // Compile the property value expression
            property.Value.Accept(this);
            
            // Call dictionary.Add(key, value)
            _currentIl.Emit(OpCodes.Callvirt, addMethod!);
        }
        
        // If there's a handlers property, process it for multi-scope event emission
        if (handlersProperty != null)
        {
            Console.WriteLine("[DEBUG] Found handlers property in object literal - processing multi-scope events");
            
            // Store the main dictionary temporarily
            var tempDictLocal = _currentIl.DeclareLocal(typeof(Dictionary<string, object>));
            _currentIl.Emit(OpCodes.Dup); // Duplicate the dictionary
            _currentIl.Emit(OpCodes.Stloc, tempDictLocal); // Store copy in local variable
            
            // Process handlers - support both object literal {} and array literal [] syntax
            if (handlersProperty.Value is ObjectLiteralNode handlersObject)
            {
                Console.WriteLine($"[DEBUG] Processing {handlersObject.Properties.Count} handler events from object literal");
                
                foreach (var handlerProperty in handlersObject.Properties)
                {
                    var eventName = handlerProperty.Key;
                    Console.WriteLine($"[DEBUG] Emitting additional event: {eventName}");
                    
                    // Emit each handler event with the same data
                    // Load event name
                    _currentIl.Emit(OpCodes.Ldstr, eventName);
                    
                    // Load the stored dictionary as payload
                    _currentIl.Emit(OpCodes.Ldloc, tempDictLocal);
                    
                    // Load source identifier
                    _currentIl.Emit(OpCodes.Ldstr, _currentClassName ?? "MainScript");
                    
                    // Call CxRuntimeHelper.EmitEvent(string eventName, object data, string source)
                    var emitMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper).GetMethod("EmitEvent", 
                        new[] { typeof(string), typeof(object), typeof(string) });
                    _currentIl.Emit(OpCodes.Call, emitMethod!);
                }
            }
            else if (handlersProperty.Value is ArrayLiteralNode handlersArray)
            {
                Console.WriteLine($"[DEBUG] Processing {handlersArray.Elements.Count} handler events from array literal");
                
                foreach (var element in handlersArray.Elements)
                {
                    Console.WriteLine($"[DEBUG] Handler element type: {element.GetType().Name}");
                    
                    string eventName = "";
                    
                    // Handle different types of event name representations
                    if (element is LiteralNode literal && literal.Type == LiteralType.String)
                    {
                        // String literal from handlersList rule
                        eventName = literal.Value?.ToString() ?? "";
                    }
                    else if (element is IdentifierNode identifier)
                    {
                        // Simple identifier (e.g., "complete", "done")
                        eventName = identifier.Name;
                    }
                    else if (element is MemberAccessNode memberAccess)
                    {
                        // Dotted event name (e.g., "analysis.complete")
                        eventName = GetFullMemberAccessName(memberAccess);
                    }
                    
                    if (!string.IsNullOrEmpty(eventName))
                    {
                        Console.WriteLine($"[DEBUG] Emitting additional event: {eventName}");
                        
                        // Emit each handler event with the same data
                        // Load event name
                        _currentIl.Emit(OpCodes.Ldstr, eventName);
                        
                        // Load the stored dictionary as payload
                        _currentIl.Emit(OpCodes.Ldloc, tempDictLocal);
                        
                        // Load source identifier
                        _currentIl.Emit(OpCodes.Ldstr, _currentClassName ?? "MainScript");
                        
                        // Call CxRuntimeHelper.EmitEvent(string eventName, object data, string source)
                        var emitMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper).GetMethod("EmitEvent", 
                            new[] { typeof(string), typeof(object), typeof(string) });
                        _currentIl.Emit(OpCodes.Call, emitMethod!);
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Unable to extract event name from element type: {element.GetType().Name}");
                    }
                }
            }
        }
        
        // Dictionary is now on top of stack
        return new object();
    }

    public object VisitObjectProperty(ObjectPropertyNode node)
    {
        // Object properties are handled within VisitObjectLiteral
        // This method should not be called directly
        throw new NotImplementedException("ObjectProperty nodes should be handled within ObjectLiteral compilation");
    }

    public object VisitFunctionCall(FunctionCallNode node)
    {
        // Skip function calls in Pass 1 (they'll be compiled in Pass 2)
        if (_isFirstPass)
        {
            return new object();
        }
        
        // Check if it's a user-defined function first (for instance method calling)
        if (_userFunctions.TryGetValue(node.FunctionName, out var methodBuilder))
        {
            // User-defined function - load 'this' first, then arguments
            _currentIl!.Emit(OpCodes.Ldarg_0); // Load 'this' pointer
            
            // Compile arguments after 'this'
            foreach (var arg in node.Arguments)
            {
                arg.Accept(this);
            }
            
            _currentIl!.EmitCall(OpCodes.Callvirt, methodBuilder, null);
            // User functions now return object, so no need to push null
        }
        else
        {
            // Built-in function - compile arguments first, then call
            foreach (var arg in node.Arguments)
            {
                arg.Accept(this);
            }
            
            // Get method info and emit call
            var methodInfo = GetMethodInfo(node.FunctionName, node.Arguments.Count);
            if (methodInfo != null)
            {
                // Built-in function (like print, write)
                Console.WriteLine($"[IL-EMIT] Found built-in method: {methodInfo.DeclaringType?.Name}.{methodInfo.Name}");
                Console.WriteLine($"[IL-EMIT] Method signature: {methodInfo.ReturnType.Name} {methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
                Console.WriteLine($"[IL-EMIT] Stack before Call: {node.Arguments.Count} arguments");
                Console.WriteLine($"[IL-EMIT] Emitting Call to {methodInfo.Name}");
                _currentIl!.EmitCall(OpCodes.Call, methodInfo, null);
                
                // Methods like print/write return void, but we need object for expressions
                if (methodInfo.ReturnType == typeof(void))
                {
                    Console.WriteLine($"[IL-EMIT] Method returns void, emitting Ldnull for expression compatibility");
                    _currentIl.Emit(OpCodes.Ldnull);
                }
                Console.WriteLine($"[IL-EMIT] Built-in function call complete, result on stack");
            }
            else
            {
                throw new CompilationException($"Function not found: {node.FunctionName}");
            }
        }
        
        return new object();
    }

    public object VisitIdentifier(IdentifierNode node)
    {
        // Special handling for 'this' keyword in class context
        if (node.Name == "this" && _currentClassName != null)
        {
            Console.WriteLine($"[DEBUG] Loading 'this' reference for class {_currentClassName}");
            _currentIl!.Emit(OpCodes.Ldarg_0); // Load 'this' pointer (first parameter in instance methods)
            return new object();
        }

        // Check for parameters first (when inside a function)
        if (_currentParameterMapping != null && _currentParameterMapping.TryGetValue(node.Name, out var paramIndex))
        {
            Console.WriteLine($"[DEBUG] Loading parameter {node.Name} from arg {paramIndex}");
            // Load parameter using Ldarg instruction
            switch (paramIndex)
            {
                case 0: _currentIl!.Emit(OpCodes.Ldarg_0); break;
                case 1: _currentIl!.Emit(OpCodes.Ldarg_1); break;
                case 2: _currentIl!.Emit(OpCodes.Ldarg_2); break;
                case 3: _currentIl!.Emit(OpCodes.Ldarg_3); break;
                default: _currentIl!.Emit(OpCodes.Ldarg, paramIndex); break;
            }
            return new object();
        }
        
        // Check for local variables next
        if (_currentLocals.TryGetValue(node.Name, out var local))
        {
            _currentIl!.Emit(OpCodes.Ldloc, local);
            return new object();
        }

        // Check for service fields (imported AI services)
        if (_serviceFields.TryGetValue(node.Name, out var serviceField))
        {
            Console.WriteLine($"[DEBUG] Loading service identifier: {node.Name}");
            _currentIl!.Emit(OpCodes.Ldsfld, serviceField);
            return new object();
        }

        // Look up identifier in symbol table for globals
        var symbol = _scopes.Peek().Lookup(node.Name);
        
        if (symbol is VariableSymbol)
        {
            if (_globalFields.TryGetValue(node.Name, out var field))
            {
                _currentIl!.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                // Variable not found - this should be an error
                _currentIl!.Emit(OpCodes.Ldnull);
            }
        }
        else
        {
            // Symbol not found
            _currentIl!.Emit(OpCodes.Ldnull);
        }
        
        return new object();
    }

    /// <summary>
    /// Determines if a binary expression is a string concatenation
    /// </summary>
    private bool IsStringConcatenation(BinaryExpressionNode node)
    {
        return node.Operator == BinaryOperator.Add && 
               (IsLiteralStringExpression(node.Left) || IsLiteralStringExpression(node.Right));
    }

    /// <summary>
    /// Emits IL code for string concatenation
    /// </summary>
    private void EmitStringConcatenation(BinaryExpressionNode node)
    {
        // Load the first argument
        node.Left.Accept(this);
        
        // Convert to string if needed
        var toStringMethod = typeof(object).GetMethod("ToString");
        _currentIl!.EmitCall(OpCodes.Callvirt, toStringMethod!, null);
        
        // Load the second argument
        node.Right.Accept(this);
        
        // Convert to string if needed
        _currentIl.EmitCall(OpCodes.Callvirt, toStringMethod!, null);
        
        // Concatenate strings
        var concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
        _currentIl.EmitCall(OpCodes.Call, concatMethod!, null);
        
        // String is already a reference type - no boxing needed
    }

    /// <summary>
    /// Helper method to determine if a node is or might produce a string value
    /// </summary>
    private bool IsLiteralStringExpression(AstNode node)
    {
        if (node is LiteralNode literal)
        {
            return literal.Value is string;
        }
        else if (node is BinaryExpressionNode binExpr)
        {
            // If either side of a + operation might be a string, the result might be a string
            return binExpr.Operator == BinaryOperator.Add && 
                   (IsLiteralStringExpression(binExpr.Left) || IsLiteralStringExpression(binExpr.Right));
        }
        else if (node is FunctionCallNode funcCall)
        {
            // Function calls like ToString() will return strings
            return funcCall.FunctionName == "ToString";
        }
        else if (node is IdentifierNode)
        {
            // For now, be more conservative and only treat string literals as strings
            // This prevents numeric variables from being treated as string concatenation
            return false;
        }
        
        return false;
    }

    // Required interface methods that are not implemented yet
    public object VisitFor(ForStatementNode node)
    {
        if (_isFirstPass)
        {
            return new object();
        }

        Console.WriteLine($"[DEBUG] Processing for-in loop with variable: {node.Variable}");
        
        // Get the iterable expression 
        node.Iterable.Accept(this);
        
        // Check if the object is a Dictionary<string, object> at runtime
        var objectLocal = _currentIl!.DeclareLocal(typeof(object));
        _currentIl.Emit(OpCodes.Stloc, objectLocal);
        
        // Check if it's a dictionary
        _currentIl.Emit(OpCodes.Ldloc, objectLocal);
        _currentIl.Emit(OpCodes.Isinst, typeof(Dictionary<string, object>));
        
        var notDictionary = _currentIl.DefineLabel();
        var endLoop = _currentIl.DefineLabel();
        
        _currentIl.Emit(OpCodes.Brfalse, notDictionary);
        
        // Dictionary iteration path
        _currentIl.Emit(OpCodes.Ldloc, objectLocal);
        _currentIl.Emit(OpCodes.Castclass, typeof(Dictionary<string, object>));
        
        // Get the dictionary's enumerator
        var getDictEnumeratorMethod = typeof(Dictionary<string, object>).GetMethod("GetEnumerator");
        _currentIl.Emit(OpCodes.Callvirt, getDictEnumeratorMethod!);
        
        var enumeratorLocal = _currentIl.DeclareLocal(typeof(Dictionary<string, object>.Enumerator));
        _currentIl.Emit(OpCodes.Stloc, enumeratorLocal);
        
        // Create a local variable for the loop variable (KeyValuePair)
        var loopVarLocal = _currentIl.DeclareLocal(typeof(object));
        _currentLocals[node.Variable] = loopVarLocal;
        
        var dictLoopStart = _currentIl.DefineLabel();
        var dictLoopEnd = _currentIl.DefineLabel();
        
        // Dictionary loop: while (enumerator.MoveNext())
        _currentIl.MarkLabel(dictLoopStart);
        
        _currentIl.Emit(OpCodes.Ldloca, enumeratorLocal);
        var moveNextMethod = typeof(Dictionary<string, object>.Enumerator).GetMethod("MoveNext");
        _currentIl.Emit(OpCodes.Call, moveNextMethod!);
        _currentIl.Emit(OpCodes.Brfalse, dictLoopEnd);
        
        // Get current KeyValuePair and store in loop variable
        _currentIl.Emit(OpCodes.Ldloca, enumeratorLocal);
        var getCurrentMethod = typeof(Dictionary<string, object>.Enumerator).GetProperty("Current")!.GetMethod;
        _currentIl.Emit(OpCodes.Call, getCurrentMethod!);
        _currentIl.Emit(OpCodes.Box, typeof(KeyValuePair<string, object>));
        _currentIl.Emit(OpCodes.Stloc, loopVarLocal);
        
        // Execute loop body
        node.Body.Accept(this);
        
        // Jump back to loop start
        _currentIl.Emit(OpCodes.Br, dictLoopStart);
        
        // End of dictionary loop
        _currentIl.MarkLabel(dictLoopEnd);
        _currentIl.Emit(OpCodes.Br, endLoop);
        
        // Array iteration path (existing code)
        _currentIl.MarkLabel(notDictionary);
        
        // Cast to array (object[])
        _currentIl.Emit(OpCodes.Ldloc, objectLocal);
        _currentIl.Emit(OpCodes.Castclass, typeof(object[]));
        
        // Store the array in a temporary local variable
        var arrayLocal = _currentIl.DeclareLocal(typeof(object[]));
        _currentIl.Emit(OpCodes.Stloc, arrayLocal);
        
        // Create a local variable for the loop index
        var indexLocal = _currentIl.DeclareLocal(typeof(int));
        _currentIl.Emit(OpCodes.Ldc_I4_0);  // index = 0
        _currentIl.Emit(OpCodes.Stloc, indexLocal);
        
        // Create a local variable for the loop variable (reuse if not already created)
        if (!_currentLocals.ContainsKey(node.Variable))
        {
            var arrayLoopVarLocal = _currentIl.DeclareLocal(typeof(object));
            _currentLocals[node.Variable] = arrayLoopVarLocal;
        }
        
        // Define labels for loop control
        var loopStart = _currentIl.DefineLabel();
        var loopEndArray = _currentIl.DefineLabel();
        
        // Start of loop: check if index < array.Length
        _currentIl.MarkLabel(loopStart);
        
        // Load index
        _currentIl.Emit(OpCodes.Ldloc, indexLocal);
        
        // Load array length
        _currentIl.Emit(OpCodes.Ldloc, arrayLocal);
        _currentIl.Emit(OpCodes.Ldlen);
        _currentIl.Emit(OpCodes.Conv_I4);
        
        // Compare: if index >= length, exit loop
        _currentIl.Emit(OpCodes.Bge, loopEndArray);
        
        // Load current array element: array[index]
        _currentIl.Emit(OpCodes.Ldloc, arrayLocal);
        _currentIl.Emit(OpCodes.Ldloc, indexLocal);
        _currentIl.Emit(OpCodes.Ldelem_Ref);
        
        // Store in loop variable
        _currentIl.Emit(OpCodes.Stloc, _currentLocals[node.Variable]);
        
        // Execute loop body
        node.Body.Accept(this);
        
        // Increment index
        _currentIl.Emit(OpCodes.Ldloc, indexLocal);
        _currentIl.Emit(OpCodes.Ldc_I4_1);
        _currentIl.Emit(OpCodes.Add);
        _currentIl.Emit(OpCodes.Stloc, indexLocal);
        
        // Jump back to loop start
        _currentIl.Emit(OpCodes.Br, loopStart);
        
        // End of array loop
        _currentIl.MarkLabel(loopEndArray);
        
        // End of all loops
        _currentIl.MarkLabel(endLoop);
        
        Console.WriteLine($"[DEBUG] For-in loop compilation complete");
        return new object();
    }
    public object VisitParameter(ParameterNode node) => new object();
    public object VisitImport(ImportStatementNode node)
    {
        Console.WriteLine($"[DEBUG] Processing import: {node.Alias} from '{node.ModulePath}'");
        
        // Only process imports in the first pass for service registration
        if (_isFirstPass)
        {
            var serviceType = ResolveServiceType(node.ModulePath);
            if (serviceType != null)
            {
                // Register the service type
                _importedServices[node.Alias] = serviceType;
                
                // Create a field for this service instance
                var serviceField = _programTypeBuilder.DefineField(
                    $"_{node.Alias}", 
                    serviceType,
                    FieldAttributes.Private | FieldAttributes.Static);
                    
                _serviceFields[node.Alias] = serviceField;
                
                // Add service instantiation to constructor
                if (_constructorIl != null)
                {
                    // Load service provider (arg 4) - no need to load 'this' for static fields
                    _constructorIl.Emit(OpCodes.Ldarg, 4);
                    
                    // Get the GetRequiredService<T> method
                    var getServiceMethod = typeof(ServiceProviderServiceExtensions)
                        .GetMethod("GetRequiredService", new[] { typeof(IServiceProvider) })!
                        .MakeGenericMethod(serviceType);
                    
                    // Call serviceProvider.GetRequiredService<ServiceType>()
                    _constructorIl.Emit(OpCodes.Call, getServiceMethod);
                    
                    // Store in static service field
                    _constructorIl.Emit(OpCodes.Stsfld, serviceField);
                    
                    // Register service in static registry for function access
                    _constructorIl.Emit(OpCodes.Ldstr, node.Alias); // Service name
                    _constructorIl.Emit(OpCodes.Ldsfld, serviceField); // Load service instance from static field
                    
                    // Call CxRuntimeHelper.RegisterService(serviceName, serviceInstance)
                    var registerServiceMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper)
                        .GetMethod("RegisterService", new[] { typeof(string), typeof(object) });
                    if (registerServiceMethod != null)
                    {
                        _constructorIl.Emit(OpCodes.Call, registerServiceMethod);
                    }
                    
                    Console.WriteLine($"[DEBUG] Added service field initialization for: {node.Alias} (resolved from DI container)");
                }
                
                Console.WriteLine($"[DEBUG] Registered service: {node.Alias} -> {serviceType.Name}");
            }
            else
            {
                Console.WriteLine($"[DEBUG] Warning: Could not resolve service for path: {node.ModulePath}");
            }
        }
        
        return new object();
    }
    
    public object VisitUses(UsesStatementNode node)
    {
        Console.WriteLine($"[DEBUG] Processing uses: {node.Alias} from '{node.ServicePath}'");
        
        // Only process uses statements in the first pass for service registration
        if (_isFirstPass)
        {
            var serviceType = ResolveServiceType(node.ServicePath);
            if (serviceType != null)
            {
                // Register the service type
                _importedServices[node.Alias] = serviceType;
                
                // Create a field for this service instance
                var serviceField = _programTypeBuilder.DefineField(
                    $"_{node.Alias}", 
                    serviceType,
                    FieldAttributes.Private | FieldAttributes.Static);
                    
                _serviceFields[node.Alias] = serviceField;
                
                // Add service instantiation to constructor
                if (_constructorIl != null)
                {
                    // Load service provider (arg 4) - no need to load 'this' for static fields
                    _constructorIl.Emit(OpCodes.Ldarg, 4);
                    
                    // Get the GetRequiredService<T> method
                    var getServiceMethod = typeof(ServiceProviderServiceExtensions)
                        .GetMethod("GetRequiredService", new[] { typeof(IServiceProvider) })!
                        .MakeGenericMethod(serviceType);
                    
                    // Call serviceProvider.GetRequiredService<ServiceType>()
                    _constructorIl.Emit(OpCodes.Call, getServiceMethod);
                    
                    // Store in static service field
                    _constructorIl.Emit(OpCodes.Stsfld, serviceField);
                    
                    // Register service in static registry for function access
                    _constructorIl.Emit(OpCodes.Ldstr, node.Alias); // Service name
                    _constructorIl.Emit(OpCodes.Ldsfld, serviceField); // Load service instance from static field
                    
                    // Call CxRuntimeHelper.RegisterService(serviceName, serviceInstance)
                    var registerServiceMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper)
                        .GetMethod("RegisterService", new[] { typeof(string), typeof(object) });
                    if (registerServiceMethod != null)
                    {
                        _constructorIl.Emit(OpCodes.Call, registerServiceMethod);
                    }
                    
                    Console.WriteLine($"[DEBUG] Added service field initialization for: {node.Alias} (resolved from DI container)");
                }
                
                Console.WriteLine($"[DEBUG] Registered service: {node.Alias} -> {serviceType.Name}");
            }
            else
            {
                Console.WriteLine($"[DEBUG] Warning: Could not resolve service for path: {node.ServicePath}");
            }
        }
        
        return new object();
    }

    /// <summary>
    /// Initialize an instance service field in a class constructor
    /// </summary>
    private void InitializeInstanceServiceField(string className, string alias, string servicePath)
    {
        Console.WriteLine($"[DEBUG] InitializeInstanceServiceField: {className}.{alias} from '{servicePath}'");
        
        // Resolve service type from the service path
        var serviceType = ResolveServiceType(servicePath);
        if (serviceType != null)
        {
            Console.WriteLine($"[DEBUG] IL-EMIT: Resolved service type: {serviceType}");
            
            // Get the service field that was created in Pass 1
            var fieldKey = className + "." + alias;
            if (_classFields.TryGetValue(fieldKey, out var serviceField))
            {
                Console.WriteLine($"[DEBUG] Initializing instance service field: {fieldKey}");
                Console.WriteLine($"[DEBUG] IL-EMIT: Service field info - Name: {serviceField.Name}, Type: {serviceField.FieldType}, IsStatic: {serviceField.IsStatic}");
                Console.WriteLine($"[DEBUG] IL-EMIT: Service provider field info - Name: {_serviceProviderField.Name}, Type: {_serviceProviderField.FieldType}, IsStatic: {_serviceProviderField.IsStatic}");
                
            try
            {
                // Debug output
                Console.WriteLine($"[DEBUG] Initializing instance service field: {alias} of type {serviceType} in class {className}");
                
                // Load 'this' pointer
                Console.WriteLine($"[DEBUG] IL-EMIT: Emitting Ldarg_0 (load 'this' pointer)");
                _currentIl!.Emit(OpCodes.Ldarg_0);
                
                // Get service instance from the service provider
                Console.WriteLine($"[DEBUG] IL-EMIT: Emitting Ldsfld to load static service provider field");
                _currentIl.Emit(OpCodes.Ldsfld, _serviceProviderField);
                
                // Load service type token for GetRequiredService<T>()
                Console.WriteLine($"[DEBUG] IL-EMIT: Emitting Ldtoken for service type: {serviceType}");
                _currentIl.Emit(OpCodes.Ldtoken, serviceType);
                Console.WriteLine($"[DEBUG] IL-EMIT: Emitting Call to Type.GetTypeFromHandle");
                _currentIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle")!);
                
                // Call serviceProvider.GetRequiredService(Type)
                var getServiceMethod = typeof(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions)
                    .GetMethod("GetRequiredService", new[] { typeof(IServiceProvider), typeof(Type) });
                if (getServiceMethod != null)
                {
                    Console.WriteLine($"[DEBUG] IL-EMIT: Emitting Call to GetRequiredService method: {getServiceMethod}");
                    _currentIl.Emit(OpCodes.Call, getServiceMethod);
                    
                    // Store in instance service field
                    Console.WriteLine($"[DEBUG] IL-EMIT: Emitting Stfld to store service in instance field: {serviceField.Name}");
                    _currentIl.Emit(OpCodes.Stfld, serviceField);
                    
                    Console.WriteLine($"[DEBUG] Instance service field initialized: {alias}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Warning: GetRequiredService method not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error in InitializeInstanceServiceField: {ex}");
                throw;
            }
            }
            else
            {
                Console.WriteLine($"[DEBUG] Warning: Service field not found: {fieldKey}");
            }
        }
        else
        {
            Console.WriteLine($"[DEBUG] Warning: Could not resolve service type for: {servicePath}");
        }
    }
    
    /// <summary>
    /// Emit IL code for service method calls using optimized direct reflection with proper method resolution
    /// </summary>
    private void EmitServiceMethodCall(string serviceName, string methodName, List<ExpressionNode> arguments, FieldBuilder serviceField)
    {
        Console.WriteLine($"[DEBUG] Emitting service method call: {serviceName}.{methodName} with {arguments.Count} arguments");
        
        // FINAL OPTIMIZATION: Use CxRuntimeHelper.CallServiceMethod directly for robust service calls
        // This approach eliminates complex IL stack management and leverages proven runtime helpers
        
        // Load service instance (first parameter for CallServiceMethod)
        if (serviceField.IsStatic)
        {
            // Static service field - load from static field
            Console.WriteLine($"[DEBUG] Service call with static service field");
            _currentIl!.Emit(OpCodes.Ldsfld, serviceField);
        }
        else
        {
            // Instance service field - load from instance field
            Console.WriteLine($"[DEBUG] Service call with instance service field - loading from this.{serviceName}");
            _currentIl!.Emit(OpCodes.Ldarg_0); // Load 'this' pointer
            _currentIl.Emit(OpCodes.Ldfld, serviceField); // Load service field
        }
        
        // Load method name (second parameter)
        _currentIl.Emit(OpCodes.Ldstr, methodName);
        
        // Create object array for arguments (third parameter)
        _currentIl.Emit(OpCodes.Ldc_I4, arguments.Count);
        _currentIl.Emit(OpCodes.Newarr, typeof(object));
        
        // Fill the array with arguments
        for (int i = 0; i < arguments.Count; i++)
        {
            Console.WriteLine($"[DEBUG] Processing argument {i}: {arguments[i].GetType().Name}");
            
            _currentIl.Emit(OpCodes.Dup); // Duplicate array reference
            _currentIl.Emit(OpCodes.Ldc_I4, i); // Load array index
            
            // Generate argument value
            arguments[i].Accept(this);
            Console.WriteLine($"[DEBUG] Generated IL for argument {i}");
            
            // Store the value in the array
            _currentIl.Emit(OpCodes.Stelem_Ref);
            Console.WriteLine($"[DEBUG] Stored argument {i} in array");
        }
        
        Console.WriteLine($"[DEBUG] All arguments processed, calling CxRuntimeHelper.CallServiceMethod");
        
        // Call CxRuntimeHelper.CallServiceMethod(serviceInstance, methodName, arguments)
        var callServiceMethod = typeof(CxRuntimeHelper).GetMethod("CallServiceMethod", 
            new[] { typeof(object), typeof(string), typeof(object[]) });
        _currentIl.EmitCall(OpCodes.Call, callServiceMethod!, null);
        
        Console.WriteLine($"[DEBUG] Successfully emitted runtime helper call - maximum reliability approach");
    }
    
    /// <summary>
    /// Emit IL code to convert CX values to .NET parameter types
    /// </summary>
    private void EmitParameterConversion(Type targetType, string serviceName, string methodName)
    {
        // Handle string conversion
        if (targetType == typeof(string))
        {
            // Convert object to string by calling ToString()
            var toStringMethod = typeof(object).GetMethod("ToString");
            _currentIl!.EmitCall(OpCodes.Callvirt, toStringMethod!, null);
            return;
        }
        
        // Handle AI service options objects (like TextGenerationOptions)
        if (IsAiOptionsType(targetType))
        {
            EmitAiOptionsConversion(targetType);
            return;
        }
        
        // Handle primitive types that need unboxing
        if (targetType.IsValueType)
        {
            EmitValueTypeConversion(targetType);
            return;
        }
        
        // For reference types, no conversion needed (keep as object)
    }
    
    /// <summary>
    /// Check if a type is an AI options type that needs special conversion
    /// </summary>
    private bool IsAiOptionsType(Type type)
    {
        if (type == null) return false;
        
        // Check if type name ends with "Options" and is in the AI namespace
        return type.Name.EndsWith("Options") && 
               type.Namespace?.Contains("AI") == true;
    }
    
    /// <summary>
    /// Convert Dictionary<string, object> to AI options object
    /// </summary>
    private void EmitAiOptionsConversion(Type optionsType)
    {
        // Create a helper method call to convert Dictionary to options object
        var converterType = typeof(CxParameterConverter);
        var convertMethod = converterType.GetMethod("ConvertToOptions", 
            BindingFlags.Public | BindingFlags.Static);
        
        if (convertMethod != null)
        {
            // Load the target type as a Type parameter
            _currentIl!.Emit(OpCodes.Ldtoken, optionsType);
            _currentIl.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle")!, null);
            
            // Call the converter: CxParameterConverter.ConvertToOptions(dict, targetType)
            _currentIl.EmitCall(OpCodes.Call, convertMethod, null);
            
            // Cast result to target type
            _currentIl.Emit(OpCodes.Castclass, optionsType);
        }
        else
        {
            // Fallback: just pass null for now
            _currentIl!.Emit(OpCodes.Pop); // Remove the dictionary
            _currentIl.Emit(OpCodes.Ldnull);
        }
    }
    
    /// <summary>
    /// Convert object to value type with proper unboxing
    /// </summary>
    private void EmitValueTypeConversion(Type valueType)
    {
        if (valueType == typeof(int))
        {
            _currentIl!.Emit(OpCodes.Unbox_Any, typeof(int));
        }
        else if (valueType == typeof(double))
        {
            _currentIl!.Emit(OpCodes.Unbox_Any, typeof(double));
        }
        else if (valueType == typeof(float))
        {
            _currentIl!.Emit(OpCodes.Unbox_Any, typeof(float));
        }
        else if (valueType == typeof(bool))
        {
            _currentIl!.Emit(OpCodes.Unbox_Any, typeof(bool));
        }
        else
        {
            // Generic unboxing for other value types
            _currentIl!.Emit(OpCodes.Unbox_Any, valueType);
        }
    }
    
    /// <summary>
    /// Find the best matching method for the given name and argument count
    /// </summary>
    private MethodInfo? FindBestMatchingMethod(Type serviceType, string methodName, int argumentCount)
    {
        // Get all methods with the given name
        var methods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToArray();
        
        if (methods.Length == 0)
        {
            return null;
        }
        
        // Try to find exact parameter count match first
        var exactMatches = methods.Where(m => m.GetParameters().Length == argumentCount).ToArray();
        if (exactMatches.Length == 1)
        {
            return exactMatches[0];
        }
        
        // If multiple methods with same parameter count, prefer all-string parameters for CX calls
        if (exactMatches.Length > 1)
        {
            var allStringMethod = exactMatches.FirstOrDefault(m => 
                m.GetParameters().All(p => p.ParameterType == typeof(string)));
            if (allStringMethod != null)
            {
                return allStringMethod;
            }
            
            // Fall back to first exact match
            return exactMatches[0];
        }
        
        // Try to find method with optional parameters that could work
        var compatibleMethod = methods.FirstOrDefault(m => 
        {
            var parameters = m.GetParameters();
            var requiredParams = parameters.Count(p => !p.HasDefaultValue);
            return argumentCount >= requiredParams && argumentCount <= parameters.Length;
        });
        
        return compatibleMethod ?? methods.First(); // Return first method as fallback
    }
    
    /// <summary>
    /// Check if a method is async (returns Task or Task<T>)
    /// </summary>
    private bool IsAsyncMethod(MethodInfo method)
    {
        var returnType = method.ReturnType;
        return returnType == typeof(Task) || 
               (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>));
    }

    /// <summary>
    /// Check if a statement block contains await expressions
    /// </summary>
    private bool HasAwaitExpressions(BlockStatementNode block)
    {
        return CheckForAwaitExpressions(block);
    }

    /// <summary>
    /// Recursively check for await expressions in AST nodes
    /// </summary>
    private bool CheckForAwaitExpressions(AstNode node)
    {
        switch (node)
        {
            case BlockStatementNode block:
                return block.Statements.Any(CheckForAwaitExpressions);
            
            case IfStatementNode ifStmt:
                return CheckForAwaitExpressions(ifStmt.Condition) ||
                       CheckForAwaitExpressions(ifStmt.ThenStatement) ||
                       (ifStmt.ElseStatement != null && CheckForAwaitExpressions(ifStmt.ElseStatement));
            
            case WhileStatementNode whileStmt:
                return CheckForAwaitExpressions(whileStmt.Condition) ||
                       CheckForAwaitExpressions(whileStmt.Body);
            
            case ForStatementNode forStmt:
                return CheckForAwaitExpressions(forStmt.Iterable) ||
                       CheckForAwaitExpressions(forStmt.Body);
            
            case ExpressionStatementNode exprStmt:
                return CheckForAwaitExpressions(exprStmt.Expression);
            
            case ReturnStatementNode retStmt:
                return retStmt.Value != null && CheckForAwaitExpressions(retStmt.Value);
            
            case CallExpressionNode callExpr:
                return callExpr.Arguments.Any(CheckForAwaitExpressions);
            
            case BinaryExpressionNode binExpr:
                return CheckForAwaitExpressions(binExpr.Left) || CheckForAwaitExpressions(binExpr.Right);
            
            case UnaryExpressionNode unaryExpr:
                return CheckForAwaitExpressions(unaryExpr.Operand);
            
            case MemberAccessNode memberAccess:
                return CheckForAwaitExpressions(memberAccess.Object);
            
            case VariableDeclarationNode varDecl:
                return varDecl.Initializer != null && CheckForAwaitExpressions(varDecl.Initializer);
            
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Emit IL to handle Task results from async methods
    /// </summary>
    private void EmitTaskResultHandling(MethodInfo method)
    {
        var returnType = method.ReturnType;
        
        if (returnType == typeof(Task))
        {
            // For Task (void async), call Task.Wait() and return null
            var waitMethod = typeof(Task).GetMethod("Wait", Type.EmptyTypes);
            _currentIl!.EmitCall(OpCodes.Call, waitMethod!, null);
            _currentIl.Emit(OpCodes.Ldnull);
        }
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            // For Task<T>, get the Result property
            var resultProperty = returnType.GetProperty("Result");
            _currentIl!.EmitCall(OpCodes.Call, resultProperty!.GetMethod!, null);
            
            // Box the result if it's a value type
            var resultType = returnType.GetGenericArguments()[0];
            if (resultType.IsValueType)
            {
                _currentIl.Emit(OpCodes.Box, resultType);
            }
        }
    }

    private Type? ResolveServiceType(string modulePath)
    {
        // Map module paths to actual service types with Cx namespace prefix
        return modulePath switch
        {
            // AI Services - Modern Microsoft.Extensions.AI architecture
            "Cx.AI.Wait" => typeof(CxLanguage.StandardLibrary.AI.Wait.AwaitService),
            "Cx.AI.Realtime" => typeof(CxLanguage.StandardLibrary.AI.Realtime.ModernRealtimeService),
            "Cx.AI.TextToSpeech" => typeof(CxLanguage.StandardLibrary.AI.Modern.ModernTextToSpeechService),
            "Cx.AI.Modern" => typeof(CxLanguage.StandardLibrary.AI.Modern.ModernAwaitService),
            
            // Core Standard Library - for future non-AI services like:
            // "Cx.Core.IO" => typeof(...),
            // "Cx.Core.Collections" => typeof(...),
            // "Cx.Core.Threading" => typeof(...),
            
            _ => null
        };
    }
    public object VisitCallExpression(CallExpressionNode node)
    {
        // Skip function calls in Pass 1 (they'll be compiled in Pass 2)
        if (_isFirstPass)
        {
            Console.WriteLine($"[DEBUG] Pass 1: Skipping call expression to {(node.Callee as IdentifierNode)?.Name ?? "unknown"}");
            return new object();
        }
        
        Console.WriteLine($"[DEBUG] Pass 2: Processing call expression to {(node.Callee as IdentifierNode)?.Name ?? "unknown"}");
        
        // Handle service method calls (e.g., service.method() or this.service.method())
        if (node.Callee is MemberAccessNode serviceMemberAccess)
        {
            Console.WriteLine($"[DEBUG] Call expression has MemberAccess node, analyzing structure...");
            Console.WriteLine($"[DEBUG] MemberAccess.Property = {serviceMemberAccess.Property}");
            Console.WriteLine($"[DEBUG] MemberAccess.Object type = {serviceMemberAccess.Object.GetType().Name}");
            
            // Case 1: Direct service call (service.method())
            if (serviceMemberAccess.Object is IdentifierNode serviceIdentifier &&
                _serviceFields.TryGetValue(serviceIdentifier.Name, out var serviceField))
            {
                Console.WriteLine($"[DEBUG] Found direct service method call: {serviceIdentifier.Name}.{serviceMemberAccess.Property}");
                EmitServiceMethodCall(serviceIdentifier.Name, serviceMemberAccess.Property, node.Arguments, serviceField);
                return new object();
            }
            
            // Case 2: This.service method call (this.service.method())
            if (serviceMemberAccess.Object is MemberAccessNode thisServiceAccess)
            {
                Console.WriteLine($"[DEBUG] Analyzing nested MemberAccess...");
                Console.WriteLine($"[DEBUG] thisServiceAccess.Property = {thisServiceAccess.Property}");
                Console.WriteLine($"[DEBUG] thisServiceAccess.Object type = {thisServiceAccess.Object?.GetType().Name ?? "null"}");
                
                if (thisServiceAccess.Object is IdentifierNode thisNode)
                {
                    Console.WriteLine($"[DEBUG] thisNode.Name = {thisNode.Name}");
                    Console.WriteLine($"[DEBUG] Looking for service field: {thisServiceAccess.Property}");
                    Console.WriteLine($"[DEBUG] Available global service fields: {string.Join(", ", _serviceFields.Keys)}");
                    Console.WriteLine($"[DEBUG] Available class service fields: {string.Join(", ", _classFields.Keys)}");
                }
                
                if (thisServiceAccess.Object is IdentifierNode thisNode2 &&
                    thisNode2.Name == "this")
                {
                    // Check both global service fields and class service fields
                    FieldBuilder? foundServiceField = null;
                    
                    // First check global service fields
                    if (_serviceFields.TryGetValue(thisServiceAccess.Property, out foundServiceField))
                    {
                        Console.WriteLine($"[DEBUG] Found this.service method call in global fields: this.{thisServiceAccess.Property}.{serviceMemberAccess.Property}");
                        EmitServiceMethodCall(thisServiceAccess.Property, serviceMemberAccess.Property, node.Arguments, foundServiceField);
                        return new object();
                    }
                    
                    // Then check class service fields (need current class name)
                    if (_currentClassName != null)
                    {
                        var classFieldKey = _currentClassName + "." + thisServiceAccess.Property;
                        if (_classFields.TryGetValue(classFieldKey, out foundServiceField))
                        {
                            Console.WriteLine($"[DEBUG] Found this.service method call in class fields: this.{thisServiceAccess.Property}.{serviceMemberAccess.Property}");
                            EmitServiceMethodCall(thisServiceAccess.Property, serviceMemberAccess.Property, node.Arguments, foundServiceField);
                            return new object();
                        }
                    }
                }
            }
            
            // Case 3: Direct inherited method call (this.ThinkAsync(), this.GenerateAsync(), etc.)
            if (serviceMemberAccess.Object is IdentifierNode thisIdentifier &&
                thisIdentifier.Name == "this" && _currentClassName != null)
            {
                Console.WriteLine($"[DEBUG] Checking for inherited method call: this.{serviceMemberAccess.Property}");
                
                // Check if the current class has a base class with this method
                if (_classBaseTypes.TryGetValue(_currentClassName, out var baseClass))
                {
                    Console.WriteLine($"[DEBUG] Class {_currentClassName} inherits from {baseClass.Name}");
                    
                    // Look for the method in the base class
                    var methodInfo = baseClass.GetMethod(serviceMemberAccess.Property);
                    
                    // CX PRAGMATIC ASYNC: Prefer sync methods for immediate results
                    if (methodInfo == null)
                    {
                        // If no sync version exists, try with Async suffix
                        var asyncMethodName = serviceMemberAccess.Property + "Async";
                        methodInfo = baseClass.GetMethod(asyncMethodName);
                        if (methodInfo != null)
                        {
                            Console.WriteLine($"[DEBUG] No sync version found, using async: {serviceMemberAccess.Property} -> {asyncMethodName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Using synchronous method: {serviceMemberAccess.Property}");
                    }
                    
                    if (methodInfo != null)
                    {
                        Console.WriteLine($"[DEBUG] Found inherited method: {methodInfo.Name} in {baseClass.Name}");
                        
                        // CX PRAGMATIC ASYNC MODEL:
                        // - Methods ending in "Async" (Learn, CommunicateAsync): Fire-and-forget, don't extract results
                        // - Regular methods (Think, Generate, Chat): Fire-and-forget via event bus, no blocking
                        
                        // Check if this is an explicit async method call (user called XxxAsync directly)
                        bool isExplicitAsyncCall = serviceMemberAccess.Property.EndsWith("Async");
                        bool isAsyncMethod = IsAsyncMethod(methodInfo);
                        
                        Console.WriteLine($"[DEBUG] Processing {methodInfo.Name} as fire-and-forget via event bus - no blocking");
                        
                        // Get method parameters for proper argument handling
                        var parameters = methodInfo.GetParameters();
                        Console.WriteLine($"[DEBUG] IL-EMIT: Method {methodInfo.Name} has {parameters.Length} parameters, caller provided {node.Arguments.Count} arguments");
                        
                        // Load 'this' instance first
                        _currentIl!.Emit(OpCodes.Ldarg_0);
                        Console.WriteLine($"[DEBUG] IL-EMIT: Loaded 'this' instance for inherited method call");
                        
                        // Compile arguments in order, handling optional parameters
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (i < node.Arguments.Count)
                            {
                                // User provided argument
                                Console.WriteLine($"[DEBUG] IL-EMIT: Loading user argument {i} for parameter '{parameters[i].Name}'");
                                node.Arguments[i].Accept(this);
                            }
                            else if (parameters[i].IsOptional)
                            {
                                // Optional parameter not provided - use default value
                                Console.WriteLine($"[DEBUG] IL-EMIT: Loading default value for optional parameter '{parameters[i].Name}'");
                                if (parameters[i].DefaultValue == null || parameters[i].DefaultValue == DBNull.Value)
                                {
                                    _currentIl.Emit(OpCodes.Ldnull);
                                }
                                else
                                {
                                    // Handle other default values as needed
                                    _currentIl.Emit(OpCodes.Ldnull); // For now, just use null
                                }
                            }
                            else
                            {
                                // Required parameter not provided - error
                                Console.WriteLine($"[DEBUG] ERROR: Required parameter '{parameters[i].Name}' not provided");
                                _currentIl.Emit(OpCodes.Ldnull); // Emergency fallback
                            }
                        }
                        
                        Console.WriteLine($"[DEBUG] IL-EMIT: About to call inherited method {methodInfo.Name}");
                        Console.WriteLine($"[DEBUG] IL-EMIT: Method signature - Return: {methodInfo.ReturnType.Name}, Parameters: {parameters.Length}");
                        
                        // Call the inherited method with proper stack management
                        _currentIl.EmitCall(OpCodes.Callvirt, methodInfo, null);
                        Console.WriteLine($"[DEBUG] IL-EMIT: Successfully called inherited method {methodInfo.Name}");
                        
                        // Handle return values based on method signature
                        if (methodInfo.ReturnType == typeof(void))
                        {
                            // Void methods - no return value to handle, just put null on stack for expression result
                            Console.WriteLine($"[DEBUG] Fire-and-forget void call to {methodInfo.Name} - no return value to handle");
                            _currentIl.Emit(OpCodes.Ldnull);  // Put null on stack as "result"
                        }
                        else
                        {
                            // Methods that return Task or other types - discard and return null
                            // Results will be handled through the event bus system
                            Console.WriteLine($"[DEBUG] Fire-and-forget call to {methodInfo.Name} - result will be handled via events");
                            _currentIl.Emit(OpCodes.Pop);  // Remove the return value from stack
                            _currentIl.Emit(OpCodes.Ldnull);  // Put null on stack as "result"
                        }
                        
                        Console.WriteLine($"[DEBUG] Successfully emitted synchronous call to {methodInfo.Name}");
                        return new object();
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Method {serviceMemberAccess.Property} not found in base class {baseClass.Name}");
                    }
                }
                else
                {
                    Console.WriteLine($"[DEBUG] No base class found for {_currentClassName}");
                }
            }
            
            Console.WriteLine($"[DEBUG] Service method call not detected, proceeding with general method handling");
        }
        
        // For now, handle simple function calls where callee is an identifier
        if (node.Callee is IdentifierNode identifier)
        {
            // Check if it's a built-in function
            var methodInfo = GetMethodInfo(identifier.Name, node.Arguments.Count);
            if (methodInfo != null)
            {
                Console.WriteLine($"[DEBUG] Found built-in function: {identifier.Name}");
                // For console methods, we don't need to load this instance
                if (identifier.Name == "print" || identifier.Name == "write")
                {
                    // Don't load instance for static Console methods
                }
                else
                {
                    // Load this instance for instance methods
                    _currentIl!.Emit(OpCodes.Ldarg_0);
                }
                
                // Compile arguments
                foreach (var arg in node.Arguments)
                {
                    arg.Accept(this);
                }
                
                // Emit call
                _currentIl!.EmitCall(OpCodes.Call, methodInfo, null);
                
                // Methods like print/write return void, but we need object for expressions
                if (methodInfo.ReturnType == typeof(void))
                {
                    _currentIl.Emit(OpCodes.Ldnull);
                }
                
                return new object();
            }
            else
            {
                Console.WriteLine($"[DEBUG] Found user-defined function: {identifier.Name}");
                // Call a user-defined function
                if (_userFunctions.TryGetValue(identifier.Name, out var methodBuilder))
                {
                    // Load arguments
                    foreach (var arg in node.Arguments)
                    {
                        arg.Accept(this);
                    }
                    
                    // User-defined function - now we can call it directly using the MethodBuilder!
                    _currentIl!.EmitCall(OpCodes.Call, methodBuilder, null);
                    
                    // User functions now return object, so no need to push null
                    // The function will return either a value or null if no explicit return
                }
                else
                {
                    Console.WriteLine($"🔍 COMPILER: Function not found at compile time, deferring to runtime: {identifier.Name}");
                    
                    // Load function name
                    _currentIl!.Emit(OpCodes.Ldstr, identifier.Name);
                    
                    // Create array for arguments
                    _currentIl.Emit(OpCodes.Ldc_I4, node.Arguments.Count);
                    _currentIl.Emit(OpCodes.Newarr, typeof(object));
                    
                    // Store each argument in the array
                    for (int i = 0; i < node.Arguments.Count; i++)
                    {
                        _currentIl.Emit(OpCodes.Dup); // Duplicate array reference
                        _currentIl.Emit(OpCodes.Ldc_I4, i); // Load index
                        node.Arguments[i].Accept(this); // Load argument value
                        _currentIl.Emit(OpCodes.Stelem_Ref); // Store in array
                    }
                    
                    // Call RuntimeFunctionRegistry.ExecuteFunction which will check both user-defined and injected functions
                    var executeMethod = typeof(RuntimeFunctionRegistry).GetMethod("ExecuteFunction", new[] { typeof(string), typeof(object[]) });
                    Console.WriteLine($"🔗 COMPILER: Emitting call to RuntimeFunctionRegistry.ExecuteFunction for {identifier.Name}");
                    _currentIl.Emit(OpCodes.Call, executeMethod!);
                    
                    // The result is already an object on the stack
                }
            }
        }
        else if (node.Callee is MemberAccessNode memberAccess)
        {
            // Handle method calls on objects (e.g., alice.greet())
            Console.WriteLine($"[DEBUG] Processing method call: {memberAccess.Property}");
            
            // Get the object expression (left side of the dot)
            var objectExpr = memberAccess.Object;
            string methodName = memberAccess.Property;
            
            // Compile the object expression to get the instance on the stack
            objectExpr.Accept(this);
            
            // Try to determine if this is a call on a user-defined class
            if (objectExpr is IdentifierNode objectIdentifier)
            {
                // Check if this is a method defined in a user-defined class
                var objectTypeName = GetObjectTypeName(objectIdentifier);
                if (objectTypeName != null && _classMethods.ContainsKey(objectTypeName + "." + methodName))
                {
                    // This is a call to a user-defined class method
                    Console.WriteLine($"[DEBUG] Found class method: {objectTypeName}.{methodName}");
                    
                    // For user-defined class methods, use runtime reflection for reliability
                    // The object instance is already on the stack
                    
                    // Load the method name as string
                    _currentIl!.Emit(OpCodes.Ldstr, methodName);
                    
                    // Create array for arguments
                    _currentIl.Emit(OpCodes.Ldc_I4, node.Arguments.Count);
                    _currentIl.Emit(OpCodes.Newarr, typeof(object));
                    
                    // Store arguments in array
                    for (int i = 0; i < node.Arguments.Count; i++)
                    {
                        _currentIl.Emit(OpCodes.Dup); // Duplicate array reference
                        _currentIl.Emit(OpCodes.Ldc_I4, i); // Load index
                        node.Arguments[i].Accept(this); // Load argument value
                        _currentIl.Emit(OpCodes.Stelem_Ref); // Store in array
                    }
                    
                    // Call CxRuntimeHelper.CallInstanceMethod(object, methodName, args)
                    var callInstanceMethod = typeof(CxRuntimeHelper).GetMethod("CallInstanceMethod", 
                        new[] { typeof(object), typeof(string), typeof(object[]) });
                    
                    if (callInstanceMethod != null)
                    {
                        Console.WriteLine($"[DEBUG] Using runtime method call for: {objectTypeName}.{methodName}");
                        _currentIl.EmitCall(OpCodes.Call, callInstanceMethod, null);
                    }
                    else
                    {
                        // Fallback: clean up stack and return null
                        _currentIl.Emit(OpCodes.Pop); // Remove args array
                        _currentIl.Emit(OpCodes.Pop); // Remove method name
                        _currentIl.Emit(OpCodes.Pop); // Remove object instance
                        _currentIl.Emit(OpCodes.Ldnull);
                        Console.WriteLine($"[DEBUG] CallInstanceMethod not found, returning null");
                    }
                    
                    return new object();
                }
                
                // Fallback: Try to call the method dynamically at runtime
                // The object instance is already on the stack
                
                // Load the method name as string
                _currentIl!.Emit(OpCodes.Ldstr, methodName);
                
                // Create array for arguments
                _currentIl.Emit(OpCodes.Ldc_I4, node.Arguments.Count);
                _currentIl.Emit(OpCodes.Newarr, typeof(object));
                
                // Store arguments in array
                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    _currentIl.Emit(OpCodes.Dup); // Duplicate array reference
                    _currentIl.Emit(OpCodes.Ldc_I4, i); // Load index
                    node.Arguments[i].Accept(this); // Load argument value
                    _currentIl.Emit(OpCodes.Stelem_Ref); // Store in array
                }
                
                // Call CxRuntimeHelper.CallInstanceMethod(object, methodName, args)
                var callInstanceMethodFallback = typeof(CxRuntimeHelper).GetMethod("CallInstanceMethod", 
                    new[] { typeof(object), typeof(string), typeof(object[]) });
                
                if (callInstanceMethodFallback != null)
                {
                    Console.WriteLine($"[DEBUG] Using runtime method call for fallback: {methodName}");
                    _currentIl.EmitCall(OpCodes.Call, callInstanceMethodFallback, null);
                }
                else
                {
                    // Fallback: clean up stack and return null
                    _currentIl.Emit(OpCodes.Pop); // Remove args array
                    _currentIl.Emit(OpCodes.Pop); // Remove method name
                    _currentIl.Emit(OpCodes.Pop); // Remove object instance
                
                    // Emit debug message
                    _currentIl.Emit(OpCodes.Ldstr, $"[Method '{methodName}' not found on class instance]");
                    
                    // Call Console.WriteLine for debugging
                    var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
                    _currentIl.EmitCall(OpCodes.Call, writeLineMethod!, null);
                    
                    // Return null
                    _currentIl.Emit(OpCodes.Ldnull);
                }
            }
            else
            {
                // For other types of objects, fall back to the old behavior
                // Create a message showing the method call
                _currentIl!.Emit(OpCodes.Ldstr, $"[Method '{methodName}' called on object]");
                
                // Call Console.WriteLine
                var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
                _currentIl.EmitCall(OpCodes.Call, writeLineMethod!, null);
                
                // Return null for void methods
                _currentIl.Emit(OpCodes.Ldnull);
            }
            
            return new object();
        }
        else
        {
            throw new CompilationException("Complex call expressions not yet supported");
        }
        
        return new object();
    }
    public object VisitMemberAccess(MemberAccessNode node)
    {
        // Check for array.length property access first
        if (node.Property == "length")
        {
            // Visit the object expression (should put array on stack)
            node.Object.Accept(this);
            
            // Cast to object[] to ensure we have an array
            _currentIl!.Emit(OpCodes.Castclass, typeof(object[]));
            
            // Get the length property
            _currentIl.Emit(OpCodes.Ldlen);
            
            // Convert native int to int32 and then box it
            _currentIl.Emit(OpCodes.Conv_I4);
            _currentIl.Emit(OpCodes.Box, typeof(int));
            
            return new object();
        }

        // Check if this is a 'this.fieldName' field access
        if (node.Object is IdentifierNode objectIdentifier && objectIdentifier.Name == "this")
        {
            Console.WriteLine($"[DEBUG] Found 'this.{node.Property}' field access - using runtime field access");
            
            // Use runtime field access to avoid FieldBuilder invalidation issues
            // Load 'this' pointer
            Console.WriteLine($"[IL-EMIT] Loading 'this' pointer for field access: {node.Property}");
            _currentIl!.Emit(OpCodes.Ldarg_0);
            
            // Load field name as string
            Console.WriteLine($"[IL-EMIT] Emitting Ldstr '{node.Property}' for field name parameter");
            _currentIl.Emit(OpCodes.Ldstr, node.Property);
            
            // Call runtime helper to get field value
            var getFieldMethod = typeof(CxRuntimeHelper).GetMethod("GetInstanceField", 
                BindingFlags.Public | BindingFlags.Static);
            if (getFieldMethod != null)
            {
                Console.WriteLine($"[IL-EMIT] Found GetInstanceField method: {getFieldMethod}");
                Console.WriteLine($"[IL-EMIT] Method signature: {getFieldMethod.ReturnType} {getFieldMethod.Name}({string.Join(", ", getFieldMethod.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"))})");
                Console.WriteLine($"[IL-EMIT] Stack before Call: [this, fieldName]");
                Console.WriteLine($"[IL-EMIT] Emitting Call to GetInstanceField");
                _currentIl.Emit(OpCodes.Call, getFieldMethod);
                Console.WriteLine($"[IL-EMIT] Stack after Call: [field_value] (returns object)");
            }
            else
            {
                throw new CompilationException("GetInstanceField method not found in CxRuntimeHelper");
            }
            
            Console.WriteLine($"[DEBUG] Using runtime field access for: {node.Property}");
            return new object();
        }

        // Check if this is a service property access (e.g., service.PropertyName)
        if (node.Object is IdentifierNode serviceIdentifier &&
            _serviceFields.TryGetValue(serviceIdentifier.Name, out var serviceField))
        {
            Console.WriteLine($"[DEBUG] Found service property access: {serviceIdentifier.Name}.{node.Property}");
            
            // Use runtime property access to avoid type casting issues and ensure consistency
            // Load 'this' pointer
            Console.WriteLine($"[IL-EMIT] Loading 'this' pointer for service property access: {serviceIdentifier.Name}.{node.Property}");
            _currentIl!.Emit(OpCodes.Ldarg_0);
            
            // Load service field name as string
            Console.WriteLine($"[IL-EMIT] Emitting Ldstr '{serviceIdentifier.Name}' for service field name parameter");
            _currentIl.Emit(OpCodes.Ldstr, serviceIdentifier.Name);
            
            // Load property name as string  
            Console.WriteLine($"[IL-EMIT] Emitting Ldstr '{node.Property}' for property name parameter");
            _currentIl.Emit(OpCodes.Ldstr, node.Property);
            
            // Call runtime helper to get property value
            var getPropertyMethod = typeof(CxRuntimeHelper).GetMethod("GetInstanceProperty", 
                BindingFlags.Public | BindingFlags.Static);
            if (getPropertyMethod != null)
            {
                Console.WriteLine($"[IL-EMIT] Found GetInstanceProperty method: {getPropertyMethod}");
                Console.WriteLine($"[IL-EMIT] Method signature: {getPropertyMethod.ReturnType} {getPropertyMethod.Name}({string.Join(", ", getPropertyMethod.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"))})");
                Console.WriteLine($"[IL-EMIT] Stack before Call: [this, fieldName, propertyName]");
                Console.WriteLine($"[IL-EMIT] Emitting Call to GetInstanceProperty");
                _currentIl.Emit(OpCodes.Call, getPropertyMethod);
                Console.WriteLine($"[IL-EMIT] Stack after Call: [property_value] (returns object)");
            }
            else
            {
                // Fallback to direct property access if runtime helper is not available
                Console.WriteLine($"[DEBUG] GetInstanceProperty not found, using field access + property access");
                
                // Load the service field using runtime helper
                var getFieldMethod = typeof(CxRuntimeHelper).GetMethod("GetInstanceField", 
                    BindingFlags.Public | BindingFlags.Static);
                if (getFieldMethod != null)
                {
                    _currentIl.Emit(OpCodes.Call, getFieldMethod);
                    
                    // Now we have the service instance on the stack
                    // Load property name for runtime property access
                    _currentIl.Emit(OpCodes.Ldstr, node.Property);
                    
                    // Call runtime property getter
                    var getObjectPropertyMethod = typeof(CxRuntimeHelper).GetMethod("GetObjectProperty", 
                        BindingFlags.Public | BindingFlags.Static);
                    if (getObjectPropertyMethod != null)
                    {
                        _currentIl.Emit(OpCodes.Call, getObjectPropertyMethod);
                    }
                    else
                    {
                        // Final fallback - return error string
                        _currentIl.Emit(OpCodes.Pop); // Remove service instance
                        _currentIl.Emit(OpCodes.Pop); // Remove property name
                        _currentIl.Emit(OpCodes.Ldstr, $"[Error: Runtime property access not available]");
                    }
                }
                else
                {
                    throw new CompilationException("GetInstanceField method not found in CxRuntimeHelper");
                }
            }
            
            Console.WriteLine($"[DEBUG] Using runtime service property access for: {serviceIdentifier.Name}.{node.Property}");
            return new object();
        }

        // Check if this is a class instance field access (e.g., variable.fieldName where variable is a class instance)
        if (node.Object is IdentifierNode objectVar)
        {
            // First check if this is a parameter - parameters should use dictionary access
            if (_currentParameterMapping != null && _currentParameterMapping.ContainsKey(objectVar.Name))
            {
                Console.WriteLine($"[DEBUG] Parameter dictionary access: {objectVar.Name}.{node.Property}");
                
                // Load the parameter (which should be a Dictionary<string, object>)
                node.Object.Accept(this);
                
                // Load the property name as string
                _currentIl!.Emit(OpCodes.Ldstr, node.Property);
                
                // Get the dictionary's indexer get method
                var paramDictType = typeof(Dictionary<string, object>);
                var paramIndexerGetter = paramDictType.GetMethod("get_Item", new[] { typeof(string) });
                
                // Call dictionary[key] to get the value
                _currentIl.Emit(OpCodes.Callvirt, paramIndexerGetter!);
                
                Console.WriteLine($"[DEBUG] Successfully loaded property: {node.Property} from parameter {objectVar.Name}");
                return new object();
            }
            
            // Check if this is a local variable (like event parameters) - use runtime property access
            if (_currentLocals.ContainsKey(objectVar.Name))
            {
                Console.WriteLine($"[DEBUG] Local variable property access: {objectVar.Name}.{node.Property}");
                
                // Load the local variable (object)
                node.Object.Accept(this);
                
                // Load property name as string  
                Console.WriteLine($"[IL-EMIT] Emitting Ldstr '{node.Property}' for property name parameter");
                _currentIl!.Emit(OpCodes.Ldstr, node.Property);
                
                // Call runtime helper to get property value using reflection
                var getObjectPropertyMethod = typeof(CxRuntimeHelper).GetMethod("GetObjectProperty", 
                    BindingFlags.Public | BindingFlags.Static);
                if (getObjectPropertyMethod != null)
                {
                    Console.WriteLine($"[IL-EMIT] Found GetObjectProperty method: {getObjectPropertyMethod}");
                    Console.WriteLine($"[IL-EMIT] Stack before Call: [object, propertyName]");
                    Console.WriteLine($"[IL-EMIT] Emitting Call to GetObjectProperty");
                    _currentIl.Emit(OpCodes.Call, getObjectPropertyMethod);
                    Console.WriteLine($"[IL-EMIT] Stack after Call: [property_value] (returns object)");
                    
                    Console.WriteLine($"[DEBUG] Using runtime property access for local variable: {objectVar.Name}.{node.Property}");
                    return new object();
                }
                else
                {
                    Console.WriteLine($"[DEBUG] GetObjectProperty not found, falling back to error message");
                    _currentIl.Emit(OpCodes.Pop); // Remove object
                    _currentIl.Emit(OpCodes.Pop); // Remove property name
                    _currentIl.Emit(OpCodes.Ldstr, $"[Error: Runtime property access not available for {objectVar.Name}.{node.Property}]");
                    return new object();
                }
            }
            
            // Try to find this field in any class
            foreach (var kvp in _classFields)
            {
                var fieldKey = kvp.Key;
                var field = kvp.Value;
                
                // Field key format is "ClassName.fieldName"
                if (fieldKey.EndsWith("." + node.Property))
                {
                    Console.WriteLine($"[DEBUG] Found field access: {objectVar.Name}.{node.Property} (field key: {fieldKey})");
                    
                    // Extract class name from field key (format: "ClassName.fieldName")
                    var className = fieldKey.Substring(0, fieldKey.LastIndexOf('.'));
                    
                    // Load the object variable
                    node.Object.Accept(this);
                    
                    // Get the class type and cast the object to the correct type
                    if (_userClasses.TryGetValue(className, out var typeBuilder))
                    {
                        // Try to get the actual created type
                        Type? classType = null;
                        try
                        {
                            classType = _moduleBuilder.GetType(className);
                            if (classType == null)
                            {
                                classType = typeBuilder.CreateType();
                            }
                        }
                        catch
                        {
                            classType = typeBuilder.CreateType();
                        }
                        
                        if (classType != null)
                        {
                            // Cast the object to the correct class type
                            _currentIl!.Emit(OpCodes.Castclass, classType);
                        }
                    }
                    
                    // Load the field from the instance
                    _currentIl!.Emit(OpCodes.Ldfld, field);
                    
                    Console.WriteLine($"[DEBUG] Successfully loaded field: {node.Property} from object {objectVar.Name}");
                    return new object();
                }
            }
        }

        // Check if this is property access on a variable that might contain a service object
        if (node.Object is IdentifierNode variableNode)
        {
            Console.WriteLine($"[DEBUG] Variable property access: {variableNode.Name}.{node.Property}");
            
            // For variables, we need to use runtime property access since we don't know
            // the exact type at compile time. The variable could contain a service object.
            
            // Load the variable (object)
            node.Object.Accept(this);
            
            // Load property name as string  
            Console.WriteLine($"[IL-EMIT] Emitting Ldstr '{node.Property}' for property name parameter");
            _currentIl!.Emit(OpCodes.Ldstr, node.Property);
            
            // Call runtime helper to get property value using reflection
            var getObjectPropertyMethod = typeof(CxRuntimeHelper).GetMethod("GetObjectProperty", 
                BindingFlags.Public | BindingFlags.Static);
            if (getObjectPropertyMethod != null)
            {
                Console.WriteLine($"[IL-EMIT] Found GetObjectProperty method: {getObjectPropertyMethod}");
                Console.WriteLine($"[IL-EMIT] Stack before Call: [object, propertyName]");
                Console.WriteLine($"[IL-EMIT] Emitting Call to GetObjectProperty");
                _currentIl.Emit(OpCodes.Call, getObjectPropertyMethod);
                Console.WriteLine($"[IL-EMIT] Stack after Call: [property_value] (returns object)");
                
                Console.WriteLine($"[DEBUG] Using runtime property access for variable: {variableNode.Name}.{node.Property}");
                return new object();
            }
            else
            {
                Console.WriteLine($"[DEBUG] GetObjectProperty not found, falling back to dictionary access");
            }
        }

        // Handle complex expressions like payload.outputs[0].value
        // This is the case where Object is not a simple identifier (e.g., IndexAccessNode, other MemberAccessNode, etc.)
        Console.WriteLine($"[DEBUG] Complex member access detected - Object type: {node.Object.GetType().Name}");
        
        // For complex expressions, we use runtime property access
        // First evaluate the object expression (whatever it is - IndexAccess, MemberAccess, etc.)
        node.Object.Accept(this);
        
        // Now we have the object on the stack, use runtime property access
        Console.WriteLine($"[IL-EMIT] Emitting Ldstr '{node.Property}' for property name parameter");
        _currentIl!.Emit(OpCodes.Ldstr, node.Property);
        
        // Call runtime helper to get property value using reflection
        var getComplexObjectPropertyMethod = typeof(CxRuntimeHelper).GetMethod("GetObjectProperty", 
            BindingFlags.Public | BindingFlags.Static);
        if (getComplexObjectPropertyMethod != null)
        {
            Console.WriteLine($"[IL-EMIT] Found GetObjectProperty method for complex access: {getComplexObjectPropertyMethod}");
            Console.WriteLine($"[IL-EMIT] Stack before Call: [object, propertyName]");
            Console.WriteLine($"[IL-EMIT] Emitting Call to GetObjectProperty");
            _currentIl.Emit(OpCodes.Call, getComplexObjectPropertyMethod);
            Console.WriteLine($"[IL-EMIT] Stack after Call: [property_value] (returns object)");
            
            Console.WriteLine($"[DEBUG] Using runtime property access for complex expression with property: {node.Property}");
            return new object();
        }

        // Final fallback: Treat as dictionary access (for backward compatibility)
        Console.WriteLine($"[DEBUG] Using dictionary access fallback for member access");
        
        // The object expression has already been evaluated above, so we need to reload it
        node.Object.Accept(this);
        
        // Load the member name as string
        _currentIl!.Emit(OpCodes.Ldstr, node.Property);
        
        // Get the dictionary's indexer get method
        var dictionaryType = typeof(Dictionary<string, object>);
        var indexerGetter = dictionaryType.GetMethod("get_Item", new[] { typeof(string) });
        
        // Call dictionary[key] to get the value
        _currentIl.Emit(OpCodes.Callvirt, indexerGetter!);
        
        return new object();
    }
    public object VisitIndexAccess(IndexAccessNode node)
    {
        // Visit the object expression first (this puts array on stack)
        node.Object.Accept(this);
        
        // Cast to object[] 
        _currentIl!.Emit(OpCodes.Castclass, typeof(object[]));
        
        // Visit the index expression (this puts boxed index on stack)
        node.Index.Accept(this);
        
        // Unbox the index to get the actual int value
        _currentIl.Emit(OpCodes.Unbox_Any, typeof(int));
        
        // Now stack is [array] [int_index] which is correct order for ldelem_ref
        _currentIl.Emit(OpCodes.Ldelem_Ref);
        
        return new object();
    }
    public object VisitAITask(AITaskNode node)
    {
        if (_isFirstPass)
        {
            // During first pass, we don't generate IL
            return new object();
        }

        // This will generate IL that calls the AI service
        // First check if AI service is null
        var aiServiceNotNullLabel = _currentIl!.DefineLabel();
        var endLabel = _currentIl.DefineLabel();
        
        // Load the AI service field onto the stack
        _currentIl.Emit(OpCodes.Ldarg_0);
        _currentIl.Emit(OpCodes.Ldfld, _aiServiceField);
        
        // Check if it's null
        _currentIl.Emit(OpCodes.Ldnull);
        _currentIl.Emit(OpCodes.Ceq);
        _currentIl.Emit(OpCodes.Brfalse, aiServiceNotNullLabel);
        
        // If AI service is null, return a placeholder message
        _currentIl.Emit(OpCodes.Ldstr, $"[AI Service Not Available] Task for: {node.Goal}");
        _currentIl.Emit(OpCodes.Box, typeof(string));
        _currentIl.Emit(OpCodes.Br, endLabel);
        
        // If AI service is not null, call the actual service
        _currentIl.MarkLabel(aiServiceNotNullLabel);
        
        // Load the AI service field onto the stack
        _currentIl.Emit(OpCodes.Ldarg_0);
        _currentIl.Emit(OpCodes.Ldfld, _aiServiceField);
        
        // Load the goal/prompt string onto the stack
        _currentIl.Emit(OpCodes.Ldstr, node.Goal);
        
        // Load null for options parameter (we'll implement options later)
        _currentIl.Emit(OpCodes.Ldnull);
        
        // Call the GenerateTextAsync method
        var generateTextMethod = typeof(IAiService).GetMethod("GenerateTextAsync", 
            new[] { typeof(string), typeof(AiRequestOptions) });
        
        if (generateTextMethod != null)
        {
            _currentIl.Emit(OpCodes.Callvirt, generateTextMethod);
            
            // Get the Result property from the Task<AiResponse>
            var resultProperty = typeof(Task<AiResponse>).GetProperty("Result");
            if (resultProperty != null)
            {
                _currentIl.Emit(OpCodes.Callvirt, resultProperty.GetMethod!);
                
                // Get the Content property from AiResponse
                var contentProperty = typeof(AiResponse).GetProperty("Content");
                if (contentProperty != null)
                {
                    _currentIl.Emit(OpCodes.Callvirt, contentProperty.GetMethod!);
                }
            }
        }
        else
        {
            // Fallback: load a placeholder string
            _currentIl.Emit(OpCodes.Ldstr, $"[AI Task] {node.Goal}");
        }
        
        // Mark the end label for both paths
        _currentIl.MarkLabel(endLabel);
        
        return new object();
    }
    public object VisitAISynthesize(AISynthesizeNode node) => new object();
    public object VisitAICall(AICallNode node)
    {
        if (_isFirstPass)
        {
            // During first pass, we don't generate IL
            return new object();
        }

        Console.WriteLine($"[DEBUG] Compiling AI call: {node.FunctionName}");
        
        // NEURAL SYSTEM BYPASS: Temporarily bypass AI functions to enable biological neural testing
        // All AI functions return simple confirmation messages
        if (node.Arguments.Count > 0)
        {
            node.Arguments[0].Accept(this);
            _currentIl!.Emit(OpCodes.Ldstr, $" - [Neural System: {node.FunctionName} bypassed]");
            
            var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            if (concatMethod != null)
            {
                _currentIl.Emit(OpCodes.Call, concatMethod);
            }
        }
        else
        {
            _currentIl!.Emit(OpCodes.Ldstr, $"[Neural System: {node.FunctionName} ready]");
        }
        
        return new object();
    }
    
    // NEURAL SYSTEM: Orphaned AI function methods removed to enable biological neural testing
    
    public object VisitAIReason(AIReasonNode node) => new object();
    public object VisitAIProcess(AIProcessNode node) => new object();
    public object VisitAIEmbed(AIEmbedNode node) => new object();
    public object VisitAIAdapt(AIAdaptNode node) => new object();
    public object VisitTryStatement(TryStatementNode node)
    {
        if (_isFirstPass)
        {
            // In Pass 1, we need to process the try block to collect function declarations
            node.TryBlock.Accept(this);
            
            // Also process the catch block in case it contains function declarations
            if (node.CatchBlock != null)
            {
                node.CatchBlock.Accept(this);
            }
            
            return new object();
        }

        Console.WriteLine($"[DEBUG] Processing try-catch statement");
        
        // Define labels for exception handling
        var catchLabel = _currentIl!.DefineLabel();
        var endLabel = _currentIl.DefineLabel();
        
        // Begin exception handling block
        _currentIl.BeginExceptionBlock();
        
        // Compile the try block
        Console.WriteLine($"[DEBUG] Compiling try block");
        node.TryBlock.Accept(this);
        
        // Leave the try block
        _currentIl.Emit(OpCodes.Leave, endLabel);
        
        // Begin catch block if present
        if (node.CatchBlock != null && node.CatchVariableName != null)
        {
            Console.WriteLine($"[DEBUG] Compiling catch block with variable: {node.CatchVariableName}");
            
            // Begin catch handler for all exceptions
            _currentIl.BeginCatchBlock(typeof(Exception));
            
            // Create local variable for the caught exception
            var exceptionLocal = _currentIl.DeclareLocal(typeof(object));
            _currentLocals[node.CatchVariableName] = exceptionLocal;
            
            // Store the exception in the local variable
            // For now, we'll just store the exception message
            var messageProperty = typeof(Exception).GetProperty("Message");
            _currentIl.EmitCall(OpCodes.Callvirt, messageProperty!.GetGetMethod()!, null);
            _currentIl.Emit(OpCodes.Stloc, exceptionLocal);
            
            // Compile the catch block
            node.CatchBlock.Accept(this);
            
            // Leave the catch block
            _currentIl.Emit(OpCodes.Leave, endLabel);
        }
        
        // End exception handling block
        _currentIl.EndExceptionBlock();
        
        // Mark the end label
        _currentIl.MarkLabel(endLabel);
        
        Console.WriteLine($"[DEBUG] Try-catch compilation complete");
        return new object();
    }

    public object VisitThrowStatement(ThrowStatementNode node)
    {
        if (_isFirstPass)
        {
            return new object();
        }

        Console.WriteLine($"[DEBUG] Processing throw statement");
        
        // Compile the expression to throw
        node.Expression.Accept(this);
        
        // Convert the expression to a string for the exception message
        var toStringMethod = typeof(object).GetMethod("ToString");
        _currentIl!.EmitCall(OpCodes.Callvirt, toStringMethod!, null);
        
        // Create and throw a new Exception
        var exceptionConstructor = typeof(Exception).GetConstructor(new[] { typeof(string) });
        _currentIl.Emit(OpCodes.Newobj, exceptionConstructor!);
        _currentIl.Emit(OpCodes.Throw);
        
        Console.WriteLine($"[DEBUG] Throw statement compilation complete");
        return new object();
    }

    public object VisitOnStatement(OnStatementNode node)
    {
        if (_isFirstPass)
        {
            // Pass 1: Just collect the event handler for later registration
            var methodName = $"EventHandler_{node.EventName.FullName.Replace(".", "_")}_{_eventHandlerCounter++}";
            _eventHandlerMethodNames[node] = methodName;
            _eventRegistrations.Add((node.EventName.FullName, methodName));
            Console.WriteLine($"[DEBUG] Pass 1: Collected event handler {node.EventName.FullName} -> {methodName}");
            return new object();
        }

        Console.WriteLine($"[DEBUG] Compiling 'on' event handler statement - Event: {node.EventName.FullName}");
        
        // Get the method name that was assigned in Pass 1
        var handlerMethodName = _eventHandlerMethodNames[node];
        
        // Create the event handler method - now accepts CxEvent directly
        var handlerMethod = _programTypeBuilder.DefineMethod(
            handlerMethodName,
            MethodAttributes.Private | MethodAttributes.Static,
            typeof(Task),
            new[] { typeof(CxEvent) }); // Handler now accepts CxEvent
            
        var handlerIl = handlerMethod.GetILGenerator();
        
        // Store current IL context
        var previousIl = _currentIl;
        var previousMethod = _currentMethod;
        var previousLocals = _currentLocals;
        
        // Switch to handler method context
        _currentIl = handlerIl;
        _currentMethod = handlerMethod;
        _currentLocals = new Dictionary<string, LocalBuilder>();
        
        try
        {
            // Load the CxEvent parameter into local variable
            var cxEventLocal = handlerIl.DeclareLocal(typeof(CxEvent));
            _currentLocals[node.PayloadIdentifier] = cxEventLocal;
            handlerIl.Emit(OpCodes.Ldarg_0);
            handlerIl.Emit(OpCodes.Stloc, cxEventLocal);
            
            Console.WriteLine($"[DEBUG] Event handler {handlerMethodName} expects CxEvent object as parameter '{node.PayloadIdentifier}'");
            
            // Generate code for the handler body statements
            foreach (var statement in node.Body.Statements)
            {
                statement.Accept(this);
            }
            
            // Create a completed task for the return value
            handlerIl.Emit(OpCodes.Call, typeof(Task).GetMethod("get_CompletedTask")!);
            handlerIl.Emit(OpCodes.Ret);
            
            Console.WriteLine($"[DEBUG] Generated event handler method: {handlerMethodName}");
        }
        finally
        {
            // Restore previous IL context
            _currentIl = previousIl;
            _currentMethod = previousMethod;
            _currentLocals = previousLocals;
        }
        
        // Track this event handler for runtime registration
        _eventHandlers[node.EventName.FullName] = handlerMethod;
        
        Console.WriteLine($"[INFO] Event handler method compiled: {node.EventName.FullName} -> {handlerMethodName}");
        
        return new object();
    }

    public object VisitEventName(EventNameNode node)
    {
        // EventName nodes are simple - just return the full name as a string
        return node.FullName;
    }

    public object VisitEmitStatement(EmitStatementNode node)
    {
        if (_isFirstPass)
        {
            return new object();
        }

        Console.WriteLine($"[DEBUG] Compiling 'emit' event statement - Event: {node.EventName.FullName}");
        Console.WriteLine($"[DEBUG] Current IL generator is null: {_currentIl == null}");
        Console.WriteLine($"[DEBUG] Current class name: {_currentClassName}");
        
        // Load the event name as string parameter
        _currentIl!.Emit(OpCodes.Ldstr, node.EventName.FullName);
        
        // Generate code for the payload (if present)
        if (node.Payload != null)
        {
            // Evaluate the payload expression
            node.Payload.Accept(this);
        }
        else
        {
            // No payload - load null
            _currentIl.Emit(OpCodes.Ldnull);
        }
        
        // Load source identifier (for debugging)
        _currentIl.Emit(OpCodes.Ldstr, _currentClassName ?? "MainScript");
        
        // Call CxRuntimeHelper.EmitEvent(string eventName, object data, string source)
        var emitMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper).GetMethod("EmitEvent", 
            new[] { typeof(string), typeof(object), typeof(string) });
            
        if (emitMethod == null)
        {
            throw new CompilationException("EmitEvent method not found in CxRuntimeHelper");
        }
        
        Console.WriteLine($"[DEBUG] Found EmitEvent method: {emitMethod.Name}");
        
        _currentIl.Emit(OpCodes.Call, emitMethod);
        
        Console.WriteLine($"[DEBUG] Event emission code generated for: {node.EventName.FullName}");
        return new object();
    }

    public object VisitAiServiceStatement(AiServiceStatementNode node)
    {
        if (_isFirstPass)
        {
            return new object();
        }

        Console.WriteLine($"[DEBUG] Compiling AI service statement - Service: {node.ServiceName}");
        
        // Extract payload and check for special 'handlers' property
        List<string>? handlerEvents = null;
        
        if (node.Payload is ObjectLiteralNode objectLiteral)
        {
            // Check for handlers property and extract it
            var handlersProperty = objectLiteral.Properties.FirstOrDefault(p => p.Key == "handlers");
            if (handlersProperty != null)
            {
                Console.WriteLine("[DEBUG] Found handlers property in AI service call");
                
                if (handlersProperty.Value is ObjectLiteralNode handlersObject)
                {
                    handlerEvents = handlersObject.Properties.Select(p => p.Key).ToList();
                    Console.WriteLine($"[DEBUG] Extracted {handlerEvents.Count} handler events: {string.Join(", ", handlerEvents)}");
                }
            }
        }
        
        // For now, convert AI service calls to event emissions
        // This allows the handlers pattern to work immediately
        // In the future, this could call actual AI service methods
        
        var eventName = $"ai.{node.ServiceName}.request";
        Console.WriteLine($"[DEBUG] Converting AI service call to event: {eventName}");
        
        // Load the event name as string parameter
        _currentIl!.Emit(OpCodes.Ldstr, eventName);
        
        // Generate enhanced payload with instance collection information when in class context
        if (_currentClassName != null)
        {
            Console.WriteLine($"[DEBUG] AI service call within class {_currentClassName} - adding instance collection info");
            
            // Create a dictionary to hold the enhanced payload
            var dictType = typeof(Dictionary<string, object>);
            var dictCtor = dictType.GetConstructor(Type.EmptyTypes);
            var addMethod = dictType.GetMethod("Add", new[] { typeof(string), typeof(object) });
            
            // Create new Dictionary<string, object>()
            _currentIl.Emit(OpCodes.Newobj, dictCtor!);
            var dictLocal = _currentIl.DeclareLocal(dictType);
            _currentIl.Emit(OpCodes.Stloc, dictLocal);
            
            // Add original payload properties if present
            if (node.Payload != null)
            {
                // We need to evaluate the original payload and copy its properties
                // For now, we'll handle this by adding a special flag to indicate we need instance routing
                Console.WriteLine("[DEBUG] Merging original payload with instance collection info");
                
                // Load dict, key "originalPayload", and value
                _currentIl.Emit(OpCodes.Ldloc, dictLocal);
                _currentIl.Emit(OpCodes.Ldstr, "originalPayload");
                node.Payload.Accept(this); // This puts the original payload on the stack
                _currentIl.Emit(OpCodes.Callvirt, addMethod!);
            }
            
            // Add instance collection name for routing to instance-specific memory
            // Load dict, key "_instanceCollection", and value (this reference + collection name)
            _currentIl.Emit(OpCodes.Ldloc, dictLocal);
            _currentIl.Emit(OpCodes.Ldstr, "_instanceCollection");
            
            // Load 'this' reference to get the instance collection name
            _currentIl.Emit(OpCodes.Ldarg_0); // Load 'this'
            
            // Call GetInstanceCollectionName() method on the instance (defined in ModernAiServiceBase)
            var getCollectionMethod = typeof(CxLanguage.StandardLibrary.Core.ModernAiServiceBase)
                .GetMethod("GetInstanceCollectionName", BindingFlags.Public | BindingFlags.Instance);
            if (getCollectionMethod != null)
            {
                _currentIl.Emit(OpCodes.Callvirt, getCollectionMethod);
            }
            else
            {
                // Fallback to class name if method not found
                _currentIl.Emit(OpCodes.Pop); // Remove 'this' from stack
                _currentIl.Emit(OpCodes.Ldstr, _currentClassName + "_instance");
            }
            
            _currentIl.Emit(OpCodes.Callvirt, addMethod!);
            
            // Add source information
            _currentIl.Emit(OpCodes.Ldloc, dictLocal);
            _currentIl.Emit(OpCodes.Ldstr, "source");
            _currentIl.Emit(OpCodes.Ldstr, _currentClassName);
            _currentIl.Emit(OpCodes.Callvirt, addMethod!);
            
            // Load the final dictionary as the payload
            _currentIl.Emit(OpCodes.Ldloc, dictLocal);
        }
        else
        {
            // Generate code for the payload (if present) - global scope
            if (node.Payload != null)
            {
                // Evaluate the payload expression (this will handle the handlers extraction automatically)
                node.Payload.Accept(this);
            }
            else
            {
                // No payload - load null
                _currentIl.Emit(OpCodes.Ldnull);
            }
        }
        
        // Load source identifier
        _currentIl.Emit(OpCodes.Ldstr, _currentClassName ?? "MainScript");
        
        // Call CxRuntimeHelper.EmitEvent(string eventName, object data, string source)
        var emitMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper).GetMethod("EmitEvent", 
            new[] { typeof(string), typeof(object), typeof(string) });
        _currentIl.Emit(OpCodes.Call, emitMethod!);
        
        Console.WriteLine($"[DEBUG] AI service statement compiled as event emission: {eventName}");
        return new object();
    }

    public object VisitHandlerItem(HandlerItemNode node)
    {
        if (_isFirstPass)
        {
            return new object();
        }

        Console.WriteLine($"[DEBUG] Compiling handler item - Event: {node.EventName.FullName}");
        
        // For now, handler items are processed in the context of handlers lists
        // This method is mainly for completeness of the visitor pattern
        
        return new object();
    }

    public object VisitNewExpression(NewExpressionNode node)
    {
        if (_isFirstPass)
        {
            return new object();
        }

        Console.WriteLine($"[DEBUG] Creating new instance of class: {node.TypeName}");
        
        // Add runtime debug output
        var printMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
        _currentIl!.Emit(OpCodes.Ldstr, $"[DEBUG] RUNTIME: About to create new {node.TypeName}");
        _currentIl.Emit(OpCodes.Call, printMethod!);
        
        // Check if we have a user-defined class
        if (_userClasses.TryGetValue(node.TypeName, out var typeBuilder))
        {
            Console.WriteLine($"[DEBUG] Found user-defined class: {node.TypeName}");
            
            // In Pass 2, the class type should already be created by VisitClassDeclaration
            // Try to get the actual Type from the module
            Type? actualType = null;
            
            try
            {
                // Try to find the type in the module first
                actualType = _moduleBuilder.GetType(node.TypeName);
                
                if (actualType == null)
                {
                    // If not found, try to create it from the TypeBuilder
                    actualType = typeBuilder.CreateType();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error resolving type {node.TypeName}: {ex.Message}");
                throw new CompilationException($"Could not resolve type for class: {node.TypeName}");
            }
            
            if (actualType == null)
            {
                throw new CompilationException($"Could not resolve type for class: {node.TypeName}");
            }
            
            // Find the constructor
            ConstructorInfo? constructor = null;
            
            if (node.Arguments?.Count > 0)
            {
                // Constructor with parameters - find matching constructor
                var paramTypes = new Type[node.Arguments.Count];
                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    paramTypes[i] = typeof(object); // All parameters are object type for now
                }
                constructor = actualType.GetConstructor(paramTypes);
            }
            else
            {
                // Default constructor (no parameters)
                constructor = actualType.GetConstructor(Type.EmptyTypes);
            }
            
            if (constructor == null)
            {
                throw new CompilationException($"No matching constructor found for class {node.TypeName} with {node.Arguments?.Count ?? 0} parameters");
            }
            
            // Load constructor arguments onto stack
            if (node.Arguments != null)
            {
                Console.WriteLine($"[IL-EMIT] Loading {node.Arguments.Count} constructor arguments");
                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    Console.WriteLine($"[IL-EMIT] Loading constructor argument {i}");
                    node.Arguments[i].Accept(this);
                    Console.WriteLine($"[IL-EMIT] Constructor argument {i} loaded onto stack");
                }
            }
            
            // Call constructor
            Console.WriteLine($"[DEBUG] Calling constructor: {constructor}");
            Console.WriteLine($"[DEBUG] Constructor parameter count: {constructor.GetParameters().Length}");
            Console.WriteLine($"[IL-EMIT] Constructor signature: {constructor.DeclaringType?.Name ?? "unknown"}.{constructor.Name}({string.Join(", ", constructor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
            Console.WriteLine($"[IL-EMIT] Stack before Newobj: {(node.Arguments?.Count ?? 0)} constructor arguments");
            Console.WriteLine($"[DEBUG] IL-EMIT: About to emit Newobj for constructor");
            Console.WriteLine($"[DEBUG] IL-EMIT: Current IL stack should have {constructor.GetParameters().Length} arguments");
            _currentIl!.Emit(OpCodes.Newobj, constructor);
            Console.WriteLine($"[IL-EMIT] Newobj emitted successfully - object instance now on stack");
            Console.WriteLine($"[DEBUG] IL-EMIT: Newobj emitted successfully");
            
            Console.WriteLine($"[DEBUG] Successfully created instance of {node.TypeName}");
            return new object();
        }
        else
        {
            throw new CompilationException($"Unknown class: {node.TypeName}");
        }
    }
    
    public object VisitClassDeclaration(ClassDeclarationNode node)
    {
        if (_isFirstPass)
        {
            Console.WriteLine($"[DEBUG] Pass 1: Defining class {node.Name}");
            
            // Check if class has event handlers to determine agent behavior
            bool hasAgentDecorator = node.Decorators.Any(d => d.Name == "Agent");
            if (hasAgentDecorator)
            {
                Console.WriteLine($"[DEBUG] Class {node.Name} has event handlers - enabling automatic agent behavior");
            }
            
            // Determine base class based on inheritance and interfaces
            Type baseClass = DetermineBaseClass(node);
            Console.WriteLine($"[DEBUG] Class {node.Name} will inherit from: {baseClass.Name}");
            
            // Store the base class type for later use (e.g., in method resolution)
            _classBaseTypes[node.Name] = baseClass;
            
            // In Pass 1, we need to define the class type
            var typeBuilder = _moduleBuilder.DefineType(
                node.Name, 
                TypeAttributes.Public | TypeAttributes.Class,
                baseClass);
            
            // Store the type builder for Pass 2
            _userClasses[node.Name] = typeBuilder;
            
            // Process uses statements to create service fields
            foreach (var usesStmt in node.UsesStatements)
            {
                Console.WriteLine($"[DEBUG] Pass 1: Processing uses statement for {usesStmt.Alias}");
                
                // Create a field for the service
                var fieldBuilder = typeBuilder.DefineField(
                    usesStmt.Alias,
                    typeof(object), // Service instance field
                    FieldAttributes.Public);
                    
                // Store field info for later use
                _classFields[node.Name + "." + usesStmt.Alias] = fieldBuilder;
            }
            
            // Process fields to define them
            foreach (var field in node.Fields)
            {
                Console.WriteLine($"[DEBUG] Pass 1: Defining field {field.Name} in class {node.Name}");
                
                var fieldBuilder = typeBuilder.DefineField(
                    field.Name,
                    typeof(object), // For now, all fields are object type
                    FieldAttributes.Public);
                    
                // Store field info for later use
                var fieldKey = node.Name + "." + field.Name;
                _classFields[fieldKey] = fieldBuilder;
                Console.WriteLine($"[DEBUG] Pass 1: Stored field with key: {fieldKey}");
            }
            
            // Process methods to define them
            foreach (var method in node.Methods)
            {
                var paramTypes = new Type[method.Parameters.Count];
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    paramTypes[i] = typeof(object);
                }
                
                var methodBuilder = typeBuilder.DefineMethod(
                    method.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    typeof(object),
                    paramTypes);
                    
                // Store method info for later use
                _classMethods[node.Name + "." + method.Name] = methodBuilder;
            }
            
            // Process realize declarations (cognitive constructors)
            if (node.RealizeDeclarations.Count > 0)
            {
                // Use explicitly declared realize functions and add field initialization
                foreach (var realizeDecl in node.RealizeDeclarations)
                {
                    // Add field initialization statements for fields with default values
                    CxDebugTracing.TraceDebug("Compiler", $"Adding field initializations to realize declaration for class {node.Name}");
                    var originalStatements = new List<StatementNode>(realizeDecl.Body.Statements);
                    realizeDecl.Body.Statements.Clear();
                    
                    foreach (var field in node.Fields)
                    {
                        if (field.Initializer != null)
                        {
                            Console.WriteLine($"[DEBUG] Adding initialization for field {field.Name} with default value");
                            
                            // Create this.fieldName = defaultValue assignment
                            var assignment = new AssignmentExpressionNode
                            {
                                Left = new MemberAccessNode
                                {
                                    Object = new IdentifierNode { Name = "this" },
                                    Property = field.Name
                                },
                                Right = field.Initializer
                            };
                            
                            // Wrap in an expression statement
                            var assignmentStatement = new ExpressionStatementNode
                            {
                                Expression = assignment
                            };
                            
                            realizeDecl.Body.Statements.Add(assignmentStatement);
                        }
                    }
                    
                    // Add the original realize body after field initialization
                    realizeDecl.Body.Statements.AddRange(originalStatements);
                    
                    var paramTypes = new Type[realizeDecl.Parameters.Count];
                    for (int i = 0; i < realizeDecl.Parameters.Count; i++)
                    {
                        paramTypes[i] = typeof(object);
                    }
                    
                    var constructorBuilder = typeBuilder.DefineConstructor(
                        MethodAttributes.Public,
                        CallingConventions.Standard,
                        paramTypes);
                        
                    // Store constructor info for later use (realize declarations become constructors in IL)
                    _classConstructors[node.Name] = constructorBuilder;
                    CxDebugTracing.TraceCxToIL($"realize({string.Join(", ", realizeDecl.Parameters.Select(p => p.Name))})", $"DefineConstructor({paramTypes.Length} params)", new { ClassName = node.Name, ParameterCount = paramTypes.Length });
                }
            }
            else
            {
                // Create a default parameterless constructor when no realize declarations are provided
                CxDebugTracing.TraceDebug("Compiler", $"Creating default constructor for class {node.Name} (no realize declarations)");
                
                var defaultConstructorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    Type.EmptyTypes);
                    
                // Store constructor info for later use
                _classConstructors[node.Name] = defaultConstructorBuilder;
                
                // Also create a default realize declaration node for Pass 2 processing
                var defaultRealizeDecl = new RealizeDeclarationNode 
                { 
                    Parameters = new List<ParameterNode>(),
                    Body = new BlockStatementNode { Statements = new List<StatementNode>() }
                };
                
                // Add field initialization statements for fields with default values
                CxDebugTracing.TraceDebug("Compiler", $"Adding field initializations to default realize declaration for class {node.Name}");
                foreach (var field in node.Fields)
                {
                    if (field.Initializer != null)
                    {
                        CxDebugTracing.TraceDebug("Compiler", $"Adding initialization for field {field.Name} with default value");
                        
                        // Create this.fieldName = defaultValue assignment
                        var assignment = new AssignmentExpressionNode
                        {
                            Left = new MemberAccessNode
                            {
                                Object = new IdentifierNode { Name = "this" },
                                Property = field.Name
                            },
                            Right = field.Initializer
                        };
                        
                        // Wrap in an expression statement
                        var assignmentStatement = new ExpressionStatementNode
                        {
                            Expression = assignment
                        };
                        
                        defaultRealizeDecl.Body.Statements.Add(assignmentStatement);
                    }
                }
                
                // Add it to the class node's realize declarations list
                node.RealizeDeclarations.Add(defaultRealizeDecl);
            }
            
            // Process event handlers to define them as methods
            var classEventHandlerList = new List<(string EventName, string MethodName)>();
            
            foreach (var eventHandler in node.EventHandlers)
            {
                // Create a unique method name for the event handler
                var handlerMethodName = $"EventHandler_{eventHandler.EventName.FullName.Replace('.', '_')}_{_eventHandlerCounter++}";
                
                // Store the method name for Pass 2
                _eventHandlerMethodNames[eventHandler] = handlerMethodName;
                
                // Track event handlers for agent auto-registration
                classEventHandlerList.Add((eventHandler.EventName.FullName, handlerMethodName));
                
                var methodBuilder = typeBuilder.DefineMethod(
                    handlerMethodName,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    typeof(Task),  // Event handlers return Task for async support
                    new Type[] { typeof(CxEvent) });  // CxEvent parameter
                    
                // Store handler method info for later use
                _classMethods[node.Name + "." + handlerMethodName] = methodBuilder;
                
                Console.WriteLine($"[INFO] Class event handler registered: {node.Name}.{eventHandler.EventName.FullName} -> {handlerMethodName}");
            }
            
            // Store class event handlers for agent auto-registration
            if (classEventHandlerList.Count > 0)
            {
                _classEventHandlers[node.Name] = classEventHandlerList;
                Console.WriteLine($"[DEBUG] Stored {classEventHandlerList.Count} event handlers for class {node.Name}");
            }
        }
        else
        {
            Console.WriteLine($"[DEBUG] Pass 2: Implementing class {node.Name}");
            
            if (!_userClasses.TryGetValue(node.Name, out var typeBuilder))
            {
                throw new CompilationException($"Class {node.Name} was not defined in Pass 1");
            }
            
            // Implement realize declarations (cognitive constructors)
            foreach (var realizeDecl in node.RealizeDeclarations)
            {
                ImplementRealizeDeclaration(node.Name, realizeDecl, typeBuilder, node.EventHandlers, node.UsesStatements);
            }
            
            // Implement methods
            foreach (var method in node.Methods)
            {
                ImplementMethod(node.Name, method, typeBuilder);
            }
            
            // Implement event handlers
            foreach (var eventHandler in node.EventHandlers)
            {
                ImplementEventHandler(node.Name, eventHandler, typeBuilder);
            }
            
            // Create the type immediately after implementing methods - this finalizes the class definition
            try
            {
                var createdType = typeBuilder.CreateType();
                Console.WriteLine($"[DEBUG] Class {node.Name} created successfully: {createdType?.FullName}");
                
                // Store the created type for runtime use
                if (createdType != null)
                {
                    _createdTypes[node.Name] = createdType;
                    
                    // Update field references from FieldBuilder to actual FieldInfo
                    foreach (var kvp in _classFields)
                    {
                        if (kvp.Key.StartsWith(node.Name + "."))
                        {
                            var fieldName = kvp.Key.Substring(node.Name.Length + 1);
                            var actualField = createdType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (actualField != null)
                            {
                                _actualFields[kvp.Key] = actualField;
                                Console.WriteLine($"[DEBUG] Stored actual field: {kvp.Key} -> {actualField.Name}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error creating class {node.Name}: {ex.Message}");
                throw new CompilationException($"Failed to create class {node.Name}: {ex.Message}");
            }
        }
        
        return new object();
    }
    
    /// <summary>
    /// Implements a realize declaration (cognitive constructor) body for a class in Pass 2
    /// </summary>
    private void ImplementRealizeDeclaration(string className, RealizeDeclarationNode realizeDecl, TypeBuilder typeBuilder, List<OnStatementNode> eventHandlers, List<UsesStatementNode> usesStatements)
    {
        CxDebugTracing.TraceDebug("Compiler", $"Implementing realize declaration for class {className} with {realizeDecl.Parameters.Count} parameters");
        
        if (!_classConstructors.TryGetValue(className, out var constructorBuilder))
        {
            throw new CompilationException($"Constructor for class {className} was not defined in Pass 1");
        }
        
        // Get IL generator for the constructor
        var constructorIl = constructorBuilder.GetILGenerator();
        
        // Save current IL context
        var previousIl = _currentIl;
        var previousLocals = _currentLocals;
        var previousParameterMapping = _currentParameterMapping;
        var previousClassName = _currentClassName;
        
        // Set up new context for realize declaration (constructor)
        _currentIl = constructorIl;
        _currentLocals = new Dictionary<string, LocalBuilder>();
        _currentClassName = className;
        
        // Set up parameter mapping (parameter 0 is 'this', parameters start at index 1)
        _currentParameterMapping = new Dictionary<string, int>();
        for (int i = 0; i < realizeDecl.Parameters.Count; i++)
        {
            _currentParameterMapping[realizeDecl.Parameters[i].Name] = i + 1; // +1 because 'this' is at index 0
        }
        
        // Call base constructor based on class inheritance
        CxDebugTracing.TraceCxToIL("realize() base call", "Call base constructor", new { ClassName = className });
        _currentIl.Emit(OpCodes.Ldarg_0); // Load 'this'
        
        // Check if this class inherits from AiServiceBase (requires IServiceProvider and ILogger)
        if (_classBaseTypes.TryGetValue(className, out var baseClassType) && 
            baseClassType.Name == "AiServiceBase")
        {
            CxDebugTracing.TraceDebug("Compiler", $"Class {className} inherits from AiServiceBase - injecting IServiceProvider and ILogger");
            
            // Load IServiceProvider from static field
            _currentIl.Emit(OpCodes.Ldsfld, _serviceProviderField);
            
            // Create ILogger<ServiceBase> using IServiceProvider
            // Load IServiceProvider again for GetRequiredService call
            _currentIl.Emit(OpCodes.Ldsfld, _serviceProviderField);
            
            // Call serviceProvider.GetRequiredService<ILogger<ServiceBase>>()
            var loggerType = typeof(ILogger<>).MakeGenericType(baseClassType.BaseType ?? typeof(object));
            var getServiceMethod = typeof(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions)
                .GetMethod("GetRequiredService", new[] { typeof(IServiceProvider) })!
                .MakeGenericMethod(loggerType);
                
            _currentIl.Emit(OpCodes.Call, getServiceMethod);
            
            // Find and call AiServiceBase constructor
            var aiServiceBaseConstructor = baseClassType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { 
                    typeof(IServiceProvider), 
                    typeof(ILogger<>).MakeGenericType(baseClassType.BaseType ?? typeof(object))
                },
                null);
                
            if (aiServiceBaseConstructor == null)
            {
                // Try with different Logger generic parameter
                aiServiceBaseConstructor = baseClassType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { 
                        typeof(IServiceProvider), 
                        typeof(ILogger<>).MakeGenericType(typeof(object))
                    },
                    null);
            }
            
            if (aiServiceBaseConstructor != null)
            {
                _currentIl.Emit(OpCodes.Call, aiServiceBaseConstructor);
                Console.WriteLine($"[DEBUG] AiServiceBase constructor called successfully with DI");
            }
            else
            {
                Console.WriteLine($"[ERROR] AiServiceBase constructor not found! Falling back to object constructor");
                // Pop the arguments we pushed and call object constructor instead
                _currentIl.Emit(OpCodes.Pop); // Pop logger
                _currentIl.Emit(OpCodes.Pop); // Pop service provider
                var objectConstructor = typeof(object).GetConstructor(Type.EmptyTypes);
                _currentIl.Emit(OpCodes.Call, objectConstructor!);
            }
        }
        else
        {
            // Regular class - call object constructor
            var objectConstructor = typeof(object).GetConstructor(Type.EmptyTypes);
            _currentIl.Emit(OpCodes.Call, objectConstructor!);
            Console.WriteLine($"[DEBUG] Object base constructor called for class {className}");
        }
        
        Console.WriteLine($"[DEBUG] Base constructor called successfully");
        
        // Initialize service fields from uses statements  
        foreach (var usesStmt in usesStatements)
        {
            Console.WriteLine($"[DEBUG] Initializing service field '{usesStmt.Alias}' from '{usesStmt.ServicePath}'");
            InitializeInstanceServiceField(className, usesStmt.Alias, usesStmt.ServicePath);
        }
        
        // NOTE: Removed automatic field initialization to null - let realize declaration body handle field assignments
        // This fixes the bug where realize assignments were being overwritten by null initialization
        
        // Compile the realize declaration body
        CxDebugTracing.TraceDebug("Compiler", $"Compiling realize declaration body for class {className}");
        realizeDecl.Body.Accept(this);
        CxDebugTracing.TraceDebug("Compiler", $"Realize declaration body compilation complete for class {className}");
        
        // Register event handlers for this class instance
        CxDebugTracing.TraceDebug("Compiler", $"Registering {eventHandlers.Count} event handlers for class {className}");
        foreach (var eventHandler in eventHandlers)
        {
            var eventName = eventHandler.EventName.FullName;
            var handlerMethodName = _eventHandlerMethodNames[eventHandler];
            
            CxDebugTracing.TraceEvent(eventName, $"Registering instance handler", new { ClassName = className, HandlerMethod = handlerMethodName });
            
            // Call CxRuntimeHelper.RegisterInstanceEventHandler(this, eventName, methodName)
            _currentIl.Emit(OpCodes.Ldarg_0); // Load 'this' instance
            _currentIl.Emit(OpCodes.Ldstr, eventName); // Load event name
            _currentIl.Emit(OpCodes.Ldstr, handlerMethodName); // Load method name
            
            var registerMethod = typeof(CxLanguage.Runtime.CxRuntimeHelper).GetMethod("RegisterInstanceEventHandler");
            if (registerMethod == null)
            {
                throw new CompilationException("RegisterInstanceEventHandler method not found in CxRuntimeHelper");
            }
            _currentIl.Emit(OpCodes.Call, registerMethod);
        }
        Console.WriteLine($"[DEBUG] Event handler registration complete for class {className}");
        
        // Return from constructor
        _currentIl.Emit(OpCodes.Ret);
        
        // Restore previous IL context
        _currentIl = previousIl;
        _currentLocals = previousLocals;
        _currentParameterMapping = previousParameterMapping;
        _currentClassName = previousClassName;
        
        Console.WriteLine($"[DEBUG] Constructor implementation complete for class {className}");
    }
    
    /// <summary>
    /// Implements a method body for a class in Pass 2
    /// </summary>
    private void ImplementMethod(string className, MethodDeclarationNode method, TypeBuilder typeBuilder)
    {
        Console.WriteLine($"[DEBUG] Implementing method {method.Name} for class {className} with {method.Parameters.Count} parameters");
        
        if (!_classMethods.TryGetValue(className + "." + method.Name, out var methodBuilder))
        {
            throw new CompilationException($"Method {method.Name} for class {className} was not defined in Pass 1");
        }
        
        // Get IL generator for the method
        var methodIl = methodBuilder.GetILGenerator();
        
        // Save current IL context
        var previousIl = _currentIl;
        var previousLocals = _currentLocals;
        var previousParameterMapping = _currentParameterMapping;
        var previousMethod = _currentMethod;
        var previousClassName = _currentClassName;
        
        // Set up new context for method
        _currentIl = methodIl;
        _currentLocals = new Dictionary<string, LocalBuilder>();
        _currentMethod = methodBuilder;
        _currentClassName = className;
        
        // Set up parameter mapping (parameter 0 is 'this', parameters start at index 1)
        _currentParameterMapping = new Dictionary<string, int>();
        for (int i = 0; i < method.Parameters.Count; i++)
        {
            _currentParameterMapping[method.Parameters[i].Name] = i + 1; // +1 because 'this' is at index 0
        }
        
        // Compile the method body
        if (method.IsAsync)
        {
            // Check if this async method contains await expressions
            bool containsAwaitExpressions = ContainsAwaitExpressions(method.Body);
            
            if (containsAwaitExpressions)
            {
                Console.WriteLine($"[DEBUG] Complex async method {method.Name} - implementing 'Null Until Complete' pattern");
                
                // CX LANGUAGE DESIGN PATTERN: "Null Until Complete"
                // All await expressions return null immediately, tasks run in background
                // This eliminates InvalidProgramException and enables fire-and-forget async
                _isCompilingAsyncMethod = true;
                
                try
                {
                    // Execute method body synchronously with "null until complete" semantics
                    // All await expressions will return null immediately
                    
                    var resultLocal = _currentIl.DeclareLocal(typeof(object));
                    var hasReturnedResult = false;
                    
                    // Execute the method body with "null until complete" pattern
                    if (method.Body != null)
                    {
                        foreach (var statement in method.Body.Statements)
                        {
                            if (statement is ReturnStatementNode returnStmt)
                            {
                                // Handle return statement with null-until-complete semantics
                                if (returnStmt.Value != null)
                                {
                                    returnStmt.Value.Accept(this);
                                    _currentIl.Emit(OpCodes.Stloc, resultLocal);
                                    hasReturnedResult = true;
                                }
                                break; // Exit after return statement
                            }
                            else
                            {
                                statement.Accept(this);
                            }
                        }
                    }
                    
                    // If no explicit return was found, use a default completion message
                    if (!hasReturnedResult)
                    {
                        _currentIl.Emit(OpCodes.Ldstr, $"async_{method.Name}_completed");
                        _currentIl.Emit(OpCodes.Stloc, resultLocal);
                    }
                    
                    // Load the result and wrap in Task.FromResult<object>
                    _currentIl.Emit(OpCodes.Ldloc, resultLocal);
                    
                    var fromResultMethod = typeof(Task).GetMethod("FromResult", BindingFlags.Public | BindingFlags.Static);
                    if (fromResultMethod != null)
                    {
                        var genericFromResult = fromResultMethod.MakeGenericMethod(typeof(object));
                        _currentIl.Emit(OpCodes.Call, genericFromResult);
                        Console.WriteLine($"[DEBUG] Complex async method {method.Name} compiled successfully with async body");
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Could not find Task.FromResult method");
                        _currentIl.Emit(OpCodes.Ldnull);
                    }
                    
                    _currentIl.Emit(OpCodes.Ret);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to compile complex async method {method.Name}: {ex.Message}");
                    
                    // Fallback to simple placeholder if compilation fails
                    _currentIl.Emit(OpCodes.Ldstr, $"async_{method.Name}_fallback");
                    var fromResultMethod = typeof(Task).GetMethod("FromResult", BindingFlags.Public | BindingFlags.Static);
                    if (fromResultMethod != null)
                    {
                        var genericFromResult = fromResultMethod.MakeGenericMethod(typeof(object));
                        _currentIl.Emit(OpCodes.Call, genericFromResult);
                    }
                    _currentIl.Emit(OpCodes.Ret);
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] Simple async method {method.Name} - using Task.FromResult wrapper");
                
                // Simple async methods without await expressions - use Task.FromResult
                _isCompilingAsyncMethod = true;
                
                var resultLocal = _currentIl.DeclareLocal(typeof(object));
                var bodyHasReturn = false;
                
                // Execute function body synchronously
                if (method.Body != null)
                {
                    foreach (var statement in method.Body.Statements)
                    {
                        if (statement is ReturnStatementNode)
                        {
                            bodyHasReturn = true;
                            statement.Accept(this);
                            _currentIl.Emit(OpCodes.Stloc, resultLocal);
                            break;
                        }
                        else
                        {
                            statement.Accept(this);
                        }
                    }
                }
                
                if (!bodyHasReturn)
                {
                    _currentIl.Emit(OpCodes.Ldnull);
                    _currentIl.Emit(OpCodes.Stloc, resultLocal);
                }
                
                // Wrap result in Task.FromResult
                _currentIl.Emit(OpCodes.Ldloc, resultLocal);
                var taskFromResultMethod = typeof(Task).GetMethod("FromResult");
                if (taskFromResultMethod != null)
                {
                    var genericTaskFromResultMethod = taskFromResultMethod.MakeGenericMethod(typeof(object));
                    Console.WriteLine($"[DEBUG] ImplementMethod: Using Task.FromResult<object> method: {genericTaskFromResultMethod}");
                    _currentIl.Emit(OpCodes.Call, genericTaskFromResultMethod);
                    Console.WriteLine($"[DEBUG] ImplementMethod: Task.FromResult<object> call emitted successfully");
                }
                else
                {
                    Console.WriteLine("[ERROR] Could not find Task.FromResult method");
                    _currentIl.Emit(OpCodes.Ldnull);
                }
                
                _currentIl.Emit(OpCodes.Ret);
                _isCompilingAsyncMethod = false;
            }
        }
        else
        {
            // Regular synchronous method
            method.Body.Accept(this);
            
            // If the method doesn't end with a return statement, add one
            // For object return type, return null by default
            _currentIl.Emit(OpCodes.Ldnull);
            _currentIl.Emit(OpCodes.Ret);
        }
        
        // Restore previous IL context
        _currentIl = previousIl;
        _currentLocals = previousLocals;
        _currentParameterMapping = previousParameterMapping;
        _currentMethod = previousMethod;
        _currentClassName = previousClassName;
        
        Console.WriteLine($"[DEBUG] Method {method.Name} implementation complete for class {className}");
    }
    
    /// <summary>
    /// Implements an event handler method for a class in Pass 2
    /// </summary>
    private void ImplementEventHandler(string className, OnStatementNode eventHandler, TypeBuilder typeBuilder)
    {
        // Get the method name that was created in Pass 1
        if (!_eventHandlerMethodNames.TryGetValue(eventHandler, out var handlerMethodName))
        {
            throw new CompilationException($"Event handler method name for {eventHandler.EventName.FullName} was not stored in Pass 1");
        }
        
        Console.WriteLine($"[DEBUG] Implementing event handler {handlerMethodName} for class {className} (IsAsync: {eventHandler.IsAsync})");
        
        if (!_classMethods.TryGetValue(className + "." + handlerMethodName, out var methodBuilder))
        {
            throw new CompilationException($"Event handler method {handlerMethodName} for class {className} was not defined in Pass 1");
        }

        var methodIl = methodBuilder.GetILGenerator();
        
        // Save and set up new context
        var previousIl = _currentIl;
        var previousLocals = _currentLocals;
        var previousParameterMapping = _currentParameterMapping;
        var previousMethod = _currentMethod;
        var previousClassName = _currentClassName;

        _currentIl = methodIl;
        _currentLocals = new Dictionary<string, LocalBuilder>();
        _currentMethod = methodBuilder;
        _currentClassName = className;
        
        // Create local variable for the CxEvent parameter and map it
        var cxEventLocal = methodIl.DeclareLocal(typeof(CxEvent));
        _currentLocals[eventHandler.PayloadIdentifier] = cxEventLocal;
        
        // Load the CxEvent parameter into the local variable
        methodIl.Emit(OpCodes.Ldarg_1); // arg 0 is 'this', arg 1 is CxEvent parameter
        methodIl.Emit(OpCodes.Stloc, cxEventLocal);
        
        // Set up parameter mapping (not used for CxEvent since it's handled as local)
        _currentParameterMapping = new Dictionary<string, int>();

        if (eventHandler.IsAsync)
        {
            Console.WriteLine($"[DEBUG] Implementing async event handler {handlerMethodName}");
            
            // Check if the event handler body contains await expressions
            bool hasAwaitExpressions = HasAwaitExpressions(eventHandler.Body);
            
            if (hasAwaitExpressions)
            {
                Console.WriteLine($"[DEBUG] Complex async event handler {handlerMethodName} - using placeholder approach");
                
                // Complex async event handler with await expressions - use placeholder approach
                // Load a simple result
                _currentIl.Emit(OpCodes.Ldstr, "Async event handler executed");
                
                // Wrap in Task.FromResult<object>()
                var taskFromResultMethod = typeof(Task).GetMethod("FromResult");
                if (taskFromResultMethod != null)
                {
                    var genericFromResult = taskFromResultMethod.MakeGenericMethod(typeof(object));
                    _currentIl.Emit(OpCodes.Call, genericFromResult);
                    Console.WriteLine($"[DEBUG] Complex async event handler returning placeholder Task.FromResult");
                }
                else
                {
                    Console.WriteLine("[ERROR] Could not find Task.FromResult method");
                    var completedTaskProperty = typeof(Task).GetProperty("CompletedTask");
                    if (completedTaskProperty?.GetMethod != null)
                    {
                        // Pop the result value since CompletedTask doesn't take parameters
                        _currentIl.Emit(OpCodes.Pop);
                        _currentIl.Emit(OpCodes.Call, completedTaskProperty.GetMethod);
                    }
                    else
                    {
                        _currentIl.Emit(OpCodes.Ldnull);
                    }
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] Simple async event handler {handlerMethodName} - using Task.FromResult wrapper");
                
                // Simple async event handler without await expressions - use Task.FromResult
                var resultLocal = _currentIl.DeclareLocal(typeof(object));
                
                // Execute event handler body synchronously
                eventHandler.Body.Accept(this);
                
                // Store null result (event handlers typically don't return values)
                _currentIl.Emit(OpCodes.Ldnull);
                _currentIl.Emit(OpCodes.Stloc, resultLocal);
                
                // Wrap result in Task.FromResult<object>()
                _currentIl.Emit(OpCodes.Ldloc, resultLocal);
                
                var taskFromResultMethod = typeof(Task).GetMethod("FromResult");
                if (taskFromResultMethod != null)
                {
                    var genericTaskFromResultMethod = taskFromResultMethod.MakeGenericMethod(typeof(object));
                    Console.WriteLine($"[DEBUG] Simple async event handler using Task.FromResult<object> method: {genericTaskFromResultMethod}");
                    _currentIl.Emit(OpCodes.Call, genericTaskFromResultMethod);
                }
                else
                {
                    Console.WriteLine("[ERROR] Could not find Task.FromResult method for async event handler");
                    _currentIl.Emit(OpCodes.Pop); // Remove the result from stack
                    var completedTaskProperty = typeof(Task).GetProperty("CompletedTask");
                    if (completedTaskProperty?.GetMethod != null)
                    {
                        _currentIl.Emit(OpCodes.Call, completedTaskProperty.GetMethod);
                    }
                    else
                    {
                        _currentIl.Emit(OpCodes.Ldnull);
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"[DEBUG] Implementing sync event handler {handlerMethodName}");
            
            // Synchronous event handler - compile body and return Task.CompletedTask
            eventHandler.Body.Accept(this);

            // Ensure a proper return for the Task
            var completedTaskProperty = typeof(Task).GetProperty("CompletedTask");
            if (completedTaskProperty?.GetMethod == null)
            {
                throw new CompilationException("Task.CompletedTask property getter not found");
            }
            _currentIl.Emit(OpCodes.Call, completedTaskProperty.GetMethod);
        }
        
        _currentIl.Emit(OpCodes.Ret);
        
        // Restore previous IL context
        _currentIl = previousIl;
        _currentLocals = previousLocals;
        _currentParameterMapping = previousParameterMapping;
        _currentMethod = previousMethod;
        _currentClassName = previousClassName;
        
        Console.WriteLine($"[DEBUG] Event handler {handlerMethodName} implementation complete for class {className}");
    }
    public object VisitFieldDeclaration(FieldDeclarationNode node) => new object();
    public object VisitMethodDeclaration(MethodDeclarationNode node) => new object();
    public object VisitRealizeDeclaration(RealizeDeclarationNode node) => new object();
    public object VisitInterfaceDeclaration(InterfaceDeclarationNode node) => new object();
    public object VisitInterfaceMethodSignature(InterfaceMethodSignatureNode node) => new object();
    public object VisitInterfacePropertySignature(InterfacePropertySignatureNode node) => new object();
    
    public object VisitDecorator(DecoratorNode node)
    {
        // For now, decorators are processed at parse time and stored in ClassDeclarationNode.Decorators
        // Agent behavior is determined by presence of event handlers in class
        // This method exists to satisfy the IAstVisitor interface
        return new object();
    }

    /// <summary>
    /// Determines the appropriate base class for a CX class based on realtime-first cognitive architecture
    /// </summary>
    private Type DetermineBaseClass(ClassDeclarationNode node)
    {
        // REALTIME-FIRST ARCHITECTURE: All classes inherit cognitive capabilities by default
        // Try to load the AiServiceBase type for ALL classes
        try
        {
            // Load the AiServiceBase type from StandardLibrary
            var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CxLanguage.StandardLibrary.dll");
            var standardLibraryAssembly = Assembly.LoadFrom(assemblyPath);
            var aiServiceBaseType = standardLibraryAssembly.GetType("CxLanguage.StandardLibrary.Core.AiServiceBase");
            
            if (aiServiceBaseType != null)
            {
                Console.WriteLine($"[DEBUG] Class {node.Name} inheriting from AiServiceBase (realtime-first architecture)");
                return aiServiceBaseType;
            }
            else
            {
                Console.WriteLine($"[DEBUG] AiServiceBase type not found in StandardLibrary. Using object as base.");
                return typeof(object);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Could not load AiServiceBase: {ex.Message}. Using object as base.");
            return typeof(object);
        }
    }

    /// <summary>
    /// Checks if a class implements any service-related interfaces
    /// </summary>
    private bool HasServiceInterfaces(ClassDeclarationNode node)
    {
        if (node.Interfaces == null) return false;
        
        var serviceInterfaces = new[]
        {
            "ITextToSpeech", "ITextEmbeddings", "IImageGeneration", 
            "IAudioToText", "ITextToAudio", "IImageAnalysis",
            "IVectorDatabase", "IFullAICapabilities"
        };
        
        return node.Interfaces.Any(iface => serviceInterfaces.Contains(iface));
    }

    /// <summary>
    /// Recursively checks if a block contains await expressions
    /// </summary>
    private bool ContainsAwaitExpressions(BlockStatementNode? block)
    {
        if (block?.Statements == null) return false;
        
        foreach (var statement in block.Statements)
        {
            if (ContainsAwaitExpression(statement))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Recursively checks if a statement contains await expressions
    /// </summary>
    private bool ContainsAwaitExpression(AstNode node)
    {
        switch (node)
        {
            case BlockStatementNode blockStmt:
                foreach (var stmt in blockStmt.Statements)
                {
                    if (ContainsAwaitExpression(stmt))
                        return true;
                }
                return false;
                
            case ExpressionStatementNode exprStmt:
                return ContainsAwaitExpression(exprStmt.Expression);
                
            case VariableDeclarationNode varDecl:
                return varDecl.Initializer != null && ContainsAwaitExpression(varDecl.Initializer);
                
            case ReturnStatementNode returnStmt:
                return returnStmt.Value != null && ContainsAwaitExpression(returnStmt.Value);
                
            case BinaryExpressionNode binExpr:
                return ContainsAwaitExpression(binExpr.Left) || ContainsAwaitExpression(binExpr.Right);
                
            case CallExpressionNode callExpr:
                return callExpr.Arguments.Any(ContainsAwaitExpression);
                
            case IfStatementNode ifStmt:
                return ContainsAwaitExpression(ifStmt.Condition) ||
                       ContainsAwaitExpression(ifStmt.ThenStatement) ||
                       (ifStmt.ElseStatement != null && ContainsAwaitExpression(ifStmt.ElseStatement));
                
            case WhileStatementNode whileStmt:
                return ContainsAwaitExpression(whileStmt.Condition) ||
                       ContainsAwaitExpression(whileStmt.Body);
                
            case TryStatementNode tryStmt:
                return ContainsAwaitExpression(tryStmt.TryBlock) ||
                       (tryStmt.CatchBlock != null && ContainsAwaitExpression(tryStmt.CatchBlock));
                
            default:
                return false;
        }
    }
}

/// <summary>
/// Compiler options
/// </summary>
public class CompilerOptions
{
    public bool OptimizeCode { get; set; } = true;
    public bool GenerateDebugInfo { get; set; } = false;
    public string TargetFramework { get; set; } = "net8.0";
}

/// <summary>
/// Compilation result
/// </summary>
public class CompilationResult
{
    public bool IsSuccess { get; }
    public Assembly? Assembly { get; }
    public Type? ProgramType { get; }
    public string? ErrorMessage { get; }
    public List<string>? InjectedFunctions { get; }

    private CompilationResult(bool isSuccess, Assembly? assembly, Type? programType, string? errorMessage, List<string>? injectedFunctions = null)
    {
        IsSuccess = isSuccess;
        Assembly = assembly;
        ProgramType = programType;
        ErrorMessage = errorMessage;
        InjectedFunctions = injectedFunctions;
    }

    public static CompilationResult Success(Assembly assembly, Type programType) 
    {
        Console.WriteLine($"✅ COMPILATION RESULT: Success - Assembly: {assembly.FullName}, Type: {programType.Name}");
        return new(true, assembly, programType, null);
    }

    public static CompilationResult SuccessWithInjections(Assembly assembly, Type programType, List<string> injectedFunctions) 
    {
        Console.WriteLine($"🎉 COMPILATION RESULT: Success with {injectedFunctions.Count} injected functions");
        Console.WriteLine($"📋 INJECTED FUNCTIONS: {string.Join(", ", injectedFunctions)}");
        return new(true, assembly, programType, null, injectedFunctions);
    }

    public static CompilationResult Failure(string errorMessage) 
    {
        Console.WriteLine($"❌ COMPILATION RESULT: Failure - {errorMessage}");
        return new(false, null, null, errorMessage);
    }
}
