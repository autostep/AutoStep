using System.Collections.Generic;
using AutoStep.Definitions;

namespace AutoStep.Execution.Contexts
{
    public class ThreadContext : TestExecutionContext
    {
        public int TestThreadId { get; }

        internal Stack<StepDefinition> DefinitionStack { get; } = new Stack<StepDefinition>();

        internal ThreadContext(int testThreadId)
        {
            TestThreadId = testThreadId;
        }
    }
}
