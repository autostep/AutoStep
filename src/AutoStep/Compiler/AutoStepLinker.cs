using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;
using AutoStep.Matching;
using AutoStep.Sources;
using AutoStep.Tracing;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Links compiled autostep content.
    /// </summary>
    /// <remarks>
    /// Linker can hold state, to be able to know what has changed.
    /// The output of the compiler can be fed repeatedly into the linker, to update the references.
    /// </remarks>
    public class AutoStepLinker
    {
        private readonly IAutoStepCompiler compiler;
        private readonly IMatchingTree linkerTree;
        private readonly ITracer? tracer;

        /// <summary>
        /// Contains the lookup of UID -> Source.
        /// </summary>
        private Dictionary<string, IStepDefinitionSource> stepDefinitionSources = new Dictionary<string, IStepDefinitionSource>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepLinker"/> class.
        /// </summary>
        /// <param name="compiler">The autostep compiler to use when processing definition statements.</param>
        public AutoStepLinker(IAutoStepCompiler compiler)
            : this(compiler, new MatchingTree())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepLinker"/> class, providing a custom matcher tree.
        /// </summary>
        /// <param name="compiler">The autostep compiler to use when processing definition statements.</param>
        /// <param name="linkerTree">A custom matcher tree used for linking.</param>
        public AutoStepLinker(IAutoStepCompiler compiler, IMatchingTree linkerTree)
        {
            this.compiler = compiler;
            this.linkerTree = linkerTree;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepLinker"/> class, providing a custom matcher tree.
        /// </summary>
        /// <param name="compiler">The autostep compiler to use when processing definition statements.</param>
        /// <param name="linkerTree">A custom matcher tree used for linking.</param>
        /// <param name="tracer">A tracer for the operations of the linker.</param>
        public AutoStepLinker(IAutoStepCompiler compiler, IMatchingTree linkerTree, ITracer tracer)
            : this(compiler, linkerTree)
        {
            this.tracer = tracer;
        }

        /// <summary>
        /// Generates a step definition from a statement body/declaration.
        /// </summary>
        /// <param name="stepType">The type of step.</param>
        /// <param name="statementBody">The body of the step.</param>
        /// <returns>The step definition parsing result (which may contain errors).</returns>
        public StepDefinitionFromBodyResult GetStepDefinitionElementFromStatementBody(StepType stepType, string statementBody)
        {
            if (statementBody is null)
            {
                throw new ArgumentNullException(nameof(statementBody));
            }

            // Trim first, we don't want to worry about the whitespace at the end.
            statementBody = statementBody.Trim();

            var errors = new List<CompilerMessage>();
            var success = false;

            // Compile the text, specifying a starting lexical mode of 'statement'.
            var parseContext = compiler.CompileEntryPoint(statementBody, null, p => p.statementBody(), out var tokenStream, out var parserErrors, AutoStepLexer.statement);

            if (parserErrors.Any(x => x.Level == CompilerMessageLevel.Error))
            {
                errors.AddRange(parserErrors);
            }

            // Now we need a visitor.
            var stepReferenceVisitor = new StepReferenceVisitor(null, tokenStream);

            // Construct a 'reference' step.
            var stepReference = stepReferenceVisitor.BuildStep(stepType, null, parseContext);

            StepDefinitionElement? definition = null;

            if (stepReference != null)
            {
                definition = new StepDefinitionElement { SourceLine = 1, SourceColumn = 1 };

                definition.UpdateFromStepReference(stepReference);

                success = true;

                if (definition.Arguments is object)
                {
                    // At this point, we'll validate the provided 'arguments' to the step. All the arguments should just be variable names.
                    foreach (var declaredArgument in definition.Arguments)
                    {
                        if (declaredArgument.Type == ArgumentType.Empty)
                        {
                            errors.Add(CompilerMessageFactory.Create(null, declaredArgument, CompilerMessageLevel.Error, CompilerMessageCode.StepVariableNameRequired));
                            success = false;
                        }
                        else if (declaredArgument.Value is null)
                        {
                            // If the value cannot be immediately determined, it means there is some dynamic component (e.g. insertion variables or example inserts).
                            // Everything else is allowed.
                            var argumentName = declaredArgument.RawArgument;

                            if (declaredArgument.Type == ArgumentType.Interpolated)
                            {
                                argumentName = ":" + argumentName;
                            }

                            errors.Add(CompilerMessageFactory.Create(null, declaredArgument, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition, argumentName!));
                            success = false;
                        }
                    }
                }
            }

            return new StepDefinitionFromBodyResult(success, errors, definition);
        }

        /// <summary>
        /// Adds a source of step definitions to the linker.
        /// </summary>
        /// <param name="source">The source to add.</param>
        public void AddStepDefinitionSource(IStepDefinitionSource source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // TODO: unload an existing source with the same UID?

            // Add/Replace the source.
            stepDefinitionSources[source.Uid] = source;

            // Load all the step definitions we have.
            RefreshStepDefinitions(source);
        }

        private void RefreshStepDefinitions(IStepDefinitionSource source)
        {
            var definitions = source.GetStepDefinitions();

            foreach (var stepDef in definitions)
            {
                if (stepDef.Definition is null)
                {
                    var definitionResult = GetStepDefinitionElementFromStatementBody(stepDef.Type, stepDef.Declaration);

                    if (definitionResult.Success)
                    {
                        stepDef.Definition = definitionResult.StepDefinition;
                    }
                }

                if (stepDef.Definition is object)
                {
                    // Add to the internal matching tree.
                    linkerTree.AddDefinition(stepDef);
                }
            }
        }

        /// <summary>
        /// Links (or re-links) a built autostep file; all step references that can be updated with step definition bindings will be.
        /// </summary>
        /// <param name="file">The file to link.</param>
        /// <returns>A link result (including a reference to the same file).</returns>
        public LinkResult Link(BuiltFile file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var messages = new List<CompilerMessage>();
            bool success = true;

            // Go through all the steps and link them.
            foreach (var stepRef in file.AllStepReferences)
            {
                var matches = linkerTree.Match(stepRef, true, out var _);

                if (matches.Count == 0)
                {
                    // No matches.
                    messages.Add(CompilerMessageFactory.Create(file.SourceName, stepRef, CompilerMessageLevel.Error, CompilerMessageCode.LinkerNoMatchingStepDefinition));
                    stepRef.Unbind();
                    success = false;
                }
                else if (matches.Count > 1)
                {
                    messages.Add(CompilerMessageFactory.Create(file.SourceName, stepRef, CompilerMessageLevel.Error, CompilerMessageCode.LinkerMultipleMatchingDefinitions));
                    stepRef.Unbind();
                    success = false;
                }
                else
                {
                    // Link successful, bind the reference.
                    stepRef.Bind(matches.First.Value.Definition);
                }
            }

            return new LinkResult(success, messages, file);
        }
    }
}
