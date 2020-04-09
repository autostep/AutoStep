using System.Collections.Generic;
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
        public InteractionSetBuilderResult Build(IInteractionsConfiguration interactionsConfig)
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
            });

            // 2. Go through all components, binding them to the traits, determining the full set of steps and the final method table.
            //     - Raise compiler errors on a component if it uses an undefined method.
            //     - Each component adds all steps that apply to it.
            foreach (var componentEntry in allComponents.Values)
            {
                // Resolve the component. This will determine the final inherited state of the component.
                ResolveComponent(componentEntry, allComponents, messages);

                if (componentEntry.FinalComponent is null)
                {
                    // Could not determine the final component state; continue.
                    continue;
                }

                if (componentEntry.Name is null)
                {
                    // The name cannot be null here if a final component has been selected,
                    // and the parser did its job.
                    throw new LanguageEngineAssertException();
                }

                // Create the built component.
                var finalComponent = new BuiltComponent(componentEntry.Name, new MethodTable(interactionsConfig.RootMethodTable));

                if (componentEntry.Traits is object && componentEntry.Traits.Any())
                {
                    // Search the trait graph for all traits that match this component (simplest first).
                    // We merge in the state of each trait into the final component.
                    traits.SearchTraits(componentEntry.Traits.Select(x => x.Name), finalComponent, (ctxt, trait) =>
                    {
                        foreach (var traitMethod in trait.Methods)
                        {
                            // Merge each method in.
                            ctxt.MethodTable.SetMethod(traitMethod.Key, traitMethod.Value);
                        }

                        foreach (var traitStep in trait.Steps)
                        {
                            // Add to the set of all bound steps.
                            allBoundSteps.Add(traitStep);

                            // Tell the trait step to include the component name for the $component$ placeholder.
                            traitStep.AddComponentMatch(ctxt.Name);
                        }
                    });
                }

                if (componentEntry.Methods is object)
                {
                    // First pass through the set of methods to to update the method table with the methods defined in the component.
                    foreach (var method in componentEntry.Methods.Values)
                    {
                        finalComponent.MethodTable.SetMethod(method.Name, method);
                    }

                    // ...and again to validate the call chain.
                    foreach (var method in componentEntry.Methods.Values)
                    {
                        callChainValidator.ValidateCallChain(method, finalComponent.MethodTable, interactionsConfig.Constants, true, messages);
                    }

                    // Finally, validate the method table to make sure there are no needs-defining methods left.
                    foreach (var method in finalComponent.MethodTable.Methods)
                    {
                        if (method.Value is FileDefinedInteractionMethod fileMethod)
                        {
                            if (fileMethod.NeedsDefining)
                            {
                                // A method required by one or more traits has not been defined. Indicate appropriately.
                                // Add a message to the component itself that it needs to implement it.
                                messages.Add(LanguageMessageFactory.Create(
                                    componentEntry.FinalComponent.SourceName,
                                    componentEntry.FinalComponent,
                                    CompilerMessageLevel.Error,
                                    CompilerMessageCode.InteractionMethodFromTraitRequiredButNotDefined,
                                    fileMethod.Name,
                                    fileMethod.MethodDefinition.SourceName ?? string.Empty,
                                    fileMethod.MethodDefinition.SourceLine));
                            }
                        }
                    }
                }

                if (componentEntry.Steps is object)
                {
                    // Add any steps defined on the component.
                    foreach (var step in componentEntry.Steps)
                    {
                        // Validate the step's call chain.
                        callChainValidator.ValidateCallChain(step, finalComponent.MethodTable, interactionsConfig.Constants, true, messages);

                        allBoundSteps.Add(step);
                    }
                }

                builtComponents[componentEntry.Name] = finalComponent;
            }

            return new InteractionSetBuilderResult(
                messages.All(x => x.Level != CompilerMessageLevel.Error),
                messages,
                new InteractionSet(interactionsConfig.Constants, builtComponents, allBoundSteps));
        }

        private void ResolveComponent(ComponentResolutionData componentData, Dictionary<string, ComponentResolutionData> allComponents, List<LanguageOperationMessage> messages)
        {
            ResolveComponent(componentData, allComponents, messages, new Stack<ComponentResolutionData>());
        }

        private void ResolveComponent(ComponentResolutionData componentData, Dictionary<string, ComponentResolutionData> allComponents, List<LanguageOperationMessage> messages, Stack<ComponentResolutionData> visited)
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
                            ResolveComponent(inheritedData, allComponents, messages, visited);

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
            }

            componentData.Visited = true;
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
