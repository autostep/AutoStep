using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Builders
{
    public interface IStepCollectionBuilder<out TBuilt>
        where TBuilt : BuiltStepCollection
    {
        TBuilt Built { get; }
    }


}
