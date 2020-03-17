using System.Collections.Generic;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Tests.Builders;
using FluentAssertions;
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
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(callSource, methodTable, constants, false, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionConstantNotDefined, 1, 7, 1, 18, "NOTACONSTANT"),
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionConstantNotDefined, 2, 7, 2, 19, "NOTACONSTANT2")
            );
        }

        [Fact]
        public void InvalidVariableGivesError()
        {
            var callSource = new CustomCallSource(new CallChainCompileTimeVariables());
            var methodBuilder = new InteractionMethodCallChainBuilder<CustomCallSource>(callSource);

            // This is the call we are validating.
            methodBuilder.Call("method", 1, 1, 1, 6, cfg => cfg.Variable("var", 7));

            var methodTable = new MethodTable();
            methodTable.Set(new DummyMethod("method", 1));

            var constants = new InteractionConstantSet();
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(callSource, methodTable, constants, false, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, 1, 7, 1, 9, "var")
            );
        }

        [Fact]
        public void InvalidVariableArrayRefGivesError()
        {
            var callSource = new CustomCallSource(new CallChainCompileTimeVariables());
            var methodBuilder = new InteractionMethodCallChainBuilder<CustomCallSource>(callSource);

            // This is the call we are validating.
            methodBuilder.Call("method", 1, 1, 1, 6, cfg => cfg.VariableArray("var", 7, 10, c => c.String("index", 1, 1)));

            var methodTable = new MethodTable();
            methodTable.Set(new DummyMethod("method", 1));

            var constants = new InteractionConstantSet();
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(callSource, methodTable, constants, false, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, 1, 7, 1, 10, "var")
            );
        }
        
        [Fact]
        public void VariableArrayRefForNonArrayVariableGivesError()
        {
            var variables = new CallChainCompileTimeVariables();
            variables.SetVariable("var", false);

            var callSource = new CustomCallSource(variables);
            var methodBuilder = new InteractionMethodCallChainBuilder<CustomCallSource>(callSource);

            // This is the call we are validating.
            methodBuilder.Call("method", 1, 1, 1, 6, cfg => cfg.VariableArray("var", 7, 10, c => c.String("index", 1, 1)));

            var methodTable = new MethodTable();
            methodTable.Set(new DummyMethod("method", 1));

            var constants = new InteractionConstantSet();
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(callSource, methodTable, constants, false, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotAnArray, 1, 7, 1, 10, "var")
            );
        }

        [Fact]
        public void ArgumentCountMismatchGivesError()
        {
            var callSource = new CustomCallSource(new CallChainCompileTimeVariables());
            var methodBuilder = new InteractionMethodCallChainBuilder<CustomCallSource>(callSource);

            // This is the call we are validating.
            methodBuilder.Call("method", 1, 1, 1, 6, cfg => cfg.String("var", 7));

            var methodTable = new MethodTable();
            methodTable.Set(new DummyMethod("method", 2));

            var constants = new InteractionConstantSet();
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(callSource, methodTable, constants, false, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodArgumentMismatch,
             lineStart: 1, colStart: 1, lineEnd: 1, colEnd: 6, 
             2, 1)
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
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(callSource, methodTable, constants, false, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNotAvailablePermitUndefined, 2, 1, 2, 6, "methodDoesNotExist")
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
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(callSource, methodTable, constants, true, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNotAvailable, 2, 1, 2, 6, "methodDoesNotExist")
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
            var methodDef = new MethodDefinitionElement("method")
            {
                SourceLine = 10,
                NeedsDefining = true
            };

            var methodTable = new MethodTable();
            methodTable.Set("method", methodDef);

            var constants = new InteractionConstantSet();
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(callSource, methodTable, constants, true, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create("custom", CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodRequiredButNotDefined, 1, 1, 1, 6, "", 10)
            );
        }
        
        [Fact]
        public void CircularMethodCallDetected()
        {
            var fileMethod = new MethodDefinitionBuilder(new MethodDefinitionElement("mydef"));
            fileMethod.Call("mydef", 1, 1, 1, 10);

            var methodTable = new MethodTable();
            methodTable.Set("mydef", fileMethod.Built);

            var constants = new InteractionConstantSet();
            var messages = new List<LanguageOperationMessage>();

            var validator = new DefaultCallChainValidator();
            validator.ValidateCallChain(fileMethod.Built, methodTable, constants, true, messages);

            messages.Should().BeEquivalentTo(
             LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodCircularReference, 1, 1, 1, 10, "", 10)
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

        private class CustomCallSource : ICallChainSource
        {
            private readonly CallChainCompileTimeVariables variableSet;

            public CustomCallSource(CallChainCompileTimeVariables variableSet)
            {
                this.variableSet = variableSet;
            }

            public string SourceName => "custom";

            public List<MethodCallElement> Calls { get; } = new List<MethodCallElement>();

            public CallChainCompileTimeVariables GetCompileTimeChainVariables()
            {
                return variableSet;
            }
        }

        private class PassingVariableSet : CallChainCompileTimeVariables
        {
            public override LanguageOperationMessage? ValidateVariable(string? sourceName, VariableArrayRefMethodArgument nameRefToken)
            {
                return null;
            }

            public override LanguageOperationMessage? ValidateVariable(string? sourceName, VariableRefMethodArgumentElement nameRefToken)
            {
                return null;
            }
        }
    }
}
