using AutoStep.Compiler.Tests.Utils;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AutoStep.Compiler.Tests
{
    public class FeatureCompileTests : CompilerTestBase
    {
        [Fact]
        public async Task FeatureWithNoScenariosProducesWarning()
        {
            const string TestFile = 
            @"                
              Feature: My Feature
                Description words

            ";

            await CompileAndAssertMessage(TestFile, new CompilerMessage
            {
                Level = CompilerMessageLevel.Warning,
                Code = CompilerMessageCode.ASC00001,
                Message = "Feature 'My Feature' contains no scenarios.",
                LineNo = 2,
                Column = 15
            });
        }
        
        [Fact]
        public async Task FeatureWithSingleScenarioPasses()
        {
            const string TestFile =
            @"
                Feature: My Feature
                    Feature Description

                    Scenario: My Scenario

                        Given I have clicked on
                        And I have gone to

            ";

            await CompileSuccessNoWarnings(TestFile);
        }
    }
}
