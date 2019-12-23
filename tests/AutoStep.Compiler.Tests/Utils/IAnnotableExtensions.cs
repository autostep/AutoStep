using AutoStep.Compiler.Tests.Builders;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Utils
{
    public static class IAnnotableExtensions
    {
        public static TBuilder Tag<TBuilder>(this TBuilder builder, string tagName, int line, int column)
             where TBuilder : IBuilder<IAnnotatable>
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
             where TBuilder : IBuilder<IAnnotatable>
        {
            builder.GetBuilt().Annotations.Add(new OptionElement
            {
                SourceLine = line,
                SourceColumn = column,
                Name = optionName
            });

            return builder;
        }
        public static TBuilder Option<TBuilder>(this TBuilder builder, string optionName, string setting, int line, int column)
            where TBuilder : IBuilder<IAnnotatable>
        {
            builder.GetBuilt().Annotations.Add(new OptionElement
            {
                SourceLine = line,
                SourceColumn = column,
                Name = optionName,
                Setting = setting
            });

            return builder;
        }
    }


}
