using System;
using Autofac;

namespace AutoStep.Execution
{
    internal class AutofacServiceScope : IServiceScope
    {
        private readonly ILifetimeScope scope;

        public AutofacServiceScope(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public TService Resolve<TService>()
        {
            return scope.Resolve<TService>();
        }

        public TServiceType Resolve<TServiceType>(Type serviceType)
        {
            var obj = scope.Resolve(serviceType);

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

        public IServiceScope BeginNewScope(string scopeTag)
        {
            return new AutofacServiceScope(scope.BeginLifetimeScope(scopeTag, cfg =>
            {
                cfg.Register<IServiceScope>(ctxt => new AutofacServiceScope(ctxt.Resolve<ILifetimeScope>())).InstancePerDependency();
            }));
        }

        public void Dispose()
        {
            scope.Dispose();
        }

    }
}
