using System.Collections.Generic;

namespace AutoStep.Core
{

    public class BuiltScenarioOutline : BuiltScenario
    {
        private readonly List<BuiltExample> examples = new List<BuiltExample>();
        private HashSet<string> allExampleVariables = new HashSet<string>();

        public IReadOnlyList<BuiltExample> Examples => examples;

        public void AddExample(BuiltExample example)
        {
            if (example is null)
            {
                throw new System.ArgumentNullException(nameof(example));
            }

            examples.Add(example);

            foreach (var header in example.Table.Header.Headers)
            {
                if (header.HeaderName is object)
                {
                    allExampleVariables.Add(header.HeaderName);
                }
            }
        }

        public bool ContainsVariable(string variableName)
        {
            return allExampleVariables.Contains(variableName);
        }
    }
}
