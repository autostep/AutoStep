using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.StepTokens;
using AutoStep.Language;

namespace AutoStep.Tests.Builders
{
    public class InteractionStringArgumentBuilder : BaseBuilder<StringMethodArgumentElement>
    {
        private readonly int startTokenIdx = 0;
        private int nextTokenIdx = 0;

        private List<StepToken> tokens;
        
        public InteractionStringArgumentBuilder(string rawText, int line, int startColumn)
        {
            Built = new StringMethodArgumentElement
            {
                StartColumn = startColumn,
                SourceLine = line,
                EndLine = line,
                Text = rawText
            };

            startTokenIdx = startColumn;
            tokens = new List<StepToken>();
        }

        public void Complete()
        {
            Built.Tokenised = new TokenisedArgumentValue(tokens.ToArray(), false, false);

            if(tokens.Count > 0)
            {
                Built.EndColumn = tokens.Last().EndColumn + 1;
            }
            else
            {
                Built.EndColumn = Built.StartColumn + 1;
            }
        }

        public InteractionStringArgumentBuilder Text(string text)
        {
            return Token(text, (s, l) => new TextToken(s, l));
        }

        public InteractionStringArgumentBuilder EscapeChar(string text, string escapedText)
        {
            return Token(text, (s, l) => new EscapedCharToken(escapedText, s, l));
        }

        public InteractionStringArgumentBuilder Variable(string varName)
        {
            return Token("<" + varName + ">", (s, l) => new VariableToken(varName, s, l));
        }

        private InteractionStringArgumentBuilder Token<TTokenType>(string text, Func<int, int, TTokenType> creator)
            where TTokenType : StepToken
        {
            var startIdx = Built.Text.IndexOf(text, nextTokenIdx);

            if (startIdx == -1)
            {
                throw new ArgumentException("Bad text; not present in text argument.");
            }

            var part = creator(startIdx, text.Length);

            part.SourceLine = Built.SourceLine;
            part.StartColumn = startTokenIdx + startIdx + 1;
            part.EndColumn = part.StartColumn + (text.Length - 1);
            part.EndLine = Built.SourceLine;

            tokens.Add(part);

            nextTokenIdx = startIdx + text.Length;

            return this;
        }
    }
}
