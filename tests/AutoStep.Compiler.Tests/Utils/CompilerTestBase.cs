using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoStep.Compiler.Tests.Utils
{
    public class CompilerTestBase
    {
        protected async Task CompileAndAssertMessage(string content, CompilerMessage expectedMessage)
        {
            var compiler = new AutoStepCompiler();
            var source = new StringContentSource(content);

            var result = await compiler.Compile(source);

            // Make sure there is only 1
            Assert.Single(result.Messages);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessage, result.Messages.First());
        }

        protected async Task CompileSuccessNoWarnings(string content)
        {
            var compiler = new AutoStepCompiler();
            var source = new StringContentSource(content);

            var result = await compiler.Compile(source);

            // Make sure there are 0 messages
            Assert.Empty(result.Messages);
            Assert.True(result.Success);
        }
    }
}
