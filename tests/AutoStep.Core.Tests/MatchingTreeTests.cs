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
        private readonly string[] words = new[]
        {
            "I",
            "it",
            "the",
            "you",
            "this",
            "have",
            "not",
            "and",
            "should",
            "to",
            "on",
            "off",
            "is",
            "after",
            "next",
            "first",
            "last",
            "second",
            "grid",
            "table",
            "will",
            "that",
            "clicked",
            "enable",
            "disable",
            "value",
            "control",
            "before",
            "previous",
        };


        [Fact]
        public void CanAddSingleStepDefinition()
        {
            var stepDef = new TestDef(StepType.Given, "I have matched",
                                      "I", " ", "have", " ", "matched");

            var tree = new MatchingTree();

            tree.AddDefinition(stepDef);

            var stepRef = FakeStepReference.Make(StepType.Given, "I", " ", "have", " ", "matched");

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

            var stepRef1 = FakeStepReference.Make(StepType.Given, "I", " ", "have");

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

            var stepRef1 = FakeStepReference.Make(StepType.Given, "I", " ", "have", " ", "not", " ", "matched");

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

            var stepRef1 = FakeStepReference.Make(StepType.Given, "I", " ", "ha");

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

            var stepRef1 = FakeStepReference.Make(StepType.Given, "I", " ", "have", " ", "not", " ", "matched");

            var list = tree.Match(stepRef1, out var partsMatched).ToList();

            list.Should().HaveCount(2);
            partsMatched.Should().Be(7);
            list[0].confidence.Should().Be(int.MaxValue);
            list[0].def.Should().Be(stepDef2);
            list[1].confidence.Should().Be(7);
            list[1].def.Should().Be(stepDef3);
        }

        [Fact]
        public void LargeTree()
        {
            var tree = new MatchingTree();

            // Pre-load the matching tree with a lot of definitions.
            // Seed the random so we always get the same set.
            var seededRandom = new Random(702561960);

            for (int idx = 0; idx < 2000; idx++)
            {
                // Determine the number of parts in the statement.
                var parts = seededRandom.Next(2, 20);
                var stepDef = new StepDefinitionElement();

                for (int partIdx = 0; partIdx < parts; partIdx++)
                {
                    var isArgument = seededRandom.Next(0, 1) == 1;

                    if (isArgument)
                    {
                        stepDef.AddMatchingPart(ArgumentType.Text);
                    }
                    else
                    {
                        // Probability of whitespace is approximately 50/50.
                        var whitespace = seededRandom.Next(0, 1) == 1;

                        if (whitespace)
                        {
                            stepDef.AddMatchingPart(" ");
                        }
                        else
                        {
                            stepDef.AddMatchingPart(words[seededRandom.Next(0, words.Length - 1)]);
                        }

                    }
                }

                var def = new TestDef(stepDef);

                tree.AddDefinition(def);
            }

            var knownStepRef = FakeStepReference.Make(StepType.Given, "I", " ", "have", " ", "not", " ", ArgumentType.Text,
                                                     " ", "clicked", " ", "the", " ", "custom", " ", ArgumentType.Text, "control");

            // Add a 'known' definition.
            var manualDef = new TestDef(FakeDefElement.Make(knownStepRef));

            tree.AddDefinition(manualDef);

            var matches = tree.Match(knownStepRef, out var partsMatched).ToArray();

            matches.Should().HaveCount(1);
            matches[0].confidence.Should().Be(int.MaxValue);
            matches[0].def.Should().Be(manualDef);
            partsMatched.Should().Be(16);
        }


        private class TestDef : StepDefinition
        {
            public TestDef(StepDefinitionElement definition) : base(definition.Type, definition.Declaration)
            {
                Definition = definition;
            }

            public TestDef(StepType type, string declaration, params object[] parts)
                : base(type, declaration)
            {
                Definition = FakeDefElement.Make(type, parts);
            }
        }

        private class FakeDefElement : StepDefinitionElement
        {
            public FakeDefElement(StepType type)
            {
                Type = type;
            }

            public static FakeDefElement Make(StepReferenceElement refElement)
            {
                var defElement = new FakeDefElement(refElement.BindingType.Value);
                defElement.UpdateFromStepReference(refElement);
                return defElement;
            }

            public static FakeDefElement Make(StepType type, params object[] parts)
            {
                var defElement = new FakeDefElement(type);

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
            public static FakeStepReference Make(StepType type, params object[] parts)
            {
                var refElement = new FakeStepReference { BindingType = type };

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
    }
}
