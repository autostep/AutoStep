using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Defines an interface for a service that can validate a call chain during compilation.
    /// </summary>
    internal interface ICallChainValidator
    {
        /// <summary>
        /// Validate a call chain, checking for variable and method references. Adds any appropriate messages to the provided set.
        /// </summary>
        /// <param name="callChain">The call chain to validate.</param>
        /// <param name="methodTable">The method table in scope for the specified call chain. These methods are used to verify method bindings.</param>
        /// <param name="constants">The set of all available constants.</param>
        /// <param name="requireMethodDefinitions">
        /// If true, then errors will be added to the set if any of the methods referenced
        /// in the call chain are marked as 'needs-defining', i.e. a concrete implementation has not been set.
        /// </param>
        /// <param name="messages">A list of messages that this call will add to if there are any issues.</param>
        void ValidateCallChain(ICallChainSource callChain, MethodTable methodTable, InteractionConstantSet constants, bool requireMethodDefinitions, List<LanguageOperationMessage> messages);
    }
}
