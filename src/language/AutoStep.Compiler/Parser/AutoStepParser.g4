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

backgroundBody: statement*;

scenarioBlock: annotations
               scenarioDefinition
               scenarioBody;

scenarioDefinition: WS? scenarioTitle NEWLINE
                    description?;

scenarioTitle: SCENARIO WS? text;

scenarioBody: scenarioBodyLine*;

scenarioBodyLine: 
      WS? statement NEWLINE |
      WS? statement WS? EOF |
      NEWLINE
      ;

statement: GIVEN statementBody #given
         | WHEN statementBody  #when
         | THEN statementBody  #then
         | AND statementBody   #and
         ;

statementBody: (WS? WORD)+;

text: (WS? WORD)+;
line: WS? text? NEWLINE;
description: NEWLINE*
             line+
             NEWLINE*;