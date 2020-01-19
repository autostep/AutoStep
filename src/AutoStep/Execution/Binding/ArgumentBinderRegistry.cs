using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Execution.Binding
{
    internal class DefaultArgumentBinder : IArgumentBinder
    {

    }

    public class ArgumentBinderRegistry
    {
        private Dictionary<Type, Type> binders = new Dictionary<Type, Type>();

        private IArgumentBinder defaultBinder = new DefaultArgumentBinder();

        public void RegisterArgumentBinder<TBinder>(Type argumentType)
        {

        }
    }
}
