using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;

namespace AutoStep.Execution.Strategy
{
    public interface IRunExecutionStrategy
    {
        Task Execute(IServiceScope runScope, RunContext runContext, FeatureExecutionSet executionSet, IEventPipeline eventPipeline);
    }
}
