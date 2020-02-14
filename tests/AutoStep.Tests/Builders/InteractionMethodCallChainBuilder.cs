using System;
using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    public class InteractionMethodCallChainBuilder<TMethodCallSource> : BaseBuilder<TMethodCallSource>
        where TMethodCallSource : IMethodCallSource
    {
        public InteractionMethodCallChainBuilder(TMethodCallSource methodSource)
        {
            Built = methodSource;
        }

        public InteractionMethodCallChainBuilder<TMethodCallSource> Call(string name, int startLine, int startCol, int endLine, int endCol, Action<InteractionMethodArgumentSetBuilder> cfg = null)
        {
            var methodCall = new MethodCallElement
            {
                MethodName = name,
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

            Built.MethodCallChain.Add(methodCall);

            return this;
        }
    }
}
