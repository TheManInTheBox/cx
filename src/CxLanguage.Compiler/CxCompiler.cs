using System.Reflection;
using System.Reflection.Emit;
using CxLanguage.Core.Ast;
using CxLanguage.Core.Symbols;
using CxLanguage.Core.Types;
using CxLanguage.Core.AI;
using Microsoft.Extensions.Logging;

namespace CxLanguage.Compiler;

/// <summary>
/// Compiles Cx AST to .NET assemblies using IL emission
/// </summary>
public class CxCompiler
{
    private readonly CompilerOptions _options;

    public CxCompiler(CompilerOptions? options = null)
    {
        _options = options ?? new CompilerOptions();
    }

    public CompilationResult Compile(ProgramNode program, string assemblyName)
    {
        try
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(assemblyName),
                AssemblyBuilderAccess.RunAndCollect);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);

            // Create the main program class
            var programType = moduleBuilder.DefineType(
                "Program",
                TypeAttributes.Public | TypeAttributes.Class);

            var compiler = new IlEmitter(moduleBuilder, programType, _options);
            
            // Compile the program
            compiler.CompileProgram(program);

            // Create the type
            var createdType = programType.CreateType();

            return CompilationResult.Success(assemblyBuilder, createdType);
        }
        catch (Exception ex)
        {
            return CompilationResult.Failure($"{ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }
}

/// <summary>
/// IL emission for Cx language constructs
/// </summary>
public class IlEmitter : IAstVisitor<object?>
{
    private readonly ModuleBuilder _moduleBuilder;
    private readonly TypeBuilder _typeBuilder;
    private readonly CompilerOptions _options;
    private readonly SymbolTable _globalScope;
    private readonly Stack<SymbolTable> _scopes;
    private readonly Dictionary<string, FieldBuilder> _globalFields;
    private readonly Dictionary<string, MethodBuilder> _methods;
    private readonly Dictionary<string, Type> _methodReturnTypes;
    private readonly Dictionary<string, LocalBuilder> _currentLocals;
    private ILGenerator? _currentIl;
    private MethodBuilder? _currentMethod;
    private Type? _currentReturnType;
    
    // AI Runtime integration
    private FieldBuilder? _agenticRuntimeField;
    private FieldBuilder? _multiModalAIField;
    private FieldBuilder? _codeSynthesizerField;

    public IlEmitter(ModuleBuilder moduleBuilder, TypeBuilder typeBuilder, CompilerOptions options)
    {
        _moduleBuilder = moduleBuilder;
        _typeBuilder = typeBuilder;
        _options = options;
        _globalScope = new SymbolTable();
        _scopes = new Stack<SymbolTable>();
        _globalFields = new Dictionary<string, FieldBuilder>();
        _methods = new Dictionary<string, MethodBuilder>();
        _methodReturnTypes = new Dictionary<string, Type>();
        _currentLocals = new Dictionary<string, LocalBuilder>();
        _scopes.Push(_globalScope);
        
        // Initialize AI runtime fields
        InitializeAIRuntime();
    }

    // Store global variable initializations for later processing
    private List<VariableDeclarationNode> _globalVariableInits = new List<VariableDeclarationNode>();

    public void CompileProgram(ProgramNode program)
    {
        // First pass: Process function declarations and collect global variables
        var nonVariableStatements = new List<AstNode>();
        
        foreach (var statement in program.Statements)
        {
            if (statement is VariableDeclarationNode globalVar)
            {
                // Don't compile yet, just collect for later initialization
                _globalVariableInits.Add(globalVar);
                
                // Define the field now (use object type for simplicity)
                var field = _typeBuilder.DefineField(
                    globalVar.Name,
                    typeof(object),
                    FieldAttributes.Public | FieldAttributes.Static);
                _globalFields[globalVar.Name] = field;
                
                // Add to global symbol table
                var symbol = new VariableSymbol(globalVar.Name, CxType.Any);
                _globalScope.TryDefine(symbol);
                
                // Do NOT add to nonVariableStatements - it's handled separately
            }
            else if (statement is FunctionDeclarationNode)
            {
                // Process function declarations immediately
                statement.Accept(this);
            }
            else
            {
                // Collect other statements for later processing (excluding VariableDeclarationNode)
                nonVariableStatements.Add(statement);
            }
        }

        // Create a static Main method
        var mainMethod = _typeBuilder.DefineMethod(
            "Main",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            Type.EmptyTypes);

        _currentMethod = mainMethod;
        _currentIl = mainMethod.GetILGenerator();

        // Initialize global variables first
        foreach (var globalVar in _globalVariableInits)
        {
            var field = _globalFields[globalVar.Name];
            globalVar.Initializer.Accept(this);
            _currentIl.Emit(OpCodes.Stsfld, field);
        }

        // Now process the remaining statements (expressions, etc.)
        foreach (var statement in nonVariableStatements)
        {
            statement.Accept(this);
        }

        // Check if there's a user-defined 'main' function
        if (_methods.ContainsKey("main"))
        {
            // Call the user's main function
            _currentIl.Emit(OpCodes.Call, _methods["main"]);
            
            // If main returns a value, we need to pop it since Main returns void
            var userMainMethod = _methods["main"];
            if (userMainMethod.ReturnType != typeof(void))
            {
                _currentIl.Emit(OpCodes.Pop);
            }
        }
        else
        {
            // No main function found - this is for top-level statements
            // In the future, we could execute top-level statements here
        }

        // Return from Main
        _currentIl.Emit(OpCodes.Ret);
    }

    public object? VisitProgram(ProgramNode node)
    {
        // This is handled by CompileProgram
        return null;
    }

    public object? VisitFunctionDeclaration(FunctionDeclarationNode node)
    {
        // Determine parameter types
        var paramTypes = node.Parameters.Select(p => GetSystemType(p.Type)).ToArray();
        var returnType = node.ReturnType != null ? GetSystemType(node.ReturnType) : typeof(void);

        // Create method
        var method = _typeBuilder.DefineMethod(
            node.Name,
            MethodAttributes.Public | MethodAttributes.Static,
            returnType,
            paramTypes);

        _methods[node.Name] = method;
        _methodReturnTypes[node.Name] = returnType;

        // Generate method body
        var previousMethod = _currentMethod;
        var previousIl = _currentIl;
        var previousReturnType = _currentReturnType;

        _currentMethod = method;
        _currentIl = method.GetILGenerator();
        _currentReturnType = returnType;

        // Clear locals for new function
        _currentLocals.Clear();

        // Create new scope for function
        var functionScope = new SymbolTable(_scopes.Peek());
        _scopes.Push(functionScope);

        // Add parameters to scope
        for (int i = 0; i < node.Parameters.Count; i++)
        {
            var param = node.Parameters[i];
            var symbol = new VariableSymbol(param.Name, param.Type);
            functionScope.TryDefine(symbol);
        }

        // Compile function body
        node.Body.Accept(this);

        // Add return if void function
        if (returnType == typeof(void))
        {
            _currentIl.Emit(OpCodes.Ret);
        }

        // Restore previous context
        _scopes.Pop();
        _currentMethod = previousMethod;
        _currentIl = previousIl;
        _currentReturnType = previousReturnType;

        return null;
    }

    public object? VisitVariableDeclaration(VariableDeclarationNode node)
    {
        // For global variables, they are handled in CompileProgram
        if (_scopes.Count == 1)
        {
            // Global variables are handled separately in CompileProgram
            // This should not be reached during normal compilation
            return null;
        }
        else
        {
            // Local variables
            var varType = GetSystemType(node.Initializer.InferredType ?? CxType.Any);
            var local = _currentIl!.DeclareLocal(varType);

            // Add to the locals dictionary for lookup
            _currentLocals[node.Name] = local;

            // Compile initializer and store in local
            node.Initializer.Accept(this);
            _currentIl.Emit(OpCodes.Stloc, local);

            // Add to symbol table
            var symbol = new VariableSymbol(node.Name, node.Initializer.InferredType ?? CxType.Any);
            _scopes.Peek().TryDefine(symbol);
        }

        return null;
    }

    public object? VisitReturnStatement(ReturnStatementNode node)
    {
        if (node.Value != null && _currentReturnType != typeof(void))
        {
            // Function has a return type and we have a value to return
            node.Value.Accept(this);
        }
        else if (node.Value != null && _currentReturnType == typeof(void))
        {
            // Function is void but trying to return a value - ignore the value
            // TODO: This should be a compile error
        }
        
        _currentIl!.Emit(OpCodes.Ret);
        return null;
    }

    public object? VisitReturn(ReturnStatementNode node)
    {
        return VisitReturnStatement(node);
    }

    public object? VisitIf(IfStatementNode node)
    {
        // Generate condition
        node.Condition.Accept(this);
        
        // Unbox the boolean result for branching
        _currentIl!.Emit(OpCodes.Unbox_Any, typeof(bool));
        
        // Create labels for branching
        var elseLabel = _currentIl.DefineLabel();
        var endLabel = _currentIl.DefineLabel();
        
        // Branch to else if condition is false
        _currentIl.Emit(OpCodes.Brfalse, elseLabel);
        
        // Generate then block
        node.ThenStatement.Accept(this);
        _currentIl.Emit(OpCodes.Br, endLabel);
        
        // Generate else block
        _currentIl.MarkLabel(elseLabel);
        if (node.ElseStatement != null)
        {
            node.ElseStatement.Accept(this);
        }
        
        _currentIl.MarkLabel(endLabel);
        return null;
    }

    public object? VisitWhile(WhileStatementNode node)
    {
        // Create labels for loop
        var loopStart = _currentIl!.DefineLabel();
        var loopEnd = _currentIl.DefineLabel();
        
        // Mark loop start
        _currentIl.MarkLabel(loopStart);
        
        // Generate condition
        node.Condition.Accept(this);
        
        // Unbox the boolean result for branching
        _currentIl.Emit(OpCodes.Unbox_Any, typeof(bool));
        
        // Branch to end if condition is false
        _currentIl.Emit(OpCodes.Brfalse, loopEnd);
        
        // Generate body
        node.Body.Accept(this);
        
        // Jump back to start
        _currentIl.Emit(OpCodes.Br, loopStart);
        
        // Mark loop end
        _currentIl.MarkLabel(loopEnd);
        return null;
    }

    public object? VisitFor(ForStatementNode node)
    {
        // ForStatementNode is for foreach-style loops (for variable in iterable)
        // Generate the iteration logic
        
        // Get the iterable
        node.Iterable.Accept(this);
        
        // TODO: Implement proper foreach iteration
        // For now, just a simple loop placeholder
        var loopStart = _currentIl!.DefineLabel();
        var loopEnd = _currentIl.DefineLabel();
        
        _currentIl.MarkLabel(loopStart);
        
        // Generate body
        node.Body.Accept(this);
        
        // Jump back to start (simplified)
        _currentIl.Emit(OpCodes.Br, loopStart);
        
        _currentIl.MarkLabel(loopEnd);
        return null;
    }

    public object? VisitIfStatement(IfStatementNode node)
    {
        var elseLabel = _currentIl!.DefineLabel();
        var endLabel = _currentIl.DefineLabel();

        // Compile condition
        node.Condition.Accept(this);
        _currentIl.Emit(OpCodes.Brfalse, elseLabel);

        // Compile then statement
        node.ThenStatement.Accept(this);
        _currentIl.Emit(OpCodes.Br, endLabel);

        // Else clause
        _currentIl.MarkLabel(elseLabel);
        if (node.ElseStatement != null)
        {
            node.ElseStatement.Accept(this);
        }

        _currentIl.MarkLabel(endLabel);
        return null;
    }

    public object? VisitWhileStatement(WhileStatementNode node)
    {
        var startLabel = _currentIl!.DefineLabel();
        var endLabel = _currentIl.DefineLabel();

        _currentIl.MarkLabel(startLabel);

        // Compile condition
        node.Condition.Accept(this);
        _currentIl.Emit(OpCodes.Brfalse, endLabel);

        // Compile body
        node.Body.Accept(this);
        _currentIl.Emit(OpCodes.Br, startLabel);

        _currentIl.MarkLabel(endLabel);
        return null;
    }

    public object? VisitForStatement(ForStatementNode node)
    {
        // For now, implement as a simple while loop
        // TODO: Implement proper iteration over arrays/collections
        var startLabel = _currentIl!.DefineLabel();
        var endLabel = _currentIl.DefineLabel();

        // Compile iterable
        node.Iterable.Accept(this);
        var iteratorLocal = _currentIl.DeclareLocal(typeof(object));
        _currentIl.Emit(OpCodes.Stloc, iteratorLocal);

        _currentIl.MarkLabel(startLabel);

        // Simple condition (needs proper iterator implementation)
        _currentIl.Emit(OpCodes.Ldc_I4_0);
        _currentIl.Emit(OpCodes.Brfalse, endLabel);

        // Compile body
        node.Body.Accept(this);
        _currentIl.Emit(OpCodes.Br, startLabel);

        _currentIl.MarkLabel(endLabel);
        return null;
    }

    public object? VisitBlockStatement(BlockStatementNode node)
    {
        // Create new scope
        var blockScope = new SymbolTable(_scopes.Peek());
        _scopes.Push(blockScope);

        foreach (var statement in node.Statements)
        {
            statement.Accept(this);
        }

        _scopes.Pop();
        return null;
    }

    public object? VisitBlock(BlockStatementNode node)
    {
        return VisitBlockStatement(node);
    }

    public object? VisitExpression(ExpressionStatementNode node)
    {
        return VisitExpressionStatement(node);
    }

    public object? VisitExpressionStatement(ExpressionStatementNode node)
    {
        node.Expression.Accept(this);
        // Pop the result since it's not used in a statement context
        _currentIl!.Emit(OpCodes.Pop);
        return null;
    }

    public object? VisitParameter(ParameterNode node)
    {
        // Parameters are handled by the function declaration
        return null;
    }

    public object? VisitImport(ImportStatementNode node)
    {
        return VisitImportStatement(node);
    }

    public object? VisitImportStatement(ImportStatementNode node)
    {
        // For now, just record the import in the symbol table
        var symbol = new ImportSymbol(node.Alias, node.ModulePath);
        _scopes.Peek().TryDefine(symbol);
        return null;
    }

    public object? VisitBinaryExpression(BinaryExpressionNode node)
    {
        // For arithmetic operations on boxed objects, we need to unbox first
        switch (node.Operator)
        {
            case BinaryOperator.Add:
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
                    case BinaryOperator.Add:
                        _currentIl.Emit(OpCodes.Add);
                        break;
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
                
            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual:
            case BinaryOperator.LessThan:
            case BinaryOperator.LessThanOrEqual:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.GreaterThanOrEqual:
                // For comparison operations - we need to handle mixed types
                node.Left.Accept(this);
                EmitUnboxToInt(_currentIl!);
                
                node.Right.Accept(this);
                EmitUnboxToInt(_currentIl);
                
                switch (node.Operator)
                {
                    case BinaryOperator.Equal:
                        _currentIl.Emit(OpCodes.Ceq);
                        break;
                    case BinaryOperator.NotEqual:
                        _currentIl.Emit(OpCodes.Ceq);
                        _currentIl.Emit(OpCodes.Ldc_I4_0);
                        _currentIl.Emit(OpCodes.Ceq);
                        break;
                    case BinaryOperator.LessThan:
                        _currentIl.Emit(OpCodes.Clt);
                        break;
                    case BinaryOperator.LessThanOrEqual:
                        _currentIl.Emit(OpCodes.Cgt);
                        _currentIl.Emit(OpCodes.Ldc_I4_0);
                        _currentIl.Emit(OpCodes.Ceq);
                        break;
                    case BinaryOperator.GreaterThan:
                        _currentIl.Emit(OpCodes.Cgt);
                        break;
                    case BinaryOperator.GreaterThanOrEqual:
                        _currentIl.Emit(OpCodes.Clt);
                        _currentIl.Emit(OpCodes.Ldc_I4_0);
                        _currentIl.Emit(OpCodes.Ceq);
                        break;
                }
                
                _currentIl.Emit(OpCodes.Box, typeof(bool));
                break;
                
            case BinaryOperator.And:
            case BinaryOperator.Or:
                // Logical operations - both operands should be boolean
                node.Left.Accept(this);
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(bool));
                
                node.Right.Accept(this);
                _currentIl.Emit(OpCodes.Unbox_Any, typeof(bool));
                
                switch (node.Operator)
                {
                    case BinaryOperator.And:
                        _currentIl.Emit(OpCodes.And);
                        break;
                    case BinaryOperator.Or:
                        _currentIl.Emit(OpCodes.Or);
                        break;
                }
                
                _currentIl.Emit(OpCodes.Box, typeof(bool));
                break;
                
            default:
                // For other operators, use the old approach for now
                node.Left.Accept(this);
                node.Right.Accept(this);
                break;
        }

        return null;
    }
    
    private bool IsLiteralBooleanExpression(AstNode node)
    {
        if (node is LiteralNode literal)
        {
            return literal.Type == LiteralType.Boolean;
        }
        // Also check for boolean variables and expressions that return boolean
        if (node is IdentifierNode identifier)
        {
            // For now, assume any identifier could be boolean
            // In the future, we could do type inference here
            return false; // Keep conservative approach for variables
        }
        if (node is BinaryExpressionNode binaryExpr)
        {
            // Comparison operators return boolean
            switch (binaryExpr.Operator)
            {
                case BinaryOperator.Equal:
                case BinaryOperator.NotEqual:
                case BinaryOperator.LessThan:
                case BinaryOperator.LessThanOrEqual:
                case BinaryOperator.GreaterThan:
                case BinaryOperator.GreaterThanOrEqual:
                case BinaryOperator.And:
                case BinaryOperator.Or:
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Emits IL to unbox an object to int, with proper handling for boolean-to-int conversion
    /// </summary>
    private void EmitUnboxToInt(ILGenerator il)
    {
        // Check if the object is an int first
        var isIntLabel = il.DefineLabel();
        var endLabel = il.DefineLabel();
        
        // Duplicate value to test type
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Isinst, typeof(int));
        il.Emit(OpCodes.Brtrue, isIntLabel);
        
        // Not an int, try boolean and convert to int
        il.Emit(OpCodes.Unbox_Any, typeof(bool));
        il.Emit(OpCodes.Conv_I4); // Convert boolean to int (false=0, true=1)
        il.Emit(OpCodes.Br, endLabel);
        
        // Is an int, just unbox
        il.MarkLabel(isIntLabel);
        il.Emit(OpCodes.Unbox_Any, typeof(int));
        
        il.MarkLabel(endLabel);
    }

    // Runtime helper methods for type-safe comparisons
    public static bool CompareEqual(object left, object right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        
        // Handle same-type comparisons
        if (left.GetType() == right.GetType())
        {
            return left.Equals(right);
        }
        
        // Handle numeric conversions
        if (IsNumeric(left) && IsNumeric(right))
        {
            return Convert.ToDouble(left) == Convert.ToDouble(right);
        }
        
        return false;
    }
    
    public static bool CompareLessThan(object left, object right)
    {
        if (left == null || right == null) return false;
        
        if (IsNumeric(left) && IsNumeric(right))
        {
            return Convert.ToDouble(left) < Convert.ToDouble(right);
        }
        
        return false;
    }
    
    public static bool CompareGreaterThan(object left, object right)
    {
        if (left == null || right == null) return false;
        
        if (IsNumeric(left) && IsNumeric(right))
        {
            return Convert.ToDouble(left) > Convert.ToDouble(right);
        }
        
        return false;
    }
    
    private static bool IsNumeric(object value)
    {
        return value is int || value is double || value is float || value is long || value is short;
    }

    public object? VisitUnaryExpression(UnaryExpressionNode node)
    {
        switch (node.Operator)
        {
            case UnaryOperator.Minus:
                node.Operand.Accept(this);
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(int));
                _currentIl.Emit(OpCodes.Neg);
                _currentIl.Emit(OpCodes.Box, typeof(int));
                break;
            case UnaryOperator.Plus:
                // Unary plus is essentially a no-op, just evaluate the operand
                node.Operand.Accept(this);
                break;
            case UnaryOperator.Not:
                node.Operand.Accept(this);
                _currentIl!.Emit(OpCodes.Unbox_Any, typeof(bool));
                _currentIl.Emit(OpCodes.Ldc_I4_0);
                _currentIl.Emit(OpCodes.Ceq);
                _currentIl.Emit(OpCodes.Box, typeof(bool));
                break;
        }

        return null;
    }

    public object? VisitCallExpression(CallExpressionNode node)
    {
        // Compile arguments
        foreach (var arg in node.Arguments)
        {
            arg.Accept(this);
        }

        // For now, handle simple method calls
        if (node.Callee is IdentifierNode identifier)
        {
            if (_methods.TryGetValue(identifier.Name, out var method))
            {
                _currentIl!.Emit(OpCodes.Call, method);
            }
            else
            {
                // Handle built-in functions or external calls
                HandleBuiltinCall(identifier.Name, node.Arguments.Count);
            }
        }

        return null;
    }

    public object? VisitMemberAccess(MemberAccessNode node)
    {
        // Generate IL for member access
        node.Object.Accept(this);
        
        // Handle property/field access
        var memberInfo = GetMemberInfo(GetExpressionType(node.Object), node.Property);
        if (memberInfo is FieldInfo field)
        {
            _currentIl!.Emit(OpCodes.Ldfld, field);
        }
        else if (memberInfo is PropertyInfo property)
        {
            _currentIl!.Emit(OpCodes.Callvirt, property.GetMethod!);
        }
        
        return null;
    }

    public object? VisitAssignmentExpression(AssignmentExpressionNode node)
    {
        // Evaluate the right side first
        node.Right.Accept(this);
        
        // Handle assignment to variable
        if (node.Left is IdentifierNode identifier)
        {
            // Only assign to existing variables (local or global)
            if (_currentLocals.TryGetValue(identifier.Name, out var local))
            {
                _currentIl!.Emit(OpCodes.Dup);
                _currentIl.Emit(OpCodes.Stloc, local);
            }
            else if (_globalFields.TryGetValue(identifier.Name, out var field))
            {
                _currentIl!.Emit(OpCodes.Dup);
                _currentIl.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                // Variable does not exist, error
                throw new Exception($"Variable '{identifier.Name}' not declared. Use 'var' keyword to declare new variables.");
            }
        }
        else
        {
            // TODO: Handle other assignment targets (member access, index access)
            throw new NotImplementedException("Assignment to non-identifier expressions not yet implemented");
        }
        
        return null;
    }

    public object? VisitIndexAccess(IndexAccessNode node)
    {
        // Generate IL for index access (array/collection indexing)
        node.Object.Accept(this);
        node.Index.Accept(this);
        
        // Emit array load instruction
        _currentIl!.Emit(OpCodes.Ldelem_Ref);
        
        return null;
    }

    public object? VisitFunctionCall(FunctionCallNode node)
    {
        // Compile arguments
        foreach (var arg in node.Arguments)
        {
            arg.Accept(this);
        }
        
        // Get method info and emit call
        var methodInfo = GetMethodInfo(node.FunctionName, node.Arguments.Count);
        if (methodInfo != null)
        {
            _currentIl!.Emit(OpCodes.Call, methodInfo);
        }
        
        return null;
    }

    public object? VisitIdentifier(IdentifierNode node)
    {
        // Check for local variables first
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

        return null;
    }

    public object? VisitLiteral(LiteralNode node)
    {
        switch (node.Type)
        {
            case LiteralType.Number:
                if (node.Value is int intValue)
                {
                    _currentIl!.Emit(OpCodes.Ldc_I4, intValue);
                    _currentIl.Emit(OpCodes.Box, typeof(int));
                }
                else if (node.Value is double doubleValue)
                {
                    _currentIl!.Emit(OpCodes.Ldc_R8, doubleValue);
                    _currentIl.Emit(OpCodes.Box, typeof(double));
                }
                break;
            case LiteralType.String:
                _currentIl!.Emit(OpCodes.Ldstr, (string)node.Value!);
                break;
            case LiteralType.Boolean:
                _currentIl!.Emit((bool)node.Value! ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                _currentIl.Emit(OpCodes.Box, typeof(bool));
                break;
            case LiteralType.Null:
                _currentIl!.Emit(OpCodes.Ldnull);
                break;
        }
        return null;
    }

    private Type GetSystemType(CxType cxType)
    {
        return cxType.Name switch
        {
            "string" => typeof(string),
            "number" => typeof(double),
            "boolean" => typeof(bool),
            "void" => typeof(void),
            _ when cxType is ArrayType => typeof(object[]),
            _ => typeof(object)
        };
    }

    private void HandleBuiltinCall(string functionName, int argCount)
    {
        switch (functionName)
        {
            case "print":
                // Use the object overload of WriteLine which handles null properly
                var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });
                _currentIl!.Emit(OpCodes.Call, writeLineMethod!);
                // Console.WriteLine is void, so push a dummy value for consistency
                _currentIl.Emit(OpCodes.Ldnull);
                break;
            default:
                // Unknown function - just pop arguments and push null
                for (int i = 0; i < argCount; i++)
                {
                    _currentIl!.Emit(OpCodes.Pop);
                }
                _currentIl.Emit(OpCodes.Ldnull);
                break;
        }
    }

    private MemberInfo? GetMemberInfo(CxType objectType, string memberName)
    {
        var systemType = GetSystemType(objectType);
        return systemType.GetMember(memberName).FirstOrDefault();
    }

    private int GetLocalVariableIndex(string variableName)
    {
        // For now, return 0 - this should lookup the variable in local scope
        // TODO: Implement proper local variable management
        return 0;
    }

    private MethodInfo? GetMethodInfo(string functionName, int argumentCount)
    {
        // For now, return null - this should lookup the method by name and signature
        // TODO: Implement proper method resolution
        return null;
    }

    private CxType GetExpressionType(ExpressionNode expression)
    {
        // For now, return a basic type - this should be enhanced with proper type inference
        // TODO: Implement proper type system
        return new PrimitiveType("object");
    }

    /// <summary>
    /// Initialize AI runtime fields for the compiled program
    /// </summary>
    private void InitializeAIRuntime()
    {
        // Create static fields for AI runtime components
        _agenticRuntimeField = _typeBuilder.DefineField(
            "_agenticRuntime",
            typeof(IAgenticRuntime),
            FieldAttributes.Private | FieldAttributes.Static);

        _multiModalAIField = _typeBuilder.DefineField(
            "_multiModalAI",
            typeof(IMultiModalAI),
            FieldAttributes.Private | FieldAttributes.Static);

        _codeSynthesizerField = _typeBuilder.DefineField(
            "_codeSynthesizer",
            typeof(ICodeSynthesizer),
            FieldAttributes.Private | FieldAttributes.Static);
    }

    /// <summary>
    /// AI task planning visitor
    /// </summary>
    public object? VisitAITask(AITaskNode node)
    {
        try
        {
            _logger?.LogInformation("Compiling AI task: {Goal}", node.Goal);

            // Load the agentic runtime
            _currentIl!.Emit(OpCodes.Ldsfld, _agenticRuntimeField!);

            // Load the goal string
            _currentIl.Emit(OpCodes.Ldstr, node.Goal);

            // Create options object (simplified for now)
            _currentIl.Emit(OpCodes.Ldnull); // TaskPlanningOptions

            // Call PlanTaskAsync
            var planTaskMethod = typeof(IAgenticRuntime).GetMethod("PlanTaskAsync");
            _currentIl.Emit(OpCodes.Callvirt, planTaskMethod!);

            // Store result if assigned to a variable
            if (!string.IsNullOrEmpty(node.AssignTo))
            {
                if (_currentLocals.TryGetValue(node.AssignTo, out var local))
                {
                    _currentIl.Emit(OpCodes.Stloc, local);
                }
                else if (_globalFields.TryGetValue(node.AssignTo, out var field))
                {
                    _currentIl.Emit(OpCodes.Stsfld, field);
                }
            }
            else
            {
                _currentIl.Emit(OpCodes.Pop); // Discard result if not assigned
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error compiling AI task");
            throw;
        }
    }

    /// <summary>
    /// AI code synthesis visitor
    /// </summary>
    public object? VisitAISynthesize(AISynthesizeNode node)
    {
        try
        {
            _logger?.LogInformation("Compiling AI synthesize: {Spec}", node.Specification);

            // Load the code synthesizer
            _currentIl!.Emit(OpCodes.Ldsfld, _codeSynthesizerField!);

            // Load the specification string
            _currentIl.Emit(OpCodes.Ldstr, node.Specification);

            // Create options object (simplified for now)
            _currentIl.Emit(OpCodes.Ldnull); // CodeSynthesisOptions

            // Call SynthesizeFunctionAsync
            var synthesizeMethod = typeof(ICodeSynthesizer).GetMethod("SynthesizeFunctionAsync");
            _currentIl.Emit(OpCodes.Callvirt, synthesizeMethod!);

            // Store result if assigned to a variable
            if (!string.IsNullOrEmpty(node.AssignTo))
            {
                if (_currentLocals.TryGetValue(node.AssignTo, out var local))
                {
                    _currentIl.Emit(OpCodes.Stloc, local);
                }
                else if (_globalFields.TryGetValue(node.AssignTo, out var field))
                {
                    _currentIl.Emit(OpCodes.Stsfld, field);
                }
            }
            else
            {
                _currentIl.Emit(OpCodes.Pop); // Discard result if not assigned
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error compiling AI synthesize");
            throw;
        }
    }

    /// <summary>
    /// AI function call visitor
    /// </summary>
    public object? VisitAICall(AICallNode node)
    {
        try
        {
            _logger?.LogInformation("Compiling AI call: {Function}", node.FunctionName);

            // Load the agentic runtime
            _currentIl!.Emit(OpCodes.Ldsfld, _agenticRuntimeField!);

            // Load function name
            _currentIl.Emit(OpCodes.Ldstr, node.FunctionName);

            // Create array for parameters
            _currentIl.Emit(OpCodes.Ldc_I4, node.Arguments.Count);
            _currentIl.Emit(OpCodes.Newarr, typeof(object));

            // Fill array with arguments
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                _currentIl.Emit(OpCodes.Dup); // Duplicate array reference
                _currentIl.Emit(OpCodes.Ldc_I4, i); // Array index
                node.Arguments[i].Accept(this); // Evaluate argument
                _currentIl.Emit(OpCodes.Stelem_Ref); // Store in array
            }

            // Create options object (simplified for now)
            _currentIl.Emit(OpCodes.Ldnull); // AIInvocationOptions

            // Call InvokeAIFunctionAsync
            var invokeMethod = typeof(IAgenticRuntime).GetMethod("InvokeAIFunctionAsync");
            _currentIl.Emit(OpCodes.Callvirt, invokeMethod!);

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error compiling AI call");
            throw;
        }
    }

    /// <summary>
    /// AI reasoning loop visitor
    /// </summary>
    public object? VisitAIReason(AIReasonNode node)
    {
        try
        {
            _logger?.LogInformation("Compiling AI reason: {Goal}", node.Goal);

            // For now, emit a placeholder that calls the agentic runtime
            // In a full implementation, this would create a reasoning loop
            _currentIl!.Emit(OpCodes.Ldstr, $"Reasoning about: {node.Goal}");
            
            // Store result if assigned to a variable
            if (!string.IsNullOrEmpty(node.AssignTo))
            {
                if (_currentLocals.TryGetValue(node.AssignTo, out var local))
                {
                    _currentIl.Emit(OpCodes.Stloc, local);
                }
                else if (_globalFields.TryGetValue(node.AssignTo, out var field))
                {
                    _currentIl.Emit(OpCodes.Stsfld, field);
                }
            }
            else
            {
                _currentIl.Emit(OpCodes.Pop); // Discard result if not assigned
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error compiling AI reason");
            throw;
        }
    }

    /// <summary>
    /// AI multi-modal processing visitor
    /// </summary>
    public object? VisitAIProcess(AIProcessNode node)
    {
        try
        {
            _logger?.LogInformation("Compiling AI process: {InputType}", node.InputType);

            // Load the multi-modal AI service
            _currentIl!.Emit(OpCodes.Ldsfld, _multiModalAIField!);

            // Process based on input type
            switch (node.InputType.ToLower())
            {
                case "text":
                    node.Input.Accept(this); // Evaluate input expression
                    _currentIl.Emit(OpCodes.Ldnull); // MultiModalOptions
                    var processTextMethod = typeof(IMultiModalAI).GetMethod("ProcessTextAsync");
                    _currentIl.Emit(OpCodes.Callvirt, processTextMethod!);
                    break;
                case "image":
                    node.Input.Accept(this); // Evaluate input expression (should be byte[])
                    _currentIl.Emit(OpCodes.Ldnull); // description
                    _currentIl.Emit(OpCodes.Ldnull); // MultiModalOptions
                    var processImageMethod = typeof(IMultiModalAI).GetMethod("ProcessImageAsync");
                    _currentIl.Emit(OpCodes.Callvirt, processImageMethod!);
                    break;
                default:
                    // Default to text processing
                    node.Input.Accept(this);
                    _currentIl.Emit(OpCodes.Ldnull);
                    var defaultMethod = typeof(IMultiModalAI).GetMethod("ProcessTextAsync");
                    _currentIl.Emit(OpCodes.Callvirt, defaultMethod!);
                    break;
            }

            // Store result if assigned to a variable
            if (!string.IsNullOrEmpty(node.AssignTo))
            {
                if (_currentLocals.TryGetValue(node.AssignTo, out var local))
                {
                    _currentIl.Emit(OpCodes.Stloc, local);
                }
                else if (_globalFields.TryGetValue(node.AssignTo, out var field))
                {
                    _currentIl.Emit(OpCodes.Stsfld, field);
                }
            }
            else
            {
                _currentIl.Emit(OpCodes.Pop); // Discard result if not assigned
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error compiling AI process");
            throw;
        }
    }

    /// <summary>
    /// AI embedding generation visitor
    /// </summary>
    public object? VisitAIEmbed(AIEmbedNode node)
    {
        try
        {
            _logger?.LogInformation("Compiling AI embed");

            // Load the multi-modal AI service
            _currentIl!.Emit(OpCodes.Ldsfld, _multiModalAIField!);

            // Load text input
            node.Text.Accept(this);

            // Create options object (simplified for now)
            _currentIl.Emit(OpCodes.Ldnull); // EmbeddingOptions

            // Call GenerateEmbeddingsAsync
            var embedMethod = typeof(IMultiModalAI).GetMethod("GenerateEmbeddingsAsync");
            _currentIl.Emit(OpCodes.Callvirt, embedMethod!);

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error compiling AI embed");
            throw;
        }
    }

    /// <summary>
    /// AI adaptive code path visitor
    /// </summary>
    public object? VisitAIAdapt(AIAdaptNode node)
    {
        try
        {
            _logger?.LogInformation("Compiling AI adapt: {CodePath}", node.CodePath);

            // Load the code synthesizer
            _currentIl!.Emit(OpCodes.Ldsfld, _codeSynthesizerField!);

            // Load code path
            _currentIl.Emit(OpCodes.Ldstr, node.CodePath);

            // Load context
            node.Context.Accept(this);

            // Create options object (simplified for now)
            _currentIl.Emit(OpCodes.Ldnull); // AdaptationOptions

            // Call AdaptCodePathAsync
            var adaptMethod = typeof(ICodeSynthesizer).GetMethod("AdaptCodePathAsync");
            _currentIl.Emit(OpCodes.Callvirt, adaptMethod!);

            // Result is bool, box it for consistency
            _currentIl.Emit(OpCodes.Box, typeof(bool));

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error compiling AI adapt");
            throw;
        }
    }

    // Add logger field for AI operations
    private ILogger? _logger;
    
    /// <summary>
    /// Set logger for AI operations
    /// </summary>
    public void SetLogger(ILogger logger)
    {
        _logger = logger;
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
