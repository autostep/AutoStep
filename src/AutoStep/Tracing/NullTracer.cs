using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Tracing
{
    public class NullTracer : ITracer
    {
        public static NullTracer Instance { get; } = new NullTracer();

        public void Debug(string format, params object[] frmtArgs)
        {
            return;
        }

        public void Error(Exception ex, string msg, object data)
        {
            return;
        }

        public void Info(string msg, object data)
        {
            return;
        }

        public void Warn(string msg, object data)
        {
            return;
        }
    }
}
