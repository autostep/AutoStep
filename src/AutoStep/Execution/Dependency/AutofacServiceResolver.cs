using System;
using Autofac;
using Autofac.Util;

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
                throw new DependencyException();
            }
        }

        public object Resolve(Type serviceType)
        {
            return scope.Resolve(serviceType);
        }

        public IServiceScope BeginNewScope<TContext>(string scopeTag, TContext contextInstance)
            where TContext : ExecutionContext
        {
            return new AutofacServiceScope(scopeTag, scope.BeginLifetimeScope(scopeTag, cfg =>
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
