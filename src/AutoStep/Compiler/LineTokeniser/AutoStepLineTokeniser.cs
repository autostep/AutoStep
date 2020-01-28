using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Compiler;
using AutoStep.Compiler.Parser;
using static AutoStep.Compiler.Parser.AutoStepParser;

namespace AutoStep
{

    public enum LineTokenCategory
    {
        Annotation,
        StepTypeKeyword,

        /// <summary>
        /// The entrance to a block, such as Feature:, Scenario:, Step: etc.
        /// </summary>
        EntryMarker,

        /// <summary>
        /// The declaration of an argument in a Step Definition.
        /// </summary>
        ArgumentDeclaration,

        /// <summary>
        /// Non-argument text content.
        /// </summary>
        TextDeclaration,
        Comment,
        EntityName
    }

    public enum LineTokenSubCategory
    {
        None,
        Tag,
        Option,
        StepDefine,
        Feature,
        Background,
        Scenario,
        ScenarioOutline,
        Examples,
        Given,
        When,
        Then,
        And
    }

    public enum LineTokeniserState
    {
        Default
    }

    public struct LineToken
    {
        public LineToken(int startPosition, LineTokenCategory category, LineTokenSubCategory subCategory = default)
        {
            StartPosition = startPosition;
            Category = category;
            SubCategory = subCategory;
        }

        public int StartPosition { get; }

        public LineTokenCategory Category { get; }

        public LineTokenSubCategory SubCategory { get; }
    }

    public class AutoStepLineTokeniser
    {
        private readonly IAutoStepLinker linker;

        public AutoStepLineTokeniser(IAutoStepLinker linker)
        {
            this.linker = linker;
        }

        public LineTokeniseResult Tokenise(string text, LineTokeniserState lastState)
        {
            var parseTree = CompileLine(text, out var tokenStream);
            var visitor = new AutoStepLineVisitor();

            return parseTree switch
            {
                OnlyLineContext ctxt => visitor.Visit(ctxt),
                // null means that the parser really had no idea, so yield an empty token set.
                null => new LineTokeniseResult(0, Enumerable.Empty<LineToken>()),
                _ => throw new InvalidOperationException("Unknown parse context")
            };
        }

        private LineTokeniseResult ProcessLineStepDefine(LineStepDefineContext lineStepDef)
        {
            
        }

        internal AutoStepParser.OnlyLineContext CompileLine(string line, out CommonTokenStream tokenStream)
        {
            var inputStream = new AntlrInputStream(line);
            var lexer = new AutoStepLexer(inputStream);

            tokenStream = new CommonTokenStream(lexer);

            // Create a parser and register our error listener.
            var parser = new AutoStepParser(tokenStream);

            return parser.onlyLine();
        }
    }
}
