using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language.Interaction;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// Base class for all interaction methods (i.e. a call that can form part of a call chain attached to a step definition or method declaration).
    /// </summary>
    public abstract class InteractionMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionMethod"/> class.
        /// </summary>
        /// <param name="name">The method name.</param>
        protected InteractionMethod(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the method name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the number of arguments.
        /// </summary>
        public abstract int ArgumentCount { get; }

        /// <summary>
        /// Invoked at compilation time to simulate a call of the method. A derived implementation
        /// can implement this method to add additional call chain variables (to <paramref name="variables"/>).
        /// based on the known arguments.
        /// </summary>
        /// <param name="arguments">The set of arguments to the method.</param>
        /// <param name="variables">The set of variables currently applied to the call chain.</param>
        public virtual void CompilerMethodCall(IReadOnlyList<MethodArgumentElement> arguments, CallChainCompileTimeVariables variables)
        {
        }

        /// <summary>
        /// Implemented by a deriving type to invoke the method.
        /// </summary>
        /// <param name="scope">The current scope.</param>
        /// <param name="context">The current method context (containing the current variables and chain value, among other things).</param>
        /// <param name="arguments">The determined set of arguments to the method.</param>
        /// <returns>A completion task for when the method has completed.</returns>
        public virtual ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object?[] arguments)
        {
            throw new NotImplementedException("Registered Interaction Method '{0}' has not been implemented".FormatWith(Name));
        }

        /// <summary>
        /// Implemented by a deriving type to invoke the method.
        /// </summary>
        /// <param name="scope">The current scope.</param>
        /// <param name="context">The current method context (containing the current variables and chain value, among other things).</param>
        /// <param name="arguments">The determined set of arguments to the method.</param>
        /// <param name="methods">The method table currently in scope.</param>
        /// <param name="callStack">The current call stack.</param>
        /// <returns>A completion task for when the method has completed.</returns>
        public virtual ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object?[] arguments, MethodTable methods, Stack<MethodContext> callStack)
        {
            return InvokeAsync(scope, context, arguments);
        }
    }
}
