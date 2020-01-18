using System;
using AutoStep.Compiler;
using AutoStep.Elements;
using AutoStep.Execution.Control;
using AutoStep.Execution.Strategy;

namespace AutoStep.Execution
{
    public class StepExecutionArgs
    {
        internal StepExecutionArgs(
            StepContext context,
            StepReferenceElement reference,
            VariableSet variables,
            StepReferenceBinding binding,
            EventManager events,
            IExecutionStateManager executionManager,
            IStepCollectionExecutionStrategy collectionExecutionStrategy)
        {
            Context = context;
            Step = reference;
            Variables = variables;
            Binding = binding;
            Events = events;
            ExecutionManager = executionManager;
            CollectionExecutionStrategy = collectionExecutionStrategy;
        }

        public StepContext Context { get; }
        public StepReferenceElement Step { get; }
        public VariableSet Variables { get; }
        
        public StepReferenceBinding Binding { get; }

        internal EventManager Events { get; }

        internal IExecutionStateManager ExecutionManager { get; }
        internal IStepCollectionExecutionStrategy CollectionExecutionStrategy { get; }
    }
}
