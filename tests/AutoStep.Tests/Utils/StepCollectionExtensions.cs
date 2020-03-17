using System;
using AutoStep.Elements.Test;
using AutoStep.Tests.Builders;

namespace AutoStep.Tests.Utils
{
    public static class StepCollectionExtensions
    {
        public static TBuilder Given<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepReferenceBuilder>? cfg = null)
            where TBuilder : IStepCollectionBuilder<StepCollectionElement>
        {
            var stepBuilder = new StepReferenceBuilder(body, StepType.Given, StepType.Given, line, column);

            cfg?.Invoke(stepBuilder);

            stepBuilder.Built.FreezeTokens();

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder When<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepReferenceBuilder>? cfg = null)
            where TBuilder : IStepCollectionBuilder<StepCollectionElement>
        {
            var stepBuilder = new StepReferenceBuilder(body, StepType.When, StepType.When, line, column);

            cfg?.Invoke(stepBuilder);

            stepBuilder.Built.FreezeTokens();

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder Then<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepReferenceBuilder>? cfg = null)
            where TBuilder : IStepCollectionBuilder<StepCollectionElement>
        {
            var stepBuilder = new StepReferenceBuilder(body, StepType.Then, StepType.Then, line, column);

            cfg?.Invoke(stepBuilder);

            stepBuilder.Built.FreezeTokens();

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder And<TBuilder>(this TBuilder builder, string body, StepType? actualType, int line, int column, Action<StepReferenceBuilder>? cfg = null)
            where TBuilder : IStepCollectionBuilder<StepCollectionElement>
        {
            var stepBuilder = new StepReferenceBuilder(body, StepType.And, actualType, line, column);

            cfg?.Invoke(stepBuilder);

            stepBuilder.Built.FreezeTokens();

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }
    }
}
