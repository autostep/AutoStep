namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Defines standard tags for the various <see cref="IServiceScope"/> scopes created during execution.
    /// </summary>
    public static class ScopeTags
    {
        /// <summary>
        /// The root scope. All other scopes are created from this.
        /// </summary>
        public const string Root = "__asRoot";

        /// <summary>
        /// The run scope. This scope is created from the root scope, and encompasses the entire text execution (across all threads).
        /// </summary>
        public const string RunTag = "__asRun";

        /// <summary>
        /// The thread scope. One of these scopes is created per-test-thread, so services resolved from here (or a lower scope)
        /// are isolated from other threads.
        /// </summary>
        public const string ThreadTag = "__asThread";

        /// <summary>
        /// The feature scope. Each feature has its own.
        /// </summary>
        public const string FeatureTag = "__asFeature";

        /// <summary>
        /// The scenario scope. Each scenario has its own.
        /// </summary>
        public const string ScenarioTag = "__asScenario";

        /// <summary>
        /// The general (untagged) scope tag. Each step will get its own scope.
        /// </summary>
        public const string GeneralScopeTag = "__asGeneral";
    }
}
