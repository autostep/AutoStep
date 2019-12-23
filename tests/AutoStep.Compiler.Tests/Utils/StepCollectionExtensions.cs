using System;
using AutoStep.Compiler.Tests.Builders;
using AutoStep.Core;

namespace AutoStep.Compiler.Tests.Utils
{
    public static class StepCollectionExtensions        
    {
        public static TBuilder Given<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepBuilder> cfg = null)
            where TBuilder : IStepCollectionBuilder<BuiltStepCollection>
        {
            var stepBuilder = new StepBuilder(body, StepType.Given, StepType.Given, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder When<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepBuilder> cfg = null)
            where TBuilder : IStepCollectionBuilder<BuiltStepCollection>
        {
            var stepBuilder = new StepBuilder(body, StepType.When, StepType.When, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder Then<TBuilder>(this TBuilder builder, string body, int line, int column, Action<StepBuilder> cfg = null)
            where TBuilder : IStepCollectionBuilder<BuiltStepCollection>
        {
            var stepBuilder = new StepBuilder(body, StepType.Then, StepType.Then, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }

        public static TBuilder And<TBuilder>(this TBuilder builder, string body, StepType? actualType, int line, int column, Action<StepBuilder> cfg = null)
            where TBuilder : IStepCollectionBuilder<BuiltStepCollection>
        {
            var stepBuilder = new StepBuilder(body, StepType.And, actualType, line, column);

            if (cfg is object)
            {
                cfg(stepBuilder);
            }

            builder.Built.Steps.Add(stepBuilder.Built);

            return builder;
        }
    }


}
