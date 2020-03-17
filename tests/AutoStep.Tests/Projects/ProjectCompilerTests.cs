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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AutoStep.Elements.Test;
using AutoStep.Language.Test;

namespace AutoStep.Tests.Projects
{
    public partial class ProjectCompilerTests
    {
        [Fact]
        public void ConstructorThrowsProjectNullArgumentException()
        {
            Action act = () => GetCompiler(null, new Mock<ITestCompiler>().Object, new Mock<ILinker>().Object);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConstructorThrowsCompilerNullArgumentException()
        {
            Action act = () => GetCompiler(new Project(), null, new Mock<ILinker>().Object);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConstructorThrowsLinkerNullArgumentException()
        {
            Action act = () => GetCompiler(new Project(), new Mock<ITestCompiler>().Object, null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CompilesNewFile()
        {
            var project = new Project();
            var mockSource = new Mock<IContentSource>();
            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);
            var mockCompiler = new Mock<ITestCompiler>();
            // Compilation will return a compilation result (with an empty file).
            mockCompiler.Setup(x => x.CompileAsync(mockSource.Object, It.IsAny<ILoggerFactory>(), default)).Returns(new ValueTask<FileCompilerResult>(
                new FileCompilerResult(true, new FileElement())
            ));
            var mockLinker = new Mock<ILinker>();

            var projFile = new ProjectTestFile("/file1", mockSource.Object);
            project.TryAddFile(projFile);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            var result = projectCompiler.CompileAsync().GetAwaiter().GetResult();

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            projFile.LastCompileResult.Should().NotBeNull();
        }

        [Fact]
        public async Task CompilesMultipleFilesWithErrors()
        {
            var project = new Project();
            var mockSource1 = new Mock<IContentSource>();
            mockSource1.SetupGet(x => x.SourceName).Returns("/file1");
            mockSource1.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);

            var mockSource2 = new Mock<IContentSource>();
            mockSource1.SetupGet(x => x.SourceName).Returns("/file2");
            mockSource2.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);

            var mockCompiler = new Mock<ITestCompiler>();

            // First compile result
            mockCompiler.Setup(x => x.CompileAsync(mockSource1.Object, It.IsAny<ILoggerFactory>(), default)).Returns(new ValueTask<FileCompilerResult>(
                new FileCompilerResult(true, new[] { new LanguageOperationMessage("/file1", CompilerMessageLevel.Error, CompilerMessageCode.SyntaxError, "") })
            ));

            // Second compile result
            mockCompiler.Setup(x => x.CompileAsync(mockSource2.Object, It.IsAny<ILoggerFactory>(), default)).Returns(new ValueTask<FileCompilerResult>(
                new FileCompilerResult(true, new[] { new LanguageOperationMessage("/file2", CompilerMessageLevel.Error, CompilerMessageCode.SyntaxError, "") })
            ));

            var mockLinker = new Mock<ILinker>();

            var projFile1 = new ProjectTestFile("/file1", mockSource1.Object);
            var projFile2 = new ProjectTestFile("/file2", mockSource2.Object);
            project.TryAddFile(projFile1);
            project.TryAddFile(projFile2);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            var result = await projectCompiler.CompileAsync();

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            var msgs = result.Messages.ToList();

            msgs.Should().HaveCount(2);

            msgs[0].SourceName.Should().Be("/file1");
            msgs[1].SourceName.Should().Be("/file2");
        }

        [Fact]
        public async Task DoesNotCompileFileThatHasNotBeenChanged()
        {
            var project = new Project();
            var mockSource = new Mock<IContentSource>();
            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);

            var mockCompiler = new Mock<ITestCompiler>();
            // Compilation will return a compilation result (with an empty file).
            mockCompiler.Setup(x => x.CompileAsync(mockSource.Object, It.IsAny<ILoggerFactory>(), default)).Returns(new ValueTask<FileCompilerResult>(
                new FileCompilerResult(true, new FileElement())
            ));
            var mockLinker = new Mock<ILinker>();

            var projFile = new ProjectTestFile("/file1", mockSource.Object);
            project.TryAddFile(projFile);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            // Compile once.
            await projectCompiler.CompileAsync();

            var originalCompilationResult = projFile.LastCompileResult;

            // Just do it again.
            await projectCompiler.CompileAsync();

            // Result should be the same.
            projFile.LastCompileResult.Should().BeSameAs(originalCompilationResult);
        }

