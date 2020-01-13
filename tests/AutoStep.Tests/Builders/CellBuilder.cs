using System;
using AutoStep.Elements;
using AutoStep.Elements.Parts;

namespace AutoStep.Tests.Builders
{
    public class CellBuilder : BaseBuilder<TableCellElement>
    {
        public CellBuilder(string body, int line, int start, int end)
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
            return Part<WordPart>(text, start);
        }

        public CellBuilder Word(string text, string escapedText, int start)
        {
            return Part<WordPart>(text, start, p => p.EscapedText = escapedText);
        }

        public CellBuilder Variable(string varName, int start)
        {
            return Part<VariablePart>("<" + varName + ">", start, p => p.VariableName = varName);
        }

        public CellBuilder Int(string text, int start)
        {
            return Part<IntPart>(text, start, p => p.Value = int.Parse(text));
        }

        public CellBuilder Float(string text, int start)
        {
            return Part<FloatPart>(text, start, p => p.Value = decimal.Parse(text));
        }
        
        public CellBuilder InterpolateStart(string text, int start)
        {
            return Part<InterpolatePart>(text, start);
        }

        public CellBuilder Colon(int column)
        {
            return Part<WordPart>(":", column);
        }

        public CellBuilder Part<TPartType>(string text, int start, Action<TPartType> extraPart = null)
            where TPartType : ContentPart, new()
        {
            var part = new TPartType();
            part.SourceLine = Built.SourceLine;
            part.SourceColumn = start;
            part.EndColumn = start + (text.Length - 1);
            var startIdx = start - Built.SourceColumn;
            part.TextRange = new Range(startIdx, startIdx + (text.Length - 1));

            if (extraPart is object)
            {
                extraPart(part);
            }

            Built.AddPart(part);

            return this;
        }
    }
}
