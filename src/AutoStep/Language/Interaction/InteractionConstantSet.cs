using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Language.Interaction
{
    public class InteractionConstantSet
    {
        private readonly Dictionary<string, object> constants = new Dictionary<string, object>();

        public InteractionConstantSet()
        {
            AddDefaultConstants();
        }

        public bool ContainsConstant(string constantName)
        {
            return constants.ContainsKey(constantName);
        }

        private void AddDefaultConstants()
        {
            constants.Add("TAB", "\t");
            constants.Add("ESC", "\x1B");
            constants.Add("ESCAPE", "\x1B");
            constants.Add("NEWLINE", Environment.NewLine);
        }
    }
}
