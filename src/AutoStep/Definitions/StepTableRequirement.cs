namespace AutoStep.Definitions
{
    /// <summary>
    /// Defines the requirements a given step definition has for tables.
    /// </summary>
    public enum StepTableRequirement
    {
        /// <summary>
        /// Tables are not supported by the step. Will give a linker warning if one is provided.
        /// </summary>
        NotSupported,

        /// <summary>
        /// The step can optionally take a table.
        /// </summary>
        Optional,

        /// <summary>
        /// The step requires a table.
        /// </summary>
        Required,
    }
}
