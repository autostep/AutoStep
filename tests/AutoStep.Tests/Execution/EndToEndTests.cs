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
using AutoStep.Definitions.Test;

namespace AutoStep.Tests.Execution
{
    public class EndToEndTests : LoggingTestBase
    {
        public EndToEndTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task SimpleEndToEndTest()
        {
            // Compile a file.
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have done something
                      And I have passed argument1 to something
                    
                    When I do this
                    Then it should be true

            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));

            var steps = new CallbackDefinitionSource();

            var doneSomethingCalled = false;
            string argumentValue = null;
            var whenCalled = false;
            var thenCalled = false;

            steps.Given("I have done something", () =>
            {
                doneSomethingCalled = true;
            });

            steps.Given("I have passed {arg} to something", (string arg1) =>
            {
                argumentValue = arg1;
            });

            steps.When("I do this", (IServiceScope scope) =>
            {
                scope.Should().NotBeNull();
                whenCalled = true;
            });

            steps.Then("it should be true", async () =>
            {
                thenCalled = true;
                await Task.Delay(1);
            });

            project.Compiler.AddStaticStepDefinitionSource(steps);

            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            var linkResult = project.Compiler.Link();

            var testRun = project.CreateTestRun();

            await testRun.ExecuteAsync(LogFactory);

            doneSomethingCalled.Should().BeTrue();
            argumentValue.Should().Be("argument1");
            whenCalled.Should().BeTrue();
            thenCalled.Should().BeTrue();
        }

        [Fact]
        public async Task ScenarioOutline()
        {
            // Compile a file.
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario Outline: My Scenario Outline

                    Given I have done something
                      And I have passed <argValue> to something

                Examples:
                    | argValue |
                    | value1   |
                    | value2   |

            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));

            var steps = new CallbackDefinitionSource();

            int doneSomethingCalled = 0;
            var argumentValues = new List<string>();

            steps.Given("I have done something", () =>
            {
                doneSomethingCalled++;
            });

            steps.Given("I have passed {arg} to something", (string arg1) =>
            {
                argumentValues.Add(arg1);
            });

            project.Compiler.AddStaticStepDefinitionSource(steps);

            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            var linkResult = project.Compiler.Link();

            var testRun = project.CreateTestRun();

            await testRun.ExecuteAsync(LogFactory);

            doneSomethingCalled.Should().Be(2);
            argumentValues[0].Should().Be("value1");
            argumentValues[1].Should().Be("value2");
        }

        [Fact]
        public async Task StepHasAsyncOperation()
        {
            // Compile a file.
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have done something
                    
                    When I have clicked something
                    
                    Then this should have happened
            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));

            var steps = new CallbackDefinitionSource();

            var doneSomethingCalled = false;
            var clickedSomethingCalled = false;
            var thisHappened = false;

            steps.Given("I have done something", async () =>
            {
                await Task.Delay(10);
                doneSomethingCalled = true;
            });

            steps.When("I have clicked something", () =>
            {
                clickedSomethingCalled = true;
            });

            steps.Then("this should have happened", () =>
            {
                thisHappened = true;
            });

            project.Compiler.AddStaticStepDefinitionSource(steps);

            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            var linkResult = project.Compiler.Link();

            var testRun = project.CreateTestRun();

            await testRun.ExecuteAsync(LogFactory);

            doneSomethingCalled.Should().BeTrue();
            clickedSomethingCalled.Should().BeTrue();
            thisHappened.Should().BeTrue();
        }

        [Fact]
        public async Task ReferencingAStepInMyFile()
        {
            // Compile a file.
            const string TestFile =
            @"                
              Step: Given I have called my defined step with {arg}

                    Given I have done something
                      And I have passed <arg> to something

              Feature: My Feature

                Scenario: My Scenario

                    Given I have called my defined step with argument1

            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));

            var steps = new CallbackDefinitionSource();

            var doneSomethingCalled = false;
            string argumentValue = null;

            steps.Given("I have done something", () =>
            {
                doneSomethingCalled = true;
            });

            steps.Given("I have passed {arg} to something", (string arg1) =>
            {
                argumentValue = arg1;
            });

            project.Compiler.AddStaticStepDefinitionSource(steps);

            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            var linkResult = project.Compiler.Link();

            var testRun = project.CreateTestRun();

            await testRun.ExecuteAsync(LogFactory);

            doneSomethingCalled.Should().BeTrue();
            argumentValue.Should().Be("argument1");
        }

        [Fact]
        public async Task CircularReferenceDetection()
        {
            // Compile a file.
            const string TestFile =
            @"                
              Step: Given I have called my defined step with {arg}

                    Given I have done something
                      And I have called my defined step with <arg>
                      And I have passed <arg> to something

              Feature: My Feature

                Scenario: My Scenario

                    Given I have called my defined step with argument1

            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));

            var steps = new CallbackDefinitionSource();

            var doneSomethingCalled = false;
            string argumentValue = null;

            steps.Given("I have done something", () =>
            {
                doneSomethingCalled = true;
            });

            steps.Given("I have passed {arg} to something", (string arg1) =>
            {
                argumentValue = arg1;
            });

            project.Compiler.AddStaticStepDefinitionSource(steps);

            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            var linkResult = project.Compiler.Link();

            var testRun = project.CreateTestRun();
            var errorCollector = new StepExceptionCollector();
            
            testRun.Events.Add(errorCollector);

            await testRun.ExecuteAsync(LogFactory);

            doneSomethingCalled.Should().BeTrue();
            argumentValue.Should().BeNull();

            errorCollector.FoundException.Should().BeOfType<StepFailureException>()
                    .Which.InnerException.Should().BeOfType<CircularStepReferenceException>()
                    .Which.StepDefinition.Declaration.Should().Be("I have called my defined step with {arg}");
        }

        private class StepExceptionCollector : BaseEventHandler
        {
            public Exception FoundException { get; set; }

            public override async ValueTask OnStep(IServiceScope scope, StepContext ctxt, Func<IServiceScope, StepContext, ValueTask> nextHandler)
            {
                
                await nextHandler(scope, ctxt);
                
                if(ctxt.FailException is object)
                {
                    FoundException = ctxt.FailException;
                }
            }
        }

        [Fact]
        public async Task ReferencingAStepInAnotherFile()
        {
            // Compile a file.
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have called my defined step with 'an argument1'

            ";

            const string StepsFile =
            @"
              Step: Given I have called my defined step with {arg}

                    Given I have done something
                      And I have passed <arg> to something
            ";

            var project = new Project();

            project.TryAddFile(new ProjectTestFile("/test", new StringContentSource(TestFile)));
            project.TryAddFile(new ProjectTestFile("/steps", new StringContentSource(StepsFile)));

            var steps = new CallbackDefinitionSource();

            var doneSomethingCalled = false;
            string argumentValue = null;

            steps.Given("I have done something", () =>
            {
                doneSomethingCalled = true;
            });

            steps.Given("I have passed {arg} to something", (string arg1) =>
            {
                argumentValue = arg1;
            });

            project.Compiler.AddStaticStepDefinitionSource(steps);

            var compileResult = await project.Compiler.CompileAsync(LogFactory);

            var linkResult = project.Compiler.Link();

            var testRun = project.CreateTestRun();

            await testRun.ExecuteAsync(LogFactory);

            doneSomethingCalled.Should().BeTrue();
            argumentValue.Should().Be("an argument1");
        }
    }
}
