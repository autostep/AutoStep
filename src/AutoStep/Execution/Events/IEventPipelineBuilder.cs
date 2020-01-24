namespace AutoStep.Execution.Events
{
    /// <summary>
    /// Interface for registering event handlers in the event pipeline.
    /// </summary>
    public interface IEventPipelineBuilder
    {
        /// <summary>
        /// Add a handler to the event pipeline.
        /// </summary>
        /// <param name="handler">The handler implementation.</param>
        /// <returns>The builder, to allow fluent pipeline construction.</returns>
        IEventPipelineBuilder Add(IEventHandler handler);
    }
}
