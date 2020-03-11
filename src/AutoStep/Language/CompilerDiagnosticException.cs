using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoStep.Language
{
    /// <summary>
    /// Represents a compiler diagnostic exception, currently only thrown by the <see cref="AutoStepLineTokeniser"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1064:Exceptions should be public", Justification = "Exception only thrown from internal type, and only consumed by tests.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Message only visible in tests.")]
    internal class CompilerDiagnosticException : Exception
    {
        private readonly LanguageOperationMessage[] parserErrors;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerDiagnosticException"/> class.
        /// </summary>
        /// <param name="parserErrors">The relevant parser errors.</param>
        /// <param name="tokenStreamDebugText">The token stream debug text block.</param>
        public CompilerDiagnosticException(IEnumerable<LanguageOperationMessage> parserErrors, string tokenStreamDebugText)
            : base("Compiler Diagnostic Exception; check Errors and TokenStream Details.")
        {
            this.parserErrors = parserErrors.ToArray();
            TokenStreamDetails = tokenStreamDebugText;
        }

        /// <summary>
        /// Gets the parser errors.
        /// </summary>
        public IReadOnlyCollection<LanguageOperationMessage> Errors => parserErrors;

        /// <summary>
        /// Gets the token debug text.
        /// </summary>
        public string TokenStreamDetails { get; }
    }
}
