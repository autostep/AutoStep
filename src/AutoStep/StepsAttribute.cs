using System;

namespace AutoStep
{
    /// <summary>
    /// Indicates that a class contains steps.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class StepsAttribute : Attribute
    {
    }
}
