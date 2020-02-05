parser grammar AutoStepInteractionsParser;

options { tokenVocab=AutoStepInteractionsLexer; }

file: entityDefinition*;

entityDefinition: traitDefinition 
                | componentDefinition
                | appDefinition;
                
appDefinition: APP_DEFINITION NAME_REF
               appItem*;

appItem: NAME_KEYWORD STRING #appName
       | COMPONENTS_KEYWORD NAME_REF (LIST_SEPARATOR NAME_REF)* #appTraits
       | methodDefinition    #appMethod
       | stepDefinitionBody  #appStep;

traitDefinition:  TRAIT_DEFINITION NAME_REF (PLUS NAME_REF)*
                  traitItem*;

traitItem: NAME_KEYWORD STRING #traitName
         | methodDefinition    #traitMethod
         | stepDefinitionBody  #traitStep;

// method(): callee() -> callee()
methodDefinition: NAME_REF METHOD_OPEN methodDefArgs? METHOD_CLOSE DEF_SEPARATOR
                  methodCall (FUNC_PASS_MARKER methodCall)*;

methodDefArgs: NAME_REF (NAME_REF LIST_SEPARATOR)*;

methodCall: NAME_REF METHOD_OPEN methodCallArgs? METHOD_CLOSE;

methodCallArgs: methodCallArg (LIST_SEPARATOR methodCallArg)*;

methodCallArg: STRING   #stringArg
             | NAME_REF #variableRef
             | NAME_REF ARR_LEFT STRING ARR_RIGHT #variableArrRef
             | CONSTANT #constantRef
             | INT      #intArg
             | FLOAT    #floatArg;

componentDefinition: COMPONENT_DEFINITION NAME_REF
                     componentItem*;

componentItem: NAME_KEYWORD STRING #componentName
             | BASEDON_KEYWORD NAME_REF #componentBasedOn
             | TRAITS_KEYWORD NAME_REF (LIST_SEPARATOR NAME_REF)* #componentTraits
             | methodDefinition    #componentMethod
             | stepDefinitionBody  #componentStep;

stepDefinitionBody: stepDefinition
                    methodCall (FUNC_PASS_MARKER methodCall)*;

stepDefinition: STEP_DEFINE DEF_WS? stepDeclaration DEF_NEWLINE;

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
                             | DEF_COMPONENT_INSERT                           # declarationComponentInsert
                             | (DEF_ESCAPED_LCURLY | DEF_ESCAPED_RCURLY)      # declarationEscaped
                             | DEF_WS                                         # declarationWs
                             | DEF_COLON                                      # declarationColon
                             ;
