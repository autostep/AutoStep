using System;
using FluentAssertions;
using AutoStep.Execution;
using AutoStep.Projects;
using Moq;
using AutoStep.Compiler;
using Xunit;
using AutoStep.Tests.Utils;

namespace AutoStep.Tests.Execution
{
    public class TestRunTests
    {
        [Fact]
        public void NullProjectArgumentException()
        {
            var project = new Project();
            var compiler = ProjectCompiler.CreateDefault(project);
            
            Action act = () => new TestRun(null, compiler, new RunConfiguration(), TestLogFactory.CreateNull());

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void NullCompilerArgumentException()
        {
            var project = new Project();
            var compiler = ProjectCompiler.CreateDefault(project);

            Action act = () => new TestRun(project, null, new RunConfiguration(), TestLogFactory.CreateNull());

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void NullConfigArgumentException()
        {
            var project = new Project();
            var compiler = ProjectCompiler.CreateDefault(project);

            Action act = () => new TestRun(project, compiler, null, TestLogFactory.CreateNull());

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ExecuteTestInvokesRunExecutionStrategy()
        {

        }
    }
}
