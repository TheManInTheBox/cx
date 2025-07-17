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
        var source = "x = 42";

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
        literal.LiteralType.Should().Be(LiteralType.Number);
    }

    [Fact]
    public void Parse_FunctionDeclaration_ShouldSucceed()
    {
        // Arrange
        var source = @"
            function greet(name: string) -> string {
                return ""Hello, "" + name
            }";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Statements.Should().HaveCount(1);
        
        var function = result.Value.Statements[0].Should().BeOfType<FunctionDeclarationNode>().Subject;
        function.Name.Should().Be("greet");
        function.IsAsync.Should().BeFalse();
        function.Parameters.Should().HaveCount(1);
        function.Parameters[0].Name.Should().Be("name");
    }

    [Fact]
    public void Parse_AsyncFunction_ShouldSucceed()
    {
        // Arrange
        var source = @"
            async function analyzeText(text: string) -> string {
                result = await ai.analyze(text)
                return result
            }";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        var function = result.Value.Statements[0].Should().BeOfType<FunctionDeclarationNode>().Subject;
        function.Name.Should().Be("analyzeText");
        function.IsAsync.Should().BeTrue();
    }

    [Fact]
    public void Parse_ImportStatement_ShouldSucceed()
    {
        // Arrange
        var source = @"using ai from ""azure.openai""";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Imports.Should().HaveCount(1);
        
        var import = result.Value.Imports[0];
        import.Alias.Should().Be("ai");
        import.ModulePath.Should().Be("azure.openai");
    }

    [Fact]
    public void Parse_ObjectLiteral_ShouldSucceed()
    {
        // Arrange
        var source = @"config = { model: ""gpt-4"", temperature: 0.7, maxTokens: 1000 }";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        var variable = result.Value!.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        var objectLiteral = variable.Initializer.Should().BeOfType<ObjectLiteralNode>().Subject;
        var properties = objectLiteral.Properties;
        properties.Should().HaveCount(3);
        properties[0].Key.Should().Be("model");
        properties[1].Key.Should().Be("temperature");
        properties[2].Key.Should().Be("maxTokens");
    }

    [Fact]
    public void Parse_ArrayLiteral_ShouldSucceed()
    {
        // Arrange
        var source = @"documents = [""doc1.txt"", ""doc2.txt"", ""doc3.txt""]";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        var variable = result.Value!.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        var arrayLiteral = variable.Initializer.Should().BeOfType<ArrayLiteralNode>().Subject;
        arrayLiteral.Elements.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_AwaitExpression_ShouldSucceed()
    {
        // Arrange
        var source = @"result = await ai.generateText(prompt)";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        var variable = result.Value!.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        variable.Initializer.Should().BeOfType<AwaitExpressionNode>();
    }

    [Fact]
    public void Parse_ParallelExpression_ShouldSucceed()
    {
        // Arrange
        var source = @"results = parallel documents.map(doc => ai.analyze(doc))";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        var variable = result.Value!.Statements[0].Should().BeOfType<VariableDeclarationNode>().Subject;
        variable.Initializer.Should().BeOfType<ParallelExpressionNode>();
    }

    [Fact]
    public void Parse_InvalidSyntax_ShouldReturnErrors()
    {
        // Arrange
        var source = @"function ( {";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Parse_CompleteProgram_ShouldSucceed()
    {
        // Arrange
        var source = @"
            using ai from ""azure.openai""
            using storage from ""azure.storage""

            model = ai.connect(""gpt-4-turbo"")
            docs = storage.connect(""documents"")

            async function analyzeDocument(path: string) -> object {
                content = docs.read(path)
                
                result = await model.analyze(content, {
                    task: ""sentiment_analysis"",
                    format: ""json"",
                    schema: {
                        sentiment: ""string"",
                        confidence: ""number""
                    }
                })
                
                return result
            }

            async function processFiles(paths: array<string>) {
                results = await parallel paths.map(path => analyzeDocument(path))
                return results
            }";

        // Act
        var result = CxLanguage.Parser.CxLanguageParser.Parse(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Imports.Should().HaveCount(2);
        result.Value.Statements.Should().HaveCount(6); // 2 imports + 2 variables + 2 functions
    }
}
