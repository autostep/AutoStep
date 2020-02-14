using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using AutoStep.Elements.Interaction;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Language.Interaction
{
    internal class AutoStepInteractionGroupBuilder
    {
        private readonly Dictionary<string, InteractionFileElement> interactionFiles = new Dictionary<string, InteractionFileElement>();
        private readonly TraitGraph traits = new TraitGraph();
        private readonly Dictionary<string, List<ComponentDefinitionElement>> allComponents = new Dictionary<string, List<ComponentDefinitionElement>>();
        private readonly List<CompilerMessage> messages = new List<CompilerMessage>();

        public IReadOnlyList<CompilerMessage> Messages => messages;

        public void AddInteractionFile(string sourceName, InteractionFileElement interactionFile)
        {
            interactionFile = interactionFile.ThrowIfNull(nameof(interactionFile));

            interactionFiles.Add(sourceName, interactionFile);

            traits.Merge(interactionFile.TraitGraph);

            // Merge in the components. We'll correlate everything in a moment.
            foreach (var comp in interactionFile.Components)
            {
                if (!allComponents.TryGetValue(comp.Id, out var allDefSet))
                {
                    allDefSet.Add(comp);
                }
                else
                {
                    allDefSet = new List<ComponentDefinitionElement>();
                }

                allDefSet.Add(comp);
            }
        }
    }

    public class AutoStepInteractionGroup
    {
        private readonly InteractionConstantSet constants;
        private readonly MethodTable rootMethodTable;

        public AutoStepInteractionGroup(InteractionConstantSet constants, MethodTable rootMethodTable, TraitGraph allTraits)
        {
            this.constants = constants;
            this.rootMethodTable = rootMethodTable;
            BuiltUtc = DateTime.UtcNow;
            TraitGraph = allTraits;
        }

        public DateTime BuiltUtc { get; }

        public TraitGraph TraitGraph { get; }

        public Dictionary<string, ComponentDefinitionElement> Components { get; } = new Dictionary<string, ComponentDefinitionElement>();
        
        public void GetMethodTableForTraits(MethodTable rootTable, NameRefElement[] traitParts)
        {
            var methodTable = new MethodTable(rootTable);

            TraitGraph.SearchTraitsSimplestFirst(traitParts, methodTable, (table, el) =>
            {
                foreach (var method in el.Methods)
                {
                    methodTable.Set(method.Name, method);
                }
            });
        }
    }
}
