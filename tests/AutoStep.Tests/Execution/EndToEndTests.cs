using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Compiler;
using AutoStep.Definitions;
using AutoStep.Execution;
using AutoStep.Projects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AutoStep.Tests.Execution
{
    public class EndToEndTests
    {
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

            ";

            var project = new Project();

            project.TryAddFile(new ProjectFile("/test", new StringContentSource(TestFile)));

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

            var compileResult = await project.Compiler.Compile();

            var linkResult = project.Compiler.Link();

            var loggerFactory = new LoggerFactory();

            var testRun = new TestRun(project, new RunConfiguration(), loggerFactory);

            await testRun.Execute();

            doneSomethingCalled.Should().BeTrue();
            argumentValue.Should().Be("argument1");
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

            ";

            var project = new Project();

            project.TryAddFile(new ProjectFile("/test", new StringContentSource(TestFile)));

            var steps = new CallbackDefinitionSource();

            var doneSomethingCalled = false;

            steps.Given("I have done something", async () =>
            {
                await Task.Delay(10);
                doneSomethingCalled = true;
            });

            project.Compiler.AddStaticStepDefinitionSource(steps);

            var compileResult = await project.Compiler.Compile();

            var linkResult = project.Compiler.Link();

            var loggerFactory = new LoggerFactory();

            var testRun = new TestRun(project, new RunConfiguration(), loggerFactory);

            await testRun.Execute();

            doneSomethingCalled.Should().BeTrue();
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

            project.TryAddFile(new ProjectFile("/test", new StringContentSource(TestFile)));

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

            var compileResult = await project.Compiler.Compile();

            var linkResult = project.Compiler.Link();

            var loggerFactory = new LoggerFactory();

            var testRun = new TestRun(project, new RunConfiguration(), loggerFactory);

            await testRun.Execute();

            doneSomethingCalled.Should().BeTrue();
            argumentValue.Should().Be("argument1");
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

            project.TryAddFile(new ProjectFile("/test", new StringContentSource(TestFile)));
            project.TryAddFile(new ProjectFile("/steps", new StringContentSource(StepsFile)));

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

            var compileResult = await project.Compiler.Compile();

            var linkResult = project.Compiler.Link();

            var loggerFactory = new LoggerFactory();

            var testRun = new TestRun(project, new RunConfiguration(), loggerFactory);

            await testRun.Execute();

            doneSomethingCalled.Should().BeTrue();
            argumentValue.Should().Be("an argument1");
        }
    }
}
