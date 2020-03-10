using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Maintains a set of variables in a call chain during compilation.
    /// </summary>
    public class CallChainCompileTimeVariables
    {
        private readonly Dictionary<string, CallChainVariable> activeVariables = new Dictionary<string, CallChainVariable>();

        /// <summary>
        /// Validate a variable reference.
        /// </summary>
        /// <param name="sourceName">The source file containing the element to validate.</param>
        /// <param name="variableRef">The variable reference.</param>
        /// <returns>A compilation message, if there are any problems.</returns>
        public virtual CompilerMessage? ValidateVariable(string? sourceName, VariableRefMethodArgumentElement variableRef)
        {
            if (variableRef is null)
            {
                throw new System.ArgumentNullException(nameof(variableRef));
            }

            if (activeVariables.TryGetValue(variableRef.VariableName, out var _))
            {
                // All good.
                return null;
            }

            // Variable does not exist.
            // Error.
            return CompilerMessageFactory.Create(sourceName, variableRef, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, variableRef.VariableName);
        }

        /// <summary>
        /// Validate a variable array reference.
        /// </summary>
        /// <param name="sourceName">The source file containing the element to validate.</param>
        /// <param name="variableArrayRef">The variable array reference.</param>
        /// <returns>A compilation message, if there are any problems.</returns>
        public virtual CompilerMessage? ValidateVariable(string? sourceName, VariableArrayRefMethodArgument variableArrayRef)
        {
            if (variableArrayRef is null)
            {
                throw new System.ArgumentNullException(nameof(variableArrayRef));
            }

            if (variableArrayRef.VariableName is null)
            {
                // Should not be possible to get a blank variable name.
                throw new LanguageEngineAssertException();
            }

            if (activeVariables.TryGetValue(variableArrayRef.VariableName, out var foundVariable))
            {
                if (!foundVariable.IsArray)
                {
                    // Trying to use a non-array variable as an array.
                    // Error.
                    return CompilerMessageFactory.Create(sourceName, variableArrayRef, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotAnArray, variableArrayRef.VariableName);
                }

                // All good.
                return null;
            }

            // Variable does not exist.
            // Error.
            return CompilerMessageFactory.Create(sourceName, variableArrayRef, CompilerMessageLevel.Error, CompilerMessageCode.InteractionVariableNotDefined, variableArrayRef.VariableName);
        }

        /// <summary>
        /// Sets a variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="isArray">Indicates whether this is an array type.</param>
        public void SetVariable(string name, bool isArray)
        {
            activeVariables[name] = new CallChainVariable(name, null, isArray);
        }

        private struct CallChainVariable
        {
            public CallChainVariable(string name, string? knownTypeHint, bool isArray)
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
