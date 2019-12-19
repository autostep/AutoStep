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

stepCollectionBodyLine: 
      WS? statement STATEMENT_NEWLINE |
      WS? statement WS? EOF |
      NEWLINE
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
                | OPEN_QUOTE CURR_SYMBOL? FLOAT CLOSE_QUOTE       # argFloat
                | OPEN_QUOTE CURR_SYMBOL? INT CLOSE_QUOTE         # argInt       
                | OPEN_QUOTE COLON statementTextContentBlock CLOSE_QUOTE      # argInterpolate
                | OPEN_QUOTE statementTextContentBlock CLOSE_QUOTE            # argText
                ;

statementTextContentBlock: (TEXT_CONTENT | INT | FLOAT | CURR_SYMBOL | COLON | ESCAPE_QUOTE)+;

text: (WS? WORD)+;
line: WS? text? NEWLINE;
description: NEWLINE*
             line+
             NEWLINE*;