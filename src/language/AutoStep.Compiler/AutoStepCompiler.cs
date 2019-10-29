using System;

namespace AutoStep.Compiler
{

    public class AutoStepCompiler
    {
        public AutoStepCompiler()
        {
            // The project is optional.
        }

        public AutoStepCompiler(ProjectSet project)
        {
            // Passing the project just gives more options.
        }

        public CompilerResult Compile(IContentSource source)
        {
            // Compile the file.
            return new CompilerResult();
        }
    }
}
