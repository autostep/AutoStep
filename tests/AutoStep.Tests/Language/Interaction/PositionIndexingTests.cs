using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Language;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Interaction
{
    public class PositionIndexingTests : InteractionsCompilerTestBase
    {
        public PositionIndexingTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
        
        [Fact]
        public async Task CreatesValidPositionIndexForEachEntity()
        {
            const string TestFile =
            @"
              Trait: clickable

                method(name): click(name, 'value')

                Step: Given I have {arg} something
                    method(arg)
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            // Trait name header.
            var pos = positions.Lookup(2, 17);
            pos.CurrentScope.Should().BeOfType<TraitDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.EntryMarker);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionTrait);

            pos = positions.Lookup(2, 26);
            pos.CurrentScope.Should().BeOfType<TraitDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionTrait);
        }    
    }
}
