using AutoStep.Execution;
using AutoStep.Execution.Dependency;

namespace AutoStep.Projects.Configuration
{
    public interface IProjectExtension
    {
        void ExtendProject(ProjectExtensionConfiguration config, Project project);

        void ExtendExecution(ProjectExtensionConfiguration extConfig, RunConfiguration runConfig, TestRun testRun);

        void ConfigureExecutionServices(ProjectExtensionConfiguration extConfig, RunConfiguration runConfig, IServicesBuilder servicesBuilder);
    }
}
