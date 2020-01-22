namespace AutoStep.Execution.Contexts
{

    public class RunContext : TestExecutionContext
    {
        public RunContext(RunConfiguration config)
        {
            Configuration = config ?? throw new System.ArgumentNullException(nameof(config));
        }

        public RunConfiguration Configuration { get; }
    }
}
