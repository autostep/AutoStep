namespace AutoStep
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
        /// Decimal argument.
        /// </summary>
        NumericDecimal,

        /// <summary>
        /// Integer argument.
        /// </summary>
        NumericInteger,
    }
}
