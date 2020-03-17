using System;
using System.Collections.Generic;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Execution.Contexts
{
    public class ExecutionContextTests
    {
        [Fact]
        public void TryGetExistingValueCorrectType()
        {
            var context = new RunContext(new RunConfiguration());
            context.Set("val", 123M);

            context.TryGet<decimal>("val", out var result).Should().BeTrue();
            result.Should().Be(123M);
        }

        [Fact]
        public void TryGetExistingValueIncorrectType()
        {
            var context = new RunContext(new RunConfiguration());
            context.Set("val", 123M);

            context.TryGet<string>("val", out var result).Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void TryGetExistingValueNoSuchName()
        {
            var context = new RunContext(new RunConfiguration());

            context.TryGet<string>("val", out var result).Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void GetExistingValue()
        {
            var context = new RunContext(new RunConfiguration());
            context.Set("val", 123M);

            context.Get<decimal>("val").Should().Be(123M);
        }

        [Fact]
        public void GetNonExistentValue()
        {
            var context = new RunContext(new RunConfiguration());

            context.Invoking(r => r.Get<string>("val")).Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void GetValueOfWrongType()
        {
            var context = new RunContext(new RunConfiguration());
            context.Set("val", 123M);

            context.Invoking(r => r.Get<string>("val")).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void SetNullKeyThrows()
        {
            var context = new RunContext(new RunConfiguration());
            context.Invoking(r => r.Set(null, 1)).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetEmptyKeyThrows()
        {
            var context = new RunContext(new RunConfiguration());
            context.Invoking(r => r.Set("", 1)).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetNullValueThrows()
        {
            var context = new RunContext(new RunConfiguration());
            context.Invoking(r => r.Set<string>("val", null)).Should().Throw<ArgumentNullException>();
        }
    }
}
