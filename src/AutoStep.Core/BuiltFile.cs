using System.Collections.Generic;

namespace AutoStep.Core
{
    public enum StepType
    {
        Given,
        When,
        Then,
        And
    }

    public class BuiltElement
    {
        public int SourceLine { get; set; }

        public int SourceColumn { get; set; }
    }

    public class StepReference : BuiltElement
    {
        public StepType Type { get; set; }

        public string Text { get; set; }
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

    public class BuiltScenario : BuiltStepCollection, IAnnotatable
    {
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        public string Name { get; set; }

        public string Description { get; set; }

        public List<StepReference> Steps { get; } = new List<StepReference>();
    }

    public interface IAnnotatable
    {
        List<AnnotationElement> Annotations { get; }
    }

    public class AnnotationElement : BuiltElement
    {

    }

    public class TagElement : AnnotationElement
    {
        public string Tag { get; set; }
    }

    public class OptionElement : AnnotationElement
    {
        public string Name { get; set; }
    }

    public class BuiltFeature : BuiltElement, IAnnotatable
    {
        public List<AnnotationElement> Annotations { get; } = new List<AnnotationElement>();

        public string Name { get; set; }

        public string Description { get; set; }

        public List<StepReference> Background { get; } = new List<StepReference>();

        public List<BuiltScenario> Scenarios { get; } = new List<BuiltScenario>();
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
