using System;

namespace AutoStep.Projects.Configuration
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
