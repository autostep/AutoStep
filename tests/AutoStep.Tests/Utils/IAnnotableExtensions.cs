using AutoStep.Tests.Builders;
using AutoStep.Elements;

namespace AutoStep.Tests.Utils
{
    public static class IAnnotableExtensions
    {
        public static TBuilder Tag<TBuilder>(this TBuilder builder, string tagName, int line, int column)
             where TBuilder : IBuilder<IAnnotatableElement>
        {
            builder.GetBuilt().Annotations.Add(new TagElement
            {
                SourceLine = line,
                SourceColumn = column,
                Tag = tagName
            });

            return builder;
        }

        public static TBuilder Option<TBuilder>(this TBuilder builder, string optionName, int line, int column)
             where TBuilder : IBuilder<IAnnotatableElement>
        {
            builder.GetBuilt().Annotations.Add(new OptionElement(optionName)
            {
                SourceLine = line,
                SourceColumn = column
            });

            return builder;
        }
        public static TBuilder Option<TBuilder>(this TBuilder builder, string optionName, string setting, int line, int column)
            where TBuilder : IBuilder<IAnnotatableElement>
        {
            builder.GetBuilt().Annotations.Add(new OptionElement(optionName)
            {
                SourceLine = line,
                SourceColumn = column,
                Setting = setting
            });

            return builder;
        }
    }


}
