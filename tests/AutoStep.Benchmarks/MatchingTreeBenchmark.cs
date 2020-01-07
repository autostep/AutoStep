using System;
using System.Collections.Generic;
using AutoStep;
using AutoStep.Elements;
using AutoStep.Matching;
using AutoStep.Sources;
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
                        stepDef.AddMatchingPart(ArgumentType.Text);
                    }
                    else
                    {
                        // Probability of whitespace is approximately 50/50.
                        var whitespace = seededRandom.Next(0, 1) == 1;

                        if(whitespace)
                        {
                            stepDef.AddMatchingPart(" ");
                        }
                        else
                        {
                            stepDef.AddMatchingPart(words[seededRandom.Next(0, words.Length - 1)]);
                        }

                    }
                }

                var def = new TestStepDef(stepDef);

                tree.AddDefinition(def);
            }

            knownStepRef = FakeStepReference.Make(StepType.Given, "I", " ", "have", " ", "not", " ", ArgumentType.Text,
                                                  " ", "clicked", " ", "the", " ", ArgumentType.Text, "control");

            // Add a 'known' definition.
            var manualDef = FakeDefElement.Make(knownStepRef);

            tree.AddDefinition(new TestStepDef(manualDef));
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
    }
}
