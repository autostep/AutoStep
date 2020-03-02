using System;
using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    internal abstract class InteractionMethodDeclarationVisitor<TElement> : BaseAutoStepInteractionVisitor<TElement>
        where TElement : class, IMethodCallSource
    {
        private MethodCallElement? currentMethodCall;

        public InteractionMethodDeclarationVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        public override void Reset()
        {
            currentMethodCall = null;
        }

        public override TElement VisitMethodCall([NotNull] MethodCallContext context)
        {
            currentMethodCall = new MethodCallElement();

            currentMethodCall.AddPositionalLineInfo(context);

            currentMethodCall.MethodName = context.NAME_REF().GetText();

            VisitChildren(context);

            Result!.MethodCallChain.Add(currentMethodCall);

            currentMethodCall = null;

            return Result;
        }

        public override TElement VisitStringArg([NotNull] StringArgContext context)
        {
            var stringMethodArgument = new StringMethodArgumentElement();

            stringMethodArgument.AddPositionalLineInfo(context);

            var methodStr = context.methodStr();

            stringMethodArgument.Text = methodStr.GetText();

            // TODO - determine the set of tokens.
            var parts = methodStr.methodStrPart();
            var initialOffset = context.Start.Column + 1;
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

            stringMethodArgument.Tokenised = new TokenisedArgumentValue(tokenSet.ToArray(), false, false);

            currentMethodCall!.Arguments.Add(stringMethodArgument);

            return Result!;
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

        public override TElement VisitVariableRef([NotNull] VariableRefContext context)
        {
            var variableRefElement = new VariableRefMethodArgumentElement();
            variableRefElement.AddPositionalLineInfo(context);

            variableRefElement.VariableName = context.PARAM_NAME().GetText();

            currentMethodCall!.Arguments.Add(variableRefElement);

            return Result!;
        }

        public override TElement VisitVariableArrRef([NotNull] VariableArrRefContext context)
        {
            var varArrElement = new VariableArrayRefMethodArgument();
            varArrElement.AddPositionalLineInfo(context);

            varArrElement.VariableName = context.PARAM_NAME(0).GetText();
            varArrElement.ArrayIndex = context.PARAM_NAME(1).GetText();

            currentMethodCall!.Arguments.Add(varArrElement);

            return Result!;
        }

        public override TElement VisitConstantRef([NotNull] ConstantRefContext context)
        {
            var constantRefElement = new ConstantMethodArgument();
            constantRefElement.AddPositionalLineInfo(context);

            constantRefElement.ConstantName = context.GetText();

            currentMethodCall!.Arguments.Add(constantRefElement);

            return Result!;
        }

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

        public override TElement VisitFloatArg([NotNull] FloatArgContext context)
        {
            var floatArgElement = new FloatMethodArgument();

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
