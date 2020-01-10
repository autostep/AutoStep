using System.Collections.Generic;
using System.Linq;
using AutoStep.Definitions;

namespace AutoStep.Tests.Utils
{
    public class TestStepDefinitionSource : IStepDefinitionSource
    {
        public readonly static TestStepDefinitionSource Blank = new TestStepDefinitionSource();

        private readonly List<StepDefinition> defs;

        public TestStepDefinitionSource(string uid, params StepDefinition[] defs)
            : this(defs)
        {
            Uid = uid;
        }

        public TestStepDefinitionSource(params StepDefinition[] defs)
        {
            this.defs = defs.ToList();
        }

        public string Uid { get; } = "test";

        public string Name => "Test";

        public void AddStepDefinition(StepType type, string declaration)
        {
            defs.Add(new LocalStepDef(this, type, declaration));
        }

        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            return defs;
        }

        private class LocalStepDef : StepDefinition
        {
            public LocalStepDef(IStepDefinitionSource source, StepType type, string declaration) : base(source, type, declaration)
            {
            }

            public override bool IsSameDefinition(StepDefinition def)
            {
                return def.Declaration == Declaration && def.Source == Source;
            }
        }
    }
}
