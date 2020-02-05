using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Interaction
{
    public class TraitDefinitionTests : InteractionsCompilerTestBase
    {
        public TraitDefinitionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void CanDefineEmptyTrait()
        {
            const string Test = @"
                Trait: clickable
            ";

            var result = CompileAndAssertSuccess(Test, cfg => { });
        }

    }
}
