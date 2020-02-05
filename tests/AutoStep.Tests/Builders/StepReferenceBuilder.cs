using System;
using AutoStep.Elements.StepTokens;
using AutoStep.Elements.Test;

namespace AutoStep.Tests.Builders
{
    public class StepReferenceBuilder : BaseBuilder<StepReferenceElement>
    {
        private int nextTokenIdx = 0;
        private readonly int startTokenIdx = 0;

        public StepReferenceBuilder(string body, StepType type, StepType? bindingType, int line, int column)
        {
            Built = new StepReferenceElement
            {
                Type = type,
                BindingType = bindingType,
                SourceLine = line,
                StartColumn = column,
                RawText = body
            };

            startTokenIdx = column + type.ToString().Length;
        }

        public StepReferenceBuilder Text(string text)
        {
            return Part(text, (s, l) => new TextToken(s, l));
        }

        public StepReferenceBuilder EscapeChar(string text, string escapedText)
        {
            return Part(text, (s, l) => new EscapedCharToken(escapedText, s, l));
        }

        public StepReferenceBuilder Variable(string varName)
        {
            return Part("<" + varName + ">", (s, l) => new VariableToken(varName, s, l));
        }

        public StepReferenceBuilder Int(string text)
        {
            return Part(text, (s, l) => new IntToken(s, l));
        }

        public StepReferenceBuilder Float(string text)
        {
            return Part(text, (s, l) => new FloatToken(s, l));
        }
        
        public StepReferenceBuilder InterpolateStart()
        {
            return Part(":", (s, l) => new InterpolateStartToken(s));
        }

        public StepReferenceBuilder Colon()
        {
            return Part(":", (s, l) => new TextToken(s, l));
        }

        public StepReferenceBuilder Quote()
        {
            return Part("'", (s, l) => new QuoteToken(false, s));
        }

        public StepReferenceBuilder DoubleQuote()
        {
            return Part("\"", (s, l) => new QuoteToken(true, s));
        }

        private StepReferenceBuilder Part<TPartType>(string text, Func<int, int, TPartType> creator)
            where TPartType : StepToken
        {
            var startIdx = Built.RawText.IndexOf(text, nextTokenIdx);

            if(startIdx == -1)
            {
                throw new ArgumentException("Bad text; not present in step definition.");
            }
            
            var part = creator(startIdx, text.Length);

            part.SourceLine = Built.SourceLine;
            part.StartColumn = startTokenIdx + startIdx + 1;
            part.EndColumn = part.StartColumn + (text.Length - 1);

            Built.AddToken(part);
            
            nextTokenIdx = startIdx + text.Length;

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
