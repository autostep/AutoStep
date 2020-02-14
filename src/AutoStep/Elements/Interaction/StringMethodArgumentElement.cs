using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Language;

namespace AutoStep.Elements.Interaction
{

    public class StringMethodArgumentElement : MethodArgumentElement
    {
        public string Text { get; set; }

        public TokenisedArgumentValue Tokenised { get; set; }
    }
}
