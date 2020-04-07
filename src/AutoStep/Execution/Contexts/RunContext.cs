using System;
using Microsoft.Extensions.Configuration;

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
        /// <param name="projectConfiguration">The configuration.</param>
        public RunContext(IConfiguration projectConfiguration)
        {
            Configuration = projectConfiguration ?? throw new System.ArgumentNullException(nameof(projectConfiguration));
        }

        /// <summary>
        /// Gets the run configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets or sets the duration of the run.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}
