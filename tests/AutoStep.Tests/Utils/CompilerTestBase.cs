using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Tests.Builders;
using AutoStep.Elements;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Elements.StepTokens;
using AutoStep.Language.Test;
using AutoStep.Elements.Test;

namespace AutoStep.Tests.Utils
{
    public class CompilerTestBase : LoggingTestBase
    {
        protected string NewLine => Environment.NewLine;

        protected CompilerTestBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected async Task CompileAndAssertErrors(string content, params LanguageOperationMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one error.", nameof(expectedMessages));

            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source, LogFactory);

            foreach (var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
            Assert.False(result.Success);
        }

        protected async Task CompileAndAssertErrors(string content, Action<FileBuilder> cfg, params LanguageOperationMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one error.", nameof(expectedMessages));

            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source, LogFactory);

            foreach (var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
            Assert.False(result.Success);

            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            AssertElementComparison(expectedBuilder.Built, result.Output, false);
        }

        protected async Task CompileAndAssertWarnings(string content, Action<FileBuilder> cfg, params LanguageOperationMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source, LogFactory);

            foreach(var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);

            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            AssertElementComparison(expectedBuilder.Built, result.Output, false);
        }

        protected async Task CompileAndAssertWarningsWithStatementParts(string content, Action<FileBuilder> cfg, params LanguageOperationMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source, LogFactory);

            foreach (var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);

            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            AssertElementComparison(expectedBuilder.Built, result.Output, true);
        }


        protected async Task CompileAndAssertWarnings(string content, params LanguageOperationMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source, LogFactory);

            foreach (var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
        }

        protected async Task CompileAndAssert(string content, Action<FileBuilder> cfg)
        {
            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source, LogFactory);

            foreach (var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            AssertElementComparison(expectedBuilder.Built, result.Output, false);
        }

        protected async Task CompileAndAssertSuccess(string content, Action<FileBuilder> cfg)
        {
            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source, LogFactory);

            // Make sure there are 0 messages
            Assert.Empty(result.Messages);
            Assert.True(result.Success);

            AssertElementComparison(expectedBuilder.Built, result.Output, false);
        }

        protected async Task CompileAndAssertSuccessWithStatementTokens(string content, Action<FileBuilder> cfg)
        {
            var expectedBuilder = new FileBuilder();
            cfg(expectedBuilder);

            var compiler = new TestCompiler(TestCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileAsync(source, LogFactory);

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
                var typeSpan = typeof(ReadOnlySpan<StepToken>);

                actual.Should().BeEquivalentTo(expected, opt => opt
                    .WithStrictOrdering()
                    .IncludingAllRuntimeProperties()
                    .Using<StepReferenceElement>(ctx =>
                    {
                        ctx.Subject.Should().BeEquivalentTo(ctx.Expectation, opt => opt.Excluding((IMemberInfo x) => x.RuntimeType == typeSpan));
                        if (includeStatementParts)
                        {
                            ctx.Subject.TokenSpan.Length.Should().Be(ctx.Expectation.TokenSpan.Length);
                            for (var idx = 0; idx < ctx.Subject.TokenSpan.Length; idx++)
                            {
                                ctx.Subject.TokenSpan[idx].Should().BeEquivalentTo(ctx.Expectation.TokenSpan[idx], "subject[{0}] should match expectation[{0}]", idx);
                            }
                        }
                    }).WhenTypeIs<StepReferenceElement>()
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
            readonly JsonSerializerOptions myOptions = new JsonSerializerOptions { WriteIndented = true };

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
