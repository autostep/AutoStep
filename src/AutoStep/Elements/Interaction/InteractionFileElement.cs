﻿using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Tree;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Elements.Interaction
{
    public class InteractionFileElement : BuiltElement
    {
        /// <summary>
        /// Gets or sets the name of the source (usually a file name).
        /// </summary>
        public string? SourceName { get; set; }

        public TraitGraph TraitGraph { get; } = new TraitGraph();

        public List<ComponentDefinitionElement> Components { get; } = new List<ComponentDefinitionElement>();
    }
}
