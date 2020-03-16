using System;
using AutoStep.Language;
using AutoStep.Language.Test;
using BenchmarkDotNet.Attributes;

namespace AutoStep.Benchmarks
{
    public class FullFileBenchmark
    {
        private string fileContent;

        [GlobalSetup]
        public void Setup()
        {
            fileContent = FullFiles.Files.GeneralBenchmark;
        }

        [Benchmark]
        public void CompileFile()
        {
            var compiler = new TestCompiler();

            var compileResult = compiler.CompileAsync(new StringContentSource(fileContent)).Result;

            if(!compileResult.Success)
            {
                throw new ApplicationException("Compilation failed");
            }
        }
    }
}
