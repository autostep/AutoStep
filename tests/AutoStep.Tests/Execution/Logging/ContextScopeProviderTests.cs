using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Logging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AutoStep.Tests.Execution.Logging
{
    public class ContextScopeProviderTests
    {
        [Fact]
        public async Task ContextMaintainedInNestedAsyncScopes()
        {
            var scopeProvider = new ContextScopeProvider();

            scopeProvider.Current.Should().BeNull();

            var context1 = new RunContext(new ConfigurationBuilder().Build());

            using (scopeProvider.EnterContextScope(context1))
            {
                scopeProvider.Current.Should().BeSameAs(context1);

                await Task.Delay(1);

                scopeProvider.Current.Should().BeSameAs(context1);

                await NestedCall(scopeProvider, context1);

                scopeProvider.Current.Should().BeSameAs(context1);
            }

            scopeProvider.Current.Should().BeNull();
        }

        private async Task NestedCall(ContextScopeProvider provider, TestExecutionContext outerContext)
        {
            await Task.Delay(1);

            var context2 = new ThreadContext(1);

            provider.Current.Should().BeSameAs(outerContext);

            using (provider.EnterContextScope(context2))
            {
                provider.Current.Should().BeSameAs(context2);

                await Task.Delay(1);

                provider.Current.Should().BeSameAs(context2);
            }

            provider.Current.Should().BeSameAs(outerContext);
        }
    }
}
