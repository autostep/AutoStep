using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Interaction
{
    public class FullFileTests : InteractionsCompilerTestBase
    {
        public FullFileTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task CanParseFullFile()
        {
            var testContent = FullFiles.Files.FullInteractionsFile;

            await CompileAndAssertSuccess(testContent, cfg => { });
        }
    }
}
