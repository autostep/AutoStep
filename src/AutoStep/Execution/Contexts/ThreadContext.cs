using System.Collections.Generic;
using AutoStep.Definitions;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// The context type for a single thread of test execution.
    /// </summary>
    public class ThreadContext : TestExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadContext"/> class.
        /// </summary>
        /// <param name="testThreadId">The test thread ID.</param>
        public ThreadContext(int testThreadId)
        {
            TestThreadId = testThreadId;
        }

        /// <summary>
        /// Gets the test thread ID.
        /// </summary>
        public int TestThreadId { get; }
    }
}
