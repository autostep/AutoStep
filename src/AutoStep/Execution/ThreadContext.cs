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

    public class ThreadContext : ExecutionContext
    {
        public int TestThreadId { get; }

        public RunContext RunContext { get; }

        internal Stack<StepDefinition> DefinitionStack { get; } = new Stack<StepDefinition>();

        internal ThreadContext(int testThreadId, RunContext runCtxt)
            : base(runCtxt.Scope.BeginNewScope(ScopeTags.ThreadTag))
        {
            TestThreadId = testThreadId;
            RunContext = runCtxt;
        }
    }
}
