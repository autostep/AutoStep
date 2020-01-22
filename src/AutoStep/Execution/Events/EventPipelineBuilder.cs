using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Tracing;

namespace AutoStep.Execution.Events
{

    internal class EventPipelineBuilder : IEventPipelineBuilder
    {
        private List<IEventHandler> handlers = new List<IEventHandler>();

        public IEventPipelineBuilder Add(IEventHandler handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            handlers.Add(handler);

            return this;
        }

        public EventPipeline Build()
        {
            // Clone the event handler set so subsequent builder changes don't affect
            // the pipeline.
            return new EventPipeline(handlers.ToList());
        }
    }
}
