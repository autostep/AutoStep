using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Language.Interaction.Traits;
using BenchmarkDotNet.Attributes;

namespace AutoStep.Benchmarks
{
    public class TraitGraphBenchmarks
    {
        private TraitGraph freshGraph;
        private SimpleTraitGraph simpleGraph;
        private string[] lookup;

        [Params(2, 3, 4, 5, 6, 7, 8)]
        public int Complexity { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(1225345345);

            var rootNodeSize = Complexity * 4;
            var numberOfCombos = Complexity * 20;
            var maxComboSize = Complexity;
            
            freshGraph = BuildGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            random = new Random(1225345345);
            simpleGraph = BuildSimpleGraph(rootNodeSize, numberOfCombos, maxComboSize, random);

            var first = freshGraph.AllTraits.First.Value;
            var second = freshGraph.AllTraits.First.Next.Value;

            lookup = first.Trait.NameParts.Union(second.Trait.NameParts).ToArray();
        }

        [Benchmark]
        public void SimpleGraph()
        {
            // Find all traits.
            var result = simpleGraph.MatchTraits(lookup);

            if (result.OrderedTraits.Count == 0)
            {
                throw new InvalidOperationException();
            }
        }

        [Benchmark]
        public void FullGraph()
        {
            // Find all traits.
            var result = freshGraph.MatchTraits(lookup);

            if(result.OrderedTraits.Count == 0)
            {
                throw new InvalidOperationException();
            }
        }
        
        private TraitGraph BuildGraph(int rootTraitCount, int numberOfCombos, int maxComboSize, Random random)
        {
            var traitGraph = new TraitGraph(true);
            var alphabet = Enumerable.Range(0, rootTraitCount).Select(i => i.ToString());

            foreach(var item in alphabet)
            {
                traitGraph.AddOrExtendTrait(new Trait(item));
            }

            for(var comboNum = 0; comboNum < numberOfCombos; comboNum++)
            {
                // Determine combo size.
                var comboSize = random.Next(1, maxComboSize);
                var comboItems = new HashSet<string>();

                while(comboItems.Count < comboSize)
                {
                    var comboItem = random.Next(1, rootTraitCount - 1);

                    comboItems.Add(comboItem.ToString());                    
                }

                traitGraph.AddOrExtendTrait(new Trait(comboItems.ToArray()));
            }

            return traitGraph;
        }


        private SimpleTraitGraph BuildSimpleGraph(int rootTraitCount, int numberOfCombos, int maxComboSize, Random random)
        {
            var traitGraph = new SimpleTraitGraph();
            var alphabet = Enumerable.Range(0, rootTraitCount).Select(i => i.ToString());

            foreach (var item in alphabet)
            {
                traitGraph.AddOrExtendTrait(new Trait(item));
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

                traitGraph.AddOrExtendTrait(new Trait(comboItems.ToArray()));
            }

            return traitGraph;
        }
    }
}
