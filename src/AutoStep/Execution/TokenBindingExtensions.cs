using System;
using System.Diagnostics;
using System.Linq;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Metadata;
using AutoStep.Elements.StepTokens;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language;

namespace AutoStep.Execution
{
    /// <summary>
    /// Extension methods for retrieving text for entities that contain tokens.
    /// </summary>
    public static class TokenBindingExtensions
    {
        /// <summary>
        /// Get the full text for a step argument.
        /// </summary>
        /// <param name="binding">The bound argument.</param>
        /// <param name="scope">The current execution scope.</param>
        /// <param name="rawText">The raw text of the step.</param>
        /// <param name="variables">The variables currently in scope.</param>
        /// <returns>The resolved text.</returns>
        public static string GetFullText(this TokenisedArgumentValue binding, IServiceProvider scope, string? rawText, VariableSet variables)
        {
            binding = binding.ThrowIfNull(nameof(binding));
            variables = variables.ThrowIfNull(nameof(variables));
            scope.ThrowIfNull(nameof(scope));

            if (string.IsNullOrEmpty(rawText))
            {
                throw new ArgumentException(ExecutionText.TokenBindingExtensions_TextNotSupplied, nameof(rawText));
            }

            return GetFullText(binding, scope, rawText, n => variables.Get(n));
        }

        /// <summary>
        /// Get the full text for a method context argument.
        /// </summary>
        /// <param name="arg">The bound argument.</param>
        /// <param name="scope">The current execution scope.</param>
        /// <param name="context">The method context (includes any variables).</param>
        /// <returns>The resolved text.</returns>
        public static string GetFullText(this StringMethodArgumentElement arg, IServiceProvider scope, MethodContext context)
        {
            arg = arg.ThrowIfNull(nameof(arg));
            scope.ThrowIfNull(nameof(scope));

            if (arg.Tokenised is null)
            {
                return string.Empty;
            }

            return GetFullText(arg.Tokenised, scope, arg.Text, n => context.Variables.Get(n)?.ToString());
        }

        /// <summary>
        /// Get the full text for a table cell.
        /// </summary>
        /// <param name="cell">The bound argument.</param>
        /// <param name="scope">The current execution scope.</param>
        /// <param name="variables">The variables currently in scope.</param>
        /// <returns>The resolved text.</returns>
        public static string GetFullText(this ITableCellInfo cell, IServiceProvider scope, VariableSet variables)
        {
            cell = cell.ThrowIfNull(nameof(cell));
            variables = variables.ThrowIfNull(nameof(variables));
            scope.ThrowIfNull(nameof(scope));

            // Ok, so we need to go get the raw text from the matched tokens.
            var tokens = cell.Tokens;

            if (tokens.Count == 0 || string.IsNullOrEmpty(cell.Text))
            {
                return string.Empty;
            }

            var lastStopIdx = 0;

            var foundVariables = new string?[tokens.Count];

            int textSize = 0;

            // Do a first pass to determine string length and variable size.
            for (int tokenIdx = 0; tokenIdx < tokens.Count; tokenIdx++)
            {
                var currentToken = tokens[tokenIdx];

                if (tokenIdx > 0)
                {
                    // Add the space between this and the last token.
                    textSize += currentToken.StartIndex - lastStopIdx;
                }

                // Add text size for the token itself.
                if (currentToken is VariableToken variable)
                {
                    var variableText = variables.Get(variable.VariableName) ?? string.Empty;
                    foundVariables[tokenIdx] = variableText;
                    textSize += variableText.Length;
                }
                else if (currentToken is InterpolateStartToken)
                {
                    // We'll come back to this.
                    textSize += currentToken.Length;
                }
                else
                {
                    textSize += currentToken.Length;
                }

                lastStopIdx = currentToken.StartIndex + currentToken.Length;
            }

            var createdString = string.Create(textSize, tokens, (chars, tokens) =>
            {
                var textSpan = cell.Text.AsSpan();

                // Now we loop over the tokens and do our actual copy.
                for (int tokenIdx = 0; tokenIdx < tokens.Count; tokenIdx++)
                {
                    var currentToken = tokens[tokenIdx];

                    if (tokenIdx > 0)
                    {
                        // Add the space between this and the last token.
                        var whiteSpaceLength = currentToken.StartIndex - lastStopIdx;

                        textSpan.Slice(lastStopIdx, whiteSpaceLength).CopyTo(chars);

                        // Move the chars along.
                        chars = chars.Slice(whiteSpaceLength);
                    }

                    var knownVariable = foundVariables[tokenIdx];

                    if (knownVariable is null)
                    {
                        // Copy the contents of the token.
                        textSpan.Slice(currentToken.StartIndex, currentToken.Length).CopyTo(chars);
                        chars = chars.Slice(currentToken.Length);
                    }
                    else
                    {
                        knownVariable.AsSpan().CopyTo(chars);
                        chars = chars.Slice(knownVariable.Length);
                    }

                    lastStopIdx = currentToken.StartIndex + currentToken.Length;
                }
            });

            return createdString;
        }

