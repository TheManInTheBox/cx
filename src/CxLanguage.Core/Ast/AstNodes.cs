using CxLanguage.Core.Types;

namespace CxLanguage.Core.Ast;

/// <summary>
/// Visitor interface for AST nodes
/// </summary>
public interface IAstVisitor<T>
{
    T VisitProgram(ProgramNode node);
    T VisitBlock(BlockStatementNode node);
    T VisitExpression(ExpressionStatementNode node);
    T VisitVariableDeclaration(VariableDeclarationNode node);
    T VisitFunctionDeclaration(FunctionDeclarationNode node);
    T VisitParameter(ParameterNode node);
    T VisitImport(ImportStatementNode node);
    T VisitReturn(ReturnStatementNode node);
    T VisitIf(IfStatementNode node);
    T VisitWhile(WhileStatementNode node);
    T VisitFor(ForStatementNode node);
    T VisitBinaryExpression(BinaryExpressionNode node);
    T VisitUnaryExpression(UnaryExpressionNode node);
    T VisitCallExpression(CallExpressionNode node);
    T VisitIdentifier(IdentifierNode node);
    T VisitLiteral(LiteralNode node);
    T VisitAssignmentExpression(AssignmentExpressionNode node);
    T VisitMemberAccess(MemberAccessNode node);
    T VisitIndexAccess(IndexAccessNode node);
    T VisitFunctionCall(FunctionCallNode node);
    
    // AI-specific visitor methods
    T VisitAITask(AITaskNode node);
    T VisitAISynthesize(AISynthesizeNode node);
    T VisitAICall(AICallNode node);
    T VisitAIReason(AIReasonNode node);
    T VisitAIProcess(AIProcessNode node);
    T VisitAIEmbed(AIEmbedNode node);
    T VisitAIAdapt(AIAdaptNode node);
    
    // Exception handling visitor methods
    T VisitTryStatement(TryStatementNode node);
    T VisitThrowStatement(ThrowStatementNode node);
    
    // Object creation visitor method
    T VisitNewExpression(NewExpressionNode node);
}

/// <summary>
/// Base class for all AST nodes
/// </summary>
public abstract class AstNode
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string? SourceFile { get; set; }

    public abstract T Accept<T>(IAstVisitor<T> visitor);
}

