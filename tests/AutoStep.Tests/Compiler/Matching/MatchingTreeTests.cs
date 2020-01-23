using System;
using System.Linq;
using AutoStep.Tests.Utils;
using AutoStep.Elements;
using FluentAssertions;
using Xunit;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;
using AutoStep.Elements.Parts;
using AutoStep.Tests.Builders;
using System.Threading.Tasks;
using AutoStep.Execution;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Contexts;

namespace AutoStep.Tests.Compiler.Matching
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
            var stepDef = new TestDef(CreateSimpleDef(StepType.Given, "I have matched"));

            var tree = new MatchingTree();

            tree.AddOrUpdateDefinition(stepDef);

            var stepRef = CreateSimpleRef(StepType.Given, "I have matched");

            var list = tree.Match(stepRef, true, out var partsMatched);

            list.Should().HaveCount(1);
            partsMatched.Should().Be(3);
            list.First.Should().NotBeNull();
            list.First.Value.IsExact.Should().BeTrue();
            list.First.Value.Confidence.Should().Be(int.MaxValue);
            list.First.Value.Definition.Should().Be(stepDef);
        }

        [Fact]
        public void CanRemoveSingleStepDefinition()
        {
            var stepDef = new TestDef(CreateSimpleDef(StepType.Given, "I have matched"));

            var tree = new MatchingTree();

            tree.AddOrUpdateDefinition(stepDef);

            var stepRef = CreateSimpleRef(StepType.Given, "I have matched");

            var list = tree.Match(stepRef, true, out var partsMatched);

            list.Should().HaveCount(1);

            tree.RemoveDefinition(stepDef);

            list = tree.Match(stepRef, true, out partsMatched);

            list.Should().BeEmpty();
        }


        [Fact]
        public void NoResultsReturnedForNoMatches()
        {
            var stepDef = new TestDef(CreateSimpleDef(StepType.Given, "I have matched"));

            var tree = new MatchingTree();

            tree.AddOrUpdateDefinition(stepDef);

            var stepRef = CreateSimpleRef(StepType.Given, "Not going to match");

            var list = tree.Match(stepRef, false, out var partsMatched);

            list.Should().HaveCount(0);
        }

        [Fact]
        public void CanAddMultipleStepDefinitionsWithOverlapPartialMatch()
        {
            var stepDef1 = new TestDef(CreateSimpleDef(StepType.Given, "I have matched"));

            var stepDef2 = new TestDef(CreateSimpleDef(StepType.Given, "I have not matched"));

            var tree = new MatchingTree();

            tree.AddOrUpdateDefinition(stepDef1);
            tree.AddOrUpdateDefinition(stepDef2);

            var stepRef1 = CreateSimpleRef(StepType.Given, "I have");

            var list = tree.Match(stepRef1, false, out var partsMatched).ToList();

            list.Should().HaveCount(2);
            partsMatched.Should().Be(2);
            list[0].IsExact.Should().BeFalse();
            list[0].Confidence.Should().Be(4);
            list[0].Definition.Should().Be(stepDef1);
            list[0].IsExact.Should().BeFalse();
            list[1].Confidence.Should().Be(4);
            list[1].Definition.Should().Be(stepDef2);
        }

        [Fact]
        public void CanAddMultipleStepDefinitionsWithOverlapExactMatch()
        {
            var stepDef1 = new TestDef(CreateSimpleDef(StepType.Given, "I have matched"));

            var stepDef2 = new TestDef(CreateSimpleDef(StepType.Given, "I have not matched"));

            var tree = new MatchingTree();

            tree.AddOrUpdateDefinition(stepDef1);
            tree.AddOrUpdateDefinition(stepDef2);

            var stepRef1 = CreateSimpleRef(StepType.Given, "I have not matched");

            var list = tree.Match(stepRef1, true, out var partsMatched).ToList();

            list.Should().HaveCount(1);
            partsMatched.Should().Be(4);
            list[0].IsExact.Should().BeTrue();
            list[0].Confidence.Should().Be(int.MaxValue);
            list[0].Definition.Should().Be(stepDef2);
        }

        [Fact]
        public void CanAddMultipleStepDefinitionsWithOverlapPartialText()
        {
            var stepDef1 = new TestDef(CreateSimpleDef(StepType.Given, "I have matched"));

            var stepDef2 = new TestDef(CreateSimpleDef(StepType.Given, "I have not matched"));

            var tree = new MatchingTree();

            tree.AddOrUpdateDefinition(stepDef1);
            tree.AddOrUpdateDefinition(stepDef2);

            var stepRef1 = CreateSimpleRef(StepType.Given, "I ha");

            var list = tree.Match(stepRef1, false, out var partsMatched).ToList();

            list.Should().HaveCount(2);
            partsMatched.Should().Be(2);
            list[0].IsExact.Should().BeFalse();
            list[0].Confidence.Should().Be(2);
            list[0].Definition.Should().Be(stepDef1);
            list[1].IsExact.Should().BeFalse();
            list[1].Confidence.Should().Be(2);
            list[1].Definition.Should().Be(stepDef2);
        }


        [Fact]
        public void CanRemoveStepDefinitionsWithOtherDeeperNode()
        {
            var stepDef1 = new TestDef(CreateSimpleDef(StepType.Given, "I have matched"));

            var stepDef2 = new TestDef(CreateSimpleDef(StepType.Given, "I have not matched"));

            var tree = new MatchingTree();

            tree.AddOrUpdateDefinition(stepDef1);
            tree.AddOrUpdateDefinition(stepDef2);

            var stepRef1 = CreateSimpleRef(StepType.Given, "I ha");

            var list = tree.Match(stepRef1, false, out var partsMatched);

            list.Should().HaveCount(2);

            tree.RemoveDefinition(stepDef1);

            list = tree.Match(stepRef1, false, out partsMatched);

            list.Should().HaveCount(1);
            list.First.Value.Definition.Should().Be(stepDef2);
        }

        [Fact]
        public void CanReplaceExistingStepDefinition()
        {
            var stepDef1 = new TestDef("1", CreateSimpleDef(StepType.Given, "I have matched"));

            var stepDef2 = new TestDef("2", CreateSimpleDef(StepType.Given, "I have not matched"));

            var stepDefReplace = new TestDef("1", CreateSimpleDef(StepType.Given, "I have matched"));

            var tree = new MatchingTree();

            var stepRef1 = CreateSimpleRef(StepType.Given, "I have matched");

            tree.AddOrUpdateDefinition(stepDef1);
            tree.AddOrUpdateDefinition(stepDef2);

            tree.Match(stepRef1, true, out var partsMatched).First.Value.Definition.Should().Be(stepDef1);

            tree.AddOrUpdateDefinition(stepDefReplace);

            tree.Match(stepRef1, true, out partsMatched).First.Value.Definition.Should().Be(stepDefReplace);
        }

        [Fact]
        public void CanAddMultipleStepDefinitionsWithExactAndDepthBeyond()
        {
            var stepDef1 = new TestDef(CreateSimpleDef(StepType.Given, "I have matched"));

            var stepDef2 = new TestDef(CreateSimpleDef(StepType.Given, "I have not matched"));

            var stepDef3 = new TestDef(CreateSimpleDef(StepType.Given, "I have not matched properly"));

            var tree = new MatchingTree();

            tree.AddOrUpdateDefinition(stepDef1);
            tree.AddOrUpdateDefinition(stepDef2);
            tree.AddOrUpdateDefinition(stepDef3);

            var stepRef1 = CreateSimpleRef(StepType.Given, "I have not matched");

            var list = tree.Match(stepRef1, false, out var partsMatched).ToList();

            list.Should().HaveCount(2);
            partsMatched.Should().Be(4);
            list[0].IsExact.Should().BeTrue();
            list[0].Confidence.Should().Be(int.MaxValue);
            list[0].Definition.Should().Be(stepDef2);
            list[1].IsExact.Should().BeFalse();
            list[1].Confidence.Should().Be(7);
            list[1].Definition.Should().Be(stepDef3);
        }

        [Fact]
        public void LargeTree()
        {
            var tree = new MatchingTree();

            // Pre-load the matching tree with a lot of definitions.
            // Seed the random so we always get the same set.
            var seededRandom = new Random(702561960);

            for (var idx = 0; idx < 2000; idx++)
            {
                // Determine the number of parts in the statement.
                var parts = seededRandom.Next(2, 20);
                var stepDef = new StepDefinitionElement();

                for (var partIdx = 0; partIdx < parts; partIdx++)
                {
                    var isArgument = seededRandom.Next(0, 1) == 1;

                    if (isArgument)
                    {
                        stepDef.AddPart(new ArgumentPart());
                    }
                    else
                    {
                        // Probability of whitespace is approximately 50/50.
                        var whitespace = seededRandom.Next(0, 1) == 1;

                        if (!whitespace)
                        {
                            stepDef.AddPart(new WordDefinitionPart(words[seededRandom.Next(0, words.Length - 1)]));
                        }

                    }
                }

                var def = new TestDef(stepDef);

                tree.AddOrUpdateDefinition(def);
            }

            var knownStepRef = CreateSimpleRef(StepType.Given, "I have not arg1 clicked the arg2 control");

            // Add a 'known' definition.
            var manualDef = CreateDef(StepType.Given, "I have not {arg1} clicked the {arg2} control", s => s
                                            .WordPart("I", 1)
                                            .WordPart("have", 3)
                                            .WordPart("not", 8)
                                            .Argument("{arg1}", "arg1", 12)
                                            .WordPart("clicked", 19)
                                            .WordPart("the", 27)
                                            .Argument("{arg2}", "arg2", 31)
                                            .WordPart("control", 38)
                                     );

            var testDef = new TestDef(manualDef);
            tree.AddOrUpdateDefinition(testDef);

            var matches = tree.Match(knownStepRef, false, out var partsMatched).ToArray();

            matches.Should().HaveCount(1);
            matches[0].Confidence.Should().Be(int.MaxValue);
            matches[0].IsExact.Should().BeTrue();
            matches[0].Definition.Should().Be(testDef);
            partsMatched.Should().Be(8);
        }

        private StepDefinitionElement CreateSimpleDef(StepType type, string declaration)
        {
            var defBuilder = new StepDefinitionBuilder(type, declaration, 1, 1);

            var position = 1;

            foreach (var item in declaration.Split(' '))
            {
                defBuilder.WordPart(item, position);
                position += item.Length + 1;
            }

            return defBuilder.Built;
        }

        private StepDefinitionElement CreateDef(StepType type, string declaration, Action<StepDefinitionBuilder> builder)
        {
            var defBuilder = new StepDefinitionBuilder(type, declaration, 1, 1);

            builder(defBuilder);

            return defBuilder.Built;
        }

        private StepReferenceElement CreateSimpleRef(StepType type, string text)
        {
            var refBuilder = new StepReferenceBuilder(text, type, type, 1, 1);
            
            foreach (var item in text.Split(' '))
            {
                refBuilder.Text(item);
            }

            refBuilder.Built.FreezeTokens();

            return refBuilder.Built;
        }

        private StepReferenceElement CreateRef(StepType type, string text, Action<StepReferenceBuilder> builder)
        {
            var refBuilder = new StepReferenceBuilder(text, type, type, 1, 1);

            builder(refBuilder);

            refBuilder.Built.FreezeTokens();

            return refBuilder.Built;
        }

        private class TestDef : StepDefinition
        {
            private string stepId;

            public TestDef(StepDefinitionElement definition) : base(TestStepDefinitionSource.Blank, definition.Type, definition.Declaration)
            {
                Definition = definition;
            }

            public TestDef(string stepId, StepDefinitionElement definition) : base(TestStepDefinitionSource.Blank, definition.Type, definition.Declaration)
            {
                this.stepId = stepId;
                Definition = definition;
            }

            public override ValueTask ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
            {
                throw new NotImplementedException();
            }

            public override bool IsSameDefinition(StepDefinition def)
            {
                if (def is TestDef testDef)
                {
                    return testDef.stepId == stepId;
                }

                return false;
            }
        }
    }
}
