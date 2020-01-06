using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Compiler.Tests.Builders;
using AutoStep.Compiler.Tests.Utils;
using AutoStep.Core;
using AutoStep.Core.Sources;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests.Linker
{
    public class AddSourceTests : CompilerTestBase
    {
        public AddSourceTests(ITestOutputHelper output)
            : base(output)
        {
        }

        private class TestDef : StepDefinition
        {
            public TestDef(StepType type, string declaration) : base(type, declaration)
            {
            }
        }


        private class TestSource : IStepDefinitionSource
        {
            private readonly TestDef[] defs;

            public TestSource(params TestDef[] defs)
            {
                this.defs = defs;
            }

            public string Uid => "uid";

            public string Name => "name";

            public DateTime GetLastModifyTime()
            {
                return DateTime.MinValue;
            }

            public IEnumerable<StepDefinition> GetStepDefinitions()
            {
                return defs;
            }
        }

        [Fact]
        public void SourceLoadPopulatesDefinition()
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);

            var linker = new AutoStepLinker(compiler);

            var def = new TestDef(StepType.Given, "I have done something");

            linker.AddStepDefinitionSource(new TestSource(def));

            def.Definition.Should().NotBeNull();
        }
    }
}
