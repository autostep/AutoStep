using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.StepTokens;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Position;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStepInteractionsParser;

    /// <summary>
    /// Base class for any section of the parse tree that visits a method call chain (in-file method definitions and steps).
    /// </summary>
    /// <typeparam name="TElement">The method type that is being generated (assignable to <see cref="ICallChainSource"/>).</typeparam>
    internal abstract class InteractionCallChainDeclarationVisitor<TElement> : BaseAutoStepInteractionVisitor<TElement>
        where TElement : class, ICallChainSource
    {
        private MethodCallElement? currentMethodCall;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionCallChainDeclarationVisitor{TElement}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The stream rewriter.</param>
        /// <param name="compilerOptions">The compiler options.</param>
        /// <param name="positionIndex">The position index (or null if not in use).</param>
        public InteractionCallChainDeclarationVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, InteractionsCompilerOptions compilerOptions, PositionIndex? positionIndex)
            : base(sourceName, tokenStream, rewriter, compilerOptions, positionIndex)
        {
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            currentMethodCall = null;
        }

        /// <inheritdoc/>
        public override TElement VisitMethodCallChain([NotNull] MethodCallChainContext context)
        {
            // Manually visit each method call.
            Visit(context.methodCall());

            foreach (var methodSet in context.methodCallWithSep())
            {
                // Add a line token for the separator, then visit the call.
                PositionIndex?.AddLineToken(methodSet.FUNC_PASS_MARKER(), LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionCallSeparator);

                Visit(methodSet.methodCall());
            }

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitMethodCall([NotNull] MethodCallContext context)
        {
            var nameRef = context.NAME_REF();

            if (nameRef is object)
            {
                currentMethodCall = new MethodCallElement(nameRef.GetText());
                currentMethodCall.AddPositionalLineInfoExcludingErrorStopToken(context, TokenStream, METHOD_STRING_ERRNL);

                PositionIndex?.PushScope(currentMethodCall, context);
                PositionIndex?.AddLineToken(nameRef, LineTokenCategory.InteractionName, LineTokenSubCategory.InteractionMethod);

                VisitChildren(context);

                PositionIndex?.PopScope(context);

                Result!.Calls.Add(currentMethodCall);

                currentMethodCall = null;
            }

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitStringArg([NotNull] StringArgContext context)
        {
            var methodStr = context.methodStr();

            var stringMethodArgument = CreateStringArgElement(methodStr);

            stringMethodArgument.AddPositionalLineInfoExcludingErrorStopToken(context, TokenStream, METHOD_STRING_ERRNL);

            currentMethodCall!.Arguments.Add(stringMethodArgument);

            return Result!;
        }

        private StringMethodArgumentElement CreateStringArgElement(MethodStrContext context)
        {
            var parts = context.methodStrPart();
            var initialOffset = context.Start.Column;
            var tokenSet = new List<StepToken>();
            var text = context.GetText();
            var stringArg = new StringMethodArgumentElement(text);

            PositionIndex?.PushScope(stringArg, context);

            foreach (var p in parts)
            {
                StepToken token = p switch
                {
                    MethodStrContentContext strContent => CreateToken(strContent, initialOffset, (s, l) => new TextToken(s, l)),
                    MethodStrEscapeContext strEscContent => CreateToken(strEscContent, initialOffset, (s, l) => new TextToken(s, l)),
                    MethodStrVariableContext strVarContent => CreateToken(strVarContent, initialOffset, (s, l) => new VariableToken(strVarContent.STR_NAME_REF().GetText(), s, l)),
                    _ => throw new LanguageEngineAssertException()
                };

                if (token is VariableToken)
                {
                    PositionIndex?.AddLineToken(token, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);
                }
                else
                {
                    PositionIndex?.AddLineToken(token, LineTokenCategory.InteractionString);
                }

                tokenSet.Add(token);
            }

            PositionIndex?.PopScope(context);

            stringArg.Tokenised = new TokenisedArgumentValue(tokenSet.ToArray(), false, false);

            return stringArg;
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

            PositionIndex?.AddLineToken(variableRefElement, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);

            currentMethodCall!.Arguments.Add(variableRefElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitVariableArrRef([NotNull] VariableArrRefContext context)
        {
            var varArrElement = new VariableArrayRefMethodArgument();
            varArrElement.AddPositionalLineInfo(context);

            var variableName = context.PARAM_NAME(0);

            varArrElement.VariableName = variableName.GetText();

            var varNameNode = context.PARAM_NAME(1);

            var varRefElement = new VariableRefMethodArgumentElement(varNameNode.GetText());
            varRefElement.AddPositionalLineInfo(varNameNode);

            varArrElement.Indexer = varRefElement;

            PositionIndex?.PushScope(varArrElement, context);

            PositionIndex?.AddLineToken(variableName, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);
            PositionIndex?.AddLineToken(varRefElement, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);

            PositionIndex?.PopScope(context);

            currentMethodCall!.Arguments.Add(varArrElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitVariableArrStrRef([NotNull] VariableArrStrRefContext context)
        {
            var varArrElement = new VariableArrayRefMethodArgument();
            varArrElement.AddPositionalLineInfo(context);

            var variableName = context.PARAM_NAME();

            varArrElement.VariableName = context.PARAM_NAME().GetText();

            var methodStrOuterContext = context.methodCallArrayRefString();

            var methodStr = methodStrOuterContext.methodStr();

            PositionIndex?.PushScope(varArrElement, context);

            PositionIndex?.AddLineToken(variableName, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable);

            var stringMethodArgument = CreateStringArgElement(methodStr);
            stringMethodArgument.AddPositionalLineInfo(methodStrOuterContext);

            varArrElement.Indexer = stringMethodArgument;

            PositionIndex?.PopScope(context);

            currentMethodCall!.Arguments.Add(varArrElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitConstantRef([NotNull] ConstantRefContext context)
        {
            var constantRefElement = new ConstantMethodArgumentElement(context.GetText());
            constantRefElement.AddPositionalLineInfo(context);

            currentMethodCall!.Arguments.Add(constantRefElement);

            PositionIndex?.AddLineToken(constantRefElement, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionConstant);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitIntArg([NotNull] IntArgContext context)
        {
            if (!int.TryParse(context.GetText(), out var intValue))
            {
                // Parser should not have allowed this through.
                throw new LanguageEngineAssertException();
            }

            var intArgElement = new IntMethodArgumentElement(intValue);

            intArgElement.AddPositionalLineInfo(context);

            PositionIndex?.AddLineToken(intArgElement, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionLiteral);

            currentMethodCall!.Arguments.Add(intArgElement);

            return Result!;
        }

        /// <inheritdoc/>
        public override TElement VisitFloatArg([NotNull] FloatArgContext context)
        {
            if (!double.TryParse(context.GetText(), out var doubleValue))
            {
                throw new LanguageEngineAssertException();
            }

            var floatArgElement = new FloatMethodArgumentElement(doubleValue);

            floatArgElement.AddPositionalLineInfo(context);

            PositionIndex?.AddLineToken(floatArgElement, LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionLiteral);

            currentMethodCall!.Arguments.Add(floatArgElement);

            return Result!;
        }
    }
}
