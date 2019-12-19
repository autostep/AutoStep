using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Core.Tracing
{
    /// <summary>
    /// Defines the types of tracing that a tracer must support.
    /// </summary>
    public interface ITracer
    {
        /// <summary>
        /// Invoked when an info-level event occurs.
        /// </summary>
        /// <param name="msg">The message body.</param>
        /// <param name="data">An object containing message values.</param>
        void TraceInfo(string msg, object data);

        /// <summary>
        /// Invoked when a warning-level event occurs.
        /// </summary>
        /// <param name="msg">The message body.</param>
        /// <param name="data">An object containing message values.</param>
        void TraceWarn(string msg, object data);

        /// <summary>
        /// Invoked when an error-level event occurs.
        /// </summary>
        /// <param name="ex">The exception related to the error.</param>
        /// <param name="msg">The message body.</param>
        /// <param name="data">An object containing message values.</param>
        void TraceError(Exception ex, string msg, object data);
    }
}
