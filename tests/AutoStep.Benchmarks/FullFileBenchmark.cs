using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Compiler;
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
            var compiler = new AutoStepCompiler();

            var compileResult = compiler.CompileAsync(new StringContentSource(fileContent)).Result;

            if(!compileResult.Success)
            {
                throw new ApplicationException("Compilation failed");
            }
        }
    }
}
