using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Logging
{
    public class ContextAwareLogFactory : ILoggerFactory
    {
        private readonly ILoggerFactory externalLoggerFactory;
        private readonly IContextScopeProvider scopeProvider;

        public ContextAwareLogFactory(ILoggerFactory externalLoggerFactory, IContextScopeProvider scopeProvider)
        {
            this.externalLoggerFactory = externalLoggerFactory;
            this.scopeProvider = scopeProvider;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new InvalidOperationException("Cannot add log providers to this wrapper log factory.");
        }

        public ILogger CreateLogger(string categoryName)
        {
            // Get a logger from the external factory.
            var externalLogger = externalLoggerFactory.CreateLogger(categoryName);

            // Create our own logger with knowledge of scopes.
            return new LoggerWrapper(scopeProvider, categoryName, externalLogger);
        }

        public void Dispose()
        {
            // Do not dispose of the external logger here; we don't control its lifetime.
        }
    }
}
