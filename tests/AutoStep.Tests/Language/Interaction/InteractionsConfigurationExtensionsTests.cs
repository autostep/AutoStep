using System;
using AutoStep.Definitions.Interaction;
using AutoStep.Language.Interaction;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Interaction
{
    public class InteractionsConfigurationExtensionsTests
    {
        [Fact]
        public void AddOrReplaceMethodDoesNotAllowNullConfig()
        {
            Action method = () => { };

            Assert.Throws<ArgumentNullException>(() => InteractionsConfigurationExtensions.AddOrReplaceMethod(null, new DelegateInteractionMethod("method", method)));
        }

        [Fact]
        public void AddOrReplaceMethodDoesNotAllowNullMethod()
        {
            Assert.Throws<ArgumentNullException>(() => new InteractionsConfig().AddOrReplaceMethod(null));
        }

        [Fact]
        public void AddMethodsDoesNotAllowNullConfig()
        {
            Assert.Throws<ArgumentNullException>(() => InteractionsConfigurationExtensions.AddMethods<MethodsClass>(null));
        }

        [Fact]
        public void AddMethodsOnlyAddsPublicMethods()
        {
            var config = new InteractionsConfig();

            config.AddMethods<MethodsClass>();

            config.RootMethodTable.TryGetMethod("myMethod", out var _).Should().BeTrue();
            config.RootMethodTable.TryGetMethod("myProtectedMethod", out var _).Should().BeFalse();            
        }

        [Fact]
        public void AddMethodsIgnoresUndecoratedMethods()
        {
            var config = new InteractionsConfig();

            config.AddMethods<MethodsClass>();

            config.RootMethodTable.TryGetMethod("myMethod", out var _).Should().BeTrue();
            config.RootMethodTable.TryGetMethod("MyUnDecoratedMethod", out var _).Should().BeFalse();
        }

        private class MethodsClass
        {
            [InteractionMethod("myMethod")]
            public void MyMethod()
            {

            }

            [InteractionMethod("myProtectedMethod")]
            protected void MyProtectedMethod()
            {

            }

            public void MyUnDecoratedMethod()
            {
            }
        }

        private class InteractionsConfig : IInteractionsConfiguration
        {
            public MethodTable RootMethodTable { get; } = new MethodTable();

            public InteractionConstantSet Constants { get; } = new InteractionConstantSet();
        }
    }
}
