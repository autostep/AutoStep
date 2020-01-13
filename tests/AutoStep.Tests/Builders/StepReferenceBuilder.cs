using System;
using AutoStep.Elements;
using AutoStep.Elements.Parts;

namespace AutoStep.Tests.Builders
{
    public class StepReferenceBuilder : BaseBuilder<StepReferenceElement>
    {
        public StepReferenceBuilder(string body, StepType type, StepType? bindingType, int line, int column)
        {
            Built = new StepReferenceElement
            {
                Type = type,
                BindingType = bindingType,
                SourceLine = line,
                SourceColumn = column,
                RawText = body
            };
        }

        public StepReferenceBuilder Word(string text, int start)
        {
            return Part<WordPart>(text, start);
        }

        public StepReferenceBuilder Word(string text, string escapedText, int start)
        {
            return Part<WordPart>(text, start, p => p.EscapedText = escapedText);
        }

        public StepReferenceBuilder Variable(string varName, int start)
        {
            return Part<VariablePart>("<" + varName + ">", start, p => p.VariableName = varName);
        }

        public StepReferenceBuilder Int(string text, int start)
        {
            return Part<IntPart>(text, start, p => p.Value = int.Parse(text));
        }

        public StepReferenceBuilder Float(string text, int start)
        {
            return Part<FloatPart>(text, start, p => p.Value = decimal.Parse(text));
        }
        
        public StepReferenceBuilder InterpolateStart(string text, int start)
        {
            return Part<InterpolatePart>(text, start);
        }

        public StepReferenceBuilder Colon(int column)
        {
            return Part<WordPart>(":", column);
        }

        public StepReferenceBuilder Quote(int column)
        {
            return Part<QuotePart>("'", column);
        }

        public StepReferenceBuilder DoubleQuote(int column)
        {
            return Part<QuotePart>("\"", column, p => p.IsDoubleQuote = true);
        }

        public StepReferenceBuilder Part<TPartType>(string text, int start, Action<TPartType> extraPart = null)
            where TPartType : ContentPart, new()
        {
            var part = new TPartType();
            part.SourceLine = Built.SourceLine;
            part.SourceColumn = start;
            part.EndColumn = start + (text.Length - 1);

            var keywordNegOffset = Built.Type.ToString().Length + 1;

            var startIdx = start - keywordNegOffset - Built.SourceColumn;
            part.TextRange = new Range(startIdx, startIdx + (text.Length - 1));

            if(extraPart is object)
            {
                extraPart(part);
            }

            Built.AddPart(part);

            return this;
        }

        public StepReferenceBuilder Table(int line, int column, Action<TableBuilder> cfg)
        {
            var tableBuilder = new TableBuilder(line, column);

            cfg(tableBuilder);

            Built.Table = tableBuilder.Built;

            return this;
        }
    }
}
