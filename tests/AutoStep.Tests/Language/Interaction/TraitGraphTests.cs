using System.Collections;
using System.Linq;
using System.Text;
using AutoStep.Elements.Interaction;
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
            var traitGraph = new TraitGraph();

            var traitA = CreateTrait("A");
            var traitB = CreateTrait("B");
            var traitC = CreateTrait("C");
            var traitD = CreateTrait("D");

            var traitAD = CreateTrait("A", "D");
            var traitBD = CreateTrait("B", "D");

            traitGraph.AddOrExtendTrait(traitA);
            traitGraph.AddOrExtendTrait(traitB);

            traitGraph.AddOrExtendTrait(traitAD);
            traitGraph.AddOrExtendTrait(traitC);
            traitGraph.AddOrExtendTrait(traitD);
            traitGraph.AddOrExtendTrait(traitBD);

            // Find all traits.
            var result = traitGraph.MatchTraits(GetNameParts(new string[] { "A", "D", "C" }));

            result.OrderedTraits.Should().Equal(traitAD, traitD, traitC, traitA);
        }

        [Fact]
        public void ComplexGraphCreate()
        {
            var traitGraph = new TraitGraph();

            var traitA = CreateTrait("A");
            var traitB = CreateTrait("B");
            var traitC = CreateTrait("C");
            var traitD = CreateTrait("D");
            var traitE = CreateTrait("E");
            var traitF = CreateTrait("F");
            var traitG = CreateTrait("G");
            
            var traitAB = CreateTrait("A", "B");
            var traitBD = CreateTrait("B", "D");
            var traitCD = CreateTrait("C", "D");
            var traitCE = CreateTrait("C", "E");
            var traitEF = CreateTrait("E", "F");
            var traitABC = CreateTrait("A", "B", "C");
            var traitCDE = CreateTrait("C", "D", "E");
            var traitABCD = CreateTrait("A", "B", "C", "D");
            var traitABCE = CreateTrait("A", "B", "C", "E");

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
            var result = traitGraph.MatchTraits(GetNameParts(new string[] { "A", "B", "C", "E", "F" }));

            result.OrderedTraits.Should().Equal(traitABCE, traitABC, 
                                                traitEF, traitCE, traitAB, 
                                                traitF, traitE, traitC, traitB, traitA);
        }

        [Fact]
        public void SearchMiddleOfGraph()
        {
            var traitGraph = new TraitGraph();

            var traitA = CreateTrait("A");
            var traitB = CreateTrait("B");
            var traitC = CreateTrait("C");
            var traitD = CreateTrait("D");
            var traitE = CreateTrait("E");
            var traitF = CreateTrait("F");
            var traitG = CreateTrait("G");

            var traitAB = CreateTrait("A", "B");
            var traitBD = CreateTrait("B", "D");
            var traitCD = CreateTrait("C", "D");
            var traitCE = CreateTrait("C", "E");
            var traitEF = CreateTrait("E", "F");
            var traitABC = CreateTrait("A", "B", "C");
            var traitCDE = CreateTrait("C", "D", "E");
            var traitABCD = CreateTrait("A", "B", "C", "D");
            var traitABCE = CreateTrait("A", "B", "C", "E");

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
            var result = traitGraph.MatchTraits(GetNameParts(new string[] { "A", "B" }));

            result.OrderedTraits.Should().Equal(traitAB, traitB, traitA);
        }


        [Fact]
        public void MergeGraph()
        {
            var traitGraph1 = new TraitGraph();

            var traitA = CreateTrait("A");
            var traitB = CreateTrait("B");
            var traitC = CreateTrait("C");
            var traitD = CreateTrait("D");
            var traitE = CreateTrait("E");
            var traitF = CreateTrait("F");
            var traitG = CreateTrait("G");

            var traitAB = CreateTrait("A", "B");
            var traitBD = CreateTrait("B", "D");
            var traitCD = CreateTrait("C", "D");
            var traitCE = CreateTrait("C", "E");
            var traitEF = CreateTrait("E", "F");
            var traitABC = CreateTrait("A", "B", "C");
            var traitCDE = CreateTrait("C", "D", "E");
            var traitABCD = CreateTrait("A", "B", "C", "D");
            var traitABCE = CreateTrait("A", "B", "C", "E");

            traitGraph1.AddOrExtendTrait(traitA);
            traitGraph1.AddOrExtendTrait(traitB);
            traitGraph1.AddOrExtendTrait(traitC);
            traitGraph1.AddOrExtendTrait(traitD);

            traitGraph1.AddOrExtendTrait(traitAB);
            traitGraph1.AddOrExtendTrait(traitBD);
            traitGraph1.AddOrExtendTrait(traitCD);
            // Define ABC in two places
            traitGraph1.AddOrExtendTrait(traitABC);
            traitGraph1.AddOrExtendTrait(traitABCD);

            var traitGraph2 = new TraitGraph();
            traitGraph2.AddOrExtendTrait(traitE);
            traitGraph2.AddOrExtendTrait(traitF);
            traitGraph2.AddOrExtendTrait(traitG);
            traitGraph2.AddOrExtendTrait(traitCE);
            traitGraph2.AddOrExtendTrait(traitEF);

            // Define ABC in two places
            traitGraph2.AddOrExtendTrait(traitABC);

            traitGraph2.AddOrExtendTrait(traitCDE);
            traitGraph2.AddOrExtendTrait(traitABCE);

            var mainGraph = new TraitGraph();
            mainGraph.Merge(traitGraph1);
            mainGraph.Merge(traitGraph2);

            // Find all traits.
            var result = mainGraph.MatchTraits(GetNameParts(new string[] { "A", "B", "C", "E", "F" }));

            result.OrderedTraits.Should().Equal(traitABCE, traitABC,
                                                traitEF, traitCE, traitAB,
                                                traitF, traitE, traitC, traitB, traitA);
        }

        private NameRefElement[] GetNameParts(params string[] nameParts)
        {
            return nameParts.Select(n => new NameRefElement { Name = n }).ToArray();
        }

        private TraitDefinitionElement CreateTrait(params string[] nameParts)
        {
            var newElement = new TraitDefinitionElement();
            newElement.Name = string.Join(" + ", nameParts);
            newElement.SetNameParts(GetNameParts(nameParts));
            return newElement;
        }
    }
}
