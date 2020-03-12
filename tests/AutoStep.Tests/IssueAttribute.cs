using System;

namespace AutoStep.Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IssueAttribute : Attribute
    {
        public IssueAttribute(string issueLink)
        {
            IssueLink = issueLink;
        }

        public string IssueLink { get; }
    }
}