        /// <summary>
        /// Gets the raw (un-expanded) text for an argument.
        /// </summary>
        /// <param name="binding">The bound argument.</param>
        /// <param name="rawText">The raw text.</param>
        /// <returns>The literal bound argument text.</returns>
        public static string GetRawText(this TokenisedArgumentValue binding, string rawText)
        {
            if (binding is null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            var tokens = binding.MatchedTokens;

            if (tokens.Length == 0 || string.IsNullOrEmpty(rawText))
            {
                return string.Empty;
            }

            var startPos = tokens[0].StartIndex;

            var lastToken = tokens[^1];
            var length = (lastToken.StartIndex - startPos) + lastToken.Length;

            if (binding.StartExclusive)
            {
                startPos++;
                length--;
            }

            if (binding.EndExclusive)
            {
                length--;
            }

            return rawText.Substring(startPos, length);
        }

        /// <summary>
        /// Get the raw length of an argument value in the raw text.
        /// </summary>
        /// <param name="binding">The argument binding.</param>
        /// <returns>The length of the binding's raw text.</returns>
        public static int GetRawLength(this TokenisedArgumentValue binding)
        {
            if (binding is null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            var tokens = binding.MatchedTokens;

            if (tokens.Length == 0)
            {
                return 0;
            }

            var lastToken = tokens[^1];
            var length = (lastToken.StartIndex - tokens[0].StartIndex) + lastToken.Length;

            if (binding.StartExclusive)
            {
                length--;
            }

            if (binding.EndExclusive)
            {
                length--;
            }

            return length;
        }

        private static string GetFullText(TokenisedArgumentValue binding, IServiceProvider scope, string rawText, Func<string, string?> getVariableValue)
        {
            // Ok, so we need to go get the raw text from the matched tokens.
            var tokens = binding.MatchedTokens;

            if (tokens.Length == 0)
            {
                return string.Empty;
            }

            var lastStopIdx = 0;

            var foundVariables = new string?[tokens.Length];

            int textSize = 0;

            // Do a first pass to determine string length and variable size.
            for (int tokenIdx = 0; tokenIdx < tokens.Length; tokenIdx++)
            {
                var currentToken = tokens[tokenIdx];

                if (binding.StartExclusive && tokenIdx == 0)
                {
                    lastStopIdx = currentToken.StartIndex + currentToken.Length;

                    // Just skip.
                    continue;
                }

                if (tokenIdx > 0)
                {
                    // Add the space between this and the last token.
                    textSize += currentToken.StartIndex - lastStopIdx;
                }

                if (!binding.EndExclusive || tokenIdx != tokens.Length - 1)
                {
                    // Add text size for the token itself.
                    if (currentToken is VariableToken variable)
                    {
                        var variableText = getVariableValue(variable.VariableName) ?? string.Empty;
                        foundVariables[tokenIdx] = variableText;
                        textSize += variableText.Length;
                    }
                    else if (currentToken is InterpolateStartToken)
                    {
                        // We'll come back to this.
                        // This is where we need the service scope.
                        Debug.Assert(scope is object);

                        textSize += currentToken.Length;
                    }
                    else
                    {
                        textSize += currentToken.Length;
                    }

                    lastStopIdx = currentToken.StartIndex + currentToken.Length;
                }
            }

            var createdString = string.Create(textSize, tokens, (chars, tokens) =>
            {
                var textSpan = rawText.AsSpan();

                // Now we loop over the tokens and do our actual copy.
                for (int tokenIdx = 0; tokenIdx < tokens.Length; tokenIdx++)
                {
                    var currentToken = tokens[tokenIdx];

                    if (binding.StartExclusive && tokenIdx == 0)
                    {
                        lastStopIdx = currentToken.StartIndex + currentToken.Length;

                        // Just skip.
                        continue;
                    }

                    if (tokenIdx > 0)
                    {
                        // Add the space between this and the last token.
                        var whiteSpaceLength = currentToken.StartIndex - lastStopIdx;

                        textSpan.Slice(lastStopIdx, whiteSpaceLength).CopyTo(chars);

                        // Move the chars along.
                        chars = chars.Slice(whiteSpaceLength);
                    }

                    if (!binding.EndExclusive || tokenIdx != tokens.Length - 1)
                    {
                        var knownVariable = foundVariables[tokenIdx];

                        if (knownVariable is null)
                        {
                            // Copy the contents of the token.
                            textSpan.Slice(currentToken.StartIndex, currentToken.Length).CopyTo(chars);
                            chars = chars.Slice(currentToken.Length);
                        }
                        else
                        {
                            knownVariable.AsSpan().CopyTo(chars);
                            chars = chars.Slice(knownVariable.Length);
                        }

                        lastStopIdx = currentToken.StartIndex + currentToken.Length;
                    }
                }
            });

            return createdString;
        }
    }
}
