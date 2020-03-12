lexer grammar AutoStepLexer;

fragment SPACE: [ \t];
fragment NL: SPACE* '\r'? '\n';
fragment ESC: '\\\'' | '\\\\';
fragment DIGIT : [0-9] ; // match single digit
fragment FLOAT : DIGIT+ '.' DIGIT* // match 1. 39. 3.14159 etc...
               | '.' DIGIT+ // match .1 .14159
               ;
fragment INT : DIGIT+;
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

FEATURE: F E A T U R E ':' -> pushMode(name);
SCENARIO: S C E N A R I O ':' -> pushMode(name);
SCENARIO_OUTLINE: S C E N A R I O ' ' O U T L I N E ':' -> pushMode(name);
EXAMPLES: E X A M P L E S ':';
STEP_DEFINE: S T E P ':' -> pushMode(definition);
BACKGROUND: 'Background:';
TAG: '@' ~[ \t\r\n] ~[#\r\n]*;
OPTION: '$' ~[ \t\r\n] ~[#\r\n]*;

GIVEN: 'Given' -> pushMode(statement);
WHEN: 'When' -> pushMode(statement);
THEN: 'Then' -> pushMode(statement);
AND: 'And' -> pushMode(statement);

NEWLINE: NL;
WORD: ~[ #\t\r\n|]+;
WS: SPACE+;
TEXT_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);
ESCAPED_TABLE_DELIMITER: '\\|';
TABLE_START: '|' -> pushMode(tableRow);

mode name;
NAME_WORD: ~[ #\t\r\n]+ -> type(WORD);
NAME_WS: SPACE+ -> type(WS);
NAME_NEWLINE: WS* (NL|EOF) -> type(NEWLINE), popMode;
NAME_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);

mode statement;
STATEMENT_ESCAPED_QUOTE: '\\\'';
STATEMENT_ESCAPED_DBLQUOTE: '\\"';
STATEMENT_ESCAPED_VARSTART: '\\<';
STATEMENT_ESCAPED_VAREND: '\\>';
STATEMENT_VAR_START: '<';
STATEMENT_VAR_STOP: '>';
STATEMENT_QUOTE: '\'';
STATEMENT_DOUBLE_QUOTE: '"';
STATEMENT_NEWLINE: STATEMENT_WS* (NL|EOF) -> type(NEWLINE), popMode;
STATEMENT_FLOAT: FLOAT;
STATEMENT_INT: INT;
STATEMENT_COLON: ':';
STATEMENT_WS: SPACE+ -> type(WS);
STATEMENT_WORD: ~[ :\\\t#<>\r\n'"0-9]+;
STATEMENT_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);

mode definition;
DEF_GIVEN: 'Given';
DEF_WHEN: 'When';
DEF_THEN: 'Then';
DEF_ESCAPED_LCURLY: '\\{';
DEF_ESCAPED_RCURLY: '\\}';
DEF_LCURLY: '{';
DEF_RCURLY: '}';
DEF_NEWLINE: DEF_WS* (NL|EOF) -> type(NEWLINE), popMode;
DEF_WS: SPACE+ -> type(WS);
DEF_COLON: ':';
DEF_WORD: ~[ :\\\t#{}\r\n]+;
DEF_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);

mode tableRow;
CELL_ESCAPED_VARSTART: '\\<';
CELL_ESCAPED_VAREND: '\\>';
CELL_VAR_START: '<';
CELL_VAR_STOP: '>';
CELL_WORD: ~[ :\\\t#<>\r\n0-9|]+;
CELL_FLOAT: FLOAT;
CELL_INT: INT;
CELL_COLON: ':';
CELL_ESCAPED_DELIMITER: '\\|';
CELL_DELIMITER: '|';
CELL_WS: SPACE+ -> type(WS);
ROW_COMMENT: SPACE* '#' ~[\r\n]* -> channel(HIDDEN);
ROW_NL: SPACE* (NL|EOF) -> type(NEWLINE), popMode;
