// ===== CX LANGUAGE GRAMMAR - CONSCIOUSNESS-FIRST PATTERNS =====
// ENFORCED PATTERNS:
// 1. Event names are IDENTIFIERS ONLY (no string literals)
// 2. ON statements MUST have event parameter: on eventName (param) { }
// 3. Conscious entities have NO inheritance - pure consciousness
// 4. realize() can have optional parameters for consciousness initialization
// 5. Object properties MUST have commas between entries
// 6. AI services use expression syntax for parameters

grammar Cx;

// Entry point
program: statement* EOF;

// Statements
statement
    : objectDeclaration
    | consciousDeclaration
    | variableDeclaration
    | expressionStatement
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

// Conscious entity declarations - ENFORCED: No inheritance, pure consciousness
consciousDeclaration
    : 'conscious' IDENTIFIER consciousBody
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

// realize() can optionally accept a single object parameter for consciousness initialization
realizeDeclaration: 'realize' '(' realizeParameter? ')' blockStatement;

// Parameter for realize() method - always an object type for consciousness data
realizeParameter: IDENTIFIER ':' 'object';

// Variable declarations
variableDeclaration: 'var' IDENTIFIER '=' expression ';';

// Cognitive Event-driven statements - ENFORCED PATTERNS
eventNamePart: IDENTIFIER;

// Event names are IDENTIFIERS ONLY - no strings allowed
eventName: eventNamePart ('.' eventNamePart)*;

// ON statements must have event parameter - NO -> syntax allowed
onStatement: 'on' 'async'? eventName '(' IDENTIFIER ')' blockStatement;

// EMIT statements - event names are identifiers, payloads are expressions
emitStatement: 'emit' eventName expression? ';';

// AI service statements - cognitive capabilities with parallel handlers
aiServiceName
    : 'iam' | 'learn' | 'think' | 'await' | 'adapt' | 'execute' | 'infer'
    ;
aiServiceStatement: aiServiceName aiServiceParameters ';';

// 🚀 ENHANCED AI SERVICE PARAMETERS - Supports parallel execution
aiServiceParameters: expression;

// Enhanced parameter structure for parallel handler detection
enhancedParameterList
    : (standardParameter | parallelHandlerParameter) (',' (standardParameter | parallelHandlerParameter))*
    ;

standardParameter: IDENTIFIER ':' expression;

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

// ENFORCED: Object properties must have commas between entries
objectPropertyList: objectProperty (',' objectProperty)*;
objectProperty: (IDENTIFIER | STRING_LITERAL) ':' (expression | handlersList | parallelHandlerParameter);

handlersList: '[' (eventHandlerReference | handlerItem) (',' (eventHandlerReference | handlerItem))* ']';
handlerItem: eventName ('{' objectPropertyList? '}')?;

// 🚀 PARALLEL HANDLER PARAMETERS - Critical Path Enhancement
parallelHandlerParameter: eventHandlerReference;
eventHandlerReference: IDENTIFIER ('.' IDENTIFIER)*;

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

// AI cognitive service keywords - RESERVED WORDS
LEARN: 'learn';
THINK: 'think';
AWAIT: 'await';
ADAPT: 'adapt';
IAM: 'iam';
REALIZE: 'realize';
INFER: 'infer';

// CONSCIOUSNESS KEYWORDS - RESERVED
CONSCIOUS: 'conscious';
EMIT: 'emit';
ON: 'on';

// OBJECT KEYWORDS  
OBJECT: 'object';
EXTENDS: 'extends';
NEW: 'new';

// LITERALS
NULL: 'null';
TRUE: 'true';
FALSE: 'false';

// ENFORCED: No '$', 'Random', 'if', 'for', 'while' - these are NOT part of CX
// CX uses pure event-driven patterns with consciousness-aware AI services

IDENTIFIER: [a-zA-Z_] [a-zA-Z_0-9]*;
STRING_LITERAL: '"' (~["\r\n] | '\\' .)* '"';
NUMBER_LITERAL: [0-9]+ ('.' [0-9]+)?;

// Punctuation
SEMICOLON: ';';

// FORBIDDEN CONSTRUCTS IN CX LANGUAGE:
// - '$' symbols (not part of CX syntax)
// - 'Random' class (use consciousness-aware data generation)
// - 'if' statements (use consciousness boolean logic with 'is {}' and 'not {}')
// - 'for'/'while' loops (use event-driven patterns)
// - 'Math' class (use AI services for calculations)
// - String event names in quotes (use identifier patterns)
// - Traditional OOP inheritance in conscious entities
// - Complex parameter lists beyond simple typed parameters

// Whitespace and comments
WS: [ \t\r\n]+ -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
