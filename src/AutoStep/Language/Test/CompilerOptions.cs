using System;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// The available Compiler Options.
    /// </summary>
    [Flags]
    public enum TestCompilerOptions
    {
        /// <summary>
        /// Default compiler behaviour.
        /// </summary>
        Default,

        /// <summary>
        /// Enable diagnostics, which causes full lexer and parser data to be written to the tracer.
        /// </summary>
        EnableDiagnostics = 0b01,

        /// <summary>
        /// Creates an entry in the line/column index for each element we add, so we can access it during intellisense
        /// generation.
        /// </summary>
        CreatePositionIndex = 0b10,
    }
}
