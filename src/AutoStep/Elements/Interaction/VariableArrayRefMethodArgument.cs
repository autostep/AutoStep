using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;

namespace AutoStep.Elements.Interaction
{

    public class VariableArrayRefMethodArgument : MethodArgumentElement
    {
        public string VariableName { get; set; }

        public MethodArgumentElement Indexer { get; set; }
    }
}
