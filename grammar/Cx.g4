grammar Cx;

// Entry point
program: statement* EOF;

// Statements
statement
    : objectDeclaration
    | consciousDeclaration
    | variableDeclaration
    | expressionStatement
    | forStatement
    | blockStatement
    | onStatement
    | emitStatement
    | aiServiceStatement
    ;

dottedIdentifier: IDENTIFIER ('.' IDENTIFIER)*;

// Object declarations with inheritance support (use : instead of implements)
objectDeclaration
    : 'object' IDENTIFIER ('extends' IDENTIFIER)? objectBody
    ;

// Conscious entity declarations - intelligent, self-aware entities
consciousDeclaration
    : 'conscious' IDENTIFIER (':' IDENTIFIER)? consciousBody
    ;

// Decorator support for objects
decorator
    : '@' IDENTIFIER
    ;

objectBody: '{' objectMember* '}';

consciousBody: '{' consciousMember* '}';

objectMember
    : realizeDeclaration
    | onStatement
    ;

consciousMember
    : realizeDeclaration
    | onStatement
    ;

realizeDeclaration: 'realize' '(' parameterList? ')' blockStatement;

parameterList: parameter (',' parameter)*;
parameter: IDENTIFIER (':' type)?;  // Make type optional

// Variable declarations
variableDeclaration: 'var' IDENTIFIER '=' expression ';';

// For-in loop for iteration (arrays, dictionaries, collections)
forStatement: 'for' '(' ('var' IDENTIFIER | IDENTIFIER) 'in' expression ')' statement;

// Cognitive Event-driven statements
eventNamePart: IDENTIFIER | 'any' | 'critical' | 'assigned' | 'tickets' | 'tasks' | 'support' | 'dev' | 'system' | 'alerts' | 'ai' | 'sync' | 'learn' | 'think' | 'generate' | 'chat' | 'communicate' | 'search' | 'execute' | 'speak' | 'listen' | 'image' | 'analyze' | 'transcribe' | 'audio' | 'await' | 'completed' | 'ready' | 'activation' | 'timing' | 'decision' | 'work' | 'new' | 'iam';
eventName: eventNamePart ('.' eventNamePart)*;
onStatement: 'on' 'async'? eventName '(' IDENTIFIER ')' blockStatement;
emitStatement: 'emit' eventName ((',' expression) | expression)? ';';

// AI service statements - cognitive capabilities only
aiServiceName
    : 'is' | 'not' | 'iam' | 'learn' | 'think' | 'await' | 'adapt' | 'execute'
    ;
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
    | 'conscious'
    | 'any'
    | IDENTIFIER  // Custom types including objects and interfaces
    ;

OBJECT: 'object';
CONSCIOUS: 'conscious';
EXTENDS: 'extends';
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
IAM: 'iam';
REALIZE: 'realize';

IDENTIFIER: [a-zA-Z_] [a-zA-Z_0-9]*;
STRING_LITERAL: '"' (~["\r\n] | '\\' .)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;

// Punctuation
SEMICOLON: ';';

// Whitespace and comments
WS: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
