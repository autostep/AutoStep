using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Execution.Dependency;
using AutoStep.Projects;
using AutoStep.Tracing;

namespace AutoStep.Execution
{

    public abstract class ExecutionContext : IDisposable
    {
        private bool isDisposed = false; // To detect redundant calls
        private readonly IServiceScope scope;

        protected ExecutionContext(IServiceScope scope)
        {
            this.scope = scope;
        }

        ~ExecutionContext()
        {
            Dispose(false);
        }

        public IServiceScope Scope => isDisposed ? throw new ObjectDisposedException(nameof(ExecutionContext)) : scope;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                Scope.Dispose();
                isDisposed = true;
            }
        }

    }
}
