using System;
using AutoStep.Compiler;
using AutoStep.Compiler.Matching;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

namespace AutoStep.Benchmarks
{
    public class DefinitionParsingBenchmark
    {
        private AutoStepCompiler compiler;

        [GlobalSetup]
        public void Setup()
        {
            compiler = new AutoStepCompiler(new LoggerFactory());
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
