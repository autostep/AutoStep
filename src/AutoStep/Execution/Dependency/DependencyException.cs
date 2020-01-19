using System;
using System.Runtime.Serialization;

namespace AutoStep.Execution.Dependency
{
    public class DependencyException : Exception
    {
        public DependencyException()
        {
        }

        public DependencyException(string message) : base(message)
        {
        }

        public DependencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
