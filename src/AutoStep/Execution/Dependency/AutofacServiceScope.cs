using System;
using System.Globalization;
using Autofac;
using Autofac.Core;
using Autofac.Util;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Implements the <see cref="IServiceScope"/> for Autofac.
    /// </summary>
    /// <remarks>Inherit from the Autofac Disposable so we get the same semantics.</remarks>
    internal class AutofacServiceScope : Disposable, IServiceScope
    {
        private readonly ILifetimeScope scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacServiceScope"/> class.
        /// </summary>
        /// <param name="tag">The scope tag.</param>
        /// <param name="scope">The backing Autofac lifetime scope.</param>
        public AutofacServiceScope(string tag, ILifetimeScope scope)
        {
            this.scope = scope;
            Tag = tag;
        }

        /// <inheritdoc/>
        public string Tag { get; }

        /// <inheritdoc/>
        public TService Resolve<TService>()
            where TService : class
        {
            try
            {
                return scope.Resolve<TService>();
            }
            catch (DependencyResolutionException autofacEx)
            {
                throw new DependencyException(ExecutionText.AutofacServiceScope_DependencyResolutionError.FormatWith(typeof(TService).Name), autofacEx);
            }
        }

        /// <inheritdoc/>
        public TServiceType Resolve<TServiceType>(Type serviceType)
        {
            var obj = Resolve(serviceType);

            if (obj is TServiceType srv)
            {
                return srv;
            }
            else
            {
                throw new DependencyException(ExecutionText.AutofacServiceScope_NotAssignable.FormatWith(serviceType.Name, typeof(TServiceType).Name));
            }
        }

        /// <inheritdoc/>
        public object Resolve(Type serviceType)
        {
            try
            {
                return scope.Resolve(serviceType);
            }
            catch (DependencyResolutionException autofacEx)
            {
                throw new DependencyException(ExecutionText.AutofacServiceScope_DependencyResolutionError.FormatWith(serviceType.Name), autofacEx);
            }
        }

        /// <inheritdoc/>
        public IServiceScope BeginNewScope<TContext>(string scopeTag, TContext contextInstance)
            where TContext : TestExecutionContext
        {
            return new AutofacServiceScope(scopeTag, scope.BeginLifetimeScope(scopeTag, cfg =>
            {
                // Register the relevant context object.
                cfg.RegisterInstance(contextInstance);
            }));
        }

        /// <inheritdoc/>
        public IServiceScope BeginNewScope<TContext>(TContext contextInstance)
            where TContext : TestExecutionContext
        {
            return new AutofacServiceScope(ScopeTags.GeneralScopeTag, scope.BeginLifetimeScope(cfg =>
            {
                // Register the relevant context object.
                cfg.RegisterInstance(contextInstance);
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
