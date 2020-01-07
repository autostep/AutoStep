using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Compiler;
using AutoStep.Definitions;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution
{
    public class ProjectCompiler
    {
        private readonly Project project;
        private readonly IAutoStepCompiler compiler;
        private readonly IAutoStepLinker linker;

        public ProjectCompiler(Project project, IAutoStepCompiler compiler, IAutoStepLinker linker)
        {
            this.project = project;
            this.compiler = compiler;
            this.linker = linker;
        }

        public async Task<ProjectCompilerResult> Compile(CancellationToken cancelToken = default)
        {
            var allMessages = new List<CompilerMessage>();

            // This method will go through all the autostep files in the project that match the filter and:
            //  - Verify that the built files are later than the source file last-modify time.
            //  - Update the project files with the built content.
            //  - Report any errors in that compilation.
            //  - Each file in the project will have its 'last' FileCompilerResult stored against it.
            foreach (var projectFile in project.AllFiles)
            {
                var file = projectFile.Value;

                // For each file.
                // Compile.
                // Add the result of the compilation to the ProjectFile.
                // Add as a new step definition source to the linker if the file defines any step definitions.
                if (file.LastCompileTime < file.ContentSource.GetLastContentModifyTime())
                {
                    var fileResult = await DoProjectFileCompile(file, cancelToken).ConfigureAwait(false);

                    allMessages.AddRange(fileResult.Messages);
                }

                cancelToken.ThrowIfCancellationRequested();
            }

            // Project compilation always succeeds, but possibly with individual file errors. We will aggregate all the file 
            // messages and report them at once.
            return new ProjectCompilerResult(true, allMessages, project);
        }

        private async Task<FileCompilerResult> DoProjectFileCompile(ProjectFile file, CancellationToken cancelToken)
        {
            var compileResult = await compiler.CompileAsync(file.ContentSource, cancelToken).ConfigureAwait(false);

            file.UpdateLastCompileResult(compileResult);

            if (file.StepDefinitionSource is object)
            {
                // Update the linker with the modified step definitions (if there are any).
                linker.AddOrUpdateStepDefinitionSource(file.StepDefinitionSource);
            }

            return compileResult;
        }

        public ProjectCompilerResult Link(CancellationToken cancelToken = default)
        {
            var allMessages = new List<CompilerMessage>();

            // This method will go through all the autostep files in the project and:
            //  - Link if needed.
            //  - Report any errors in the link.
            //  - Each file in the project will have its 'last' LinkResult stored against it.
            foreach (var projectFile in project.AllFiles)
            {
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

                    var linkerDeps = new List<IUpdatableStepDefinitionSource>();

                    // Check for any file sources.
                    foreach (var refSource in linkResult.ReferencedSources)
                    {
                        if (refSource is IUpdatableStepDefinitionSource updatableSource)
                        {
                            // The source is another file in the project; add it as a referenced step.
                            linkerDeps.Add(updatableSource);
                        }
                    }

                    file.UpdateLinkerDependencies(linkerDeps);

                    allMessages.AddRange(linkResult.Messages);
                }

                cancelToken.ThrowIfCancellationRequested();
            }

            return new ProjectCompilerResult(true, allMessages, project);
        }

        private bool AnyLinkerDependenciesUpdated(ProjectFile file)
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
    }
}
