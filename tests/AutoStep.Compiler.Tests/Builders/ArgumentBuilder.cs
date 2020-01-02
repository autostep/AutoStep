using System.Collections.Generic;
using AutoStep.Core;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler.Tests.Builders
{
    public class ArgumentBuilder : BaseBuilder<StepArgumentElement>
    {
        private bool moddedValue = false;
        private bool customSections = false;
        private List<ArgumentSectionElement> addedSections = new List<ArgumentSectionElement>();

        public ArgumentBuilder(StepDefinitionElement stepDefinition, string rawValue, ArgumentType type, int start, int end)
        {
            Built = new StepArgumentElement
            {
                SourceLine = stepDefinition.SourceLine,
                Type = type,
                RawArgument = rawValue,
                EscapedArgument = rawValue,
                Value = rawValue,
                SourceColumn = start,
                EndColumn = end
            };

            AddDefaultSection(delimiterOffset: 1);
        }

        public ArgumentBuilder(StepReferenceElement containingStep, string rawValue, ArgumentType type, int start, int end)
        {
            Built = new StepArgumentElement
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
                    new ArgumentSectionElement
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

        public ArgumentBuilder(TableCellElement containingCell, string rawValue, ArgumentType type, int start, int end)
        {
            Built = new StepArgumentElement
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

            addedSections.Add(new ArgumentSectionElement
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

            addedSections.Add(new ArgumentSectionElement
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
        public ArgumentBuilder VariableInsertion(string variableName, int start, int end)
        {
            customSections = true;

            addedSections.Add(new ArgumentSectionElement
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
