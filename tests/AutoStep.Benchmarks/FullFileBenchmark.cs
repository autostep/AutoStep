using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Compiler;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

namespace AutoStep.Benchmarks
{
    public class FullFileBenchmark
    {
        private string fileContent;
        private ILoggerFactory logFactory;

        [GlobalSetup]
        public void Setup()
        {
            fileContent = FullFiles.Files.GeneralBenchmark;
            logFactory = new LoggerFactory();
        }

        [Benchmark]
        public void CompileFile()
        {
            var compiler = new AutoStepCompiler(logFactory);

            var compileResult = compiler.CompileAsync(new StringContentSource(fileContent)).Result;

            if(!compileResult.Success)
            {
                throw new ApplicationException("Compilation failed");
            }
        }
    }
}
