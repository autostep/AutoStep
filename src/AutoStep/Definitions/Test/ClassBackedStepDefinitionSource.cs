using System;
using System.Collections.Generic;
using System.Reflection;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using Microsoft.Extensions.Logging;

namespace AutoStep.Definitions.Test
{
    /// <summary>
    /// Loads steps with types that have attributes on them.
    /// </summary>
    public abstract class ClassBackedStepDefinitionSource : IStepDefinitionSource
    {
        private readonly ILogger logger;
        private readonly List<StepDefinition> definitions = new List<StepDefinition>();
        private readonly List<Type> definitionOwningTypes = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBackedStepDefinitionSource"/> class.
        /// </summary>
        /// <param name="logFactory">A log factory (to create a logger from).</param>
        public ClassBackedStepDefinitionSource(ILoggerFactory logFactory)
        {
            if (logFactory is null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            this.logger = logFactory.CreateLogger<ClassBackedStepDefinitionSource>();
        }

        /// <summary>
        /// Gets a Unique Identifier for the source.
        /// </summary>
        public abstract string Uid { get; }

        /// <summary>
        /// Gets a Unique Identifier for the source.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Configures any services required by the step definition source (including any consumers that want to resolve services).
        /// </summary>
        /// <param name="servicesBuilder">The services builder.</param>
        /// <param name="configuration">The run configuration.</param>
        public void ConfigureServices(IServicesBuilder servicesBuilder, RunConfiguration configuration)
        {
            servicesBuilder = servicesBuilder.ThrowIfNull(nameof(servicesBuilder));

            EnsureDefinitionsLoaded();

            // All types providing steps should be registered.
            // We'll see about reloading DLLs later (TODO).
            foreach (var definitionType in definitionOwningTypes)
            {
                servicesBuilder.RegisterPerResolveService(definitionType);
            }
        }

        /// <summary>
        /// Get the step definitions.
        /// </summary>
        /// <returns>The step definitions.</returns>
        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            EnsureDefinitionsLoaded();

            return definitions;
        }

        /// <summary>
        /// Reset the known definitions so they can be recalculated.
        /// </summary>
        protected void ResetDefinitions()
        {
            definitions.Clear();
            definitionOwningTypes.Clear();
        }

        private void EnsureDefinitionsLoaded()
        {
            if (definitions.Count == 0)
            {
                ScanDefinitions();
            }
        }

        /// <summary>
        /// When implemented by a derived class, scans all possible classes for step-holding types.
        /// </summary>
        protected abstract void ScanDefinitions();

        /// <summary>
        /// Try to register the definitions in a class.
        /// </summary>
        /// <param name="owningType">The class type.</param>
        /// <returns>True if there were any steps in the class. False otherwise.</returns>
        protected bool TryAddType(Type owningType)
        {
            owningType = owningType.ThrowIfNull(nameof(owningType));

            if (!owningType.IsClass)
            {
                throw new ArgumentException(DefinitionsMessages.ClassBackedStepDefinitionSource_ProvidedTypeMustBeClass);
            }

            var hasStep = false;

            // This type may contain steps.
            foreach (var method in owningType.GetMethods())
            {
                var definition = method.GetCustomAttribute<StepDefinitionAttribute>(true);

                if (definition is object)
                {
                    logger.LogDebug(DefinitionsMessages.ClassBackedStepDefinitionSource_FoundStepMethod, definition.Type, definition.Declaration, method.Name);

                    definitions.Add(new ClassStepDefinition(this, owningType, method, definition));
                    hasStep = true;
                }
            }

            if (hasStep)
            {
                definitionOwningTypes.Add(owningType);
            }

            return hasStep;
        }
    }
}
