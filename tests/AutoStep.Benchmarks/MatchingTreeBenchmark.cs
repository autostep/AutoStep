using System;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Elements.Parts;
using AutoStep.Elements.Test;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Language.Test.Matching;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using BenchmarkDotNet.Attributes;

namespace AutoStep.Benchmarks
{
    public class MatchingTreeBenchmark
    {
        private MatchingTree tree;
        private StepReferenceElement knownStepRef;
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

        [Params(10, 100, 1000, 10000)]
        public int SizeOfTree { get; set; }

        [Params(5, 10, 20, 40)]
        public int MaxTreeDepth { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            tree = new MatchingTree();

            // Pre-load the matching tree with a lot of definitions.
            // Seed the random so we always get the same set.
            var seededRandom = new Random(702561960);

            for(int idx = 0; idx < SizeOfTree; idx++)
            {
                // Determine the number of parts in the statement.
                var parts = seededRandom.Next(2, MaxTreeDepth);
                var stepDef = new StepDefinitionElement();

                for(int partIdx = 0; partIdx < parts; partIdx++)
                {
                    var isArgument = seededRandom.Next(0, 1) == 1;

                    if(isArgument)
                    {
                        stepDef.AddPart(new ArgumentPart());
                    }
                    else
                    {
                        // Probability of whitespace is approximately 50/50.
                        var whitespace = seededRandom.Next(0, 1) == 1;

                        if(!whitespace)
                        {
                            stepDef.AddPart(new WordDefinitionPart(words[seededRandom.Next(0, words.Length - 1)]));
                        }

                    }
                }

                var def = new TestDef(stepDef);

                tree.AddOrUpdateDefinition(def);
            }

            knownStepRef = CreateSimpleRef(StepType.Given, "I have not arg1 clicked the arg2 control");

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

            tree.AddOrUpdateDefinition(new TestDef(manualDef));
        }

        [Benchmark]
        public void LookupKnownStep()
        {
            var matches = tree.Match(knownStepRef, true, out var partsMatched);

            if(!matches.First.Value.IsExact)
            {
                throw new Exception("That's not right.");
            }
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

        private class TestDef : StepDefinition
        {
            private readonly string stepId;

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
