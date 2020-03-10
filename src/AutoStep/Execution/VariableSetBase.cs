using System;
using System.Collections.Generic;

namespace AutoStep.Execution
{
    public class VariableSetBase<TValue>
        where TValue : class
    {
        private readonly Dictionary<string, TValue?> valuesStore = new Dictionary<string, TValue?>();
        private readonly bool isReadOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSetBase{TValue}"/> class.
        /// </summary>
        /// <param name="isReadOnly">Set to true to prevent modification of this set after it has been created.</param>
        public VariableSetBase(bool isReadOnly = false)
        {
            this.isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Gets the set of known variables.
        /// </summary>
        public IReadOnlyDictionary<string, TValue?> Variables => valuesStore;

        /// <summary>
        /// Get the value of a variable. If the variable is not set, an empty string will be returned.
        /// </summary>
        /// <param name="variableName">The name of a variable.</param>
        /// <returns>The value of the variable.</returns>
        public TValue? Get(string variableName)
        {
            if (valuesStore.TryGetValue(variableName, out var varValue))
            {
                return varValue;
            }

            return GetDefault();
        }

        /// <summary>
        /// Set a value in the variable set.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value to set.</param>
        public void Set(string name, TValue? value)
        {
            if (isReadOnly)
            {
                throw new InvalidOperationException(ExecutionText.VariableSet_ReadOnly);
            }

            valuesStore[name] = value;
        }

        protected virtual TValue? GetDefault()
        {
            return default;
        }
    }
}