        [Fact]
        public async Task ReCompilesFileThatHasBeenChanged()
        {
            var project = new Project();
            var mockSource = new Mock<IContentSource>();

            var changeTime = DateTime.Today;

            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(() => changeTime);

            var mockCompiler = new Mock<ITestCompiler>();
            // Compilation will return a compilation result (with an empty file).
            mockCompiler.Setup(x => x.CompileAsync(mockSource.Object, It.IsAny<ILoggerFactory>(), default)).Returns(new ValueTask<FileCompilerResult>(
                new FileCompilerResult(true, new FileElement())
            ));
            var mockLinker = new Mock<ILinker>();

            var projFile = new ProjectTestFile("/file1", mockSource.Object);
            project.TryAddFile(projFile);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            // Compile once.
            await projectCompiler.CompileAsync();

            var originalCompilationResult = projFile.LastCompileResult;

            // Change the file timestamp.
            changeTime = projFile.LastCompileTime.AddMinutes(1);

            // Run it again.
            await projectCompiler.CompileAsync();

            // Result should have changed.
            projFile.Should().NotBeSameAs(originalCompilationResult);
        }

        [Fact]
        public async Task AddsCompilationMessagesToProjectMessages()
        {
            var project = new Project();
            var mockSource = new Mock<IContentSource>();
            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);

            var fileMessage = new LanguageOperationMessage("/path", CompilerMessageLevel.Error, CompilerMessageCode.SyntaxError, "");

            var mockCompiler = new Mock<ITestCompiler>();
            // Compilation will return a compilation result (with an empty file).
            mockCompiler.Setup(x => x.CompileAsync(mockSource.Object, It.IsAny<ILoggerFactory>(), default)).Returns(new ValueTask<FileCompilerResult>(
                new FileCompilerResult(true, new[] { fileMessage })
            ));
            var mockLinker = new Mock<ILinker>();

            var projFile = new ProjectTestFile("/file1", mockSource.Object);
            project.TryAddFile(projFile);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            // Compile once.
            var overallResult = await projectCompiler.CompileAsync();

            overallResult.Messages.Should().Contain(fileMessage);
        }

        [Fact]
        public void AddsIOExceptionCompilerMessageIfIOExceptionThrownInCompiler()
        {
            var project = new Project();
            var mockSource = new Mock<IContentSource>();
            mockSource.Setup(s => s.SourceName).Returns("/file1");
            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);

            var mockCompiler = new Mock<ITestCompiler>();
            // Compilation will return a compilation result (with an empty file).
            mockCompiler.Setup(x => x.CompileAsync(mockSource.Object, It.IsAny<ILoggerFactory>(), default)).Throws(new IOException("IO Error"));

            var mockLinker = new Mock<ILinker>();

            var projFile = new ProjectTestFile("/file1", mockSource.Object);
            project.TryAddFile(projFile);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            // Compile once.
            var overallResult = projectCompiler.CompileAsync().GetAwaiter().GetResult();

            var expectedMessage = new LanguageOperationMessage("/file1", CompilerMessageLevel.Error, CompilerMessageCode.IOException,
                                                      "File access error: IO Error", 0, 0);

