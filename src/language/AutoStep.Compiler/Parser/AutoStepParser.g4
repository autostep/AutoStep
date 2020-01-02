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
               scenarioBody
               examples;

scenarioDefinition: WS? scenarioTitle NEWLINE
                    description?;

scenarioTitle: SCENARIO WS? text            #normalScenarioTitle
             | SCENARIO_OUTLINE WS? text    #scenarioOutlineTitle
             ;

scenarioBody: stepCollectionBodyLine*;

stepCollectionBodyLine: statementBlock 
                      | NEWLINE
                      ;

statementBlock: WS? statement STATEMENT_NEWLINE NEWLINE* tableBlock #statementWithTable
              | WS? statement STATEMENT_NEWLINE            #statementLineTerminated
              | WS? statement WS? EOF                      #statementEofTerminated
              ;

statement: GIVEN statementBody #given
         | WHEN statementBody  #when
         | THEN statementBody  #then
         | AND statementBody   #and
         ;

statementBody: statementSection+;
            
statementSection: STATEMENT_SECTION                                   # statementSectionPart
                | STATEMENT_WS                                        # statementWs
                | OPEN_QUOTE CLOSE_QUOTE                              # argEmpty
                | OPEN_QUOTE ARG_CURR_SYMBOL? ARG_FLOAT CLOSE_QUOTE   # argFloat
                | OPEN_QUOTE ARG_CURR_SYMBOL? ARG_INT CLOSE_QUOTE     # argInt
                | OPEN_QUOTE ARG_COLON statementArgument CLOSE_QUOTE  # argInterpolate
                | OPEN_QUOTE statementArgument CLOSE_QUOTE            # argText
                ;

statementArgument: statementArgumentBlock+;

statementArgumentBlock:  ARG_EXAMPLE_START argumentExampleNameBody ARG_EXAMPLE_END #exampleArgBlock
                      |  argumentBody+?                                            #textArgBlock
                      ;

argumentExampleNameBody: argumentExampleNameBodyContent (ARG_WS? argumentExampleNameBodyContent)*;

argumentExampleNameBodyContent: ARG_TEXT_CONTENT
                              | ARG_INT
                              | ARG_FLOAT
                              | ARG_COLON
                              | ARG_CURR_SYMBOL
                              | ARG_ESCAPE_QUOTE
                              | ARG_EXAMPLE_START
                              | ARG_EXAMPLE_START_ESCAPE
                              | ARG_EXAMPLE_END_ESCAPE
                              ;

argumentBody: ARG_WS              
            | ARG_TEXT_CONTENT    
            | ARG_INT             
            | ARG_FLOAT           
            | ARG_CURR_SYMBOL     
            | ARG_COLON           
            | ARG_ESCAPE_QUOTE
            | ARG_EXAMPLE_START_ESCAPE
            | ARG_EXAMPLE_END_ESCAPE
            | ARG_EXAMPLE_START
            | ARG_EXAMPLE_END
            ;

examples: exampleBlock*;

exampleBlock: annotations
              WS? EXAMPLES NEWLINE+
              tableBlock;

tableBlock: WS? tableHeader
            (WS? tableRow | NEWLINE)*;

tableHeader: tableHeaderCell+ CELL_DELIMITER ROW_NL;

tableHeaderCell: (TABLE_START | CELL_DELIMITER) CELL_WS? headerCell? CELL_WS?;

tableRow: tableRowCell+ CELL_DELIMITER ROW_NL;

tableRowCell: (TABLE_START | CELL_DELIMITER) CELL_WS? tableRowCellContent? CELL_WS?;

tableRowCellContent: CELL_CURR_SYMBOL? CELL_FLOAT  # cellFloat
                   | CELL_CURR_SYMBOL? CELL_INT    # cellInt       
                   | CELL_COLON cellArgument       # cellInterpolate
                   | cellArgument                  # cellText;

headerCell: headerCellBody+?;

cellArgument: cellArgumentBlock+?;

cellArgumentBlock: CELL_EXAMPLE_START cellExampleNameBody CELL_EXAMPLE_END #exampleCellBlock
                 | generalCellBody+?                                     #textCellBlock
                 ;

headerCellBody: CELL_WS
              | CELL_TEXT_CONTENT
              | CELL_INT
              | CELL_FLOAT
              | CELL_CURR_SYMBOL
              | CELL_COLON
              | ESCAPE_CELL_DELIMITER
              ;

cellExampleNameBody: cellExampleNameBodyContent (CELL_WS? cellExampleNameBodyContent)*;

cellExampleNameBodyContent: CELL_TEXT_CONTENT
                          | CELL_INT
                          | CELL_FLOAT
                          | CELL_COLON
                          | CELL_CURR_SYMBOL
                          | ESCAPE_CELL_DELIMITER
                          | CELL_EXAMPLE_START
                          | CELL_EXAMPLE_START_ESCAPE
                          | CELL_EXAMPLE_END_ESCAPE
                          ;

generalCellBody: CELL_WS              
               | CELL_TEXT_CONTENT    
               | CELL_INT             
               | CELL_FLOAT           
               | CELL_CURR_SYMBOL     
               | CELL_COLON           
               | ESCAPE_CELL_DELIMITER
               | CELL_EXAMPLE_START_ESCAPE
               | CELL_EXAMPLE_END_ESCAPE
               | CELL_EXAMPLE_START
               | CELL_EXAMPLE_END
               ;

text: (WS? WORD)+;
line: WS? text? NEWLINE;
description: NEWLINE*
             line+
             NEWLINE*;