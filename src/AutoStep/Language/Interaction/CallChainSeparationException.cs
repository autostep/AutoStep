using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Interaction.Visitors;
using AutoStep.Language.Test;
using AutoStep.Language.Test.Parser;
using Microsoft.Extensions.Logging;

namespace AutoStep.Language.Interaction
{
    using static AutoStepInteractionsParser;

    public class CallChainSeparationException : InputMismatchException
    {
        public CallChainSeparationException(Antlr4.Runtime.Parser recognizer)
            : base(recognizer)
        {
        }
    }
}
