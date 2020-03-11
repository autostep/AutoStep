using System;
using System.Linq;
using AutoStep.Language;
using AutoStep.Elements;
using AutoStep.Projects;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using AutoStep.Elements.Test;
using AutoStep.Language.Test;

namespace AutoStep.Tests.Projects
{
    public class ProjectFileTests
    {
        [Fact]
        public void ConstructorPathCannotBeNull()
        {
            Action act = () => new ProjectTestFile(null, new StringContentSource("something"));

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConstructorSourceCannotBeNull()
        {
            Action act = () => new ProjectTestFile("/test", null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LastResultsDefaults()
        {
            var projFile = new ProjectTestFile("/test", new StringContentSource("something"));

            projFile.LastCompileResult.Should().BeNull();
            projFile.LastCompileTime.Should().Be(DateTime.MinValue);

            projFile.LastLinkResult.Should().BeNull();
            projFile.LastLinkTime.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void UpdateLastCompileResultChangesLastCompileResultAndTime()
        {
            var projFile = new ProjectTestFile("/test", new StringContentSource("something"));

            var result = new FileCompilerResult(false);
            projFile.UpdateLastCompileResult(result);

            projFile.LastCompileResult.Should().Be(result);
            projFile.LastCompileResult.Should().NotBe(DateTime.MinValue);
        }

        [Fact]
        public void UpdateLastCompileResultWillCreateFileDefinitionSourceForStepDefs()
        {
            var projFile = new ProjectTestFile("/test", new StringContentSource("something"));

            var builtFile = new FileElement();
            builtFile.AddStepDefinition(new StepDefinitionElement { Type = StepType.Given, Declaration = "I have done a thing" });

            var result = new FileCompilerResult(false, builtFile);
            projFile.UpdateLastCompileResult(result);

            projFile.StepDefinitionSource.Should().NotBeNull();
        }

        [Fact]
        public void UpdateLastLinkResultChangesLastLinkResultAndTime()
        {
            var projFile = new ProjectTestFile("/test", new StringContentSource("something"));

            var result = new LinkResult(false, Enumerable.Empty<LanguageOperationMessage>());
            projFile.UpdateLastLinkResult(result);

            projFile.LastLinkResult.Should().Be(result);
            projFile.LastLinkResult.Should().NotBe(DateTime.MinValue);            
        }

        [Fact]
        public void UpdateLastLinkResultUpdatesLinkedSources()
        {
            var projFile = new ProjectTestFile("/test", new StringContentSource("something"));

            var defSource = new UpdatableTestStepDefinitionSource();

            var result = new LinkResult(true, Enumerable.Empty<LanguageOperationMessage>(), new[] { defSource });
            projFile.UpdateLastLinkResult(result);

            projFile.LinkerDependencies.Should().Contain(defSource);
        }
        
        [Fact]
        public void UpdateLastLinkResultDoesNotTrackNonUpdatableSources()
        {
            var projFile = new ProjectTestFile("/test", new StringContentSource("something"));

            var defSource = new TestStepDefinitionSource();

            var result = new LinkResult(true, Enumerable.Empty<LanguageOperationMessage>(), new[] { defSource });
            projFile.UpdateLastLinkResult(result);

            projFile.LinkerDependencies.Should().BeEmpty();
        }

    }
}
