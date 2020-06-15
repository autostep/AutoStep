using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Results;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Execution.Results
{
    public class ResultsCollectorTests
    {
        [Fact]
        public void CanCollectMultipleFeatures()
        {
            var results = ResultsFor(("/test1", @"
                
                Feature: My Feature

                Scenario: Scenario 1

                    Given I do something

            "), ("/test2", @"
                
                Feature: Second Feature

                Scenario: Scenario 1
    
                    Given I do something
            
            "));

            var features = results.Features.ToList();

            features.Should().HaveCount(2);
        }

        [Fact]
        public void FeaturesReportedInFileNameOrder()
        {
            var results = ResultsFor(("/def", @"
                
                Feature: DEF

                Scenario: Scenario 1

                    Given I do something

            "), ("/abc", @"
                
                Feature: ABC

                Scenario: Scenario 1
    
                    Given I do something
            
            "));

            var features = results.Features.ToList();

            features.Should().HaveCount(2);
            features[0].Feature.Name.Should().Be("ABC");
            features[1].Feature.Name.Should().Be("DEF");
        }

        [Fact]
        public void FeatureSuccessRecorded()
        {
            var results = ResultsFor(("test", @"
                
                Feature: My Feature

                Scenario: Scenario 1

                    Given I do something

            "));

            var features = results.Features.ToList();

            features.Should().HaveCount(1);

            var feature = features[0];

            feature.Feature.Name.Should().Be("My Feature");
            feature.FeatureFailureException.Should().BeNull();
            feature.Passed.Should().BeTrue();
            feature.Scenarios.Should().HaveCount(1);
            feature.StartTimeUtc.Should().BeCloseTo(DateTime.UtcNow, 1000);
            feature.EndTimeUtc.Should().BeAfter(feature.StartTimeUtc);
        }

        [Fact]
        public void FeatureFailureRecorded()
        {
            var results = ResultsFor(("test", @"
                
                Feature: My Feature

                Scenario: Scenario 1

                    Then it fails

            "));

            var features = results.Features.ToList();

            features.Should().HaveCount(1);

            var feature = features[0];

            feature.Feature.Name.Should().Be("My Feature");
            feature.FeatureFailureException.Should().BeNull();
            feature.Passed.Should().BeFalse();
            feature.Scenarios.Should().HaveCount(1);
            feature.StartTimeUtc.Should().BeCloseTo(DateTime.UtcNow, 1000);
            feature.EndTimeUtc.Should().BeAfter(feature.StartTimeUtc);
        }

        [Fact]
        public void MultipleScenariosCaptured()
        {
            var results = ResultsFor(("test", @"
                
                Feature: My Feature

                Scenario: Scenario 1

                    Then it fails

                Scenario: Scenario 2

                    Given I do something

            "));

            var features = results.Features.ToList();

            features.Should().HaveCount(1);

            var feature = features[0];

            feature.Scenarios.Should().HaveCount(2);
        }

        [Fact]
        public void ScenariosReportedInPositionOrder()
        {
            var results = ResultsFor(("test", @"
                
                Feature: My Feature

                Scenario: DEF

                    Given I do something

                Scenario: ABC

                    Given I do something

            "));

            var features = results.Features.ToList();

            features.Should().HaveCount(1);

            var feature = features[0];

            var scenarios = feature.Scenarios.ToList();

            scenarios[0].Scenario.Name.Should().Be("DEF");
            scenarios[1].Scenario.Name.Should().Be("ABC");
        }

        [Fact]
        public void ScenarioInvocationCapturedForNormalPassingScenario()
        {
            var results = ResultsFor(("test", @"
                
                Feature: My Feature

                Scenario: Scenario 1

                    Given I do something

            "));

            var features = results.Features.ToList();

            features.Should().HaveCount(1);

            var feature = features[0];

            var scenario = feature.Scenarios.First();

            scenario.IsScenarioOutline.Should().BeFalse();
            scenario.Invocations.Should().HaveCount(1);

            var invoke = scenario.Invocations.First();

            invoke.Scenario.Name.Should().Be("Scenario 1");
            invoke.Passed.Should().BeTrue();
            invoke.StartTimeUtc.Should().BeOnOrAfter(feature.StartTimeUtc);
            invoke.EndTimeUtc.Should().BeAfter(invoke.StartTimeUtc);
            invoke.EndTimeUtc.Should().BeOnOrBefore(invoke.EndTimeUtc);
            invoke.Elapsed.Should().BeGreaterThan(TimeSpan.Zero);
            invoke.FailException.Should().BeNull();
            invoke.FailingStep.Should().BeNull();
            invoke.OutlineVariables.Should().BeNull();
            invoke.InvocationName.Should().BeNull();
        }

        [Fact]
        public void ScenarioInvocationCapturedForNormalFailingScenario()
        {
            var results = ResultsFor(("test", @"
                
                Feature: My Feature

                Scenario: Scenario 1

                    Then it fails

            "));

            var features = results.Features.ToList();

            features.Should().HaveCount(1);

            var feature = features[0];

            var scenario = feature.Scenarios.First();

            scenario.IsScenarioOutline.Should().BeFalse();
            scenario.Invocations.Should().HaveCount(1);
            scenario.Passed.Should().BeFalse();

            var invoke = scenario.Invocations.First();

            invoke.Scenario.Name.Should().Be("Scenario 1");
            invoke.Passed.Should().BeFalse();
            invoke.StartTimeUtc.Should().BeOnOrAfter(feature.StartTimeUtc);
            invoke.EndTimeUtc.Should().BeAfter(invoke.StartTimeUtc);
            invoke.EndTimeUtc.Should().BeOnOrBefore(invoke.EndTimeUtc);
            invoke.Elapsed.Should().BeGreaterThan(TimeSpan.Zero);
            invoke.FailException.Should().BeOfType<StepFailureException>();
            invoke.FailingStep!.Text.Should().Be("it fails");
            invoke.OutlineVariables.Should().BeNull();
            invoke.InvocationName.Should().BeNull();
        }

        [Fact]
        public void MultipleScenarioInvocationsForScenarioOutline()
        {
            var results = ResultsFor(("test", @"
                
                Feature: My Feature

                Scenario Outline: Scenario 1

                    Then it will <input>
                
                Examples:
                
                    | input |
                    | pass  |
                    | fail  |

            "));

            var feature = results.Features.First();
            var scenario = feature.Scenarios.First();

            scenario.IsScenarioOutline.Should().BeTrue();
            scenario.Invocations.Should().HaveCount(2);
            scenario.Passed.Should().BeFalse();

            var invokes = scenario.Invocations.ToList();

            var pass = invokes[0];
            pass.Passed.Should().BeTrue();
            pass.InvocationName.Should().Be("pass");
            pass.OutlineVariables.Should().NotBeNull();
            pass.OutlineVariables!.Get("input").Should().Be("pass");

            var fail = invokes[1];
            fail.Passed.Should().BeFalse();
            fail.InvocationName.Should().Be("fail");
            fail.OutlineVariables.Should().NotBeNull();
            fail.OutlineVariables!.Get("input").Should().Be("fail");
        }

        [Fact]
        public void MultipleScenarioInvocationsAllPass()
        {
            var results = ResultsFor(("test", @"
                
                Feature: My Feature

                Scenario Outline: Scenario 1

                    Then it will <input>
                
                Examples:
                
                    | name | input |
                    | 1    | pass  |
                    | 2    | pass  |

            "));

            var feature = results.Features.First();
            var scenario = feature.Scenarios.First();

            scenario.IsScenarioOutline.Should().BeTrue();
            scenario.Invocations.Should().HaveCount(2);
            scenario.Passed.Should().BeTrue();

            var invokes = scenario.Invocations.ToList();

            var pass = invokes[0];
            pass.Passed.Should().BeTrue();
            pass.InvocationName.Should().Be("1");

            var fail = invokes[1];
            fail.Passed.Should().BeTrue();
            fail.InvocationName.Should().Be("2");
        }

        /// <summary>
        /// Give us a shortcut for getting results that's synchronous.
        /// </summary>
        private IRunResultSet ResultsFor(params (string path, string content)[] testFiles)
        {
            var project = ProjectMocks.CreateBuiltProject(testFiles).GetAwaiter().GetResult();

            var testRun = project.CreateTestRun();

            var collector = new DummyResultsCollector();

            testRun.Events.Add(collector);

            testRun.ExecuteAsync(default).GetAwaiter().GetResult();

            collector.Results.Should().NotBeNull();

            return collector.Results!;
        }


        private class DummyResultsCollector : ResultsCollector
        {
            public WorkingResultSet? Results { get; set; }

            protected override ValueTask OnResultsReady(ILifetimeScope scope, RunContext ctxt, WorkingResultSet results, CancellationToken cancelToken)
            {
                Results = results;

                return default;
            }
        }
    }
}
