using System;
using BenchmarkDotNet.Running;

namespace AutoStep.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var bench = new TraitGraphBenchmarks();
            ////bench.Complexity = 16;

            //bench.Setup();

            //bench.FindInGraph();

            new BenchmarkSwitcher(AllBenchmarks).Run(args, new BenchmarkConfig());
        }

        private static readonly Type[] AllBenchmarks =
        {
            typeof(FullFileBenchmark),
            typeof(DefinitionParsingBenchmark),
            typeof(MatchingTreeBenchmark),
            typeof(TraitGraphBenchmarks)
        };
    }
}
