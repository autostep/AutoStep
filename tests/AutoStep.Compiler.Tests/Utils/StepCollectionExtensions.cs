using System;
using AutoStep.Compiler.Tests.Builders;
using AutoStep.Core;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler.Tests.Utils
{

    public static class StepCollectionExtensions        
    {
        public static TBuilder Given<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepReferenceBuilder> cfg = null)
            where TBuilder : IStepCollectionBuilder<StepCollectionElement>
        {
            var stepBuilder = new StepReferenceBuilder(body, StepType.Given, StepType.Given, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder When<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepReferenceBuilder> cfg = null)
            where TBuilder : IStepCollectionBuilder<StepCollectionElement>
        {
            var stepBuilder = new StepReferenceBuilder(body, StepType.When, StepType.When, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder Then<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepReferenceBuilder> cfg = null)
            where TBuilder : IStepCollectionBuilder<StepCollectionElement>
        {
            var stepBuilder = new StepReferenceBuilder(body, StepType.Then, StepType.Then, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder And<TBuilder>(this TBuilder builder, string body, StepType? actualType, int line, int column, Action<StepReferenceBuilder> cfg = null)
            where TBuilder : IStepCollectionBuilder<StepCollectionElement>
        {
            var stepBuilder = new StepReferenceBuilder(body, StepType.And, actualType, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }
    }


}
