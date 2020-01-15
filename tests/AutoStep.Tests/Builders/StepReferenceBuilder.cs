using System;
using AutoStep.Elements;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Tests.Builders
{
    public class StepReferenceBuilder : BaseBuilder<StepReferenceElement>
    {
        public StepReferenceBuilder(string body, StepType type, StepType? bindingType, int line, int column, bool relativeToTextContent = false)
            : base(relativeToTextContent)
        {
            Built = new StepReferenceElement
            {
                Type = type,
                BindingType = bindingType,
                SourceLine = line,
                StartColumn = column,
                RawText = body
            };
        }

        public StepReferenceBuilder Word(string text, int start)
        {
            return Part(text, start, (s, l) => new WordToken(s, l));
        }

        public StepReferenceBuilder EscapeChar(string text, string escapedText, int start)
        {
            return Part(text, start, (s, l) => new EscapedCharToken(s, l) { EscapedValue = escapedText });
        }

        public StepReferenceBuilder Variable(string varName, int start)
        {
            return Part("<" + varName + ">", start, (s, l) => new VariableToken(s, l) { VariableName = varName });
        }

        public StepReferenceBuilder Int(string text, int start)
        {
            return Part(text, start, (s, l) => new IntToken(s, l));
        }

        public StepReferenceBuilder Float(string text, int start)
        {
            return Part(text, start, (s, l) => new FloatToken(s, l));
        }
        
        public StepReferenceBuilder InterpolateStart(string text, int start)
        {
            return Part(text, start, (s, l) => new InterpolateStartToken(s));
        }

        public StepReferenceBuilder Colon(int column)
        {
            return Part(":", column, (s, l) => new WordToken(s, l));
        }

        public StepReferenceBuilder Quote(int column)
        {
            return Part("'", column, (s, l) => new QuoteToken(s));
        }

        public StepReferenceBuilder DoubleQuote(int column)
        {
            return Part("\"", column, (s, l) => new QuoteToken(s) { IsDoubleQuote = true });
        }

        private StepReferenceBuilder Part<TPartType>(string text, int start, Func<int, int, TPartType> creator)
            where TPartType : StepToken
        {
            var startIdx = start - Built.StartColumn;

            // Adjust for the keyword in the line.
            if(RelativeToTextContent)
            {
                startIdx -= Built.Type.ToString().Length + 1;
            }

            if(startIdx < 0)
            {
                throw new ArgumentException("Supplied start column occurs before the declared beginning of the line.", nameof(start));
            }
            
            var part = creator(startIdx, text.Length);

            part.SourceLine = Built.SourceLine;
            part.StartColumn = start;
            part.EndColumn = start + (text.Length - 1);

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
