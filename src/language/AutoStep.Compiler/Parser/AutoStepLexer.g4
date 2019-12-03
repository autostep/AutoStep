lexer grammar AutoStepLexer;

FEATURE: 'Feature:';
SCENARIO: 'Scenario:';
BACKGROUND: 'Background:';
TAG: '@' [a-zA-Z0-9_]+;
OPTION: '$' [a-zA-Z0-9_]+;
NEWLINE: [ \t]* '\r'? '\n';
WORD: ~[ #\t\r\n]+;
WS: [ \t]+;
TEXT_COMMENT: [ \t]* '#' ~[\r\n]* -> skip;
//BLANKBLOCK: NEWLINE NEWLINE+ -> channel(HIDDEN);


GIVEN: 'Given ';
WHEN: 'When ';
THEN: 'Then ';
AND: 'And ';