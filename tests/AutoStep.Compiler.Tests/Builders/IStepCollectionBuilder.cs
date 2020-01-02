using AutoStep.Core.Elements;

namespace AutoStep.Compiler.Tests.Builders
{
    public interface IStepCollectionBuilder<out TBuilt>
        where TBuilt : StepCollectionElement
    {
        TBuilt Built { get; }
    }


}
