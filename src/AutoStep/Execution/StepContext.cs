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

    public class StepContext : ErrorCapturingContext
    {
        internal StepContext(ExecutionContext parentContext, int stepIndex, StepReferenceElement step, VariableSet variables)
            : base(parentContext.Scope.BeginNewScope(ScopeTags.StepTag))
        {
            ParentContext = parentContext;
            StepIndex = stepIndex;
            Step = step;
            Variables = variables;
        }

        public ExecutionContext ParentContext { get; }
        public int StepIndex { get; }
        public StepReferenceElement Step { get; }

        public VariableSet Variables { get; }
    }
}
