using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements;
using AutoStep.Elements.Parts;
using AutoStep.Language.Position;
using AutoStep.Language.Test.Parser;

namespace AutoStep.Language.Test.Visitors
{
    /// <summary>
    /// Handles generating step definitions from the Antlr parse context.
    /// </summary>
    internal class StepDefinitionVisitor : BaseTestVisitor<StepDefinitionElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="compilerOptions">The compiler options.</param>
        /// <param name="positionIndex">The position index (or null if not in use).</param>
        public StepDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TestCompilerOptions compilerOptions, PositionIndex? positionIndex)
            : base(sourceName, tokenStream, compilerOptions, positionIndex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">A shared escape rewriter.</param>
        /// <param name="compilerOptions">The compiler options.</param>
        /// <param name="positionIndex">The position index (or null if not in use).</param>
        public StepDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, TestCompilerOptions compilerOptions, PositionIndex? positionIndex)
            : base(sourceName, tokenStream, rewriter, compilerOptions, positionIndex)
        {
        }

        /// <summary>
        /// Builds a step, taking the Step Type, and the Antlr context for the declaration body.
        /// </summary>
        /// <param name="blankElement">A blank step definition element to populate.</param>
        /// <param name="type">The step type.</param>
        /// <param name="declarationContext">The step declaration context.</param>
        /// <param name="bodyContext">The statement body context.</param>
        public void BuildStepDefinition(StepDefinitionElement blankElement, StepType type, AutoStepParser.StepDeclarationContext declarationContext, AutoStepParser.StepDeclarationBodyContext bodyContext)
        {
            blankElement.Type = type;
            blankElement.Declaration = bodyContext.GetText();

            Result = blankElement;

            blankElement.AddPositionalLineInfo(declarationContext);

            VisitChildren(bodyContext);
        }

        /// <summary>
        /// Builds a step, taking the Step Type, and the Antlr context for the declaration body.
        /// </summary>
        /// <param name="type">The step type.</param>
        /// <param name="bodyContext">The statement body context.</param>
        /// <returns>A generated step reference.</returns>
        public StepDefinitionElement BuildStepDefinition(StepType type, AutoStepParser.StepDeclarationBodyContext bodyContext)
        {
            var step = new StepDefinitionElement
            {
                Type = type,
                Declaration = bodyContext.GetText(),
            };

            Result = step;

            step.AddPositionalLineInfo(bodyContext);

            VisitChildren(bodyContext);

            return Result;
        }

        /// <summary>
        /// Visits a declaration argument.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step definition element.</returns>
        public override StepDefinitionElement VisitDeclarationArgument([NotNull] AutoStepParser.DeclarationArgumentContext context)
        {
            Debug.Assert(Result is object);

            var content = context.stepDeclarationArgument();

            var name = content.stepDeclarationArgumentName().GetText();
            var hint = DetermineTypeHint(name, content.stepDeclarationTypeHint()?.GetText());

            var part = new ArgumentPart(context.GetText(), name, hint).AddPositionalLineInfo(context);

            AddPart(part);

            return Result;
        }

        private ArgumentType? DetermineTypeHint(string name, string? typeHint)
        {
            if (typeHint == null)
            {
                // No explicit type hint, but we might be able to take it from the name.
                typeHint = name;
            }

            return typeHint switch
            {
                "int" => ArgumentType.NumericInteger,
                "long" => ArgumentType.NumericInteger,
                "float" => ArgumentType.NumericDecimal,
                "double" => ArgumentType.NumericDecimal,
                "decimal" => ArgumentType.NumericDecimal,
                "word" => ArgumentType.Text,
                _ => null
            };
        }

        /// <summary>
        /// Visits a word declaration part.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step definition.</returns>
        public override StepDefinitionElement VisitDeclarationWord([NotNull] AutoStepParser.DeclarationWordContext context)
        {
            AddPart(new WordDefinitionPart(context.GetText()).AddPositionalLineInfo(context));

            return Result!;
        }

        /// <summary>
        /// Visits an escaped character part.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step definition.</returns>
        public override StepDefinitionElement VisitDeclarationEscaped([NotNull] AutoStepParser.DeclarationEscapedContext context)
        {
            var part = new WordDefinitionPart(context.GetText()).AddPositionalLineInfo(context);

            // TODO
            // part.EscapedText = EscapeText(context, escapeReplacements);
            AddPart(part);

            return Result!;
        }

        /// <summary>
        /// Visits a colon part.
        /// </summary>
        /// <param name="context">The parser context.</param>
        /// <returns>The step definition.</returns>
        public override StepDefinitionElement VisitDeclarationColon([NotNull] AutoStepParser.DeclarationColonContext context)
        {
            AddPart(new WordDefinitionPart(context.GetText()).AddPositionalLineInfo(context));

            return Result!;
        }

        private void AddPart(DefinitionPart part)
        {
            Result!.AddPart(part);
            PositionIndex?.AddLineToken(part, LineTokenCategory.StepText, LineTokenSubCategory.Declaration);
        }

        private void AddPart(ArgumentPart part)
        {
            Result!.AddPart(part);
            PositionIndex?.AddLineToken(part, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration);
        }
    }
}
