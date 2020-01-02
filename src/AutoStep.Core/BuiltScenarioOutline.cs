using System.Collections.Generic;

namespace AutoStep.Core
{
    /// <summary>
    /// A built scenario outline.
    /// </summary>
    public class BuiltScenarioOutline : BuiltScenario
    {
        private readonly List<BuiltExample> examples = new List<BuiltExample>();
        private HashSet<string> allExampleVariables = new HashSet<string>();

        /// <summary>
        /// Gets the contained example blocks.
        /// </summary>
        public IReadOnlyList<BuiltExample> Examples => examples;

        /// <summary>
        /// Adds an example to the scenario outline.
        /// </summary>
        /// <param name="example">The example to add.</param>
        public void AddExample(BuiltExample example)
        {
            if (example is null)
            {
                throw new System.ArgumentNullException(nameof(example));
            }

            examples.Add(example);

            // Add to the set of all headers (for faster variable checking).
            foreach (var header in example.Table.Header.Headers)
            {
                if (header.HeaderName is object)
                {
                    allExampleVariables.Add(header.HeaderName);
                }
            }
        }

        /// <summary>
        /// Checks whether any of the example tables in the scenario outline contains
        /// the specified insertion variable name.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <returns>True if the variable is available, false otherwise.</returns>
        public bool ContainsVariable(string variableName)
        {
            return allExampleVariables.Contains(variableName);
        }
    }
}
