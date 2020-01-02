using AutoStep.Core.Elements;

namespace AutoStep.Compiler.Tests.Builders
{
    public class BackgroundBuilder : BaseBuilder<BackgroundElement>, IStepCollectionBuilder<BackgroundElement>
    {
        public BackgroundBuilder(int line, int column)
        {
            Built = new BackgroundElement
            {
                SourceLine = line,
                SourceColumn = column
            };
        }
    }
}
