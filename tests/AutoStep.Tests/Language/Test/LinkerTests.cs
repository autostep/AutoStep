using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Language;
using AutoStep.Definitions;
using System.Threading.Tasks;
using System;
using System.Linq;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Contexts;
using AutoStep.Language.Test;
using AutoStep.Elements;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using System.Threading;
using Autofac;

namespace AutoStep.Tests.Language.Test
{
    public class LinkerTests : CompilerTestBase
    {
        public LinkerTests(ITestOutputHelper output)
            : base(output)
        {
        }

        private class TestDef : StepDefinition
        {

            public TestDef(IStepDefinitionSource source, StepType type, string declaration) : base(source, type, declaration)
            {
            }

            public TestDef(StepDefinitionElement def)
                : base(TestStepDefinitionSource.Blank, def.Type, def.Declaration!)
            {
                Definition = def;
            }

            public TestDef(StepType type, string declaration) : base(TestStepDefinitionSource.Blank, type, declaration)
            {
            }

            public TestDef(StepType type, string declaration, StepTableRequirement tableRequirement) : base(TestStepDefinitionSource.Blank, type, declaration)
            {
                TableRequirement = tableRequirement;
            }

            public override StepTableRequirement TableRequirement { get; } = StepTableRequirement.Optional;

            public override bool IsSameDefinition(StepDefinition def)
            {
                return ReferenceEquals(def, this);
            }

            public override ValueTask ExecuteStepAsync(ILifetimeScope stepScope, StepContext context, VariableSet variables, CancellationToken cancelToken)
            {
                throw new NotImplementedException();
            }

            public override string? GetDocumentation()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void SourceLoadPopulatesDefinition()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();
        }

        [Fact]
        public void AddStepDefinitionSourceNullArgument()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            linker.Invoking(l => l.AddStepDefinitionSource(null!)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LinksStepToDefinition()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given("I have done something", 1, 1, step => step
                        .Text("I")
                        .Text("have")
                        .Text("done")
                        .Text("something")
                    )
                ));

            var file = fileBuilder.Built!;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            var binding = file.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            // After linking, the step reference should have a definition assigned.
            binding!.Definition.Should().Be(def);
            binding!.Arguments.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void CanRemoveDefinitionSource()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            var source = new TestStepDefinitionSource(def);

            linker.AddStepDefinitionSource(source);

