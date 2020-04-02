using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;

namespace AutoStep.Projects.Configuration
{
    public class ExtensionSet
    {
        private readonly List<ExtensionEntry> extensions = new List<ExtensionEntry>();

        public static async Task<ExtensionSet> Create(ProjectConfiguration config, Func<ProjectExtensionConfiguration, CancellationToken, Task<IProjectExtension>> loader, CancellationToken cancelToken)
        {
            var extSet = new ExtensionSet();

            foreach (var extConfig in config.Extensions.Values)
            {
                var loaded = await loader(extConfig, cancelToken).ConfigureAwait(false);

                extSet.Add(new ExtensionEntry { Configuration = extConfig, Extension = loaded });
            }

            return extSet;
        }

        /// <summary>
        /// Called prior to execution.
        /// </summary>
        /// <param name="builder">The services builder.</param>
        /// <param name="configuration">The test run configuration.</param>
        public void ConfigureExtensionServices(IServicesBuilder builder, RunConfiguration configuration)
        {
            foreach (var ext in extensions)
            {
                ext.Extension.ConfigureExecutionServices(ext.Configuration, configuration, builder);
            }
        }

        public void ExtendProject(Project project)
        {
            foreach (var ext in extensions)
            {
                ext.Extension.ExtendProject(ext.Configuration, project);
            }
        }

        public void ExtendExecution(RunConfiguration runConfig, TestRun testRun)
        {
            foreach (var ext in extensions)
            {
                ext.Extension.ExtendExecution(ext.Configuration, runConfig, testRun);
            }
        }

        private void Add(ExtensionEntry entry)
        {
            extensions.Add(entry);
        }

        private struct ExtensionEntry
        {
            public ProjectExtensionConfiguration Configuration { get; set; }

            public IProjectExtension Extension { get; set; }
        }
    }
}
