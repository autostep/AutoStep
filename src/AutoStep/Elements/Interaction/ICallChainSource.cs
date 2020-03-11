using System.Collections.Generic;
using AutoStep.Language.Interaction;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Defines a source of a call chain, for example a method declaration, or a step definition.
    /// </summary>
    internal interface ICallChainSource
    {
        /// <summary>
        /// Gets the name of the source file.
        /// </summary>
        public string? SourceName { get; }

        /// <summary>
        /// Gets the set of method calls in the chain.
        /// </summary>
        public List<MethodCallElement> Calls { get; }

        /// <summary>
        /// Called during compilation, gets the set of variables available to the call chain based on the signature of the call chain source.
        /// </summary>
        /// <returns>A declared set of variables.</returns>
        CallChainCompileTimeVariables GetCompileTimeChainVariables();
    }
}
