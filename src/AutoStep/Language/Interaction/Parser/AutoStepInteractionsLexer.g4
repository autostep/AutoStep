lexer grammar AutoStepInteractionsLexer;

fragment SPACE: [ \t];
fragment NL: SPACE* '\r'? '\n';
fragment DIGIT : [0-9] ; // match single digit
fragment VAR_NAME : [A-Za-z] [A-Za-z0-9-]*;

fragment COMMENT: SPACE* '#' ~[\r\n]*;
fragment DOC_COMMENT: SPACE* '##' ~[\r\n]*;

APP_DEFINITION: 'App:';

// Exact match first
TRAIT_DEFINITION: 'Trait:';
COMPONENT_DEFINITION: 'Component:';

TRAITS_KEYWORD: 'traits:';
NAME_KEYWORD: 'name:';
INHERITS_KEYWORD: 'inherits:';
COMPONENTS_KEYWORD: 'components:';

STEP_DEFINE: 'Step:' -> pushMode(definition);

LIST_SEPARATOR: ',';
DEF_SEPARATOR: ':';

NEEDS_DEFINING: 'needs-defining';

METHOD_OPEN: '(' -> pushMode(methodArgs);
NAME_REF: VAR_NAME;
PLUS: '+';

FUNC_PASS_MARKER: '->';

STRING: '"' (.| '\\"')+? '"';
STRING_SINGLE: '\'' (.| '\\\'')+? '\'' -> type(STRING);

NEWLINE: NL -> channel(HIDDEN);
TEXT_DOC_COMMENT: DOC_COMMENT -> channel(HIDDEN);
TEXT_COMMENT: COMMENT -> channel(HIDDEN);
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
METH_ARGS_NEWLINE: NL -> type(NEWLINE), channel(HIDDEN);
ARGS_TEXT_COMMENT: COMMENT -> type(TEXT_COMMENT), channel(HIDDEN);
ERR_CHAR_METHOD: . -> type(ERR_CHAR), popMode;

mode stringArg;
STR_ANGLE_LEFT: '<' -> pushMode(stringVariable);
METHOD_STR_ESCAPE_QUOTE: '\\"';
METHOD_STRING_END: '"' -> popMode;
// If we get a new line in the middle of a string, things are obviously pretty broken.
// Reset to the default mode to attempt to recover.
METHOD_STRING_ERRNL: ('\r'? '\n' | EOF) -> mode(DEFAULT_MODE);
STR_CONTENT: ~[<\\"\r\n]+;

mode stringArgSingle;
SINGLE_STR_ANGLE_LEFT: '<' -> type(STR_ANGLE_LEFT), pushMode(stringVariable);
SINGLE_METHOD_STR_ESCAPE_QUOTE: '\\\'' -> type(METHOD_STR_ESCAPE_QUOTE);
SINGLE_METHOD_STRING_END: '\'' -> type(METHOD_STRING_END), popMode;
// If we get a new line in the middle of a string, things are obviously pretty broken.
// Reset to the default mode to attempt to recover.
SINGLE_METHOD_STRING_ERRNL: ('\r'? '\n' | EOF) -> type(METHOD_STRING_ERRNL), mode(DEFAULT_MODE);
SINGLE_STR_CONTENT: ~[<\\'\r\n]+ -> type(STR_CONTENT);

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
DEF_COMMENT: COMMENT -> type(TEXT_COMMENT), channel(HIDDEN);