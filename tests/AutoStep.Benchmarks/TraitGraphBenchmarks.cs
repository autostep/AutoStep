using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Traits;
using BenchmarkDotNet.Attributes;

namespace AutoStep.Benchmarks
{
    public class TraitGraphBenchmarks
    {
        private Random random = null!;
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

            var first = freshGraph.AllTraits.First();
            var second = freshGraph.AllTraits.Skip(1).First();

            var _ = first.NameElements.Union(second.NameElements).ToArray();
        }

        [Benchmark]
        public void MatchAllForward()
        {
            // Find all traits.
            var graph = BuildGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            // Go through every item (top down) and match it.
            foreach(var trait in graph.AllTraits)
            {
                graph.SearchTraits(trait.NameElements.Select(x => x.Name), (object?) null, (c, el) => { });
            }
        }

        [Benchmark]
        public void MatchAllBackwards()
        {
            // Find all traits.
            var graph = BuildGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            foreach (var trait in graph.AllTraits.Reverse())
            {
                graph.SearchTraits(trait.NameElements.Select(x => x.Name), (object?)null, (c, el) => { });
            }
        }

        [Benchmark]
        public void SingleLookup()
        {
            var freshGraph = BuildGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            var first = freshGraph.AllTraits.First();
            var second = freshGraph.AllTraits.Skip(1).First();

            var lookup = first.NameElements.Select(x => x.Name).Union(second.NameElements.Select(x => x.Name)).ToArray();

            // Find all traits.
            freshGraph.SearchTraits(lookup, (object?)null, (c, el) => { });
        }

        private TraitGraph BuildGraph(int rootTraitCount, int numberOfCombos, int maxComboSize, Random random)
        {
            var traitGraph = new TraitGraph();
            var alphabet = Enumerable.Range(0, rootTraitCount).Select(i => i.ToString());

            foreach (var item in alphabet)
            {
                var newEl = new TraitDefinitionElement(item, new[] { new NameRefElement(item) });

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

                var traitDefEl = new TraitDefinitionElement(string.Join(" + ", comboItems), comboItems.Select(c => new NameRefElement(c)).ToArray());
                traitGraph.AddOrExtendTrait(traitDefEl);
            }

            return traitGraph;
        }
    }
}
