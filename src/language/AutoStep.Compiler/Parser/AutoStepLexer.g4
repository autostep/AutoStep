lexer grammar AutoStepLexer;

fragment NL: [ \t]* '\r'? '\n';

FEATURE: 'Feature:';
SCENARIO: S C E N A R I O ':';
BACKGROUND: 'Background:';
TAG: '@' ~[ \t\r\n] ~[#\r\n]*;
OPTION: '$' ~[ \t\r\n] ~[#\r\n]*;
NEWLINE: NL;
WORD: ~[ #\t\r\n]+;
WS: [ \t]+;
TEXT_COMMENT: [ \t]* '#' ~[\r\n]* -> skip;
//BLANKBLOCK: NEWLINE NEWLINE+ -> channel(HIDDEN);

GIVEN: 'Given ' -> pushMode(statement);
WHEN: 'When ' -> pushMode(statement);
THEN: 'Then ' -> pushMode(statement);
AND: 'And ' -> pushMode(statement);

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

mode statement;
ESCAPED_QUOTE: '\\\'';
OPEN_QUOTE: '\'' -> pushMode(argument);
STATEMENT_NEWLINE: STATEMENT_WS* (NL|EOF) -> popMode;
STATEMENT_SECTION: ~[ \t#'\r\n]+;
STATEMENT_WS: [ \t];
STATEMENT_COMMENT: [ \t]* '#' ~[\r\n]* -> skip;

mode argument;
FLOAT: DIGIT+ '.' DIGIT* // match 1. 39. 3.14159 etc...
     | '.' DIGIT+ // match .1 .14159
     ;
INT: DIGIT+;
ESCAPE_QUOTE: '\\\'';
CLOSE_QUOTE: '\'' -> popMode;
COLON: ':';
CURR_SYMBOL: DOLLAR | POUND;
TEXT_CONTENT: ~[\r\n]+?;
ARG_ERR_UNEXPECTEDTERMINATOR: NL;

fragment ESC: '\\\'' | '\\\\';
fragment DIGIT : [0-9] ; // match single digit
fragment DOLLAR : '$';
fragment POUND : '£' | '\u00A3'; // UTF8 and regular encoding of the value
