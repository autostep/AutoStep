using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AutoStep.Compiler.Tests.Utils
{
    public class CompilerTestBase
    {
        protected void CompileAndAssertMessage(string content, CompilerMessage expectedMessage)
        {
            var compiler = new AutoStepCompiler();
            var source = new StringContentSource(content);

            var result = compiler.Compile(source);

            // Make sure there is only 1
            Assert.Single(result.Messages);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessage, result.Messages.First());
        }

    }
}
