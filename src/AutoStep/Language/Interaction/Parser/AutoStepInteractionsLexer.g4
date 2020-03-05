lexer grammar AutoStepInteractionsLexer;

fragment SPACE: [ \t];
fragment NL: SPACE* '\r'? '\n';
fragment DIGIT : [0-9] ; // match single digit
fragment VAR_NAME : [A-Za-z] [A-Za-z0-9-]*;

APP_DEFINITION: 'App:';

// Exact match first
TRAIT_DEFINITION: 'Trait:';
COMPONENT_DEFINITION: 'Component:';
COLLECTION_DEFINITION: 'Collection:';

TRAITS_KEYWORD: 'traits:';
NAME_KEYWORD: 'name:';
COMPONENTS_KEYWORD: 'components:';
INHERITS_KEYWORD: 'inherits:';

STEP_DEFINE: 'Step:' -> pushMode(definition);

LIST_SEPARATOR: ',';
DEF_SEPARATOR: ':';

NEEDS_DEFINING: 'needs-defining';

METHOD_OPEN: '(' -> pushMode(methodArgs);
NAME_REF: VAR_NAME;
PLUS: '+';

COMPONENT_INSERT: '$component$';

FUNC_PASS_MARKER: '->';

STRING: '"' (.| '\\"')+? '"';
STRING_SINGLE: '\'' (.| '\\\'')+? '\'' -> type(STRING);

NEWLINE: NL -> channel(HIDDEN);
TEXT_DOC_COMMENT: SPACE* '##' ~[\r\n]* -> channel(HIDDEN);
TEXT_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);
WS: SPACE+ -> channel(HIDDEN);
ERR_CHAR: .;

mode methodArgs;
METHOD_STRING_START: '"' -> pushMode(stringArg);
METHOD_STRING_SINGLE_START: '\'' -> type(METHOD_STRING_START), pushMode(stringArgSingle);
CONSTANT: [A-Z]+;
PARAM_NAME: VAR_NAME;
ARR_LEFT: '[';
ARR_RIGHT: ']';
PARAM_SEPARATOR: ',';
FLOAT : DIGIT+ '.' DIGIT* // match 1. 39. 3.14159 etc...
               | '.' DIGIT+ // match .1 .14159
               ;

INT : DIGIT+;
METHOD_CLOSE: ')' -> popMode;
METH_WS: SPACE+ -> channel(HIDDEN);
ERR_CHAR_METHOD: . -> type(ERR_CHAR);

mode stringArg;
STR_ANGLE_LEFT: '<' -> pushMode(stringVariable);
METHOD_STR_ESCAPE_QUOTE: '\\"';
METHOD_STRING_END: '"' -> popMode;
STR_CONTENT: ~[<\\"]+;

mode stringArgSingle;
SINGLE_STR_ANGLE_LEFT: '<' -> type(STR_ANGLE_LEFT), pushMode(stringVariable);
SINGLE_METHOD_STR_ESCAPE_QUOTE: '\\\'' -> type(METHOD_STR_ESCAPE_QUOTE);
SINGLE_METHOD_STRING_END: '\'' -> type(METHOD_STRING_END), popMode;
SINGLE_STR_CONTENT: ~[<\\']+ -> type(STR_CONTENT);

mode stringVariable;
STR_NAME_REF: VAR_NAME;
STR_ANGLE_RIGHT: '>' -> popMode;
ERR_CHAR_STR_VAR: . -> type(ERR_CHAR);

mode definition;
DEF_GIVEN: 'Given';
DEF_WHEN: 'When';
DEF_THEN: 'Then';
DEF_ESCAPED_LCURLY: '\\{';
DEF_ESCAPED_RCURLY: '\\}';
DEF_LCURLY: '{';
DEF_RCURLY: '}';
DEF_NEWLINE: DEF_WS* (NL|EOF) -> popMode;
DEF_WS: SPACE+;
DEF_COLON: ':';
DEF_COMPONENT_INSERT: '$component$';
DEF_WORD: ~[ :\\\t#{}\r\n]+;
DEF_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);