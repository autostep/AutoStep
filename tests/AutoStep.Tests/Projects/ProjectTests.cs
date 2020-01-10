using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Compiler;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Projects
{
    public class ProjectTests
    {
        [Fact]
        public void CanAddFile()
        {
            var project = new Project();

            var file = new ProjectFile("/test", new StringContentSource("content"));

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

            var file = new ProjectFile("/test", new StringContentSource("content"));

            project.TryAddFile(file).Should().BeTrue();

            project.TryAddFile(file).Should().BeFalse();

            project.AllFiles.Should().HaveCount(1);
            project.AllFiles.ContainsKey("/test").Should().BeTrue();
            project.AllFiles["/test"].Should().Be(file);
        }

        [Fact]
        public void AddFileArgumentNullException()
        {
            var project = new Project();

            project.Invoking(p => p.TryAddFile(null)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanRemoveFile()
        {
            var project = new Project();

            var file = new ProjectFile("/test", new StringContentSource("content"));

            project.TryAddFile(file).Should().BeTrue();
            file.IsAttachedToProject.Should().BeTrue();

            project.TryRemoveFile(file).Should().BeTrue();
            file.IsAttachedToProject.Should().BeFalse();
        }

        [Fact]
        public void RemoveFileReturnsFalseIfFileNotAdded()
        {
            var project = new Project();

            var file = new ProjectFile("/test", new StringContentSource("content"));

            project.TryRemoveFile(file).Should().BeFalse();
        }

        [Fact]
        public void RemoveFileArgumentNullException()
        {
            var project = new Project();

            project.Invoking(p => p.TryRemoveFile(null)).Should().Throw<ArgumentNullException>();
        }
    }
}
