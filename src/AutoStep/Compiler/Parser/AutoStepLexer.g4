lexer grammar AutoStepLexer;

fragment SPACE: [ \t];
fragment NL: SPACE* '\r'? '\n';
fragment ESC: '\\\'' | '\\\\';
fragment DIGIT : [0-9] ; // match single digit
fragment DOLLAR : '$';
fragment POUND : '£' | '\u00A3'; // UTF8 and regular encoding of the value
fragment FLOAT : DIGIT+ '.' DIGIT* // match 1. 39. 3.14159 etc...
               | '.' DIGIT+ // match .1 .14159
               ;
fragment INT : DIGIT+;
fragment CURR_SYMBOL: DOLLAR | POUND;
fragment TEXT_CONTENT: ~[ \t\r\n]+?;


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

FEATURE: F E A T U R E ':';
SCENARIO: S C E N A R I O ':';
SCENARIO_OUTLINE: S C E N A R I O ' ' O U T L I N E ':';
EXAMPLES: E X A M P L E S ':';
STEP_DEFINE: S T E P ':';
BACKGROUND: 'Background:';
TAG: '@' ~[ \t\r\n] ~[#\r\n]*;
OPTION: '$' ~[ \t\r\n] ~[#\r\n]*;
NEWLINE: NL;
WORD: ~[ #\t\r\n|]+;
WS: SPACE+;
TEXT_COMMENT: SPACE* '#' ~[\r\n]* -> skip;
ESCAPED_TABLE_DELIMITER: '\\|';
TABLE_START: '|' -> pushMode(tableRow);

//BLANKBLOCK: NEWLINE NEWLINE+ -> channel(HIDDEN);

GIVEN: 'Given ' -> pushMode(statement);
WHEN: 'When ' -> pushMode(statement);
THEN: 'Then ' -> pushMode(statement);
AND: 'And ' -> pushMode(statement);


mode statement;
ESCAPED_QUOTE: '\\\'';
OPEN_QUOTE: '\'' -> pushMode(argument);
STATEMENT_NEWLINE: STATEMENT_WS* (NL|EOF) -> popMode;
STATEMENT_SECTION: ~[ \t#'\r\n]+;
STATEMENT_WS: SPACE;
STATEMENT_COMMENT: SPACE* '#' ~[\r\n]* -> skip;

mode argument;
ARG_FLOAT: FLOAT;
ARG_INT: INT;
ARG_ESCAPE_QUOTE: '\\\'';
CLOSE_QUOTE: '\'' -> popMode;
ARG_EXAMPLE_START_ESCAPE: '\\<';
ARG_EXAMPLE_START: '<';
ARG_EXAMPLE_END_ESCAPE: '\\>';
ARG_EXAMPLE_END: '>';
ARG_COLON: ':';
ARG_CURR_SYMBOL: CURR_SYMBOL;
ARG_TEXT_CONTENT: TEXT_CONTENT;
ARG_WS: SPACE+;
ARG_ERR_UNEXPECTEDTERMINATOR: NL;

mode tableRow;
CELL_FLOAT: FLOAT;
CELL_INT: INT;
ESCAPE_CELL_DELIMITER: '\\|';
CELL_DELIMITER: '|';
CELL_COLON: ':';
CELL_CURR_SYMBOL: CURR_SYMBOL;
CELL_EXAMPLE_START_ESCAPE: '\\<';
CELL_EXAMPLE_START: '<';
CELL_EXAMPLE_END_ESCAPE: '\\>';
CELL_EXAMPLE_END: '>';
CELL_TEXT_CONTENT: TEXT_CONTENT;
CELL_WS: SPACE+;
ROW_COMMENT: SPACE* '#' ~[\r\n]* -> skip;
ROW_NL: NL -> popMode;
