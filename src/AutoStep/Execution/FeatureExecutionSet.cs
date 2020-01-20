using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Projects;
using AutoStep.Tracing;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution
{
    public class FeatureExecutionSet
    {
        private readonly Project project;
        private readonly IRunFilter filter;
        private readonly ILogger<FeatureExecutionSet> logger;

        // This contains the list of everything to run.
        private readonly List<FeatureElement> features = new List<FeatureElement>();

        private FeatureExecutionSet(Project project, IRunFilter filter, ILoggerFactory logFactory)
        {
            this.project = project;
            this.filter = filter;
            this.logger = logFactory.CreateLogger<FeatureExecutionSet>();
        }

        public static FeatureExecutionSet Create(Project project, IRunFilter filter, ILoggerFactory logFactory)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (logFactory is null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            var order = new FeatureExecutionSet(project, filter, logFactory);

            order.Populate();

            return order;
        }

        public IReadOnlyList<IFeatureInfo> Features => features;

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
            var lastCompile = file.LastCompileResult;

            if (lastCompile is object && lastCompile.Success && lastLinkResult is object && lastLinkResult.Success)
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
                logger.LogInformation("Excluded File '{0}' from test run because it has not been successfully compiled.", file.Path);
            }
        }
    }
}
