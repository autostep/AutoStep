using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Definitions;
using Microsoft.Extensions.Logging;
using AutoStep.Language.Test;

namespace AutoStep.Projects
{
    /// <summary>
    /// Provides the functionality to compile and link an entire project.
    /// </summary>
    public class ProjectCompiler : IProjectCompiler
    {
        private readonly Project project;
        private readonly IAutoStepCompiler compiler;
        private readonly IAutoStepLinker linker;
        private readonly AutoStepLineTokeniser lineTokeniser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectCompiler"/> class.
        /// </summary>
        /// <param name="project">The project to work on.</param>
        /// <param name="compiler">The compiler implementation to use.</param>
        /// <param name="linker">The linker implementation to use.</param>
        public ProjectCompiler(Project project, IAutoStepCompiler compiler, IAutoStepLinker linker)
        {
            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            this.linker = linker ?? throw new ArgumentNullException(nameof(linker));
            this.lineTokeniser = new AutoStepLineTokeniser(linker);
        }

        /// <summary>
        /// Creates a default project compiler with the normal compiler and linker settings.
        /// </summary>
        /// <param name="project">The project to work against.</param>
        /// <returns>A project compiler.</returns>
        public static ProjectCompiler CreateDefault(Project project)
        {
            var compiler = new AutoStepCompiler(CompilerOptions.Default);
            return new ProjectCompiler(project, compiler, new AutoStepLinker(compiler));
        }

