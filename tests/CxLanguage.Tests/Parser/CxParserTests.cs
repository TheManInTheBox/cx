using FluentAssertions;
using CxLanguage.Core.Ast;
using Xunit;

namespace CxLanguage.Tests.Parser;

public class CxParserTests
{
    [Fact]
    public void Parse_SimpleVariableDeclaration_ShouldSucceed()
    {
        // Arrange
        var source = "var x = 42";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Statements.Should().HaveCount(1);
        
        var statement = result.Value.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        statement.Name.Should().Be("x");
        
        var literal = statement.Initializer.Should().BeOfType<LiteralNode>().Subject;
        literal.Value.Should().Be(42);
        literal.Type.Should().Be(LiteralType.Number);
    }

    [Fact]
    public void Parse_BinaryExpression_ShouldSucceed()
    {
        // Arrange
        var source = "var result = 10 + 5";

        // Act
        var parseResult = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        parseResult.IsSuccess.Should().BeTrue();
        parseResult.Value.Should().NotBeNull();
        
        var variable = parseResult.Value!.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        variable.Name.Should().Be("result");
        
        var binaryExpr = variable.Initializer.Should().BeOfType<BinaryExpressionNode>().Subject;
        binaryExpr.Operator.Should().Be(BinaryOperator.Add);
    }

    [Fact]
    public void Parse_LogicalExpression_ShouldSucceed()
    {
        // Arrange
        var source = "var result = true && false";

        // Act
        var parseResult = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        parseResult.IsSuccess.Should().BeTrue();
        parseResult.Value.Should().NotBeNull();
        
        var variable = parseResult.Value!.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        variable.Name.Should().Be("result");
        
        var binaryExpr = variable.Initializer.Should().BeOfType<BinaryExpressionNode>().Subject;
        binaryExpr.Operator.Should().Be(BinaryOperator.And);
    }

    [Fact]
    public void Parse_StringLiteral_ShouldSucceed()
    {
        // Arrange
        var source = "var message = \"Hello World\"";

        // Act
        var parseResult = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        parseResult.IsSuccess.Should().BeTrue();
        parseResult.Value.Should().NotBeNull();
        
        var variable = parseResult.Value!.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        variable.Name.Should().Be("message");
        
        var literal = variable.Initializer.Should().BeOfType<LiteralNode>().Subject;
        literal.Value.Should().Be("Hello World");
        literal.Type.Should().Be(LiteralType.String);
    }

    [Fact]
    public void Parse_BooleanLiteral_ShouldSucceed()
    {
        // Arrange
        var source = "var flag = true";

        // Act
        var parseResult = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        parseResult.IsSuccess.Should().BeTrue();
        parseResult.Value.Should().NotBeNull();
        
        var variable = parseResult.Value!.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        variable.Name.Should().Be("flag");
        
        var literal = variable.Initializer.Should().BeOfType<LiteralNode>().Subject;
        literal.Value.Should().Be(true);
        literal.Type.Should().Be(LiteralType.Boolean);
    }

    [Fact]
    public void Parse_InvalidSyntax_ShouldReturnErrors()
    {
        // Arrange
        var source = @"var x = ";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
