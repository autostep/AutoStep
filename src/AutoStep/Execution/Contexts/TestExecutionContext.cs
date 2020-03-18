using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// Defines the base test execution context type, from which all contexts derive.
    /// </summary>
    public abstract class TestExecutionContext
    {
        private ConcurrentDictionary<string, object> contextValues = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Get a value from the context.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <returns>The value.</returns>
        public TValue Get<TValue>(string name)
        {
            if (contextValues.TryGetValue(name, out object foundValue))
            {
                if (foundValue is TValue result)
                {
                    return result;
                }

                throw new InvalidOperationException(ExecutionText.TextExecutionContext_NotExpectedType.FormatWith(name, foundValue.GetType().Name));
            }

            throw new KeyNotFoundException(ExecutionText.TextExecutionContext_NotFound.FormatWith(name));
        }

        /// <summary>
        /// Try to retrieve a value from the context.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="val">A variable to receive the value.</param>
        /// <returns>True if the value exists and is of the correct type.</returns>
        public bool TryGet<TValue>(string name, out TValue val)
        {
            if (contextValues.TryGetValue(name, out object foundValue))
            {
                if (foundValue is TValue result)
                {
                    val = result;

                    return true;
                }
            }

            val = default!;
            return false;
        }

        /// <summary>
        /// Try to retrieve a value from the context.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="valueToAddWhenNotPresent">A function that will return a new value to return.</param>
        /// <returns>True if the value exists and is of the correct type.</returns>
        public TValue GetOrAdd<TValue>(string name, Func<TValue> valueToAddWhenNotPresent)
            where TValue : notnull
        {
            valueToAddWhenNotPresent = valueToAddWhenNotPresent.ThrowIfNull(nameof(valueToAddWhenNotPresent));

            var foundValue = contextValues.GetOrAdd(name, k => valueToAddWhenNotPresent());

            if (foundValue is TValue result)
            {
                return result;
            }

            throw new InvalidOperationException(ExecutionText.TextExecutionContext_NotExpectedType.FormatWith(name, foundValue.GetType().Name));
        }

        /// <summary>
        /// Set a value in the context.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value to store.</param>
        public void Set<TValue>(string name, TValue value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(ExecutionText.TextExecutionContext_NameCannotBeEmpty, nameof(name));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            contextValues.AddOrUpdate(name, value, (k, v) => v);
        }
    }
}
