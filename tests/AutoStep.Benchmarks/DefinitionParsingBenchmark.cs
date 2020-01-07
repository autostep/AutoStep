using System;
using AutoStep.Compiler;
using AutoStep.Compiler.Matching;
using BenchmarkDotNet.Attributes;

namespace AutoStep.Benchmarks
{
    public class DefinitionParsingBenchmark
    {
        private AutoStepCompiler compiler;
        private AutoStepLinker linker;

        [GlobalSetup]
        public void Setup()
        {
            compiler = new AutoStepCompiler();
            linker = new AutoStepLinker(compiler);
        }
        
        [Benchmark]
        public void NoArguments()
        {
            const string TestStep = "I have done something";

            var matched = linker.GetStepDefinitionElementFromStatementBody(StepType.Given, TestStep);

            if(!matched.Success)
            {
                throw new ApplicationException("Parse failed");
            }
        }

        [Benchmark]
        public void WithArguments()
        {
            const string TestStep = "I have done something with 'argument1'";

            var matched = linker.GetStepDefinitionElementFromStatementBody(StepType.Given, TestStep);

            if (!matched.Success)
            {
                throw new ApplicationException("Parse failed");
            }
        }
    }
}
