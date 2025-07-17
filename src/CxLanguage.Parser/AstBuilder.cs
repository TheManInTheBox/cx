using CxLanguage.Core.Ast;
using CxLanguage.Core.Types;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static CxParser;

namespace CxLanguage.Parser;

/// <summary>
/// Converts ANTLR parse tree to Cx AST
/// </summary>
public class AstBuilder : CxBaseVisitor<AstNode>
{
    private readonly string? _fileName;

    public AstBuilder(string? fileName = null)
    {
        _fileName = fileName;
    }

    public AstNode BuildAst(IParseTree tree)
    {
        return Visit(tree);
    }

    public override AstNode VisitProgram(ProgramContext context)
    {
        var program = new ProgramNode();
        SetLocation(program, context);

        foreach (var stmtContext in context.statement())
        {
            var statement = (StatementNode)Visit(stmtContext);
            if (statement != null)
            {
                program.Statements.Add(statement);
            }
        }

        return program;
    }

    public override AstNode VisitStatement(StatementContext context)
    {
        // Get the first child of the statement (the actual statement type)
        if (context.ChildCount > 0)
        {
            var child = context.GetChild(0);
            var result = Visit(child);
            return result;
        }
        
        throw new InvalidOperationException("Statement context has no children");
    }

    public override AstNode VisitVariableDeclaration(VariableDeclarationContext context)
    {
        var varDecl = new VariableDeclarationNode();
        SetLocation(varDecl, context);

        varDecl.Name = context.IDENTIFIER().GetText();
        varDecl.Initializer = (ExpressionNode)Visit(context.expression());

        return varDecl;
    }

    public override AstNode VisitFunctionDeclaration(FunctionDeclarationContext context)
    {
        var funcDecl = new FunctionDeclarationNode();
        SetLocation(funcDecl, context);

        funcDecl.Name = context.IDENTIFIER().GetText();
        funcDecl.IsAsync = context.GetText().StartsWith("async");
        
        // Parameters
        if (context.parameterList() != null)
        {
            foreach (var paramContext in context.parameterList().parameter())
            {
                var param = new ParameterNode
                {
                    Name = paramContext.IDENTIFIER().GetText(),
                    Type = paramContext.type() != null ? ParseType(paramContext.type()) : CxType.Any
                };
                SetLocation(param, paramContext);
                funcDecl.Parameters.Add(param);
            }
        }

        // Return type
        if (context.type() != null)
        {
            funcDecl.ReturnType = ParseType(context.type());
        }

        // Body
        funcDecl.Body = (BlockStatementNode)Visit(context.blockStatement());

        return funcDecl;
    }

    public override AstNode VisitImportStatement(ImportStatementContext context)
    {
        var importStmt = new ImportStatementNode();
        SetLocation(importStmt, context);

        importStmt.Alias = context.IDENTIFIER().GetText();
        importStmt.ModulePath = context.STRING_LITERAL().GetText().Trim('"');

        return importStmt;
    }

    public override AstNode VisitBlockStatement(BlockStatementContext context)
    {
        var block = new BlockStatementNode();
        SetLocation(block, context);

        foreach (var stmtContext in context.statement())
        {
            var statement = (StatementNode)Visit(stmtContext);
            block.Statements.Add(statement);
        }

        return block;
    }

    public override AstNode VisitExpressionStatement(ExpressionStatementContext context)
    {
        var exprStmt = new ExpressionStatementNode();
        SetLocation(exprStmt, context);

        exprStmt.Expression = (ExpressionNode)Visit(context.expression());

        return exprStmt;
    }

    public override AstNode VisitReturnStatement(ReturnStatementContext context)
    {
        var returnStmt = new ReturnStatementNode();
        SetLocation(returnStmt, context);

        if (context.expression() != null)
        {
            returnStmt.Value = (ExpressionNode)Visit(context.expression());
        }

        return returnStmt;
    }

