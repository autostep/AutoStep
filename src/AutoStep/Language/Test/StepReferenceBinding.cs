using System;
using System.Collections.Generic;
using AutoStep.Definitions;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// Represents a step reference binding from reference -> definition, with any associated arguments.
    /// </summary>
    public class StepReferenceBinding
    {
        private readonly ArgumentBinding[]? args;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepReferenceBinding"/> class.
        /// </summary>
        /// <param name="def">The step definition.</param>
        /// <param name="args">Any associated arguments.</param>
        public StepReferenceBinding(StepDefinition def, ArgumentBinding[]? args, IReadOnlyDictionary<string, string>? placeholders)
        {
            Definition = def;
            Placeholders = placeholders;
            this.args = args;
        }

        /// <summary>
        /// Gets the step definition.
        /// </summary>
        public StepDefinition Definition { get; }

        /// <summary>
        /// Gets the set of arguments (may be null if there are no arguments).
        /// </summary>
        public ReadOnlySpan<ArgumentBinding> Arguments
        {
            get
            {
                if (args is null)
                {
                    return default;
                }

                return args.AsSpan();
            }
        }

        public IReadOnlyDictionary<string, string>? Placeholders { get; }
    }
}
