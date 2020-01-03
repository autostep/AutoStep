using System.Collections.Generic;

namespace AutoStep.Core.Matching
{
    public class StepMatchingPart
    {
        public StepMatchingPart(string textContent)
        {
            TextContent = textContent ?? throw new System.ArgumentNullException(nameof(textContent));
        }

        public StepMatchingPart(ArgumentType argument)
        {
            ArgumentType = argument;
        }

        public string TextContent { get; }

        public bool IsText => TextContent is string;

        public bool IsArgument => ArgumentType.HasValue;

        public ArgumentType? ArgumentType { get; set; }
    }
}
