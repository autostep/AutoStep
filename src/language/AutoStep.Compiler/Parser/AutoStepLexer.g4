lexer grammar AutoStepLexer;

FEATURE: 'Feature:';
SCENARIO: 'Scenario:';
BACKGROUND: 'Background:';
TAG: '@' ~[ \t\r\n] ~[#\r\n]*;
OPTION: '$' ~[ \t\r\n] ~[#\r\n]*;
NEWLINE: [ \t]* '\r'? '\n';
WORD: ~[ #\t\r\n]+;
WS: [ \t]+;
TEXT_COMMENT: [ \t]* '#' ~[\r\n]* -> skip;
//BLANKBLOCK: NEWLINE NEWLINE+ -> channel(HIDDEN);

GIVEN: 'Given ';
WHEN: 'When ';
THEN: 'Then ';
AND: 'And ';