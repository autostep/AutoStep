using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Binding
{
    internal class DefaultArgumentBinder : IArgumentBinder
    {
        public object Bind(string textValue, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return textValue;
            }

            // Throw a better exception here.
            return Convert.ChangeType(textValue, destinationType, CultureInfo.CurrentCulture);
        }
    }
}