    public override AstNode VisitIfStatement(IfStatementContext context)
    {
        var ifStmt = new IfStatementNode();
        SetLocation(ifStmt, context);

        ifStmt.Condition = (ExpressionNode)Visit(context.expression());
        ifStmt.ThenStatement = (StatementNode)Visit(context.statement(0));
        
        if (context.statement().Length > 1)
        {
            ifStmt.ElseStatement = (StatementNode)Visit(context.statement(1));
        }

        return ifStmt;
    }

    public override AstNode VisitWhileStatement(WhileStatementContext context)
    {
        var whileStmt = new WhileStatementNode();
        SetLocation(whileStmt, context);

        whileStmt.Condition = (ExpressionNode)Visit(context.expression());
        whileStmt.Body = (StatementNode)Visit(context.statement());

        return whileStmt;
    }

    public override AstNode VisitForStatement(ForStatementContext context)
    {
        var forStmt = new ForStatementNode();
        SetLocation(forStmt, context);

        forStmt.Variable = context.IDENTIFIER().GetText();
        forStmt.Iterable = (ExpressionNode)Visit(context.expression());
        forStmt.Body = (StatementNode)Visit(context.statement());

        return forStmt;
    }

    // Expression visitors
    public override AstNode VisitPrimaryExpression(PrimaryExpressionContext context)
    {
        return Visit(context.primary());
    }

    public override AstNode VisitPrimary(PrimaryContext context)
    {
        if (context.IDENTIFIER() != null)
        {
            var identifier = new IdentifierNode();
            SetLocation(identifier, context);
            identifier.Name = context.IDENTIFIER().GetText();
            return identifier;
        }
        else if (context.STRING_LITERAL() != null)
        {
            var literal = new LiteralNode();
            SetLocation(literal, context);
            literal.Value = context.STRING_LITERAL().GetText().Trim('"');
            literal.Type = LiteralType.String;
            return literal;
        }
        else if (context.NUMBER_LITERAL() != null)
        {
            var literal = new LiteralNode();
            SetLocation(literal, context);
            var text = context.NUMBER_LITERAL().GetText();
            
            if (text.Contains('.'))
            {
                literal.Value = double.Parse(text);
            }
            else
            {
                literal.Value = int.Parse(text);
            }
            literal.Type = LiteralType.Number;
            return literal;
        }
        else if (context.BOOLEAN_LITERAL() != null)
        {
            var literal = new LiteralNode();
            SetLocation(literal, context);
            literal.Value = bool.Parse(context.BOOLEAN_LITERAL().GetText());
            literal.Type = LiteralType.Boolean;
            return literal;
        }
        else if (context.NULL() != null)
        {
            var literal = new LiteralNode();
            SetLocation(literal, context);
            literal.Value = null;
            literal.Type = LiteralType.Null;
            return literal;
        }
        else if (context.expression() != null)
        {
            return Visit(context.expression());
        }
        else if (context.aiFunction() != null)
        {
            return Visit(context.aiFunction());
        }

        throw new InvalidOperationException($"Unknown primary expression: {context.GetText()}");
    }

    public override AstNode VisitMemberAccess(MemberAccessContext context)
    {
        var memberAccess = new MemberAccessNode();
        SetLocation(memberAccess, context);

        memberAccess.Object = (ExpressionNode)Visit(context.expression());
        memberAccess.Property = context.IDENTIFIER().GetText();

        return memberAccess;
    }

    public override AstNode VisitFunctionCall(FunctionCallContext context)
    {
        var funcCall = new CallExpressionNode();
        SetLocation(funcCall, context);

        funcCall.Callee = (ExpressionNode)Visit(context.expression());

        if (context.argumentList() != null)
        {
            foreach (var argContext in context.argumentList().expression())
            {
                var argument = (ExpressionNode)Visit(argContext);
                funcCall.Arguments.Add(argument);
            }
        }

        return funcCall;
    }

    public override AstNode VisitIndexAccess(IndexAccessContext context)
    {
        var indexAccess = new IndexAccessNode();
        SetLocation(indexAccess, context);

        indexAccess.Object = (ExpressionNode)Visit(context.expression(0));
        indexAccess.Index = (ExpressionNode)Visit(context.expression(1));

        return indexAccess;
    }

