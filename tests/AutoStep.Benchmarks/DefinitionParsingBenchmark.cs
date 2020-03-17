using System;
using AutoStep.Language.Test;
using BenchmarkDotNet.Attributes;

namespace AutoStep.Benchmarks
{
    public class DefinitionParsingBenchmark
    {
        private TestCompiler compiler = null!;

        [GlobalSetup]
        public void Setup()
        {
            compiler = new TestCompiler();
        }

        [Benchmark]
        public void NoArguments()
        {
            const string TestStep = "I have done something";

            var matched = compiler.CompileStepDefinitionElementFromStatementBody(StepType.Given, TestStep);

            if(!matched.Success)
            {
                throw new ApplicationException("Parse failed");
            }
        }

        [Benchmark]
        public void WithArguments()
        {
            const string TestStep = "I have done something with {argument1}";

            var matched = compiler.CompileStepDefinitionElementFromStatementBody(StepType.Given, TestStep);

            if (!matched.Success)
            {
                throw new ApplicationException("Parse failed");
            }
        }
    }
}
