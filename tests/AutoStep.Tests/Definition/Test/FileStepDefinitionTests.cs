using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Definitions.Test;
using AutoStep.Elements;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Definition
{
    public class FileStepDefinitionTests
    {
        [Fact]
        public void FileStepDefinitionNullElementException()
        {
            Action act = () => new FileStepDefinition(TestStepDefinitionSource.Blank, null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StepDefinitionMatchesAnotherWithSameDeclaration()
        {
            var originalDef = new FileStepDefinition(TestStepDefinitionSource.Blank, new StepDefinitionElement
            {
                Type = StepType.Then,
                Declaration = "something"
            });

            var otherDef = new FileStepDefinition(TestStepDefinitionSource.Blank, new StepDefinitionElement
            {
                Type = StepType.Then,
                Declaration = "something"
            });

            originalDef.IsSameDefinition(otherDef).Should().BeTrue();
        }

    }
}
