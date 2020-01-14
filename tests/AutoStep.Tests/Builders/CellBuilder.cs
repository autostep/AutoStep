using System;
using AutoStep.Elements;
using AutoStep.Elements.Parts;

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
                SourceColumn = start,
                EndColumn = end
            };            
        }

        public CellBuilder Word(string text, int start)
        {
            return Part(text, start, (s, l) => new WordPart(s, l));
        }

        public CellBuilder Word(string text, string escapedText, int start)
        {
            return Part(text, start, (s, l) => new WordPart(s, l) { EscapedText = escapedText });
        }

        public CellBuilder Variable(string varName, int start)
        {
            return Part("<" + varName + ">", start, (s, l) => new VariablePart(s, l) { VariableName = varName });
        }

        public CellBuilder Int(string text, int start)
        {
            return Part(text, start, (s, l) => new IntPart(s, l));
        }

        public CellBuilder Float(string text, int start)
        {
            return Part(text, start, (s, l) => new FloatPart(s, l));
        }

        public CellBuilder InterpolateStart(string text, int start)
        {
            return Part(text, start, (s, l) => new InterpolatePart(s, l));
        }

        public CellBuilder Colon(int column)
        {
            return Part(":", column, (s, l) => new WordPart(s, l));
        }

        public CellBuilder Quote(int column)
        {
            return Part("'", column, (s, l) => new QuotePart(s));
        }

        public CellBuilder DoubleQuote(int column)
        {
            return Part("\"", column, (s, l) => new QuotePart(s) { IsDoubleQuote = true });
        }

        private CellBuilder Part<TPartType>(string text, int start, Func<int, int, TPartType> creator)
            where TPartType : ContentPart
        {
            var startIdx = start - Built.SourceColumn;

            var part = creator(startIdx, text.Length);

            part.SourceLine = Built.SourceLine;
            part.SourceColumn = start;
            part.EndColumn = start + (text.Length - 1);

            Built.AddPart(part);

            return this;
        }
    }
}
