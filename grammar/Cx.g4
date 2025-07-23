grammar Cx;

// Entry point
program: statement* EOF;

// Statements
statement
    : classDeclaration
    | variableDeclaration
    | expressionStatement
    | forStatement
    | blockStatement
    | onStatement
    | emitStatement
    | aiServiceStatement
    ;

dottedIdentifier: IDENTIFIER ('.' IDENTIFIER)*;

// Class declarations with inheritance support (use : instead of implements)
classDeclaration
    : 'class' IDENTIFIER ('extends' IDENTIFIER)? classBody
    ;

// Decorator support for classes
decorator
    : '@' IDENTIFIER
    ;

classBody: '{' classMember* '}';

classMember
    : fieldDeclaration
    | constructorDeclaration
    | onStatement
    ;

fieldDeclaration: IDENTIFIER ':' type ('=' expression)? ';';

constructorDeclaration: 'constructor' '(' parameterList? ')' blockStatement;

parameterList: parameter (',' parameter)*;
parameter: IDENTIFIER (':' type)?;  // Make type optional

// Variable declarations
variableDeclaration: 'var' IDENTIFIER '=' expression ';';

// For-in loop for iteration (arrays, dictionaries, collections)
forStatement: 'for' '(' ('var' IDENTIFIER | IDENTIFIER) 'in' expression ')' statement;

// Cognitive Event-driven statements
eventNamePart: IDENTIFIER | 'any' | 'critical' | 'assigned' | 'tickets' | 'tasks' | 'support' | 'dev' | 'system' | 'alerts' | 'user' | 'ai' | 'sync' | 'learn' | 'think' | 'generate' | 'chat' | 'communicate' | 'search' | 'execute' | 'speak' | 'listen' | 'image' | 'analyze' | 'transcribe' | 'audio' | 'await' | 'completed' | 'ready' | 'activation' | 'timing' | 'decision' | 'work' | 'new';
eventName: eventNamePart ('.' eventNamePart)*;
onStatement: 'on' 'async'? eventName '(' IDENTIFIER ')' blockStatement;
emitStatement: 'emit' eventName ((',' expression) | expression)? ';';

// AI service statements - cognitive capabilities only
aiServiceName: 'is' | 'not' | 'learn' | 'think' | 'await' | 'adapt';
aiServiceStatement: aiServiceName expression ';';

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
    | ('!' | '-' | '+') expression                      # UnaryExpression
    | expression ('*' | '/' | '%') expression           # MultiplicativeExpression
    | expression ('+' | '-') expression                 # AdditiveExpression
    | expression ('<' | '>' | '<=' | '>=' | '==' | '!=') expression # RelationalExpression
    | expression ('&&' | '||') expression               # LogicalExpression
    | expression ('=' | '+=' | '-=' | '*=' | '/=') expression # AssignmentExpression
    | '{' objectPropertyList? '}'                       # ObjectLiteral
    | '[' argumentList? ']'                             # ArrayLiteral
    ;

objectPropertyList: objectProperty (',' objectProperty)*;
objectProperty: (IDENTIFIER | STRING_LITERAL) ':' (expression | handlersList);

handlersList: '[' handlerItem (',' handlerItem)* ']';
handlerItem: eventName ('{' objectPropertyList? '}')?;

argumentList: expression (',' expression)*;

// Primary expressions
primary
    : IDENTIFIER
    | STRING_LITERAL
    | NUMBER_LITERAL
    | TRUE
    | FALSE
    | NULL
    | 'this'
    | '(' expression ')'
    ;

type
    : 'string'
    | 'number'
    | 'boolean'
    | 'array' '<' type '>'
    | 'object'
    | 'any'
    | IDENTIFIER  // Custom types including classes and interfaces
    ;

CLASS: 'class';
EXTENDS: 'extends';
CONSTRUCTOR: 'constructor';
NEW: 'new';
NULL: 'null';
TRUE: 'true';
FALSE: 'false';
ON: 'on';
EMIT: 'emit';

// AI cognitive service keywords
LEARN: 'learn';
THINK: 'think';
AWAIT: 'await';
ADAPT: 'adapt';
NOT: 'not';

IDENTIFIER: [a-zA-Z_] [a-zA-Z_0-9]*;
STRING_LITERAL: '"' (~["\r\n] | '\\' .)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;

// Punctuation
SEMICOLON: ';';

// Whitespace and comments
WS: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
