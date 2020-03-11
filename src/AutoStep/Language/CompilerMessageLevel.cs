namespace AutoStep.Language
{
    /// <summary>
    /// Defines the possible compiler message levels.
    /// </summary>
    public enum CompilerMessageLevel
    {
        /// <summary>
        /// Information level message.
        /// </summary>
        Info,

        /// <summary>
        /// Warning level message, does not prevent compilation.
        /// </summary>
        Warning,

        /// <summary>
        /// Error level message, prevents succesful compilation.
        /// </summary>
        Error,
    }
}
