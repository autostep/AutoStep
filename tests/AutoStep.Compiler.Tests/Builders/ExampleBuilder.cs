using System;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class ExampleBuilder : BaseBuilder<BuiltExample>
    {
        public ExampleBuilder(int line, int column)
        {
            Built = new BuiltExample
            {
                SourceLine = line,
                SourceColumn = column
            };
        }
        
        public ExampleBuilder Table(int line, int column, Action<TableBuilder> cfg)
        {
            var tableBuilder = new TableBuilder(line, column);

            cfg(tableBuilder);

            Built.Table = tableBuilder.Built;

            return this;
        }
    }


}
