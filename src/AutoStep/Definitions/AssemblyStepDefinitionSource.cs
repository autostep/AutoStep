using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Tracing;
using Microsoft.Extensions.Logging;

namespace AutoStep.Definitions
{
    /// <summary>
    /// The assembly step definition source loads steps from .NET assemblies.
    /// </summary>
    public class AssemblyStepDefinitionSource : IStepDefinitionSource
    {
        private readonly Assembly assembly;
        private readonly ILogger logger;
        private readonly List<StepDefinition> definitions = new List<StepDefinition>();
        private readonly List<Type> definitionOwningTypes = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyStepDefinitionSource"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to load steps from.</param>
        /// <param name="logFactory">A log factory (to create a logger from).</param>
        public AssemblyStepDefinitionSource(Assembly assembly, ILoggerFactory logFactory)
        {
            if (logFactory is null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            this.logger = logFactory.CreateLogger($"AssemblySteps-{assembly.GetName().Name}");
        }

        /// <summary>
        /// Gets a unique, non-human-readable identifier for the source. Two sources with the same UID cannot share steps. For assembly sources
        /// this is the assembly location.
        /// </summary>
        public string Uid => assembly.Location;

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        public string Name => assembly.Location;

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
                servicesBuilder.RegisterConsumer(definitionType);
            }
        }

        /// <summary>
        /// Gets the last modification time of the assembly.
        /// </summary>
        /// <returns>The timestamp.</returns>
        public DateTime GetLastModifyTime()
        {
            // Last modify will be the last write time of the step definitions.
            return new FileInfo(assembly.Location).LastWriteTimeUtc;
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

        private void EnsureDefinitionsLoaded()
        {
            // Only load once.
            if (definitions.Count == 0)
            {
                // Search for public types that are decorated with a Steps attribute.
                var allPublicTypes = assembly.GetExportedTypes();

                foreach (var type in allPublicTypes)
                {
                    if (type.GetCustomAttribute<StepsAttribute>(true) is object && !type.IsAbstract)
                    {
                        var hasStep = false;

                        logger.LogInformation(DefinitionsLogMessages.AssemblyStepDefinitionSource_LookingInTypeForSteps, type.FullName);

                        // This type may contain steps.
                        foreach (var method in type.GetMethods())
                        {
                            var definition = method.GetCustomAttribute<StepDefinitionAttribute>(true);

                            if (definition is object)
                            {
                                logger.LogInformation(DefinitionsLogMessages.AssemblyStepDefinitionSource_FoundStepMethod, definition.Type, definition.Declaration, method.Name);

                                definitions.Add(new ClassStepDefinition(this, type, method, definition));
                                hasStep = true;
                            }
                        }

                        if (hasStep)
                        {
                            definitionOwningTypes.Add(type);
                        }
                    }
                }
            }
        }
    }
}
