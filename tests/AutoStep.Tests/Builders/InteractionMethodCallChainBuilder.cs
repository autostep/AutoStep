using System;
using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    internal class InteractionMethodCallChainBuilder<TMethodCallSource> : BaseBuilder<TMethodCallSource>
        where TMethodCallSource : ICallChainSource
    {
        public InteractionMethodCallChainBuilder(TMethodCallSource methodSource)
        {
            Built = methodSource;
        }

        public InteractionMethodCallChainBuilder<TMethodCallSource> Call(string name, int startLine, int startCol, int endLine, int endCol, Action<InteractionMethodArgumentSetBuilder> cfg = null)
        {
            var methodCall = new MethodCallElement(name)
            {
                SourceLine = startLine,
                StartColumn = startCol,
                EndColumn = endCol,
                EndLine = endLine,
            };

            if(cfg is object)
            {
                var methodArgumentSetBuilder = new InteractionMethodArgumentSetBuilder(methodCall);

                cfg(methodArgumentSetBuilder);
            }

            Built.Calls.Add(methodCall);

            return this;
        }
    }
}
