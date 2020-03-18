using System;
using AutoStep.Definitions.Test;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Definition.Test
{
    public class ClassStepDefinitionSourceTests : LoggingTestBase
    {
        public ClassStepDefinitionSourceTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void CanRegisterClass()
        {
            var classDef = new ClassStepDefinitionSource(LogFactory);

            classDef.AddClass<MyClass>();

            var defs = classDef.GetStepDefinitions();

            defs.Should().ContainSingle(x => x.Type == StepType.Given);
            defs.Should().ContainSingle(x => x.Type == StepType.When);
            defs.Should().ContainSingle(x => x.Type == StepType.Then);
        }


        [Fact]
        public void CannotRegisterClassTwice()
        {
            var classDef = new ClassStepDefinitionSource(LogFactory);

            classDef.AddClass<MyClass>();

            classDef.Invoking(c => c.AddClass<MyClass>()).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CannotRegisterAbstractClass()
        {
            var classDef = new ClassStepDefinitionSource(LogFactory);

            classDef.Invoking(c => c.AddClass<AbstractClass>()).Should().Throw<ArgumentException>();
        }

        private class MyClass
        {
            [Given("I have")]
            public void Given()
            {
            }

            [When("I do")]
            public void When()
            {
            }

            [Then("this will")]
            public void Then()
            {
            }
        }

        abstract class AbstractClass
        {
        }
    }
}
