using System;
using System.Threading.Tasks;
using AutoStep.Compiler;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Elements.StepTokens;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests.Definition
{
    public class DelegateBackedStepDefinitionTests
    {
        [Fact]
        public async Task CanInvokeDelegateDefinition()
        {
            var source = new Mock<IStepDefinitionSource>();
            var mockScope = new Mock<IServiceScope>();

            var callbackInvoked = false;

            Action<IServiceScope> callback = sc =>
            {
                callbackInvoked = true;
            };
            
            var delDefinition = new DelegateBackedStepDefinition(source.Object, callback.Target, callback.Method, StepType.Given, "I test");

            var stepRefInfo = new StepReferenceElement();
            stepRefInfo.FreezeTokens();
            stepRefInfo.Bind(new StepReferenceBinding(delDefinition, null));

            await delDefinition.ExecuteStepAsync(mockScope.Object, new StepContext(0, null, stepRefInfo, VariableSet.Blank), VariableSet.Blank);

            callbackInvoked.Should().BeTrue();
        }
    }
}
