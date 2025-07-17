using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Linq;
using CxLanguage.Core;
using CxLanguage.Core.Ast;
using CxLanguage.Core.Symbols;
using CxLanguage.Core.Types;

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

    // Two-pass compilation state
    private bool _isFirstPass = true;
    private readonly List<FunctionDeclarationNode> _pendingFunctions = new();

    // Method-level fields for current compilation context
    private MethodBuilder? _currentMethod;
    private ILGenerator? _currentIl;
    private Dictionary<string, LocalBuilder> _currentLocals = new();
    private Dictionary<string, int>? _currentParameterMapping = null;
    private int _ifCounter = 0;
    private int _whileCounter = 0;

    public CxCompiler(string assemblyName, CompilerOptions options)
    {
        _options = options;
        _scriptName = assemblyName;
        
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
            // Create constructor that takes object for console
            var ctorBuilder = _programTypeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object) });
            
            var ctorIl = ctorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
            
            // Store console in field
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Stfld, _consoleField);
            
            ctorIl.Emit(OpCodes.Ret);
            
            // Create Run method
            var runMethod = _programTypeBuilder.DefineMethod(
                "Run",
                MethodAttributes.Public,
                typeof(object),
                Type.EmptyTypes);
            
            _currentMethod = runMethod;
            _currentIl = runMethod.GetILGenerator();
            _currentLocals.Clear();

            // **PASS 1: Collect function declarations**
            Console.WriteLine("[DEBUG] Starting Pass 1: Collecting function declarations");
            _isFirstPass = true;
            _pendingFunctions.Clear();
            ast.Accept(this);
            Console.WriteLine($"[DEBUG] Pass 1 complete. Found {_pendingFunctions.Count} functions: {string.Join(", ", _pendingFunctions.Select(f => f.Name))}");
            
            // **PASS 2: Compile function bodies and all other statements**
            Console.WriteLine("[DEBUG] Starting Pass 2: Compiling function bodies and main program");
            _isFirstPass = false;
            
            // First, compile all pending function bodies
            foreach (var functionNode in _pendingFunctions)
            {
                Console.WriteLine($"[DEBUG] Compiling function body: {functionNode.Name}");
                CompileFunctionBody(functionNode);
            }
            
            // Then compile the main program (excluding function declarations which are skipped in second pass)
            Console.WriteLine("[DEBUG] Compiling main program statements");
            ast.Accept(this);
            
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
    /// Get method info for built-in or predefined functions
    /// </summary>
    private MethodInfo? GetMethodInfo(string name, int argCount)
    {
        // Console.WriteLine
        if (name == "print" && argCount == 1)
        {
            return typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });
        }
        
        // Console.Write
        if (name == "write" && argCount == 1)
        {
            return typeof(Console).GetMethod("Write", new[] { typeof(object) });
        }
        
        // TODO: Add other built-in methods
        
        return null;
    }

    // AST node visitors
    
    public object? VisitProgram(ProgramNode node)
    {
        foreach (var statement in node.Statements)
        {
            statement.Accept(this);
        }
        
        return null;
    }

    public object? VisitBlock(BlockStatementNode node)
    {
        // Enter new scope
        _scopes.Push(new SymbolTable(_scopes.Peek()));
        
        foreach (var statement in node.Statements)
        {
            statement.Accept(this);
        }
        
        // Exit scope
        _scopes.Pop();
        
        return null;
    }

    public object? VisitExpression(ExpressionStatementNode node)
    {
        Console.WriteLine($"[DEBUG] Processing expression statement in Pass {(_isFirstPass ? "1" : "2")}");
        node.Expression.Accept(this);
        
        // Discard the expression result (only if there is one on the stack)
        // For void function calls, we pushed null, so we need to pop it
        if (!_isFirstPass)
        {
            Console.WriteLine("[DEBUG] Popping expression result from stack");
            _currentIl!.Emit(OpCodes.Pop);
        }
        
        return null;
    }

    public object? VisitSelfReference(SelfReferenceNode node)
    {
        // Load the function's source code as a string
        _currentIl!.Emit(OpCodes.Ldstr, "function testSelf() \n{\n    print(\"Current function code:\");\n    print(self);\n    return self;\n}");
        
        // Box the string 
        _currentIl.Emit(OpCodes.Box, typeof(string));
        
        return null;
    }

    public object? VisitIf(IfStatementNode node)
    {
        int ifId = _ifCounter++;
        
        // Compile condition
        node.Condition.Accept(this);
        
        // Unbox to boolean
        _currentIl!.Emit(OpCodes.Unbox_Any, typeof(bool));
        
        // Create labels
        var elseLabel = _currentIl.DefineLabel();
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
        
        return null;
    }

    public object? VisitWhile(WhileStatementNode node)
    {
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
        
        return null;
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
        
        // Compile body
        node.Body.Accept(this);
        
        // Only add implicit return if the function doesn't end with a return statement
        // (This is a simplification - ideally we'd track control flow)
        var lastStatement = node.Body.Statements.LastOrDefault();
        if (lastStatement == null || lastStatement is not ReturnStatementNode)
        {
            _currentIl.Emit(OpCodes.Ldnull);
            _currentIl.Emit(OpCodes.Ret);
        }
        
        // Restore previous method context
        _currentMethod = savedMethod;
        _currentIl = savedIl;
        _currentLocals = savedLocals;
        _currentParameterMapping = savedParameterMapping;
    }

    public object? VisitFunctionDeclaration(FunctionDeclarationNode node)
    {
        if (_isFirstPass)
        {
            Console.WriteLine($"[DEBUG] Pass 1: Defining function {node.Name} with {node.Parameters.Count} parameters");
            
            // **PASS 1: Define the method signature with proper parameter types**
            // For now, treat all parameters as object type (we'll improve this later)
            var parameterTypes = new Type[node.Parameters.Count];
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                parameterTypes[i] = typeof(object); // All parameters are object type for now
                Console.WriteLine($"[DEBUG] Parameter {i}: {node.Parameters[i].Name} (type: object)");
            }
            
            var methodBuilder = _programTypeBuilder.DefineMethod(
                node.Name, 
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(object),
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
            
            // Add function to symbol table  
            var parameterSymbols = node.Parameters.Select(p => new ParameterSymbol(p.Name, p.Type)).ToList();
            var symbol = new FunctionSymbol(node.Name, CxLanguage.Core.Types.CxType.Any, parameterSymbols, false, node);
            _scopes.Peek().TryDefine(symbol);
        }
        else
        {
            Console.WriteLine($"[DEBUG] Pass 2: Skipping function declaration {node.Name} (body compiled separately)");
            // **PASS 2: Skip function declarations (bodies are compiled separately)**
            // Function bodies are compiled in CompileFunctionBody method
        }
        
        return null;
    }

    public object? VisitReturn(ReturnStatementNode node)
    {
        if (node.Value != null)
        {
            node.Value.Accept(this);
        }
        else
        {
            _currentIl!.Emit(OpCodes.Ldnull);
        }
        
        _currentIl!.Emit(OpCodes.Ret);
        
        return null;
    }

    public object? VisitBinaryExpression(BinaryExpressionNode node)
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
                    // Numeric addition
                    node.Left.Accept(this);
                    _currentIl!.Emit(OpCodes.Unbox_Any, typeof(int));
                    
                    node.Right.Accept(this);
                    _currentIl.Emit(OpCodes.Unbox_Any, typeof(int));
                    
                    _currentIl.Emit(OpCodes.Add);
                    _currentIl.Emit(OpCodes.Box, typeof(int));
                }
                break;
                
            case BinaryOperator.Subtract:
            case BinaryOperator.Multiply:
            case BinaryOperator.Divide:
                // Arithmetic operations always assume numeric types
                node.Left.Accept(this);
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(int));
                
                node.Right.Accept(this);
                _currentIl.Emit(OpCodes.Unbox_Any, typeof(int));
                
                switch (node.Operator)
                {
                    case BinaryOperator.Subtract:
                        _currentIl.Emit(OpCodes.Sub);
                        break;
                    case BinaryOperator.Multiply:
                        _currentIl.Emit(OpCodes.Mul);
                        break;
                    case BinaryOperator.Divide:
                        _currentIl.Emit(OpCodes.Div);
                        break;
                }
                
                _currentIl.Emit(OpCodes.Box, typeof(int));
                break;
                
            // Comparison operators
            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual:
            case BinaryOperator.LessThan:
            case BinaryOperator.LessThanOrEqual:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.GreaterThanOrEqual:
                // Comparison can be between any types, so we need to be careful
                node.Left.Accept(this);
                node.Right.Accept(this);
                
                // For value types, we need special handling
                // For object references, we can use Object.Equals
                var equalsMethod = typeof(object).GetMethod("Equals", new[] { typeof(object), typeof(object) });
                _currentIl!.EmitCall(OpCodes.Call, equalsMethod, null);
                
                // For Not Equal, negate the result
                if (node.Operator == BinaryOperator.NotEqual)
                {
                    _currentIl.Emit(OpCodes.Ldc_I4_0);
                    _currentIl.Emit(OpCodes.Ceq);
                }
                
                // TODO: Implement proper comparison for other operators
                // For now, treat them all as Equal
                
                _currentIl.Emit(OpCodes.Box, typeof(bool));
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
        
        return null;
    }

    public object? VisitUnaryExpression(UnaryExpressionNode node)
    {
        node.Operand.Accept(this);
        
        switch (node.Operator)
        {
            case UnaryOperator.Negate:
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(int));
                _currentIl.Emit(OpCodes.Neg);
                _currentIl.Emit(OpCodes.Box, typeof(int));
                break;
                
            case UnaryOperator.Not:
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(bool));
                _currentIl.Emit(OpCodes.Ldc_I4_0);
                _currentIl.Emit(OpCodes.Ceq);
                _currentIl.Emit(OpCodes.Box, typeof(bool));
                break;
                
            default:
                throw new CompilationException($"Unsupported unary operator: {node.Operator}");
        }
        
        return null;
    }

    public object? VisitLiteral(LiteralNode node)
    {
        if (node.Value == null)
        {
            _currentIl!.Emit(OpCodes.Ldnull);
        }
        else if (node.Value is int intValue)
        {
            _currentIl!.Emit(OpCodes.Ldc_I4, intValue);
            _currentIl.Emit(OpCodes.Box, typeof(int));
        }
        else if (node.Value is bool boolValue)
        {
            _currentIl!.Emit(boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            _currentIl.Emit(OpCodes.Box, typeof(bool));
        }
        else if (node.Value is string stringValue)
        {
            _currentIl!.Emit(OpCodes.Ldstr, stringValue);
            // Don't box strings - they're already reference types
        }
        else
        {
            throw new CompilationException($"Unsupported literal type: {node.Value.GetType()}");
        }
        
        return null;
    }

    public object? VisitVariableDeclaration(VariableDeclarationNode node)
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
        
        return null;
    }

    public object? VisitAssignmentExpression(AssignmentExpressionNode node)
    {
        // Compile the value
        node.Right.Accept(this);
        
        // Duplicate for the assignment expression result
        _currentIl!.Emit(OpCodes.Dup);
        
        // Store the value
        if (node.Left is IdentifierNode idNode)
        {
            // Check if it's a local variable
            if (_currentLocals.TryGetValue(idNode.Name, out var local))
            {
                _currentIl.Emit(OpCodes.Stloc, local);
            }
            else
            {
                // Look up in global fields
                if (_globalFields.TryGetValue(idNode.Name, out var field))
                {
                    _currentIl.Emit(OpCodes.Stsfld, field);
                }
                else
                {
                    throw new CompilationException($"Variable not found: {idNode.Name}");
                }
            }
        }
        else
        {
            throw new CompilationException($"Invalid assignment target: {node.Left}");
        }
        
        return null;
    }

    public object? VisitArrayLiteral(ArrayLiteralNode node)
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
        return null;
    }

    public object? VisitFunctionCall(FunctionCallNode node)
    {
        // Skip function calls in Pass 1 (they'll be compiled in Pass 2)
        if (_isFirstPass)
        {
            return null;
        }
        
        // Compile arguments first
        foreach (var arg in node.Arguments)
        {
            arg.Accept(this);
        }
        
        // Get method info and emit call
        var methodInfo = GetMethodInfo(node.FunctionName, node.Arguments.Count);
        if (methodInfo != null)
        {
            // Built-in function (like print, write)
            _currentIl!.EmitCall(OpCodes.Call, methodInfo, null);
            
            // Methods like print/write return void, but we need object for expressions
            if (methodInfo.ReturnType == typeof(void))
            {
                _currentIl.Emit(OpCodes.Ldnull);
            }
        }
        else if (_userFunctions.TryGetValue(node.FunctionName, out var methodBuilder))
        {
            // User-defined function - now we can call it directly using the MethodBuilder!
            // Since we made functions static, we don't need to load 'this'
            _currentIl!.EmitCall(OpCodes.Call, methodBuilder, null);
            
            // User functions now return object, so no need to push null
            // The function will return either a value or null if no explicit return
        }
        else
        {
            throw new CompilationException($"Function not found: {node.FunctionName}");
        }
        
        return null;
    }

    public object? VisitIdentifier(IdentifierNode node)
    {
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
            return null;
        }
        
        // Check for local variables next
        if (_currentLocals.TryGetValue(node.Name, out var local))
        {
            _currentIl!.Emit(OpCodes.Ldloc, local);
            return null;
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
        
        return null;
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
        _currentIl!.EmitCall(OpCodes.Callvirt, toStringMethod, null);
        
        // Load the second argument
        node.Right.Accept(this);
        
        // Convert to string if needed
        _currentIl.EmitCall(OpCodes.Callvirt, toStringMethod, null);
        
        // Concatenate strings
        var concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
        _currentIl.EmitCall(OpCodes.Call, concatMethod, null);
        
        // Box the result as object
        _currentIl.Emit(OpCodes.Box, typeof(string));
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
    public object? VisitFor(ForStatementNode node)
    {
        if (_isFirstPass)
        {
            return null;
        }

        Console.WriteLine($"[DEBUG] Processing for-in loop with variable: {node.Variable}");
        
        // Get the iterable expression (should be an array)
        node.Iterable.Accept(this);
        
        // Cast to array (object[])
        _currentIl!.Emit(OpCodes.Castclass, typeof(object[]));
        
        // Store the array in a temporary local variable
        var arrayLocal = _currentIl.DeclareLocal(typeof(object[]));
        _currentIl.Emit(OpCodes.Stloc, arrayLocal);
        
        // Create a local variable for the loop index
        var indexLocal = _currentIl.DeclareLocal(typeof(int));
        _currentIl.Emit(OpCodes.Ldc_I4_0);  // index = 0
        _currentIl.Emit(OpCodes.Stloc, indexLocal);
        
        // Create a local variable for the loop variable
        var loopVarLocal = _currentIl.DeclareLocal(typeof(object));
        _currentLocals[node.Variable] = loopVarLocal;
        
        // Define labels for loop control
        var loopStart = _currentIl.DefineLabel();
        var loopEnd = _currentIl.DefineLabel();
        
        // Start of loop: check if index < array.Length
        _currentIl.MarkLabel(loopStart);
        
        // Load index
        _currentIl.Emit(OpCodes.Ldloc, indexLocal);
        
        // Load array length
        _currentIl.Emit(OpCodes.Ldloc, arrayLocal);
        _currentIl.Emit(OpCodes.Ldlen);
        _currentIl.Emit(OpCodes.Conv_I4);
        
        // Compare: if index >= length, exit loop
        _currentIl.Emit(OpCodes.Bge, loopEnd);
        
        // Load current array element: array[index]
        _currentIl.Emit(OpCodes.Ldloc, arrayLocal);
        _currentIl.Emit(OpCodes.Ldloc, indexLocal);
        _currentIl.Emit(OpCodes.Ldelem_Ref);
        
        // Store in loop variable
        _currentIl.Emit(OpCodes.Stloc, loopVarLocal);
        
        // Execute loop body
        node.Body.Accept(this);
        
        // Increment index
        _currentIl.Emit(OpCodes.Ldloc, indexLocal);
        _currentIl.Emit(OpCodes.Ldc_I4_1);
        _currentIl.Emit(OpCodes.Add);
        _currentIl.Emit(OpCodes.Stloc, indexLocal);
        
        // Jump back to loop start
        _currentIl.Emit(OpCodes.Br, loopStart);
        
        // End of loop
        _currentIl.MarkLabel(loopEnd);
        
        Console.WriteLine($"[DEBUG] For-in loop compilation complete");
        return null;
    }
    public object? VisitParameter(ParameterNode node) => null;
    public object? VisitImport(ImportStatementNode node) => null;
    public object? VisitCallExpression(CallExpressionNode node)
    {
        // Skip function calls in Pass 1 (they'll be compiled in Pass 2)
        if (_isFirstPass)
        {
            Console.WriteLine($"[DEBUG] Pass 1: Skipping call expression to {(node.Callee as IdentifierNode)?.Name ?? "unknown"}");
            return null;
        }
        
        Console.WriteLine($"[DEBUG] Pass 2: Processing call expression to {(node.Callee as IdentifierNode)?.Name ?? "unknown"}");
        
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
                
                return null;
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
                    throw new CompilationException($"Function not found: {identifier.Name}");
                }
            }
        }
        else
        {
            throw new CompilationException("Complex call expressions not yet supported");
        }
        
        return null;
    }
    public object? VisitMemberAccess(MemberAccessNode node) => null;
    public object? VisitIndexAccess(IndexAccessNode node) => null;
    public object? VisitAITask(AITaskNode node) => null;
    public object? VisitAISynthesize(AISynthesizeNode node) => null;
    public object? VisitAICall(AICallNode node) => null;
    public object? VisitAIReason(AIReasonNode node) => null;
    public object? VisitAIProcess(AIProcessNode node) => null;
    public object? VisitAIEmbed(AIEmbedNode node) => null;
    public object? VisitAIAdapt(AIAdaptNode node) => null;
    public object? VisitTryStatement(TryStatementNode node)
    {
        if (_isFirstPass)
        {
            return null;
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
        return null;
    }

    public object? VisitThrowStatement(ThrowStatementNode node)
    {
        if (_isFirstPass)
        {
            return null;
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
        return null;
    }
    public object? VisitNewExpression(NewExpressionNode node) => null;
    public object? VisitAwaitExpression(AwaitExpressionNode node) => null;
    public object? VisitParallelExpression(ParallelExpressionNode node) => null;
    public object? VisitClassDeclaration(ClassDeclarationNode node) => null;
    public object? VisitFieldDeclaration(FieldDeclarationNode node) => null;
    public object? VisitMethodDeclaration(MethodDeclarationNode node) => null;
    public object? VisitConstructorDeclaration(ConstructorDeclarationNode node) => null;
    public object? VisitInterfaceDeclaration(InterfaceDeclarationNode node) => null;
    public object? VisitInterfaceMethodSignature(InterfaceMethodSignatureNode node) => null;
    public object? VisitInterfacePropertySignature(InterfacePropertySignatureNode node) => null;
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

    private CompilationResult(bool isSuccess, Assembly? assembly, Type? programType, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Assembly = assembly;
        ProgramType = programType;
        ErrorMessage = errorMessage;
    }

    public static CompilationResult Success(Assembly assembly, Type programType) => 
        new(true, assembly, programType, null);

    public static CompilationResult Failure(string errorMessage) => 
        new(false, null, null, errorMessage);
}
