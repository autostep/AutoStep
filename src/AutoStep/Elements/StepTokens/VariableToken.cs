namespace AutoStep.Elements.StepTokens
{
    internal class VariableToken : StepToken
    {
        public VariableToken(int startIndex, int length) : base(startIndex, length)
        {
        }

        public string VariableName { get; set; }
    }
}
