using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Tests.Builders;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests.Language.Interaction
{
    public class DefaultCallChainValidatorTests
    {
        [Fact]
        public void InvalidConstantGivesError()
        {
            var callSource = new CustomCallSource(new PassingVariableSet());
            var methodBuilder = new InteractionMethodCallChainBuilder<CustomCallSource>(callSource);

            // This is the call we are validating.
            methodBuilder.Call("method", 1, 1, 1, 6, cfg => cfg.Constant("NOTACONSTANT", 7));
            methodBuilder.Call("method", 2, 1, 2, 6, cfg => cfg.Constant("NOTACONSTANT2", 7));

            var methodTable = new MethodTable();
            methodTable.Set(new DummyMethod("method", 1));
            
            var constants = new InteractionConstantSet();
            var messages = new List<CompilerMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(null, callSource, methodTable, constants, false, messages);

            messages.Should().BeEquivalentTo(
             CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionConstantNotDefined, 1, 7, 1, 18, "NOTACONSTANT"),
             CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionConstantNotDefined, 2, 7, 2, 19, "NOTACONSTANT2")
            );
        }

        [Fact]
        public void NonExistentMethodGivesSpecificErrorIfMethodDefinitionsAreNotRequired()
        {
            var callSource = new CustomCallSource(new PassingVariableSet());
            var methodBuilder = new InteractionMethodCallChainBuilder<CustomCallSource>(callSource);

            // This is the call we are validating.
            methodBuilder.Call("method", 1, 1, 1, 6);
            methodBuilder.Call("methodDoesNotExist", 2, 1, 2, 6);

            var methodTable = new MethodTable();
            methodTable.Set(new DummyMethod("method", 0));

            var constants = new InteractionConstantSet();
            var messages = new List<CompilerMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(null, callSource, methodTable, constants, false, messages);

            messages.Should().BeEquivalentTo(
             CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNotAvailablePermitUndefined, 2, 1, 2, 6, "methodDoesNotExist")
            );
        }

        [Fact]
        public void NonExistentMethodGivesSpecificErrorIfMethodDefinitionsAreRequired()
        {
            var callSource = new CustomCallSource(new PassingVariableSet());
            var methodBuilder = new InteractionMethodCallChainBuilder<CustomCallSource>(callSource);

            // This is the call we are validating.
            methodBuilder.Call("method", 1, 1, 1, 6);
            methodBuilder.Call("methodDoesNotExist", 2, 1, 2, 6);

            var methodTable = new MethodTable();
            methodTable.Set(new DummyMethod("method", 0));

            var constants = new InteractionConstantSet();
            var messages = new List<CompilerMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(null, callSource, methodTable, constants, true, messages);

            messages.Should().BeEquivalentTo(
             CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNotAvailable, 2, 1, 2, 6, "methodDoesNotExist")
            );
        }

        [Fact]
        public void MethodThatNeedsDefiningGivesErrorWhenDefinitionsAreRequired()
        {
            var callSource = new CustomCallSource(new PassingVariableSet());
            var methodBuilder = new InteractionMethodCallChainBuilder<CustomCallSource>(callSource);

            // This is the call we are validating.
            methodBuilder.Call("method", 1, 1, 1, 6);

            // Register it as 'needs-defining'
            var methodDef = new MethodDefinitionElement();
            methodDef.Name = "method";
            methodDef.SourceLine = 10;
            methodDef.NeedsDefining = true;

            var methodTable = new MethodTable();
            methodTable.Set("method", methodDef);

            var constants = new InteractionConstantSet();
            var messages = new List<CompilerMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(null, callSource, methodTable, constants, true, messages);

            messages.Should().BeEquivalentTo(
             CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodRequiredButNotDefined, 1, 1, 1, 6, "", 10)
            );
        }

        private class DummyMethod : InteractionMethod
        {
            public DummyMethod(string name, int argCount) : base(name)
            {
                ArgumentCount = argCount;
            }

            public override int ArgumentCount { get; }
        }

        private class CustomCallSource : IMethodCallSource
        {
            private readonly InteractionMethodChainVariables variableSet;

            public CustomCallSource(InteractionMethodChainVariables variableSet)
            {
                this.variableSet = variableSet;
            }

            public string SourceName => "custom";

            public List<MethodCallElement> MethodCallChain { get; } = new List<MethodCallElement>();

            public InteractionMethodChainVariables GetInitialMethodChainVariables()
            {
                return variableSet;
            }
        }

        private class FailingVariableSet : InteractionMethodChainVariables
        {
            public override CompilerMessage ValidateVariable(string sourceName, VariableArrayRefMethodArgument nameRefToken)
            {
                return new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, "eek");
            }

            public override CompilerMessage ValidateVariable(string sourceName, VariableRefMethodArgumentElement nameRefToken)
            {
                return new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, "eek");
            }
        }

        private class PassingVariableSet : InteractionMethodChainVariables
        {
            public override CompilerMessage ValidateVariable(string sourceName, VariableArrayRefMethodArgument nameRefToken)
            {
                return new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, "eek");
            }

            public override CompilerMessage ValidateVariable(string sourceName, VariableRefMethodArgumentElement nameRefToken)
            {
                return new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, "eek");
            }
        }
    }
}
