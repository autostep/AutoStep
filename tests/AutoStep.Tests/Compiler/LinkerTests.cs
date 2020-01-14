using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Compiler;
using AutoStep.Definitions;
using System.Threading.Tasks;
using System;

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
        }

        [Fact]
        public void SourceLoadPopulatesDefinition()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);

            var linker = new AutoStepLinker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();
        }

        [Fact]
        public void LinksStepToDefinition()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);

            var linker = new AutoStepLinker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given("I have done something", 1, 1, step => step
                        .Word("I", 1)
                        .Word("have", 3)
                        .Word("done", 8)
                        .Word("something", 13)
                    )
                ));

            var file = fileBuilder.Built;

            var linkResult = linker.Link(file);

            linkResult.Success.Should().BeTrue();
            linkResult.Messages.Should().BeEmpty();
            // After linking, the step reference should have a definition assigned.
            file.AllStepReferences.First.Value.BoundDefinition.Should().Be(def);
        }

        [Fact]
        public void LinkingErrorOnOneStepAllowsContinue()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);

            var linker = new AutoStepLinker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(new TestStepDefinitionSource(def));

            def.Definition.Should().NotBeNull();

            // Built a file and check it links.
            var fileBuilder = new FileBuilder();
            fileBuilder.Feature("My Feature", 1, 1, feat => feat
                .Scenario("My Scenario", 1, 1, scen => scen
                    .Given("This will not match", 1, 1, step => step
                        .Word("This", 1)
                        .Word("will", 6)
                        .Word("not", 11)
                        .Word("match", 15)
                    )
                    .Given("I have done something", 1, 1, step => step
                        .Word("I", 1)
                        .Word("have", 3)
                        .Word("done", 8)
                        .Word("something", 13)
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
            file.AllStepReferences.First.Value.BoundDefinition.Should().BeNull();

            // After linking, the last step reference should have a definition assigned.
            file.AllStepReferences.Last.Value.BoundDefinition.Should().Be(def);
        }

        [Fact]
        public void LinkingErrorMultipleDefinitions()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);

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
                        .Word("I", 1)
                        .Word("have", 3)
                        .Word("done", 8)
                        .Word("something", 13)
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
            file.AllStepReferences.First.Value.BoundDefinition.Should().BeNull();
        }

        [Fact]
        public async Task StepMultipleArguments()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'argument1' and 'argument2' to something

            ";
            
            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'argument1' and 'argument2' to something", 6, 21)//, step => step
                            //.Argument(ArgumentType.Text, "argument1", 41, 51)
                            //.Argument(ArgumentType.Text, "argument2", 57, 67)
            )));
        }
    }
}
