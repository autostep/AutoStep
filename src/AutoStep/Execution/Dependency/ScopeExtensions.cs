using Autofac;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Lifetime scope extension methods.
    /// </summary>
    public static class ScopeExtensions
    {
        /// <summary>
        /// Begin a new Autofac lifetime scope, with the specified context instance registered within it.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        /// <param name="scope">The current scope.</param>
        /// <param name="scopeTag">The tag for the new scope (typically from <see cref="ScopeTags"/>).</param>
        /// <param name="contextInstance">The context instance to register.</param>
        /// <returns>The new lifetime scope.</returns>
        public static ILifetimeScope BeginContextScope<TContext>(this ILifetimeScope scope, string scopeTag, TContext contextInstance)
            where TContext : TestExecutionContext
        {
            if (scope is null)
            {
                throw new System.ArgumentNullException(nameof(scope));
            }

            if (scopeTag is null)
            {
                throw new System.ArgumentNullException(nameof(scopeTag));
            }

            return scope.BeginLifetimeScope(scopeTag, cfg =>
            {
                cfg.RegisterInstance(contextInstance);
            });
        }
    }
}
