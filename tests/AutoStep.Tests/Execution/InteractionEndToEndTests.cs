using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Definitions;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Events;
using AutoStep.Projects;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Execution.Interaction;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction;
using System.Linq;
using System.Configuration;
using AutoStep.Definitions.Interaction;
using AutoStep.Assertion;

namespace AutoStep.Tests.Execution
{
    public class InteractionEndToEndTests : LoggingTestBase
    {
        public InteractionEndToEndTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task SimpleEndToEndTest()
        {
            const string InteractionsFile =
            @"
                Trait: named
                    
                    locateNamed(name): needs-defining

                Trait: clickable + named

                    Step: Given I have clicked the {name} $component$
                        locateNamed(name) 
                          -> click()

                Component: button

                    traits: clickable, named

                    locateNamed(name): select('button')
            ";

            // Compile a file.
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have clicked the Submit button
            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));

            project.TryAddFile(new ProjectInteractionFile("/comp", new StringContentSource(InteractionsFile)));

            var selectCalled = false;
            var clickCalled = false;

            project.Compiler.Interactions.AddOrReplaceMethod("select", (IServiceScope scope, MethodContext ctxt, string selector) =>
            {
                ctxt.ChainValue.Should().BeNull();
                ctxt.ChainValue = "element";
                selectCalled = true;
            });

            project.Compiler.Interactions.AddOrReplaceMethod("click", (IServiceScope scope, MethodContext ctxt) =>
            {
                ctxt.ChainValue.Should().Be("element");
                clickCalled = true;
            });
                        
            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            compileResult.Messages.Should().BeEmpty();

            var linkResult = project.Compiler.Link();

            linkResult.Messages.Should().BeEmpty();

            var testRun = project.CreateTestRun();

            await testRun.ExecuteAsync(LogFactory);

