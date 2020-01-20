using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Elements
{
    /// <summary>
    /// Defines a built Feature block, that can contain Background and Scenarios.
    /// </summary>
    public class FeatureElement : BuiltElement, IAnnotatableElement, IFeatureInfo
    {
        /// <summary>
        /// Gets the annotations applied to the feature, in applied order.
        /// </summary>
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        IReadOnlyList<IAnnotationInfo> IFeatureInfo.Annotations => Annotations;

        /// <summary>
        /// Gets or sets the name of the feature.
        /// </summary>
        public string? Name { get; set; }

        string IFeatureInfo.Name => Name ?? throw new LanguageEngineAssertException();

        /// <summary>
        /// Gets or sets the description body (if any).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the background set, or null if none is specified.
        /// </summary>
        public BackgroundElement? Background { get; set; }

        IBackgroundInfo? IFeatureInfo.Background => Background;

        /// <summary>
        /// Gets the list of scenarios.
        /// </summary>
        public List<ScenarioElement> Scenarios { get; } = new List<ScenarioElement>();

        IReadOnlyList<IScenarioInfo> IFeatureInfo.Scenarios => Scenarios;

        /// <summary>
        /// Creates a copy of this feature, with a filtered set of scenarios.
        /// </summary>
        /// <returns>A clone.</returns>
        public FeatureElement CloneWithFilteredScenarios(Func<ScenarioElement, ExampleElement?, bool> scenarioPredicate)
        {
            var newFeature = new FeatureElement
            {
                Name = Name,
                Background = Background,
                Description = Description,
                SourceLine = SourceLine,
                StartColumn = StartColumn,
            };

            newFeature.Annotations.AddRange(Annotations);

            foreach (var scen in Scenarios)
            {
                if (scen is ScenarioOutlineElement outline)
                {
                    List<ExampleElement>? validExamples = null;

                    // Go through all the examples.
                    foreach (var example in outline.Examples)
                    {
                        if (scenarioPredicate(scen, example))
                        {
                            // Example on scenario outline is wanted.
                            if (validExamples is null)
                            {
                                validExamples = new List<ExampleElement>();
                            }

                            validExamples.Add(example);
                        }
                    }

                    if (validExamples?.Count > 0)
                    {
                        if (validExamples.Count == outline.Examples.Count)
                        {
                            // All the examples are valid, just use the scenario directly.
                            newFeature.Scenarios.Add(outline);
                        }
                        else
                        {
                            // We've matched on one or more examples, add a copy of the outline with only the examples we want.
                            var newOutline = new ScenarioOutlineElement
                            {
                                Name = outline.Name,
                                Description = outline.Description,
                                SourceLine = outline.SourceLine,
                                StartColumn = outline.StartColumn,
                            };

                            newOutline.Annotations.AddRange(outline.Annotations);

                            newOutline.UseStepsFrom(outline);

                            foreach (var example in validExamples)
                            {
                                newOutline.AddExample(example);
                            }

                            newFeature.Scenarios.Add(newOutline);
                        }
                    }
                }
                else if (scenarioPredicate(scen, null))
                {
                    // We want to run this scenario.
                    newFeature.Scenarios.Add(scen);
                }
            }

            return newFeature;
        }
    }
}