/// <summary>
/// Root node representing a complete program
/// </summary>
public class ProgramNode : AstNode
{
    public List<StatementNode> Statements { get; set; } = new();
    public List<ImportStatementNode> Imports { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitProgram(this);
}

/// <summary>
/// Base class for all statement nodes
/// </summary>
public abstract class StatementNode : AstNode
{
}

/// <summary>
/// Base class for all expression nodes
/// </summary>
public abstract class ExpressionNode : AstNode
{
    public CxType? InferredType { get; set; }
}

/// <summary>
/// Function declaration statement
/// </summary>
public class FunctionDeclarationNode : StatementNode
{
    public string Name { get; set; } = string.Empty;
    public List<ParameterNode> Parameters { get; set; } = new();
    public CxType? ReturnType { get; set; }
    public BlockStatementNode Body { get; set; } = new();
    public bool IsAsync { get; set; }

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitFunctionDeclaration(this);
}

/// <summary>
/// Function parameter
/// </summary>
public class ParameterNode : AstNode
{
    public string Name { get; set; } = string.Empty;
    public CxType Type { get; set; } = CxType.Any;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitParameter(this);
}

/// <summary>
/// Variable declaration statement
/// </summary>
public class VariableDeclarationNode : StatementNode
{
    public string Name { get; set; } = string.Empty;
    public ExpressionNode Initializer { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitVariableDeclaration(this);
}

/// <summary>
/// Import statement
/// </summary>
public class ImportStatementNode : StatementNode
{
    public string Alias { get; set; } = string.Empty;
    public string ModulePath { get; set; } = string.Empty;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitImport(this);
}

/// <summary>
/// Return statement
/// </summary>
public class ReturnStatementNode : StatementNode
{
    public ExpressionNode? Value { get; set; }

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitReturn(this);
}

/// <summary>
/// If statement
/// </summary>
public class IfStatementNode : StatementNode
{
    public ExpressionNode Condition { get; set; } = null!;
    public StatementNode ThenStatement { get; set; } = null!;
    public StatementNode? ElseStatement { get; set; }

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitIf(this);
}

/// <summary>
/// While loop statement
/// </summary>
public class WhileStatementNode : StatementNode
{
    public ExpressionNode Condition { get; set; } = null!;
    public StatementNode Body { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitWhile(this);
}

/// <summary>
/// For loop statement
/// </summary>
public class ForStatementNode : StatementNode
{
    public string Variable { get; set; } = string.Empty;
    public ExpressionNode Iterable { get; set; } = null!;
    public StatementNode Body { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitFor(this);
}

/// <summary>
/// Block statement containing multiple statements
/// </summary>
public class BlockStatementNode : StatementNode
{
    public List<StatementNode> Statements { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitBlock(this);
}

/// <summary>
/// Expression statement (expression used as statement)
/// </summary>
public class ExpressionStatementNode : StatementNode
{
    public ExpressionNode Expression { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitExpression(this);
}

/// <summary>
/// Binary expression (e.g., a + b, x == y)
/// </summary>
public class BinaryExpressionNode : ExpressionNode
{
    public ExpressionNode Left { get; set; } = null!;
    public BinaryOperator Operator { get; set; }
    public ExpressionNode Right { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitBinaryExpression(this);
}

/// <summary>
/// Unary expression (e.g., !a, -b)
/// </summary>
public class UnaryExpressionNode : ExpressionNode
{
    public UnaryOperator Operator { get; set; }
    public ExpressionNode Operand { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitUnaryExpression(this);
}

/// <summary>
/// Function call expression
/// </summary>
public class CallExpressionNode : ExpressionNode
{
    public ExpressionNode Callee { get; set; } = null!;
    public List<ExpressionNode> Arguments { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitCallExpression(this);
}

/// <summary>
/// Identifier expression
/// </summary>
public class IdentifierNode : ExpressionNode
{
    public string Name { get; set; } = string.Empty;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitIdentifier(this);
}

/// <summary>
/// Literal value expression
/// </summary>
public class LiteralNode : ExpressionNode
{
    public object? Value { get; set; }
    public LiteralType Type { get; set; }

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitLiteral(this);
}

/// <summary>
/// Assignment expression (e.g., x = 5)
/// </summary>
public class AssignmentExpressionNode : ExpressionNode
{
    public ExpressionNode Left { get; set; } = null!;
    public ExpressionNode Right { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAssignmentExpression(this);
}

/// <summary>
/// Member access expression (e.g., obj.property)
/// </summary>
public class MemberAccessNode : ExpressionNode
{
    public ExpressionNode Object { get; set; } = null!;
    public string Property { get; set; } = string.Empty;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitMemberAccess(this);
}

/// <summary>
/// Index access expression (e.g., arr[0])
/// </summary>
public class IndexAccessNode : ExpressionNode
{
    public ExpressionNode Object { get; set; } = null!;
    public ExpressionNode Index { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitIndexAccess(this);
}

/// <summary>
/// Function call node (alternative to CallExpressionNode)
/// </summary>
public class FunctionCallNode : ExpressionNode
{
    public string FunctionName { get; set; } = string.Empty;
    public List<ExpressionNode> Arguments { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitFunctionCall(this);
}

/// <summary>
/// Enum for binary operators
/// </summary>
public enum BinaryOperator
{
    Add, Subtract, Multiply, Divide, Modulo,
    Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
    And, Or, BitwiseAnd, BitwiseOr, BitwiseXor,
    LeftShift, RightShift
}

/// <summary>
/// Enum for unary operators
/// </summary>
public enum UnaryOperator
{
    Negate, Not, BitwiseNot, Plus, Minus
}

/// <summary>
/// Enum for literal types
/// </summary>
public enum LiteralType
{
    String, Number, Boolean, Null
}

/// <summary>
/// AI task planning statement
/// </summary>
public class AITaskNode : StatementNode
{
    public string Goal { get; set; } = string.Empty;
    public Dictionary<string, object> Options { get; set; } = new();
    public string? AssignTo { get; set; } // Variable to assign the result to

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAITask(this);
}

/// <summary>
/// AI code synthesis statement
/// </summary>
public class AISynthesizeNode : StatementNode
{
    public string Specification { get; set; } = string.Empty;
    public string? TargetLanguage { get; set; } = "cx";
    public Dictionary<string, object> Options { get; set; } = new();
    public string? AssignTo { get; set; } // Variable to assign the generated code to

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAISynthesize(this);
}

/// <summary>
/// AI function call expression
/// </summary>
public class AICallNode : ExpressionNode
{
    public string FunctionName { get; set; } = string.Empty;
    public List<ExpressionNode> Arguments { get; set; } = new();
    public Dictionary<string, object> Options { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAICall(this);
}

/// <summary>
/// AI reasoning loop statement
/// </summary>
public class AIReasonNode : StatementNode
{
    public string Goal { get; set; } = string.Empty;
    public int MaxIterations { get; set; } = 3;
    public double SatisfactionThreshold { get; set; } = 80.0;
    public Dictionary<string, object> Options { get; set; } = new();
    public string? AssignTo { get; set; } // Variable to assign the reasoning result to

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAIReason(this);
}

/// <summary>
/// Multi-modal AI processing statement
/// </summary>
public class AIProcessNode : StatementNode
{
    public string InputType { get; set; } = "text"; // text, image, audio, video
    public ExpressionNode Input { get; set; } = new LiteralNode { Type = LiteralType.Null, Value = null };
    public Dictionary<string, object> Options { get; set; } = new();
    public string? AssignTo { get; set; } // Variable to assign the result to

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAIProcess(this);
}

/// <summary>
/// AI embedding generation expression
/// </summary>
public class AIEmbedNode : ExpressionNode
{
    public ExpressionNode Text { get; set; } = new LiteralNode { Type = LiteralType.String, Value = "" };
    public Dictionary<string, object> Options { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAIEmbed(this);
}

/// <summary>
/// Adaptive code path statement
/// </summary>
public class AIAdaptNode : StatementNode
{
    public string CodePath { get; set; } = string.Empty;
    public ExpressionNode Context { get; set; } = new LiteralNode { Type = LiteralType.Null, Value = null };
    public Dictionary<string, object> Options { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAIAdapt(this);
}

/// <summary>
/// Try-catch statement
/// </summary>
public class TryStatementNode : StatementNode
{
    public StatementNode TryBlock { get; set; } = null!;
    public string? CatchVariableName { get; set; }
    public StatementNode? CatchBlock { get; set; }

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitTryStatement(this);
}

/// <summary>
/// Throw statement
/// </summary>
public class ThrowStatementNode : StatementNode
{
    public ExpressionNode Expression { get; set; } = null!;

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitThrowStatement(this);
}

/// <summary>
/// New expression (object creation)
/// </summary>
public class NewExpressionNode : ExpressionNode
{
    public string TypeName { get; set; } = string.Empty;
    public List<ExpressionNode> Arguments { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitNewExpression(this);
}
