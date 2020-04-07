using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep
{
    public class ProjectConfigurationException : Exception
    {
        public ProjectConfigurationException(string message)
            : base(message)
        {
        }

        public ProjectConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
