using System.Collections;
using System.Linq;
using System.Text;
using AutoStep.Language.Interaction.Traits;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Interaction
{
    public class TraitGraphTests
    {
        [Fact]
        public void SimpleGraphCreate()
        {
            var traitGraph = new SimpleTraitGraph();

            var traitA = new Trait("A");
            var traitB = new Trait("B");
            var traitC = new Trait("C");
            var traitD = new Trait("D");

            var traitAD = new Trait("A", "D");
            var traitBD = new Trait("B", "D");

            traitGraph.AddOrExtendTrait(traitA);
            traitGraph.AddOrExtendTrait(traitB);

            traitGraph.AddOrExtendTrait(traitAD);
            traitGraph.AddOrExtendTrait(traitC);
            traitGraph.AddOrExtendTrait(traitD);
            traitGraph.AddOrExtendTrait(traitBD);

            // Find all traits.
            var result = traitGraph.MatchTraits(new string[] { "A", "D", "C" });

            result.OrderedTraits.Should().Equal(traitD, traitA, traitAD, traitC);
        }

        [Fact]
        public void ComplexGraphCreate()
        {
            var traitGraph = new SimpleTraitGraph();

            var traitA = new Trait("A");
            var traitB = new Trait("B");
            var traitC = new Trait("C");
            var traitD = new Trait("D");
            var traitE = new Trait("E");
            var traitF = new Trait("F");
            var traitG = new Trait("G");
            
            var traitAB = new Trait("A", "B");
            var traitBD = new Trait("B", "D");
            var traitCD = new Trait("C", "D");
            var traitCE = new Trait("C", "E");
            var traitEF = new Trait("E", "F");
            var traitABC = new Trait("A", "B", "C");
            var traitCDE = new Trait("C", "D", "E");
            var traitABCD = new Trait("A", "B", "C", "D");
            var traitABCE = new Trait("A", "B", "C", "E");

            traitGraph.AddOrExtendTrait(traitA);
            traitGraph.AddOrExtendTrait(traitB);
            traitGraph.AddOrExtendTrait(traitC);
            traitGraph.AddOrExtendTrait(traitD);
            traitGraph.AddOrExtendTrait(traitE);
            traitGraph.AddOrExtendTrait(traitF);
            traitGraph.AddOrExtendTrait(traitG);
            traitGraph.AddOrExtendTrait(traitAB);
            traitGraph.AddOrExtendTrait(traitBD);
            traitGraph.AddOrExtendTrait(traitCD);
            traitGraph.AddOrExtendTrait(traitCE);
            traitGraph.AddOrExtendTrait(traitEF);
            traitGraph.AddOrExtendTrait(traitABC);
            traitGraph.AddOrExtendTrait(traitCDE);
            traitGraph.AddOrExtendTrait(traitABCD);
            traitGraph.AddOrExtendTrait(traitABCE);

            // Find all traits.
            var result = traitGraph.MatchTraits(new string[] { "A", "B", "C", "E", "F" });

            result.OrderedTraits.Should().Equal(traitB, traitA,
                                                         traitAB, traitC,
                                                         traitABC,
                                                         traitE, traitCE,
                                                         traitABCE,
                                                         traitF,
                                                         traitEF);
        }
    }
}