    public override AstNode VisitAwaitExpression(AwaitExpressionContext context)
    {
        // For now, just return the inner expression
        return Visit(context.expression());
    }

    public override AstNode VisitParallelExpression(ParallelExpressionContext context)
    {
        // For now, just return the inner expression  
        return Visit(context.expression());
    }

    public override AstNode VisitAdditiveExpression(AdditiveExpressionContext context)
    {
        var binaryExpr = new BinaryExpressionNode();
        SetLocation(binaryExpr, context);

        binaryExpr.Left = (ExpressionNode)Visit(context.expression(0));
        binaryExpr.Right = (ExpressionNode)Visit(context.expression(1));
        
        var operatorText = context.children[1].GetText();
        binaryExpr.Operator = operatorText switch
        {
            "+" => BinaryOperator.Add,
            "-" => BinaryOperator.Subtract,
            _ => throw new InvalidOperationException($"Unknown additive operator: {operatorText}")
        };

        return binaryExpr;
    }

    public override AstNode VisitMultiplicativeExpression(MultiplicativeExpressionContext context)
    {
        var binaryExpr = new BinaryExpressionNode();
        SetLocation(binaryExpr, context);

        binaryExpr.Left = (ExpressionNode)Visit(context.expression(0));
        binaryExpr.Right = (ExpressionNode)Visit(context.expression(1));
        
        var operatorText = context.children[1].GetText();
        binaryExpr.Operator = operatorText switch
        {
            "*" => BinaryOperator.Multiply,
            "/" => BinaryOperator.Divide,
            "%" => BinaryOperator.Modulo,
            _ => throw new InvalidOperationException($"Unknown multiplicative operator: {operatorText}")
        };

        return binaryExpr;
    }

    public override AstNode VisitRelationalExpression(RelationalExpressionContext context)
    {
        var binaryExpr = new BinaryExpressionNode();
        SetLocation(binaryExpr, context);

        binaryExpr.Left = (ExpressionNode)Visit(context.expression(0));
        binaryExpr.Right = (ExpressionNode)Visit(context.expression(1));
        
        var operatorText = context.children[1].GetText();
        binaryExpr.Operator = operatorText switch
        {
            "<" => BinaryOperator.LessThan,
            ">" => BinaryOperator.GreaterThan,
            "<=" => BinaryOperator.LessThanOrEqual,
            ">=" => BinaryOperator.GreaterThanOrEqual,
            "==" => BinaryOperator.Equal,
            "!=" => BinaryOperator.NotEqual,
            _ => throw new InvalidOperationException($"Unknown relational operator: {operatorText}")
        };

        return binaryExpr;
    }

    public override AstNode VisitLogicalExpression(LogicalExpressionContext context)
    {
        var binaryExpr = new BinaryExpressionNode();
        SetLocation(binaryExpr, context);

        binaryExpr.Left = (ExpressionNode)Visit(context.expression(0));
        binaryExpr.Right = (ExpressionNode)Visit(context.expression(1));
        
        var operatorText = context.children[1].GetText();
        binaryExpr.Operator = operatorText switch
        {
            "&&" => BinaryOperator.And,
            "||" => BinaryOperator.Or,
            _ => throw new InvalidOperationException($"Unknown logical operator: {operatorText}")
        };

        return binaryExpr;
    }

    public override AstNode VisitAssignmentExpression(AssignmentExpressionContext context)
    {
        var assignment = new AssignmentExpressionNode();
        SetLocation(assignment, context);

        assignment.Left = (ExpressionNode)Visit(context.expression(0));
        assignment.Right = (ExpressionNode)Visit(context.expression(1));

        // Determine the assignment operator
        var operatorText = context.children[1].GetText(); // The operator is always the second child
        assignment.Operator = operatorText switch
        {
            "=" => AssignmentOperator.Assign,
            "+=" => AssignmentOperator.AddAssign,
            "-=" => AssignmentOperator.SubtractAssign,
            "*=" => AssignmentOperator.MultiplyAssign,
            "/=" => AssignmentOperator.DivideAssign,
            _ => AssignmentOperator.Assign
        };

        return assignment;
    }

