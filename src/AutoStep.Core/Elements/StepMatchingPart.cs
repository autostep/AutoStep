using System.Collections.Generic;

namespace AutoStep.Core.Elements
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

        public ArgumentType? ArgumentType { get; set; }
    }
}
