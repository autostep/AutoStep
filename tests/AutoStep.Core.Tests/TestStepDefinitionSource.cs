using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Core.Sources;

namespace AutoStep.Core.Tests
{
    public class TestStepDefinitionSource : IStepDefinitionSource
    {
        public readonly static TestStepDefinitionSource Blank = new TestStepDefinitionSource();

        private readonly StepDefinition[] defs;

        public TestStepDefinitionSource(params StepDefinition[] defs)
        {
            this.defs = defs;
        }

        public string Uid => "test";

        public string Name => "Test";

        public DateTime GetLastModifyTime()
        {
            return DateTime.MinValue;
        }

        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            return defs;
        }
    }
}
