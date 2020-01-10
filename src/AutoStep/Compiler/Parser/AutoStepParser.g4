parser grammar AutoStepParser;

options { tokenVocab=AutoStepLexer; }

file: NEWLINE*
      (featureBlock|stepDefinitionBlock)+ // Multiple feature blocks aren't valid, 
                                          // but we want to give an error later instead of failing
                                          // the parse stage.
      WS? EOF;


stepDefinitionBlock: annotations
                     stepDefinition
                     stepDefinitionBody;

stepDefinition: WS? STEP_DEFINE WS? stepDeclaration DEF_NEWLINE
                    description?;

stepDefinitionBody: stepCollectionBodyLine*;

stepDeclaration: DEF_GIVEN stepDeclarationBody #declareGiven
               | DEF_WHEN stepDeclarationBody  #declareWhen
               | DEF_THEN stepDeclarationBody  #declareThen
               ;

stepDeclarationBody: stepDeclarationSection+;

stepDeclarationSection: DEF_LCURLY stepDeclarationArgument DEF_RCURLY       # declarationArgument
                      | stepDeclarationSectionContent                       # declarationSection;

stepDeclarationArgument: stepDeclarationArgumentName (DEF_COLON stepDeclarationTypeHint)?
                       ;

stepDeclarationArgumentName: DEF_WORD;

stepDeclarationTypeHint: DEF_WORD;

stepDeclarationSectionContent: DEF_WORD   # declarationWord
                             | (DEF_ESCAPED_LCURLY | DEF_ESCAPED_RCURLY) # declarationEscaped
                             | DEF_WS                 # declarationWs
                             | DEF_COLON              # declarationColon
                             ;

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
              | WS? statement STATEMENT_NEWLINE                     #statementLineTerminated
              | WS? statement WS? EOF                               #statementEofTerminated
              ;

statement: GIVEN statementBody #given
         | WHEN statementBody  #when
         | THEN statementBody  #then
         | AND statementBody   #and
         ;

statementBody: statementSection+;
            
statementSection: (STATEMENT_QUOTE statementSectionBlock* STATEMENT_QUOTE |
                   STATEMENT_DOUBLE_QUOTE statementSectionBlock* STATEMENT_DOUBLE_QUOTE
                  )                         # statementQuotedString                
                  | statementSectionBlock   # statementSingleBlock
                  ;

statementSectionBlock: STATEMENT_WORD                                                # statementWord
                     | (
                             STATEMENT_ESCAPED_QUOTE
                           | STATEMENT_ESCAPED_DBLQUOTE
                           | STATEMENT_ESCAPED_VARSTART
                           | STATEMENT_ESCAPED_VAREND
                       )                                                             # statementEscapedChar
                     | STATEMENT_VAR_START statementVariableName STATEMENT_VAR_STOP  # statementVariable
                     | STATEMENT_SYMBOL? STATEMENT_INT                               # statementInt
                     | STATEMENT_SYMBOL? STATEMENT_FLOAT                             # statementFloat
                     | STATEMENT_SYMBOL                                              # statementSymbol
                     | STATEMENT_COLON STATEMENT_WORD                                # statementInterpolate
                     | STATEMENT_WS                                                  # statementBlockWs                     
                     ;

statementVariableName: STATEMENT_WORD (STATEMENT_WS STATEMENT_WORD)*;

examples: exampleBlock*;

exampleBlock: annotations
              WS? EXAMPLES NEWLINE+
              tableBlock;

tableBlock: WS? tableHeader
            (WS? tableRow | NEWLINE)*;

tableHeader: tableHeaderCell+ CELL_DELIMITER ROW_NL;

tableHeaderCell: (TABLE_START | CELL_DELIMITER) CELL_WS? cellVariableName? CELL_WS?;

tableRow: tableRowCell+ CELL_DELIMITER ROW_NL;

tableRowCell: (TABLE_START | CELL_DELIMITER) CELL_WS? tableRowCellContent? CELL_WS?;

tableRowCellContent: (CELL_WS? cellContentBlock)+;

cellContentBlock: CELL_WORD                                         # cellWord
                  | 
                        (
                          CELL_ESCAPED_DELIMITER
                        | CELL_ESCAPED_VARSTART
                        | CELL_ESCAPED_VAREND
                        )                                           # cellEscapedChar
                  | CELL_VAR_START cellVariableName CELL_VAR_STOP   # cellVariable
                  | CELL_COLON CELL_WORD                            # cellInterpolate                  
                  ;

cellVariableName: CELL_WORD (CELL_WS CELL_WORD)*;

text: (WS? WORD)+;
line: WS? text? NEWLINE;
description: NEWLINE*
             line+
             NEWLINE*;