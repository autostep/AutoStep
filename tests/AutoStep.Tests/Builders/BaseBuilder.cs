namespace AutoStep.Tests.Builders
{
    public interface IBuilder<out TBuiltComponent>
    {
        TBuiltComponent GetBuilt();
    }

    public class BaseBuilder<TBuiltComponent> : IBuilder<TBuiltComponent>
    {
        public TBuiltComponent Built { get; }

        public BaseBuilder(TBuiltComponent built)
        {
            Built = built;
        }

        public TBuiltComponent GetBuilt()
        {
            return Built;
        }
    }
}
