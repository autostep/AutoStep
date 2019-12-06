using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Core.Tracing
{
    public interface ITracer
    {
        void Info(string msg, object data);

        void Warn(string msg, object data);

        void Error(Exception ex, string msg, object data);
    }
}
