using AutoStep.Elements.Metadata;
using AutoStep.Projects;

namespace AutoStep.Execution
{
    /// <summary>
    /// Defines a filter that includes everything.
    /// </summary>
    public class RunAllFilter : IRunFilter
    {
        /// <inheritdoc/>
        public bool MatchesFile(ProjectTestFile file)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool MatchesFeature(ProjectTestFile file, IFeatureInfo feature)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool MatchesScenario(IScenarioInfo scen, IExampleInfo? example)
        {
            return true;
        }
    }
}
