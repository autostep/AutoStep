using System;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Results;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Execution.Results
{
    public class ExportableResultsCollectorTests
    {
        [Fact]
        public async Task InvokesExportersWithResults()
        {
            var project = await ProjectMocks.CreateBuiltProject(("test", @"
                
                Feature: ABC

                Scenario: Scenario 1

                    Given I do something

            "));

            var run = project.CreateTestRun();

            run.AddDefaultResultsCollector();

            IRunResultSet? results = null;

            var exporter = new DummyExporter(r => results = r);

            await run.ExecuteAsync(default, (cfg, build) =>
            {
                build.RegisterInstance<IResultsExporter>(exporter);
            });

            results.Should().NotBeNull();
        }


        [Fact]
        public async Task ExporterErrorGetsConsumed()
        {
            var project = await ProjectMocks.CreateBuiltProject(("test", @"
                
                Feature: ABC

                Scenario: Scenario 1

                    Given I do something

            "));

            var run = project.CreateTestRun();

            run.AddDefaultResultsCollector();

            var exporter = new DummyExporter(r => throw new InvalidOperationException());

            await run.ExecuteAsync(default, (cfg, build) =>
            {
                build.RegisterInstance<IResultsExporter>(exporter);
            });
        }

        [Fact]
        public async Task CanHaveNoExporters()
        {
            var project = await ProjectMocks.CreateBuiltProject(("test", @"
                
                Feature: ABC

                Scenario: Scenario 1

                    Given I do something

            "));

            var run = project.CreateTestRun();

            run.AddDefaultResultsCollector();

            await run.ExecuteAsync(default);
        }

        private class DummyExporter : IResultsExporter
        {
            private readonly Action<IRunResultSet> callback;

            public DummyExporter(Action<IRunResultSet> callback)
            {
                this.callback = callback;
            }

            public ValueTask ExportAsync(IServiceProvider scope, RunContext runContext, IRunResultSet results, CancellationToken cancelToken)
            {
                scope.Should().NotBeNull();
                runContext.Should().NotBeNull();

                callback(results);

                return default;
            }
        }
    }
}
