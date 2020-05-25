parser grammar AutoStepParser;

options { tokenVocab=AutoStepLexer; }

file: fileEntity* // Multiple feature blocks aren't valid, 
                  // but we want to give an error later instead of failing
                  // the parse stage.
      EOF;

fileEntity: NEWLINE* (featureBlock | stepDefinitionBlock) WS?;

stepDefinitionBlock: annotations
                     stepDefinition
                     stepDefinitionBody;

stepDefinition: WS? STEP_DEFINE WS? stepDeclaration NEWLINE
                    description?;

stepDefinitionBody: stepCollectionBodyLine+;

stepDeclaration: DEF_GIVEN WS? stepDeclarationBody #declareGiven
               | DEF_WHEN WS? stepDeclarationBody  #declareWhen
               | DEF_THEN WS? stepDeclarationBody  #declareThen
               ;

stepDeclarationBody: stepDeclarationSection*;

stepDeclarationSection: 
                      (DEF_QUOTE|DEF_DBLQUOTE) DEF_LCURLY stepDeclarationArgument DEF_RCURLY (DEF_QUOTE | DEF_DBLQUOTE) # declarationArgumentErrBoundedQuotes                       
                      | DEF_LCURLY stepDeclarationArgument DEF_RCURLY       # declarationArgument
                      | stepDeclarationSectionContent                       # declarationSection;

stepDeclarationArgument: stepDeclarationArgumentName (DEF_COLON stepDeclarationTypeHint)?
                       ;

stepDeclarationArgumentName: DEF_WORD;

stepDeclarationTypeHint: DEF_WORD;

stepDeclarationSectionContent: (DEF_WORD | DEF_GIVEN | DEF_WHEN | DEF_THEN | DEF_QUOTE | DEF_DBLQUOTE)   # declarationWord
                             | (DEF_ESCAPED_LCURLY | DEF_ESCAPED_RCURLY)      # declarationEscaped
                             | WS                                             # declarationWs
                             | DEF_COLON                                      # declarationColon
                             ;

featureBlock: annotations 
              featureDefinition
              backgroundBlock?
              featureBody;

annotations: annotation*;

annotation: WS? annotationBody ANNOTATION_NEWLINE #annotationLine
          | NEWLINE                    #blank
          ;

annotationBody: TAG ANNOTATION_TEXT    #tagAnnotation
              | OPTION ANNOTATION_TEXT #optionAnnotation
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

statementBlock: WS? statement NEWLINE NEWLINE* tableBlock #statementWithTable
              | WS? statement NEWLINE                     #statementLineTerminated
              | WS? statement EOF                         #statementEofTerminated;

statement: GIVEN (WS statementBody)? #given
         | WHEN (WS statementBody)?  #when
         | THEN (WS statementBody)?  #then
         | AND (WS statementBody)?   #and
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
                  | WS                                                            # statementBlockWs
                  ;

statementVariableName: statementVarPhrase (WS statementVarPhrase)*;

statementVarPhrase: (STATEMENT_WORD | STATEMENT_INT)+;

examples: exampleBlock*;

exampleBlock: annotations
              WS? EXAMPLES NEWLINE+
              tableBlock;

tableBlock: WS? tableHeader
            (WS? tableRow | NEWLINE)*;

tableHeader: tableHeaderCell+ CELL_DELIMITER NEWLINE;

tableHeaderCell: (TABLE_START | CELL_DELIMITER) WS? cellVariableName? WS?;

tableRow: tableRowCell+ CELL_DELIMITER NEWLINE;

tableRowCell: (TABLE_START | CELL_DELIMITER) WS? tableRowCellContent? WS?;

tableRowCellContent: (WS? cellContentBlock)+;

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

cellVariableName: cellVarPhrase (WS cellVarPhrase)*;

cellVarPhrase: (CELL_WORD | CELL_INT)+;

// Need to include the special tokens that might appear inside a normal
// sentence.
text: (WS? WORD)+;
line: WS? text? NEWLINE;
description: NEWLINE*
             line+
             NEWLINE*;


// This parser rule is only used for line tokenisation
// it doesn't natively understand more context than a single line.
// It is also more forgiving than the normal parser.
onlyLine: WS? TAG ANNOTATION_TEXT lineTerm                    #lineTag
        | WS? OPTION ANNOTATION_TEXT lineTerm                 #lineOpt
        | WS? STEP_DEFINE WS? stepDeclaration? lineTerm       #lineStepDefine
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

lineTerm: NEWLINE
        | WS? EOF;