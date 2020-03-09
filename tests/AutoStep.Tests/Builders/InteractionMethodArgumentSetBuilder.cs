using System;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.StepTokens;
using AutoStep.Language;

namespace AutoStep.Tests.Builders
{

    public class InteractionMethodArgumentSetBuilder : BaseBuilder<MethodCallElement>
    {
        public InteractionMethodArgumentSetBuilder(MethodCallElement methodCall)
        {
            Built = methodCall;
        }

        public InteractionMethodArgumentSetBuilder String(string content, int startColumn)
        {
            var strArg = new InteractionStringArgumentBuilder(content, Built.SourceLine, startColumn);

            strArg.Text(content);

            strArg.Complete();

            Built.Arguments.Add(strArg.Built);

            return this;
        }

        public InteractionMethodArgumentSetBuilder String(string content, int startColumn, Action<InteractionStringArgumentBuilder> cfg)
        {
            var strArg = new InteractionStringArgumentBuilder(content, Built.SourceLine, startColumn);

            cfg(strArg);

            strArg.Complete();

            Built.Arguments.Add(strArg.Built);

            return this;
        }

        public InteractionMethodArgumentSetBuilder String(string content, int line, int startColumn)
        {
            var strArg = new InteractionStringArgumentBuilder(content, line, startColumn);

            strArg.Text(content);

            strArg.Complete();

            Built.Arguments.Add(strArg.Built);

            return this;
        }

        public InteractionMethodArgumentSetBuilder String(string content, int line, int startColumn, int endColumn)
        {
            var strArg = new InteractionStringArgumentBuilder(content, line, startColumn);

            strArg.Text(content);

            strArg.Complete();

            strArg.Built.EndColumn = endColumn;

            Built.Arguments.Add(strArg.Built);

            return this;
        }

        public InteractionMethodArgumentSetBuilder Int(int value, int startColumn)
        {
            Built.Arguments.Add(new IntMethodArgumentElement
            {
                SourceLine = Built.SourceLine,
                StartColumn = startColumn,
                EndLine = Built.SourceLine,
                EndColumn = startColumn + value.ToString().Length - 1,
                Value = value
            });

            return this;
        }

        public InteractionMethodArgumentSetBuilder Float(double value, int startColumn)
        {
            Built.Arguments.Add(new FloatMethodArgumentElement
            {
                SourceLine = Built.SourceLine,
                StartColumn = startColumn,
                EndLine = Built.SourceLine,
                EndColumn = startColumn + value.ToString().Length - 1,
                Value = value
            });

            return this;
        }

        public InteractionMethodArgumentSetBuilder Constant(string constantName, int startColumn)
        {
            Built.Arguments.Add(new ConstantMethodArgumentElement
            {
                SourceLine = Built.SourceLine,
                StartColumn = startColumn,
                EndLine = Built.SourceLine,
                ConstantName = constantName,
                EndColumn = startColumn + constantName.Length - 1
            });

            return this;
        }

        public InteractionMethodArgumentSetBuilder Variable(string varName, int startColumn)
        {
            Built.Arguments.Add(new VariableRefMethodArgumentElement
            {
                SourceLine = Built.SourceLine,
                StartColumn = startColumn,
                EndLine = Built.SourceLine,
                VariableName = varName,
                EndColumn = startColumn + varName.Length - 1
            });

            return this;
        }

        public InteractionMethodArgumentSetBuilder VariableArray(string varName, int startColumn, int endColumn, Action<VariableArrayRefBuilder> cfg)
        {
            var arg = new VariableArrayRefMethodArgument
            {
                SourceLine = Built.SourceLine,
                StartColumn = startColumn,
                EndLine = Built.SourceLine,
                VariableName = varName,
                EndColumn = endColumn
            };

            var builder = new VariableArrayRefBuilder(arg);

            cfg(builder);

            Built.Arguments.Add(arg);

            return this;
        }
    }
}
