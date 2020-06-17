using System;

namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Step signature container.
    /// </summary>
    internal struct StepDeclarationSignature : IEquatable<StepDeclarationSignature>
    {
        private readonly StepType type;
        private readonly string declaration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDeclarationSignature"/> struct.
        /// </summary>
        /// <param name="type">Step type.</param>
        /// <param name="declaration">Step declaration.</param>
        public StepDeclarationSignature(StepType type, string declaration)
        {
            this.type = type;
            this.declaration = declaration;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is StepDeclarationSignature signature && Equals(signature);
        }

        /// <inheritdoc/>
        public bool Equals(StepDeclarationSignature other)
        {
            return type == other.type &&
                   declaration == other.declaration;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(type, declaration);
        }

        public static bool operator ==(StepDeclarationSignature left, StepDeclarationSignature right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StepDeclarationSignature left, StepDeclarationSignature right)
        {
            return !(left == right);
        }
    }
}
