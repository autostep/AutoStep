namespace AutoStep.Elements.StepTokens
{

    internal class EscapedCharToken : StepToken
    {
        public EscapedCharToken(int startIndex, int length) : base(startIndex, length)
        {
        }

        public string EscapedValue { get; set; }
    }
}
