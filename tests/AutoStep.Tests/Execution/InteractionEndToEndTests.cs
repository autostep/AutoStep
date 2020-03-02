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

            await testRun.Execute(LogFactory);

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

            await testRun.Execute(LogFactory);

            selectCalled.Should().BeTrue();
            clickCalled.Should().BeTrue();
        }
    }
}
