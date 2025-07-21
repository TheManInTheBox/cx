grammar Cx;

// Entry point
program: statement* EOF;

// Statements
statement
    : functionDeclaration
    | classDeclaration
    | interfaceDeclaration
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
    | onStatement
    | emitStatement
    ;

// Import statement for CX modules
importStatement: 'import' IDENTIFIER 'from' STRING_LITERAL ';';

// Uses statement for dependency injection inside classes
usesStatement: 'uses' IDENTIFIER 'from' dottedIdentifier ';';

dottedIdentifier: IDENTIFIER ('.' IDENTIFIER)*;

// Function declarations with optional async support
functionDeclaration
    : accessModifier? 'async'? 'function' IDENTIFIER '(' parameterList? ')' ('->' type)? blockStatement
    ;

// Class declarations with inheritance support (use : instead of implements)
classDeclaration
    : decorator* accessModifier? 'class' IDENTIFIER ('extends' IDENTIFIER)? (':' interfaceList)? classBody
    ;

// Decorator support for classes
decorator
    : '@' IDENTIFIER
    ;

classBody: '{' classMember* '}';

classMember
    : fieldDeclaration
    | methodDeclaration
    | constructorDeclaration
    | onStatement
    | usesStatement
    ;

fieldDeclaration: accessModifier? IDENTIFIER ':' type ('=' expression)? ';';

methodDeclaration: accessModifier? 'async'? 'function' IDENTIFIER '(' parameterList? ')' ('->' type)? blockStatement;

constructorDeclaration: accessModifier? 'constructor' '(' parameterList? ')' blockStatement;

// Interface declarations
interfaceDeclaration
    : accessModifier? 'interface' IDENTIFIER ('extends' interfaceList)? interfaceBody
    ;

interfaceBody: '{' interfaceMember* '}';

interfaceMember
    : interfaceMethodSignature
    | interfacePropertySignature
    ;

interfaceMethodSignature: IDENTIFIER '(' parameterList? ')' ('->' type)? ';';

interfacePropertySignature: IDENTIFIER ':' type ';';

interfaceList: IDENTIFIER (',' IDENTIFIER)*;

// Access modifiers
accessModifier: 'public' | 'private' | 'protected';

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

// Event-driven statements
eventNamePart: IDENTIFIER | 'any' | 'new' | 'critical' | 'assigned' | 'tickets' | 'tasks' | 'support' | 'dev' | 'system' | 'alerts' | 'user' | 'ai' | 'async' | 'sync';
eventName: eventNamePart ('.' eventNamePart)*;
onStatement: 'on' 'async'? eventName '(' IDENTIFIER ')' blockStatement;
emitStatement: 'emit' eventName (',' expression)? ';';

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
objectProperty: (IDENTIFIER | STRING_LITERAL) ':' expression;

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
INTERFACE: 'interface';
EXTENDS: 'extends';
IMPORT: 'import';
USES: 'uses';
CONSTRUCTOR: 'constructor';
PUBLIC: 'public';
PRIVATE: 'private';
PROTECTED: 'protected';
TRY: 'try';
CATCH: 'catch';
THROW: 'throw';
NEW: 'new';
NULL: 'null';
TRUE: 'true';
FALSE: 'false';
ON: 'on';
EMIT: 'emit';

IDENTIFIER: [a-zA-Z_] [a-zA-Z_0-9]*;
STRING_LITERAL: '"' (~["\r\n] | '\\' .)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;

// Punctuation
SEMICOLON: ';';

// Whitespace and comments
WS: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
