using System.Linq;
using AutoStep.Elements.Interaction;
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
            var nameMatcher = new TraitNameMatchingSet(GetNameParts(new[] { "C", "A", "B" }));

            nameMatcher.Contains(new NameRefElement { Name = "A" }).Should().BeTrue();
        }

        [Fact]
        public void MatchesMultiple()
        {
            var nameMatcher = new TraitNameMatchingSet(GetNameParts(new[] { "A", "B", "C", "D", "E" }));

            nameMatcher.Contains(GetNameParts(new[] { "A", "B", "E" })).Should().BeTrue();

            // Check for non-overlap.
            nameMatcher.Contains(GetNameParts(new[] { "D", "E", "F" })).Should().BeFalse();
            
            // Consume the remainder.
            nameMatcher.Contains(GetNameParts(new[] { "C", "D" })).Should().BeTrue();
        }

        private NameRefElement[] GetNameParts(params string[] nameParts)
        {
            return nameParts.Select(n => new NameRefElement { Name = n }).ToArray();
        }
    }
}
