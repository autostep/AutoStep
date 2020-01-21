using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Represents a source of step definitions backed by registered callbacks.
    /// </summary>
    public class CallbackDefinitionSource : IStepDefinitionSource
    {
        private List<DelegateBackedStepDefinition> stepDefs = new List<DelegateBackedStepDefinition>();

        /// <summary>
        /// Gets the unique identifier for the source.
        /// </summary>
        public string Uid { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        public string Name => "Callbacks";

        /// <summary>
        /// Called before any tests execute to allow the source to register its own services to be resolved.
        /// </summary>
        /// <param name="servicesBuilder">The services builder.</param>
        /// <param name="configuration">The run-time configuration.</param>
        public void ConfigureServices(IServicesBuilder servicesBuilder, RunConfiguration configuration)
        {
            // No extra services.
        }

        /// <inheritdoc/>
        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            return stepDefs;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public CallbackDefinitionSource Given(string declaration, Action callback)
        {
            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // Wrap the callback because it doesn't need a scope.
            Action<IServiceScope> actual = _ => callback();

            Add(new DelegateBackedStepDefinition(this, actual.Target, actual.Method, StepType.Given, declaration));

            return this;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <typeparam name="T1">Argument type 1.</typeparam>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public CallbackDefinitionSource Given<T1>(string declaration, Action<T1> callback)
        {
            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // Wrap the callback because it doesn't need a scope.
            Action<IServiceScope, T1> actual = (_, p1) => callback(p1);

            Add(new DelegateBackedStepDefinition(this, actual.Target, actual.Method, StepType.Given, declaration));

            return this;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <typeparam name="T1">Argument type 1.</typeparam>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public CallbackDefinitionSource GivenAsync<T1>(string declaration, Func<T1, Task> callback)
        {
            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // Wrap the callback because it doesn't need a scope.
            Func<IServiceScope, T1, Task> actual = (_, p1) => callback(p1);

            Add(new DelegateBackedStepDefinition(this, actual.Target, actual.Method, StepType.Given, declaration));

            return this;
        }

        private void Add(DelegateBackedStepDefinition stepDef)
        {
            stepDefs.Add(stepDef);
        }
    }
}