    public override AstNode VisitObjectLiteral(ObjectLiteralContext context)
    {
        // For now, just return a null literal - object literals not fully implemented
        var literal = new LiteralNode();
        SetLocation(literal, context);
        literal.Value = null;
        literal.Type = LiteralType.Null;
        return literal;
    }

    public override AstNode VisitArrayLiteral(ArrayLiteralContext context)
    {
        var arrayLiteral = new ArrayLiteralNode();
        SetLocation(arrayLiteral, context);
        
        // Parse argument list if present
        if (context.argumentList() != null)
        {
            var argListContext = context.argumentList();
            foreach (var exprContext in argListContext.expression())
            {
                arrayLiteral.Elements.Add((ExpressionNode)Visit(exprContext));
            }
        }
        
        return arrayLiteral;
    }

    public override AstNode VisitUnaryExpression(UnaryExpressionContext context)
    {
        var unaryExpr = new UnaryExpressionNode();
        SetLocation(unaryExpr, context);

        unaryExpr.Operand = (ExpressionNode)Visit(context.expression());
        
        var operatorText = context.children[0].GetText();
        unaryExpr.Operator = operatorText switch
        {
            "!" => UnaryOperator.Not,
            "-" => UnaryOperator.Minus,
            "+" => UnaryOperator.Plus,
            _ => throw new InvalidOperationException($"Unknown unary operator: {operatorText}")
        };

        return unaryExpr;
    }

    private CxType ParseType(TypeContext context)
    {
        var typeText = context.GetText();
        return typeText switch
        {
            "string" => CxType.String,
            "number" => CxType.Number,
            "boolean" => CxType.Boolean,
            "any" => CxType.Any,
            "object" => CxType.Object,
            _ when typeText.StartsWith("array<") => CxType.Object, // Arrays as objects for now
            _ => CxType.Any // Fallback for unknown types
        };
    }

    private void SetLocation(AstNode node, IParseTree? context)
    {
        if (context is ParserRuleContext ruleContext && _fileName != null)
        {
            node.Line = ruleContext.Start.Line;
            node.Column = ruleContext.Start.Column;
            node.SourceFile = _fileName;
        }
    }

    // AI Function visitors
    public override AstNode VisitTaskFunction(TaskFunctionContext context)
    {
        var aiCall = new AICallNode();
        SetLocation(aiCall, context);
        
        aiCall.FunctionName = "task";
        aiCall.Arguments.Add((ExpressionNode)Visit(context.expression()));
        
        // Parse options from objectPropertyList if present
        if (context.objectPropertyList() != null)
        {
            ParseAIOptions(aiCall, context.objectPropertyList());
        }
        
        return aiCall;
    }

    private void ParseAIOptions(AICallNode aiCall, ObjectPropertyListContext optionsContext)
    {
        foreach (var property in optionsContext.objectProperty())
        {
            string key;
            if (property.IDENTIFIER() != null)
            {
                key = property.IDENTIFIER().GetText();
            }
            else if (property.STRING_LITERAL() != null)
            {
                key = property.STRING_LITERAL().GetText().Trim('"');
            }
            else
            {
                continue; // Skip invalid property names
            }

            var valueExpr = (ExpressionNode)Visit(property.expression());
            
            // For now, store the expression directly
            // In a more complete implementation, we might want to evaluate literals
            if (valueExpr is LiteralNode literal)
            {
                aiCall.Options[key] = literal.Value ?? "";
            }
            else
            {
                // Store complex expressions as-is for later evaluation
                aiCall.Options[key] = valueExpr;
            }
        }
    }

    public override AstNode VisitSynthesizeFunction(SynthesizeFunctionContext context)
    {
        var aiCall = new AICallNode();
        SetLocation(aiCall, context);
        
        aiCall.FunctionName = "synthesize";
        aiCall.Arguments.Add((ExpressionNode)Visit(context.expression()));
        
        // Parse options from objectPropertyList if present
        if (context.objectPropertyList() != null)
        {
            ParseAIOptions(aiCall, context.objectPropertyList());
        }
        
        return aiCall;
    }

