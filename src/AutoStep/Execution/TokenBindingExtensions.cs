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
            var tokens = binding.MatchedTokens.AsSpan();

            if (binding.StartExclusive)
            {
                tokens = tokens.Slice(1);
            }

            if (binding.EndExclusive)
            {
                tokens = tokens.Slice(0, tokens.Length - 1);
            }

            var workingSpace = new StepToken[tokens.Length];
            int textSize = 0;

            // Do a first pass to determine string length and variable size.
            for (int tokenIdx = 0; tokenIdx < tokens.Length; tokenIdx++)
            {
                var tok = tokens[tokenIdx];

                if (tok is VariableToken variable)
                {

                }
                else if (tok is InterpolateStartToken inter)
                {
                    // We'll come back to this.
                    throw new NotImplementedException();
                }
            }

            return string.Empty;
        }
    }
}
