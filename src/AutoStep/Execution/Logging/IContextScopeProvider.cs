using System;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Logging
{
    /// <summary>
    /// Defines a service for maintaining the concept of a 'current' <see cref="TestExecutionContext" />.
    /// </summary>
    public interface IContextScopeProvider
    {
        /// <summary>
        /// Gets the current in-scope execution context.
        /// </summary>
        TestExecutionContext? Current { get; }

        /// <summary>
        /// Enters a new context scope. Dispose of the result of this method to exit the scope.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A disposable that will exit the scope when disposed.</returns>
        IDisposable EnterContextScope(TestExecutionContext context);
    }
}