    public override AstNode VisitReasonFunction(ReasonFunctionContext context)
    {
        var aiCall = new AICallNode();
        SetLocation(aiCall, context);
        
        aiCall.FunctionName = "reason";
        aiCall.Arguments.Add((ExpressionNode)Visit(context.expression()));
        
        // Parse options from objectPropertyList if present
        if (context.objectPropertyList() != null)
        {
            ParseAIOptions(aiCall, context.objectPropertyList());
        }
        
        return aiCall;
    }

    public override AstNode VisitProcessFunction(ProcessFunctionContext context)
    {
        var aiCall = new AICallNode();
        SetLocation(aiCall, context);
        
        aiCall.FunctionName = "process";
        
        // ProcessFunction has two expressions
        var expressions = context.expression();
        aiCall.Arguments.Add((ExpressionNode)Visit(expressions[0]));
        aiCall.Arguments.Add((ExpressionNode)Visit(expressions[1]));
        
        // Parse options from objectPropertyList if present
        if (context.objectPropertyList() != null)
        {
            ParseAIOptions(aiCall, context.objectPropertyList());
        }
        
        return aiCall;
    }

    public override AstNode VisitGenerateFunction(GenerateFunctionContext context)
    {
        var aiCall = new AICallNode();
        SetLocation(aiCall, context);
        
        aiCall.FunctionName = "generate";
        aiCall.Arguments.Add((ExpressionNode)Visit(context.expression()));
        
        // Parse options from objectPropertyList if present
        if (context.objectPropertyList() != null)
        {
            ParseAIOptions(aiCall, context.objectPropertyList());
        }
        
        return aiCall;
    }

    public override AstNode VisitEmbedFunction(EmbedFunctionContext context)
    {
        var aiCall = new AICallNode();
        SetLocation(aiCall, context);
        
        aiCall.FunctionName = "embed";
        aiCall.Arguments.Add((ExpressionNode)Visit(context.expression()));
        
        // Parse options from objectPropertyList if present
        if (context.objectPropertyList() != null)
        {
            ParseAIOptions(aiCall, context.objectPropertyList());
        }
        
        return aiCall;
    }

    public override AstNode VisitAdaptFunction(AdaptFunctionContext context)
    {
        var aiCall = new AICallNode();
        SetLocation(aiCall, context);
        
        aiCall.FunctionName = "adapt";
        aiCall.Arguments.Add((ExpressionNode)Visit(context.expression()));
        
        // Parse options from objectPropertyList if present
        if (context.objectPropertyList() != null)
        {
            ParseAIOptions(aiCall, context.objectPropertyList());
        }
        
        return aiCall;
    }

    public override AstNode VisitTryStatement(TryStatementContext context)
    {
        var tryStmt = new TryStatementNode();
        SetLocation(tryStmt, context);

        tryStmt.TryBlock = (StatementNode)Visit(context.blockStatement(0));

        if (context.blockStatement().Length > 1)
        {
            tryStmt.CatchVariableName = context.IDENTIFIER()?.GetText();
            tryStmt.CatchBlock = (StatementNode)Visit(context.blockStatement(1));
        }

        return tryStmt;
    }

    public override AstNode VisitThrowStatement(ThrowStatementContext context)
    {
        var throwStmt = new ThrowStatementNode();
        SetLocation(throwStmt, context);

        throwStmt.Expression = (ExpressionNode)Visit(context.expression());

        return throwStmt;
    }

    public override AstNode VisitNewExpression(NewExpressionContext context)
    {
        var newExpr = new NewExpressionNode();
        SetLocation(newExpr, context);

        newExpr.TypeName = context.IDENTIFIER().GetText();

        if (context.argumentList() != null)
        {
            foreach (var argContext in context.argumentList().expression())
            {
                newExpr.Arguments.Add((ExpressionNode)Visit(argContext));
            }
        }

        return newExpr;
    }
}
