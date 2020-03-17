using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Interaction
{
    /// <summary>
    /// Defines the context for a method invocation. Typically only lives for the lifetime of a single method call.
    /// </summary>
    public class MethodContext : TestExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodContext"/> class.
        /// </summary>
        public MethodContext()
        {
            Variables = new InteractionVariables();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodContext"/> class.
        /// </summary>
        /// <param name="call">The method call that is invoking the method.</param>
        /// <param name="methodDef">The method definition actually being invoked.</param>
        /// <param name="variables">The set of variables available inside the method.</param>
        public MethodContext(MethodCallElement call, InteractionMethod methodDef, InteractionVariables variables)
        {
            MethodCall = call;
            MethodDefinition = methodDef;
            Variables = variables;
        }

        /// <summary>
        /// Gets or sets the 'chain value'. This value will be passed throughout the call stack of a method chain expression,
        /// so can be used to maintain sets of values, and 'return' values to be consumed by the next stage in the pipeline.
        /// </summary>
        public object? ChainValue { get; set; }

        /// <summary>
        /// Gets the method definition currently being invoked. Will be null if the current context is from a step definition.
        /// </summary>
        public InteractionMethod? MethodDefinition { get; }

        /// <summary>
        /// Gets the method call currently being invoked. Will be null if the current context is from a step definition.
        /// </summary>
        public MethodCallElement? MethodCall { get; }

        /// <summary>
        /// Gets the set of variables available to the method. Modifying variables in this set will
        /// make the changes available to the current method chain, but not to any callers.
        /// Use <see cref="ChainValue"/> to pass values back to the caller.
        /// </summary>
        public InteractionVariables Variables { get; }
    }
}
