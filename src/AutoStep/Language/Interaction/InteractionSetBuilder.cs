using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Provides the functionality to generate an interaction set from a set of files.
    /// </summary>
    internal class InteractionSetBuilder : IInteractionSetBuilder
    {
        private readonly TraitGraph traits = new TraitGraph();
        private readonly Dictionary<string, ComponentResolutionData> allComponents = new Dictionary<string, ComponentResolutionData>();
        private readonly ICallChainValidator callChainValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionSetBuilder"/> class.
        /// </summary>
        /// <param name="callChainValidator">The call chain validator.</param>
        public InteractionSetBuilder(ICallChainValidator callChainValidator)
        {
            this.callChainValidator = callChainValidator;
        }

        /// <inheritdoc/>
        public void AddInteractionFile(InteractionFileElement interactionFile)
        {
            interactionFile = interactionFile.ThrowIfNull(nameof(interactionFile));

            traits.Merge(interactionFile.TraitGraph);

            // Merge in the components. We'll correlate everything in a moment.
            foreach (var comp in interactionFile.Components)
            {
                if (!allComponents.TryGetValue(comp.Id, out var allDefSet))
                {
                    allComponents[comp.Id] = allDefSet = new ComponentResolutionData(comp.Id);
                }

                allDefSet.AllComponents.Add(comp);
                allDefSet.Visited = false;
            }
        }

        /// <inheritdoc/>
        public InteractionSetBuilderResult Build(IInteractionsConfiguration interactionsConfig, bool collectExtendedMethodTableReferences)
        {
            // Building the autostep interaction group involves:
            // 1. Going through the complete list of traits, resolving the method table for each one, and validating
            //    the methods called inside there.
            //
            //    Can't call self. Compiler error.
            //
            //    Indirect circular references will be detected at runtime.
            var messages = new List<LanguageOperationMessage>();
            var allBoundSteps = new HashSet<InteractionStepDefinitionElement>();
            var builtComponents = new Dictionary<string, BuiltComponent>();
            ExtendedMethodTableReferences? extendedRef = null;

            if (collectExtendedMethodTableReferences)
            {
                extendedRef = new ExtendedMethodTableReferences();
            }

            traits.MethodTableWalk(interactionsConfig.RootMethodTable, (trait, methodTable) =>
            {
                // This gets invoked for each trait, with the complete method table.
                ValidateTrait(trait, methodTable, interactionsConfig.Constants, messages);

                // In each trait, for each step, we need to reset the registered matching components.
                // These may be added back in a minute.
                foreach (var step in trait.Steps)
                {
                    step.ClearAllComponentMatchData();
                }

                if (extendedRef is object)
                {
                    extendedRef.AddMethodTableReference(trait, methodTable);
                }
            });

            // 2. Go through all components, binding them to the traits, determining the full set of steps and the final method table.
            //     - Raise compiler errors on a component if it uses an undefined method.
            //     - Each component adds all steps that apply to it.
            foreach (var componentEntry in allComponents.Values)
            {
                // Resolve the component. This will determine the final inherited state of the component.
                var finalComponent = ResolveComponent(componentEntry, allComponents, messages, allBoundSteps, interactionsConfig, extendedRef);

                if (finalComponent is null)
                {
                    // Could not determine the final component state; continue.
                    continue;
                }

                builtComponents[finalComponent.Name] = finalComponent;
            }

            return new InteractionSetBuilderResult(
                messages.All(x => x.Level != CompilerMessageLevel.Error),
                messages,
                new InteractionSet(interactionsConfig.Constants, builtComponents, allBoundSteps, extendedRef));
        }

        private BuiltComponent? ResolveComponent(
            ComponentResolutionData componentData,
            Dictionary<string, ComponentResolutionData> allComponents,
            List<LanguageOperationMessage> messages,
            HashSet<InteractionStepDefinitionElement> interactionStepDefs,
            IInteractionsConfiguration interactionsConfig,
            ExtendedMethodTableReferences? extendedRef)
        {
            ResolveComponent(componentData, allComponents, messages, interactionsConfig, interactionStepDefs, extendedRef, new Stack<ComponentResolutionData>());

            if (componentData.FinalComponent is object)
            {
                if (componentData.Name is null)
                {
                    // The name cannot be null here if a final component has been selected,
                    // and the parser did its job.
                    throw new LanguageEngineAssertException();
                }

                return new BuiltComponent(componentData.Name, componentData.FinalMethodTable ?? interactionsConfig.RootMethodTable);
            }

            return null;
        }

        private void ResolveComponent(
            ComponentResolutionData componentData,
            Dictionary<string, ComponentResolutionData> allComponents,
            List<LanguageOperationMessage> messages,
            IInteractionsConfiguration interactionsConfig,
            HashSet<InteractionStepDefinitionElement> allSteps,
            ExtendedMethodTableReferences? extendedRef,
            Stack<ComponentResolutionData> visited)
        {
            // We've already visited this component; it's in as good a state as it's going to be.
            if (componentData.Visited)
            {
                return;
            }

            // For each candidate component, we need the full list of steps and the full list of traits.
            // The last one wins in all cases, but we'll walk forwards to make inheriting easier.
            for (var compIdx = 0; compIdx < componentData.AllComponents.Count; compIdx++)
            {
                var consideredComponent = componentData.AllComponents[compIdx];

                if (consideredComponent.Inherits is null)
                {
                    componentData.Replace(consideredComponent);
                }
                else if (consideredComponent.Inherits.Name == componentData.Id)
                {
                    // We are inheriting from ourselves.
                    // Merge this component onto the current state.
                    componentData.Merge(consideredComponent);
                }
                else
                {
                    // We are inheriting from something else.
                    // Go find it.
                    var inheritId = consideredComponent.Inherits.Name;

                    if (allComponents.TryGetValue(inheritId, out var inheritedData))
                    {
                        visited.Push(componentData);

                        if (visited.Contains(inheritedData))
                        {
                            // Circular reference.
                            // Add a message with the call stack referenced.
                            messages.Add(LanguageMessageFactory.Create(
                                consideredComponent.SourceName,
                                consideredComponent,
                                CompilerMessageLevel.Error,
                                CompilerMessageCode.InteractionComponentInheritanceLoop,
                                string.Join(" -> ", visited.Reverse().Select(x => x.Id).Concat(new[] { inheritId }))));
                        }
                        else
                        {
                            ResolveComponent(inheritedData, allComponents, messages, interactionsConfig, allSteps, extendedRef, visited);

                            // There is at least something to inherit from.
                            if (inheritedData.FinalComponent is object)
                            {
                                componentData.RebaseOnOtherComponent(inheritedData, consideredComponent);
                            }
                        }

                        visited.Pop();
                    }
                    else
                    {
                        // Inheritance error, referenced field does not exist.
                        messages.Add(LanguageMessageFactory.Create(
                            consideredComponent.SourceName,
                            consideredComponent.Inherits,
                            CompilerMessageLevel.Error,
                            CompilerMessageCode.InteractionComponentInheritedComponentNotFound,
                            inheritId));
                    }
                }

                if (componentData.FinalComponent is object)
                {
                    var isLastComponentToProcess = compIdx == componentData.AllComponents.Count - 1;

                    ProcessComponentData(interactionsConfig, componentData, messages, allSteps, isLastComponentToProcess);

                    if (extendedRef is object && componentData.FinalMethodTable is object)
                    {
                        extendedRef.AddMethodTableReference(componentData.FinalComponent, componentData.FinalMethodTable);
                    }
                }
            }

            componentData.Visited = true;
        }

        private void ProcessComponentData(
            IInteractionsConfiguration interactionsConfig,
            ComponentResolutionData currentData,
            List<LanguageOperationMessage> messages,
            HashSet<InteractionStepDefinitionElement> allSteps,
            bool isConcreteComponent)
        {
            var finalMethodTable = new MethodTable(interactionsConfig.RootMethodTable);

            if (currentData.Traits is object && currentData.Traits.Any())
            {
                // Need a set of trait steps that we want to consider registering this component with.
                List<InteractionStepDefinitionElement?>? consideringTraitSteps = null;

                if (isConcreteComponent)
                {
                    consideringTraitSteps = new List<InteractionStepDefinitionElement?>();
                }

                // Search the trait graph for all traits that match this component (simplest first).
                // We merge in the state of each trait into the final component.
                traits.SearchTraits(currentData.Traits.Select(x => x.Name), finalMethodTable, (ctxt, trait) =>
                {
                    foreach (var traitMethod in trait.Methods)
                    {
                        // Merge each method in.
                        ctxt.SetMethod(traitMethod.Key, traitMethod.Value);
                    }

                    if (isConcreteComponent)
                    {
                        consideringTraitSteps!.AddRange(trait.Steps);
                    }
                });

                if (isConcreteComponent)
                {
                    ApplyComponentToStepCandidates(currentData, allSteps, consideringTraitSteps!);
                }
            }

            if (currentData.Methods is object)
            {
                // First pass through the set of methods to to update the method table with the methods defined in the component.
                foreach (var method in currentData.Methods.Values)
                {
                    finalMethodTable.SetMethod(method.Name, method);
                }

                // ...and again to validate the call chain.
                foreach (var method in currentData.Methods.Values)
                {
                    callChainValidator.ValidateCallChain(method, finalMethodTable, interactionsConfig.Constants, isConcreteComponent, messages);
                }

                if (isConcreteComponent && currentData.FinalComponent is object)
                {
                    // Finally, validate the method table to make sure there are no needs-defining methods left.
                    foreach (var method in finalMethodTable.Methods)
                    {
                        if (method.Value is FileDefinedInteractionMethod fileMethod)
                        {
                            if (fileMethod.NeedsDefining)
                            {
                                // A method required by one or more traits has not been defined. Indicate appropriately.
                                // Add a message to the component itself that it needs to implement it.
                                messages.Add(LanguageMessageFactory.Create(
                                    currentData.FinalComponent.SourceName,
                                    currentData.FinalComponent,
                                    CompilerMessageLevel.Error,
                                    CompilerMessageCode.InteractionMethodFromTraitRequiredButNotDefined,
                                    fileMethod.Name,
                                    fileMethod.MethodDefinition.SourceName ?? string.Empty,
                                    fileMethod.MethodDefinition.SourceLine));
                            }
                        }
                    }
                }
            }

            if (currentData.Steps is object)
            {
                // Add any steps defined on the component.
                foreach (var step in currentData.Steps)
                {
                    // Validate the step's call chain.
                    callChainValidator.ValidateCallChain(step, finalMethodTable, interactionsConfig.Constants, isConcreteComponent, messages);

                    if (isConcreteComponent)
                    {
                        allSteps.Add(step);
                    }
                }
            }

            currentData.FinalMethodTable = finalMethodTable;
        }

        /// <summary>
        /// For a given set of steps that came from traits, register the component as being applicable to each step.
        /// Consider inheritance of steps, where a more complex trait that declares step can override a simpler trait's declaration of the same step.
        /// </summary>
        private static void ApplyComponentToStepCandidates(ComponentResolutionData currentData, HashSet<InteractionStepDefinitionElement> allSteps, List<InteractionStepDefinitionElement?> candidateTraitSteps)
        {
            // Walk backwards through the set of considered trait steps (most complex first).
            // For each one, we walk through the remainder of the traits, and if they have the same declaration signature, then we remove them.
            for (var currentPos = candidateTraitSteps.Count - 1; currentPos >= 0; currentPos--)
            {
                var item = candidateTraitSteps[currentPos];

                // This item has already been cleared by an earlier, more complex step.
                if (item is null)
                {
                    continue;
                }

                var declarationSignature = item.GetDeclarationOnlySignature();

                allSteps.Add(item);

                // Tell the trait step to include the component name for the $component$ placeholder.
                item.AddComponentMatch(currentData.Name!);

                // Now, continue down the set of steps for the component; we will blank out
                // steps with the same declaration text from a less complex trait.
                for (var nestedCurrentPos = currentPos - 1; nestedCurrentPos >= 0; nestedCurrentPos--)
                {
                    var nestedSearchItem = candidateTraitSteps[nestedCurrentPos];

                    if (nestedSearchItem is null)
                    {
                        continue;
                    }

                    if (nestedSearchItem.GetDeclarationOnlySignature() == declarationSignature)
                    {
                        // Found a match, null it out.
                        candidateTraitSteps[nestedCurrentPos] = null;
                    }
                }
            }
        }

        private void ValidateTrait(TraitDefinitionElement trait, MethodTable methodTable, InteractionConstantSet constants, List<LanguageOperationMessage> messages)
        {
            // For each method definition and step.
            // Walk the call chain for the expression and validate.
            foreach (var methodDef in trait.Methods)
            {
                // Go through the call chain.
                callChainValidator.ValidateCallChain(methodDef.Value, methodTable, constants, false, messages);
            }

            foreach (var step in trait.Steps)
            {
                callChainValidator.ValidateCallChain(step, methodTable, constants, false, messages);
            }
        }
    }
}
