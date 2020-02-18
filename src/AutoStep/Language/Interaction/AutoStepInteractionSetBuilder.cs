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
        private readonly TraitGraph traits = new TraitGraph();
        private readonly Dictionary<string, List<ComponentDefinitionElement>> allComponents = new Dictionary<string, List<ComponentDefinitionElement>>();
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
                    allComponents[comp.Id] = allDefSet = new List<ComponentDefinitionElement>();
                }

                allDefSet.Add(comp);
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
            //     - Raise compiler errors on a component if it has a method
            //     - Each component becomes a step definition source. It adds all steps that apply to it.
            foreach (var componentEntry in allComponents)
            {
                var component = componentEntry.Value.Last();

                var finalComponent = new BuiltComponent
                {
                    Name = component.Name,
                    MethodTable = new MethodTable(rootMethodTable),
                };

                if (component.Traits.Any())
                {
                    traits.SearchTraitsSimplestFirst(component.Traits.ToArray(), finalComponent, (ctxt, trait) =>
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

                // Do it once to update the method table...
                foreach (var method in component.Methods)
                {
                    finalComponent.MethodTable.Set(method.Name, method);
                }

                // ...and again to validate the call chain.
                foreach (var method in component.Methods)
                {
                    ValidateCallChain(component.SourceName, method, finalComponent.MethodTable, constants, true, messages);
                }

                // Add any methods and steps for the component itself.
                foreach (var step in component.Steps)
                {
                    ValidateCallChain(step.SourceName, step, finalComponent.MethodTable, constants, true, messages);

                    allBoundSteps.Add(step);
                }

                builtComponents[component.Name] = finalComponent;
            }

            return new AutoStepInteractionGroupBuilderResult(
                messages.All(x => x.Level != CompilerMessageLevel.Error),
                messages,
                new AutoStepInteractionSet(constants, builtComponents, allBoundSteps));
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
                    call.BoundMethod = foundMethod;

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
                    foundMethod.UpdateVariablesAfterMethod(variableSet);
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
