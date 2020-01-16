using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Compiler
{
    public class ArgumentBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBinding"/> class.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="boundTokens"></param>
        internal ArgumentBinding(ArgumentPart part, StepReferenceMatchResult matchResult)
        {
            Part = part;
            MatchedTokens = matchResult.MatchedTokens.ToArray();
            StartExclusive = matchResult.StartExclusive;
            EndExclusive = matchResult.EndExclusive;
        }

        internal ArgumentPart Part { get; }

        internal StepToken[] MatchedTokens { get; }

        internal bool StartExclusive { get; }

        internal bool EndExclusive { get; }

        public ArgumentType? DeterminedType { get; internal set; }
    }

}
