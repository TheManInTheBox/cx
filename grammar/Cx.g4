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
    | tryStatement
    | throwStatement
    ;

// Import statements for AI services
importStatement: 'using' IDENTIFIER 'from' STRING_LITERAL ';';

// Function declarations with optional async support
functionDeclaration
    : 'async'? 'function' IDENTIFIER '(' parameterList? ')' ('->' type)? blockStatement
    ;

parameterList: parameter (',' parameter)*;
parameter: IDENTIFIER (':' type)?;  // Make type optional

// Variable declarations
variableDeclaration: 'var' IDENTIFIER '=' expression ';';

// Control flow
ifStatement: 'if' '(' expression ')' statement ('else' statement)?;
whileStatement: 'while' '(' expression ')' statement;
forStatement: 'for' '(' ('var' IDENTIFIER | IDENTIFIER) 'in' expression ')' statement;
returnStatement: 'return' expression? ';';

// Exception handling
tryStatement: 'try' blockStatement ('catch' '(' IDENTIFIER ')' blockStatement)?;
throwStatement: 'throw' expression ';';

// Blocks
blockStatement: '{' statement* '}';
expressionStatement: expression ';';

// Expressions
expression
    : primary                                           # PrimaryExpression
    | 'new' IDENTIFIER '(' argumentList? ')'           # NewExpression
    | expression '.' IDENTIFIER                         # MemberAccess
    | expression '(' argumentList? ')'                  # FunctionCall
    | expression '[' expression ']'                     # IndexAccess
    | 'await' expression                                # AwaitExpression
    | 'parallel' expression                             # ParallelExpression
    | ('!' | '-' | '+') expression                      # UnaryExpression
    | expression ('*' | '/' | '%') expression           # MultiplicativeExpression
    | expression ('+' | '-') expression                 # AdditiveExpression
    | expression ('<' | '>' | '<=' | '>=' | '==' | '!=') expression # RelationalExpression
    | expression ('&&' | '||') expression               # LogicalExpression
    | expression ('=' | '+=' | '-=' | '*=' | '/=') expression # AssignmentExpression
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
    | NULL
    | '(' expression ')'
    | aiFunction
    ;

// AI-native functions
aiFunction
    : TASK '(' expression (',' objectPropertyList)? ')'           # TaskFunction
    | SYNTHESIZE '(' expression (',' objectPropertyList)? ')'     # SynthesizeFunction
    | REASON '(' expression (',' objectPropertyList)? ')'         # ReasonFunction
    | PROCESS '(' expression ',' expression (',' objectPropertyList)? ')' # ProcessFunction
    | GENERATE '(' expression (',' objectPropertyList)? ')'       # GenerateFunction
    | EMBED '(' expression (',' objectPropertyList)? ')'          # EmbedFunction
    | ADAPT '(' expression (',' objectPropertyList)? ')'          # AdaptFunction
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
TASK: 'task';
SYNTHESIZE: 'synthesize';
REASON: 'reason';
PROCESS: 'process';
GENERATE: 'generate';
EMBED: 'embed';
ADAPT: 'adapt';
TRY: 'try';
CATCH: 'catch';
THROW: 'throw';
NEW: 'new';
NULL: 'null';
BOOLEAN_LITERAL: 'true' | 'false';
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
STRING_LITERAL: '"' (~["\r\n] | '\\' .)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;

// Punctuation
SEMICOLON: ';';

// Whitespace and comments
WS: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
