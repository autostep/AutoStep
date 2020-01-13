using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Elements.Parts;
using BenchmarkDotNet.Attributes;

namespace AutoStep.Benchmarks
{
    public class MatchingTreeBenchmark
    {
        private MatchingTree tree;
        private FakeStepReference knownStepRef;
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
                        stepDef.AddPart(new ArgumentPart { Text = "n" });
                    }
                    else
                    {
                        // Probability of whitespace is approximately 50/50.
                        var whitespace = seededRandom.Next(0, 1) == 1;

                        if(!whitespace)
                        {
                            stepDef.AddPart(new WordDefinitionPart() { Text = words[seededRandom.Next(0, words.Length - 1)] });
                        }

                    }
                }

                var def = new TestStepDef(stepDef);

                tree.AddOrUpdateDefinition(def);
            }

            knownStepRef = FakeStepReference.Make(StepType.Given, "I", "have", "not", "arg1",
                                                  "clicked", "the", "arg2", "control");

            // Add a 'known' definition.
            var manualDef = FakeDefElement.Make(StepType.Given, "I", "have", "not", ArgumentType.Text, 
                                                "clicked", "the", ArgumentType.NumericInteger, "control");

            tree.AddOrUpdateDefinition(new TestStepDef(manualDef));
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

        private class TestSource : IStepDefinitionSource
        {
            public string Uid => throw new NotImplementedException();

            public string Name => throw new NotImplementedException();

            public DateTime GetLastModifyTime()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<StepDefinition> GetStepDefinitions()
            {
                throw new NotImplementedException();
            }
        }

        private class TestStepDef : StepDefinition
        {
            public TestStepDef(StepDefinitionElement definition) : base(new TestSource(), definition.Type, definition.Declaration)
            {
                Definition = definition;
            }

            public override bool IsSameDefinition(StepDefinition def)
            {
                return ReferenceEquals(this, def);
            }
        }

        private class FakeStepReference : StepReferenceElement
        {
            public static FakeStepReference Make(StepType type, params object[] parts)
            {
                var refElement = new FakeStepReference();
                refElement.BindingType = type;
                refElement.RawText = string.Join(' ', parts);
                var currentIndex = 1;

                foreach (var part in parts)
                {
                    if (part is string str)
                    {
                        var lastIndex = currentIndex + str.Length - 1;
                        refElement.AddPart(new WordPart() { TextRange = new Range(currentIndex,  lastIndex)});
                        // Along 1 to move to the space, and another to move to the start of the next word.
                        currentIndex += 2;
                    }
                    else
                    {
                        throw new ArgumentException("Bad make argument");
                    }
                }

                return refElement;
            }
        }

        private class FakeDefElement : StepDefinitionElement
        {
            public FakeDefElement(StepType type)
            {
                Type = type;
            }

            public static FakeDefElement Make(StepType type, params object[] parts)
            {
                var defElement = new FakeDefElement(type);

                foreach (var part in parts)
                {
                    if (part is string str)
                    {
                        defElement.AddPart(new WordDefinitionPart() { Text = str });
                    }
                    else if(part is ArgumentType argType)
                    {
                        defElement.AddPart(new ArgumentPart { Name = "n", TypeHint = argType });
                    }
                    else
                    {
                        throw new ArgumentException("Bad make argument");
                    }
                }

                return defElement;
            }
        }
    }
}
