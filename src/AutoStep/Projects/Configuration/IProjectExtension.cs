using AutoStep.Execution;

namespace AutoStep.Projects.Configuration
{
    public interface IProjectExtension
    {
        void ExtendProject(ProjectExtensionConfiguration config, Project project);

        void ExtendExecution(ProjectExtensionConfiguration extConfig, RunConfiguration runConfig, TestRun testRun);
    }
}
