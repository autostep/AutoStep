using System;
using AutoStep.Elements.StepTokens;
using AutoStep.Elements.Test;

namespace AutoStep.Tests.Builders
{
    public class CellBuilder : BaseBuilder<TableCellElement>
    {
        private int nextTokenIdx = 0;

        public CellBuilder(string body, int line, int start, int end)
        {
            Built = new TableCellElement
            {
                Text = body,
                SourceLine = line,
                StartColumn = start,
                EndColumn = end,
                EndLine = line
            };
        }

        public CellBuilder Text(string text)
        {
            return Part(text, (s, l) => new TextToken(s, l));
        }

        public CellBuilder EscapeChar(string text, string escapedText)
        {
            return Part(text, (s, l) => new EscapedCharToken(escapedText, s, l));
        }

        public CellBuilder Variable(string varName)
        {
            return Part("<" + varName + ">", (s, l) => new VariableToken(varName, s, l));
        }

        public CellBuilder Int(string text)
        {
            return Part(text, (s, l) => new IntToken(s, l));
        }

        public CellBuilder Float(string text)
        {
            return Part(text, (s, l) => new FloatToken(s, l));
        }

        public CellBuilder InterpolateStart()
        {
            return Part(":", (s, l) => new InterpolateStartToken(s));
        }

        public CellBuilder Colon()
        {
            return Part(":", (s, l) => new TextToken(s, l));
        }

        public CellBuilder Quote()
        {
            return Part("'", (s, l) => new QuoteToken(false, s));
        }

        public CellBuilder DoubleQuote()
        {
            return Part("\"", (s, l) => new QuoteToken(true, s));
        }

        private CellBuilder Part<TPartType>(string text, Func<int, int, TPartType> creator)
            where TPartType : StepToken
        {
            var startIdx = Built.Text.IndexOf(text, nextTokenIdx);

            if (startIdx == -1)
            {
                throw new ArgumentException("Bad text; not present in cell.");
            }

            var part = creator(startIdx, text.Length);

            part.SourceLine = Built.SourceLine;
            part.StartColumn = Built.StartColumn + startIdx;
            part.EndColumn = part.StartColumn + (text.Length - 1);
            part.EndLine = part.SourceLine;

            Built.AddToken(part);

            nextTokenIdx = startIdx + text.Length;

            return this;
        }
    }
}
