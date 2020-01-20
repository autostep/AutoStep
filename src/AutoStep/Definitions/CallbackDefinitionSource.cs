using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions
{
    public class CallbackDefinitionSource : IStepDefinitionSource
    {
        private List<DelegateBackedStepDefinition> stepDefs = new List<DelegateBackedStepDefinition>();

        public string Uid { get; } = Guid.NewGuid().ToString();

        public string Name => "Callbacks";

        public void ConfigureServices(IServicesBuilder servicesBuilder, RunConfiguration configuration)
        {
            // No extra services.
        }

        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            return stepDefs;
        }

        private void Add(DelegateBackedStepDefinition stepDef)
        {
            stepDefs.Add(stepDef);
        }

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

        public CallbackDefinitionSource Given<T1>(string declaration, Func<T1, Task> callback)
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
    }
}
