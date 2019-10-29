using AutoStep.Compiler.Tests.Utils;
using System;
using Xunit;

namespace AutoStep.Compiler.Tests
{
    public class FeatureCompileTests : CompilerTestBase
    {
        [Fact]
        public void FeatureWithNoScenariosProducesWarning()
        {
            const string TestFile = 
            @"                
              Feature: My Feature
                Description words

            ";

            CompileAndAssertMessage(TestFile, new CompilerMessage
            {
                Level = CompilerMessageLevel.Warning,
                Code = CompilerMessageCode.AS00001,
                Message = "Feature 'My Feature' contains no scenarios.",
                LineNo = 2,
                Column = 15
            });
        }
    }
}
