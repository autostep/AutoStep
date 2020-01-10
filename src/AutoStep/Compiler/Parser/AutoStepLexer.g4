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
STEP_DEFINE: S T E P ':' -> pushMode(definition);
BACKGROUND: 'Background:';
TAG: '@' ~[ \t\r\n] ~[#\r\n]*;
OPTION: '$' ~[ \t\r\n] ~[#\r\n]*;
NEWLINE: NL;
WORD: ~[ #\t\r\n|]+;
WS: SPACE+;
TEXT_COMMENT: SPACE* '#' ~[\r\n]* -> skip;
ESCAPED_TABLE_DELIMITER: '\\|';
TABLE_START: '|' -> pushMode(tableRow);

GIVEN: 'Given ' -> pushMode(statement);
WHEN: 'When ' -> pushMode(statement);
THEN: 'Then ' -> pushMode(statement);
AND: 'And ' -> pushMode(statement);

mode statement;
STATEMENT_ESCAPED_QUOTE: '\\\'';
STATEMENT_ESCAPED_DBLQUOTE: '\\"';
STATEMENT_ESCAPED_VARSTART: '\\<';
STATEMENT_ESCAPED_VAREND: '\\>';
STATEMENT_VAR_START: '<';
STATEMENT_VAR_STOP: '>';
STATEMENT_QUOTE: '\'';
STATEMENT_DOUBLE_QUOTE: '"';
STATEMENT_NEWLINE: STATEMENT_WS* (NL|EOF) -> popMode;
STATEMENT_INT: INT;
STATEMENT_FLOAT: FLOAT;
STATEMENT_SYMBOL: CURR_SYMBOL;
STATEMENT_WORD: ~[ :\\\t#<>\r\n]+;
STATEMENT_COLON: ':';
STATEMENT_WS: SPACE+;
STATEMENT_COMMENT: SPACE* '#' ~[\r\n]* -> skip;

mode definition;
DEF_GIVEN: 'Given';
DEF_WHEN: 'When';
DEF_THEN: 'Then';
DEF_ESCAPED_LCURLY: '\\{';
DEF_ESCAPED_RCURLY: '\\}';
DEF_LCURLY: '{';
DEF_RCURLY: '}';
DEF_NEWLINE: DEF_WS* (NL|EOF) -> popMode;
DEF_SYMBOL: CURR_SYMBOL;
DEF_WORD: ~[ :\\\t#{}\r\n]+;
DEF_WS: SPACE+;
DEF_COMMENT: SPACE* '#' ~[\r\n]* -> skip;
DEF_COLON: ':';

mode tableRow;
CELL_ESCAPED_VARSTART: '\\<';
CELL_ESCAPED_VAREND: '\\>';
CELL_VAR_START: '<';
CELL_VAR_STOP: '>';
CELL_WORD: ~[ :\\\t#<>\r\n]+;
CELL_COLON: ':';
CELL_ESCAPED_DELIMITER: '\\|';
CELL_DELIMITER: '|';
CELL_WS: SPACE+;
ROW_COMMENT: SPACE* '#' ~[\r\n]* -> skip;
ROW_NL: NL -> popMode;
