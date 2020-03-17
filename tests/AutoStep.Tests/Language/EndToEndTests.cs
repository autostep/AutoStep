using System;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Projects;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using System.Threading;

namespace AutoStep.Tests.Language
{
    public class EndToEndTests : LoggingTestBase
    {
        public EndToEndTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/42")]
        public async Task RemoveStepDefinitionAfterInitialVersion()
        {
            // Compile a file.
            const string TestFileWithDef =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    When I press 'Add'

               Step: When I press {button}

            ";

            const string TestFileWithoutDef =
             @"
              Feature: My Feature

                Scenario: My Scenario

                    When I press 'Add'
            ";


            var project = new Project();

            var source = new UpdatableContentSource(TestFileWithDef);

            var projFile = new ProjectTestFile("/test", source);

            project.TryAddFile(projFile);

            await project.Compiler.CompileAsync(LogFactory);

            project.Compiler.Link();

            projFile.LastLinkResult!.Success.Should().BeTrue();

            // First step should have bound.
            projFile.LastLinkResult!.Output!.AllStepReferences!.First!.Value.Binding.Should().NotBeNull();

            // Now update the source to remove the step definition.
            source.Content = TestFileWithoutDef;
            source.LastModify = projFile.LastCompileTime.AddSeconds(10);

            await project.Compiler.CompileAsync(LogFactory);

            project.Compiler.Link();

            projFile.LastLinkResult!.Success.Should().BeFalse();

            // First step should have bound.
            projFile.LastLinkResult!.Output!.AllStepReferences.First!.Value.Binding.Should().BeNull();
        }

        private class UpdatableContentSource : IContentSource
        {
            public UpdatableContentSource(string content)
            {
                Content = content;
                LastModify = DateTime.Now;
            }

            public string? SourceName => null;

            public string Content { get; set; }

            public DateTime LastModify { get; set; }

            public ValueTask<string> GetContentAsync(CancellationToken cancelToken = default)
            {
                return new ValueTask<string>(Content);
            }

            public DateTime GetLastContentModifyTime()
            {
                return LastModify;
            }
        }
    }
}