            overallResult.Messages.Should().Contain(expectedMessage);
        }

        [Fact]
        public void AddsUncategorisedExceptionCompilerMessageIfGeneralExceptionThrownInCompiler()
        {
            var project = new Project();
            var mockSource = new Mock<IContentSource>();
            mockSource.Setup(s => s.SourceName).Returns("/file1");
            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);

            var mockCompiler = new Mock<ITestCompiler>();
            // Compilation will return a compilation result (with an empty file).
            mockCompiler.Setup(x => x.CompileAsync(mockSource.Object, It.IsAny<ILoggerFactory>(), default)).Throws(new ApplicationException("Unknown Error"));

            var mockLinker = new Mock<ILinker>();

            var projFile = new ProjectTestFile("/file1", mockSource.Object);
            project.TryAddFile(projFile);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            // Compile once.
            var overallResult = projectCompiler.CompileAsync().GetAwaiter().GetResult();

            var expectedMessage = new LanguageOperationMessage("/file1", CompilerMessageLevel.Error, CompilerMessageCode.UncategorisedException,
                                                      "Internal Error: Unknown Error", 0, 0);

            overallResult.Messages.Should().Contain(expectedMessage);
        }

        [Fact]
        public void CompileCanBeCancelled()
        {
            var project = new Project();
            var mockCompiler = new Mock<ITestCompiler>();
            var mockLinker = new Mock<ILinker>();

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            var projFile = new ProjectTestFile("/file1", new Mock<IContentSource>().Object);
            project.TryAddFile(projFile);

            var cancelledToken = new CancellationToken(true);

            // Compile once.
            var overallResult = projectCompiler.Invoking(c => c.CompileAsync(cancelledToken)).Should().Throw<OperationCanceledException>();
        }

        [Fact]
        public void AddsStepDefinitionSourceForFileWithStepDefinitions()
        {
            var project = new Project();
            var mockSource = new Mock<IContentSource>();
            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);
            var mockCompiler = new Mock<ITestCompiler>();

            var compiledFile = new FileBuilder()
                                .StepDefinition(StepType.Given, "I have defined something", 2, 2)
                                .Built;

            // Compilation will return a compilation result (with an empty file).
            mockCompiler.Setup(x => x.CompileAsync(mockSource.Object, It.IsAny<ILoggerFactory>(), default)).Returns(new ValueTask<FileCompilerResult>(
                new FileCompilerResult(true, compiledFile)
            ));

            var mockLinker = new Mock<ILinker>();

            IUpdatableStepDefinitionSource? addedSource = null;

            // Check that the linker is invoked.
            mockLinker.Setup(x => x.AddOrUpdateStepDefinitionSource(It.IsAny<IUpdatableStepDefinitionSource>()))
                      .Callback((IUpdatableStepDefinitionSource s) => addedSource = s);

            var projFile = new ProjectTestFile("/file1", mockSource.Object);
            project.TryAddFile(projFile);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            var result = projectCompiler.CompileAsync().GetAwaiter().GetResult();

            projFile.StepDefinitionSource.Should().NotBeNull();
            // Linker was called.
            addedSource.Should().NotBeNull();
        }

        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/42")]
        public void InvokesUpdateStepDefinitionSourceAfterEachRecompile()
        {
            var project = new Project();
            var mockSource = new Mock<IContentSource>();
            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(DateTime.Today);
            var mockCompiler = new Mock<ITestCompiler>();

            var compiledFile = new FileBuilder()
                                .StepDefinition(StepType.Given, "I have defined something", 2, 2)
                                .Built;

            // Compilation will return a compilation result (with an empty file).
            mockCompiler.Setup(x => x.CompileAsync(mockSource.Object, It.IsAny<ILoggerFactory>(), default)).Returns(new ValueTask<FileCompilerResult>(
                new FileCompilerResult(true, compiledFile)
            ));

            var mockLinker = new Mock<ILinker>();

            int updatedSourceCount = 0;

            // Check that the linker is invoked.
            mockLinker.Setup(x => x.AddOrUpdateStepDefinitionSource(It.IsAny<IUpdatableStepDefinitionSource>()))
                      .Callback((IUpdatableStepDefinitionSource s) => updatedSourceCount++);

            var projFile = new ProjectTestFile("/file1", mockSource.Object);
            project.TryAddFile(projFile);

            var projectCompiler = GetCompiler(project, mockCompiler.Object, mockLinker.Object);

            var result = projectCompiler.CompileAsync().GetAwaiter().GetResult();

            projFile.StepDefinitionSource.Should().NotBeNull();

            // Linker was called.
            updatedSourceCount.Should().Be(1);

            // Indicate the file has changed.
            mockSource.Setup(s => s.GetLastContentModifyTime()).Returns(projFile.LastCompileTime + TimeSpan.FromSeconds(10));

            // Compile again.
            projectCompiler.CompileAsync().GetAwaiter().GetResult();

            // We should have updated the source.
            updatedSourceCount.Should().Be(2);
        }
    }
}
