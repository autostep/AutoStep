using System;
using System.Globalization;

namespace AutoStep.Execution.Binding
{
    /// <summary>
    /// Presents the default argument binder, which binds using the <see cref="Convert"/> class.
    /// </summary>
    internal class DefaultArgumentBinder : IArgumentBinder
    {
        /// <inheritdoc/>
        public object Bind(string textValue, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return textValue;
            }

            return Convert.ChangeType(textValue, destinationType, CultureInfo.CurrentCulture);
        }
    }
}
