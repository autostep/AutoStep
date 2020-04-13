using System;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction.Visitors
{
    using static AutoStepInteractionsParser;

    /// <summary>
    /// Base class for visitors that generate interaction definition elements.
    /// </summary>
    /// <typeparam name="TResult">The element type.</typeparam>
    internal abstract class InteractionDefinitionVisitor<TResult> : BaseAutoStepInteractionVisitor<TResult>
        where TResult : InteractionDefinitionElement
    {
        private readonly InteractionStepDefinitionVisitor stepVisitor;
        private readonly MethodDefinitionVisitor methodVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionDefinitionVisitor{TResult}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">The stream rewriter.</param>
        public InteractionDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
            stepVisitor = new InteractionStepDefinitionVisitor(sourceName, tokenStream, rewriter);
            methodVisitor = new MethodDefinitionVisitor(sourceName, tokenStream, rewriter);
        }

        /// <summary>
        /// Gets or sets a value indicating whether an explicit name has been provided.
        /// </summary>
        protected bool ExplicitNameProvided { get; set; }

        /// <summary>
        /// Build an element from the provided context.
        /// </summary>
        /// <param name="owningContext">The containing Antlr context.</param>
        /// <returns>An instance of the element.</returns>
        public TResult? Build(ParserRuleContext owningContext)
        {
            Visit(owningContext);

            if (Result is object)
            {
                Result.SourceName = SourceName;
                Result.Documentation = GetDocumentationBlockForElement(owningContext);
            }

            return Result;
        }

        /// <inheritdoc/>
        public override TResult VisitMethodDefinition([NotNull] MethodDefinitionContext context)
        {
            var methodDef = methodVisitor.Build(context);

            MergeVisitorAndReset(methodVisitor);

            if (!Result!.Methods.TryAdd(methodDef.Name, methodDef))
            {
                // Duplicate method definition.
                MessageSet.Add(context, CompilerMessageLevel.Error, CompilerMessageCode.InteractionDuplicateMethodDefinition, methodDef.Name);
            }

            return Result;
        }

        /// <inheritdoc/>
        public override TResult VisitStepDefinitionBody([NotNull] StepDefinitionBodyContext context)
        {
            var stepDef = stepVisitor.BuildStepDefinition(context);

            MergeVisitorAndReset(stepVisitor);

            if (ValidateAddedStepDefinition(stepDef, context))
            {
                Result!.Steps.Add(stepDef);
            }

            return base.VisitStepDefinitionBody(context);
        }

        /// <summary>
        /// Invoked by the visitor to validate an added step definition.
        /// </summary>
        /// <param name="stepDef">The step definition.</param>
        /// <param name="bodyContext">The Antlr context for the step definition body.</param>
        /// <returns>True if valid, false otherwise.</returns>
        protected virtual bool ValidateAddedStepDefinition(InteractionStepDefinitionElement stepDef, StepDefinitionBodyContext bodyContext)
        {
            // Blank declarations are not allowed.
            return !string.IsNullOrEmpty(stepDef.Declaration);
        }

        /// <summary>
        /// Provides an explicit name for the entity.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="nameItemContext">The context item that set the name.</param>
        protected void ProvideName(string name, ParserRuleContext nameItemContext)
        {
            Debug.Assert(Result != null);

            if (ExplicitNameProvided)
            {
                // An explicit name has already been set. Can't do it twice.
                MessageSet.Add(nameItemContext, CompilerMessageLevel.Error, CompilerMessageCode.InteractionNameAlreadySet, Result!.Name);
            }
            else
            {
                Result!.Name = name;
                ExplicitNameProvided = true;
            }
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();

            ExplicitNameProvided = false;
        }
    }
}
