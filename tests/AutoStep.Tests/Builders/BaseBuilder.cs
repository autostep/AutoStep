namespace AutoStep.Tests.Builders
{
    public interface IBuilder<out TBuiltComponent>
    {
        TBuiltComponent GetBuilt();
    }

    public class BaseBuilder<TBuiltComponent> : IBuilder<TBuiltComponent>
    {
        public TBuiltComponent Built { get; set; }

        public TBuiltComponent GetBuilt()
        {
            return Built;
        }
    }
}
