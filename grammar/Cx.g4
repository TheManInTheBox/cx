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

importStatement: 'using' IDENTIFIER 'from' STRING_LITERAL ';';

// Function declarations with optional async support
functionDeclaration
    : accessModifier? 'async'? 'function' IDENTIFIER '(' parameterList? ')' ('->' type)? blockStatement
    ;

// Class declarations with inheritance support
classDeclaration
    : accessModifier? 'class' IDENTIFIER ('extends' IDENTIFIER)? ('implements' interfaceList)? classBody
    ;

classBody: '{' classMember* '}';

classMember
    : fieldDeclaration
    | methodDeclaration
    | constructorDeclaration
    | onStatement
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
eventNamePart: IDENTIFIER | 'any' | 'agent' | 'new' | 'critical' | 'assigned' | 'tickets' | 'tasks' | 'support' | 'dev' | 'system' | 'alerts';
eventName: eventNamePart ('.' eventNamePart)*;
onStatement: 'on' eventName '(' IDENTIFIER ')' blockStatement;
emitStatement: 'emit' eventName (',' expression)? ';';

// Blocks
blockStatement: '{' statement* '}';
expressionStatement: expression ';';

// Expressions
expression
    : primary                                           # PrimaryExpression
    | 'new' IDENTIFIER '(' argumentList? ')'           # NewExpression
    | 'agent' IDENTIFIER '(' argumentList? ')'         # AgentExpression
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
    | SELF
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
IMPLEMENTS: 'implements';
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
AGENT: 'agent';

// Thread context keyword - provides access to current thread context and state
SELF: 'self';

IDENTIFIER: [a-zA-Z_] [a-zA-Z_0-9]*;
STRING_LITERAL: '"' (~["\r\n] | '\\' .)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;

// Punctuation
SEMICOLON: ';';

// Whitespace and comments
WS: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
