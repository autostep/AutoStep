using System;
using AutoStep.Elements;

namespace AutoStep.Tests.Builders
{
    public class ExampleBuilder : BaseBuilder<ExampleElement>
    {
        public ExampleBuilder(int line, int column, bool relativeToTextContent = false)
            : base(relativeToTextContent)
        {
            Built = new ExampleElement
            {
                SourceLine = line,
                SourceColumn = column
            };
        }
        
        public ExampleBuilder Table(int line, int column, Action<TableBuilder> cfg)
        {
            var tableBuilder = new TableBuilder(line, column, RelativeToTextContent);

            cfg(tableBuilder);

            Built.Table = tableBuilder.Built;

            return this;
        }
    }


}
