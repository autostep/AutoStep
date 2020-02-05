using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Definitions;
using AutoStep.Elements.StepTokens;
using AutoStep.Elements.Test;
using AutoStep.Language.Test.Matching;
using Microsoft.Extensions.Logging;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// Links compiled autostep content.
    /// </summary>
    /// <remarks>
    /// Linker can hold state, to be able to know what has changed.
    /// The output of the compiler can be fed repeatedly into the linker, to update the references.
    /// </remarks>
    public class AutoStepLinker : IAutoStepLinker
    {
        private readonly IAutoStepCompiler compiler;
        private readonly IMatchingTree linkerTree;
        private readonly Dictionary<string, StepSourceWithTracking> trackedSources = new Dictionary<string, StepSourceWithTracking>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepLinker"/> class.
        /// </summary>
        /// <param name="compiler">The autostep compiler to use when processing definition statements.</param>
        public AutoStepLinker(IAutoStepCompiler compiler)
        {
            this.compiler = compiler;
            linkerTree = new MatchingTree();
        }

        /// <summary>
        /// Gets the set of all known step definition sources.
        /// </summary>
        public IEnumerable<IStepDefinitionSource> AllStepDefinitionSources => trackedSources.Values.Select(x => x.Source);

        /// <summary>
        /// Adds a source of step definitions to the linker.
        /// </summary>
        /// <param name="source">The source to add.</param>
        public void AddStepDefinitionSource(IStepDefinitionSource source)
        {
            source = source.ThrowIfNull(nameof(source));

            if (trackedSources.Remove(source.Uid, out var oldSource))
            {
                // Unload an existing source with the same UID.
                oldSource.DeleteAllSteps(linkerTree);
            }

            var trackedSource = new StepSourceWithTracking(source);

            // Add/Replace the source.
            trackedSources.Add(source.Uid, trackedSource);

            // Load all the step definitions we have.
            RefreshStepDefinitions(trackedSource);
        }

        /// <summary>
        /// Add/Update an updatable step definition source. All step definitions in the source will be refreshed.
        /// </summary>
        /// <param name="stepDefinitionSource">The step definition source.</param>
        public void AddOrUpdateStepDefinitionSource(IUpdatableStepDefinitionSource stepDefinitionSource)
        {
            if (stepDefinitionSource is null)
            {
                throw new ArgumentNullException(nameof(stepDefinitionSource));
            }

            if (!trackedSources.TryGetValue(stepDefinitionSource.Uid, out var tracked))
            {
                tracked = new StepSourceWithTracking(stepDefinitionSource);
                trackedSources.Add(stepDefinitionSource.Uid, tracked);
            }

            tracked.UpdateSteps(linkerTree, stepDefinitionSource.GetStepDefinitions());
        }

        /// <summary>
        /// Remove a step definition source.
        /// </summary>
        /// <param name="stepDefinitionSource">The step definition source to remove.</param>
        public void RemoveStepDefinitionSource(IStepDefinitionSource stepDefinitionSource)
        {
            if (stepDefinitionSource is null)
            {
                throw new ArgumentNullException(nameof(stepDefinitionSource));
            }

            if (trackedSources.Remove(stepDefinitionSource.Uid, out var trackedSource))
            {
                trackedSource.DeleteAllSteps(linkerTree);
            }
            else
            {
                throw new InvalidOperationException(AutoStepLinkerMessages.CannotRemoveDefinitionSource);
            }
        }

        /// <summary>
        /// Links (or re-links) a built autostep file; all step references that can be updated with step definition bindings will be.
        /// </summary>
        /// <param name="file">The file to link.</param>
        /// <returns>A link result (including a reference to the same file).</returns>
        public LinkResult Link(FileElement file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var messages = new List<CompilerMessage>();
            bool success = true;

            var allFoundStepSources = new Dictionary<string, IStepDefinitionSource>();

            // Go through all the steps and link them.
            foreach (var stepRef in file.AllStepReferences)
            {
                if (BindSingleStep(stepRef, file.SourceName, messages))
                {
                    var defSource = stepRef.Binding!.Definition.Source;

                    allFoundStepSources.TryAdd(defSource.Uid, defSource);
                }
                else
                {
                    success = false;
                }
            }

            return new LinkResult(success, messages, allFoundStepSources.Values, file);
        }

        /// <summary>
        /// Bind a single step.
        /// </summary>
        /// <param name="stepReference">The step reference.</param>
        /// <param name="sourceName">The relevant source for any messages.</param>
        /// <param name="messages">An optional set to add messages to.</param>
        /// <returns>True if binding is successful, false otherwise.</returns>
        public bool BindSingleStep(StepReferenceElement stepReference, string? sourceName = null, IList<CompilerMessage>? messages = null)
        {
            if (stepReference is null)
            {
                throw new ArgumentNullException(nameof(stepReference));
            }

            var matches = linkerTree.Match(stepReference, true, out var _);
            var success = true;

            if (matches.Count == 0 || !matches.First.Value.IsExact)
            {
                // No matches.
                if (messages is object)
                {
                    messages.Add(CompilerMessageFactory.Create(sourceName, stepReference, CompilerMessageLevel.Error, CompilerMessageCode.LinkerNoMatchingStepDefinition));
                }

                stepReference.Unbind();
                success = false;
            }
            else if (matches.Count > 1)
            {
                if (messages is object)
                {
                    messages.Add(CompilerMessageFactory.Create(sourceName, stepReference, CompilerMessageLevel.Error, CompilerMessageCode.LinkerMultipleMatchingDefinitions));
                }

                stepReference.Unbind();
                success = false;
            }
            else
            {
                var foundMatch = matches.First.Value;

                // Link successful, bind the reference.
                if (foundMatch.ArgumentSet is object && messages is object)
                {
                    // Run argument validation on the step (doesn't prevent it binding).
                    if (!ValidateArgumentBinding(sourceName, foundMatch.ArgumentSet, messages))
                    {
                        success = false;
                    }
                }

                // Needs to materialise the argument set here. Assign the reference binding.
                var referenceBinding = new StepReferenceBinding(foundMatch.Definition, foundMatch.ArgumentSet?.ToArray());

                stepReference.Bind(referenceBinding);
            }

            return success;
        }

        private bool ValidateArgumentBinding(string? sourceName, IEnumerable<ArgumentBinding> argumentSet, IList<CompilerMessage> messages)
        {
            var success = true;

            foreach (var arg in argumentSet)
            {
                var bindingMessage = GetBindingMessage(sourceName, arg);

                if (bindingMessage is object)
                {
                    if (bindingMessage.Level == CompilerMessageLevel.Error)
                    {
                        success = false;
                    }

                    messages.Add(bindingMessage);
                }
            }

            return success;
        }

        /// <summary>
        /// Generate an optional compiler message for the matched argument. Only invoked on a successful exact match for the entire step reference.
        /// </summary>
        /// <returns>An optional compiler message.</returns>
        private static CompilerMessage? GetBindingMessage(string? sourceName, ArgumentBinding binding)
        {
            var part = binding.Part;
            var resultTokens = binding.MatchedTokens.AsSpan();
            var rangeTestTokens = resultTokens;

            var containsWhiteSpacePadding = false;

            if (resultTokens.Length > 1)
            {
                if (binding.StartExclusive)
                {
                    if (!rangeTestTokens[0].IsImmediatelyFollowedBy(rangeTestTokens[1]))
                    {
                        // There is whitespace.
                        containsWhiteSpacePadding = true;
                    }

                    // Ignore the first token.
                    resultTokens = resultTokens.Slice(1);
                }

                if (binding.EndExclusive)
                {
                    // If the last token is not immediately after the penultimate one.
                    if (!rangeTestTokens[rangeTestTokens.Length - 2].IsImmediatelyFollowedBy(rangeTestTokens[rangeTestTokens.Length - 1]))
                    {
                        // We have whitespace.
                        containsWhiteSpacePadding = true;
                    }

                    // Ignore the last character.
                    resultTokens = resultTokens.Slice(0, resultTokens.Length - 1);
                }
            }

            ArgumentType? effectiveType;
            CompilerMessage? message = null;

            if (resultTokens.Length == 0)
            {
                // No tokens either means entirely whitespace or an empty string.
                effectiveType = ArgumentType.Text;

                // No tokens. This is a problem for int and decimal types, which expect a value.
                if (part.TypeHint == ArgumentType.NumericDecimal || part.TypeHint == ArgumentType.NumericInteger)
                {
                    // We are expecting a value.
                    message = CompilerMessageFactory.Create(sourceName, CompilerMessageLevel.Error, CompilerMessageCode.TypeRequiresValueForArgument, binding, part.TypeHint);
                }
            }
            else if (resultTokens.Length > 1 || containsWhiteSpacePadding)
            {
                // If there's more than 1 token, or contains any whitespace, we are always going to be text or unknown.
                // (because if everything could go in one token, i.e. an int, a float, then it will).
                effectiveType = ArgumentType.Text;

                foreach (var knownToken in resultTokens)
                {
                    if (knownToken is InterpolateStartToken || knownToken is VariableToken)
                    {
                        // Effective type cannot be known.
                        effectiveType = null;
                        break;
                    }
                }
            }
            else
            {
                effectiveType = resultTokens[0] switch
                {
                    TextToken _ => ArgumentType.Text,
                    EscapedCharToken _ => ArgumentType.Text,
                    QuoteToken _ => ArgumentType.Text,
                    IntToken _ => ArgumentType.NumericInteger,
                    FloatToken _ => ArgumentType.NumericDecimal,

                    InterpolateStartToken _ => null, // cannot know the type of an interpolation result
                    VariableToken _ => null, // cannot know the type of a variable insert
                    _ => null
                };
            }

            // Store the determined type for later use.
            binding.DeterminedType = effectiveType;

            if (message is null && effectiveType != null && part.TypeHint != null && effectiveType < part.TypeHint)
            {
                // The type hint indicates an incompatible type assignment (float can't be assigned to int, text can't be assigned to int, etc).
                message = CompilerMessageFactory.Create(sourceName, CompilerMessageLevel.Error, CompilerMessageCode.ArgumentTypeNotCompatible, binding, effectiveType, part.TypeHint);
            }

            return message;
        }

        private void RefreshStepDefinitions(StepSourceWithTracking trackedSource)
        {
            var definitions = trackedSource.Source.GetStepDefinitions();

            // Make sure we have definitions.
            foreach (var stepDef in definitions)
            {
                if (stepDef.Definition is null)
                {
                    var definitionResult = compiler.CompileStepDefinitionElementFromStatementBody(stepDef.Type, stepDef.Declaration);

                    if (definitionResult.Success)
                    {
                        stepDef.Definition = definitionResult.Output;
                    }
                }
            }

            trackedSource.UpdateSteps(linkerTree, definitions);
        }

        private class StepSourceWithTracking
        {
            private readonly Dictionary<(StepType Type, string Declaration), StepDefinition> trackedSteps;

            public StepSourceWithTracking(IStepDefinitionSource source)
            {
                Source = source;
                trackedSteps = new Dictionary<(StepType Type, string Declaration), StepDefinition>();
            }

            public IStepDefinitionSource Source { get; }

            public void DeleteAllSteps(IMatchingTree tree) => UpdateSteps(tree, Enumerable.Empty<StepDefinition>());

            public void UpdateSteps(IMatchingTree tree, IEnumerable<StepDefinition> replaceDefinitions)
            {
                if (replaceDefinitions is null)
                {
                    throw new ArgumentNullException(nameof(replaceDefinitions));
                }

                var keysToRemove = new List<(StepType Type, string Declaration)>(trackedSteps.Keys);

                foreach (var compiledStepDef in replaceDefinitions)
                {
                    if (compiledStepDef.Declaration is null)
                    {
                        // We can't use the compiled step definition if it has no declaration.
                        continue;
                    }

                    var key = (compiledStepDef.Type, compiledStepDef.Declaration);

                    // There are some step definitions.
                    // Look at the declaration and see if we need to update a definition.
                    if (trackedSteps.TryGetValue(key, out var existingDef))
                    {
                        // There is already a definition with the same 'signature'. So we can just do an in-place swap.
                        existingDef.Definition = compiledStepDef.Definition;

                        // Don't remove this one, we've found it again.
                        keysToRemove.Remove(key);
                    }
                    else
                    {
                        // Need to add a new one.
                        trackedSteps.Add((compiledStepDef.Type, compiledStepDef.Declaration), compiledStepDef);

                        // Update the matching tree.
                        tree.AddOrUpdateDefinition(compiledStepDef);
                    }
                }

                foreach (var item in keysToRemove)
                {
                    if (trackedSteps.Remove(item, out var existingDef))
                    {
                        // Remove from the matching tree.
                        tree.RemoveDefinition(existingDef);
                    }
                }
            }
        }
    }
}
