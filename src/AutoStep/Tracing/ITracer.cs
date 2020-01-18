using System;

namespace AutoStep.Tracing
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
        void Info(string msg, object data);

        /// <summary>
        /// Invoked when a warning-level event occurs.
        /// </summary>
        /// <param name="msg">The message body.</param>
        /// <param name="data">An object containing message values.</param>
        void Warn(string msg, object data);

        /// <summary>
        /// Invoked when an error-level event occurs.
        /// </summary>
        /// <param name="ex">The exception related to the error.</param>
        /// <param name="msg">The message body.</param>
        /// <param name="data">An object containing message values.</param>
        void Error(Exception ex, string msg, object data);

        /// <summary>
        /// Invoked with debug-level data.
        /// </summary>
        /// <param name="format">The message body.</param>
        /// <param name="frmtArgs">Format arguments.</param>
        void Debug(string format, params object[] frmtArgs);
    }
}
