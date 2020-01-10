using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Provides extensions to assist in rendering debug data inside the compiler.
    /// </summary>
    internal static class ParserDiagnosticExtensions
    {
        /// <summary>
        /// Generate token debug text for a token stream.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="vocab">The token vocabulary.</param>
        /// <returns>The textual representation of the tokens.</returns>
        public static string GetTokenDebugText(this CommonTokenStream tokenStream, IVocabulary vocab)
        {
            return string.Join('\n', tokenStream.GetTokens().Select(x => $"{vocab.GetDisplayName(x.Type)} {x}"));
        }

        /// <summary>
        /// Generate a parse tree with nesting that represents the outcome of a parse process.
        /// </summary>
        /// <param name="context">The context to start from.</param>
        /// <param name="parser">The parser that generated the rule context.</param>
        /// <returns>A textual representation of the parse tree.</returns>
        public static string GetParseTreeDebugText(this ParserRuleContext context, AutoStepParser parser)
        {
            var listener = new ParseDebugListener(parser);

            var treeWalker = new ParseTreeWalker();
            treeWalker.Walk(listener, context);

            return listener.Result;
        }

        private class ParseDebugListener : AutoStepParserBaseListener
        {
            private readonly StringBuilder output;
            private readonly AutoStepParser parser;
            private int indendation = 0;

            public ParseDebugListener(AutoStepParser parser)
            {
                output = new StringBuilder();
                this.parser = parser;
            }

            public string Result => output.ToString();

            public override void EnterEveryRule(ParserRuleContext context)
            {
                var description = context.RuleIndex switch
                {
                    AutoStepParser.RULE_line => context.GetText(),
                    AutoStepParser.RULE_text => context.GetText(),
                    AutoStepParser.RULE_statement => context.GetText(),
                    AutoStepParser.RULE_statementSectionBlock => GetStatementSectionType(context) + " : " + context.GetText(),
                    AutoStepParser.RULE_stepDeclaration => context.GetText(),
                    AutoStepParser.RULE_stepDeclarationSection => context.GetText(),
                    AutoStepParser.RULE_cellContentBlock => context.GetText(),
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

            private string GetStatementSectionType(ParserRuleContext context)
            {
                return context switch
                {
                    AutoStepParser.StatementWordContext _ => "statementWord",
                    AutoStepParser.StatementEscapedCharContext _ => "statementEscapedChar",
                    AutoStepParser.StatementIntContext _ => "statementInt",
                    AutoStepParser.StatementFloatContext _ => "statementFloat",
                    AutoStepParser.StatementSymbolContext _ => "statementSymbol",
                    AutoStepParser.StatementInterpolateContext _ => "statementInterpolate",
                    AutoStepParser.StatementBlockWsContext _ => "statementWs",
                    _ => throw new ArgumentException($"Unexpected statement section alternate, context type {context.GetType().Name}", nameof(context))
                };
            }
        }
    }
}
