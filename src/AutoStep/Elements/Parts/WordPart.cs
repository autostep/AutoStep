using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoStep.Compiler;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{
    internal class WordPart : ContentPart
    {
        public WordPart(int startIndex, int length) : base(startIndex, length)
        {
        }

        public string EscapedText { get; set; }
    }

}
