using System;
using System.Collections.Generic;
using Autofac;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using Microsoft.Extensions.Configuration;

namespace AutoStep.Definitions.Test
{
    /// <summary>
    /// Represents a source of step definitions backed by registered callbacks.
    /// </summary>
    public class CallbackDefinitionSource : IStepDefinitionSource
    {
        private readonly List<DelegateBackedStepDefinition> stepDefs = new List<DelegateBackedStepDefinition>();

        /// <summary>
        /// Gets the unique identifier for the source.
        /// </summary>
        public string Uid { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        public string Name => "Callbacks";

        /// <inheritdoc/>
        public void ConfigureServices(ContainerBuilder containerBuilder, IConfiguration configuration)
        {
            // No extra services.
        }

        /// <inheritdoc/>
        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            return stepDefs;
        }

        /// <summary>
        /// Add a delegate-backed step definition.
        /// </summary>
        /// <param name="stepDef">The definition.</param>
        public void Add(DelegateBackedStepDefinition stepDef)
        {
            stepDefs.Add(stepDef.ThrowIfNull(nameof(stepDef)));
        }
    }
}
