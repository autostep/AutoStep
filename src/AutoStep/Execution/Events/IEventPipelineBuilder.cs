namespace AutoStep.Execution
{
    /// <summary>
    /// Extension methods can extend this to add custom event handler registration.
    /// </summary>
    public interface IEventPipelineBuilder
    {
        IEventPipelineBuilder Add(IEventHandler handler);
    }
}
