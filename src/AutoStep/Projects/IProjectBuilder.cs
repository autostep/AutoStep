using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Definitions;
using AutoStep.Elements.Test;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Language.Test.LineTokeniser;
using AutoStep.Language.Test.Matching;
using Microsoft.Extensions.Logging;

namespace AutoStep.Projects
{
    /// <summary>
    /// Defines an interface to a project builder, which manages compilation and linking for a project.
    /// </summary>
    public interface IProjectBuilder
    {
        /// <summary>
        /// Gets the interaction configuration, that allows the root set of methods and constants to be defined.
        /// </summary>
        IInteractionsConfiguration Interactions { get; }

        /// <summary>
        /// Add a static step definition source (i.e. one that cannot change after it is registered).
        /// </summary>
        /// <param name="source">The step definition source.</param>
        void AddStepDefinitionSource(IStepDefinitionSource source);

        /// <summary>
        /// Add an updateable step definition source (i.e. one that can change dynamically).
        /// </summary>
        /// <param name="source">The step definition source.</param>
        void AddUpdatableStepDefinitionSource(IUpdatableStepDefinitionSource source);

        /// <summary>
        /// Compile the project. Goes through all the project files and compiles those that need compilation.
        /// </summary>
        /// <param name="logFactory">A logger factory.</param>
        /// <param name="cancelToken">A cancellation token that halts compilation partway through.</param>
        /// <returns>The overall project compilation result.</returns>
        Task<ProjectBuilderResult> CompileAsync(ILoggerFactory logFactory, CancellationToken cancelToken = default);

        /// <summary>
        /// Compile the project. Goes through all the project files and compiles those that need compilation.
        /// </summary>
        /// <param name="cancelToken">A cancellation token that halts compilation partway through.</param>
        /// <returns>The overall project compilation result.</returns>
        Task<ProjectBuilderResult> CompileAsync(CancellationToken cancelToken = default);

        /// <summary>
        /// Retrieve the set of all step definition sources.
        /// </summary>
        /// <returns>The set of registered step definition sources.</returns>
        IEnumerable<IStepDefinitionSource> EnumerateStepDefinitionSources();

        /// <summary>
        /// Links the entire project. Files that need to be re-linked will be.
        /// </summary>
        /// <param name="cancelToken">A cancellation token for the linker process.</param>
        /// <returns>The overall project link result.</returns>
        ProjectBuilderResult Link(CancellationToken cancelToken = default);

        /// <summary>
        /// Tokenises a line of text, returning a set of line tokens. Used mostly for syntax highlighting; faster than a regular compile.
        /// </summary>
        /// <param name="line">The line of text to tokenise.</param>
        /// <param name="lastTokeniserState">
        /// The value of <see cref="LineTokeniseResult{TStateIndicator}.EndState"/> from
        /// the previous call to this method for the same file.
        /// </param>
        /// <returns>The tokenisation result.</returns>
        LineTokeniseResult<LineTokeniserState> TokeniseTestLine(string line, LineTokeniserState lastTokeniserState = LineTokeniserState.Default);

        /// <summary>
        /// Tokenises a line of text, returning a set of line tokens. Used mostly for syntax highlighting; faster than a regular compile.
        /// </summary>
        /// <param name="line">The line of text to tokenise.</param>
        /// <param name="lastTokeniserState">
        /// The value of <see cref="LineTokeniseResult{TStateIndicator}.EndState"/> from
        /// the previous call to this method for the same file.
        /// </param>
        /// <returns>The tokenisation result.</returns>
        LineTokeniseResult<int> TokeniseInteractionLine(string line, int lastTokeniserState = 0);

        /// <summary>
        /// Retrieves the set of possible matches for the step definition.
        /// </summary>
        /// <param name="element">The step reference.</param>
        /// <returns>The set of results.</returns>
        IEnumerable<IMatchResult> GetPossibleStepDefinitions(StepReferenceElement element);

        /// <summary>
        /// Retrieves the last built interaction set (or null if there is no active interaction set).
        /// </summary>
        /// <returns>The active interaction set.</returns>
        public IInteractionSet? GetCurrentInteractionSet();
    }
}
