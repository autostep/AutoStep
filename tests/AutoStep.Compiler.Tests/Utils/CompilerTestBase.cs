using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoStep.Compiler.Tests.Builders;
using AutoStep.Core;
using AutoStep.Core.Elements;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests.Utils
{
    public class CompilerTestBase
    {
        protected ITestOutputHelper TestOutput { get; }

        protected string NewLine => Environment.NewLine;

        protected CompilerTestBase(ITestOutputHelper output)
        {
            TestOutput = output;
        }

        protected async Task CompileAndAssertErrors(string content, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one error.", nameof(expectedMessages));

            var tracer = new TestTracer(TestOutput);
            var compiler = new AutoStepCompiler(AutoStepCompiler.CompilerOptions.EnableDiagnostics, tracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
            Assert.False(result.Success);
        }

        protected async Task CompileAndAssertWarnings(string content, Action<FileBuilder> cfg, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var tracer = new TestTracer(TestOutput);
            var compiler = new AutoStepCompiler(AutoStepCompiler.CompilerOptions.EnableDiagnostics, tracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);

            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            AssertFileComparison(expectedBuilder.Built, result.Output);
        }


        protected async Task CompileAndAssertWarnings(string content, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var tracer = new TestTracer(TestOutput);
            var compiler = new AutoStepCompiler(AutoStepCompiler.CompilerOptions.EnableDiagnostics, tracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
        }

        protected async Task CompileAndAssert(string content, Action<FileBuilder> cfg)
        {
            var tracer = new TestTracer(TestOutput);
            var compiler = new AutoStepCompiler(AutoStepCompiler.CompilerOptions.EnableDiagnostics, tracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            AssertFileComparison(expectedBuilder.Built, result.Output);
        }

        protected async Task CompileAndAssertSuccess(string content, Action<FileBuilder> cfg)
        {
            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            var tracer = new TestTracer(TestOutput);
            var compiler = new AutoStepCompiler(AutoStepCompiler.CompilerOptions.EnableDiagnostics, tracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure there are 0 messages
            Assert.Empty(result.Messages);
            Assert.True(result.Success);

            AssertFileComparison(expectedBuilder.Built, result.Output);
        }

        private void AssertFileComparison(BuiltFile expected, BuiltFile actual)
        {
            Assert.NotNull(actual);

            try
            {
                actual.Should().BeEquivalentTo(expected, opt => opt
                    .WithStrictOrdering()
                    .IncludingAllRuntimeProperties());
            }
            catch
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                options.Converters.Add(new PolymorphicWriteOnlyJsonConverter<AnnotationElement>());
                options.Converters.Add(new PolymorphicWriteOnlyJsonConverter<ScenarioElement>());

                var resultAsJson = JsonSerializer.Serialize(actual, options);
                var expectedAsJson = JsonSerializer.Serialize(expected, options);

                TestOutput.WriteLine("Full Expected Tree");
                TestOutput.WriteLine(expectedAsJson);
                TestOutput.WriteLine("Full Actual Tree");
                TestOutput.WriteLine(resultAsJson);

                throw;
            }
        }

        private class PolymorphicWriteOnlyJsonConverter<T> : JsonConverter<T>
        {
            JsonSerializerOptions myOptions = new JsonSerializerOptions { WriteIndented = true };

            public PolymorphicWriteOnlyJsonConverter()
            {   
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions myOptions)
            {
                throw new NotImplementedException($"Deserializing not supported. Type={typeToConvert}.");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), myOptions);
            }
        }
    }
}
