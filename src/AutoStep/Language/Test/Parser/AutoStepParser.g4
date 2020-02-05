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

stepDefinition: WS? STEP_DEFINE DEF_WS? stepDeclaration (DEF_NEWLINE | EOF)
                    description?;

stepDefinitionBody: stepCollectionBodyLine*;

stepDeclaration: DEF_GIVEN DEF_WS? stepDeclarationBody #declareGiven
               | DEF_WHEN DEF_WS? stepDeclarationBody  #declareWhen
               | DEF_THEN DEF_WS? stepDeclarationBody  #declareThen
               ;

stepDeclarationBody: stepDeclarationSection+;

stepDeclarationSection: DEF_LCURLY stepDeclarationArgument DEF_RCURLY       # declarationArgument
                      | stepDeclarationSectionContent                       # declarationSection;

stepDeclarationArgument: stepDeclarationArgumentName (DEF_COLON stepDeclarationTypeHint)?
                       ;

stepDeclarationArgumentName: DEF_WORD;

stepDeclarationTypeHint: DEF_WORD;

stepDeclarationSectionContent: (DEF_WORD | DEF_GIVEN | DEF_WHEN | DEF_THEN)   # declarationWord
                             | (DEF_ESCAPED_LCURLY | DEF_ESCAPED_RCURLY)      # declarationEscaped
                             | DEF_WS                                         # declarationWs
                             | DEF_COLON                                      # declarationColon
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

statement: GIVEN STATEMENT_WS statementBody #given
         | WHEN STATEMENT_WS statementBody  #when
         | THEN STATEMENT_WS statementBody  #then
         | AND STATEMENT_WS statementBody   #and
         ;

statementBody: statementSection+;
            
statementSection:   STATEMENT_QUOTE                                               # statementQuote
                  | STATEMENT_DOUBLE_QUOTE                                        # statementDoubleQuote
                  | STATEMENT_VAR_START statementVariableName STATEMENT_VAR_STOP  # statementVariable
                  | (
                        STATEMENT_ESCAPED_QUOTE
                      | STATEMENT_ESCAPED_DBLQUOTE
                      | STATEMENT_ESCAPED_VARSTART
                      | STATEMENT_ESCAPED_VAREND
                    )                                                             # statementEscapedChar
                  | STATEMENT_INT                                                 # statementInt
                  | STATEMENT_FLOAT                                               # statementFloat
                  | STATEMENT_COLON STATEMENT_WORD                                # statementInterpolate
                  | STATEMENT_COLON                                               # statementColon
                  | STATEMENT_WORD                                                # statementWord
                  | (STATEMENT_VAR_START | STATEMENT_VAR_STOP)                    # statementVarUnmatched  
                  | STATEMENT_WS                                                  # statementBlockWs
                  ;

statementVariableName: statementVarPhrase (STATEMENT_WS statementVarPhrase)*;

statementVarPhrase: (STATEMENT_WORD | STATEMENT_INT)+;

examples: exampleBlock*;

exampleBlock: annotations
              WS? EXAMPLES NEWLINE+
              tableBlock;

tableBlock: WS? tableHeader
            (WS? tableRow | NEWLINE)*;

tableHeader: tableHeaderCell+ CELL_DELIMITER (ROW_NL | EOF);

tableHeaderCell: (TABLE_START | CELL_DELIMITER) CELL_WS? cellVariableName? CELL_WS?;

tableRow: tableRowCell+ CELL_DELIMITER (ROW_NL | EOF);

tableRowCell: (TABLE_START | CELL_DELIMITER) CELL_WS? tableRowCellContent? CELL_WS?;

tableRowCellContent: (CELL_WS? cellContentBlock)+;

cellContentBlock:       (
                          CELL_ESCAPED_DELIMITER
                        | CELL_ESCAPED_VARSTART
                        | CELL_ESCAPED_VAREND
                        )                                           # cellEscapedChar
                  | CELL_VAR_START cellVariableName CELL_VAR_STOP   # cellVariable
                  | CELL_INT                                        # cellInt
                  | CELL_FLOAT                                      # cellFloat
                  | CELL_COLON CELL_WORD                            # cellInterpolate
                  | CELL_COLON                                      # cellColon
                  | CELL_WORD                                       # cellWord
                  ;

cellVariableName: cellVarPhrase (CELL_WS cellVarPhrase)*;

cellVarPhrase: (CELL_WORD | CELL_INT)+;

text: (WS? WORD)+;
line: WS? text? NEWLINE;
description: NEWLINE*
             line+
             NEWLINE*;


// This parser rule is only used for line tokenisation
// it doesn't natively understand more context than a single line.
// It is also more forgiving than the normal parser.
onlyLine: WS? TAG lineTerm                                    #lineTag
        | WS? OPTION lineTerm                                 #lineOpt
        | WS? STEP_DEFINE DEF_WS? stepDeclaration? lineTerm   #lineStepDefine
        | WS? FEATURE WS? text? lineTerm                      #lineFeature
        | WS? BACKGROUND lineTerm                             #lineBackground
        | WS? SCENARIO WS? text? lineTerm                     #lineScenario
        | WS? SCENARIO_OUTLINE WS? text? lineTerm             #lineScenarioOutline
        | WS? EXAMPLES lineTerm                               #lineExamples
        | WS? tableRowCell+ CELL_DELIMITER                    #lineTableRow
        | WS? GIVEN statementBody? lineTerm                   #lineGiven
        | WS? WHEN statementBody? lineTerm                    #lineWhen
        | WS? THEN statementBody? lineTerm                    #lineThen
        | WS? AND statementBody? lineTerm                     #lineAnd
        | WS? text? lineTerm                                  #lineText
        ;

lineTerm: STATEMENT_NEWLINE 
        | DEF_NEWLINE
        | ROW_NL
        | WS? EOF;