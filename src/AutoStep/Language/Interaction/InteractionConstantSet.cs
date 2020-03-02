using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Language.Interaction
{
    public class InteractionConstantSet
    {
        private readonly Dictionary<string, string> constants = new Dictionary<string, string>();

        public InteractionConstantSet()
        {
            AddDefaultConstants();
        }

        public bool ContainsConstant(string constantName)
        {
            return constants.ContainsKey(constantName);
        }

        public string GetConstantValue(string constantName)
        {
            if (constants.TryGetValue(constantName, out var constant))
            {
                return constant;
            }

            throw new InvalidOperationException("Specified constant does not exist in the set.");
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
