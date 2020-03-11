using AutoStep.Elements.Test;

namespace AutoStep.Tests.Builders
{
    public interface IStepCollectionBuilder<out TBuilt>
        where TBuilt : StepCollectionElement
    {
        TBuilt Built { get; }
    }


}
