using System.Linq;
using AutoStep.Language;
using FluentAssertions;

namespace AutoStep.Tests.Utils
{
    static class LineTokenAssertionExtensions
    {
        public static void AssertToken<TStateIndicator>(this LineTokeniseResult<TStateIndicator> result, int idx, int column, LineTokenCategory category, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
            where TStateIndicator : struct
        {
            var tokenList = result.Tokens.ToList();

            tokenList[idx].Category.Should().Be(category, "token {0} should have the required category", idx);
            tokenList[idx].SubCategory.Should().Be(subCategory, "token {0} should have the required sub-category", idx);
            tokenList[idx].StartPosition.Should().Be(column, "token {0} should have the required column", idx);
        }
    }
}
