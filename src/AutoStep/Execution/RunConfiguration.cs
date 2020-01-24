namespace AutoStep.Execution
{
    /// <summary>
    /// A class that represents the configuration used for a run. This will be the 'final' state, after all other configuration has been calculated,
    /// merged, etc.
    /// </summary>
    public class RunConfiguration
    {
        /// <summary>
        /// Gets the maximum number of parallel test executions.
        /// </summary>
        public int ParallelCount { get; internal set; } = 1;
    }
}
