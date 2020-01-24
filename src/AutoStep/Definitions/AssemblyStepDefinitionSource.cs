using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace AutoStep.Definitions
{
    /// <summary>
    /// The assembly step definition source loads steps from .NET assemblies.
    /// </summary>
    public class AssemblyStepDefinitionSource : ClassBackedStepDefinitionSource
    {
        private readonly Assembly assembly;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyStepDefinitionSource"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to load steps from.</param>
        /// <param name="logFactory">A log factory (to create a logger from).</param>
        public AssemblyStepDefinitionSource(Assembly assembly, ILoggerFactory logFactory)
            : base(logFactory)
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
        public override string Uid => assembly.Location;

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        public override string Name => assembly.Location;

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
        /// Scan the assembly for relevant types.
        /// </summary>
        protected override void ScanDefinitions()
        {
            // Search for public types that are decorated with a Steps attribute.
            var allPublicTypes = assembly.GetExportedTypes();

            foreach (var type in allPublicTypes)
            {
                if (type.IsClass && !type.IsAbstract && type.GetCustomAttribute<StepsAttribute>(true) is object)
                {
                    logger.LogInformation(DefinitionsMessages.AssemblyStepDefinitionSource_LookingInTypeForSteps, type.FullName);

                    TryAddType(type);
                }
            }
        }
    }
}
