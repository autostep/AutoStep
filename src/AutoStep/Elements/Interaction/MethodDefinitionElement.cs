using System.Collections.Generic;
using AutoStep.Language.Interaction;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a method definition inside an interaction file.
    /// </summary>
    public class MethodDefinitionElement : PositionalElement, ICallChainSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinitionElement"/> class.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        public MethodDefinitionElement(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the method needs to be defined by
        /// a derived trait or component.
        /// </summary>
        public bool NeedsDefining { get; set; }

        /// <summary>
        /// Gets or sets the source file that contains this element.
        /// </summary>
        public string? SourceName { get; set; }

        /// <summary>
        /// Gets or sets the documentation element associated with
        /// this method definition.
        /// </summary>
        public string? Documentation { get; set; }

        /// <summary>
        /// Gets the set of calls in the method definition.
        /// </summary>
        public List<MethodCallElement> Calls { get; } = new List<MethodCallElement>();

        /// <summary>
        /// Gets the set of arguments to the method.
        /// </summary>
        public List<MethodDefinitionArgumentElement> Arguments { get; } = new List<MethodDefinitionArgumentElement>();

        /// <summary>
        /// Gets the set of compile time variables made available by the arguments to the method.
        /// </summary>
        /// <returns>The variables.</returns>
        public CallChainCompileTimeVariables GetCompileTimeChainVariables()
        {
            var variableSet = new CallChainCompileTimeVariables();

            foreach (var arg in Arguments)
            {
                variableSet.SetVariable(arg.Name, false);
            }

            return variableSet;
        }
    }
}
