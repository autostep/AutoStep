using System;
using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    public class VariableArrayRefBuilder : BaseBuilder<VariableArrayRefMethodArgument>
    {
        public VariableArrayRefBuilder(VariableArrayRefMethodArgument arrayRefArg)
        {
            Built = arrayRefArg;
        }

        public void String(string content, int startColumn)
        {
            var strArg = new InteractionStringArgumentBuilder(content, Built.SourceLine, startColumn);

            strArg.Text(content);

            strArg.Complete();

            Built.Indexer = strArg.Built;
        }

        public void String(string content, int startColumn, Action<InteractionStringArgumentBuilder> cfg)
        {
            var strArg = new InteractionStringArgumentBuilder(content, Built.SourceLine, startColumn);

            cfg(strArg);

            strArg.Complete();

            Built.Indexer = strArg.Built;
        }

        public void String(string content, int line, int startColumn)
        {
            var strArg = new InteractionStringArgumentBuilder(content, line, startColumn);

            strArg.Text(content);

            strArg.Complete();

            Built.Indexer = strArg.Built;
        }

        public void Variable(string varName, int startColumn)
        {
            Built.Indexer = new VariableRefMethodArgumentElement(varName)
            {
                SourceLine = Built.SourceLine,
                StartColumn = startColumn,
                EndLine = Built.SourceLine,
                EndColumn = startColumn + varName.Length - 1
            };
        }
    }    
}
