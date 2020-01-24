namespace AutoStep.Execution.Control
{
    /// <summary>
    /// Enum for the state of execution for a test thread.
    /// </summary>
    public enum TestThreadState
    {
        /// <summary>
        /// Thread starting.
        /// </summary>
        Starting,

        /// <summary>
        /// Feature starting.
        /// </summary>
        StartingFeature,

        /// <summary>
        /// Scenario starting.
        /// </summary>
        StartingScenario,

        /// <summary>
        /// Step starting.
        /// </summary>
        StartingStep,
    }
}
