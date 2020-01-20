namespace AutoStep.Elements.ReadOnly
{
    public interface IOptionInfo : IAnnotationInfo
    {
        string Name { get; }

        string? Setting { get; }
    }
}
