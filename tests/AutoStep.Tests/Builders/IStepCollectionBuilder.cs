using AutoStep.Elements;

namespace AutoStep.Tests.Builders
{
    public interface IStepCollectionBuilder<out TBuilt>
        where TBuilt : StepCollectionElement
    {
        TBuilt Built { get; }
    }


}
