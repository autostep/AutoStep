using System;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction
{
    using static AutoStepInteractionsParser;

    internal static class InteractionsParserDiagnosticExtensions
    {
        /// <summary>
        /// Generate a parse tree with nesting that represents the outcome of a parse process.
        /// </summary>
        /// <param name="context">The context to start from.</param>
        /// <param name="parser">The parser that generated the rule context.</param>
        /// <returns>A textual representation of the parse tree.</returns>
        public static string GetParseTreeDebugText(this ParserRuleContext context, AutoStepInteractionsParser parser)
        {
            var listener = new AutoStepInteractionsListener(parser);

            var treeWalker = new ParseTreeWalker();
            treeWalker.Walk(listener, context);

            return listener.Result;
        }

        private class AutoStepInteractionsListener : AutoStepInteractionsParserBaseListener
        {
            private readonly StringBuilder output;
            private readonly AutoStepInteractionsParser parser;
            private int indendation = 0;

            public AutoStepInteractionsListener(AutoStepInteractionsParser parser)
            {
                output = new StringBuilder();
                this.parser = parser;
            }

            public string Result => output.ToString();

            public override void EnterEveryRule(ParserRuleContext context)
            {
                var description = context switch
                {
                    TraitDefinitionContext t => "Trait: " + t.traitRefList().GetText(),
                    _ => context.ToString()
                };

                output.AppendLine($"{new string(' ', indendation * 4)}( {parser.RuleNames[context.RuleIndex]} : {description}");
                indendation++;
            }

            public override void ExitEveryRule(ParserRuleContext context)
            {
                indendation--;
                output.AppendLine($"{new string(' ', indendation * 4)})");
            }
        }
    }
}
