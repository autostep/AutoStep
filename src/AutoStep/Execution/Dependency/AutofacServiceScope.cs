using System;
using System.Globalization;
using Autofac;
using Autofac.Util;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Inherit from the Autofac Disposable so we get the same semantics.
    /// </summary>
    internal class AutofacServiceScope : Disposable, IServiceScope
    {
        private readonly ILifetimeScope scope;

        public string Tag { get; }

        public AutofacServiceScope(string tag, ILifetimeScope scope)
        {
            this.scope = scope;
            Tag = tag;
        }

        public TService Resolve<TService>()
        {
            try
            {
                return scope.Resolve<TService>();
            }
            catch(Autofac.Core.DependencyResolutionException autofacEx)
            {
                throw new DependencyException("Dependency Resolution Error", autofacEx);
            }
        }

        public TServiceType Resolve<TServiceType>(Type serviceType)
        {
            var obj = Resolve(serviceType);

            if (obj is TServiceType srv)
            {
                return srv;
            }
            else
            {
                // TODO: message.
                throw new DependencyException(string.Format(CultureInfo.CurrentCulture, "Cannot resolve service of type '{0}'; it is not assignable to '{1}'.", serviceType.Name, typeof(TServiceType).Name));
            }
        }

        public object Resolve(Type serviceType)
        {
            return scope.Resolve(serviceType);
        }

        public IServiceScope BeginNewScope<TContext>(string scopeTag, TContext contextInstance)
            where TContext : TestExecutionContext
        {
            return new AutofacServiceScope(scopeTag, scope.BeginLifetimeScope(scopeTag, cfg =>
            {
                // Register the relevant context object.
                cfg.RegisterInstance(contextInstance);
            }));
        }

        public IServiceScope BeginNewScope<TContext>(TContext contextInstance)
            where TContext : TestExecutionContext
        {
            return new AutofacServiceScope(ScopeTags.GeneralScopeTag, scope.BeginLifetimeScope(cfg =>
            {
                // Register the relevant context object.
                cfg.RegisterInstance(contextInstance);
            }));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                scope.Dispose();
            }
        }

    }
}
