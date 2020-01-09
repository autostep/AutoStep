using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Compiler;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution
{
        /// <summary>
    /// Allows the RunContext to interact with a state manager that can control program flow (step over, step into, pause, etc).
    /// It's a bit like an attached debugger. There will be a 'default' execution state manager that just says 'continue' for everything,
    /// which is basically like just running a test.
    /// </summary>
    public interface IExecutionStateManager
    {

    }

    /// <summary>
    /// A class that represents the configuration used for a run. This will be the 'final' state, after all other configuration has been calculated,
    /// merged, etc.
    /// </summary>
    public class RunConfiguration
    {

    }

    /// <summary>
    /// Represents the outcome of a run. This should include run failure details, including aggregated
    /// </summary>
    public class RunResult
    {

    }

    /// <summary>
    /// Result of a prepare operation.
    /// </summary>
    public class PrepareResult
    {

    }

    /// <summary>
    /// Event source that can be injected into a consumer.
    /// </summary>
    public interface IEventSource
    {
        event EventHandler EventHappened;
    }

    public enum RunContextState
    {
        Created,
        Prepared,
        ExecutionStarted,
        FeatureRunning,
        ScenarioRunning,
        StepRunning
    }

    public class RunContext : IEventSource
    {
        private IServiceProvider serviceProvider;

        public RunContext(Project project, RunConfiguration configuration, IExecutionStateManager executionStateManager)
        {

        }

        public async Task<PrepareResult> Prepare()
        {
            // Prepare can only be called once. This method sets up any services for execution and loads plugins.

            // The run context will 

            // Built the DI container for the execution.
            var services = new ServiceCollection();

            services.AddSingleton<IEventSource>(this);

            var provider = services.BuildServiceProvider();

            return null;
        }

        /// <summary>
        /// Events are on the RunContext by default. They will exposed through an events interface.
        /// </summary>
        public event EventHandler EventHappened;

        public async Task<RunResult> Execute()
        {
            return await Task.FromResult(new RunResult());
        }
    }
}
