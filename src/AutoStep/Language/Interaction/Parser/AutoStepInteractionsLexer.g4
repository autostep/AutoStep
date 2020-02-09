lexer grammar AutoStepInteractionsLexer;

fragment SPACE: [ \t];
fragment NL: SPACE* '\r'? '\n';
fragment DIGIT : [0-9] ; // match single digit

APP_DEFINITION: 'App:';

// Exact match first
TRAIT_DEFINITION: 'Trait:';
COMPONENT_DEFINITION: 'Component:';
COLLECTION_DEFINITION: 'Collection:';

TRAITS_KEYWORD: 'traits:';
NAME_KEYWORD: 'name:';
COMPONENTS_KEYWORD: 'components:';
BASEDON_KEYWORD: 'based-on:';

STEP_DEFINE: 'Step:' -> pushMode(definition);

LIST_SEPARATOR: ',';
DEF_SEPARATOR: ':';

METHOD_OPEN: '(' -> pushMode(methodArgs);
NAME_REF: [a-zA-Z-]+;
PLUS: '+';

COMPONENT_INSERT: '$component$';

FUNC_PASS_MARKER: '->';

STRING: '"' (.| '\\"')+? '"';

NEWLINE: NL -> channel(HIDDEN);
TEXT_DOC_COMMENT: SPACE* '##' ~[\r\n]* -> channel(HIDDEN);
TEXT_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);
WS: SPACE+ -> channel(HIDDEN);

mode methodArgs;
METHOD_STRING_START: '"' -> pushMode(stringArg);
PARAM_NAME: [a-zA-Z-]+;
ARR_LEFT: '[';
ARR_RIGHT: ']';
PARAM_SEPARATOR: ',';
FLOAT : DIGIT+ '.' DIGIT* // match 1. 39. 3.14159 etc...
               | '.' DIGIT+ // match .1 .14159
               ;

INT : DIGIT+;
CONSTANT: [A-Z]+;
METHOD_CLOSE: ')' -> popMode;

mode stringArg;
STR_ANGLE_LEFT: '<' -> pushMode(stringVariable);
METHOD_STR_ESCAPE_QUOTE: '\\"';
METHOD_STRING_END: '"' -> popMode;
STR_CONTENT: .+?;

mode stringVariable;
STR_NAME_REF: NAME_REF;
STR_ANGLE_RIGHT: '>' -> popMode;

mode definition;
DEF_GIVEN: 'Given';
DEF_WHEN: 'When';
DEF_THEN: 'Then';
DEF_ESCAPED_LCURLY: '\\{';
DEF_ESCAPED_RCURLY: '\\}';
DEF_LCURLY: '{';
DEF_RCURLY: '}';
DEF_NEWLINE: DEF_WS* (NL|EOF) -> popMode;
DEF_WS: SPACE+ -> channel(HIDDEN);
DEF_COLON: ':';
DEF_COMPONENT_INSERT: '$component$';
DEF_WORD: ~[ :\\\t#{}\r\n]+;
DEF_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);