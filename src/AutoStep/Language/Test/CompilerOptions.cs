using System;

namespace AutoStep.Language
{
    /// <summary>
    /// The available Compiler Options.
    /// </summary>
    [Flags]
    public enum CompilerOptions
    {
        /// <summary>
        /// Default compiler behaviour.
        /// </summary>
        Default,

        /// <summary>
        /// Enable diagnostics, which causes full lexer and parser data to be written to the tracer.
        /// </summary>
        EnableDiagnostics,
    }
}
