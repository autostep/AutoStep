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

            var compiler = ProjectCompiler.CreateDefault(project);

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

            compiler.AddStaticStepDefinitionSource(steps);

            var compileResult = await compiler.Compile();

            var linkResult = compiler.Link();

            var loggerFactory = new LoggerFactory();

            var testRun = new TestRun(project, compiler, new RunConfiguration(), loggerFactory);

            await testRun.Execute();

            doneSomethingCalled.Should().BeTrue();
            argumentValue.Should().Be("argument1");
        }
    }
}
