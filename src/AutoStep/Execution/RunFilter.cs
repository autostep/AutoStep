using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Compiler;
using AutoStep.Elements;
using AutoStep.Projects;

namespace AutoStep.Execution
{
    public interface IRunFilter
    {
        bool MatchesFile(ProjectFile file);

        bool MatchesFeature(ProjectFile file, FeatureElement feature);

        bool MatchesScenario(ScenarioElement scen, ExampleElement? example);
    }

    /// <summary>
    /// Defines a filter on a project, restricting what tests to run.
    /// </summary>
    public class RunFilter
    {
        // What filter types should there be?
        //   Exact File? File globbing support
        //   By Feature name/regex
        //   Exact Scenario Name or list of scenarios (need to be able to define a 'name' for a scenario that can be filtered on).
        //   Scenario regex.
        //   By Tag?
    }

    public class RunAllFilter : IRunFilter
    {
        public bool MatchesFile(ProjectFile file)
        {
            return true;
        }

        public bool MatchesFeature(ProjectFile file, FeatureElement feature)
        {
            return true;
        }

        public bool MatchesScenario(ScenarioElement scen, ExampleElement? example)
        {
            return true;
        }
    }
}
