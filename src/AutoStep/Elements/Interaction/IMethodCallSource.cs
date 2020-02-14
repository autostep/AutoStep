using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Elements.Interaction
{
    public interface IMethodCallSource
    {
        public List<MethodCallElement> MethodCallChain { get; }
    }
}
