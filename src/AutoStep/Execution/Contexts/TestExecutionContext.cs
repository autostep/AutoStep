using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoStep.Execution
{
    public abstract class TestExecutionContext
    {
        private ConcurrentDictionary<string, object> contextValues = new ConcurrentDictionary<string, object>();

        public TValue Get<TValue>(string name)
        {
            if (contextValues.TryGetValue(name, out object foundValue))
            {
                if (foundValue is TValue result)
                {
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Required context value '{0}' is not of the expected type '{1}'", name, foundValue.GetType().Name));
                }
            }
            else
            {
                throw new KeyNotFoundException(string.Format("Required context value '{0}' not found", name));
            }
        }

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

        public void Set<TValue>(string name, TValue value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value name cannot be null or empty.", nameof(name));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            contextValues.AddOrUpdate(name, value, (k, v) => v);
        }
    }
}
