using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace AutoStep
{
    /// <summary>
    /// Helper methods for asserting method contracts.
    /// </summary>
    internal static class AssertExtensions
    {
        /// <summary>
        /// Throw an ArgumentNullException if the object is null.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="value">The object.</param>
        /// <param name="paramName">The name of the argument parameter that provided the value.</param>
        /// <returns>The object (if not null).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static TObject ThrowIfNull<TObject>(this TObject value, string paramName)
        {
            if (value is null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

        /// <summary>
        /// Uses a string to format some arguments, in the correct culture.
        /// </summary>
        /// <param name="fmt">The format string.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatWith(this string fmt, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, fmt, args);
        }
    }
}
