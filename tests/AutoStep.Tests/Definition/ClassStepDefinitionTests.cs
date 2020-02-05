using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Definitions;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using AutoStep.Elements.Test;

namespace AutoStep.Tests.Definition
{
    public class ClassStepDefinitionTests
    {
        [Fact]
        public void NoDeclaringAttributeThrowsNullArgument()
        {
            Action act = () => new ClassStepDefinition(TestStepDefinitionSource.Blank, typeof(MyStepDef), typeof(MyStepDef).GetMethod(nameof(MyStepDef.GivenIHaveClicked)), null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task CanInvokeClassMethod()
        {
            var method = typeof(MyStepDef).GetMethod(nameof(MyStepDef.GivenIHaveClicked));
            var attr = method.GetCustomAttribute<GivenAttribute>();
            var stepDef = new ClassStepDefinition(TestStepDefinitionSource.Blank, typeof(MyStepDef), method, attr);

            var builder = new AutofacServiceBuilder();
            builder.RegisterPerScopeService<MyStepDef>();

            var scope = builder.BuildRootScope();

            var stepRef = new StepReferenceElement();
            stepRef.Bind(new StepReferenceBinding(stepDef, null));

            var stepContext = new StepContext(0, new StepCollectionContext(), stepRef, VariableSet.Blank);

            await stepDef.ExecuteStepAsync(scope, stepContext, VariableSet.Blank);

            var stepClassInstance = scope.Resolve<MyStepDef>();
            stepClassInstance.ClickCount.Should().Be(1);
        }

        private class MyStepDef
        {
            public int ClickCount { get; set; }

            [Given("something")]
            public void GivenIHaveClicked()
            {
                ClickCount++;
            }
        }

    }
}
