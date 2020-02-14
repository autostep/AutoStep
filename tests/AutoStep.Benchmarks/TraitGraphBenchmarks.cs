using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Traits;
using BenchmarkDotNet.Attributes;

namespace AutoStep.Benchmarks
{
    public class TraitGraphBenchmarks
    {
        private Random random;
        private int rootNodeSize;
        private int numberOfCombos;
        private int maxComboSize;

        [Params(2, 4, 8, 16)]
        public int Complexity { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // Static seed to give predictable comparisons.
            random = new Random(1225345345);

            rootNodeSize = Complexity * 4;
            numberOfCombos = Complexity * 20;
            maxComboSize = Complexity;
        }

        [Benchmark(Baseline = true)]
        public void BaseLineGraphCreate()
        {
            var freshGraph = BuildGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            var first = freshGraph.AllTraits.First.Value;
            var second = freshGraph.AllTraits.First.Next.Value;

            var _ = first.Trait.NameParts.Union(second.Trait.NameParts).ToArray();
        }
        
        [Benchmark]
        public void MatchAllForward()
        {
            // Find all traits.
            var graph = BuildGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            // Go through every item (top down) and match it.
            var node = graph.AllTraits.First;

            while (node != null)
            {
                graph.MatchTraits(node.Value.Trait.NameParts);

                node = node.Next;
            }
        }

        [Benchmark]
        public void MatchAllBackwards()
        {
            // Find all traits.
            var graph = BuildGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            // Go through every item (top down) and match it.
            var node = graph.AllTraits.Last;

            while (node != null)
            {
                graph.MatchTraits(node.Value.Trait.NameParts);

                node = node.Previous;
            }
        }

        [Benchmark]
        public void SingleLookup()
        {
            var freshGraph = BuildGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            var first = freshGraph.AllTraits.First.Value;
            var second = freshGraph.AllTraits.First.Next.Value;

            var lookup = first.Trait.NameParts.Union(second.Trait.NameParts).ToArray();

            // Find all traits.
            var result = freshGraph.MatchTraits(lookup);

            if (result.OrderedTraits.Count == 0)
            {
                throw new InvalidOperationException();
            }
        }

        private TraitGraph BuildGraph(int rootTraitCount, int numberOfCombos, int maxComboSize, Random random)
        {
            var traitGraph = new TraitGraph();
            var alphabet = Enumerable.Range(0, rootTraitCount).Select(i => i.ToString());

            foreach (var item in alphabet)
            {
                var newEl = new TraitDefinitionElement() { Name = item };
                newEl.SetNameParts(new NameRefElement { Name = item });

                traitGraph.AddOrExtendTrait(newEl);
            }

            for (var comboNum = 0; comboNum < numberOfCombos; comboNum++)
            {
                // Determine combo size.
                var comboSize = random.Next(1, maxComboSize);
                var comboItems = new HashSet<string>();

                while (comboItems.Count < comboSize)
                {
                    var comboItem = random.Next(1, rootTraitCount - 1);

                    comboItems.Add(comboItem.ToString());
                }

                var traitDefEl = new TraitDefinitionElement();
                traitDefEl.Name = string.Join(" + ", comboItems);
                traitDefEl.SetNameParts(comboItems.Select(c => new NameRefElement { Name = c }).ToArray());

                traitGraph.AddOrExtendTrait(traitDefEl);
            }

            return traitGraph;
        }
    }
}
