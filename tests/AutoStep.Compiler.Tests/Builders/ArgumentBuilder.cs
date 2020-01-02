using System.Collections.Generic;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class ArgumentBuilder : BaseBuilder<StepArgument>
    {
        private bool moddedValue = false;
        private bool customSections = false;
        private List<ArgumentSection> addedSections = new List<ArgumentSection>();

        public ArgumentBuilder(StepReference containingStep, string rawValue, ArgumentType type, int start, int end)
        {
            Built = new StepArgument
            {
                SourceLine = containingStep.SourceLine,
                Type = type,
                RawArgument = rawValue,
                EscapedArgument = rawValue,
                Value = rawValue,
                SourceColumn = start,
                EndColumn = end
            };

            AddDefaultSection(delimiterOffset: 1);
        }

        private void AddDefaultSection(int delimiterOffset = 0)
        {
            if (Built.Type == ArgumentType.Interpolated || Built.Type == ArgumentType.Text)
            {
                var startOffset = delimiterOffset;
                if(Built.Type == ArgumentType.Interpolated)
                {
                    startOffset++;
                }

                Built.ReplaceSections(new[]
                {
                    new ArgumentSection
                    {
                        SourceColumn = Built.SourceColumn + startOffset,
                        EndColumn = Built.EndColumn - delimiterOffset,
                        SourceLine= Built.SourceLine,
                        RawText = Built.RawArgument,
                        EscapedText = Built.EscapedArgument
                    }
                });
            }
        }

        public ArgumentBuilder(TableCell containingCell, string rawValue, ArgumentType type, int start, int end)
        {
            Built = new StepArgument
            {
                SourceLine = containingCell.SourceLine,
                Type = type,
                RawArgument = rawValue,
                EscapedArgument = rawValue,
                Value = rawValue,
                SourceColumn = start,
                EndColumn = end
            };

            AddDefaultSection();
        }

        public ArgumentBuilder Section(string sectionText, int start, int end)
        {
            customSections = true;

            addedSections.Add(new ArgumentSection
            {
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = end,
                RawText = sectionText,
                EscapedText = sectionText
            });

            Built.ReplaceSections(addedSections);

            return this;
        }
        public ArgumentBuilder Section(string sectionText, string escapedText, int start, int end)
        {
            customSections = true;

            addedSections.Add(new ArgumentSection
            {
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = end,
                RawText = sectionText,
                EscapedText = escapedText
            });

            Built.ReplaceSections(addedSections);

            return this;
        }
        public ArgumentBuilder ExampleVariable(string variableName, int start, int end)
        {
            customSections = true;

            addedSections.Add(new ArgumentSection
            {
                SourceLine = Built.SourceLine,
                SourceColumn = start,
                EndColumn = end,
                RawText = "<" + variableName + ">",
                EscapedText = "<" + variableName + ">",
                ExampleInsertionName = variableName
            });

            Built.ReplaceSections(addedSections);

            return this;
        }

        public ArgumentBuilder Escaped(string value)
        {
            Built.EscapedArgument = value;
            
            if(!moddedValue)
            {
                Built.Value = value;
            }

            if(!customSections)
            {
                Built.Sections[0].EscapedText = value;
            }

            return this;
        }

        public ArgumentBuilder Value(int value)
        {
            Built.Value = value;
            moddedValue = true;

            return this;
        }

        public ArgumentBuilder NullValue()
        {
            Built.Value = null;
            moddedValue = true;

            return this;
        }

        public ArgumentBuilder Value(decimal value)
        {
            Built.Value = value; 
            moddedValue = true;

            return this;
        }

        public ArgumentBuilder Symbol(string symbol)
        {
            Built.Symbol = symbol;            

            return this;
        }
    }


}
