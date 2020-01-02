using System;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler.Tests.Builders
{
    public class ExampleBuilder : BaseBuilder<ExampleElement>
    {
        public ExampleBuilder(int line, int column)
        {
            Built = new ExampleElement
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
