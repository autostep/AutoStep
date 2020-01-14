using System;
using AutoStep.Elements;
using AutoStep.Elements.Parts;

namespace AutoStep.Tests.Builders
{
    public class StepDefinitionBuilder : BaseBuilder<StepDefinitionElement>, IStepCollectionBuilder<StepDefinitionElement>
    {
        public StepDefinitionBuilder(StepType type, string declaration, int line, int column, bool relativeToTextContent = false)
            : base(relativeToTextContent)
        {
            Built = new StepDefinitionElement
            {
                SourceLine = line,
                SourceColumn = column,
                Type = type,
                Declaration = declaration
            };
        }

        public StepDefinitionBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public StepDefinitionBuilder WordPart(string word, int start)
        {
            Built.AddPart(new WordDefinitionPart
            {
                Text = word,
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = start + (word.Length - 1)
            });

            return this;
        }

        public StepDefinitionBuilder WordPart(string word, string escaped, int start)
        {
            Built.AddPart(new WordDefinitionPart
            {
                Text = word,
                EscapedText = escaped,
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = start + (word.Length - 1)
            });

            return this;
        }

        public StepDefinitionBuilder Argument(string text, string variableName, ArgumentType typeHint, int start, int end)
        {
            Built.AddPart(new ArgumentPart
            {
                Text = text,
                Name = variableName,
                TypeHint = typeHint,
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = start + (text.Length - 1)
            });

            return this;
        }

        public StepDefinitionBuilder Argument(string text, string variableName, int start)
        {
            Built.AddPart(new ArgumentPart
            {
                Text = text,
                Name = variableName,
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = start + (text.Length - 1)
            });

            return this;
        }
    }


}
