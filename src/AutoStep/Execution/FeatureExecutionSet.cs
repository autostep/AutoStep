using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements;
using AutoStep.Projects;
using AutoStep.Tracing;

namespace AutoStep.Execution
{
    public class FeatureExecutionSet
    {
        private readonly Project project;
        private readonly IRunFilter filter;
        private readonly ITracer tracer;

        // This contains the list of everything to run.
        private readonly List<FeatureElement> features = new List<FeatureElement>();

        private FeatureExecutionSet(Project project, IRunFilter filter, ITracer tracer)
        {
            this.project = project;
            this.filter = filter;
            this.tracer = tracer;
        }

        public static FeatureExecutionSet Create(Project project, IRunFilter filter, ITracer tracer)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (tracer is null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }

            var order = new FeatureExecutionSet(project, filter, tracer);

            order.Populate();

            return order;
        }

        public IReadOnlyList<FeatureElement> Features => features;

        private void Populate()
        {
            foreach (var file in project.AllFiles.Values)
            {
                if (filter.MatchesFile(file))
                {
                    AddFile(file);
                }
            }
        }

        private void AddFile(ProjectFile file)
        {
            var lastLinkResult = file.LastLinkResult;

            if (lastLinkResult is object && lastLinkResult.Success)
            {
                var built = lastLinkResult.Output;

                if (built?.Feature is object)
                {
                    if (filter.MatchesFeature(file, built.Feature))
                    {
                        // Create a feature.
                        var clonedFeature = built.Feature.CloneWithFilteredScenarios((scen, example) => filter.MatchesScenario(scen, example));

                        if (clonedFeature.Scenarios.Count > 0)
                        {
                            // The feature contains something, add it.
                            features.Add(clonedFeature);
                        }
                    }
                }
            }
            else
            {
                // Trace skip event.
            }
        }
    }
}
