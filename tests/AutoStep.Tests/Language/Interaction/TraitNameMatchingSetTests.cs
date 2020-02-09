using AutoStep.Language.Interaction.Traits;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Interaction
{
    public class TraitNameMatchingSetTests
    {
        [Fact]
        public void CheckConsumesSingleName()
        {
            var nameMatcher = new TraitNameMatchingSet(new[] { "A", "B" });
            var currentMask = 0ul;

            nameMatcher.AnyLeft(currentMask).Should().BeTrue();

            nameMatcher.ConsumeIfContains("A", ref currentMask).Should().BeTrue();

            currentMask.Should().Be(0b01);

            nameMatcher.AnyLeft(currentMask).Should().BeTrue();

            nameMatcher.ConsumeIfContains("B", ref currentMask).Should().BeTrue();

            currentMask.Should().Be(0b11);

            nameMatcher.AnyLeft(currentMask).Should().BeFalse();
        }

        [Fact]
        public void CheckConsumesSet()
        {
            var nameMatcher = new TraitNameMatchingSet(new[] { "A", "B", "C", "D", "E" });
            var currentMask = 0ul;

            nameMatcher.AnyLeft(currentMask).Should().BeTrue();

            nameMatcher.ConsumeIfContains(new[] { "A", "B", "E" }, ref currentMask).Should().BeTrue();

            currentMask.Should().Be(0b10011);

            nameMatcher.AnyLeft(currentMask).Should().BeTrue();

            // Check for non-overlap.
            nameMatcher.ConsumeIfContains(new[] { "D", "E", "F" }, ref currentMask).Should().BeFalse();

            // No change in the mask.
            currentMask.Should().Be(0b10011);

            // Already consumed, should not consume.
            nameMatcher.ConsumeIfContains(new[] { "A", "B", "E" }, ref currentMask).Should().BeFalse();

            // No change in the mask.
            currentMask.Should().Be(0b10011);

            // Consume the remainder.
            nameMatcher.ConsumeIfContains(new[] { "C", "D" }, ref currentMask).Should().BeTrue();

            currentMask.Should().Be(0b11111);

            nameMatcher.AnyLeft(currentMask).Should().BeFalse();
        }
    }
}
