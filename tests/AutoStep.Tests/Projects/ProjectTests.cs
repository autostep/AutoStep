using System;
using AutoStep.Language;
using AutoStep.Projects;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Projects
{
    public class ProjectTests
    {
        [Fact]
        public void NullCompilerNullArgumentException()
        {
#pragma warning disable 8625
            Action act = () => new Project(null);
#pragma warning restore 8625

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanAddFile()
        {
            var project = new Project();

            var file = new ProjectTestFile("/test", new StringContentSource("content"));

            project.TryAddFile(file).Should().BeTrue();

            project.AllFiles.Should().HaveCount(1);
            project.AllFiles.ContainsKey("/test").Should().BeTrue();
            project.AllFiles["/test"].Should().Be(file);
            file.IsAttachedToProject.Should().BeTrue();
        }

        [Fact]
        public void CannotAddFileIfPathAlreadyInProject()
        {
            var project = new Project();

            var file = new ProjectTestFile("/test", new StringContentSource("content"));

            project.TryAddFile(file).Should().BeTrue();

            project.TryAddFile(file).Should().BeFalse();

            project.AllFiles.Should().HaveCount(1);
            project.AllFiles.ContainsKey("/test").Should().BeTrue();
            project.AllFiles["/test"].Should().Be(file);
        }

        [Fact]
        public void AddTestFileArgumentNullException()
        {
            var project = new Project();

#pragma warning disable 8600
            project.Invoking(p => p.TryAddFile((ProjectTestFile)null)).Should().Throw<ArgumentNullException>();
#pragma warning restore 8600
        }

        [Fact]
        public void AddInteractionFileArgumentNullException()
        {
            var project = new Project();

#pragma warning disable 8600
            project.Invoking(p => p.TryAddFile((ProjectInteractionFile)null)).Should().Throw<ArgumentNullException>();
#pragma warning restore 8600
        }

        [Fact]
        public void CanRemoveFile()
        {
            var project = new Project();

            var file = new ProjectTestFile("/test", new StringContentSource("content"));

            project.TryAddFile(file).Should().BeTrue();
            file.IsAttachedToProject.Should().BeTrue();

            project.TryRemoveFile(file).Should().BeTrue();
            file.IsAttachedToProject.Should().BeFalse();
        }

        [Fact]
        public void RemoveFileReturnsFalseIfFileNotAdded()
        {
            var project = new Project();

            var file = new ProjectTestFile("/test", new StringContentSource("content"));

            project.TryRemoveFile(file).Should().BeFalse();
        }

        [Fact]
        public void RemoveFileArgumentNullException()
        {
            var project = new Project();

#pragma warning disable 8625
            project.Invoking(p => p.TryRemoveFile(null)).Should().Throw<ArgumentNullException>();
#pragma warning restore 8625
        }
    }
}
