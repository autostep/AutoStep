using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStep.Language.Interaction.Parser.AutoStepInteractionsParser;

    internal class InteractionDefinitionVisitor<TResult> : BaseAutoStepInteractionVisitor<TResult>
        where TResult : InteractionDefinitionElement
    {
        protected bool ExplicitNameProvided { get; set; }

        private InteractionStepDefinitionVisitor stepVisitor;
        private MethodDefinitionVisitor methodVisitor;

        public InteractionDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
            stepVisitor = new InteractionStepDefinitionVisitor(sourceName, tokenStream, rewriter);
            methodVisitor = new MethodDefinitionVisitor(sourceName, tokenStream, rewriter);
        }

        public TResult? Build(ParserRuleContext owningContext)
        {
            Visit(owningContext);

            return Result;
        }

        public override TResult VisitMethodDefinition([NotNull] MethodDefinitionContext context)
        {
            var methodDef = methodVisitor.Build(context);

            MergeVisitorAndReset(methodVisitor);

            Result!.Methods.Add(methodDef);

            return Result;
        }

        public override TResult VisitStepDefinitionBody([NotNull] StepDefinitionBodyContext context)
        {
            var stepDef = stepVisitor.BuildStepDefinition(context);

            MergeVisitorAndReset(stepVisitor);

            Result!.Steps.Add(stepDef);

            return base.VisitStepDefinitionBody(context);
        }

        protected void ProvideName(string name, ParserRuleContext nameItemContext)
        {
            Debug.Assert(Result != null);

            if (ExplicitNameProvided)
            {
                // An explicit name has already been set. Can't do it twice.
                MessageSet.Add(nameItemContext, CompilerMessageLevel.Error, CompilerMessageCode.InteractionNameAlreadySet, Result.Name);
            }
            else
            {
                Result.Name = name;
                ExplicitNameProvided = true;
            }
        }

        public override void Reset()
        {
            base.Reset();

            ExplicitNameProvided = false;
        }
    }
}
