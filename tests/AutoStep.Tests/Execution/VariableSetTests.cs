using AutoStep.Execution;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Execution
{
    public class VariableSetTests
    {
        [Fact]
        public void CanRetrieveVariableValue()
        {
            var variableSet = new VariableSet();

            variableSet.Set("variable", "value");

            variableSet.Get("variable").Should().Be("value");
        }

        [Fact]
        public void HoldsMultipleValues()
        {
            var variableSet = new VariableSet();

            variableSet.Set("variable1", "value1");
            variableSet.Set("variable2", "value2");

            variableSet.Get("variable1").Should().Be("value1");
        }

        [Fact]
        public void CanUpdateValue()
        {
            var variableSet = new VariableSet();

            variableSet.Set("variable1", "value1");
            variableSet.Set("variable1", "value2");

            variableSet.Get("variable1").Should().Be("value2");

        }

        [Fact]
        public void NoValueGivesEmptyString()
        {
            var variableSet = new VariableSet();

            variableSet.Get("variable").Should().BeEmpty();
        }
    }
}
