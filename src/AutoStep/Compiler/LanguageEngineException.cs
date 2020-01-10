using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Compiler
{
    public class LanguageEngineException : Exception
    {
        public LanguageEngineException(string message) : base(message)
        {
        }
    }
}
