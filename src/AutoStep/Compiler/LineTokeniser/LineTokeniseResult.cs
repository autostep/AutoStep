using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Compiler;
using AutoStep.Compiler.Parser;
using static AutoStep.Compiler.Parser.AutoStepParser;

namespace AutoStep
{
    public class LineTokeniseResult
    {
        public LineTokeniserState EndState { get; }

        public IEnumerable<LineToken> Tokens { get; }

        public LineTokeniseResult(LineTokeniserState endState, IEnumerable<LineToken> tokens)
        {
            EndState = endState;
            Tokens = tokens;
        }

        public LineTokeniseResult(IEnumerable<LineToken> tokens)
        {
            Tokens = tokens;
        }

        public LineTokeniseResult(LineTokeniserState endState, params LineToken[] tokens)
        {
            EndState = endState;
            Tokens = tokens;
        }

        public LineTokeniseResult(params LineToken[] tokens)
        {
            Tokens = tokens;
        }
    }
}
