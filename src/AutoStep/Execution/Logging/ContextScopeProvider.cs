using System;
using System.Threading;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Logging
{
    public interface IContextScopeProvider
    {
        TestExecutionContext? Current { get; }

        IDisposable EnterContextScope(TestExecutionContext context);
    }

    internal class ContextScopeProvider : IContextScopeProvider
    {
        private AsyncLocal<Scope?> activeScope = new AsyncLocal<Scope?>();

        public ContextScopeProvider()
        {
            activeScope.Value = null;
        }

        public TestExecutionContext? Current => activeScope.Value?.Context;

        public IDisposable EnterContextScope(TestExecutionContext context)
        {
            var newScope = new Scope(this, context, activeScope.Value);
            activeScope.Value = newScope;
            return newScope;
        }

        private class Scope : IDisposable
        {
            private readonly ContextScopeProvider scopeProvider;
            private readonly Scope? parent;
            private bool disposed;

            public Scope(ContextScopeProvider scopeProvider, TestExecutionContext context, Scope? parent)
            {
                this.scopeProvider = scopeProvider;
                this.parent = parent;

                Context = context;
            }

            public TestExecutionContext Context { get; }

            public void Dispose()
            {
                if (!disposed)
                {
                    scopeProvider.activeScope.Value = parent;
                    disposed = true;
                }
            }
        }
    }
}