        /// <summary>
        /// Compile the project. Goes through all the project files and compiles those that need compilation.
        /// </summary>
        /// <param name="cancelToken">A cancellation token that halts compilation partway through.</param>
        /// <returns>The overall project compilation result.</returns>
        public async Task<ProjectCompilerResult> CompileAsync(CancellationToken cancelToken = default)
        {
            using var logFactory = new LoggerFactory();

            return await CompileAsync(logFactory, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Compile the project. Goes through all the project files and compiles those that need compilation.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <param name="cancelToken">A cancellation token that halts compilation partway through.</param>
        /// <returns>The overall project compilation result.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to convert exceptions into compiler messsages.")]
        public async Task<ProjectCompilerResult> CompileAsync(ILoggerFactory loggerFactory, CancellationToken cancelToken = default)
        {
            var allMessages = new List<CompilerMessage>();

            // This method will go through all the autostep files in the project that match the filter and:
            //  - Verify that the built files are later than the source file last-modify time.
            //  - Update the project files with the built content.
            //  - Report any errors in that compilation.
            //  - Each file in the project will have its 'last' FileCompilerResult stored against it.
            foreach (var projectFile in project.AllFiles)
            {
                cancelToken.ThrowIfCancellationRequested();

                try
                {
                    var file = projectFile.Value;

                    // For each file.
                    // Compile.
                    // Add the result of the compilation to the ProjectFile.
                    // Add as a new step definition source to the linker if the file defines any step definitions.
                    if (file.LastCompileTime < file.ContentSource.GetLastContentModifyTime())
                    {
                        var fileResult = await DoProjectFileCompile(file, loggerFactory, cancelToken).ConfigureAwait(false);

                        allMessages.AddRange(fileResult.Messages);
                    }
                }
                catch (IOException ex)
                {
                    allMessages.Add(CompilerMessageFactory.Create(projectFile.Value.Path, CompilerMessageLevel.Error, CompilerMessageCode.IOException, 0, 0, ex.Message));
                }
                catch (Exception ex)
                {
                    // Severe enough error occurred inside the compilation process that we couldn't convert into
                    // a compiler message internally, and wasn't a more specific Exception we can catch.
                    // Add a catch all compilation error.
                    allMessages.Add(CompilerMessageFactory.Create(projectFile.Value.Path, CompilerMessageLevel.Error, CompilerMessageCode.UncategorisedException, 0, 0, ex.Message));
                }
            }

            // Project compilation always succeeds, but possibly with individual file errors. We will aggregate all the file
            // messages and report them at once.
            return new ProjectCompilerResult(true, allMessages, project);
        }

        private async Task<FileCompilerResult> DoProjectFileCompile(ProjectFile file, ILoggerFactory loggerFactory, CancellationToken cancelToken)
        {
            var compileResult = await compiler.CompileAsync(file.ContentSource, loggerFactory, cancelToken).ConfigureAwait(false);

            file.UpdateLastCompileResult(compileResult);

            if (file.StepDefinitionSource is object)
            {
                // Update the linker with the modified step definitions (if there are any).
                linker.AddOrUpdateStepDefinitionSource(file.StepDefinitionSource);
            }

            return compileResult;
        }

        /// <summary>
        /// Add a static step definition source (i.e. one that cannot change after it is registered).
        /// </summary>
        /// <param name="source">The step definition source.</param>
        public void AddStaticStepDefinitionSource(IStepDefinitionSource source)
        {
            linker.AddStepDefinitionSource(source.ThrowIfNull(nameof(source)));
        }

        /// <summary>
        /// Add an updateable step definition source (i.e. one that can change dynamically).
        /// </summary>
        /// <param name="source">The step definition source.</param>
        public void AddUpdatableStepDefinitionSource(IUpdatableStepDefinitionSource source)
        {
            linker.AddOrUpdateStepDefinitionSource(source.ThrowIfNull(nameof(source)));
        }

        /// <summary>
        /// Retrieve the set of all step definition sources.
        /// </summary>
        /// <returns>The set of registered step definition sources.</returns>
        public IEnumerable<IStepDefinitionSource> EnumerateStepDefinitionSources()
        {
            return linker.AllStepDefinitionSources;
        }

        /// <summary>
        /// Links the entire project. Files that need to be re-linked will be.
        /// </summary>
        /// <param name="cancelToken">A cancellation token for the linker process.</param>
        /// <returns>The overall project link result.</returns>
        public ProjectCompilerResult Link(CancellationToken cancelToken = default)
        {
            var allMessages = new List<CompilerMessage>();

            // This method will go through all the autostep files in the project and:
            //  - Link if needed.
            //  - Report any errors in the link.
            //  - Each file in the project will have its 'last' LinkResult stored against it.
            foreach (var projectFile in project.AllFiles)
            {
                cancelToken.ThrowIfCancellationRequested();

                var file = projectFile.Value;
                if (file.LastCompileResult?.Output is null)
                {
                    // Without a compilation result, we can't link.
                    continue;
                }

                // For each file, if any of the following are true, then we link:
                //  - Have we linked before?
                //  - Did the previous linking have any problems?
                //  - Has the file been re-compiled since the last link?
                //  - Have any of the dependencies detected at the previous link been changed?
                if (file.LastLinkResult is null ||
                    file.LastLinkResult.AnyIssues ||
                    file.LastLinkTime < file.LastCompileTime ||
                    AnyLinkerDependenciesUpdated(file))
                {
                    var linkResult = linker.Link(file.LastCompileResult.Output);

                    file.UpdateLastLinkResult(linkResult);

                    allMessages.AddRange(linkResult.Messages);
                }
            }

            return new ProjectCompilerResult(true, allMessages, project);
        }

        private static bool AnyLinkerDependenciesUpdated(ProjectFile file)
        {
            if (file.LinkerDependencies is null)
            {
                return false;
            }

            foreach (var dep in file.LinkerDependencies)
            {
                if (dep.GetLastModifyTime() > file.LastLinkTime)
                {
                    // The dependency has been changed
                    // more recently than this file was last linked, so we need to re-link.
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tokenises a line of text, returning a set of line tokens. Used mostly for syntax highlighting; faster than a regular compile.
        /// </summary>
        /// <param name="line">The line of text to tokenise.</param>
        /// <param name="lastTokeniserState">
        /// The value of <see cref="LineTokeniseResult.EndState"/> from
        /// the previous call to this method for the same file.
        /// </param>
        /// <returns>The tokenisation result.</returns>
        public LineTokeniseResult TokeniseLine(string line, LineTokeniserState lastTokeniserState = LineTokeniserState.Default)
        {
            return lineTokeniser.Tokenise(line, lastTokeniserState);
        }
    }
}
