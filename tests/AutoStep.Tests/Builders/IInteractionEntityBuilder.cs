using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    public interface IInteractionEntityBuilder<out TBuilt>
        where TBuilt : InteractionDefinitionElement
    {
        TBuilt Built { get; }
    }
}
