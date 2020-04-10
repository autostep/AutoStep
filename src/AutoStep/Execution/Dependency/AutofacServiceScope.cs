using System;
using Autofac;
using Autofac.Core;
using Autofac.Util;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Implements the <see cref="IAutoStepServiceScope"/> for Autofac.
    /// </summary>
    /// <remarks>Inherit from the Autofac Disposable so we get the same semantics.</remarks>
    internal class AutofacServiceScope : Disposable, IAutoStepServiceScope
    {
        private readonly ILifetimeScope scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacServiceScope"/> class.
        /// </summary>
        /// <param name="tag">The scope tag.</param>
        /// <param name="scopeFactory">Factory method for the lifetime scope.</param>
        public AutofacServiceScope(string tag, Func<IAutoStepServiceScope, ILifetimeScope> scopeFactory)
        {
            scope = scopeFactory(this);
            Tag = tag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacServiceScope"/> class.
        /// </summary>
        /// <param name="tag">The scope tag.</param>
        /// <param name="container">The container.</param>
        public AutofacServiceScope(string tag, IContainer container)
            : this(tag, _ => container)
        {
        }

        /// <inheritdoc/>
        public string Tag { get; }

        /// <inheritdoc/>
        object? IServiceProvider.GetService(Type serviceType)
        {
            try
            {
                return scope.ResolveOptional(serviceType);
            }
            catch (DependencyResolutionException ex)
            {
                throw new DependencyException(ExecutionText.AutofacServiceScope_DependencyResolutionError, ex);
            }
        }

        /// <inheritdoc/>
        public IAutoStepServiceScope BeginNewScope<TContext>(string scopeTag, TContext contextInstance)
            where TContext : TestExecutionContext
        {
            return new AutofacServiceScope(scopeTag, newScope => scope.BeginLifetimeScope(scopeTag, cfg =>
            {
                // Register the relevant context object.
                cfg.RegisterInstance(contextInstance);

                cfg.RegisterInstance(newScope).As<IServiceProvider>();
            }));
        }

        /// <inheritdoc/>
        public IAutoStepServiceScope BeginNewScope<TContext>(TContext contextInstance)
            where TContext : TestExecutionContext
        {
            return new AutofacServiceScope(ScopeTags.GeneralScopeTag, newScope => scope.BeginLifetimeScope(cfg =>
            {
                // Register the relevant context object.
                cfg.RegisterInstance(contextInstance);

                cfg.RegisterInstance(newScope).As<IServiceProvider>();
            }));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                scope.Dispose();
            }
        }
    }
}
