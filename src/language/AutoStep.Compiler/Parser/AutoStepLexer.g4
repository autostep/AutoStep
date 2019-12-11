lexer grammar AutoStepLexer;

FEATURE: 'Feature:';
SCENARIO: S C E N A R I O ':';
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

fragment A : [aA]; // match either an 'a' or 'A'
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];
