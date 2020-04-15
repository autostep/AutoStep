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
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="AutoStepInteractionsParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
internal interface IAutoStepInteractionsParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFile([NotNull] AutoStepInteractionsParser.FileContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.entityDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEntityDefinition([NotNull] AutoStepInteractionsParser.EntityDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.appDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAppDefinition([NotNull] AutoStepInteractionsParser.AppDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>appName</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAppName([NotNull] AutoStepInteractionsParser.AppNameContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>appTraits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAppTraits([NotNull] AutoStepInteractionsParser.AppTraitsContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>appMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAppMethod([NotNull] AutoStepInteractionsParser.AppMethodContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>appStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.appItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAppStep([NotNull] AutoStepInteractionsParser.AppStepContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.traitDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTraitDefinition([NotNull] AutoStepInteractionsParser.TraitDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.traitDefinitionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTraitDefinitionDeclaration([NotNull] AutoStepInteractionsParser.TraitDefinitionDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.traitRefList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTraitRefList([NotNull] AutoStepInteractionsParser.TraitRefListContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>traitMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.traitItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTraitMethod([NotNull] AutoStepInteractionsParser.TraitMethodContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>traitStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.traitItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTraitStep([NotNull] AutoStepInteractionsParser.TraitStepContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodDefinition([NotNull] AutoStepInteractionsParser.MethodDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodDeclaration([NotNull] AutoStepInteractionsParser.MethodDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodDefArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodDefArgs([NotNull] AutoStepInteractionsParser.MethodDefArgsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallChain"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodCallChain([NotNull] AutoStepInteractionsParser.MethodCallChainContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallWithSep"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodCallWithSep([NotNull] AutoStepInteractionsParser.MethodCallWithSepContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodCall([NotNull] AutoStepInteractionsParser.MethodCallContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodCallArgs([NotNull] AutoStepInteractionsParser.MethodCallArgsContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>stringArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStringArg([NotNull] AutoStepInteractionsParser.StringArgContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>variableRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariableRef([NotNull] AutoStepInteractionsParser.VariableRefContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>variableArrRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariableArrRef([NotNull] AutoStepInteractionsParser.VariableArrRefContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>variableArrStrRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariableArrStrRef([NotNull] AutoStepInteractionsParser.VariableArrStrRefContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>constantRef</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstantRef([NotNull] AutoStepInteractionsParser.ConstantRefContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>intArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIntArg([NotNull] AutoStepInteractionsParser.IntArgContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>floatArg</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodCallArg"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFloatArg([NotNull] AutoStepInteractionsParser.FloatArgContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodCallArrayRefString"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodCallArrayRefString([NotNull] AutoStepInteractionsParser.MethodCallArrayRefStringContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.methodStr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodStr([NotNull] AutoStepInteractionsParser.MethodStrContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>methodStrContent</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodStrContent([NotNull] AutoStepInteractionsParser.MethodStrContentContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>methodStrEscape</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodStrEscape([NotNull] AutoStepInteractionsParser.MethodStrEscapeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>methodStrVariable</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.methodStrPart"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodStrVariable([NotNull] AutoStepInteractionsParser.MethodStrVariableContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.componentDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComponentDefinition([NotNull] AutoStepInteractionsParser.ComponentDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.componentDefinitionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComponentDefinitionDeclaration([NotNull] AutoStepInteractionsParser.ComponentDefinitionDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>componentName</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComponentName([NotNull] AutoStepInteractionsParser.ComponentNameContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>componentInherits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComponentInherits([NotNull] AutoStepInteractionsParser.ComponentInheritsContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>componentTraits</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComponentTraits([NotNull] AutoStepInteractionsParser.ComponentTraitsContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>componentMethod</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComponentMethod([NotNull] AutoStepInteractionsParser.ComponentMethodContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>componentStep</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.componentItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComponentStep([NotNull] AutoStepInteractionsParser.ComponentStepContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDefinitionBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStepDefinitionBody([NotNull] AutoStepInteractionsParser.StepDefinitionBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStepDefinition([NotNull] AutoStepInteractionsParser.StepDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declareGiven</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclareGiven([NotNull] AutoStepInteractionsParser.DeclareGivenContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declareWhen</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclareWhen([NotNull] AutoStepInteractionsParser.DeclareWhenContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declareThen</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclareThen([NotNull] AutoStepInteractionsParser.DeclareThenContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStepDeclarationBody([NotNull] AutoStepInteractionsParser.StepDeclarationBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declarationArgument</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSection"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationArgument([NotNull] AutoStepInteractionsParser.DeclarationArgumentContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declarationSection</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSection"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationSection([NotNull] AutoStepInteractionsParser.DeclarationSectionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationArgument"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStepDeclarationArgument([NotNull] AutoStepInteractionsParser.StepDeclarationArgumentContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationArgumentName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStepDeclarationArgumentName([NotNull] AutoStepInteractionsParser.StepDeclarationArgumentNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="AutoStepInteractionsParser.stepDeclarationTypeHint"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStepDeclarationTypeHint([NotNull] AutoStepInteractionsParser.StepDeclarationTypeHintContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declarationWord</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationWord([NotNull] AutoStepInteractionsParser.DeclarationWordContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declarationComponentInsert</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationComponentInsert([NotNull] AutoStepInteractionsParser.DeclarationComponentInsertContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declarationEscaped</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationEscaped([NotNull] AutoStepInteractionsParser.DeclarationEscapedContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declarationWs</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationWs([NotNull] AutoStepInteractionsParser.DeclarationWsContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>declarationColon</c>
	/// labeled alternative in <see cref="AutoStepInteractionsParser.stepDeclarationSectionContent"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationColon([NotNull] AutoStepInteractionsParser.DeclarationColonContext context);
}
} // namespace AutoStep.Language.Interaction.Parser

