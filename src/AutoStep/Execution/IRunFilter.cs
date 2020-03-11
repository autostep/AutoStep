using AutoStep.Elements.Metadata;
using AutoStep.Projects;

namespace AutoStep.Execution
{
    /// <summary>
    /// Defines an interface for an implementation that can filter out files, features and scenarios from
    /// a test run.
    /// </summary>
    public interface IRunFilter
    {
        /// <summary>
        /// Called to check if a file should be included in the test. Use this to filter on file path.
        /// </summary>
        /// <param name="file">The project file.</param>
        /// <returns>True to include the file, false otherwise.</returns>
        bool MatchesFile(ProjectTestFile file);

        /// <summary>
        /// Called to check if a feature should be included in the test. Use this to filter on feature name/tags.
        /// </summary>
        /// <param name="file">The project file.</param>
        /// <param name="feature">The feature metadata.</param>
        /// <returns>True to include the feature, false otherwise.</returns>
        bool MatchesFeature(ProjectTestFile file, IFeatureInfo feature);

        /// <summary>
        /// Called to check if a scenario/example pair should be included in the test. Use this to filter on scenario or example tags.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <param name="example">The example data if the scenario is an outline.</param>
        /// <returns>True to include the scenario/example pair, false otherwise.</returns>
        bool MatchesScenario(IScenarioInfo scenario, IExampleInfo? example);
    }
}
