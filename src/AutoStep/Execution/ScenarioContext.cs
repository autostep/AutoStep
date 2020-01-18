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

    public class ScenarioContext : ErrorCapturingContext
    {
        private ExampleElement? example;

        internal ScenarioContext(FeatureContext featureContext, ScenarioElement scenario, VariableSet example)
            : base(featureContext.Scope.BeginNewScope(ScopeTags.ScenarioTag))
        {
            FeatureContext = featureContext;
            Scenario = scenario;
            Variables = example;
        }

        public FeatureContext FeatureContext { get; }

        public ScenarioElement Scenario { get; }

        public VariableSet Variables { get; }
    }
}
