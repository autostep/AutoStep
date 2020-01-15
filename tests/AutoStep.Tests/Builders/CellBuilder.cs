using System;
using AutoStep.Elements;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Tests.Builders
{
    public class CellBuilder : BaseBuilder<TableCellElement>
    {
        public CellBuilder(string body, int line, int start, int end, bool relativeToTextContent = false)
            : base(relativeToTextContent)
        {
            Built = new TableCellElement
            {   
                Text = body,
                SourceLine = line,
                StartColumn = start,
                EndColumn = end
            };            
        }

        public CellBuilder Word(string text, int start)
        {
            return Part(text, start, (s, l) => new WordToken(s, l));
        }

        public CellBuilder EscapeChar(string text, string escapedText, int start)
        {
            return Part(text, start, (s, l) => new EscapedCharToken(s, l) { EscapedValue = escapedText });
        }

        public CellBuilder Variable(string varName, int start)
        {
            return Part("<" + varName + ">", start, (s, l) => new VariableToken(s, l) { VariableName = varName });
        }

        public CellBuilder Int(string text, int start)
        {
            return Part(text, start, (s, l) => new IntToken(s, l));
        }

        public CellBuilder Float(string text, int start)
        {
            return Part(text, start, (s, l) => new FloatToken(s, l));
        }

        public CellBuilder InterpolateStart(string text, int start)
        {
            return Part(text, start, (s, l) => new InterpolateStartToken(s));
        }

        public CellBuilder Colon(int column)
        {
            return Part(":", column, (s, l) => new WordToken(s, l));
        }

        public CellBuilder Quote(int column)
        {
            return Part("'", column, (s, l) => new QuoteToken(s));
        }

        public CellBuilder DoubleQuote(int column)
        {
            return Part("\"", column, (s, l) => new QuoteToken(s) { IsDoubleQuote = true });
        }

        private CellBuilder Part<TPartType>(string text, int start, Func<int, int, TPartType> creator)
            where TPartType : StepToken
        {
            var startIdx = start - Built.StartColumn;

            var part = creator(startIdx, text.Length);

            part.SourceLine = Built.SourceLine;
            part.StartColumn = start;
            part.EndColumn = start + (text.Length - 1);

            Built.AddPart(part);

            return this;
        }
    }
}
