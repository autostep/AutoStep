lexer grammar AutoStepInteractionsLexer;

fragment SPACE: [ \t];
fragment NL: SPACE* '\r'? '\n';
fragment DIGIT : [0-9] ; // match single digit

FLOAT : DIGIT+ '.' DIGIT* // match 1. 39. 3.14159 etc...
               | '.' DIGIT+ // match .1 .14159
               ;

INT : DIGIT+;


STRING: '\'' (.| '\\\'')+? '\'';

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

METHOD_OPEN: '(';
METHOD_CLOSE: ')';
NAME_REF: [a-zA-Z-]+;

ARR_LEFT: '[';
ARR_RIGHT: ']';

PLUS: '+';

COMPONENT_INSERT: '$component$';

FUNC_PASS_MARKER: '->';

CONSTANT: [A-Z]+;

NEWLINE: NL -> channel(HIDDEN);
TEXT_DOC_COMMENT: SPACE* '##' ~[\r\n]* -> channel(HIDDEN);
TEXT_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);
WS: SPACE+ -> channel(HIDDEN);

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