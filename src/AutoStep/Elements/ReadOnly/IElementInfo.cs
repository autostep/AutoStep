namespace AutoStep.Elements.ReadOnly
{
    public interface IElementInfo
    {
        int SourceLine { get; }

        int StartColumn { get; }
    }
}
