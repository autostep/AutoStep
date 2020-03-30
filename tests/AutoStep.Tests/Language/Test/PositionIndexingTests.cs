using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Elements.Test;
using AutoStep.Language;
using AutoStep.Language.Test;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Test
{
    public class PositionIndexingTests : CompilerTestBase
    {
        public PositionIndexingTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task CreatesValidPositionIndexForEachScope()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have done something

                Examples:
                    | header1 |
                    | value1  |
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            // Feature header.
            var pos = positions.Lookup(2, 17);
            pos.CurrentScope.Should().BeOfType<FeatureElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.EntryMarker);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.Feature);

            // Scenario names.
            pos = positions.Lookup(4, 28);
            pos.CurrentScope.Should().BeOfType<ScenarioElement>();

            // Blank line in scenario.
            pos = positions.Lookup(5, 1);
            pos.CurrentScope.Should().BeOfType<ScenarioElement>();

            // Step reference.
            pos = positions.Lookup(6, 23);
            pos.CurrentScope.Should().BeOfType<StepReferenceElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.StepTypeKeyword);
        }
    }
}
