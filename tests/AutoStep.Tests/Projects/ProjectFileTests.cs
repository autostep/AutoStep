using System;
using System.Linq;
using AutoStep.Compiler;
using AutoStep.Elements;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Projects
{
    public class ProjectFileTests
    {
        [Fact]
        public void ConstructorPathCannotBeNull()
        {
            Action act = () => new ProjectFile(null, new StringContentSource("something"));

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConstructorSourceCannotBeNull()
        {
            Action act = () => new ProjectFile("/test", null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LastResultsDefaults()
        {
            var projFile = new ProjectFile("/test", new StringContentSource("something"));

            projFile.LastCompileResult.Should().BeNull();
            projFile.LastCompileTime.Should().Be(DateTime.MinValue);

            projFile.LastLinkResult.Should().BeNull();
            projFile.LastLinkTime.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void UpdateLastCompileResultChangesLastCompileResultAndTime()
        {
            var projFile = new ProjectFile("/test", new StringContentSource("something"));

            var result = new FileCompilerResult(false);
            projFile.UpdateLastCompileResult(result);

            projFile.LastCompileResult.Should().Be(result);
            projFile.LastCompileResult.Should().NotBe(DateTime.MinValue);
        }

        [Fact]
        public void UpdateLastCompileResultWillCreateFileDefinitionSourceForStepDefs()
        {
            var projFile = new ProjectFile("/test", new StringContentSource("something"));

            var builtFile = new BuiltFile();
            builtFile.AddStepDefinition(new StepDefinitionElement { Type = StepType.Given, Declaration = "I have done a thing" });

            var result = new FileCompilerResult(false, builtFile);
            projFile.UpdateLastCompileResult(result);

            projFile.StepDefinitionSource.Should().NotBeNull();
        }

        [Fact]
        public void UpdateLastLinkResultChangesLastLinkResultAndTime()
        {
            var projFile = new ProjectFile("/test", new StringContentSource("something"));

            var result = new LinkResult(false, Enumerable.Empty<CompilerMessage>());
            projFile.UpdateLastLinkResult(result);

            projFile.LastLinkResult.Should().Be(result);
            projFile.LastLinkResult.Should().NotBe(DateTime.MinValue);            
        }

        [Fact]
        public void UpdateLastLinkResultUpdatesLinkedSources()
        {
            var projFile = new ProjectFile("/test", new StringContentSource("something"));

            var defSource = new UpdatableTestStepDefinitionSource();

            var result = new LinkResult(true, Enumerable.Empty<CompilerMessage>(), new[] { defSource });
            projFile.UpdateLastLinkResult(result);

            projFile.LinkerDependencies.Should().Contain(defSource);
        }
        
        [Fact]
        public void UpdateLastLinkResultDoesNotTrackNonUpdatableSources()
        {
            var projFile = new ProjectFile("/test", new StringContentSource("something"));

            var defSource = new TestStepDefinitionSource();

            var result = new LinkResult(true, Enumerable.Empty<CompilerMessage>(), new[] { defSource });
            projFile.UpdateLastLinkResult(result);

            projFile.LinkerDependencies.Should().BeEmpty();
        }

    }
}
