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
            program.Statements.Add(statement);
        }

        return program;
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
                    Type = ParseType(paramContext.type())
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
        else if (context.expression() != null)
        {
            return Visit(context.expression());
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
        // For now, just return a null literal - array literals not fully implemented
        var literal = new LiteralNode();
        SetLocation(literal, context);
        literal.Value = null;
        literal.Type = LiteralType.Null;
        return literal;
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
}
