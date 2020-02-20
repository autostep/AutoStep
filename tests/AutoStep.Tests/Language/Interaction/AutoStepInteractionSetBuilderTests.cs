using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Definitions;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;
using AutoStep.Language.Interaction;
using AutoStep.Language.Interaction.Parser;
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

            public override void Invoke()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void CanBuiltInteractionSetFromTraitAndComponent()
        {
            var setBuilder = new AutoStepInteractionSetBuilder();

            var rootMethodTable = new MethodTable();
            rootMethodTable.Set(new DummyMethod("select", 1));
            rootMethodTable.Set(new DummyMethod("click", 0));

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

            setBuilder.AddInteractionFile(null, file.Built);

            var result = setBuilder.Build(rootMethodTable);

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
    }
}
