using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.StepTokens;
using AutoStep.Elements.Test;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Elements.Test
{
    public class StepReferenceElementTests
    {
        [Fact]
        public void ReferencedVariablesThrowsIfNotFrozen()
        {
            var stepRef = new StepReferenceElement();
            stepRef.AddToken(new VariableToken("var1", 1, 10));

            stepRef.Invoking(r => r.ReferencedVariables).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ReferencedVariablesGivesVariableNames()
        {
            var stepRef = new StepReferenceElement();
            stepRef.AddToken(new VariableToken("var1", 1, 10));
            stepRef.AddToken(new TextToken(11, 5));
            stepRef.AddToken(new TextToken(20, 4));
            stepRef.AddToken(new VariableToken("var2", 24, 5));
            stepRef.AddToken(new VariableToken("var3", 29, 5));
            stepRef.FreezeTokens();

            stepRef.ReferencedVariables.Should().BeEquivalentTo("var1", "var2", "var3");
        }
    }
}
