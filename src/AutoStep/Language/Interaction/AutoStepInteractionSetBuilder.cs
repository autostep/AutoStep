using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Definitions;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{
    internal class AutoStepInteractionSetBuilder
    {
        private class ComponentResolutionData
        {
            private bool createdOwnSteps;

            public ComponentResolutionData(string id)
            {
                Id = id;
            }

            public List<ComponentDefinitionElement> AllComponents { get; } = new List<ComponentDefinitionElement>();

            public bool Visited { get; set; }

            public string Id { get; }

            public string Name { get; set; }

            public ComponentDefinitionElement? FinalComponent { get; set; }

            public Dictionary<string, MethodDefinitionElement>? Methods { get; set; }

            public List<InteractionStepDefinitionElement>? Steps { get; set; }

            public IReadOnlyList<NameRefElement>? Traits { get; set; }

            public void Replace(ComponentDefinitionElement definition)
            {
                Methods = definition.Methods.ToDictionary(x => x.Name);
                Steps = definition.Steps;
                Traits = definition.Traits;
                Name = definition.Name;
                FinalComponent = definition;
            }

            public void ReplaceInheritance(ComponentResolutionData baseData, ComponentDefinitionElement derivedDefinition)
            {
                if (baseData.Methods is object)
                {
                    Methods = new Dictionary<string, MethodDefinitionElement>(baseData.Methods);
                }
                else
                {
                    Methods = null;
                }

                if (baseData.Steps is object)
                {
                    Steps = baseData.Steps;
                }
                else
                {
                    Steps = null;
                }

                Traits = baseData.Traits;

                Merge(derivedDefinition);
            }

            public void Merge(ComponentDefinitionElement definition)
            {
                // Method replacements.
                if (Methods is null)
                {
                    if (definition.Methods.Any())
                    {
                        Methods = definition.Methods.ToDictionary(x => x.Name);
                    }
                }
                else if (definition.Methods.Any())
                {
                    foreach (var def in definition.Methods)
                    {
                        Methods[def.Name] = def;
                    }
                }

                if (Steps is null)
                {
                    if (definition.Steps.Any())
                    {
                        Steps = definition.Steps;
                    }
                }
                else if (definition.Steps.Any())
                {
                    if (!createdOwnSteps)
                    {
                        // Now we need to copy the list.
                        Steps = new List<InteractionStepDefinitionElement>(Steps);
                        createdOwnSteps = true;
                    }

                    Steps.AddRange(definition.Steps);
                }

                if (definition.Traits.Any())
                {
                    // Traits aren't merged, they are simply added.
                    Traits = definition.Traits;
                }

                Name = definition.Name;
                FinalComponent = definition;
            }
        }

        private readonly TraitGraph traits = new TraitGraph();
        private readonly Dictionary<string, ComponentResolutionData> allComponents = new Dictionary<string, ComponentResolutionData>();
        private InteractionConstantSet constants = new InteractionConstantSet();

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
                        ValidateCallChain(componentEntry.FinalComponent.SourceName, method, finalComponent.MethodTable, constants, true, messages);
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
                        ValidateCallChain(step.SourceName, step, finalComponent.MethodTable, constants, true, messages);

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

                    break;
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
                        if (visited.Contains(inheritedData))
                        {
                            // Circular reference.
                            // Add a message with the call stack referenced.
                            messages.Add(CompilerMessageFactory.Create(
                                consideredComponent.SourceName,
                                consideredComponent,
                                CompilerMessageLevel.Error,
                                CompilerMessageCode.InteractionComponentInheritanceLoop,
                                string.Join(" -> ", visited.Select(x => x.Id).Concat(new[] { inheritId }))));
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
                ValidateCallChain(trait.SourceName, methodDef, methodTable, constants, false, messages);
            }

            foreach (var step in trait.Steps)
            {
                ValidateCallChain(trait.SourceName, step, methodTable, constants, false, messages);
            }
        }

        private void ValidateCallChain(string? sourceFileName, IMethodCallSource definition, MethodTable methodTable, InteractionConstantSet constants, bool requireMethodDefinitions, List<CompilerMessage> messages)
        {
            var variableSet = definition.GetInitialMethodChainVariables();

            foreach (var call in definition.MethodCallChain)
            {
                // Validate the arguments.
                for (var callArgIdx = 0; callArgIdx < call.Arguments.Count; callArgIdx++)
                {
                    var callArg = call.Arguments[callArgIdx];

                    if (callArg is VariableRefMethodArgumentElement varArg)
                    {
                        var msg = variableSet.ValidateVariable(sourceFileName, varArg);

                        if (msg is object)
                        {
                            messages.Add(msg);
                        }
                    }
                    else if (callArg is VariableArrayRefMethodArgument varArrArg)
                    {
                        var msg = variableSet.ValidateVariable(sourceFileName, varArrArg);

                        if (msg is object)
                        {
                            messages.Add(msg);
                        }
                    }
                    else if (callArg is ConstantMethodArgument constantArg)
                    {
                        if (!constants.ContainsConstant(constantArg.ConstantName))
                        {
                            // Not a valid constant.
                            messages.Add(CompilerMessageFactory.Create(sourceFileName, constantArg, CompilerMessageLevel.Error, CompilerMessageCode.InteractionConstantNotDefined, constantArg.ConstantName));
                        }
                    }
                }

                // Look up the method in the method table.
                if (methodTable.TryGetMethod(call.MethodName, out var foundMethod))
                {
                    // Method is in the method table
                    if (foundMethod is FileDefinedInteractionMethod fileMethod)
                    {
                        if (requireMethodDefinitions && fileMethod.NeedsDefining)
                        {
                            // Error.
                            // File-based method needs a definition.
                            messages.Add(CompilerMessageFactory.Create(
                                sourceFileName,
                                call,
                                CompilerMessageLevel.Error,
                                CompilerMessageCode.InteractionMethodRequiredButNotDefined,
                                fileMethod.MethodDefinition.SourceName ?? string.Empty,
                                fileMethod.MethodDefinition.SourceLine));
                        }

                        if (ReferenceEquals(fileMethod.MethodDefinition, definition))
                        {
                            // Circular reference detection.
                            messages.Add(CompilerMessageFactory.Create(
                                sourceFileName,
                                call,
                                CompilerMessageLevel.Error,
                                CompilerMessageCode.InteractionMethodCircularReference));
                        }
                    }

                    // Match the provided arguments against the bound method.
                    if (foundMethod.ArgumentCount != call.Arguments.Count)
                    {
                        // Argument count mismatch.
                        messages.Add(CompilerMessageFactory.Create(
                            sourceFileName,
                            call,
                            CompilerMessageLevel.Error,
                            CompilerMessageCode.InteractionMethodArgumentMismatch,
                            foundMethod.ArgumentCount,
                            call.Arguments.Count));
                    }

                    // Let this method update the set of available variables for the next one.
                    foundMethod.CompilerMethodCall(call.Arguments, variableSet);
                }
                else if (requireMethodDefinitions)
                {
                    // Error.
                    // Method does not exist (and 'needs-defining' is not allowed).
                    messages.Add(CompilerMessageFactory.Create(
                        sourceFileName,
                        call,
                        CompilerMessageLevel.Error,
                        CompilerMessageCode.InteractionMethodNotAvailable,
                        call.MethodName));
                }
                else
                {
                    // Error.
                    // Method does not exist.
                    messages.Add(CompilerMessageFactory.Create(
                        sourceFileName,
                        call,
                        CompilerMessageLevel.Error,
                        CompilerMessageCode.InteractionMethodNotAvailablePermitUndefined,
                        call.MethodName));
                }
            }
        }
    }
}
