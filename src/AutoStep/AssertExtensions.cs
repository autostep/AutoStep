using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AutoStep
{
    internal static class AssertExtensions
    {
        [return: NotNullIfNotNull("value")]
        public static TObject ThrowIfNull<TObject>(this TObject value, string paramName)
        {
            if (value is null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }
    }
}
