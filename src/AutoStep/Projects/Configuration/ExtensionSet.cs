using System;
using System.Collections.Generic;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;

namespace AutoStep.Projects.Configuration
{
    public class ExtensionSet : IDisposable
    {
        private readonly List<ExtensionEntry> extensions = new List<ExtensionEntry>();

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

        public void AttachToProject(Project project)
        {
            foreach (var ext in extensions)
            {
                ext.Extension.AttachToProject(ext.Configuration, project);
            }
        }

        public void ExtendExecution(RunConfiguration runConfig, TestRun testRun)
        {
            foreach (var ext in extensions)
            {
                ext.Extension.ExtendExecution(ext.Configuration, runConfig, testRun);
            }
        }

        public void Add(ProjectExtensionConfiguration configuration, IProjectExtension extensionEntryPoint)
        {
            extensions.Add(new ExtensionEntry { Configuration = configuration, Extension = extensionEntryPoint });
        }

        public virtual void Dispose()
        {
            foreach (var ext in extensions)
            {
                ext.Extension.Dispose();
            }
        }

        private struct ExtensionEntry
        {
            public ProjectExtensionConfiguration Configuration { get; set; }

            public IProjectExtension Extension { get; set; }
        }
    }
}
