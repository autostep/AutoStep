using System.Collections.Generic;
using System.Linq;

namespace AutoStep.Execution.Events
{
    /// <summary>
    /// Event Pipeline Creator.
    /// </summary>
    internal class EventPipelineBuilder : IEventPipelineBuilder
    {
        private List<IEventHandler> handlers = new List<IEventHandler>();

        /// <inheritdoc/>
        public IEventPipelineBuilder Add(IEventHandler handler)
        {
            handlers.Add(handler.ThrowIfNull(nameof(handler)));

            return this;
        }

        /// <summary>
        /// Builds the pipeline.
        /// </summary>
        /// <returns>A new event pipeline.</returns>
        public EventPipeline Build()
        {
            // Clone the event handler set so subsequent builder changes don't affect
            // the pipeline.
            return new EventPipeline(handlers.ToList());
        }
    }
}
