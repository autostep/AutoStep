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
using AutoStep.Language.Interaction;
using AutoStep.Elements.Test;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Tests.Utils
{
    public class InteractionsCompilerTestBase : LoggingTestBase
    {
        protected string NewLine => Environment.NewLine;

        protected InteractionsCompilerTestBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected Task CompileAndAssertErrors(string content, params CompilerMessage[] expectedMessages)
        {
            return CompileAndAssertErrors(content, null, expectedMessages);
        }

        protected async Task CompileAndAssertErrors(string content, Action<InteractionFileBuilder> cfg, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one error.", nameof(expectedMessages));

            var compiler = new AutoStepInteractionCompiler(InteractionsCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileInteractionsAsync(source, LogFactory);

            foreach (var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
            Assert.False(result.Success);

            if (cfg is object)
            {
                var expectedBuilder = new InteractionFileBuilder();
                cfg(expectedBuilder);

                AssertElementComparison(expectedBuilder.Built, result.Output, false);
            }
        }

        protected async Task CompileAndAssertWarnings(string content, params CompilerMessage[] expectedMessages)
        {
            if (expectedMessages.Length == 0) throw new ArgumentException("Must provide at least one warning.", nameof(expectedMessages));

            var compiler = new AutoStepInteractionCompiler(InteractionsCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileInteractionsAsync(source, LogFactory);

            foreach (var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            // Make sure the messages are the same.
            Assert.Equal(expectedMessages, result.Messages);
        }

        protected async Task CompileAndAssert(string content, Action<InteractionFileBuilder> cfg)
        {
            var compiler = new AutoStepInteractionCompiler(InteractionsCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileInteractionsAsync(source, LogFactory);

            foreach (var message in result.Messages)
            {
                TestOutput.WriteLine(message.ToString());
            }

            var expectedBuilder = new InteractionFileBuilder();
            cfg(expectedBuilder);

            AssertElementComparison(expectedBuilder.Built, result.Output, false);
        }

        protected async Task CompileAndAssertSuccess(string content, Action<InteractionFileBuilder> cfg = null)
        {   
            var compiler = new AutoStepInteractionCompiler(InteractionsCompilerOptions.EnableDiagnostics);
            var source = new StringContentSource(content);

            var result = await compiler.CompileInteractionsAsync(source, LogFactory);

            // Make sure there are 0 messages
            Assert.Empty(result.Messages);
            Assert.True(result.Success);

            if (cfg is object)
            {
                var expectedBuilder = new InteractionFileBuilder();

                cfg(expectedBuilder);

                AssertElementComparison(expectedBuilder.Built, result.Output, false);
            }
        }

        protected void AssertElementComparison(BuiltElement expected, BuiltElement actual, bool includeStatementParts)
        {
            Assert.NotNull(actual);

            try
            {
                var typeSpan = typeof(ReadOnlySpan<StepToken>);

                actual.Should().BeEquivalentTo(expected, opt => opt
                    .WithStrictOrdering()
                    .AllowingInfiniteRecursion()
                    .IncludingAllRuntimeProperties()
                    .ComparingByMembers<TraitNode>()
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
