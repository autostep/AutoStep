using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Compiler;
using AutoStep.Elements;
using AutoStep.Projects;

namespace AutoStep.Tests.Utils
{
    public static class ProjectFileExtensions
    {
        public static void SetFileReadyForRunTest(this ProjectFile projFile, FileElement builtFile)
        {
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));
            projFile.UpdateLastLinkResult(new LinkResult(true, Enumerable.Empty<CompilerMessage>(), output: builtFile));
        }
    }
}
