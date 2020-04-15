using System;
using AutoStep.Definitions.Interaction;
using AutoStep.Language.Interaction;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Definition.Interaction
{
    public class ClassBackedInteractionMethodTests
    {
        private class StandardDocs
        {
            [InteractionMethod("method1", Documentation = @"
                Call method1

                After blank

                
            ")]
            public void Method()
            {
            }
        }

        [Fact]
        public void DocumentationBlockParsedCorrectly()
        {
            var loadedMethod = GetInteractionMethod<StandardDocs>("method1");

            var docs = loadedMethod.GetDocumentation();
            docs.Should().NotBeNullOrEmpty();
            docs!.Split(Environment.NewLine).Should().BeEquivalentTo(
                "Call method1",
                "",
                "After blank");
        }

        private class IndentedDocs
        {
            [InteractionMethod("method1", Documentation = @"
                Call method1

                   After blank")]
            public void Method()
            {
            }
        }

        [Fact]
        public void DocumentationIndentedBlockParsedCorrectly()
        {
            var loadedMethod = GetInteractionMethod<IndentedDocs>("method1");

            var docs = loadedMethod.GetDocumentation();
            docs.Should().NotBeNullOrEmpty();
            docs!.Split(Environment.NewLine).Should().BeEquivalentTo(
                "Call method1",
                "",
                "   After blank");
        }

        private class PaddedBlankLines
        {
            [InteractionMethod("method1", Documentation = @"

                Call method1

                   After blank

            ")]
            public void Method()
            {
            }
        }

        [Fact]
        public void DocumentationPaddedLinesParsedCorrectly()
        {
            var loadedMethod = GetInteractionMethod<PaddedBlankLines>("method1");

            var docs = loadedMethod.GetDocumentation();
            docs.Should().NotBeNullOrEmpty();
            docs!.Split(Environment.NewLine).Should().BeEquivalentTo(
                "Call method1",
                "",
                "   After blank");
        }

        private class LinuxLineEndings
        {
            [InteractionMethod("method1", Documentation = "Call method1\n   \n   After Blank\n")]
            public void Method()
            {
            }
        }

        [Fact]
        public void DocumentationBlockLinuxLineEndingsParsedCorrectly()
        {
            var loadedMethod = GetInteractionMethod<LinuxLineEndings>("method1");

            var docs = loadedMethod.GetDocumentation();
            docs.Should().NotBeNullOrEmpty();
            docs!.Split(Environment.NewLine).Should().BeEquivalentTo(
                "Call method1",
                "",
                "   After Blank");
        }

        private ClassBackedInteractionMethod GetInteractionMethod<TClass>(string name)
            where TClass : class
        {
            var interactionsConfig = new DummyConfig();

            interactionsConfig.AddMethods<TClass>();

            interactionsConfig.RootMethodTable.TryGetMethod(name, out var result).Should().BeTrue();

            return (ClassBackedInteractionMethod)result!;
        }

        private class DummyConfig : IInteractionsConfiguration
        {
            public RootMethodTable RootMethodTable { get; } = new RootMethodTable();

            public InteractionConstantSet Constants => throw new NotImplementedException();
        }
    }
}
