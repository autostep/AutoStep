using System;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Logging
{
    /// <summary>
    /// Defines a logger factory that wraps an existing logger factory and creates <see cref="CapturingLogger" /> instances.
    /// </summary>
    internal class CapturingLogFactory : ILoggerFactory
    {
        private readonly ILoggerFactory externalLoggerFactory;
        private readonly IContextScopeProvider scopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturingLogFactory"/> class.
        /// </summary>
        /// <param name="externalLoggerFactory">The logger factory to retrieve concrete loggers from.</param>
        /// <param name="scopeProvider">The context scope provider.</param>
        public CapturingLogFactory(ILoggerFactory externalLoggerFactory, IContextScopeProvider scopeProvider)
        {
            this.externalLoggerFactory = externalLoggerFactory;
            this.scopeProvider = scopeProvider;
        }

        /// <inheritdoc/>
        public void AddProvider(ILoggerProvider provider)
        {
            throw new InvalidOperationException(LoggingMessages.CannotAddProviders);
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            // Get a logger from the external factory.
            var externalLogger = externalLoggerFactory.CreateLogger(categoryName);

            // Create our own logger with knowledge of scopes.
            return new CapturingLogger(scopeProvider, categoryName, externalLogger);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not dispose of the external logger here; we don't control its lifetime.
        }
    }
}
