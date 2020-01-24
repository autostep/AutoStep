using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Compiler;
using AutoStep.Definitions;
using System.Threading.Tasks;
using System;
using System.Linq;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Contexts;

namespace AutoStep.Tests.Compiler
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

            public TestDef(StepType type, string declaration) : base(TestStepDefinitionSource.Blank, type, declaration)
            {
            }

            public override bool IsSameDefinition(StepDefinition def)
            {
                return ReferenceEquals(def, this);
            }

            public override ValueTask ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void SourceLoadPopulatesDefinition()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var linker = new AutoStepLinker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();
        }

        [Fact]
        public void AddStepDefinitionSourceNullArgument()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var linker = new AutoStepLinker(compiler);

            linker.Invoking(l => l.AddStepDefinitionSource(null)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LinksStepToDefinition()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var linker = new AutoStepLinker(compiler);

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

            var file = fileBuilder.Built;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            var binding = file.AllStepReferences.First.Value.Binding;
            binding.Should().NotBeNull();
            // After linking, the step reference should have a definition assigned.
            binding.Definition.Should().Be(def);
            binding.Arguments.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void CanRemoveDefinitionSource()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var linker = new AutoStepLinker(compiler);

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
        public void CannotRemoveUnregisteredStepDefinition()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var linker = new AutoStepLinker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            var source = new TestStepDefinitionSource(def);
            
            linker.Invoking(l => l.RemoveStepDefinitionSource(source)).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void LinkingErrorOnOneStepAllowsContinue()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var linker = new AutoStepLinker(compiler);

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
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.LinkerNoMatchingStepDefinition,
                                    "No step definitions could be found that match this step.", 1, 1)
            });

            // The failing definition should not have a bound definition.
            file.AllStepReferences.First.Value.Binding.Should().BeNull();

            // After linking, the last step reference should have a definition assigned.
            var binding = file.AllStepReferences.Last.Value.Binding;
            binding.Should().NotBeNull();
            binding.Definition.Should().Be(def);
            binding.Arguments.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void LinkingErrorMultipleDefinitions()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var linker = new AutoStepLinker(compiler);

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
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.LinkerMultipleMatchingDefinitions,
                                    "There are multiple matching step definitions that match this step.", 3, 1)
            });

            // The failing definition should not have a bound definition.
            file.AllStepReferences.First.Value.Binding.Should().BeNull();
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].Part.Name.Should().Be("arg");
            // Start and end are the same.
            binding.Arguments[0].MatchedTokens.Length.Should().Be(1);
            binding.Arguments[0].GetText(refText).Should().Be("value");
            binding.Arguments[0].StartExclusive.Should().BeFalse();
            binding.Arguments[0].EndExclusive.Should().BeFalse();
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(2);
            binding.Arguments[0].Part.Name.Should().Be("arg");
            // Start and end are the same.
            binding.Arguments[0].MatchedTokens.Length.Should().Be(1);
            binding.Arguments[0].GetText(refText).Should().Be("value");
            binding.Arguments[0].MatchedTokens[0].GetText(refText).Should().Be("value");

            binding.Arguments[1].Part.Name.Should().Be("arg2");

            // Start and end are different.
            binding.Arguments[1].MatchedTokens.Length.Should().Be(2);
            binding.Arguments[1].MatchedTokens[0].GetText(refText).Should().Be("value");
            binding.Arguments[1].MatchedTokens[1].GetText(refText).Should().Be("2");
            binding.Arguments[1].GetText(refText).Should().Be("value2");
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].Part.Name.Should().Be("arg");
            // Start and end are different
            binding.Arguments[0].MatchedTokens[0].GetText(refText).Should().Be("'");
            binding.Arguments[0].MatchedTokens[5].GetText(refText).Should().Be("'");            
            binding.Arguments[0].MatchedTokens.Length.Should().Be(6);
            binding.Arguments[0].GetText(refText).Should().Be("value and value2");
            binding.Arguments[0].StartExclusive.Should().BeTrue();
            binding.Arguments[0].EndExclusive.Should().BeTrue();
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].Part.Name.Should().Be("arg");
            // Start and end are different
            binding.Arguments[0].MatchedTokens[0].GetText(refText).Should().Be("'");
            binding.Arguments[0].MatchedTokens[1].GetText(refText).Should().Be("'");
            binding.Arguments[0].GetText(refText).Should().Be("");
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].Part.Name.Should().Be("arg");
            // Start and end are different
            binding.Arguments[0].MatchedTokens[0].GetText(refText).Should().Be("'");
            binding.Arguments[0].MatchedTokens[1].GetText(refText).Should().Be("'");
            binding.Arguments[0].GetText(refText).Should().Be("  ");
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
            binding.Arguments[0].GetText(refText).Should().Be(" 100.5");
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
            binding.Arguments[0].GetText(refText).Should().Be("100.5 ");
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
            binding.Arguments[0].GetText(refText).Should().Be(" 100.5 ");
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericDecimal);
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericInteger);
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericInteger);
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericDecimal);
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);
            
            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(CompilerMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.ArgumentTypeNotCompatible, binding.Arguments[0], 
                                                    binding.Arguments[0].DeterminedType, binding.Arguments[0].Part.TypeHint));
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(CompilerMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.ArgumentTypeNotCompatible, binding.Arguments[0],
                                                    binding.Arguments[0].DeterminedType, binding.Arguments[0].Part.TypeHint));
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.NumericDecimal);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(CompilerMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.ArgumentTypeNotCompatible, binding.Arguments[0],
                                                    binding.Arguments[0].DeterminedType, binding.Arguments[0].Part.TypeHint));
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(CompilerMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.TypeRequiresValueForArgument, binding.Arguments[0],
                                                    binding.Arguments[0].Part.TypeHint));
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            binding.Arguments[0].DeterminedType.Should().Be(ArgumentType.Text);

            linkResult.Messages.Should().HaveCount(1);
            linkResult.Messages.First().Should().Be(CompilerMessageFactory.Create(null, CompilerMessageLevel.Error,
                                                    CompilerMessageCode.TypeRequiresValueForArgument, binding.Arguments[0],
                                                    binding.Arguments[0].Part.TypeHint));
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            // No determined type if using interpolation.
            binding.Arguments[0].DeterminedType.Should().BeNull();
            binding.Arguments[0].GetText(refText).Should().Be(":time");
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            // No determined type if using interpolation.
            binding.Arguments[0].DeterminedType.Should().BeNull();
            binding.Arguments[0].GetText(refText).Should().Be("a :time");
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            // No determined type if using variables.
            binding.Arguments[0].DeterminedType.Should().BeNull();
            binding.Arguments[0].GetText(refText).Should().Be("<var>");
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
            var binding = linkResult.Output.AllStepReferences.First.Value.Binding;
            binding.Definition.Should().NotBeNull();
            binding.Arguments.Length.Should().Be(1);
            // No determined type if using variables.
            binding.Arguments[0].DeterminedType.Should().BeNull();
            binding.Arguments[0].GetText(refText).Should().Be("a <var>");
        }

        private LinkResult LinkTest(StepType type, string defText, string refText, Action<StepReferenceBuilder> builder)
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics);

            var linker = new AutoStepLinker(compiler);

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
