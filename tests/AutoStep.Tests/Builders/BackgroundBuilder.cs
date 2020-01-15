using AutoStep.Elements;

namespace AutoStep.Tests.Builders
{
    public class BackgroundBuilder : BaseBuilder<BackgroundElement>, IStepCollectionBuilder<BackgroundElement>
    {

        public BackgroundBuilder(int line, int column, bool relativeToTextContent = false) : base(relativeToTextContent)
        {
            Built = new BackgroundElement
            {
                SourceLine = line,
                StartColumn = column
            };
        }
    }
}
