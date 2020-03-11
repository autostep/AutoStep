using AutoStep.Language.Interaction.Traits;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Interaction
{
    public class TraitNameMatchingSetTests
    {
        [Fact]
        public void MatchesSingleName()
        {
            var nameMatcher = new TraitNameMatchingSet(new[] { "C", "A", "B" });

            nameMatcher.Contains("A").Should().BeTrue();
        }

        [Fact]
        public void MatchesMultiple()
        {
            var nameMatcher = new TraitNameMatchingSet(new [] { "A", "B", "C", "D", "E" });

            nameMatcher.Contains(new[] { "A", "B", "E" }).Should().BeTrue();

            // Check for non-overlap.
            nameMatcher.Contains(new[] { "D", "E", "F" }).Should().BeFalse();
            
            // Consume the remainder.
            nameMatcher.Contains(new[] { "C", "D" }).Should().BeTrue();
        }
    }
}
