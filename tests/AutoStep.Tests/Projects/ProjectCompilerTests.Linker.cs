using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Definitions;
using AutoStep.Projects;
using AutoStep.Tests.Builders;
using FluentAssertions;
using Moq;
using Xunit;
using AutoStep.Elements.Test;
using AutoStep.Language.Test;

namespace AutoStep.Tests.Projects
{
    public partial class ProjectCompilerTests
    {
        [Fact]
        public void LinksForTheFirstTime()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();
            
            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            project.TryAddFile(projFile);

            var builtFile = new FileBuilder().Feature("My Feature", 1, 1).Built;

            var linkResult = new LinkResult(true, Enumerable.Empty<LanguageOperationMessage>(), null, builtFile);

            mockLinker.Setup(x => x.Link(builtFile)).Returns(linkResult);

            // Tell the file it's been compiled before.
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            var result = projectCompiler.Link();

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            projFile.LastLinkResult.Should().BeSameAs(linkResult);
        }

        [Fact]
        public void DoNotLinkFileThatHasNotBeenCompiled()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();

            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            project.TryAddFile(projFile);

            mockLinker.Setup(x => x.Link(It.IsAny<FileElement>())).Verifiable();

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            projectCompiler.Link();

            mockLinker.Verify(l => l.Link(It.IsAny<FileElement>()), Times.Never());
        }

        [Fact]
        public void DoNotRelinkLinkedFile()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();

            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            project.TryAddFile(projFile);

            var builtFile = new FileBuilder().Feature("My Feature", 1, 1).Built;

            var linkResult = new LinkResult(true, Enumerable.Empty<LanguageOperationMessage>(), null, builtFile);

            mockLinker.Setup(x => x.Link(builtFile)).Returns(linkResult).Verifiable();

            // Tell the file it's been compiled before.
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            projectCompiler.Link();

            // The second link shouldn't do anything, because nothing has changed.
            projectCompiler.Link();

            mockLinker.Verify(x => x.Link(builtFile), Times.Once());
        }

        [Fact]
        public void RelinkFileIfPreviousLinkHadErrors()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();

            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            project.TryAddFile(projFile);

            var builtFile = new FileBuilder().Feature("My Feature", 1, 1).Built;

            var linkResult = new LinkResult(false, new []
            {
                new LanguageOperationMessage("/file1", CompilerMessageLevel.Error, CompilerMessageCode.LinkerNoMatchingStepDefinition, "")
            }, null, builtFile);

            mockLinker.Setup(x => x.Link(builtFile)).Returns(linkResult).Verifiable();

            // Tell the file it's been compiled before.
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            projectCompiler.Link();

            // First link failed, so this will link again.
            projectCompiler.Link();

            mockLinker.Verify(x => x.Link(builtFile), Times.Exactly(2));
        }

        [Fact]
        public void RelinkFileIfPreviousLinkHadWarnings()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();

            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            project.TryAddFile(projFile);

            var builtFile = new FileBuilder().Feature("My Feature", 1, 1).Built;

            var linkResult = new LinkResult(true, new[]
            {
                new LanguageOperationMessage("/file1", CompilerMessageLevel.Warning, CompilerMessageCode.LinkerNoMatchingStepDefinition, "")
            }, null, builtFile);

            mockLinker.Setup(x => x.Link(builtFile)).Returns(linkResult).Verifiable();

            // Tell the file it's been compiled before.
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            projectCompiler.Link();

            // First link has warnings, so this will link again.
            projectCompiler.Link();

            mockLinker.Verify(x => x.Link(builtFile), Times.Exactly(2));
        }

        [Fact]
        public void RelinkFileIfCompileResultChanged()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();

            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            project.TryAddFile(projFile);

            var builtFile = new FileBuilder().Feature("My Feature", 1, 1).Built;

            var linkResult = new LinkResult(true, Enumerable.Empty<LanguageOperationMessage>(), null, builtFile);

            mockLinker.Setup(x => x.Link(builtFile)).Returns(linkResult).Verifiable();

            // Tell the file it's been compiled before.
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            projectCompiler.Link();

            // Sleep for a moment to indicate a delay before the next compile.
            Thread.Sleep(1);

            // Update the compile results.
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));

            // This should relink.
            projectCompiler.Link();

            mockLinker.Verify(x => x.Link(builtFile), Times.Exactly(2));
        }

        [Fact]
        public void FileLinkerMessagesIncludedInOverall()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();

            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            project.TryAddFile(projFile);

            var builtFile = new FileBuilder().Feature("My Feature", 1, 1).Built;

            var msg = new LanguageOperationMessage("/file1", CompilerMessageLevel.Warning, CompilerMessageCode.LinkerNoMatchingStepDefinition, "");

            var linkResult = new LinkResult(true, new[]
            {
                msg
            }, null, builtFile);

            mockLinker.Setup(x => x.Link(builtFile)).Returns(linkResult);

            // Tell the file it's been compiled before.
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            var overallLink = projectCompiler.Link();

            overallLink.Messages.First().Should().Be(msg);
        }
        
        [Fact]
        public void RelinkFileIfLinkerDependencyUpdated()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();

            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);

            var linkerDepLastModify = DateTime.Today;

            var stepDefSource = new Mock<IUpdatableStepDefinitionSource>();
            stepDefSource.Setup(x => x.GetLastModifyTime()).Returns(() => linkerDepLastModify);

            project.TryAddFile(projFile);

            var builtFile = new FileBuilder().Feature("My Feature", 1, 1).Built;

            var linkResult = new LinkResult(true, Enumerable.Empty<LanguageOperationMessage>(), new[] { stepDefSource.Object }, builtFile);

            mockLinker.Setup(x => x.Link(builtFile)).Returns(linkResult).Verifiable();

            // Tell the file it's been compiled before.
            projFile.UpdateLastCompileResult(new FileCompilerResult(true, builtFile));

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            projectCompiler.Link();

            // Change the linker dependency modification time.
            linkerDepLastModify = projFile.LastLinkTime.Value.AddMinutes(1);

            // This should relink.
            projectCompiler.Link();

            mockLinker.Verify(x => x.Link(builtFile), Times.Exactly(2));
        }

        [Fact]
        public void LinkCanBeCancelled()
        {
            var mockCompiler = new Mock<IAutoStepCompiler>();
            var mockLinker = new Mock<IAutoStepLinker>();

            var project = new Project();
            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            
            project.TryAddFile(projFile);

            mockLinker.Setup(x => x.Link(It.IsAny<FileElement>())).Verifiable();

            var projectCompiler = new ProjectCompiler(project, mockCompiler.Object, mockLinker.Object);

            var cancelledToken = new CancellationToken(true);

            projectCompiler.Invoking(c => c.Link(cancelledToken)).Should().Throw<OperationCanceledException>();
        }
    }
}
