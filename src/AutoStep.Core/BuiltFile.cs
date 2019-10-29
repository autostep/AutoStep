using System.Collections.Generic;

namespace AutoStep.Core
{
    public class BuiltElement
    {
        public string SourceLine { get; set; }

        public string SourceColumn { get; set; }
    }

    public class StepReference : BuiltElement
    {
    }

    public class DefinedStepReference : StepReference
    {
    }

    public class CompiledStepReference : StepReference
    {
    }

    public class UnknownStepReference : StepReference
    {
    }

    public class BuiltStepCollection : BuiltElement
    {
        
    }

    public class BuiltStepDefinition : BuiltStepCollection
    {

    }

    public class BuiltScenario : BuiltStepCollection
    {

    }

    public class BuiltFeature : BuiltElement
    {

    }

    public class BuiltContent
    {
        public IEnumerable<BuiltStepDefinition> Steps { get; set; }

        public BuiltFeature Feature { get; set; }
    }

    public class BuiltFile : BuiltContent
    {
        public string SourceName { get; set; }
    }
}
