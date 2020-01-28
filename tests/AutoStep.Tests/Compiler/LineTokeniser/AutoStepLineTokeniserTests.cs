using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Compiler.Parser;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Compiler.LineTokeniser
{
    public class AutoStepLineTokeniserTests
    {
        [Fact]
        public void GeneratesGivenContextFromKeywordOnly()
        {
            const string Test = "   Given";

            var lineTokeniser = new AutoStepLineTokeniser();

            var context = lineTokeniser.CompileLine(Test);

            context.Should().BeOfType<AutoStepParser.LineGivenContext>();
        }

        [Fact]
        public void GeneratesGivenContext()
        {
            const string Test = "   Given I have a thing";

            var lineTokeniser = new AutoStepLineTokeniser();

            var context = lineTokeniser.CompileLine(Test);

            context.Should().BeOfType<AutoStepParser.LineGivenContext>();
        }

        [Fact]
        public void GeneratesTableRowCellContext()
        {
            const string Test = " | ";

            var lineTokeniser = new AutoStepLineTokeniser();

            var context = lineTokeniser.CompileLine(Test);

            context.Should().BeOfType<AutoStepParser.LineTableRowContext>();
        }
    }
}
