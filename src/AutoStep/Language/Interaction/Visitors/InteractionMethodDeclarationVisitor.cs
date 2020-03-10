using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    /// <summary>
    /// Base class for any section of the parse tree that visits a method call chain (in-file method definitions and steps).
    /// </summary>
    /// <typeparam name="TElement">The method type that is being generated (assignable to <see cref="ICallChainSource"/>).</typeparam>
    internal abstract class InteractionMethodDeclarationVisitor<TElement> : BaseAutoStepInteractionVisitor<TElement>
        where TElement : class, ICallChainSource
    {
        private MethodCallElement? currentMethodCall;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionMethodDeclarationVisitor{TElement}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The stream rewriter.</param>
        public InteractionMethodDeclarationVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            currentMethodCall = null;
        }

        /// <inheritdoc/>
        public override TElement VisitMethodCall([NotNull] MethodCallContext context)
        {
            if (context.NAME_REF() is object)
            {
                currentMethodCall = new MethodCallElement(context.NAME_REF().GetText());

                currentMethodCall.AddPositionalLineInfoExcludingErrorStopToken(context, TokenStream, METHOD_STRING_ERRNL);

                VisitChildren(context);

                Result!.Calls.Add(currentMethodCall);

                currentMethodCall = null;
            }

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitStringArg([NotNull] StringArgContext context)
        {
            var stringMethodArgument = new StringMethodArgumentElement();

            stringMethodArgument.AddPositionalLineInfoExcludingErrorStopToken(context, TokenStream, METHOD_STRING_ERRNL);

            var methodStr = context.methodStr();

            PopulateStringArgElement(stringMethodArgument, methodStr);

            currentMethodCall!.Arguments.Add(stringMethodArgument);

            return Result!;
        }

        private void PopulateStringArgElement(StringMethodArgumentElement element, MethodStrContext context)
        {
            element.Text = context.GetText();

            var parts = context.methodStrPart();
            var initialOffset = context.Start.Column;
            var tokenSet = new List<StepToken>();

            foreach (var p in parts)
            {
                StepToken token = p switch
                {
                    MethodStrContentContext strContent => CreateToken(strContent, initialOffset, (s, l) => new TextToken(s, l)),
                    MethodStrEscapeContext strEscContent => CreateToken(strEscContent, initialOffset, (s, l) => new TextToken(s, l)),
                    MethodStrVariableContext strVarContent => CreateToken(strVarContent, initialOffset, (s, l) => new VariableToken(strVarContent.STR_NAME_REF().GetText(), s, l)),
                    _ => throw new LanguageEngineAssertException()
                };

                tokenSet.Add(token);
            }

            element.Tokenised = new TokenisedArgumentValue(tokenSet.ToArray(), false, false);
        }

        private static TToken CreateToken<TToken>(ParserRuleContext ctxt, int offset, Func<int, int, TToken> creator)
            where TToken : StepToken
        {
            var start = ctxt.Start.Column - offset;
            var startIndex = ctxt.Start.StartIndex;

            var part = creator(start, ctxt.Stop.StopIndex - startIndex + 1);

            part.AddPositionalLineInfo(ctxt);

            return part;
        }

        /// <inheritdoc/>
        public override TElement VisitVariableRef([NotNull] VariableRefContext context)
        {
            var variableRefElement = new VariableRefMethodArgumentElement(context.PARAM_NAME().GetText());
            variableRefElement.AddPositionalLineInfo(context);

            currentMethodCall!.Arguments.Add(variableRefElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitVariableArrRef([NotNull] VariableArrRefContext context)
        {
            var varArrElement = new VariableArrayRefMethodArgument();
            varArrElement.AddPositionalLineInfo(context);

            varArrElement.VariableName = context.PARAM_NAME(0).GetText();

            var varNameNode = context.PARAM_NAME(1);

            var varRefElement = new VariableRefMethodArgumentElement(varNameNode.GetText());
            varRefElement.AddPositionalLineInfo(varNameNode);

            varArrElement.Indexer = varRefElement;

            currentMethodCall!.Arguments.Add(varArrElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitVariableArrStrRef([NotNull] VariableArrStrRefContext context)
        {
            var varArrElement = new VariableArrayRefMethodArgument();
            varArrElement.AddPositionalLineInfo(context);

            varArrElement.VariableName = context.PARAM_NAME().GetText();

            var methodStrOuterContext = context.methodCallArrayRefString();

            var stringMethodArgument = new StringMethodArgumentElement();
            stringMethodArgument.AddPositionalLineInfo(methodStrOuterContext);

            var methodStr = methodStrOuterContext.methodStr();

            PopulateStringArgElement(stringMethodArgument, methodStr);

            varArrElement.Indexer = stringMethodArgument;

            currentMethodCall!.Arguments.Add(varArrElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitConstantRef([NotNull] ConstantRefContext context)
        {
            var constantRefElement = new ConstantMethodArgumentElement();
            constantRefElement.AddPositionalLineInfo(context);

            constantRefElement.ConstantName = context.GetText();

            currentMethodCall!.Arguments.Add(constantRefElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitIntArg([NotNull] IntArgContext context)
        {
            var intArgElement = new IntMethodArgumentElement();

            intArgElement.AddPositionalLineInfo(context);

            if (!int.TryParse(context.GetText(), out var intValue))
            {
                // Parser should not have allowed this through.
                throw new LanguageEngineAssertException();
            }

            intArgElement.Value = intValue;

            currentMethodCall!.Arguments.Add(intArgElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitFloatArg([NotNull] FloatArgContext context)
        {
            var floatArgElement = new FloatMethodArgumentElement();

            floatArgElement.AddPositionalLineInfo(context);

            if (!double.TryParse(context.GetText(), out var doubleValue))
            {
                throw new LanguageEngineAssertException();
            }

            floatArgElement.Value = doubleValue;

            currentMethodCall!.Arguments.Add(floatArgElement);

            return Result!;
        }
    }
}
