using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    internal static class ParserDiagnosticExtensions
    {

        public static string GetTokenDebugText(this CommonTokenStream tokenStream, IVocabulary vocab)
        {
            return string.Join('\n', tokenStream.GetTokens().Select(x => $"{vocab.GetDisplayName(x.Type)} {x}"));
        }

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
                    AutoStepParser.RULE_statementSection => GetStatementSectionType(context) + " : " + context.GetText(),
                    AutoStepParser.RULE_statementTextContentBlock => context.GetText(),
                    _ => context.ToString()
                };

                output.AppendLine($"{new string(' ', indendation * 4)}( {parser.RuleNames[context.RuleIndex]} : {description}");
                indendation++;
            }

            private string GetStatementSectionType(ParserRuleContext context)
            {
                return context switch
                {
                    AutoStepParser.StatementSectionPartContext _ => "statementSectionPart",
                    AutoStepParser.StatementWsContext _ => "statementWs",
                    AutoStepParser.ArgEmptyContext _ => "argEmpty",
                    AutoStepParser.ArgInterpolateContext _ => "argInterpolate",
                    AutoStepParser.ArgTextContext _ => "argText",
                    AutoStepParser.ArgFloatContext _ => "argFloat",
                    AutoStepParser.ArgIntContext _ => "argInt",
                    _ => throw new ArgumentException($"Unexpected statement section alternate, context type {context.GetType().Name}", nameof(context))
                };
            }

            public override void ExitEveryRule(ParserRuleContext context)
            {
                indendation--;
                output.AppendLine($"{new string(' ', indendation * 4)})");
            }
        }
    }
}
