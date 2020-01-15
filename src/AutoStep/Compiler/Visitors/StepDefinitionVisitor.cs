using System;
using System.Diagnostics;
using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;
using AutoStep.Elements.Parts;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Handles generating step references from the Antlr parse context.
    /// </summary>
    internal class StepDefinitionVisitor : BaseAutoStepVisitor<StepDefinitionElement>
    {
        // private readonly (int TokenType, string Replace)[] escapeReplacements = new[]
        // {
        //    (AutoStepParser.DEF_ESCAPED_LCURLY, "{"),
        //    (AutoStepParser.DEF_ESCAPED_RCURLY, "}"),
        // };

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        public StepDefinitionVisitor(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinitionVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">A shared escape rewriter.</param>
        public StepDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        /// <summary>
        /// Builds a step, taking the Step Type, and the Antlr context for the declaration body.
        /// </summary>
        /// <param name="type">The step type.</param>
        /// <param name="declarationContext">The step declaration context.</param>
        /// <param name="bodyContext">The statement body context.</param>
        /// <returns>A generated step reference.</returns>
        public StepDefinitionElement BuildStepDefinition(StepType type, AutoStepParser.StepDeclarationContext declarationContext, AutoStepParser.StepDeclarationBodyContext bodyContext)
        {
            var step = new StepDefinitionElement
            {
                Type = type,
                Declaration = bodyContext.GetText(),
            };

            Result = step;

            LineInfo(step, declarationContext);

            VisitChildren(bodyContext);

            return Result;
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

            LineInfo(step, bodyContext);

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

            var part = PositionalLineInfo(new ArgumentPart(context.GetText(), name, hint), context);

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
        /// <param name="context">The partser context.</param>
        /// <returns>The step definition.</returns>
        public override StepDefinitionElement VisitDeclarationWord([NotNull] AutoStepParser.DeclarationWordContext context)
        {
            AddPart(PositionalLineInfo(new WordDefinitionPart(context.GetText()), context));

            return Result!;
        }

        /// <summary>
        /// Visits an escaped character part.
        /// </summary>
        /// <param name="context">The partser context.</param>
        /// <returns>The step definition.</returns>
        public override StepDefinitionElement VisitDeclarationEscaped([NotNull] AutoStepParser.DeclarationEscapedContext context)
        {
            var part = PositionalLineInfo(new WordDefinitionPart(context.GetText()), context);

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
            AddPart(PositionalLineInfo(new WordDefinitionPart(context.GetText()), context));

            return Result!;
        }

        private void AddPart(DefinitionPart part)
        {
            Result!.AddPart(part);
        }
    }
}
