parser grammar AutoStepParser;

options { tokenVocab=AutoStepLexer; }

file: NEWLINE*
      featureBlock+ // Multiple feature blocks aren't valid, 
                    // but we want to give a warning later
      WS? EOF;

featureBlock: annotations? 
              featureDefinition
              backgroundBlock?
              featureBody;

annotations: annotation+;

annotation: TAG NEWLINE    # tagAnnotation
          | OPTION NEWLINE # optionAnnotation
          | NEWLINE        # blankAnnotation;

featureDefinition: WS? featureTitle NEWLINE
                   description?;
 
featureTitle: FEATURE WS? text;

featureBody: scenarioBlock*;

backgroundBlock: WS? BACKGROUND NEWLINE
                 backgroundBody;

backgroundBody: statement*;

scenarioBlock: annotations?
               scenarioDefinition
               scenarioBody;

scenarioDefinition: WS? SCENARIO WS? text NEWLINE
                    description?;

scenarioBody: (WS? statement NEWLINE | NEWLINE)*;

statement: GIVEN statementBody #given
         | WHEN statementBody  #when
         | THEN statementBody  #then
         | AND statementBody   #and
         ;

statementBody: (WS? WORD)+;

text: (WS? WORD)+;
line: (WS? WORD)* NEWLINE;
description: line*
             text
             line*;