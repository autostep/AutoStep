using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Test.Parsing
{
    public class EndOfFileTests : CompilerTestBase
    {
        public EndOfFileTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task EmptyFileNoError()
        {
            const string TestFile =
            @"";

            await CompileAndAssertSuccess(TestFile, cfg => { });
        }

        [Fact]
        public async Task EofAfterFeature()
        {
            const string TestFile =
            @"Feature: My Feature";

            await CompileAndAssertWarnings(TestFile, cfg => cfg.Feature("My Feature", 1, 1),
                                           LanguageMessageFactory.Create(null, CompilerMessageLevel.Warning, CompilerMessageCode.NoScenarios, 1, 1, 1, 19, "My Feature"));
        }

        [Fact]
        public async Task EofAfterScenario()
        {
            const string TestFile =
            @"Feature: My Feature

               Scenario: My Scenario";

            await CompileAndAssertSuccess(TestFile, cfg => cfg
                    .Feature("My Feature", 1, 1, f => f
                        .Scenario("My Scenario", 3, 16)
                    ));
        }
        
        [Fact]
        public async Task EofDuringStep()
        {
            const string TestFile =
            @"Feature: My Feature

               Scenario: My Scenario

                   Given I have 'sad";

            await CompileAndAssertSuccess(TestFile, cfg => cfg
                    .Feature("My Feature", 1, 1, f => f
                        .Scenario("My Scenario", 3, 16, s => s
                            .Given("I have 'sad", 5, 20, r => r.Text("I").Text("have").Quote().Text("sad"))
                        )
                    ));
        }

    }
}
