using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Binding
{

    public class ArgumentBinderRegistry
    {
        private Dictionary<Type, Type> binders = new Dictionary<Type, Type>();

        private IArgumentBinder defaultBinder = new DefaultArgumentBinder();

        public void RegisterArgumentBinder<TBinder>(Type argumentType)
            where TBinder : IArgumentBinder
        {
            if (argumentType is null)
            {
                throw new ArgumentNullException(nameof(argumentType));
            }

            binders[argumentType] = typeof(TBinder);
        }

        public IArgumentBinder GetBinderForType(IServiceScope scope, Type parameterType)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (parameterType is null)
            {
                throw new ArgumentNullException(nameof(parameterType));
            }

            if (binders.TryGetValue(parameterType, out var binder))
            {
                return scope.Resolve<IArgumentBinder>(binder);
            }

            return defaultBinder;
        }
    }
}
