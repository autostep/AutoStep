using AutoStep.Tests.Builders;
using AutoStep.Elements;

namespace AutoStep.Tests.Utils
{
    public static class IAnnotableExtensions
    {
        public static TBuilder Tag<TBuilder>(this TBuilder builder, string tagName, int line, int column)
             where TBuilder : IBuilder<IAnnotatableElement>
        {
            builder.GetBuilt().Annotations.Add(new TagElement(tagName)
            {
                SourceLine = line,
                StartColumn = column
            });

            return builder;
        }

        public static TBuilder Option<TBuilder>(this TBuilder builder, string optionName, int line, int column)
             where TBuilder : IBuilder<IAnnotatableElement>
        {
            builder.GetBuilt().Annotations.Add(new OptionElement(optionName)
            {
                SourceLine = line,
                StartColumn = column
            });

            return builder;
        }
        public static TBuilder Option<TBuilder>(this TBuilder builder, string optionName, string setting, int line, int column)
            where TBuilder : IBuilder<IAnnotatableElement>
        {
            builder.GetBuilt().Annotations.Add(new OptionElement(optionName)
            {
                SourceLine = line,
                StartColumn = column,
                Setting = setting
            });

            return builder;
        }
    }


}
