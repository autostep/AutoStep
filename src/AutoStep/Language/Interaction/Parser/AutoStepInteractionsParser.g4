parser grammar AutoStepInteractionsParser;

options { tokenVocab=AutoStepInteractionsLexer; }

file: entityDefinition* EOF;

entityDefinition: traitDefinition
                | componentDefinition
                | appDefinition;
                
appDefinition: APP_DEFINITION NAME_REF
               appItem*;

appItem: NAME_KEYWORD STRING #appName
       | COMPONENTS_KEYWORD NAME_REF (LIST_SEPARATOR NAME_REF)* #appTraits
       | methodDefinition    #appMethod
       | stepDefinitionBody  #appStep;

traitDefinition:  TRAIT_DEFINITION traitRefList
                  traitItem*;

traitRefList: NAME_REF (PLUS NAME_REF)*;

traitItem: NAME_KEYWORD STRING #traitName
         | methodDefinition    #traitMethod
         | stepDefinitionBody  #traitStep
         ;

// method(): callee() -> callee()
methodDefinition: methodDeclaration DEF_SEPARATOR
                  (NEEDS_DEFINING | methodCallChain)
                  ;

methodDeclaration: NAME_REF METHOD_OPEN methodDefArgs? METHOD_CLOSE;

methodDefArgs: PARAM_NAME (PARAM_SEPARATOR PARAM_NAME)*;

methodCallChain: methodCall (FUNC_PASS_MARKER methodCall)*;

methodCall: NAME_REF METHOD_OPEN methodCallArgs? METHOD_CLOSE;

methodCallArgs: methodCallArg (PARAM_SEPARATOR methodCallArg)*;

methodCallArg: METHOD_STRING_START methodStr METHOD_STRING_END  #stringArg
             | PARAM_NAME #variableRef
             | PARAM_NAME ARR_LEFT PARAM_NAME ARR_RIGHT #variableArrRef
             | PARAM_NAME ARR_LEFT METHOD_STRING_START methodStr METHOD_STRING_END ARR_RIGHT #variableArrStrRef
             | CONSTANT #constantRef
             | INT      #intArg
             | FLOAT    #floatArg;

methodStr: methodStrPart+;

methodStrPart: STR_CONTENT                                 #methodStrContent
             | METHOD_STR_ESCAPE_QUOTE                     #methodStrEscape
             | STR_ANGLE_LEFT STR_NAME_REF STR_ANGLE_RIGHT #methodStrVariable
             ;

componentDefinition: COMPONENT_DEFINITION NAME_REF
                     componentItem*;

componentItem: NAME_KEYWORD STRING #componentName
             | INHERITS_KEYWORD NAME_REF #componentInherits
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
