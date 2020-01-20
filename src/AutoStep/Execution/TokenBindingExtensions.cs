using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Compiler;
using AutoStep.Elements.StepTokens;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution
{
    public static class TokenBindingExtensions
    {
        public static string GetFullText(this ArgumentBinding binding, IServiceScope scope, string rawText, VariableSet variables)
        {
            if (binding is null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            if (string.IsNullOrEmpty(rawText))
            {
                throw new ArgumentException("Text must be supplied.", nameof(rawText));
            }

            if (variables is null)
            {
                throw new ArgumentNullException(nameof(variables));
            }

            // Ok, so we need to go get the raw text from the matched tokens.
            var tokens = binding.MatchedTokens;

            if (tokens.Length == 0)
            {
                return string.Empty;
            }

            var lastToken = tokens[0];
            var lastStopIdx = 0;
            var currentTextReadIdx = lastToken.StartIndex;

            var startTokIdx = 0;
            var length = tokens.Length;

            var foundVariables = new string?[tokens.Length];

            int textSize = 0;

            // Do a first pass to determine string length and variable size.
            for (int tokenIdx = 0; tokenIdx < tokens.Length; tokenIdx++)
            {
                var currentToken = tokens[tokenIdx];

                if (binding.StartExclusive && tokenIdx == 0)
                {
                    lastToken = currentToken;
                    lastStopIdx = lastToken.StartIndex + lastToken.Length;

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
                        var variableText = variables.Get(variable.VariableName);
                        foundVariables[tokenIdx] = variableText;
                        textSize += variableText.Length;
                    }
                    else if (currentToken is InterpolateStartToken inter)
                    {
                        // We'll come back to this.
                        throw new NotImplementedException();
                    }
                    else
                    {
                        textSize += currentToken.Length;
                    }

                    lastToken = currentToken;
                    lastStopIdx = lastToken.StartIndex + lastToken.Length;
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
                        lastToken = currentToken;
                        lastStopIdx = lastToken.StartIndex + lastToken.Length;

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

                        lastToken = currentToken;
                        lastStopIdx = lastToken.StartIndex + lastToken.Length;
                    }
                }
            });

            return createdString;
        }
    }
}
