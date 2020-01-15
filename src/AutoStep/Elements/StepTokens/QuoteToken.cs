namespace AutoStep.Elements.StepTokens
{
    internal class QuoteToken : StepToken
    {
        public QuoteToken(int startIndex) : base(startIndex, 1)
        {
        }

        public bool IsDoubleQuote { get; set; }
    }
}
