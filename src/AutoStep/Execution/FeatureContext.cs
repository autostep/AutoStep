using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Projects;
using AutoStep.Tracing;

namespace AutoStep.Execution
{

    public class FeatureContext : ExecutionContext
    {
        internal FeatureContext(FeatureElement feature, ThreadContext threadContext)
            : base(threadContext.Scope.BeginNewScope(ScopeTags.FeatureTag))
        {
            Feature = feature;
        }

        public FeatureElement Feature { get; }
    }
}
