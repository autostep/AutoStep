using System;
using AutoStep.Core.Elements;
using AutoStep.Core.Matching;
using AutoStep.Core.Sources;
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

        [Fact]
        public void CanAddSingleStepDefinition()
        {
            var stepDef = new TestDef(StepType.Given, "I have matched",
                                      "I", " ", "have", " ", "matched");

            var tree = new MatchingTree();

            tree.AddDefinition(stepDef);
        }

        [Fact]
        public void CanAddMultipleStepDefinitionsWithOverlap()
        {
            var stepDef1 = new TestDef(StepType.Given, "I have matched", 
                                       "I", " ", "have", " ", "matched");

            var stepDef2 = new TestDef(StepType.Given, "I have not matched",
                                       "I", " ", "have", " ", "not", " ", "matched");

            var tree = new MatchingTree();

            tree.AddDefinition(stepDef1);
            tree.AddDefinition(stepDef2);
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
        }
    }
}
