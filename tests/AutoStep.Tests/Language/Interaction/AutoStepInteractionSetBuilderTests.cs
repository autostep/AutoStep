using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Interaction
{
    public class AutoStepInteractionSetBuilderTests
    {
        private class DummyMethod : InteractionMethod
        {
            private readonly int argCount;

            public DummyMethod(string name, int argCount)
                : base(name)
            {
                this.argCount = argCount;
            }

            public override int ArgumentCount => argCount;

            public override ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object[] arguments, MethodTable methods, Stack<MethodContext> callStack)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void CanBuiltInteractionSetFromTraitAndComponent()
        {
            var setBuilder = new InteractionSetBuilder(new DefaultCallChainValidator());

            var interactions = new InteractionsConfig();
            interactions.AddOrReplaceMethod(new DummyMethod("select", 1));
            interactions.AddOrReplaceMethod(new DummyMethod("click", 0));

            // Create an example file.
            var file = new InteractionFileBuilder();
            file.Trait("clickable", 1, 1, t => t.NamePart("clickable", 1));
            file.Trait("named", 1, 1, t => t.NamePart("named", 1));
            file.Trait("clickable + named", 1, 1, t => t
                .NamePart("clickable", 1)
                .NamePart("named", 2)
                .Method("locateNamed", 3, 1, c => c
                    .Argument("name", 4, 1)
                    .NeedsDefining()
                )
                .StepDefinition(StepType.Given, "I have clicked on the {name} $component$", 7, 1, s => s
                    .WordPart("I", 1)
                    .WordPart("have", 3)
                    .WordPart("clicked", 8)
                    .WordPart("on", 16)
                    .WordPart("the", 19)
                    .Argument("{name}", "name", 23)
                    .ComponentMatch(30)
                    .Expression(e => e
                        .Call("locateNamed", 8, 1, 8, 1, m => m.Variable("name", 1))
                        .Call("click", 9, 1, 9, 1)
                    )
                )
            );
            file.Component("button", 10, 1, c => c
                .Trait("clickable", 11, 1)
                .Trait("named", 11, 1)
                .Method("locateNamed", 12, 1, m => m
                    .Argument("name", 12, 1)
                    .Call("select", 13, 1, 13, 1, cfg => cfg.Variable("name", 1))
                    .Call("click", 14, 1, 14, 1)
                )
            );

            setBuilder.AddInteractionFile(file.Built);

            var result = setBuilder.Build(interactions);

            result.Success.Should().BeTrue();

            var builtSet = result.Output;

            builtSet.Components.Should().HaveCount(1);
            var button = builtSet.Components["button"];

            button.MethodTable.TryGetMethod("select", out var foundSelect).Should().BeTrue();
            foundSelect.Should().BeOfType<DummyMethod>();

            button.MethodTable.TryGetMethod("click", out var foundClick).Should().BeTrue();
            foundClick.Should().BeOfType<DummyMethod>();

            button.MethodTable.TryGetMethod("locateNamed", out var foundLocateNamed).Should().BeTrue();
            foundLocateNamed.Should().BeOfType<FileDefinedInteractionMethod>()
                                     .Subject.NeedsDefining.Should().BeFalse();

            var stepSource = TestStepDefinitionSource.Blank;

            var stepDefs = builtSet.GetStepDefinitions(stepSource).ToList();

            stepDefs.Should().HaveCount(1);
            stepDefs[0].Should().BeOfType<InteractionStepDefinition>();
            var stepParts = stepDefs[0].Definition.Should().BeOfType<InteractionStepDefinitionElement>()
                                                           .Subject.Parts.OfType<PlaceholderMatchPart>();

            // Check that the component matches what we expect.
            stepParts.Should().HaveCount(1);
            var match = stepParts.First().DoStepReferenceMatch("button", new[] { new TextToken(0, "button".Length) });

            match.IsExact.Should().BeTrue();
        }

        [Fact]
        public void ComponentSteps()
        {
            var setBuilder = new InteractionSetBuilder(new DefaultCallChainValidator());

            var interactions = new InteractionsConfig();
            interactions.AddOrReplaceMethod(new DummyMethod("select", 1));
            interactions.AddOrReplaceMethod(new DummyMethod("click", 0));

            // Create an example file.
            var file = new InteractionFileBuilder();
            file.Component("button", 1, 1, c => c
                .StepDefinition(StepType.Given, "I have done", 2, 1, s => s
                    .WordPart("I", 1)
                    .WordPart("have", 3)
                    .WordPart("done", 8)
                    .Expression(e => e
                        .Call("select", 3, 1, 3, 2, s => s.String("something", 3))
                        .Call("click", 4, 1, 4, 2)
                    )
                )
            );

            setBuilder.AddInteractionFile(file.Built);

            var result = setBuilder.Build(interactions);

            result.Success.Should().BeTrue();

            var builtSet = result.Output;

            builtSet.Components.Should().HaveCount(1);
            var button = builtSet.Components["button"];

            button.MethodTable.TryGetMethod("select", out var foundSelect).Should().BeTrue();
            foundSelect.Should().BeOfType<DummyMethod>();

            button.MethodTable.TryGetMethod("click", out var foundClick).Should().BeTrue();
            foundClick.Should().BeOfType<DummyMethod>();

            var stepSource = TestStepDefinitionSource.Blank;

            var stepDefs = builtSet.GetStepDefinitions(stepSource).ToList();

            stepDefs.Should().HaveCount(1);
            stepDefs[0].Should().BeOfType<InteractionStepDefinition>();
            stepDefs[0].Declaration.Should().Be("I have done");
        }

        [Fact]
        public void CanInheritFromExistingCopyOfSelf()
        {
            var setBuilder = new InteractionSetBuilder(new DefaultCallChainValidator());

            var interactions = new InteractionsConfig();
            interactions.AddOrReplaceMethod(new DummyMethod("select", 1));
            interactions.AddOrReplaceMethod(new DummyMethod("click", 0));

            // Create an example file.
            var file = new InteractionFileBuilder();
            file.Component("button", 10, 1, c => c
                .Method("locateNamed", 12, 1, m => m
                    .Argument("name", 12, 1)
                    .Call("select", 13, 1, 13, 1, cfg => cfg.Variable("name", 1))
                    .Call("click", 14, 1, 14, 1)
                )
            );
            file.Component("button", 15, 1, c => c
                .Inherits("button", 16, 1)
                .Method("anotherMethod", 17, 1, m => m.Call("click", 18, 1, 18, 1))
            );

            setBuilder.AddInteractionFile(file.Built);

            var result = setBuilder.Build(interactions);

            result.Success.Should().BeTrue();

            var builtSet = result.Output;

            builtSet.Components.Should().HaveCount(1);
            var button = builtSet.Components["button"];

            button.MethodTable.TryGetMethod("select", out var foundSelect).Should().BeTrue();
            foundSelect.Should().BeOfType<DummyMethod>();

            button.MethodTable.TryGetMethod("click", out var foundClick).Should().BeTrue();
            foundClick.Should().BeOfType<DummyMethod>();

            button.MethodTable.TryGetMethod("locateNamed", out var foundLocateNamed).Should().BeTrue();
            foundLocateNamed.Should().BeOfType<FileDefinedInteractionMethod>()
                                     .Subject.NeedsDefining.Should().BeFalse();

            button.MethodTable.TryGetMethod("anotherMethod", out var foundAnotherMethod).Should().BeTrue();
            foundAnotherMethod.Should().BeOfType<FileDefinedInteractionMethod>()
                                       .Subject.MethodDefinition.Calls[0].MethodName.Should().Be("click");
        }

        [Fact]
        public void CanInheritFromADifferentControl()
        {
            var setBuilder = new InteractionSetBuilder(new DefaultCallChainValidator());

            var interactions = new InteractionsConfig();
            interactions.AddOrReplaceMethod(new DummyMethod("select", 1));
            interactions.AddOrReplaceMethod(new DummyMethod("click", 0));

            // Create an example file.
            var file = new InteractionFileBuilder();
            file.Component("field", 10, 1, c => c
                .Method("locateNamed", 12, 1, m => m
                    .Argument("name", 12, 1)
                    .Call("select", 13, 1, 13, 1, cfg => cfg.Variable("name", 1))
                    .Call("click", 14, 1, 14, 1)
                )
            );
            file.Component("button", 15, 1, c => c
                .Inherits("field", 16, 1)
                .Method("anotherMethod", 17, 1, m => m.Call("click", 18, 1, 18, 1))
            );

            setBuilder.AddInteractionFile(file.Built);

            var result = setBuilder.Build(interactions);

            result.Success.Should().BeTrue();

            var builtSet = result.Output;

            builtSet.Components.Should().HaveCount(2);
            var button = builtSet.Components["button"];

            button.MethodTable.TryGetMethod("select", out var foundSelect).Should().BeTrue();
            foundSelect.Should().BeOfType<DummyMethod>();

            button.MethodTable.TryGetMethod("click", out var foundClick).Should().BeTrue();
            foundClick.Should().BeOfType<DummyMethod>();

            button.MethodTable.TryGetMethod("locateNamed", out var foundLocateNamed).Should().BeTrue();
            foundLocateNamed.Should().BeOfType<FileDefinedInteractionMethod>()
                                     .Subject.NeedsDefining.Should().BeFalse();

            button.MethodTable.TryGetMethod("anotherMethod", out var foundAnotherMethod).Should().BeTrue();
            foundAnotherMethod.Should().BeOfType<FileDefinedInteractionMethod>()
                                       .Subject.MethodDefinition.Calls[0].MethodName.Should().Be("click");
        }

        [Fact]
        public void DirectCircularInheritanceReferenceErrorDetected()
        {
            var setBuilder = new InteractionSetBuilder(new DefaultCallChainValidator());

            var interactions = new InteractionsConfig();
            interactions.AddOrReplaceMethod(new DummyMethod("select", 1));
            interactions.AddOrReplaceMethod(new DummyMethod("click", 0));

            // Create an example file.
            var file = new InteractionFileBuilder();
            file.Component("field", 10, 1, c => c
                .Inherits("button", 11, 1)
                .Method("locateNamed", 12, 1, m => m
                    .Argument("name", 12, 1)
                    .Call("select", 13, 1, 13, 1, cfg => cfg.Variable("name", 1))
                    .Call("click", 14, 1, 14, 1)
                )
            );
            file.Component("button", 15, 1, c => c
                .Inherits("field", 16, 1)
                .Method("anotherMethod", 17, 1, m => m.Call("click", 18, 1, 18, 1))
            );

            setBuilder.AddInteractionFile(file.Built);

            var result = setBuilder.Build(interactions);

            result.Success.Should().BeFalse();
            result.Messages.Should().BeEquivalentTo(
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionComponentInheritanceLoop, 15, 1,
                                              "field -> button -> field"));
        }

        [Fact]
        public void InDirectCircularInheritanceReferenceErrorDetected()
        {
            var setBuilder = new InteractionSetBuilder(new DefaultCallChainValidator());

            var interactions = new InteractionsConfig();
            interactions.AddOrReplaceMethod(new DummyMethod("select", 1));
            interactions.AddOrReplaceMethod(new DummyMethod("click", 0));

            // Create an example file.
            var file = new InteractionFileBuilder();
            file.Component("field", 10, 1, c => c
                .Inherits("input", 11, 1)
            );
            file.Component("button", 15, 1, c => c
                .Inherits("field", 16, 1)
            );
            file.Component("input", 18, 1, c => c
                .Inherits("button", 19, 1)
            );

            setBuilder.AddInteractionFile(file.Built);

            var result = setBuilder.Build(interactions);

            result.Success.Should().BeFalse();
            result.Messages.Should().BeEquivalentTo(
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionComponentInheritanceLoop, 15, 1,
                                              "field -> input -> button -> field"));
        }

        [Fact]
        public void NeedsDefiningMethodGivesError()
        {
            var setBuilder = new InteractionSetBuilder(new DefaultCallChainValidator());

            var interactions = new InteractionsConfig();
            interactions.AddOrReplaceMethod(new DummyMethod("select", 1));
            interactions.AddOrReplaceMethod(new DummyMethod("click", 0));

            // Create an example file.
            var file = new InteractionFileBuilder();
            file.Trait("clickable", 1, 1, t => t.NamePart("clickable", 1));
            file.Trait("named", 1, 1, t => t.NamePart("named", 1));
            file.Trait("clickable + named", 1, 1, t => t
                .NamePart("clickable", 1)
                .NamePart("named", 2)
                // Define locateNamed here, but without indicating it needs defining.
                .Method("locateNamed", 3, 1, c => c
                    .Argument("name", 4, 1)
                    .NeedsDefining()
                )
                .StepDefinition(StepType.Given, "I have clicked on the {name} $component$", 7, 1, s => s
                    .WordPart("I", 1)
                    .WordPart("have", 3)
                    .WordPart("clicked", 8)
                    .WordPart("on", 16)
                    .WordPart("the", 19)
                    .Argument("{name}", "name", 23)
                    .ComponentMatch(30)
                    .Expression(e => e
                        .Call("locateNamed", 8, 1, 8, 1, m => m.Variable("name", 1))
                        .Call("click", 9, 1, 9, 1)
                    )
                )
            );
            file.Component("button", 10, 1, c => c
                .Trait("clickable", 11, 1)
                .Trait("named", 11, 1)
                // We haven't defined the locateNamed method, so I'm expecting an error.
            );

            setBuilder.AddInteractionFile(file.Built);

            var result = setBuilder.Build(interactions);

            result.Success.Should().BeFalse();

            result.Messages.Should().HaveCount(1);
            result.Messages.First().Code.Should().Be(CompilerMessageCode.InteractionMethodFromTraitRequiredButNotDefined);
        }


        [Fact]
        public void ComponentWithNoTraitsDoesNotRequireTheirMethods()
        {
            var setBuilder = new InteractionSetBuilder(new DefaultCallChainValidator());

            var interactions = new InteractionsConfig();

            // Create an example file.
            var file = new InteractionFileBuilder();
            file.Trait("clickable", 1, 1, t => t
                .NamePart("clickable", 1)
                // Define locateNamed here, but without indicating it needs defining.
                .Method("call", 3, 1, c => c
                    .Argument("name", 4, 1)
                    .NeedsDefining()
                )
                .StepDefinition(StepType.Given, "I have clicked on the {name} $component$", 7, 1, s => s
                    .WordPart("I", 1)
                    .WordPart("have", 3)
                    .WordPart("clicked", 8)
                    .WordPart("on", 16)
                    .WordPart("the", 19)
                    .Argument("{name}", "name", 23)
                    .ComponentMatch(30)
                    .Expression(e => e
                        .Call("call", 8, 1, 8, 1, m => m.Variable("name", 1))
                    )
                )
            );

            // Component without traits should not claim it needs defining.
            file.Component("button", 10, 1);

            setBuilder.AddInteractionFile(file.Built);

            var result = setBuilder.Build(interactions);

            result.Success.Should().BeTrue();
        }

        private class InteractionsConfig : IInteractionsConfiguration
        {
            public MethodTable RootMethodTable { get; } = new MethodTable();

            public InteractionConstantSet Constants { get; } = new InteractionConstantSet();
        }
    }
}
