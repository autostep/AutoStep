namespace AutoStep.Tests.Builders
{
    public interface IBuilder<out TBuiltComponent>
    {
        TBuiltComponent GetBuilt();
    }

    public class BaseBuilder<TBuiltComponent> : IBuilder<TBuiltComponent>
        where TBuiltComponent : class
    {
        public TBuiltComponent Built { get; set; } = null!;

        public TBuiltComponent GetBuilt()
        {
            return Built;
        }
    }
}
