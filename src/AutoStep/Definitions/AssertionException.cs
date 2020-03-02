using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Definitions
{
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message)
        {
        }
    }
}
