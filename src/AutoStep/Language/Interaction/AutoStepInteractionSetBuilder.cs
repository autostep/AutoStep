using System.Collections.Generic;
using System.Linq;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{
    internal class AutoStepInteractionSetBuilder
    {
        private readonly TraitGraph traits = new TraitGraph();
        private readonly Dictionary<string, ComponentResolutionData> allComponents = new Dictionary<string, ComponentResolutionData>();
        private readonly ICallChainValidator callChainValidator;
        private InteractionConstantSet constants = new InteractionConstantSet();

        public AutoStepInteractionSetBuilder(ICallChainValidator callChainValidator)
        {
            this.callChainValidator = callChainValidator;
        }

        public void AddInteractionFile(string? sourceName, InteractionFileElement interactionFile)
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

        public AutoStepInteractionGroupBuilderResult Build(MethodTable rootMethodTable)
        {
            // Building the autostep interaction group involves:
            // 1. Going through the complete list of traits, resolving the method table for each one, and validating
            //    the methods called inside there.
            //
            //    Can't call self. Compiler error.
            //
            //    Indirect circular references will be detected at runtime.
            var messages = new List<CompilerMessage>();
            var allBoundSteps = new HashSet<InteractionStepDefinitionElement>();
            var builtComponents = new Dictionary<string, BuiltComponent>();

            traits.MethodTableWalk(rootMethodTable, (trait, methodTable) =>
            {
                // This gets invoked for each trait, with the complete method table.
                ValidateTrait(trait, methodTable, constants, messages);

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
                ResolveComponent(componentEntry, allComponents, messages);

                if (componentEntry.FinalComponent is null)
                {
                    // Could not determine the final component state; continue.
                    continue;
                }

                var finalComponent = new BuiltComponent
                {
                    Name = componentEntry.Name,
                    MethodTable = new MethodTable(rootMethodTable),
                };

                if (componentEntry.Traits is object && componentEntry.Traits.Any())
                {
                    traits.SearchTraitsSimplestFirst(componentEntry.Traits.ToArray(), finalComponent, (ctxt, trait) =>
                    {
                        foreach (var traitMethod in trait.Methods)
                        {
                            // Merge each method in.
                            ctxt.MethodTable.Set(traitMethod.Name, traitMethod);
                        }

                        foreach (var traitStep in trait.Steps)
                        {
                            allBoundSteps.Add(traitStep);
                            traitStep.AddComponentMatch(ctxt.Name);
                        }
                    });
                }

                if (componentEntry.Methods is object)
                {
                    // Do it once to update the method table...
                    foreach (var method in componentEntry.Methods.Values)
                    {
                        finalComponent.MethodTable.Set(method.Name, method);
                    }

                    // ...and again to validate the call chain.
                    foreach (var method in componentEntry.Methods.Values)
                    {
                        callChainValidator.ValidateCallChain(componentEntry.FinalComponent.SourceName, method, finalComponent.MethodTable, constants, true, messages);
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
                                messages.Add(CompilerMessageFactory.Create(
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
                    // Add any methods and steps for the component itself.
                    foreach (var step in componentEntry.Steps)
                    {
                        callChainValidator.ValidateCallChain(step.SourceName, step, finalComponent.MethodTable, constants, true, messages);

                        allBoundSteps.Add(step);
                    }
                }

                builtComponents[componentEntry.Name] = finalComponent;
            }

            return new AutoStepInteractionGroupBuilderResult(
                messages.All(x => x.Level != CompilerMessageLevel.Error),
                messages,
                new AutoStepInteractionSet(constants, builtComponents, allBoundSteps));
        }

        private void ResolveComponent(ComponentResolutionData componentData, Dictionary<string, ComponentResolutionData> allComponents, List<CompilerMessage> messages)
        {
            ResolveComponent(componentData, allComponents, messages, new Stack<ComponentResolutionData>());
        }

        private void ResolveComponent(ComponentResolutionData componentData, Dictionary<string, ComponentResolutionData> allComponents, List<CompilerMessage> messages, Stack<ComponentResolutionData> visited)
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
                    visited.Push(componentData);

                    // We are inheriting from something else.
                    // Go find it.
                    var inheritId = consideredComponent.Inherits.Name;

                    if (allComponents.TryGetValue(inheritId, out var inheritedData))
                    {
                        if (visited.Contains(inheritedData))
                        {
                            // Circular reference.
                            // Add a message with the call stack referenced.
                            messages.Add(CompilerMessageFactory.Create(
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
                                componentData.ReplaceInheritance(inheritedData, consideredComponent);
                            }
                        }
                    }

                    visited.Pop();
                }
            }

            componentData.Visited = true;
        }

        private void ValidateTrait(TraitDefinitionElement trait, MethodTable methodTable, InteractionConstantSet constants, List<CompilerMessage> messages)
        {
            // For each method definition and step.
            // Walk the call chain for the expression and validate.
            foreach (var methodDef in trait.Methods)
            {
                // Go through the call chain.
                callChainValidator.ValidateCallChain(trait.SourceName, methodDef, methodTable, constants, false, messages);
            }

            foreach (var step in trait.Steps)
            {
                callChainValidator.ValidateCallChain(trait.SourceName, step, methodTable, constants, false, messages);
            }
        }
    }
}
