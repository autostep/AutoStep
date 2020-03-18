using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using AutoStep.Language.Interaction.Parser;

namespace AutoStep.Language.Interaction
{
    using static AutoStepInteractionsParser;

    /// <summary>
    /// Provides line tokenisation. Outputs a set of line tokens, optimised for syntax highlighting rather than full compilation.
    /// The line tokeniser is much more forgiving of errors, and generally strives to extract as much info as it can.
    /// </summary>
    internal class InteractionLineTokeniser : ILineTokeniser<int>
    {
        /// <summary>
        /// Tokenises a given line of text.
        /// </summary>
        /// <param name="text">The text to tokenise (no line terminators expected).</param>
        /// <param name="lastState">The state of the tokeniser as returned from this method for the previous line in a file.</param>
        /// <returns>The result of tokenisation.</returns>
        public LineTokeniseResult<int> Tokenise(string text, int lastState)
        {
            // For now, let's just use the lexer to do this. We'll use the actual tokens.
            var inputStream = new AntlrInputStream(text);
            var lexer = new AutoStepInteractionsLexer(inputStream);

            if (lastState != Lexer.DEFAULT_MODE)
            {
                // Put the lexer into the previous mode.
                lexer.PushMode(lastState);
            }

            var tokenStream = new CommonTokenStream(lexer);

            IToken token;
            int currentIndex = 0;
            var tokens = new List<LineToken>();

            // Just lex the tokens all at once.
            tokenStream.Fill();

            while ((token = tokenStream.Get(currentIndex)).Type != Lexer.Eof)
            {
                // Convert the token into a line token.
                if (token.Type == STEP_DEFINE)
                {
                    // Use the actual parser.
                    var parser = new AutoStepInteractionsParser(tokenStream);

                    // No errors.
                    parser.RemoveErrorListeners();

                    var stepDefineTree = parser.stepDefinition();

                    var visitor = new InteractionStepDefineLineVisitor(tokens);

                    visitor.VisitStepDefinition(stepDefineTree);

                    // Check for any comments at the end of the line.
                    var commentToken = tokenStream.GetHiddenTokensToRight(stepDefineTree.Stop.TokenIndex)?.FirstOrDefault();

                    if (commentToken is object)
                    {
                        tokens.Add(GetCommentToken(commentToken));
                    }
                }
                else if (GetLineToken(token, out var lineToken))
                {
                    tokens.Add(lineToken);
                }

                currentIndex++;
            }

            var finalMode = lexer.CurrentMode;

            // The only lexer mode in which line tokenisation can actually end is the method args (because that's the only lexer mode that crosses lines)
            if (finalMode != AutoStepInteractionsLexer.methodArgs)
            {
                finalMode = Lexer.DEFAULT_MODE;
            }

            return new LineTokeniseResult<int>(finalMode, tokens);
        }

        private static LineToken GetCommentToken(IToken commentToken)
        {
            // Token column position
            var commentColumn = commentToken.Column;

            // Skip over whitespace.
            var commentMarkerIndex = commentToken.Text.IndexOf('#', StringComparison.CurrentCulture);

            // Move the column along to ignore whitespace.
            commentColumn += commentMarkerIndex;

            return new LineToken(commentColumn, LineTokenCategory.Comment);
        }

        private static bool GetLineToken(IToken token, out LineToken lineToken)
        {
            var (primary, sub) = token.Type switch
            {
                // Default mode
                APP_DEFINITION => (LineTokenCategory.EntryMarker, LineTokenSubCategory.InteractionApp),
                TRAIT_DEFINITION => (LineTokenCategory.EntryMarker, LineTokenSubCategory.InteractionTrait),
                COMPONENT_DEFINITION => (LineTokenCategory.EntryMarker, LineTokenSubCategory.InteractionComponent),
                TRAITS_KEYWORD => (LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionTrait),
                NAME_KEYWORD => (LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionName),
                INHERITS_KEYWORD => (LineTokenCategory.InteractionPropertyName, LineTokenSubCategory.InteractionInherits),
                NEEDS_DEFINING => (LineTokenCategory.InteractionNeedsDefining, LineTokenSubCategory.None),
                NAME_REF => (LineTokenCategory.InteractionName, LineTokenSubCategory.None),
                LIST_SEPARATOR => (LineTokenCategory.InteractionSeparator, LineTokenSubCategory.None),
                METHOD_OPEN => (LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses),
                DEF_SEPARATOR => (LineTokenCategory.InteractionSeparator, LineTokenSubCategory.None),
                PLUS => (LineTokenCategory.InteractionSeparator, LineTokenSubCategory.None),
                FUNC_PASS_MARKER => (LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionCallSeparator),
                STRING => (LineTokenCategory.InteractionString, LineTokenSubCategory.None),
                TEXT_COMMENT => (LineTokenCategory.Comment, LineTokenSubCategory.None),
                TEXT_DOC_COMMENT => (LineTokenCategory.Comment, LineTokenSubCategory.None),

                // Method Arguments mode
                METHOD_STRING_START => (LineTokenCategory.InteractionString, LineTokenSubCategory.None),
                CONSTANT => (LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionConstant),
                PARAM_NAME => (LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionVariable),
                PARAM_SEPARATOR => (LineTokenCategory.InteractionSeparator, LineTokenSubCategory.None),
                ARR_LEFT => (LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionArray),
                FLOAT => (LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionLiteral),
                INT => (LineTokenCategory.InteractionArguments, LineTokenSubCategory.InteractionLiteral),
                METHOD_CLOSE => (LineTokenCategory.InteractionSeparator, LineTokenSubCategory.InteractionParentheses),

                // String Arg Mode
                STR_ANGLE_LEFT => (LineTokenCategory.Variable, LineTokenSubCategory.InteractionVariable),
                METHOD_STRING_END => (LineTokenCategory.InteractionString, LineTokenSubCategory.None),
                STR_CONTENT => (LineTokenCategory.InteractionString, LineTokenSubCategory.None),
                METHOD_STR_ESCAPE_QUOTE => (LineTokenCategory.InteractionString, LineTokenSubCategory.None),

                // Method variable mode
                STR_NAME_REF => (LineTokenCategory.Variable, LineTokenSubCategory.InteractionVariable),
                STR_ANGLE_RIGHT => (LineTokenCategory.Variable, LineTokenSubCategory.InteractionVariable),
                _ => (LineTokenCategory.Text, LineTokenSubCategory.None)
            };

            // Use this to indicate 'ignoring'.
            if (primary == LineTokenCategory.Text)
            {
                lineToken = default;
                return false;
            }

            if (primary == LineTokenCategory.Comment)
            {
                lineToken = GetCommentToken(token);
            }
            else
            {
                lineToken = new LineToken(token.Column, primary, sub);
            }

            return true;
        }
    }
}
