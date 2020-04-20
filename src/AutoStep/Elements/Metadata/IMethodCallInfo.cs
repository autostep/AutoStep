using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Elements.Metadata
{
    /// <summary>
    /// Provides metadata for an interaction method call.
    /// </summary>
    public interface IMethodCallInfo : IPositionalElementInfo
    {
        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// Gets the set of arguments to the method.
        /// </summary>
        IReadOnlyList<IMethodCallArgumentInfo> Arguments { get; }
    }
}
