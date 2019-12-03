parser grammar AutoStepParser;

options { tokenVocab=AutoStepLexer; }

file: featureBlock EOF;

featureBlock: annotations 
              featureDefinition
              backgroundBlock?
              featureBody;

annotations: annotation*;

featureDefinition: WS? FEATURE name
                   description?;
 
featureBody: scenarioBlock*;

backgroundBlock: WS? BACKGROUND NEWLINE
                 backgroundBody;

backgroundBody: statement*;

scenarioBlock: annotation*
               scenarioDefinition
               scenarioBody;

scenarioDefinition: WS? SCENARIO name
                    description?;

scenarioBody: statement*;

statement: WS? GIVEN statementBody
         | WS? WHEN statementBody
         | WS? THEN statementBody
         | WS? AND statementBody
         | NEWLINE;

statementBody: (WS? WORD)+ (NEWLINE | EOF);

name: (WS? WORD)+ (NEWLINE | EOF);
line: (WS? WORD)* NEWLINE;
description: line+;

annotation: TAG NEWLINE    # tag
          | OPTION NEWLINE # option
          ;

//mode ANNOTATION;
//ID: [a-zA-Z0-9]+;
//WS: [ \t]+ -> CHANNEL(HIDDEN);