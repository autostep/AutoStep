using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    internal class InteractionMethodChainVariables
    {
        private class MethodChainVariable
        {
            public string Name { get; }

            public string KnownTypeHint { get; }
        }

        Dictionary<string, MethodChainVariable> activeVariables;
    }

    internal abstract class InteractionMethodHoldingVisitor<TElement> : BaseAutoStepInteractionVisitor<TElement>
        where TElement : class, IMethodCallSource
    {
        private MethodCallElement? currentMethodCall;
        private MethodCallElement? previousMethodCall;
        private InteractionMethodChainVariables? variableChain;

        public InteractionMethodHoldingVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        public override void Reset()
        {
            previousMethodCall = null;
            currentMethodCall = null;
        }

        protected abstract void ValidateArgumentVariable(ParserRuleContext nameRefToken, string variableName, bool isArrayRef);

        protected abstract void ValidateConstant(ParserRuleContext constantToken, string constantName);

        protected abstract InteractionMethodChainVariables GetInitialMethodChainVariables();

        public override TElement VisitMethodCall([NotNull] MethodCallContext context)
        {
            if (previousMethodCall is null)
            {
                
            }

            currentMethodCall = new MethodCallElement();

            currentMethodCall.AddPositionalLineInfo(context);

            currentMethodCall.MethodName = context.NAME_REF().GetText();

            VisitChildren(context);

            Result!.MethodCallChain.Add(currentMethodCall);

            previousMethodCall = currentMethodCall;
            currentMethodCall = null;

            return Result;
        }

        public override TElement VisitStringArg([NotNull] StringArgContext context)
        {
            var stringMethodArgument = new StringMethodArgumentElement();

            stringMethodArgument.AddPositionalLineInfo(context);
            stringMethodArgument.Text = context.methodStr().GetText();

            // TODO - determine the set of tokens.

            currentMethodCall!.Arguments.Add(stringMethodArgument);

            return Result!;
        }

        public override TElement VisitVariableRef([NotNull] VariableRefContext context)
        {
            var variableRefElement = new VariableRefMethodArgumentElement();
            variableRefElement.AddPositionalLineInfo(context);

            variableRefElement.VariableName = context.NAME_REF().GetText();

            currentMethodCall!.Arguments.Add(variableRefElement);

            ValidateArgumentVariable(context, variableRefElement.VariableName, false);

            return Result!;
        }

        public override TElement VisitVariableArrRef([NotNull] VariableArrRefContext context)
        {
            var varArrElement = new VariableArrayRefMethodArgument();
            varArrElement.AddPositionalLineInfo(context);

            varArrElement.VariableName = context.NAME_REF().GetText();
            varArrElement.ArrayIndex = GetTextFromStringToken(context.STRING());

            currentMethodCall!.Arguments.Add(varArrElement);

            ValidateArgumentVariable(context, varArrElement.VariableName, true);

            return Result!;
        }

        public override TElement VisitConstantRef([NotNull] ConstantRefContext context)
        {
            var constantRefElement = new ConstantMethodArgument();
            constantRefElement.AddPositionalLineInfo(context);

            constantRefElement.ConstantName = context.GetText();

            currentMethodCall!.Arguments.Add(constantRefElement);

            // TODO: Validate the provided constant.
            ValidateConstant(context, constantRefElement.ConstantName);

            return Result;
        }

        public override TElement VisitIntArg([NotNull] IntArgContext context)
        {
            var intArgElement = new IntMethodArgument();

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
