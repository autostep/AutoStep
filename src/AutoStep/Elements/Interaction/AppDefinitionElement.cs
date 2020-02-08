using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Elements.Interaction
{
    public class InteractionNameRefElement : PositionalElement
    {
        public string NameRef { get; set; }
    }

    public class AppDefinitionElement : InteractionDefinitionElement
    {
        public InteractionNameRefElement[] Components { get; set; }
    }
}
