using System;
using AutoStep.Elements;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;

namespace AutoStep.Tests.Builders
{
    public class InteractionStepDefinitionBuilder : BaseBuilder<InteractionStepDefinitionElement>
    {
        public InteractionStepDefinitionBuilder(StepType type, string declaration, int line, int column)
        {
            Built = new InteractionStepDefinitionElement
            {
                SourceLine = line,
                StartColumn = column,
                Type = type,
                Declaration = declaration
            };
        }

        public InteractionStepDefinitionBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public InteractionStepDefinitionBuilder WordPart(string word, int start)
        {
            Built.AddPart(new WordDefinitionPart(word)
            {
                SourceLine = Built.SourceLine,
                StartColumn = start,
                EndColumn = start + (word.Length - 1),
                EndLine = Built.SourceLine,
            });

            return this;
        }

        public InteractionStepDefinitionBuilder WordPart(string word, string escaped, int start)
        {
            Built.AddPart(new WordDefinitionPart(word)
            {
                // TODO:
                // EscapedText = escaped,
                SourceLine = Built.SourceLine,
                StartColumn = start,
                EndColumn = start + (word.Length - 1),
                EndLine = Built.SourceLine,
            });

            return this;
        }

        public InteractionStepDefinitionBuilder Argument(string text, string variableName, ArgumentType typeHint, int start, int end)
        {
            Built.AddPart(new ArgumentPart(text, variableName, typeHint)
            {
                SourceLine = Built.SourceLine,
                StartColumn = start,
                EndColumn = start + (text.Length - 1),
                EndLine = Built.SourceLine,
            });

            return this;
        }

        public InteractionStepDefinitionBuilder Argument(string text, string variableName, int start)
        {
            Built.AddPart(new ArgumentPart(text, variableName)
            {
                SourceLine = Built.SourceLine,
                StartColumn = start,
                EndColumn = start + (text.Length - 1),
                EndLine = Built.SourceLine,
            });

            return this;
        }
        public InteractionStepDefinitionBuilder ComponentMatch(int start)
        {
            Built.AddPart(new ComponentMatchPart
            {
                SourceLine = Built.SourceLine,
                StartColumn = start,
                EndColumn = start + ("$component$".Length - 1),
                EndLine = Built.SourceLine,
            });

            return this;
        }

        public InteractionStepDefinitionBuilder Expression(Action<InteractionMethodCallChainBuilder<InteractionStepDefinitionElement>> expr)
        {
            var builder = new InteractionMethodCallChainBuilder<InteractionStepDefinitionElement>(Built);

            expr(builder);

            return this;
        }
    }
}
