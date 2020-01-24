using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Elements;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Elements
{
    public class FeatureElementTests
    {
        [Fact]
        public void CloneIncludesScenario()
        {
            var featureElement = new FeatureBuilder("My Feature", 10, 12)
                                     .Description("Description")
                                     .Background(2, 2, cfg => { })
                                     .Tag("tag1", 1, 1)
                                     .Scenario("Scenario 1", 1, 1)
                                     .Scenario("Scenario 2", 2, 1)
                                     .Built;

            var cloned = featureElement.CloneWithFilteredScenarios((scen, ex) => true);

            cloned.Should().NotBeSameAs(featureElement);

            cloned.Name.Should().Be("My Feature");
            cloned.Description.Should().Be("Description");
            cloned.Background.Should().NotBeNull();
            cloned.SourceLine.Should().Be(10);
            cloned.StartColumn.Should().Be(12);
            
            cloned.Annotations.Should().ContainInOrder(new[] { featureElement.Annotations[0] });
            cloned.Scenarios[0].Should().BeSameAs(featureElement.Scenarios[0]);
            cloned.Scenarios[1].Should().BeSameAs(featureElement.Scenarios[1]);
        }

        [Fact]
        public void CloneIncludesSingleScenarioOutlineInstance()
        {
            var featureElement = new FeatureBuilder("My Feature", 1, 1)                                     
                                     .ScenarioOutline("Scenario Outline 1", 1, 1, cfg => cfg
                                        .Examples(1, 1, ex => ex
                                            .Tag("valid", 1, 1)
                                            .Table(1 , 1, t => { })
                                        )
                                     )
                                     .Built;

            var cloned = featureElement.CloneWithFilteredScenarios((scen, ex) => true);

            cloned.Should().NotBeSameAs(featureElement);

            cloned.Scenarios[0].Should().BeSameAs(featureElement.Scenarios[0]);
        }

        [Fact]
        public void CloneIncludesScenarioOutlineNotAllExamplesValid()
        {
            var featureElement = new FeatureBuilder("My Feature", 1, 1)
                                     .ScenarioOutline("Scenario Outline 1", 11, 12, cfg => cfg
                                        .Tag("scen", 1, 1)
                                        .Description("Desc")
                                        .Examples(1, 1, ex => ex
                                            .Tag("valid", 1, 1)
                                            .Table(1, 1, t => { })
                                        )
                                        .Examples(1, 1, ex => ex
                                            .Tag("notvalid", 1, 1)
                                            .Table(1, 1, t => { })
                                        )
                                     )
                                     .Built;

            var cloned = featureElement.CloneWithFilteredScenarios((scen, ex) => ex.Annotations.OfType<TagElement>().Any(t => t.Tag != "notvalid"));

            var originalScenario = (ScenarioOutlineElement)featureElement.Scenarios[0];
            var clonedScenario = (ScenarioOutlineElement) cloned.Scenarios[0];
            
            clonedScenario.Should().NotBeSameAs(originalScenario);
            clonedScenario.Name.Should().Be("Scenario Outline 1");
            clonedScenario.Description.Should().Be("Desc");
            clonedScenario.SourceLine.Should().Be(11);
            clonedScenario.StartColumn.Should().Be(12);
            clonedScenario.Annotations[0].Should().BeOfType<TagElement>().Which.Tag.Should().Be("scen");
            clonedScenario.Steps.Should().BeSameAs(originalScenario.Steps);
            clonedScenario.Examples.Should().HaveCount(1);
            clonedScenario.Examples[0].Should().BeSameAs(originalScenario.Examples[0]);
        }

        [Fact]
        public void CloneExcludesScenarioNoExamplesValid()
        {
            var featureElement = new FeatureBuilder("My Feature", 1, 1)
                                     .ScenarioOutline("Scenario Outline 1", 11, 12, cfg => cfg
                                        .Description("Desc")
                                        .Examples(1, 1, ex => ex
                                            .Tag("valid", 1, 1)
                                            .Table(1, 1, t => { })
                                        )
                                        .Examples(1, 1, ex => ex
                                            .Tag("notvalid", 1, 1)
                                            .Table(1, 1, t => { })
                                        )
                                     )
                                     .Built;

            var cloned = featureElement.CloneWithFilteredScenarios((scen, ex) => false);

            cloned.Scenarios.Should().HaveCount(0);
        }
    }
}
