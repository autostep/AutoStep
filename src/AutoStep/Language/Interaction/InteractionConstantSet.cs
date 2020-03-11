using System;
using System.Collections.Generic;
using System.Globalization;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Defines a set of available constant values (e.g. TAB, ESC, NEWLINE) that can be referenced
    /// in the interaction language.
    /// </summary>
    public class InteractionConstantSet
    {
        private readonly Dictionary<string, string> constants = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionConstantSet"/> class.
        /// </summary>
        public InteractionConstantSet()
        {
            AddDefaultConstants();
        }

        /// <summary>
        /// Checks if this constant set contains the specified constant.
        /// </summary>
        /// <param name="constantName">The name of the constant.</param>
        /// <returns>True if the constant is defined; false otherwise.</returns>
        public bool ContainsConstant(string constantName)
        {
            return constants.ContainsKey(constantName);
        }

        /// <summary>
        /// Get a constant value, given the name.
        /// </summary>
        /// <param name="constantName">The name of the constant.</param>
        /// <returns>The constant value.</returns>
        public string GetConstantValue(string constantName)
        {
            if (constants.TryGetValue(constantName, out var constant))
            {
                return constant;
            }

            throw new InvalidOperationException(LanguageEngineExceptionMessages.InteractionConstantSet_ConstantNotAvailable);
        }

        /// <summary>
        /// Set a constant value.
        /// </summary>
        /// <param name="name">The name of the constant; must be all upper-case, e.g. TAB.</param>
        /// <param name="value">The value of the constant.</param>
        public void SetConstant(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(LanguageEngineExceptionMessages.InteractionConstantSet_ConstantNameRequired, nameof(name));
            }

            if (name.ToUpper(CultureInfo.CurrentCulture) == name)
            {
                throw new ArgumentException(LanguageEngineExceptionMessages.InteractionConstantSet_ConstantNamesMustBeUpperCase, nameof(name));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            constants[name] = value;
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
