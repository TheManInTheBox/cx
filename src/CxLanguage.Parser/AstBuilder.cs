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
    
    // Reserved keywords that cannot be used as identifiers in most contexts
    // Note: Agent behavior determined by event handlers - no special keywords needed
    private static readonly HashSet<string> ReservedKeywords = new HashSet<string>
    {
        "class", "interface", "extends", "import", "uses", "constructor",
        "public", "private", "protected", "try", "catch", "throw", "new",
        "null", "true", "false", "on", "emit", "function",
        "var", "in", "async", "await"
    };

    public AstBuilder(string? fileName = null)
    {
        _fileName = fileName;
    }

    /// <summary>
    /// Validates that an identifier is not a reserved keyword in contexts where keywords are forbidden
    /// </summary>
    private void ValidateIdentifierNotKeyword(string identifier, IParseTree context, string contextDescription)
    {
        if (ReservedKeywords.Contains(identifier))
        {
            throw new InvalidOperationException(
                $"Keyword '{identifier}' cannot be used as {contextDescription} at {GetLocationString(context)}");
        }
    }
    
    /// <summary>
    /// Gets location string for error reporting
    /// </summary>
    private string GetLocationString(IParseTree context)
    {
        if (context is ParserRuleContext ruleContext)
        {
            var start = ruleContext.Start;
            return $"line {start.Line}, column {start.Column}";
        }
        return "unknown location";
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

        var identifier = context.IDENTIFIER().GetText();
        ValidateIdentifierNotKeyword(identifier, context, "variable name");
        varDecl.Name = identifier;
        varDecl.Initializer = (ExpressionNode)Visit(context.expression());

        return varDecl;
    }

    public override AstNode VisitDottedIdentifier(DottedIdentifierContext context)
    {
        // Build the dotted identifier string (e.g., "Cx.AI.TextGeneration")
        var parts = context.IDENTIFIER().Select(id => id.GetText()).ToList();
        return new LiteralNode { Value = string.Join(".", parts) };
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
        else if (context.TRUE() != null || context.FALSE() != null)
        {
            var literal = new LiteralNode();
            SetLocation(literal, context);
            literal.Value = context.TRUE() != null ? true : false;
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
        else if (context.GetText() == "this")
        {
            var identifier = new IdentifierNode { Name = "this" };
            SetLocation(identifier, context);
            return identifier;
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
        var objectLiteral = new ObjectLiteralNode();
        SetLocation(objectLiteral, context);
        
        // Parse object properties if present
        if (context.objectPropertyList() != null)
        {
            var propListContext = context.objectPropertyList();
            foreach (var propContext in propListContext.objectProperty())
            {
                var property = (ObjectPropertyNode)Visit(propContext);
                objectLiteral.Properties.Add(property);
            }
        }
        
        return objectLiteral;
    }

    public override AstNode VisitObjectProperty(CxParser.ObjectPropertyContext context)
    {
        var property = new ObjectPropertyNode();
        SetLocation(property, context);
        
        // Get the key (identifier or string literal)
        if (context.IDENTIFIER() != null)
        {
            var identifier = context.IDENTIFIER().GetText();
            // Validate that keywords cannot be used as object property names
            ValidateIdentifierNotKeyword(identifier, context, "object property name");
            property.Key = identifier;
        }
        else if (context.STRING_LITERAL() != null)
        {
            // Remove quotes from string literal - string literals can contain any text including keywords
            var stringLiteral = context.STRING_LITERAL().GetText();
            property.Key = stringLiteral.Substring(1, stringLiteral.Length - 2);
        }
        
        // Get the value (either expression or handlersList)
        if (context.expression() != null)
        {
            // Property has an expression value
            property.Value = (ExpressionNode)Visit(context.expression());
        }
        else if (context.handlersList() != null)
        {
            // Property has a handlersList value
            // For now, treat handlers list as a special array literal
            var handlersList = (ArrayLiteralNode)Visit(context.handlersList());
            property.Value = handlersList;
        }
        else
        {
            throw new InvalidOperationException($"Object property must have either expression or handlersList at {GetLocationString(context)}");
        }
        
        return property;
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

    public override AstNode VisitHandlersList(HandlersListContext context)
    {
        // HandlersList is '[' (eventHandlerReference | handlerItem) (',' (eventHandlerReference | handlerItem))* ']'
        // We'll represent it as an ArrayLiteral of handler items
        var arrayLiteral = new ArrayLiteralNode();
        SetLocation(arrayLiteral, context);
        
        Console.WriteLine($"🔧 AST: VisitHandlersList called with context: {context.GetText()}");
        Console.WriteLine($"🔧 AST: Children count: {context.ChildCount}");
        
        // Let's examine all children to understand the structure
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            Console.WriteLine($"🔧 AST: Child {i}: {child.GetType().Name} = '{child.GetText()}'");
            
            // Try to visit the child if it's a rule context
            if (child is ParserRuleContext ruleContext)
            {
                Console.WriteLine($"🔧 AST: Attempting to visit rule context: {ruleContext.GetType().Name}");
                try
                {
                    var childResult = Visit(child);
                    if (childResult != null)
                    {
                        Console.WriteLine($"🔧 AST: Visit result: {childResult.GetType().Name}");
                        if (childResult is ExpressionNode expr)
                        {
                            arrayLiteral.Elements.Add(expr);
                            Console.WriteLine($"✅ AST: Added expression as handler");
                        }
                        else if (childResult is LiteralNode literal)
                        {
                            arrayLiteral.Elements.Add(literal);
                            Console.WriteLine($"✅ AST: Added literal as handler: '{literal.Value}'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ AST: Error visiting child: {ex.Message}");
                }
            }
        }
        
        // Fallback: if no elements were added, try to extract from the text directly
        if (arrayLiteral.Elements.Count == 0)
        {
            var text = context.GetText();
            Console.WriteLine($"🔧 AST: Fallback parsing of text: '{text}'");
            
            // Remove brackets and split by comma
            if (text.StartsWith("[") && text.EndsWith("]"))
            {
                var content = text.Substring(1, text.Length - 2).Trim();
                if (!string.IsNullOrEmpty(content))
                {
                    var handlers = content.Split(',').Select(h => h.Trim()).Where(h => !string.IsNullOrEmpty(h));
                    foreach (var handler in handlers)
                    {
                        Console.WriteLine($"🔧 AST: Fallback creating literal for: '{handler}'");
                        var literal = new LiteralNode
                        {
                            Type = LiteralType.String,
                            Value = handler
                        };
                        arrayLiteral.Elements.Add(literal);
                        Console.WriteLine($"✅ AST: Fallback added handler: '{handler}'");
                    }
                }
            }
        }
        
        Console.WriteLine($"🔧 AST: VisitHandlersList returning ArrayLiteral with {arrayLiteral.Elements.Count} elements");
        return arrayLiteral;
    }

    public override AstNode VisitHandlerItem(CxParser.HandlerItemContext context)
    {
        var handlerItem = new HandlerItemNode();
        SetLocation(handlerItem, context);
        
        // Parse the event name
        handlerItem.EventName = (EventNameNode)Visit(context.eventName());
        
        // Check if there's a custom payload (optional object literal)
        var objectPropertyListContext = context.objectPropertyList();
        if (objectPropertyListContext != null)
        {
            // Create an ObjectLiteralNode from the property list
            var objectLiteral = new ObjectLiteralNode();
            SetLocation(objectLiteral, objectPropertyListContext);
            
            foreach (var propertyContext in objectPropertyListContext.objectProperty())
            {
                var property = (ObjectPropertyNode)Visit(propertyContext);
                objectLiteral.Properties.Add(property);
            }
            
            handlerItem.CustomPayload = objectLiteral;
        }
        
        return handlerItem;
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

    // Object system visitors
    public override AstNode VisitObjectDeclaration(ObjectDeclarationContext context)
    {
        var objectDecl = new ClassDeclarationNode(); // Reusing ClassDeclarationNode internally
        SetLocation(objectDecl, context);

        var objectName = context.IDENTIFIER(0).GetText();
        ValidateIdentifierNotKeyword(objectName, context, "object name");
        objectDecl.Name = objectName;
        objectDecl.IsConsciousEntity = false; // Mark as simple object
        
        // Parse base class if present (extends keyword)
        if (context.IDENTIFIER().Length > 1)
        {
            objectDecl.BaseClass = context.IDENTIFIER(1).GetText();
        }

        // Parse object body (no fields, only realize and event handlers)
        if (context.objectBody() != null)
        {
            foreach (var memberContext in context.objectBody().objectMember())
            {
                if (memberContext.realizeDeclaration() != null)
                {
                    objectDecl.RealizeDeclarations.Add((RealizeDeclarationNode)Visit(memberContext.realizeDeclaration()));
                }
                else if (memberContext.onStatement() != null)
                {
                    // Handle event handlers inside objects
                    var eventHandler = (OnStatementNode)Visit(memberContext.onStatement());
                    objectDecl.EventHandlers.Add(eventHandler);
                }
            }
        }

        return objectDecl;
    }

    // Conscious entity system visitors
    public override AstNode VisitConsciousDeclaration(ConsciousDeclarationContext context)
    {
        var consciousDecl = new ClassDeclarationNode(); // Reusing ClassDeclarationNode internally
        SetLocation(consciousDecl, context);

        var consciousName = context.IDENTIFIER().GetText();
        ValidateIdentifierNotKeyword(consciousName, context, "conscious entity name");
        consciousDecl.Name = consciousName;
        consciousDecl.IsConsciousEntity = true; // Mark as intelligent conscious entity
        
        // No inheritance support in pure consciousness patterns

        // Parse conscious body (no fields, only realize and event handlers)
        if (context.consciousBody() != null)
        {
            foreach (var memberContext in context.consciousBody().consciousMember())
            {
                if (memberContext.realizeDeclaration() != null)
                {
                    consciousDecl.RealizeDeclarations.Add((RealizeDeclarationNode)Visit(memberContext.realizeDeclaration()));
                }
                else if (memberContext.onStatement() != null)
                {
                    // Handle event handlers inside conscious entities
                    var eventHandler = (OnStatementNode)Visit(memberContext.onStatement());
                    consciousDecl.EventHandlers.Add(eventHandler);
                }
            }
        }

        return consciousDecl;
    }

    public override AstNode VisitDecorator(DecoratorContext context)
    {
        var decorator = new DecoratorNode();
        SetLocation(decorator, context);
        
        decorator.Name = context.IDENTIFIER().GetText();
        
        return decorator;
    }

    public override AstNode VisitRealizeDeclaration(RealizeDeclarationContext context)
    {
        var realizeDecl = new RealizeDeclarationNode();
        SetLocation(realizeDecl, context);

        // Optional object parameter for consciousness initialization
        if (context.realizeParameter() != null)
        {
            var param = new ParameterNode
            {
                Name = context.realizeParameter().IDENTIFIER().GetText(),
                Type = CxType.Object // Always object type for consciousness data
            };
            SetLocation(param, context.realizeParameter());
            realizeDecl.Parameters.Add(param);
        }

        // Body
        realizeDecl.Body = (BlockStatementNode)Visit(context.blockStatement());

        return realizeDecl;
    }

    public override AstNode VisitEventName(EventNameContext context)
    {
        var eventName = new EventNameNode();
        SetLocation(eventName, context);

        // Parse all eventNamePart tokens separated by dots
        foreach (var eventNamePart in context.eventNamePart())
        {
            eventName.Parts.Add(eventNamePart.GetText());
        }

        return eventName;
    }

    public override AstNode VisitOnStatement(OnStatementContext context)
    {
        var onStmt = new OnStatementNode();
        SetLocation(onStmt, context);

        // Check if this is an async event handler
        onStmt.IsAsync = context.GetText().StartsWith("onasync");

        // Parse event name (now using eventName rule)
        onStmt.EventName = (EventNameNode)Visit(context.eventName());

        // Parse payload identifier
        onStmt.PayloadIdentifier = context.IDENTIFIER().GetText();

        // Parse body
        onStmt.Body = (BlockStatementNode)Visit(context.blockStatement());

        return onStmt;
    }

    public override AstNode VisitEmitStatement(EmitStatementContext context)
    {
        var emitStmt = new EmitStatementNode();
        SetLocation(emitStmt, context);

        // Parse event name (now using eventName rule)
        emitStmt.EventName = (EventNameNode)Visit(context.eventName());

        // Parse payload (expression, if present)
        if (context.expression() != null)
        {
            emitStmt.Payload = (ExpressionNode)Visit(context.expression());
        }

        return emitStmt;
    }

    public override AstNode VisitAiServiceStatement(AiServiceStatementContext context)
    {
        var aiServiceStmt = new AiServiceStatementNode();
        SetLocation(aiServiceStmt, context);

        // Parse AI service name
        aiServiceStmt.ServiceName = context.aiServiceName().GetText();

        // Parse payload (expression, if present)
        if (context.aiServiceParameters() != null)
        {
            aiServiceStmt.Payload = (ExpressionNode)Visit(context.aiServiceParameters().expression());
        }

        return aiServiceStmt;
    }

    public override AstNode VisitEventHandlerReference(CxParser.EventHandlerReferenceContext context)
    {
        // EventHandlerReference is IDENTIFIER ('.' IDENTIFIER)*
        // This should be treated as a function call reference like math.calculation.complete
        Console.WriteLine($"🔧 AST: VisitEventHandlerReference called with: '{context.GetText()}'");
        
        var identifiers = context.IDENTIFIER();
        if (identifiers.Length == 1)
        {
            // Simple identifier - create an IdentifierNode
            var identifier = new IdentifierNode
            {
                Name = identifiers[0].GetText()
            };
            SetLocation(identifier, context);
            Console.WriteLine($"✅ AST: Created simple identifier: '{identifier.Name}'");
            return identifier;
        }
        else
        {
            // Multiple identifiers separated by dots - create a MemberAccessNode chain
            ExpressionNode current = new IdentifierNode { Name = identifiers[0].GetText() };
            SetLocation(current, identifiers[0]);
            
            for (int i = 1; i < identifiers.Length; i++)
            {
                var memberAccess = new MemberAccessNode
                {
                    Object = current,
                    Property = identifiers[i].GetText()
                };
                SetLocation(memberAccess, identifiers[i]);
                current = memberAccess;
                Console.WriteLine($"🔧 AST: Added member access: '{identifiers[i].GetText()}'");
            }
            
            Console.WriteLine($"✅ AST: Created member access chain for: '{context.GetText()}'");
            return current;
        }
    }
}
