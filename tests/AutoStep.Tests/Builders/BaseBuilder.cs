namespace AutoStep.Tests.Builders
{
    public interface IBuilder<out TBuiltComponent>
    {
        TBuiltComponent GetBuilt();
    }

    public class BaseBuilder<TBuiltComponent> : IBuilder<TBuiltComponent>
    {
        protected BaseBuilder(bool relativeToTextContent)
        {
            RelativeToTextContent = relativeToTextContent;
        }

        public TBuiltComponent Built { get; set; }

        public bool RelativeToTextContent { get; }

        public TBuiltComponent GetBuilt()
        {
            return Built;
        }
    }


}
