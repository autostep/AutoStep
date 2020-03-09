using System.Collections.Generic;
using Antlr4.Runtime;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    public class InteractionMethodChainVariables
    {
        private readonly Dictionary<string, MethodChainVariable> activeVariables = new Dictionary<string, MethodChainVariable>();

        public virtual CompilerMessage? ValidateVariable(string? sourceName, VariableRefMethodArgumentElement nameRefToken)
        {
            if (activeVariables.TryGetValue(nameRefToken.VariableName, out var foundVariable))
            {
                // All good.
                return null;
            }

            // Variable does not exist.
            // Error.
            return CompilerMessageFactory.Create(sourceName, nameRefToken, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, nameRefToken.VariableName);
        }

        public virtual CompilerMessage? ValidateVariable(string? sourceName, VariableArrayRefMethodArgument nameRefToken)
        {
            if (activeVariables.TryGetValue(nameRefToken.VariableName, out var foundVariable))
            {
                if (!foundVariable.IsArray)
                {
                    // Trying to use a non-array variable as an array.
                    // Error.
                    return CompilerMessageFactory.Create(sourceName, nameRefToken, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotAnArray, nameRefToken.VariableName);
                }

                // All good.
                return null;
            }

            // Variable does not exist.
            // Error.
            return CompilerMessageFactory.Create(sourceName, nameRefToken, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, nameRefToken.VariableName);
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
