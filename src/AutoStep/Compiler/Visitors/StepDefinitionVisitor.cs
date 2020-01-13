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
        private readonly (int TokenType, string Replace)[] escapeReplacements = new[]
        {
            (AutoStepParser.DEF_ESCAPED_LCURLY, "{"),
            (AutoStepParser.DEF_ESCAPED_RCURLY, "}"),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="StepReferenceVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="insertionNameValidator">The insertion name validator callback, used to check for valid insertion names.</param>
        public StepDefinitionVisitor(string? sourceName, ITokenStream tokenStream)
            : base(sourceName, tokenStream)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepReferenceVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">A shared escape rewriter.</param>
        /// <param name="insertionNameValidator">The insertion name validator callback, used to check for valid insertion names.</param>
        public StepDefinitionVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter)
            : base(sourceName, tokenStream, rewriter)
        {
        }

        /// <summary>
        /// Builds a step, taking the Step Type, and the Antlr context for the declaration body.
        /// </summary>
        /// <param name="type">The step type.</param>
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

        public override StepDefinitionElement VisitDeclarationArgument([NotNull] AutoStepParser.DeclarationArgumentContext context)
        {
            Debug.Assert(Result is object);

            var content = context.stepDeclarationArgument();

            var part = CreatePart<ArgumentPart>(context);

            part.Name = content.stepDeclarationArgumentName().GetText();

            part.TypeHint = DetermineTypeHint(part.Name, content.stepDeclarationTypeHint()?.GetText());

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

        public override StepDefinitionElement VisitDeclarationWord([NotNull] AutoStepParser.DeclarationWordContext context)
        {
            AddPart(CreatePart<WordDefinitionPart>(context));

            return Result!;
        }

        public override StepDefinitionElement VisitDeclarationEscaped([NotNull] AutoStepParser.DeclarationEscapedContext context)
        {
            var part = CreatePart<WordDefinitionPart>(context);

            part.EscapedText = EscapeText(context, escapeReplacements);

            AddPart(part);

            return Result!;
        }

        public override StepDefinitionElement VisitDeclarationColon([NotNull] AutoStepParser.DeclarationColonContext context)
        {
            AddPart(CreatePart<WordDefinitionPart>(context));

            return Result!;
        }

        private void AddPart(DefinitionContentPart part)
        {
            Result!.AddPart(part);
        }

        private TStepPart CreatePart<TStepPart>(ParserRuleContext ctxt)
            where TStepPart : DefinitionContentPart, new()
        {
            var part = new TStepPart();
            part.Text = ctxt.GetText();
            PositionalLineInfo(part, ctxt);
            return part;
        }

        private TStepPart CreatePart<TStepPart>(ITerminalNode ctxt)
            where TStepPart : DefinitionContentPart, new()
        {
            var part = new TStepPart();
            part.Text = ctxt.GetText();
            PositionalLineInfo(part, ctxt);
            return part;
        }
    }
}
