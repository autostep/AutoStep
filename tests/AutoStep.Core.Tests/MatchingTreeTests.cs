using System;
using System.Linq;
using AutoStep.Core.Elements;
using AutoStep.Core.Matching;
using AutoStep.Core.Sources;
using FluentAssertions;
using Xunit;

namespace AutoStep.Core.Tests
{
    public class MatchingTreeTests
    {
        private class TestDef : StepDefinition
        {
            public TestDef(StepType type, string declaration, params object[] parts)
                : base(type, declaration)
            {
                Definition = FakeDefElement.Make(parts);
            }
        }

        private class FakeDefElement : StepDefinitionElement
        {
            public static FakeDefElement Make(params object[] parts)
            {
                var defElement = new FakeDefElement();

                foreach (var part in parts)
                {
                    if (part is ArgumentType arg)
                    {
                        defElement.AddMatchingPart(arg);
                    }
                    else if (part is string str)
                    {
                        defElement.AddMatchingPart(str);
                    }
                    else
                    {
                        throw new ArgumentException("Bad make argument");
                    }
                }

                return defElement;
            }
        }

        private class FakeStepReference : StepReferenceElement
        {
            public static FakeStepReference Make(params object[] parts)
            {
                var refElement = new FakeStepReference();

                foreach (var part in parts)
                {
                    if (part is ArgumentType arg)
                    {
                        refElement.AddArgument(new StepArgumentElement { Type = arg });
                    }
                    else if (part is string str)
                    {
                        refElement.AddMatchingText(str);
                    }
                    else
                    {
                        throw new ArgumentException("Bad make argument");
                    }
                }

                return refElement;
            }
        }

        [Fact]
        public void CanAddSingleStepDefinition()
        {
            var stepDef = new TestDef(StepType.Given, "I have matched",
                                      "I", " ", "have", " ", "matched");

            var tree = new MatchingTree();

            tree.AddDefinition(stepDef);

            var stepRef = FakeStepReference.Make("I", " ", "have", " ", "matched");

            var list = tree.Match(stepRef, out var partsMatched);

            list.Should().HaveCount(1);
            partsMatched.Should().Be(5);
            list.First.Should().NotBeNull();
            list.First.Value.confidence.Should().Be(int.MaxValue);
            list.First.Value.def.Should().Be(stepDef);
        }

        [Fact]
        public void CanAddMultipleStepDefinitionsWithOverlapPartialMatch()
        {
            var stepDef1 = new TestDef(StepType.Given, "I have matched", 
                                       "I", " ", "have", " ", "matched");

            var stepDef2 = new TestDef(StepType.Given, "I have not matched",
                                       "I", " ", "have", " ", "not", " ", "matched");

            var tree = new MatchingTree();

            tree.AddDefinition(stepDef1);
            tree.AddDefinition(stepDef2);

            var stepRef1 = FakeStepReference.Make("I", " ", "have");

            var list = tree.Match(stepRef1, out var partsMatched).ToList();

            list.Should().HaveCount(2);
            partsMatched.Should().Be(3);
            list[0].confidence.Should().Be(4);
            list[0].def.Should().Be(stepDef1);
            list[1].confidence.Should().Be(4);
            list[1].def.Should().Be(stepDef2);
        }
        
        [Fact]
        public void CanAddMultipleStepDefinitionsWithOverlapExactMatch()
        {
            var stepDef1 = new TestDef(StepType.Given, "I have matched",
                                       "I", " ", "have", " ", "matched");

            var stepDef2 = new TestDef(StepType.Given, "I have not matched",
                                       "I", " ", "have", " ", "not", " ", "matched");

            var tree = new MatchingTree();

            tree.AddDefinition(stepDef1);
            tree.AddDefinition(stepDef2);

            var stepRef1 = FakeStepReference.Make("I", " ", "have", " ", "not", " ", "matched");

            var list = tree.Match(stepRef1, out var partsMatched).ToList();

            list.Should().HaveCount(1);
            partsMatched.Should().Be(7);
            list[0].confidence.Should().Be(int.MaxValue);
            list[0].def.Should().Be(stepDef2);
        }

        [Fact]
        public void CanAddMultipleStepDefinitionsWithOverlapPartialText()
        {
            var stepDef1 = new TestDef(StepType.Given, "I have matched",
                                       "I", " ", "have", " ", "matched");

            var stepDef2 = new TestDef(StepType.Given, "I have not matched",
                                       "I", " ", "have", " ", "not", " ", "matched");

            var tree = new MatchingTree();

            tree.AddDefinition(stepDef1);
            tree.AddDefinition(stepDef2);

            var stepRef1 = FakeStepReference.Make("I", " ", "ha");

            var list = tree.Match(stepRef1, out var partsMatched).ToList();

            list.Should().HaveCount(2);
            partsMatched.Should().Be(3);
            list[0].confidence.Should().Be(2);
            list[0].def.Should().Be(stepDef1);
            list[1].confidence.Should().Be(2);
            list[1].def.Should().Be(stepDef2);
        }

        [Fact]
        public void CanAddMultipleStepDefinitionsWithExactAndDepthBeyond()
        {
            var stepDef1 = new TestDef(StepType.Given, "I have matched",
                                       "I", " ", "have", " ", "matched");

            var stepDef2 = new TestDef(StepType.Given, "I have not matched",
                                       "I", " ", "have", " ", "not", " ", "matched");

            var stepDef3 = new TestDef(StepType.Given, "I have not matched properly",
                                       "I", " ", "have", " ", "not", " ", "matched", " ", "properly");

            var tree = new MatchingTree();

            tree.AddDefinition(stepDef1);
            tree.AddDefinition(stepDef2);
            tree.AddDefinition(stepDef3);

            var stepRef1 = FakeStepReference.Make("I", " ", "have", " ", "not", " ", "matched");

            var list = tree.Match(stepRef1, out var partsMatched).ToList();

            list.Should().HaveCount(2);
            partsMatched.Should().Be(7);
            list[0].confidence.Should().Be(int.MaxValue);
            list[0].def.Should().Be(stepDef2);
            list[1].confidence.Should().Be(7);
            list[1].def.Should().Be(stepDef3);
        }


    }
}
