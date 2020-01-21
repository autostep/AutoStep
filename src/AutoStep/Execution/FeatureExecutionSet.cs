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
            if (logFactory is null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
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
                else
                {
                    logger.LogInformation("Excluded File '{0}', does not match filter.", file.Path);
                }
            }
        }

        private void AddFile(ProjectFile file)
        {
            var lastLinkResult = file.LastLinkResult;
            var lastCompile = file.LastCompileResult;

            if (lastCompile is null || !lastCompile.Success)
            {
                // Trace skip event.
                logger.LogInformation("Excluded File '{0}' from the test run because it has not been successfully compiled.", file.Path);
            }
            else if (lastLinkResult is null || !lastLinkResult.Success)
            {
                // Trace skip event.
                logger.LogInformation("Excluded File '{0}' from the test run because it has not been successfully linked.", file.Path);
            }
            else
            {
                var built = lastLinkResult.Output;

                if (built?.Feature is object)
                {
                    if (filter.MatchesFeature(file, built.Feature))
                    {
                        // Create a feature.
                        var clonedFeature = built.Feature.CloneWithFilteredScenarios((scen, example) =>
                        {
                            if (filter.MatchesScenario(scen, example))
                            {
                                if (example is null)
                                {
                                    logger.LogInformation("Included Scenario '{0}' for Feature '{1}'", scen.Name, built.Feature.Name);
                                }
                                else
                                {
                                    logger.LogInformation("Included Example Set for Scenario '{0}', for Feature '{1}'", scen.Name, built.Feature.Name);
                                }

                                return true;
                            }
                            else
                            {
                                if (example is null)
                                {
                                    logger.LogInformation("Excluded Scenario '{0}' in Feature '{1}' because it is not matched by the filter.", scen.Name, built.Feature.Name);
                                }
                                else
                                {
                                    logger.LogInformation("Excluded Example Set for Scenario '{0}', in Feature '{1}', because it is not matched by the filter.", scen.Name, built.Feature.Name);
                                }

                                return false;
                            }
                        });

                        if (clonedFeature.Scenarios.Count > 0)
                        {
                            // The feature contains something, add it.
                            features.Add(clonedFeature);
                        }
                        else
                        {
                            logger.LogInformation("Excluded Feature '{0}' in the file '{1}' because no scenarios were matched by the filter.", clonedFeature.Name, file.Path);
                        }
                    }
                    else
                    {
                        logger.LogInformation("Excluded File '{0}' from the test run because the feature is excluded by the filter.", file.Path);
                    }
                }
                else
                {
                    // Trace skip event.
                    logger.LogInformation("Excluded File '{0}' from the test run because it does not have a feature.", file.Path);
                }
            }
        }
    }
}
