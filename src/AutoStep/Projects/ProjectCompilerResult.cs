using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Compiler;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution
{

    public class ProjectCompilerResult : CompilerResult<Project>
    {
        public ProjectCompilerResult(bool success, IEnumerable<CompilerMessage> messages, Project? output = null) 
            : base(success, messages, output)
        {
        }
    }
}
