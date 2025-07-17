grammar Cx;

// Entry point
program: statement* EOF;

// Statements
statement
    : functionDeclaration
    | variableDeclaration
    | expressionStatement
    | importStatement
    | returnStatement
    | ifStatement
    | whileStatement
    | forStatement
    | blockStatement
    ;

// Import statements for AI services
importStatement: 'using' IDENTIFIER 'from' STRING_LITERAL;

// Function declarations with optional async support
functionDeclaration
    : 'async'? 'function' IDENTIFIER '(' parameterList? ')' ('->' type)? blockStatement
    ;

parameterList: parameter (',' parameter)*;
parameter: IDENTIFIER ':' type;

// Variable declarations
variableDeclaration: 'var' IDENTIFIER '=' expression;

// Control flow
ifStatement: 'if' '(' expression ')' statement ('else' statement)?;
whileStatement: 'while' '(' expression ')' statement;
forStatement: 'for' '(' IDENTIFIER 'in' expression ')' statement;
returnStatement: 'return' expression?;

// Blocks
blockStatement: '{' statement* '}';
expressionStatement: expression;

// Expressions
expression
    : primary                                           # PrimaryExpression
    | expression '.' IDENTIFIER                         # MemberAccess
    | expression '(' argumentList? ')'                  # FunctionCall
    | expression '[' expression ']'                     # IndexAccess
    | 'await' expression                                # AwaitExpression
    | 'parallel' expression                             # ParallelExpression
    | expression ('*' | '/' | '%') expression           # MultiplicativeExpression
    | expression ('+' | '-') expression                 # AdditiveExpression
    | expression ('<' | '>' | '<=' | '>=' | '==' | '!=') expression # RelationalExpression
    | expression ('&&' | '||') expression               # LogicalExpression
    | expression '=' expression                         # AssignmentExpression
    | '{' objectPropertyList? '}'                       # ObjectLiteral
    | '[' argumentList? ']'                             # ArrayLiteral
    ;

// Object properties for AI configuration
objectPropertyList: objectProperty (',' objectProperty)*;
objectProperty: (IDENTIFIER | STRING_LITERAL) ':' expression;

argumentList: expression (',' expression)*;

// Primary expressions
primary
    : IDENTIFIER
    | STRING_LITERAL
    | NUMBER_LITERAL
    | BOOLEAN_LITERAL
    | '(' expression ')'
    ;

// Types
type
    : 'string'
    | 'number'
    | 'boolean'
    | 'array' '<' type '>'
    | 'object'
    | 'any'
    | IDENTIFIER  // Custom types
    ;

// Tokens
BOOLEAN_LITERAL: 'true' | 'false';
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
STRING_LITERAL: '"' (~["\r\n] | '\\' .)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;

// Whitespace and comments
WS: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
