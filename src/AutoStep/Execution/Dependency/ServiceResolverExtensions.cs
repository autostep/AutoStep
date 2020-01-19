using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Execution.Dependency
{
    public static class ServiceResolverExtensions
    {
        public static ThreadContext ThreadContext(this IServiceScope sc)
        {
            if (sc is null)
            {
                throw new ArgumentNullException(nameof(sc));
            }

            return sc.Resolve<ThreadContext>();
        }
    }
}
