using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoStep.Compiler.Tests.Builders;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests.Utils
{
    public class CompilerTestBase
    {
        protected ITestOutputHelper TestOutput { get; }

        protected CompilerTestBase(ITestOutputHelper output)
        {
            TestOutput = output;
        }

        protected async Task CompileAndAssertMessages(string content, params CompilerMessage[] expectedMessages)
        {
            var tracer = new TestTracer(TestOutput);
            var compiler = new AutoStepCompiler(AutoStepCompiler.CompilerOptions.EnableDiagnostics, tracer);
            var source = new StringContentSource(content);

            var result = await compiler.Compile(source);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
        }

        protected async Task CompileSuccessNoWarnings(string content, Action<FileBuilder> cfg)
        {
            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            var tracer = new TestTracer(TestOutput);
            var compiler = new AutoStepCompiler(AutoStepCompiler.CompilerOptions.EnableDiagnostics, tracer);
            var source = new StringContentSource(content);

            var result = await compiler.Compile(source);

            // Make sure there are 0 messages
            Assert.Empty(result.Messages);
            Assert.True(result.Success);
            
            var resultAsJson = JsonSerializer.Serialize(result.Output, new JsonSerializerOptions { WriteIndented = true });
            var expectedAsJson = JsonSerializer.Serialize(expectedBuilder.Built, new JsonSerializerOptions { WriteIndented = true });

            TestOutput.WriteLine("Full Expected Tree");
            TestOutput.WriteLine(expectedAsJson);
            TestOutput.WriteLine("Full Actual Tree");
            TestOutput.WriteLine(resultAsJson);

            Assert.Equal(expectedAsJson, resultAsJson);            
        }
    }
}
