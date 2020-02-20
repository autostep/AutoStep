using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Language;
using AutoStep.Projects;
using AutoStep.Elements.Test;
using AutoStep.Language.Test;

namespace AutoStep.Tests.Utils
{
    public static class ProjectFileExtensions
    {
        public static void SetFileReadyForRunTest(this ProjectTestFile projFile, FileElement builtFile)
        {
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));
            projFile.UpdateLastLinkResult(new LinkResult(true, Enumerable.Empty<CompilerMessage>(), output: builtFile));
        }
    }
}
