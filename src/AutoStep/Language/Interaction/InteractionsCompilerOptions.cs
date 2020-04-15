using System;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Defines the available interaction compilation options.
    /// </summary>
    [Flags]
    public enum InteractionsCompilerOptions
    {
        /// <summary>
        /// Default behaviour.
        /// </summary>
        Default,

        /// <summary>
        /// Enable diagnostics, which causes full lexer and parser data to be written to the tracer.
        /// </summary>
        EnableDiagnostics = 0b1,

        /// <summary>
        /// Creates an entry in the line/column index for each element we add, so we can access it during intellisense
        /// generation.
        /// </summary>
        CreatePositionIndex = 0b10,
    }
}
