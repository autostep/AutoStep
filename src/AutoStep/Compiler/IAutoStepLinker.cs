using AutoStep.Definitions;

namespace AutoStep.Compiler
{
    public interface IAutoStepLinker
    {
        LinkResult Link(BuiltFile file);

        void AddStepDefinitionSource(IStepDefinitionSource source);

        void AddOrUpdateStepDefinitionSource(IUpdatableStepDefinitionSource stepDefinitionSource);

        void RemoveStepDefinitionSource(IStepDefinitionSource stepDefinitionSource);
    }
}
