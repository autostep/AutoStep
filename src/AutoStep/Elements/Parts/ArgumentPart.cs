using AutoStep.Compiler;

namespace AutoStep.Elements.Parts
{
    public class ArgumentPart : DefinitionContentPart
    {
        public string? Name { get; set; }

        public ArgumentType? TypeHint { get; set; }

        public override StepReferenceMatchResult DoStepReferenceMatch(ContentPart referencePart)
        {
            // Arguments always match in the tree, but might get additional errors when GetBindingMessage is called.
            var matches = new StepReferenceMatchResult(1, true);

            return matches;
        }

        public CompilerMessage? GetBindingMessage(ContentPart referencePart)
        {
            // Ok, so, does a step reference match? A step reference will 'match' any other part, but then we are going to apply some extra
            // logic and see whether the value fits.
            if (referencePart is VariablePart var)
            {
                // It's a match, a variable can be anything (late-bound), so we will say it matches.

            }
            else if (referencePart is WordPart word)
            {
                // Text will match, but let's look at the type of the argument.

            }

            // TODO
            return null;
        }

        public override bool IsDefinitionPartMatch(DefinitionContentPart part)
        {
            return part is ArgumentPart otherArg &&
                   Name == otherArg.Name &&
                   TypeHint == otherArg.TypeHint;
        }
    }

}
