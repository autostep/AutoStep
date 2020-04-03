using System;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using Microsoft.Extensions.Logging;

namespace AutoStep.Projects.Configuration
{
    public interface IProjectExtension : IDisposable
    {
        void AttachToProject(ProjectExtensionConfiguration config, Project project);

        void DetachFromProject(ProjectExtensionConfiguration config, Project project);

        void ExtendExecution(ProjectExtensionConfiguration extConfig, RunConfiguration runConfig, TestRun testRun);

        void ConfigureExecutionServices(ProjectExtensionConfiguration extConfig, RunConfiguration runConfig, IServicesBuilder servicesBuilder);
    }
}