            def.Definition.Should().NotBeNull();

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given("I have done something", 1, 1, step => step
                        .Text("I")
                        .Text("have")
                        .Text("done")
                        .Text("something")
                    )
                ));

            var file = fileBuilder.Built;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();

            linker.RemoveStepDefinitionSource(source);

            // Relink after removing the source; linking should fail.
            linkResult = linker.Link(file);

            linkResult.Success.Should().BeFalse();
            linkResult.Messages.Should().HaveCount(1);
        }

        [Fact]
        public void UpdatePlaceholderDefinition()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            InteractionStepDefinitionElement DefForComponentName(string name)
            {
                var stepDefBuilder = new InteractionStepDefinitionBuilder(StepType.Given, "The $component$ value", 1, 1);
                stepDefBuilder.WordPart("The", 1).ComponentMatch(5).WordPart("value", 17);

                var interactionStep1 = stepDefBuilder.Built;

                interactionStep1.AddComponentMatch(name);

                return interactionStep1;
            }

            var testDef = new InteractionStepDefinition(TestStepDefinitionSource.Blank, DefForComponentName("button"));

            var source = new UpdatableTestStepDefinitionSource(testDef);

            linker.AddStepDefinitionSource(source);

            var reference = new StepReferenceBuilder("The field value", StepType.Given, StepType.Given, 1, 1)
                                                    .Text("The").Text("field").Text("value").Built;
            reference.FreezeTokens();

            linker.BindSingleStep(reference).Should().BeFalse();

            // Now update the source.
            source.ReplaceStepDefinitions(new InteractionStepDefinition(TestStepDefinitionSource.Blank, DefForComponentName("field")));

            linker.AddOrUpdateStepDefinitionSource(source);

            // Linker should now bind.
            linker.BindSingleStep(reference).Should().BeTrue();
        }


        [Fact]
        public void CannotRemoveUnregisteredStepDefinition()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            var source = new TestStepDefinitionSource(def);

            linker.Invoking(l => l.RemoveStepDefinitionSource(source)).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void LinkingErrorOnOneStepAllowsContinue()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given("This will not match", 1, 1, step => step
                        .Text("This")
                        .Text("will")
                        .Text("not")
                        .Text("match")
                    )
                    .Given("I have done something", 1, 1, step => step
                        .Text("I")
                        .Text("have")
                        .Text("done")
                        .Text("something")
                    )
                ));

            var file = fileBuilder.Built;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeFalse();

            linkResult.Messages.Should().BeEquivalentTo(new[]
            {
                new LanguageOperationMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.LinkerNoMatchingStepDefinition,
                                    "No step definitions could be found that match this step.", 1, 1, 1, 25)
            });

            // The failing definition should not have a bound definition.
            file.AllStepReferences.First!.Value.Binding.Should().BeNull();

            // After linking, the last step reference should have a definition assigned.
            var binding = file.AllStepReferences.Last!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().Be(def);
            binding!.Arguments.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void LinkingErrorMultipleDefinitions()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var src1 = new TestStepDefinitionSource("src1");

            src1.AddStepDefinition(StepType.Given, "I have done something");

            var src2 = new TestStepDefinitionSource("src2");

            src2.AddStepDefinition(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(src1);
            linker.AddStepDefinitionSource(src2);

            // Build a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 2, 1, scen => scen
                    .Given("I have done something", 3, 1, step => step
                        .Text("I")
                        .Text("have")
                        .Text("done")
                        .Text("something")
                    )
                ));

            var file = fileBuilder.Built;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeFalse();

            linkResult.Messages.Should().BeEquivalentTo(new[]
            {
                new LanguageOperationMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.LinkerMultipleMatchingDefinitions,
                                    "There are multiple matching step definitions that match this step.", 3, 1, 3, 27)
            });

            // The failing definition should not have a bound definition.
            file.AllStepReferences.First!.Value.Binding.Should().BeNull();
        }

        [Fact]
        public void StepArgumentBindingArguments()
        {
            const string refText = "I have provided value";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Text("value")
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].Part.Name.Should().Be("arg");
            // Start and end are the same.
            arguments[0].MatchedTokens.Length.Should().Be(1);
            arguments[0].GetText(refText).Should().Be("value");
            arguments[0].StartExclusive.Should().BeFalse();
            arguments[0].EndExclusive.Should().BeFalse();
        }

        [Fact]
        public void StepArgumentBindingMultipleArguments()
        {
            const string refText = "I have provided value and value2";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg} and {arg2}", refText, step => step
                                         .Text("I")
                                         .Text("have")
                                         .Text("provided")
                                         .Text("value")
                                         .Text("and")
                                         .Text("value")
                                         .Int("2")
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(2);
            arguments[0].Part.Name.Should().Be("arg");
            // Start and end are the same.
            arguments[0].MatchedTokens.Length.Should().Be(1);
            arguments[0].GetText(refText).Should().Be("value");
            arguments[0].MatchedTokens[0].GetText(refText).Should().Be("value");

            arguments[1].Part.Name.Should().Be("arg2");

            // Start and end are different.
            arguments[1].MatchedTokens.Length.Should().Be(2);
            arguments[1].MatchedTokens[0].GetText(refText).Should().Be("value");
            arguments[1].MatchedTokens[1].GetText(refText).Should().Be("2");
            arguments[1].GetText(refText).Should().Be("value2");
        }

        [Fact]
        public void StepArgumentBindingQuotedArgument()
        {
            const string refText = "I have provided 'value and value2'";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Text("value")
                                        .Text("and")
                                        .Text("value")
                                        .Int("2")
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].Part.Name.Should().Be("arg");
            // Start and end are different
            arguments[0].MatchedTokens[0].GetText(refText).Should().Be("'");
            arguments[0].MatchedTokens[5].GetText(refText).Should().Be("'");
            arguments[0].MatchedTokens.Length.Should().Be(6);
            arguments[0].GetText(refText).Should().Be("value and value2");
            arguments[0].StartExclusive.Should().BeTrue();
            arguments[0].EndExclusive.Should().BeTrue();
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
        }

        [Fact]
        public void StepArgumentBindingEmptyQuotedArgument()
        {
            const string refText = "I have provided ''";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].Part.Name.Should().Be("arg");
            // Start and end are different
            arguments[0].MatchedTokens[0].GetText(refText).Should().Be("'");
            arguments[0].MatchedTokens[1].GetText(refText).Should().Be("'");
            arguments[0].GetText(refText).Should().Be("");
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
        }

        [Fact]
        public void StepArgumentBindingWhiteSpaceOnlyQuotedArgument()
        {
            const string refText = "I have provided '  '";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].Part.Name.Should().Be("arg");
            // Start and end are different
            arguments[0].MatchedTokens[0].GetText(refText).Should().Be("'");
            arguments[0].MatchedTokens[1].GetText(refText).Should().Be("'");
            arguments[0].GetText(refText).Should().Be("  ");
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
        }

        [Fact]
        public void StepArgumentWhitespaceBeforeTokenTextDetected()
        {
            const string refText = "I have provided ' 100.5'";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Float("100.5")
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();

            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
            arguments[0].GetText(refText).Should().Be(" 100.5");
        }

        [Fact]
        public void StepArgumentWhitespaceAfterTokenTextDetected()
        {
            const string refText = "I have provided '100.5 '";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Float("100.5")
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();

            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
            arguments[0].GetText(refText).Should().Be("100.5 ");
        }

        [Fact]
        public void StepArgumentWhitespaceBeforeAndAfterTokenTextDetected()
        {
            const string refText = "I have provided ' 100.5 '";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Float("100.5")
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();

            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
            arguments[0].GetText(refText).Should().Be(" 100.5 ");
        }

        [Fact]
        public void StepArgumentFloatTypeDetected()
        {
            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", "I have provided 100.5", step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Float("100.5")
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            binding!.Arguments.Length.Should().Be(1);
            binding!.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericDecimal);
        }

        [Fact]
        public void StepArgumentIntTypeDetected()
        {
            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", "I have provided 100", step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Int("100")
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            binding!.Arguments.Length.Should().Be(1);
            binding!.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericInteger);
        }

        [Fact]
        public void StepArgumentIntTypeDetectedInsideQuotes()
        {
            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", "I have provided '100'", step => step
                                      .Text("I")
                                      .Text("have")
                                      .Text("provided")
                                      .Quote()
                                      .Int("100")
                                      .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            binding!.Arguments.Length.Should().Be(1);
            binding!.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericInteger);
        }

        [Fact]
        public void StepArgumentFloatTypeDetectedInsideQuotes()
        {
            const string refText = "I have provided '100.5'";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Float("100.5")
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            binding!.Arguments.Length.Should().Be(1);
            binding!.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericDecimal);
        }

        [Fact]
        public void StepArgumentTextToIntCompilerError()
        {
            const string refText = "I have provided text";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg:int}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Text("text")
                                     );

            linkResult.Success.Should().BeFalse();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var span = binding!.Arguments;
            span.Length.Should().Be(1);
            span[0].DeterminedType.Should().Be(ArgumentType.Text);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(LanguageMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.ArgumentTypeNotCompatible, span[0],
                                                    span[0].DeterminedType!, span[0].Part.TypeHint!));
        }

        [Fact]
        public void StepArgumentTextToFloatCompilerError()
        {
            const string refText = "I have provided text";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg:float}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Text("text")
                                     );

            linkResult.Success.Should().BeFalse();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(LanguageMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.ArgumentTypeNotCompatible, arguments[0],
                                                    arguments[0].DeterminedType!, arguments[0].Part.TypeHint!));
        }

        [Fact]
        public void StepArgumentFloatToIntCompilerError()
        {
            const string refText = "I have provided 100.5";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg:int}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Float("100.5")
                                     );

            linkResult.Success.Should().BeFalse();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].DeterminedType.Should().Be(ArgumentType.NumericDecimal);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(LanguageMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.ArgumentTypeNotCompatible, arguments[0],
                                                    arguments[0].DeterminedType!, arguments[0].Part.TypeHint!));
        }

        [Fact]
        public void StepArgumentEmptyValueForFloatCompilerError()
        {
            const string refText = "I have provided ''";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg:float}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Quote()
                                     );

            linkResult.Success.Should().BeFalse();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(LanguageMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.TypeRequiresValueForArgument, arguments[0],
                                                    arguments[0].Part.TypeHint!));
        }

        [Fact]
        public void StepArgumentEmptyValueForIntCompilerError()
        {
            const string refText = "I have provided ''";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg:int}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Quote()
                                     );

            linkResult.Success.Should().BeFalse();
            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            arguments[0].DeterminedType.Should().Be(ArgumentType.Text);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(LanguageMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.TypeRequiresValueForArgument, arguments[0],
                                                    arguments[0].Part.TypeHint!));
        }

        [Fact]
        public void StepArgumentInterpolatedGivesUnknownType()
        {
            const string refText = "I have provided :time";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .InterpolateStart()
                                        .Text("time")
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();

            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            // No determined type if using interpolation.
            arguments[0].DeterminedType.Should().BeNull();
            arguments[0].GetText(refText).Should().Be(":time");
        }

        [Fact]
        public void StepArgumentInterpolatedWithMultipleTokensGivesUnknownType()
        {
            const string refText = "I have provided 'a :time'";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Text("a")
                                        .InterpolateStart()
                                        .Text("time")
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();

            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            // No determined type if using interpolation.
            arguments[0].DeterminedType.Should().BeNull();
            arguments[0].GetText(refText).Should().Be("a :time");
        }

        [Fact]
        public void StepArgumentVariableGivesUnknownType()
        {
            const string refText = "I have provided <var>";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Variable("var")
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();

            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            // No determined type if using variables.
            arguments[0].DeterminedType.Should().BeNull();
            arguments[0].GetText(refText).Should().Be("<var>");
        }

        [Fact]
        public void StepArgumentVariableWithMultipleTokensGivesUnknownType()
        {
            const string refText = "I have provided 'a <var>'";

            var linkResult = LinkTest(StepType.Given, "I have provided {arg}", refText, step => step
                                        .Text("I")
                                        .Text("have")
                                        .Text("provided")
                                        .Quote()
                                        .Text("a")
                                        .Variable("var")
                                        .Quote()
                                     );

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();

            // After linking, the step reference should have a definition assigned.
            var binding = linkResult.Output!.AllStepReferences.First!.Value.Binding;
            binding.Should().NotBeNull();
            binding!.Definition.Should().NotBeNull();
            var arguments = binding!.Arguments;
            arguments.Length.Should().Be(1);
            // No determined type if using variables.
            arguments[0].DeterminedType.Should().BeNull();
            arguments[0].GetText(refText).Should().Be("a <var>");
        }

        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/42")]
        public void RemovingStepUnbindsOnNextLink()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            var stepSource = new UpdatableTestStepDefinitionSource(def);

            linker.AddOrUpdateStepDefinitionSource(stepSource);

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given("I have done something", 1, 1, step => step
                        .Text("I")
                        .Text("have")
                        .Text("done")
                        .Text("something")
                    )
                ));

            var file = fileBuilder.Built;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeTrue();

            // The failing definition should not have a bound definition.
            file.AllStepReferences.First!.Value.Binding.Should().NotBeNull();

            stepSource.RemoveStepDefinition(def);

            // Need to update the step definition source.
            linker.AddOrUpdateStepDefinitionSource(stepSource);

            linkResult = linker.Link(file);

            linkResult.Success.Should().BeFalse();

            file.AllStepReferences.First!.Value.Binding.Should().BeNull();
        }

        [Fact]
        public void ReturnsAllPossibleMatches()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I will have done something");
            var def2 = new TestDef(StepType.Given, "I will have");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def, def2));

            var reference = new StepReferenceBuilder("", StepType.Given, StepType.Given, 1, 1).Built;
            reference.FreezeTokens();

            var result = linker.GetPossibleMatches(reference);

            result.Should().HaveCount(2);
        }

        [Fact]
        public void ReturnsAllPossibleMatchesIncludingExact()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I will have done something");
            var def2 = new TestDef(StepType.Given, "I will have");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def, def2));

            var reference = new StepReferenceBuilder("I will have", StepType.Given, StepType.Given, 1, 1)
                                                    .Text("I").Text("will").Text("have").Built;
            reference.FreezeTokens();

            var result = linker.GetPossibleMatches(reference);

            result.Should().HaveCount(2);

            result.First().IsExact.Should().BeTrue();
        }

        [Fact]
        public void PartialMatchesIncludeBoundArgument()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I will {arg} done something");
            var def2 = new TestDef(StepType.Given, "I will {arg} not");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def, def2));

            var reference = new StepReferenceBuilder("I will really", StepType.Given, StepType.Given, 1, 1)
                                                    .Text("I").Text("will").Text("really").Built;
            reference.FreezeTokens();

            var result = linker.GetPossibleMatches(reference).ToList();

            result.Should().HaveCount(2);
            result[0].Arguments.First().GetRawText("I will really").Should().Be("really");
            result[0].MatchedParts.Should().Be(3);
            result[1].Arguments.First().GetRawText("I will really").Should().Be("really");
            result[1].MatchedParts.Should().Be(3);
        }

        [Fact]
        public void PartialMatchesIncludePlaceholderArgument()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var stepDefBuilder = new InteractionStepDefinitionBuilder(StepType.Given, "The $component$ value", 1, 1);
            stepDefBuilder.WordPart("The", 1).ComponentMatch(5).WordPart("value", 17);

            var stepDefBuilder2 = new InteractionStepDefinitionBuilder(StepType.Given, "The $component$ will not", 1, 1);
            stepDefBuilder2.WordPart("The", 1).ComponentMatch(5).WordPart("will", 17).WordPart("not", 22);

            var interactionStep1 = stepDefBuilder.Built;
            var interactionStep2 = stepDefBuilder2.Built;

            interactionStep1.AddComponentMatch("button");
            interactionStep2.AddComponentMatch("button");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(new TestDef(interactionStep1), new TestDef(interactionStep2)));

            var reference = new StepReferenceBuilder("The button", StepType.Given, StepType.Given, 1, 1)
                                                    .Text("The").Text("button").Built;
            reference.FreezeTokens();

            var result = linker.GetPossibleMatches(reference).ToList();

            result.Should().HaveCount(2);
            result[0].PlaceholderValues!["component"].Should().Be("button");
            result[0].MatchedParts.Should().Be(2);
            result[1].PlaceholderValues!["component"].Should().Be("button");
            result[1].MatchedParts.Should().Be(2);
        }

        [Fact]
        public void StepsCanDifferOnlyByMatchingPlaceholders()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var stepDefBuilder = new InteractionStepDefinitionBuilder(StepType.Given, "The $component$ value", 1, 1);
            stepDefBuilder.WordPart("The", 1).ComponentMatch(5).WordPart("value", 17);

            var stepDefBuilder2 = new InteractionStepDefinitionBuilder(StepType.Given, "The $component$ value", 1, 1);
            stepDefBuilder2.WordPart("The", 1).ComponentMatch(5).WordPart("value", 17);

            var interactionStep1 = stepDefBuilder.Built;
            var interactionStep2 = stepDefBuilder2.Built;

            interactionStep1.AddComponentMatch("button");
            interactionStep2.AddComponentMatch("field");

            var interactionStepDef1 = new InteractionStepDefinition(TestStepDefinitionSource.Blank, interactionStep1);
            var interactionStepDef2 = new InteractionStepDefinition(TestStepDefinitionSource.Blank, interactionStep2);

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(interactionStepDef1, interactionStepDef2));

            var reference1 = new StepReferenceBuilder("The button value", StepType.Given, StepType.Given, 1, 1)
                                                    .Text("The").Text("button").Text("value").Built;
            reference1.FreezeTokens();

            linker.BindSingleStep(reference1).Should().BeTrue();

            reference1.Binding!.Definition.Should().BeSameAs(interactionStepDef1);

            var reference2 = new StepReferenceBuilder("The field value", StepType.Given, StepType.Given, 1, 1)
                                                    .Text("The").Text("field").Text("value").Built;
            reference2.FreezeTokens();

            linker.BindSingleStep(reference2).Should().BeTrue();

            reference2.Binding!.Definition.Should().BeSameAs(interactionStepDef2);
        }

        [Fact]
        public void TableProvidedButNotSupportedGivesWarning()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I have done something", StepTableRequirement.NotSupported);

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given("I have done something", 1, 1, step => step
                        .Text("I")
                        .Text("have")
                        .Text("done")
                        .Text("something")
                        .Table(2, 1, table => table
                            .Headers(2, 1, ("header1", 1, 1))
                            .Row(3, 1, ("value", 1, 1, null))
                    )
                )));

            var file = fileBuilder.Built!;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.Should().Contain(LanguageMessageFactory.Create(null, CompilerMessageLevel.Warning, CompilerMessageCode.TableNotRequired, 1, 1, 1, 27));
        }

        [Fact]
        public void TableRequiredButNotProvidedGivesError()
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(StepType.Given, "I have done something", StepTableRequirement.Required);

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given("I have done something", 1, 1, step => step
                        .Text("I")
                        .Text("have")
                        .Text("done")
                        .Text("something")
                    )
                ));

            var file = fileBuilder.Built!;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeFalse();
            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.Should().Contain(LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.TableRequired, 1, 1, 1, 27));
        }

        private LinkResult LinkTest(StepType type, string defText, string refText, Action<StepReferenceBuilder> builder)
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);

            var linker = new Linker(compiler);

            var def = new TestDef(type, defText);

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given(refText, 1, 1, builder)
                ));

            var file = fileBuilder.Built;

            return linker.Link(file);
        }
    }
}
