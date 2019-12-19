using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public class BackgroundBuilder : BaseBuilder<BuiltBackground>, IStepCollectionBuilder<BuiltBackground>
    {
        public BackgroundBuilder(int line, int column)
        {
            Built = new BuiltBackground
            {
                SourceLine = line,
                SourceColumn = column
            };
        }
    }
}
