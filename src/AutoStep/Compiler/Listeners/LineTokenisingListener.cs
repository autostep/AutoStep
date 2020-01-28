using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Antlr4.Runtime.Misc;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler.Listeners
{
    public class LineToken
    {
        public int TokenId { get; set; }
    }

    internal class LineTokenisingListener : AutoStepParserBaseListener
    {
        public override void EnterLine(AutoStepParser.LineContext context)
        {
            
        }
    }
}
