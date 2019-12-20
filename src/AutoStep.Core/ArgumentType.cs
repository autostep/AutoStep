namespace AutoStep.Core
{
    /// <summary>
    /// Defines the various types of step arguments.
    /// </summary>
    public enum ArgumentType
    {
        /// <summary>
        /// Text-only argument (this is the default if we can't detect anything more specific).
        /// </summary>
        Text,

        /// <summary>
        /// Integer argument (optionally prefixed with a symbol).
        /// </summary>
        NumericInteger,

        /// <summary>
        /// Decimal argument (optionally prefixed with a symbol).
        /// </summary>
        NumericDecimal,

        /// <summary>
        /// An interpolated argument (text based, but evaluation delayed until runtime).
        /// </summary>
        Interpolated,

        /// <summary>
        /// An empty argument.
        /// </summary>
        Empty,
    }
}
