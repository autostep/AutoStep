using System;
using System.Collections.Generic;
using Autofac;
using AutoStep.Execution.Dependency;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution.Binding
{
    /// <summary>
    /// Provides a registry of all known argument binders.
    /// </summary>
    public class ArgumentBinderRegistry
    {
        private readonly Dictionary<Type, Type> binders = new Dictionary<Type, Type>();
        private readonly IArgumentBinder defaultBinder = new DefaultArgumentBinder();

        /// <summary>
        /// Register an argument binder type that provides binding for the specified argument type.
        /// </summary>
        /// <typeparam name="TBinder">The binder type.</typeparam>
        /// <param name="parameterType">The parameter type the specified IArgumentBinder can bind.</param>
        public void RegisterArgumentBinder<TBinder>(Type parameterType)
            where TBinder : IArgumentBinder
        {
            if (parameterType is null)
            {
                throw new ArgumentNullException(nameof(parameterType));
            }

            binders[parameterType] = typeof(TBinder);
        }

        /// <summary>
        /// Gets the binder for the specified type.
        /// </summary>
        /// <param name="scope">The current scope.</param>
        /// <param name="parameterType">The argument type to convert.</param>
        /// <returns>The binder.</returns>
        public IArgumentBinder GetBinderForType(ILifetimeScope scope, Type parameterType)
        {
            scope = scope.ThrowIfNull(nameof(scope));
            parameterType = parameterType.ThrowIfNull(nameof(parameterType));

            if (binders.TryGetValue(parameterType, out var binder))
            {
                return (IArgumentBinder)scope.Resolve(binder);
            }

            return defaultBinder;
        }
    }
}
