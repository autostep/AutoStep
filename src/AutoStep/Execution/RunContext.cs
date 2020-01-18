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
using AutoStep.Execution.Dependency;
using AutoStep.Projects;
using AutoStep.Tracing;

namespace AutoStep.Execution
{

    public class RunContext : ExecutionContext
    {
        internal RunContext(IServiceScope scope) : base(scope)
        {
        }
    }
}
