using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Strategy
{
    public interface IFeatureExecutionStrategy
    {
        Task Execute(IServiceScope threadScope, IEventPipeline events, IFeatureInfo feature);
    }
}
