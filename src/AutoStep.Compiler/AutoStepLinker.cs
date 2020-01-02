using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Core;
using AutoStep.Core.Sources;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Links compiled autostep content.
    /// </summary>
    /// <remarks>
    /// Linker can hold state, to be able to know what has changed.
    /// The output of the compiler can be fed repeatedly into the linker, to update the references.
    /// </remarks>
    public class AutoStepLinker
    {
        public void AddStepDefinitionSource(IStepDefinitionSource source)
        {

        }

        public void Link(BuiltFile file)
        {

        }
    }
}
