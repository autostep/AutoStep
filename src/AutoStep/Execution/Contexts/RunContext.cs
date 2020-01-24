using System;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// Defines the context for the entire run. This context is the result of a test run, so results should be placed in here.
    /// </summary>
    public class RunContext : TestExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunContext"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public RunContext(RunConfiguration config)
        {
            Configuration = config ?? throw new System.ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Gets the run configuration.
        /// </summary>
        public RunConfiguration Configuration { get; }

        /// <summary>
        /// Gets or sets the duration of the run.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}
