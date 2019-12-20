parser grammar AutoStepParser;

options { tokenVocab=AutoStepLexer; }

file: NEWLINE*
      featureBlock+ // Multiple feature blocks aren't valid, 
                    // but we want to give an error later instead of failing
                    // the parse stage.
      WS? EOF;

featureBlock: annotations 
              featureDefinition
              backgroundBlock?
              featureBody;

annotations: annotation*;

annotation: WS? TAG NEWLINE          #tagAnnotation
          | WS? OPTION NEWLINE       #optionAnnotation
          | NEWLINE                  #blank
          ;

featureDefinition: WS? featureTitle NEWLINE
                   description?;
 
featureTitle: FEATURE WS? text;

featureBody: scenarioBlock*;

backgroundBlock: WS? BACKGROUND NEWLINE
                 backgroundBody;

backgroundBody: stepCollectionBodyLine*;

scenarioBlock: annotations
               scenarioDefinition
               scenarioBody;

scenarioDefinition: WS? scenarioTitle NEWLINE
                    description?;

scenarioTitle: SCENARIO WS? text;

scenarioBody: stepCollectionBodyLine*;


stepCollectionBodyLine: statementBlock 
                      | NEWLINE
                      ;

statementBlock: WS? statement STATEMENT_NEWLINE tableBlock #statementWithTable
              | WS? statement STATEMENT_NEWLINE            #statementLineTerminated
              | WS? statement WS? EOF                      #statementEofTerminated
              ;

statement: GIVEN statementBody #given
         | WHEN statementBody  #when
         | THEN statementBody  #then
         | AND statementBody   #and
         ;

statementBody: statementSection+;
            
statementSection: STATEMENT_SECTION                               # statementSectionPart
                | STATEMENT_WS                                    # statementWs
                | OPEN_QUOTE CLOSE_QUOTE                          # argEmpty
                | OPEN_QUOTE ARG_CURR_SYMBOL? ARG_FLOAT CLOSE_QUOTE       # argFloat
                | OPEN_QUOTE ARG_CURR_SYMBOL? ARG_INT CLOSE_QUOTE         # argInt       
                | OPEN_QUOTE ARG_COLON statementTextContentBlock CLOSE_QUOTE      # argInterpolate
                | OPEN_QUOTE statementTextContentBlock CLOSE_QUOTE            # argText
                ;

statementTextContentBlock: (
                            ARG_WS              |
                            ARG_TEXT_CONTENT    |
                            ARG_INT             |
                            ARG_FLOAT           |
                            ARG_CURR_SYMBOL     |
                            ARG_COLON           |
                            ESCAPE_QUOTE
                           )+;

tableBlock: WS? tableHeader
            (WS? tableRow)*;

tableHeader: tableHeaderCell+ ROW_END;

tableHeaderCell: (TABLE_START | CELL_DELIMITER) CELL_WS? tableCellTextBlock CELL_WS?;

tableRow: tableRowCell+ ROW_END;

tableRowCell: (TABLE_START | CELL_DELIMITER) CELL_WS? tableRowCellContent? CELL_WS?;

tableRowCellContent: CELL_CURR_SYMBOL? CELL_FLOAT          # cellFloat
                   | CELL_CURR_SYMBOL? CELL_INT            # cellInt       
                   | CELL_COLON tableCellTextBlock  # cellInterpolate
                   | tableCellTextBlock             # cellText
                   ;

tableCellTextBlock: (
                        CELL_WS              |
                        CELL_TEXT_CONTENT    |
                        CELL_INT             |
                        CELL_FLOAT           |
                        CELL_CURR_SYMBOL     |
                        CELL_COLON           |
                        ESCAPE_CELL_DELIMITER
                    )+?;

text: (WS? WORD)+;
line: WS? text? NEWLINE;
description: NEWLINE*
             line+
             NEWLINE*;