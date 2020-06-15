using Autofac;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Defines standard tags for the various <see cref="ILifetimeScope"/> scopes created during execution.
    /// </summary>
    public static class ScopeTags
    {
        /// <summary>
        /// The run scope. This scope is created from the root container, and encompasses the entire text execution (across all threads).
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
        /// The step scope tag. Each step will get its own scope.
        /// </summary>
        public const string StepTag = "__asStep";

        /// <summary>
        /// The method scope tag. Each defined method will get its own scope.
        /// </summary>
        public const string MethodTag = "__asMethod";
    }
}
