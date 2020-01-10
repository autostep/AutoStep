using System;
using AutoStep.Elements;
using AutoStep.Elements.Parts;

namespace AutoStep.Tests.Builders
{
    public class StepDefinitionBuilder : BaseBuilder<StepDefinitionElement>, IStepCollectionBuilder<StepDefinitionElement>
    {
        public StepDefinitionBuilder(StepType type, string declaration, int line, int column)
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

        public StepDefinitionBuilder WordPart(string word, int start, int end)
        {
            Built.AddPart(new WordPart()
            {
                Text = word,
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = end
            });

            return this;
        }

        public StepDefinitionBuilder WordPart(string word, string escaped, int start, int end)
        {
            Built.AddPart(new WordPart()
            {
                Text = word,
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = end
            });

            return this;
        }
    }


}
