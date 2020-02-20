using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Language;
using AutoStep.Elements;
using AutoStep.Elements.Metadata;
using AutoStep.Execution;
using AutoStep.Projects;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Language.Test;

namespace AutoStep.Tests.Execution
{
    public class FeatureExecutionSetTests : LoggingTestBase
    {
        public FeatureExecutionSetTests(ITestOutputHelper outputHelper) 
            : base(outputHelper)
        {
        }

        [Fact]
        public void CanCreateFeatureSetFromIncludeEverythingFilter()
        {
            var project = new Project();
            var file = new ProjectTestFile("/path", new StringContentSource("my file"));

            project.TryAddFile(file);

            var builtFile = new FileBuilder().Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1)
            ).Built;

            file.SetFileReadyForRunTest(builtFile);

            var featureSet = FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);

            featureSet.Features.Should().HaveCount(1);
            featureSet.Features[0].Name.Should().Be("My Feature");
            
            // Assert that the feature is a clone.
            featureSet.Features[0].Should().NotBe(builtFile.Feature);
        }

        [Fact]
        public void CanCreateFeatureSetFromMultipleFiles()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));
            var file2 = new ProjectTestFile("/path2", new StringContentSource("my file 2"));

            project.TryAddFile(file1);
            project.TryAddFile(file2);

            var builtFile1 = new FileBuilder().Feature("My Feature 1", 1, 1, feat => feat
                .Scenario("My Scenario 1", 1, 1)
            ).Built;

            var builtFile2 = new FileBuilder().Feature("My Feature 2", 1, 1, feat => feat
                .Scenario("My Scenario 2", 1, 1)
            ).Built;

            file1.SetFileReadyForRunTest(builtFile1);
            file2.SetFileReadyForRunTest(builtFile2);

            var featureSet = FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);

            featureSet.Features.Should().HaveCount(2);
            featureSet.Features[0].Name.Should().Be("My Feature 1");
            featureSet.Features[1].Name.Should().Be("My Feature 2");
        }

        [Fact]
        public void CanFilterOutAFile()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));
            var file2 = new ProjectTestFile("/path2", new StringContentSource("my file 2"));

            project.TryAddFile(file1);
            project.TryAddFile(file2);

            var builtFile1 = new FileBuilder().Feature("My Feature 1", 1, 1, feat => feat
                .Scenario("My Scenario 1", 1, 1)
            ).Built;

            var builtFile2 = new FileBuilder().Feature("My Feature 2", 1, 1, feat => feat
                .Scenario("My Scenario 2", 1, 1)
            ).Built;

            file1.SetFileReadyForRunTest(builtFile1);
            file2.SetFileReadyForRunTest(builtFile2);

            var featureSet = FeatureExecutionSet.Create(project, new FilterOutFile("/path2"), LogFactory);

            featureSet.Features.Should().HaveCount(1);
            featureSet.Features[0].Name.Should().Be("My Feature 1");

            LoggedMessages.ShouldContain(LogLevel.Debug, "Excluded", "/path2", "does not match filter");
            LoggedMessages.ShouldContain(LogLevel.Debug, "Included", "Scenario", "My Scenario 1");
        }

        [Fact]
        public void ExcludesFileWithNoFeature()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));

            project.TryAddFile(file1);

            var builtFile1 = new FileBuilder().Built;

            file1.SetFileReadyForRunTest(builtFile1);

            var featureSet = FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);

            featureSet.Features.Should().HaveCount(0);

            LoggedMessages.LastShouldContain(LogLevel.Debug, "Excluded", "/path1", "does not have a feature");
        }

        [Fact]
        public void ExcludesFeaturesWithNoScenarios()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));

            project.TryAddFile(file1);

            var builtFile1 = new FileBuilder().Feature("My Feature 1", 1, 1).Built;

            file1.SetFileReadyForRunTest(builtFile1);

            var featureSet = FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);

            featureSet.Features.Should().HaveCount(0);

            LoggedMessages.LastShouldContain(LogLevel.Debug, "Excluded", "My Feature 1", "/path1", "no scenarios");
        }

        [Fact]
        public void ExcludesFilesWithNoCompilationResult()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));

            project.TryAddFile(file1);

            var featureSet = FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);

            LoggedMessages.LastShouldContain(LogLevel.Debug, "Excluded", "/path1", "compiled");

            featureSet.Features.Should().HaveCount(0);
        }

        [Fact]
        public void ExcludesFilesWithFailedCompilationResult()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));

            project.TryAddFile(file1);

            var builtFile1 = new FileBuilder().Feature("My Feature 1", 1, 1).Built;

            file1.UpdateLastCompileResult(new FileCompilerResult(false, builtFile1));

            var featureSet = FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);

            LoggedMessages.LastShouldContain(LogLevel.Debug, "Excluded", "/path1", "compiled");

            featureSet.Features.Should().HaveCount(0);
        }

        [Fact]
        public void ExcludesFilesWithNoLinkResult()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));

            project.TryAddFile(file1);

            var builtFile1 = new FileBuilder().Feature("My Feature 1", 1, 1).Built;
            file1.UpdateLastCompileResult(new FileCompilerResult(true, builtFile1));

            var featureSet = FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);

            LoggedMessages.LastShouldContain(LogLevel.Debug, "Excluded", "/path1", "linked");

            featureSet.Features.Should().HaveCount(0);
        }

        [Fact]
        public void ExcludesFilesWithFailedLinkResult()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));

            project.TryAddFile(file1);

            var builtFile1 = new FileBuilder().Feature("My Feature 1", 1, 1).Built;

            file1.UpdateLastCompileResult(new FileCompilerResult(true, builtFile1));
            file1.UpdateLastLinkResult(new LinkResult(false, Enumerable.Empty<CompilerMessage>()));

            var featureSet = FeatureExecutionSet.Create(project, new RunAllFilter(), LogFactory);

            LoggedMessages.LastShouldContain(LogLevel.Debug, "Excluded", "/path1", "linked");

            featureSet.Features.Should().HaveCount(0);
        }

        [Fact]
        public void CanFilterOutAFeature()
        {
            var project = new Project();
            var file1 = new ProjectTestFile("/path1", new StringContentSource("my file 1"));
            var file2 = new ProjectTestFile("/path2", new StringContentSource("my file 2"));

            project.TryAddFile(file1);
            project.TryAddFile(file2);

            var builtFile1 = new FileBuilder().Feature("My Feature 1", 1, 1, feat => feat
                .Scenario("My Scenario 1", 1, 1)
            ).Built;

            var builtFile2 = new FileBuilder().Feature("My Feature 2", 1, 1, feat => feat
                .Scenario("My Scenario 2", 1, 1)
            ).Built;

            file1.SetFileReadyForRunTest(builtFile1);
            file2.SetFileReadyForRunTest(builtFile2);

            var featureSet = FeatureExecutionSet.Create(project, new FilterOutFeature("My Feature 1"), LogFactory);

            featureSet.Features.Should().HaveCount(1);
            featureSet.Features[0].Name.Should().Be("My Feature 2");

            LoggedMessages.ShouldContain(LogLevel.Debug, "Included", "My Scenario 2");
            LoggedMessages.ShouldContain(LogLevel.Debug, "Excluded", "/path1", "excluded by the filter");
        }

        private class FilterOutFeature : IRunFilter
        {
            private readonly string featureName;

            public FilterOutFeature(string featureName)
            {
                this.featureName = featureName;
            }

            public bool MatchesFeature(ProjectTestFile file, IFeatureInfo feature)
            {
                return feature.Name != featureName;
            }

            public bool MatchesFile(ProjectTestFile file)
            {
                return true;
            }

            public bool MatchesScenario(IScenarioInfo scen, IExampleInfo example)
            {
                return true;
            }
        }

        private class FilterOutFile : IRunFilter
        {
            private string path;

            public FilterOutFile(string path)
            {
                this.path = path;
            }
            public bool MatchesFeature(ProjectTestFile file, IFeatureInfo feature)
            {
                return true;
            }

            public bool MatchesFile(ProjectTestFile file)
            {
                return file.Path != path;
            }

            public bool MatchesScenario(IScenarioInfo scen, IExampleInfo example)
            {
                return true;
            }
        }
    }
}
