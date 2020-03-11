using System;
using System.Collections.Generic;
using AutoStep.Elements.Metadata;
using AutoStep.Elements.Test;
using AutoStep.Projects;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution
{
    /// <summary>
    /// Represents the set of all features that will be executed as part of a test run.
    /// </summary>
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

        /// <summary>
        /// Gets the list of all features to execute (and their filtered set of scenarios).
        /// </summary>
        public IReadOnlyList<IFeatureInfo> Features => features;

        /// <summary>
        /// Create a new feature execution set.
        /// </summary>
        /// <param name="project">The project to take files from.</param>
        /// <param name="filter">A filter to apply that determines the final state.</param>
        /// <param name="logFactory">A logger factory.</param>
        /// <returns>The execution.</returns>
        public static FeatureExecutionSet Create(Project project, IRunFilter filter, ILoggerFactory logFactory)
        {
            project = project.ThrowIfNull(nameof(project));
            filter = filter.ThrowIfNull(nameof(filter));
            logFactory = logFactory.ThrowIfNull(nameof(logFactory));

            var order = new FeatureExecutionSet(project, filter, logFactory);

            order.Populate();

            return order;
        }

        private void Populate()
        {
            foreach (var file in project.AllTestFiles.Values)
            {
                if (filter.MatchesFile(file))
                {
                    AddFile(file);
                }
                else
                {
                    logger.LogDebug(ExecutionText.FeatureExecutionSet_ExcludedFile, file.Path);
                }
            }
        }

        private void AddFile(ProjectTestFile file)
        {
            var lastLinkResult = file.LastLinkResult;
            var lastCompile = file.LastCompileResult;

            if (lastCompile is null || !lastCompile.Success)
            {
                // Trace skip event.
                logger.LogDebug(ExecutionText.FeatureExecutionSet_ExcludedFileNotCompiled, file.Path);
            }
            else if (lastLinkResult is null || !lastLinkResult.Success)
            {
                // Trace skip event.
                logger.LogDebug(ExecutionText.FeatureExecutionSet_ExcludedFileNotLinked, file.Path);
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
                                    logger.LogDebug(ExecutionText.FeatureExecutionSet_IncludedScenario, scen.Name, built.Feature.Name);
                                }
                                else
                                {
                                    logger.LogDebug(ExecutionText.FeatureExecutionSet_IncludedExample, scen.Name, built.Feature.Name);
                                }

                                return true;
                            }
                            else
                            {
                                if (example is null)
                                {
                                    logger.LogDebug(ExecutionText.FeatureExecutionSet_ExcludedScenario, scen.Name, built.Feature.Name);
                                }
                                else
                                {
                                    logger.LogDebug(ExecutionText.FeatureExecutionSet_ExcludedExample, scen.Name, built.Feature.Name);
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
                            logger.LogDebug(ExecutionText.FeatureExecutionSet_ExcludedFeatureNoScenarios, clonedFeature.Name, file.Path);
                        }
                    }
                    else
                    {
                        logger.LogDebug(ExecutionText.FeatureExecutionSet_ExcludedFileFeatureFilter, file.Path);
                    }
                }
                else
                {
                    // Trace skip event.
                    logger.LogDebug(ExecutionText.FeatureExecutionSet_ExcludedFileNoFeature, file.Path);
                }
            }
        }
    }
}
