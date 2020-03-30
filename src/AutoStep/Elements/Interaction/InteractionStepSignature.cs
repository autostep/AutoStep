using System;
using System.Collections.Generic;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Custom key for interaction step signature.
    /// </summary>
    internal struct InteractionStepSignature : IEquatable<InteractionStepSignature>
    {
        private readonly StepType type;
        private readonly string declaration;
        private readonly ISet<string> components;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionStepSignature"/> struct.
        /// </summary>
        /// <param name="type">Step type.</param>
        /// <param name="declaration">Step declaration.</param>
        /// <param name="components">Set of valid components.</param>
        public InteractionStepSignature(StepType type, string declaration, ISet<string> components)
        {
            this.type = type;
            this.declaration = declaration;
            this.components = components;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is InteractionStepSignature signature && Equals(signature);
        }

        /// <inheritdoc/>
        public bool Equals(InteractionStepSignature other)
        {
            return type == other.type &&
                   declaration == other.declaration &&
                   components.SetEquals(other.components);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Allow the hashcode to only go to the depth of the declaration,
            // so the equals check will then compare component names.
            return HashCode.Combine(type, declaration);
        }

        public static bool operator ==(InteractionStepSignature left, InteractionStepSignature right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InteractionStepSignature left, InteractionStepSignature right)
        {
            return !(left == right);
        }
    }
}
