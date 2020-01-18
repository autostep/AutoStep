using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Elements;
using AutoStep.Projects;
using AutoStep.Tracing;

namespace AutoStep.Execution
{
    public class FeatureResult
    {
        public FeatureResult(Exception ex)
        {

        }
    }

    public class ScenarioResult
    {
        public ScenarioResult()
        {
        }

        public ScenarioResult(Exception ex)
        {
        }

        public ScenarioElement Scenario { get; set; }
        
        public ExampleElement? Example { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public Exception? Error { get; set; }
    }
}
