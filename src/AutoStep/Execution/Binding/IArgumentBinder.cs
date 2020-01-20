using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Execution.Binding
{
    public interface IArgumentBinder
    {
        object Bind(string textValue, Type destinationType);
    }
}
