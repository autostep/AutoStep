//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from .\AutoStepInteractionsParser.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace AutoStep.Language.Interaction.Parser {
using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="AutoStepInteractionsParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
internal interface IAutoStepInteractionsParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFile([NotNull] AutoStepInteractionsParser.FileContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFile([NotNull] AutoStepInteractionsParser.FileContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.entityDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEntityDefinition([NotNull] AutoStepInteractionsParser.EntityDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.entityDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEntityDefinition([NotNull] AutoStepInteractionsParser.EntityDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.appDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAppDefinition([NotNull] AutoStepInteractionsParser.AppDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.appDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAppDefinition([NotNull] AutoStepInteractionsParser.AppDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>appName</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAppName([NotNull] AutoStepInteractionsParser.AppNameContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>appName</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAppName([NotNull] AutoStepInteractionsParser.AppNameContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>appTraits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAppTraits([NotNull] AutoStepInteractionsParser.AppTraitsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>appTraits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAppTraits([NotNull] AutoStepInteractionsParser.AppTraitsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>appMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAppMethod([NotNull] AutoStepInteractionsParser.AppMethodContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>appMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAppMethod([NotNull] AutoStepInteractionsParser.AppMethodContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>appStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAppStep([NotNull] AutoStepInteractionsParser.AppStepContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>appStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAppStep([NotNull] AutoStepInteractionsParser.AppStepContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.traitDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTraitDefinition([NotNull] AutoStepInteractionsParser.TraitDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.traitDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTraitDefinition([NotNull] AutoStepInteractionsParser.TraitDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.traitDefinitionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTraitDefinitionDeclaration([NotNull] AutoStepInteractionsParser.TraitDefinitionDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.traitDefinitionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTraitDefinitionDeclaration([NotNull] AutoStepInteractionsParser.TraitDefinitionDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.traitRefList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTraitRefList([NotNull] AutoStepInteractionsParser.TraitRefListContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.traitRefList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTraitRefList([NotNull] AutoStepInteractionsParser.TraitRefListContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>traitMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.traitItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTraitMethod([NotNull] AutoStepInteractionsParser.TraitMethodContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>traitMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.traitItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTraitMethod([NotNull] AutoStepInteractionsParser.TraitMethodContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>traitStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.traitItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTraitStep([NotNull] AutoStepInteractionsParser.TraitStepContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>traitStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.traitItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTraitStep([NotNull] AutoStepInteractionsParser.TraitStepContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodDefinition([NotNull] AutoStepInteractionsParser.MethodDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodDefinition([NotNull] AutoStepInteractionsParser.MethodDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodDeclaration([NotNull] AutoStepInteractionsParser.MethodDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodDeclaration([NotNull] AutoStepInteractionsParser.MethodDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodDefArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodDefArgs([NotNull] AutoStepInteractionsParser.MethodDefArgsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodDefArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodDefArgs([NotNull] AutoStepInteractionsParser.MethodDefArgsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallChain"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodCallChain([NotNull] AutoStepInteractionsParser.MethodCallChainContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallChain"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodCallChain([NotNull] AutoStepInteractionsParser.MethodCallChainContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallWithSep"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodCallWithSep([NotNull] AutoStepInteractionsParser.MethodCallWithSepContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallWithSep"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodCallWithSep([NotNull] AutoStepInteractionsParser.MethodCallWithSepContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodCall([NotNull] AutoStepInteractionsParser.MethodCallContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodCall([NotNull] AutoStepInteractionsParser.MethodCallContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodCallArgs([NotNull] AutoStepInteractionsParser.MethodCallArgsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodCallArgs([NotNull] AutoStepInteractionsParser.MethodCallArgsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>stringArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStringArg([NotNull] AutoStepInteractionsParser.StringArgContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>stringArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStringArg([NotNull] AutoStepInteractionsParser.StringArgContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>variableRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableRef([NotNull] AutoStepInteractionsParser.VariableRefContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>variableRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableRef([NotNull] AutoStepInteractionsParser.VariableRefContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>variableArrRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableArrRef([NotNull] AutoStepInteractionsParser.VariableArrRefContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>variableArrRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableArrRef([NotNull] AutoStepInteractionsParser.VariableArrRefContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>variableArrStrRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableArrStrRef([NotNull] AutoStepInteractionsParser.VariableArrStrRefContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>variableArrStrRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableArrStrRef([NotNull] AutoStepInteractionsParser.VariableArrStrRefContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>constantRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterConstantRef([NotNull] AutoStepInteractionsParser.ConstantRefContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>constantRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitConstantRef([NotNull] AutoStepInteractionsParser.ConstantRefContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>intArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIntArg([NotNull] AutoStepInteractionsParser.IntArgContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>intArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIntArg([NotNull] AutoStepInteractionsParser.IntArgContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>floatArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFloatArg([NotNull] AutoStepInteractionsParser.FloatArgContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>floatArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFloatArg([NotNull] AutoStepInteractionsParser.FloatArgContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallArrayRefString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodCallArrayRefString([NotNull] AutoStepInteractionsParser.MethodCallArrayRefStringContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallArrayRefString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodCallArrayRefString([NotNull] AutoStepInteractionsParser.MethodCallArrayRefStringContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.methodStr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodStr([NotNull] AutoStepInteractionsParser.MethodStrContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.methodStr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodStr([NotNull] AutoStepInteractionsParser.MethodStrContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>methodStrContent</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodStrContent([NotNull] AutoStepInteractionsParser.MethodStrContentContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>methodStrContent</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodStrContent([NotNull] AutoStepInteractionsParser.MethodStrContentContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>methodStrEscape</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodStrEscape([NotNull] AutoStepInteractionsParser.MethodStrEscapeContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>methodStrEscape</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodStrEscape([NotNull] AutoStepInteractionsParser.MethodStrEscapeContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>methodStrVariable</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMethodStrVariable([NotNull] AutoStepInteractionsParser.MethodStrVariableContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>methodStrVariable</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMethodStrVariable([NotNull] AutoStepInteractionsParser.MethodStrVariableContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.componentDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComponentDefinition([NotNull] AutoStepInteractionsParser.ComponentDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.componentDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComponentDefinition([NotNull] AutoStepInteractionsParser.ComponentDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.componentDefinitionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComponentDefinitionDeclaration([NotNull] AutoStepInteractionsParser.ComponentDefinitionDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.componentDefinitionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComponentDefinitionDeclaration([NotNull] AutoStepInteractionsParser.ComponentDefinitionDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>componentName</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComponentName([NotNull] AutoStepInteractionsParser.ComponentNameContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>componentName</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComponentName([NotNull] AutoStepInteractionsParser.ComponentNameContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>componentInherits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComponentInherits([NotNull] AutoStepInteractionsParser.ComponentInheritsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>componentInherits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComponentInherits([NotNull] AutoStepInteractionsParser.ComponentInheritsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>componentTraits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComponentTraits([NotNull] AutoStepInteractionsParser.ComponentTraitsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>componentTraits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComponentTraits([NotNull] AutoStepInteractionsParser.ComponentTraitsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>componentMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComponentMethod([NotNull] AutoStepInteractionsParser.ComponentMethodContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>componentMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComponentMethod([NotNull] AutoStepInteractionsParser.ComponentMethodContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>componentStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterComponentStep([NotNull] AutoStepInteractionsParser.ComponentStepContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>componentStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitComponentStep([NotNull] AutoStepInteractionsParser.ComponentStepContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.stepDefinitionBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStepDefinitionBody([NotNull] AutoStepInteractionsParser.StepDefinitionBodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDefinitionBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStepDefinitionBody([NotNull] AutoStepInteractionsParser.StepDefinitionBodyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.stepDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStepDefinition([NotNull] AutoStepInteractionsParser.StepDefinitionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStepDefinition([NotNull] AutoStepInteractionsParser.StepDefinitionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declareGiven</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclareGiven([NotNull] AutoStepInteractionsParser.DeclareGivenContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declareGiven</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclareGiven([NotNull] AutoStepInteractionsParser.DeclareGivenContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declareWhen</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclareWhen([NotNull] AutoStepInteractionsParser.DeclareWhenContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declareWhen</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclareWhen([NotNull] AutoStepInteractionsParser.DeclareWhenContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declareThen</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclareThen([NotNull] AutoStepInteractionsParser.DeclareThenContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declareThen</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclareThen([NotNull] AutoStepInteractionsParser.DeclareThenContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStepDeclarationBody([NotNull] AutoStepInteractionsParser.StepDeclarationBodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStepDeclarationBody([NotNull] AutoStepInteractionsParser.StepDeclarationBodyContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declarationArgument</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSection"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclarationArgument([NotNull] AutoStepInteractionsParser.DeclarationArgumentContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declarationArgument</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSection"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclarationArgument([NotNull] AutoStepInteractionsParser.DeclarationArgumentContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declarationSection</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSection"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclarationSection([NotNull] AutoStepInteractionsParser.DeclarationSectionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declarationSection</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSection"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclarationSection([NotNull] AutoStepInteractionsParser.DeclarationSectionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationArgument"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStepDeclarationArgument([NotNull] AutoStepInteractionsParser.StepDeclarationArgumentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationArgument"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStepDeclarationArgument([NotNull] AutoStepInteractionsParser.StepDeclarationArgumentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationArgumentName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStepDeclarationArgumentName([NotNull] AutoStepInteractionsParser.StepDeclarationArgumentNameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationArgumentName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStepDeclarationArgumentName([NotNull] AutoStepInteractionsParser.StepDeclarationArgumentNameContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationTypeHint"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStepDeclarationTypeHint([NotNull] AutoStepInteractionsParser.StepDeclarationTypeHintContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationTypeHint"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStepDeclarationTypeHint([NotNull] AutoStepInteractionsParser.StepDeclarationTypeHintContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declarationWord</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclarationWord([NotNull] AutoStepInteractionsParser.DeclarationWordContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declarationWord</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclarationWord([NotNull] AutoStepInteractionsParser.DeclarationWordContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declarationComponentInsert</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclarationComponentInsert([NotNull] AutoStepInteractionsParser.DeclarationComponentInsertContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declarationComponentInsert</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclarationComponentInsert([NotNull] AutoStepInteractionsParser.DeclarationComponentInsertContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declarationEscaped</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclarationEscaped([NotNull] AutoStepInteractionsParser.DeclarationEscapedContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declarationEscaped</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclarationEscaped([NotNull] AutoStepInteractionsParser.DeclarationEscapedContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declarationWs</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclarationWs([NotNull] AutoStepInteractionsParser.DeclarationWsContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declarationWs</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclarationWs([NotNull] AutoStepInteractionsParser.DeclarationWsContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>declarationColon</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDeclarationColon([NotNull] AutoStepInteractionsParser.DeclarationColonContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>declarationColon</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDeclarationColon([NotNull] AutoStepInteractionsParser.DeclarationColonContext context);
}
} // namespace AutoStep.Language.Interaction.Parser

