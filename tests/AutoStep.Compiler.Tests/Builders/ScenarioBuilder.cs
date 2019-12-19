using System;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class ScenarioBuilder : BaseBuilder<BuiltScenario>
    {
        public ScenarioBuilder(string name, int line, int column)
        {
            Built = new BuiltScenario
            {
                SourceLine = line,
                SourceColumn = column,
                Name = name
            };
        }

        public ScenarioBuilder Tag(string tagName, int line, int column)
        {
            Built.Annotations.Add(new TagElement
            {
                SourceLine = line,
                SourceColumn = column,
                Tag = tagName
            });

            return this;
        }

        public ScenarioBuilder Option(string optionName, int line, int column)
        {
            Built.Annotations.Add(new OptionElement
            {
                SourceLine = line,
                SourceColumn = column,
                Name = optionName
            });

            return this;
        }
        public ScenarioBuilder Option(string optionName, string setting, int line, int column)
        {
            Built.Annotations.Add(new OptionElement
            {
                SourceLine = line,
                SourceColumn = column,
                Name = optionName,
                Setting = setting
            });

            return this;
        }


        public ScenarioBuilder Description(string description)
        {
            Built.Description = description;

            return this;
        }

        public ScenarioBuilder Given(string body, int line, int column, Action<StepBuilder> cfg = null)
        {
            var stepBuilder = new StepBuilder(body, StepType.Given, StepType.Given, line, column);
            
            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            Built.Steps.Add(stepBuilder.Built);

            return this;
        }        

        public ScenarioBuilder When(string body, int line, int column, Action<StepBuilder> cfg = null)
        {
            var stepBuilder = new StepBuilder(body, StepType.When, StepType.When, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            Built.Steps.Add(stepBuilder.Built);

            return this;
        }

        public ScenarioBuilder Then(string body, int line, int column, Action<StepBuilder> cfg = null)
        {
            var stepBuilder = new StepBuilder(body, StepType.Then, StepType.Then, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            Built.Steps.Add(stepBuilder.Built);

            return this;
        }

        public ScenarioBuilder And(string body, StepType? actualType, int line, int column, Action<StepBuilder> cfg = null)
        {

            var stepBuilder = new StepBuilder(body, StepType.And, actualType, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            Built.Steps.Add(stepBuilder.Built);

            return this;
        }
    }

    public class StepBuilder : BaseBuilder<UnknownStepReference>
    {
        public StepBuilder(string body, StepType type, StepType? bindingType, int line, int column)
        {
            Built = new UnknownStepReference
            {
                Type = type,
                BindingType = bindingType,
                SourceLine = line,
                SourceColumn = column,
                RawText = body
            };
        }

        public StepBuilder Argument(ArgumentType type, string rawValue, int start, int end, Action<ArgumentBuilder> cfg = null)
        {
            var argumentBuilder = new ArgumentBuilder(Built, rawValue, type, start, end);

            if(cfg is object)
            {
                cfg(argumentBuilder);
            }

            Built.AddArgument(argumentBuilder.Built);

            return this;
        }
    }

    public class ArgumentBuilder : BaseBuilder<StepArgument>
    {
        private bool moddedValue = false;

        public ArgumentBuilder(StepReference containingStep, string rawValue, ArgumentType type, int start, int end)
        {
            Built = new StepArgument
            {
                SourceLine = containingStep.SourceLine,
                Type = type,
                RawArgument = rawValue,
                UnescapedArgument = rawValue,
                Value = rawValue,
                SourceColumn = start,
                EndColumn = end
            };
        }

        public ArgumentBuilder Unescaped(string value)
        {
            Built.UnescapedArgument = value;
            
            if(!moddedValue)
            {
                Built.Value = value;
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
