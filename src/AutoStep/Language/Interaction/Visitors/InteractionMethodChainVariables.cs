using System.Collections.Generic;
using Antlr4.Runtime;

namespace AutoStep.Language.Interaction.Visitors
{
    internal class InteractionMethodChainVariables
    {
        private readonly Dictionary<string, MethodChainVariable> activeVariables = new Dictionary<string, MethodChainVariable>();
        private readonly string? sourceName;

        public InteractionMethodChainVariables(string? sourceName)
        {
            this.sourceName = sourceName;
        }

        public CompilerMessage? ValidateVariable(ParserRuleContext nameRefToken, string variableName, bool isArrayRef)
        {
            if (activeVariables.TryGetValue(variableName, out var foundVariable))
            {
                if (isArrayRef && !foundVariable.IsArray)
                {
                    // Trying to use a non-array variable as an array.
                    // Error.
                    return CompilerMessageFactory.Create(sourceName, nameRefToken, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotAnArray, variableName);
                }

                // All good.
                return null;
            }

            // Variable does not exist.
            // Error.
            return CompilerMessageFactory.Create(sourceName, nameRefToken, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, variableName);
        }

        public void SetVariable(string name, bool isArray)
        {
            activeVariables[name] = new MethodChainVariable(name, null, isArray);
        }

        private struct MethodChainVariable
        {
            public MethodChainVariable(string name, string? knownTypeHint, bool isArray)
            {
                Name = name;
                KnownTypeHint = knownTypeHint;
                IsArray = isArray;
            }

            public string Name { get; }

            public string? KnownTypeHint { get; }

            public bool IsArray { get; }
        }
    }
}
