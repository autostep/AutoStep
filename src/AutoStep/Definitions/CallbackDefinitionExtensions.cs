using System;
using System.Threading.Tasks;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Defines extension methods for registering callback variants.
    /// </summary>
    public static class CallbackDefinitionExtensions
    {
        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public static CallbackDefinitionSource Given(this CallbackDefinitionSource source, string declaration, Action callback)
        {
            callback = callback.ThrowIfNull(nameof(callback));
            source = source.ThrowIfNull(nameof(source));

            source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, StepType.Given, declaration));

            return source;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <typeparam name="T1">Argument type 1.</typeparam>
        /// <param name="source">The source instance.</param>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public static CallbackDefinitionSource Given<T1>(this CallbackDefinitionSource source, string declaration, Action<T1> callback)
        {
            callback = callback.ThrowIfNull(nameof(callback));
            source = source.ThrowIfNull(nameof(source));

            source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, StepType.Given, declaration));

            return source;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public static CallbackDefinitionSource Given(this CallbackDefinitionSource source, string declaration, Func<ValueTask> callback)
        {
            callback = callback.ThrowIfNull(nameof(callback));
            source = source.ThrowIfNull(nameof(source));

            source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, StepType.Given, declaration));

            return source;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <typeparam name="T1">Argument type 1.</typeparam>
        /// <param name="source">The callback source.</param>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public static CallbackDefinitionSource Given<T1>(this CallbackDefinitionSource source, string declaration, Func<T1, ValueTask> callback)
        {
            callback = callback.ThrowIfNull(nameof(callback));
            source = source.ThrowIfNull(nameof(source));

            source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, StepType.Given, declaration));

            return source;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public static CallbackDefinitionSource When(this CallbackDefinitionSource source, string declaration, Func<ValueTask> callback)
        {
            callback = callback.ThrowIfNull(nameof(callback));
            source = source.ThrowIfNull(nameof(source));

            source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, StepType.When, declaration));

            return source;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public static CallbackDefinitionSource When(this CallbackDefinitionSource source, string declaration, Action callback)
        {
            callback = callback.ThrowIfNull(nameof(callback));
            source = source.ThrowIfNull(nameof(source));

            source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, StepType.When, declaration));

            return source;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public static CallbackDefinitionSource Then(this CallbackDefinitionSource source, string declaration, Func<ValueTask> callback)
        {
            callback = callback.ThrowIfNull(nameof(callback));
            source = source.ThrowIfNull(nameof(source));

            source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, StepType.Then, declaration));

            return source;
        }

        /// <summary>
        /// Register a 'Given' step definition, with a callback to be invoked when that step is used in a test.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="declaration">The step declaration.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>Itself.</returns>
        public static CallbackDefinitionSource Then(this CallbackDefinitionSource source, string declaration, Action callback)
        {
            callback = callback.ThrowIfNull(nameof(callback));
            source = source.ThrowIfNull(nameof(source));

            source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, StepType.Then, declaration));

            return source;
        }
    }
}