            selectCalled.Should().BeTrue();
            clickCalled.Should().BeTrue();
        }

        [Fact]
        public async Task FieldLocationByLabel()
        {
            const string InteractionsFile =
            @"
                Trait: named
                    
                    locateNamed(name): needs-defining

                    Step: Then the {name} $component$ should exist
                        locateNamed(name)
                          -> assertExists()

                Trait: labeled + named

                    locateLabel(name): needs-defining

                    locateNamed(name): locateLabel(name)
                                       -> attributeToVariable('for', 'id1')
                                       -> selectById(id1)

                    Step: Then the label for the {name} $component$ should exist
                        locateLabel(name)
                          -> assertExists()

                Component: field

                    traits: named, labeled

                    locateLabel(name): select('label') -> withText(name)
            ";

            // Compile a file.
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Then the label for the Name field should exist
                     And the Name field should exist
            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));

            project.TryAddFile(new ProjectInteractionFile("/comp", new StringContentSource(InteractionsFile)));

            var selectCalled = false;
            var selectByIdCalled = false;
            var assertExistsCount = 0;

            var testElements = new[]
            {
                new TestElement { Type = "label", Text = "Name", For = "name" },
                new TestElement { Type = "label", Text = "Age", For = "age" },
                new TestElement { Type = "input", Id = "name" },
                new TestElement { Type = "input", Id = "age" }
            };

            project.Compiler.Interactions.AddOrReplaceMethod("select", (MethodContext ctxt, string selector) =>
            {
                ctxt.ChainValue.Should().BeNull();
                ctxt.ChainValue = testElements.Where(c => c.Type == selector);
                selectCalled = true;
            });

            project.Compiler.Interactions.AddOrReplaceMethod("selectById", (MethodContext ctxt, string id) =>
            {
                ctxt.ChainValue = testElements.Where(c => c.Id == id);
                selectByIdCalled = true;
            });

            project.Compiler.Interactions.AddOrReplaceMethod("withText", (MethodContext ctxt, string text) =>
            {
                // The chain contains an IEnumerable of TestElement.
                var elements = ctxt.ChainValue as IEnumerable<TestElement>;

                ctxt.ChainValue = elements.Where(x => x.Text == text);
            });

            project.Compiler.Interactions.AddOrReplaceMethod("assertExists", (MethodContext ctxt) =>
            {
                var elements = ctxt.ChainValue as IEnumerable<TestElement>;

                if(elements.Any())
                {
                    // Pass
                    assertExistsCount++;
                }
                else
                {
                    // Fail
                    throw new AssertionException("Does not exist.");
                }
            });

            project.Compiler.Interactions.AddOrReplaceMethod(new AttributeToVariableMethod());

            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            compileResult.Messages.Should().BeEmpty();

            var linkResult = project.Compiler.Link();

            linkResult.Messages.Should().BeEmpty();

            var testRun = project.CreateTestRun();

            await testRun.ExecuteAsync(LogFactory);

            selectCalled.Should().BeTrue();
            selectByIdCalled.Should().BeTrue();

            // Assert should have been called twice.
            assertExistsCount.Should().Be(2);
        }

        [Fact]
        public async Task InheritedComponentsTest()
        {
            const string InteractionsFile =
            @"
                Trait: named

                    locateNamed(name): needs-defining

                    Step: Then the {name} $component$ should exist
                        locateNamed(name) -> assertExists()

                Trait: named + clickable

                    Step: Given I have clicked the {name} $component$
                       locateNamed(name) -> click()

                Component: field

                    traits: named, clickable

                    locateNamed(name): select('input[<name>]')

                Component: text

                    name: 'text box'
                    inherits: field

                    locateNamed(name): select('input[type=text][<name>]')
            ";

            // Compile a file.
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have clicked the Name field
                      And I have clicked the Age text box
                      
                    Then the Name field should exist
                     And the Age text box should exist
            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));

            project.TryAddFile(new ProjectInteractionFile("/comp", new StringContentSource(InteractionsFile)));

            var actions = new List<string>();

            project.Compiler.Interactions.AddOrReplaceMethod("select", (MethodContext ctxt, string selector) =>
            {
                actions.Add(selector);
            });

            project.Compiler.Interactions.AddOrReplaceMethod("click", (MethodContext ctxt) =>
            {
                actions.Add("click");
            });

            project.Compiler.Interactions.AddOrReplaceMethod("assertExists", (MethodContext ctxt) =>
            {
                actions.Add("assertExists");
            });

            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            compileResult.Messages.Should().BeEmpty();

            var linkResult = project.Compiler.Link();

            linkResult.Messages.Should().BeEmpty();

            var testRun = project.CreateTestRun();

            await testRun.ExecuteAsync(LogFactory);

            // Verify the expected order of method invokes.
            actions.Should().BeEquivalentTo(new[]
            {
                "input[Name]",
                "click",
                "input[type=text][Age]",
                "click",
                "input[Name]",
                "assertExists",
                "input[type=text][Age]",
                "assertExists"
            });
        }

        private class TestElement
        {
            public string Type { get; set; }

            public string For { get; set;  }

            public string Id { get; set; }

            public string Text { get; set; }
        }

        private class AttributeToVariableMethod : InteractionMethod
        {
            public AttributeToVariableMethod() : base("attributeToVariable")
            {
            }

            public override void CompilerMethodCall(IReadOnlyList<MethodArgumentElement> arguments, CallChainCompileTimeVariables variables)
            {
                // The second argument is used as the name of a variable.
                if (arguments.Count > 1)
                {
                    var myArg = arguments[1];

                    if(myArg is StringMethodArgumentElement strArg)
                    {
                        // Only if the value is a string.
                        // Use the literal value only.
                        var text = strArg.Text;

                        // The text value becomes the name of a new variable.
                        variables.SetVariable(text, false);
                    }
                }
            }

            public override int ArgumentCount => 2;

            public override ValueTask InvokeAsync(IServiceScope scope, MethodContext context, object[] arguments)
            {
                // Get the chain value, and update the variables with a name variable.
                var propName = arguments[0].ToString();
                var varName = arguments[1].ToString();

                // Get the first chain value.
                var first = (context.ChainValue as IEnumerable<TestElement>).First();

                if(propName == "for")
                {
                    context.Variables.Set(varName, first.For);
                }

                return default;
            }
        }
    }
}
