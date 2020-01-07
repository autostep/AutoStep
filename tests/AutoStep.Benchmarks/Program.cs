using System;
using System.Diagnostics;
using BenchmarkDotNet.Running;

namespace AutoStep.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            new BenchmarkSwitcher(AllBenchmarks).Run(args, new BenchmarkConfig());
        }

        private static readonly Type[] AllBenchmarks =
        {
            typeof(FullFileBenchmark),
            typeof(DefinitionParsingBenchmark),
            typeof(MatchingTreeBenchmark)
        };
    }
}
