using System;
using AutoStep.Compiler;
using AutoStep.Elements;
using AutoStep.Execution.Control;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Strategy;

namespace AutoStep.Execution
{
    public class StepExecutionArgs
    {
        internal StepExecutionArgs(
            IServiceScope stepScope,
            StepContext context,
            StepReferenceElement reference,
            VariableSet variables,
            StepReferenceBinding binding,
            EventPipeline events,
            IExecutionStateManager executionManager,
            IStepCollectionExecutionStrategy collectionExecutionStrategy)
        {
            Scope = stepScope;
            Context = context;
            Step = reference;
            Variables = variables;
            Binding = binding;
            Events = events;
            ExecutionManager = executionManager;
            CollectionExecutionStrategy = collectionExecutionStrategy;
        }

        public IServiceScope Scope { get; }

        public StepContext Context { get; }
        public StepReferenceElement Step { get; }
        public VariableSet Variables { get; }
        
        public StepReferenceBinding Binding { get; }

        internal EventPipeline Events { get; }

        internal IExecutionStateManager ExecutionManager { get; }
        internal IStepCollectionExecutionStrategy CollectionExecutionStrategy { get; }
    }
}
