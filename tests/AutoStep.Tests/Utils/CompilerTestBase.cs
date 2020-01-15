using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoStep.Compiler;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using AutoStep.Elements;
using AutoStep.Tracing;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Tests.Utils
{
    public class CompilerTestBase
    {
        protected ITestOutputHelper TestOutput { get; }

        internal ITracer TestTracer { get; }

        protected string NewLine => Environment.NewLine;

        protected CompilerTestBase(ITestOutputHelper output)
        {
            TestOutput = output;

            TestTracer = new TestTracer(output);
        }

        protected async Task CompileAndAssertErrors(string content, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one error.", nameof(expectedMessages));

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
            Assert.False(result.Success);
        }

        protected async Task CompileAndAssertWarnings(string content, Action<FileBuilder> cfg, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);

            var expectedBuilder = new FileBuilder(true);
            cfg(expectedBuilder);

            AssertElementComparison(expectedBuilder.Built, result.Output, false);
        }

        protected async Task CompileAndAssertWarningsWithStatementParts(string content, Action<FileBuilder> cfg, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);

            var expectedBuilder = new FileBuilder(true);
            cfg(expectedBuilder);

            AssertElementComparison(expectedBuilder.Built, result.Output, true);
        }


        protected async Task CompileAndAssertWarnings(string content, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
        }

        protected async Task CompileAndAssert(string content, Action<FileBuilder> cfg)
        {
            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            var expectedBuilder = new FileBuilder(true);
            cfg(expectedBuilder);

            AssertElementComparison(expectedBuilder.Built, result.Output, false);
        }

        protected async Task CompileAndAssertSuccess(string content, Action<FileBuilder> cfg)
        {
            var expectedBuilder = new FileBuilder(true);
            cfg(expectedBuilder);

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure there are 0 messages
            Assert.Empty(result.Messages);
            Assert.True(result.Success);

            AssertElementComparison(expectedBuilder.Built, result.Output, false);
        }

        protected async Task CompileAndAssertSuccessWithStatementTokens(string content, Action<FileBuilder> cfg)
        {
            var expectedBuilder = new FileBuilder(true);
            cfg(expectedBuilder);

            var compiler = new AutoStepCompiler(CompilerOptions.EnableDiagnostics, TestTracer);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source);

            // Make sure there are 0 messages
            Assert.Empty(result.Messages);
            Assert.True(result.Success);

            AssertElementComparison(expectedBuilder.Built, result.Output, true);
        }

        protected void AssertElementComparison(BuiltElement expected, BuiltElement actual, bool includeStatementParts)
        {
            Assert.NotNull(actual);

            try
            {
                var spanType = typeof(ReadOnlySpan<StepToken>);

                actual.Should().BeEquivalentTo(expected, opt => opt
                    .WithStrictOrdering()
                    .IncludingAllRuntimeProperties()                    
                    .Excluding((IMemberInfo member) => spanType.IsAssignableFrom(member.SelectedMemberInfo.MemberType) ||
                                                       (
                                                       !includeStatementParts && 
                                                        member.SelectedMemberInfo != null &&
                                                        (typeof(StepToken).IsAssignableFrom(member.SelectedMemberInfo.MemberType) ||
                                                        typeof(IEnumerable<StepToken>).IsAssignableFrom(member.SelectedMemberInfo.MemberType)
                                                        )))
                );
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

        private class AllExceptInternalPropertiesSelectionRule : IMemberSelectionRule
        {
            public bool IncludesMembers
            {
                get { return false; }
            }

            public IEnumerable<SelectedMemberInfo> SelectMembers(
                IEnumerable<SelectedMemberInfo> selectedMembers,
                IMemberInfo context,
                IEquivalencyAssertionOptions config)
            {
                return selectedMembers.Except(context.RuntimeType.GetNonPrivateProperties().Where(p => p.GetMethod.IsAssembly).Select(SelectedMemberInfo.Create));
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
